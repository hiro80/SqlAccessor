using System;
using System.Reflection;
using System.Collections.Generic;
using MiniSqlParser;

namespace SqlAccessor
{
  /// <summary>
  /// レコードをロックする機能を提供する
  /// </summary>
  /// <remarks>条件付スレッドセーフ
  /// このクラスを継承するサブクラスはスレッドセーフであること</remarks>
  internal abstract class LockManager: ILockManager
  {
    /// <summary>
    /// ロックデータを表す
    /// </summary>
    /// <remarks></remarks>
    [Serializable()]
    protected class LockData: IRecord
    {
      //乱数の種
      private static readonly Random _random = new Random();

      //ロックデータ
      private long _lockId = long.MinValue;
      private long _apTranId = long.MinValue;
      private string _recordName;
      private string _tableOwner;
      private string _tableName;
      private CNFofEquation _predicate;
      //_predicate.ToString()の結果のキャッシュ
      private string _predicateStr;

      //レコードとしても利用するため引数無しのコンストラクタを定義する
      public LockData() {
      }

      public LockData(long apTranId
                    , string recordName
                    , string tableOwner
                    , string tableName
                    , CNFofEquation predicate = null) {
        //LockIDは乱数で決定する
        _lockId = _random.Next();
        _apTranId = apTranId;
        _recordName = recordName;
        _tableOwner = tableOwner;
        _tableName = tableName;
        _predicate = predicate;
      }

      public long LockId {
        get { return _lockId; }
        set { _lockId = value; }
      }

      public long ApTranId {
        get { return _apTranId; }
        set { _apTranId = value; }
      }

      public string RecordName {
        get { return _recordName; }
        set { _recordName = value; }
      }

      public string TableOwner {
        get { return _tableOwner; }
        set { _tableOwner = value; }
      }

      public string TableName {
        get { return _tableName; }
        set { _tableName = value; }
      }

      public CNFofEquation Predicate {
        get { return _predicate; }
      }

      public string PredicateStr {
        get {
          if(_predicate == null) {
            return "";
          }

          if(_predicateStr != null) {
            return _predicateStr;
          }

          _predicateStr = _predicate.ToString();
          return _predicateStr;
        }
        set {
          _predicateStr = null;
          _predicate = new CNFofEquation(value);
        }
      }
    }

    protected readonly RecordViewTableMapFactory _aRecordViewTableMapFactory;
    protected readonly ICaster _caster;
    protected readonly object _lock = new object();

    public LockManager(RecordViewTableMapFactory aRecordViewTableMapFactory
                     , ICaster aCaster) {
      _aRecordViewTableMapFactory = aRecordViewTableMapFactory;
      _caster = aCaster;
    }

    //包摂候補(異なるApTranIDかつ同一TableName)となるLockDataを抽出する
    protected abstract IEnumerable<LockData> FindCandidateSubsumptionLockData(long apTranId
                                                                            , string tableName);
    //LockDataオブジェクトの数を返す
    protected abstract int CountLockData(long apTranId);
    //LockDataオブジェクトを保存する
    protected abstract void SaveLockData(LockData aLockData);
    //LockDataオブジェクトを削除する
    protected abstract void DeleteLockData(long apTranId);

    /// <summary>
    /// SQL文の抽出条件からロックを作成する
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="apTranId">APトランザクションID</param>
    /// <param name="sql">SQL文</param>
    /// <returns>作成したロックデータ</returns>
    /// <remarks></remarks>
    private List<LockData> CreateLockData<TRecord>(long apTranId
                                                 , SqlBuilder sql) 
    where TRecord: class, IRecord, new() {
      List<LockData> ret = new List<LockData>();

      //TRecord型レコードとテーブルのマッピング情報を取得する
      RecordViewTableMap<TRecord> aRecordViewTableMap = _aRecordViewTableMapFactory.CreateRecordViewTableMap<TRecord>();
      //TRecord型レコードのメタ情報を取得する
      RecordInfo<TRecord> aRecordInfo = aRecordViewTableMap.GetRecordInfo();

      //テーブル列リストを作成する
      Dictionary<string, IEnumerable<string>> tableColumns = new Dictionary<string, IEnumerable<string>>();
      foreach(string srcTableName in sql.GetSrcTableNames()) {
        //相関テーブルの主キーリストを作成する
        List<string> allColumnNames = new List<string>();
        foreach(ColumnInfo column in aRecordViewTableMap.GetTableInfo(srcTableName).GetAllColumns()) {
          allColumnNames.Add(column.ColumnName);
        }
        tableColumns.Add(srcTableName, allColumnNames);
      }

      //SQL文の抽出条件からロック情報を作成する
      foreach(KeyValuePair<SqlTable, Dictionary<string, string>> tableNameCnfPair in sql.GetCNF(tableColumns)) {
        string srcTableName = tableNameCnfPair.Key.Name;
        Dictionary<string, string> cnf = tableNameCnfPair.Value;
        LockData aLockData = this.CreateLockDataSub(aRecordViewTableMap
                                                  , apTranId
                                                  , aRecordInfo.Name
                                                  , srcTableName
                                                  , cnf);
        ret.Add(aLockData);
      }

      return ret;
    }

    private LockData CreateLockDataSub<TRecord>(RecordViewTableMap<TRecord> aRecordViewTableMap
                                              , long apTranId
                                              , string recordName
                                              , string tableName
                                              , Dictionary<string, string> cnf) 
    where TRecord: class, IRecord, new() {
      //主キーを含む全ての一致条件を格納するオブジェクト
      CNFofEquation cnfOfEquation = new CNFofEquation();
      //主キーに対する一致条件のみを格納するオブジェクト
      CNFofEquation cnfOfPKey = new CNFofEquation();

      //乗法標準形を用意する
      foreach(KeyValuePair<string, string> eqPredicate in cnf) {
        string varName = eqPredicate.Key;
        //SQLリテラル表記で値を取得する
        string value = eqPredicate.Value;

        //valueがIsNullPropertyValue()でNULL値と判定された場合、Predicateに含まれない(暫定実装)
        TableInfo aTableInfo = aRecordViewTableMap.GetTableInfo(tableName);
        ColumnInfo aColumnInfo = aTableInfo[varName];

        //一致条件を述語に追加する
        cnfOfEquation.Add(new Equation(varName, value));

        //主キーに対する一致条件であればcnfOfPKeyにも格納する
        if(aColumnInfo.PrimaryKey.HasValue && aColumnInfo.PrimaryKey.Value) {
          cnfOfPKey.Add(new Equation(varName, value));
          //テーブルの全ての主キーに対する一致条件が取得できれば、
          //主キーに対する一致条件のみの述語だけで必要十分である
          if(cnfOfPKey.Count == aTableInfo.GetPrimaryKeys().Count) {
            return new LockData(apTranId, recordName, "", tableName, cnfOfPKey);
          }
        }
      }

      return new LockData(apTranId, recordName, "", tableName, cnfOfEquation);
    }

    private List<LockData> CreateLockDataFromRecord_new<TRecord>(long apTranId
                                                               , TRecord aRecord
                                                               , IEnumerable<SqlTable> usedTableNames) 
    where TRecord: class, IRecord, new() {
      List<LockData> ret = new List<LockData>();

      //TRecord型レコードとテーブルのマッピング情報を取得する
      RecordViewTableMap<TRecord> aRecordViewTableMap = _aRecordViewTableMapFactory.CreateRecordViewTableMap<TRecord>();
      //TRecord型レコードのメタ情報を取得する
      RecordInfo<TRecord> aRecordInfo = aRecordViewTableMap.GetRecordInfo();

      foreach(KeyValuePair<SqlTable, Dictionary<string, SqlExpr>> kv in aRecordViewTableMap.GetCNFExpression(aRecord
                                                                                                           , usedTableNames)) {
        string tableName = kv.Key.Name;
        //CNFを作成する
        Dictionary<string, string> cnf = new Dictionary<string, string>();
        foreach(KeyValuePair<string, SqlExpr> eq in kv.Value) {
          //CNFの素論理式はリテラル値への一致条件のみとする
          if(eq.Value.IsLiteral) {
            cnf.Add(eq.Key, eq.Value.ToString());
          }
        }

        //ロック情報を作成する
        LockData aLockData = this.CreateLockDataSub(aRecordViewTableMap
                                                  , apTranId
                                                  , aRecordInfo.Name
                                                  , tableName
                                                  , cnf);
        ret.Add(aLockData);
      }

      return ret;
    }

    /// <summary>
    /// 2つの乗法標準形の論理式が包摂しているか判定する
    /// </summary>
    /// <param name="lCnf">論理式1</param>
    /// <param name="rCnf">論理式2</param>
    /// <returns>包摂している:True、していない:False</returns>
    /// <remarks></remarks>
    private bool IsSubsumption(CNFofEquation lCnf, CNFofEquation rCnf) {
      //一致条件の左辺の文字コード順で並べ替える
      lCnf.Sort();
      rCnf.Sort();

      //最初にeq2とeq1の変数名を比較するため、Forループ前にMoveNext()する
      if(!rCnf.MoveNext()) {
        //cnfに素論理式がなければ、包摂している
        lCnf.Reset();
        rCnf.Reset();
        return true;
      }
      Equation eq2 = rCnf.Current;

      foreach(Equation eq1 in lCnf) {
        //'eq1と同じ素論理式までrCnfをイテレートする
        //Dim eq2 As Equation = Nothing
        //While rCnf.MoveNext
        //    eq2 = rCnf.Current
        //    If eq2.Variable >= eq1.Variable Then
        //        Exit While
        //    End If
        //End While

        //eq1と同じ素論理式までrCnfをイテレートする
        while(eq2.Variable.CompareTo(eq1.Variable) < 0 && rCnf.MoveNext()) {
          eq2 = rCnf.Current;
        }

        //cnfが最後までイテレートされていれば、包摂している
        if(eq2 == null) {
          //読込位置をリセットする
          lCnf.Reset();
          rCnf.Reset();
          return true;
        }

        //素論理式の包摂判定
        if(eq2.Variable == eq1.Variable && eq2.Value != eq1.Value) {
          //読込位置をリセットする
          lCnf.Reset();
          rCnf.Reset();
          //包摂していない
          return false;
        }
      }

      //読込位置をリセットする
      lCnf.Reset();
      rCnf.Reset();
      return true;
    }

    /// <summary>
    /// 論理式同士の包摂判定を行う
    /// </summary>
    /// <param name="cnf">論理式1</param>
    /// <param name="cnfs">論理式2-N</param>
    /// <returns>包摂している:True、していない:False</returns>
    /// <remarks>論理式2-N同士の包摂判定は行わない</remarks>
    private bool IsSubsumption(CNFofEquation cnf, params CNFofEquation[] cnfs) {
      //判定対象の論理式が無ければ包摂していないとする
      if(cnfs.Length == 0) {
        return false;
      }

      foreach(CNFofEquation cnf1 in cnfs) {
        if(this.IsSubsumption(cnf, cnf1)) {
          return true;
        }
      }

      //全てのcnfsと包摂していなければ包摂していないと判定する
      return false;
    }

    /// <summary>
    /// 指定したロックデータが既にロックされているデータか判定する
    /// </summary>
    /// <param name="aLockDataSet">判定対象のロックデータ</param>
    /// <returns>ロックされている:True、されていない:False</returns>
    /// <remarks></remarks>
    private bool IsLocked(IEnumerable<LockData> aLockDataSet) {
      //LockData毎に包摂判定を行う
      foreach(LockData aLockData in aLockDataSet) {
        //1つ以上のLockDataが包摂していれば、Trueを返す
        if(this.IsLocked(aLockData)) {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// 指定したロックデータが既にロックされているデータか判定する
    /// </summary>
    /// <param name="aLockData">判定対象のロックデータ</param>
    /// <returns>ロックされている:True、されていない:False</returns>
    /// <remarks></remarks>
    private bool IsLocked(LockData aLockData) {
      //包摂候補(異なるApTranIDかつ同一テーブル)となるLockDataの述語を抽出する
      List<CNFofEquation> predicates = new List<CNFofEquation>();
      foreach(LockData candidateLockData in this.FindCandidateSubsumptionLockData(aLockData.ApTranId
                                                                                , aLockData.TableName)) {
        predicates.Add(candidateLockData.Predicate);
      }

      //述語同士の包摂判定
      if(this.IsSubsumption(aLockData.Predicate, predicates.ToArray())) {
        return true;
      }

      return false;
    }

    public bool Lock<TRecord>(long apTranId
                            , TRecord aRecord
                            , IEnumerable<SqlTable> usedTableNames = null) 
    where TRecord: class, IRecord, new() {
      //LockDataSetオブジェクトを作成する
      List<LockData> newLockDataSet = this.CreateLockDataFromRecord_new(apTranId
                                                                      , aRecord
                                                                      , usedTableNames);
      return this.LockImp(newLockDataSet);
    }

    public bool Lock<TRecord>(long apTranId
                            , SqlBuilder sql) 
    where TRecord: class, IRecord, new() {
      //LockDataSetオブジェクトを作成する
      List<LockData> newLockDataSet = this.CreateLockData<TRecord>(apTranId, sql);
      return this.LockImp(newLockDataSet);
    }

    private bool LockImp(List<LockData> newLockDataSet) {
      // IsLocked()とSaveLockData()の処理の間に、
      // lockDataと包摂するロックデータを他スレッドから格納されるのを防ぐ
      lock(_lock) {
        // 作成したLockDataSetオブジェクトと、_lockDataSetに格納されている
        // 全てのLockDataオブジェクトが包摂していないかチェックする
        if(this.IsLocked(newLockDataSet)) {
          //Lock出来ない時は、Falseを返す
          return false;
        }

        //包摂していなければLockDataオブジェクトを追加する
        foreach(LockData lockData in newLockDataSet) {
          this.SaveLockData(lockData);
        }
      }

      //ロックが成功したらTrueを返す
      return true;
    }

    /// <summary>
    /// ロックデータの件数を取得する
    /// </summary>
    /// <param name="apTranId">APトランザクションID</param>
    /// <returns>ロックデータの件数</returns>
    /// <remarks></remarks>
    public int CountLock(long apTranId) {
      return this.CountLockData(apTranId);
    }

    /// <summary>
    /// ロックデータを削除する
    /// </summary>
    /// <param name="apTranId">APトランザクションID</param>
    /// <remarks></remarks>
    public void UnLock(long apTranId) {
      //apTranIdを抽出条件としてロックデータを削除する
      this.DeleteLockData(apTranId);
    }
  }
}
