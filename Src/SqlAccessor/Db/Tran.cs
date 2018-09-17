using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MiniSqlParser;

namespace SqlAccessor
{
  /// <summary>
  /// APトランザクションを表す
  /// </summary>
  /// <remarks></remarks>
  [Serializable()]
  [System.Diagnostics.DebuggerDisplay("ID:{_apTranId}  State:{_state}")]
  public partial class Tran: Disposable, ISerializable
  {
    [Serializable()]
    private class ExecutedSql
    {
      public ExecutedSql(string aSql, string aUpdateTable) {
        Sql = aSql;
        UpdateTable = aUpdateTable;
      }
      public readonly string Sql;
      public readonly string UpdateTable;
    }

    /// <summary>
    /// Tranオブジェクトの状態を表す
    /// </summary>
    /// <remarks></remarks>
    private enum State: int
    {
      Start,
      OutOfDbTransaction,
      DbTransaction,
      Rollbacked,
      End,
      Error1,
      Error2
    }

    /// <summary>
    /// DBからレコードを抽出する時の読込モードを表す
    /// </summary>
    /// <remarks></remarks>
    public enum LoadMode: int
    {
      ReadOnly,
      ReadWrite
    }

    /// <summary>
    /// DBからレコードを抽出する時のキャッシュ方法を表す
    /// </summary>
    /// <remarks></remarks>
    public enum CacheStrategy: int
    {
      //キャッシュを使わない
      NoCache,
      //もしキャッシュがあれば使う
      UseCacheIfExists,
      //(キャッシュが無ければ作成して)キャッシュを使う
      UseCache
    }

    //Dbオブジェクトへのリファレンス(Deserialize時に使用)
    private static readonly SyncDictionary<long, Db> _dbRef = new SyncDictionary<long, Db>();
    //APトランザクションIDを生成するための乱数の種
    private static readonly Random _random = new Random();
    //自オブジェクトを生成したDbオブジェクトへのリファレンス
    private readonly Db _aDb;
    //データベースへの接続
    private IDbConn _aDbConn;
    //レコードとTableのマッピング情報
    private readonly RecordViewTableMapFactory _aRecordViewTableMapFactory;
    //SqlPod生成器
    private readonly SqlPodFactory _aSqlPodFactory;
    //キャスター
    private readonly ICaster _aCaster;
    //ロックマネージャ
    private readonly ILockManager _aLockManager;
    //SQL作成クラス
    private readonly SqlMaker _sqlMaker;
    //APトランザクションID
    private readonly long _apTranId;
    //更新系SQLの実行履歴
    private readonly List<ExecutedSql> _executedSqls;
    //最後に発行したSQL文
    private string _lastExecutedSql;
    //IF文の判定のために発行するSELECT文のキャッシュ動作方式
    private readonly CacheStrategy _ifStmtCacheStrategy;
    //状態変数
    private State _state;

    internal Tran(Db aDb) {
      _aDb = aDb;
      _aRecordViewTableMapFactory = _aDb.GetRecordViewTableMapFactory();
      _aSqlPodFactory = _aDb.GetSqlPodFactory();
      _aCaster = _aDb.GetCaster();
      //ロック情報をDBに1件も格納していない場合、Unlock時に削除処理を省略する
      _aLockManager = new LockManagerProxy(_aDb.GetLockManager());
      _sqlMaker = new SqlMaker(_aRecordViewTableMapFactory, _aSqlPodFactory);
      //APトランザクションIDを決定する
      _apTranId = _random.Next();
      _executedSqls = new List<ExecutedSql>();
      //IF文の判定のために発行するSELECT文のキャッシュ動作方式は、設定ファイルで決める
      if(_aDb.GetCacheType() == DbParameters.CacheType.Null) {
        _ifStmtCacheStrategy = CacheStrategy.NoCache;
      } else {
        _ifStmtCacheStrategy = CacheStrategy.UseCache;
      }
      _state = State.Start;

      //次状態への遷移
      this.GoToNextState(State.OutOfDbTransaction);
    }

    internal Tran(Db aDb, long apTranId)
      : this(aDb) {
      //APトランザクションIDは引数により指定される
      _apTranId = apTranId;
    }

    /// <summary>
    /// デシリアライズ時に行う処理
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    /// <remarks></remarks>
    protected Tran(SerializationInfo info
                 , StreamingContext context) {
      //固有のメンバ変数をデシリアライズする
      _apTranId = info.GetInt64("apTranId");
      _state = (State)info.GetInt32("state");
      _executedSqls = (List<ExecutedSql>)info.GetValue("executedSqls", typeof(List<ExecutedSql>));

      if(!_dbRef.ContainsKey(_apTranId)) {
        //何らかの理由で自身のApTranIdが失われていた場合、例外を送出する
        throw new SerializationException("デシリアライズに失敗しました");
      }
      _aDb = _dbRef[_apTranId];
      if(_aDb == null) {
        //既にDbオブジェクトが破棄されていた場合など(_dbRefで保持し続けているため有り得ないはずだが)
        throw new SerializationException("Tranオブジェクトを生成したDbオブジェクトが既に破棄されているため、デシリアライズに失敗しました");
      }
      //Dbオブジェクトへのリファレンスを削除する
      _dbRef.Remove(_apTranId);

      _aRecordViewTableMapFactory = _aDb.GetRecordViewTableMapFactory();
      _aSqlPodFactory = _aDb.GetSqlPodFactory();
      _aCaster = _aDb.GetCaster();
      _aLockManager = new LockManagerProxy(_aDb.GetLockManager());
      _sqlMaker = new SqlMaker(_aRecordViewTableMapFactory, _aSqlPodFactory);

      if(_aDb.GetCacheType() == DbParameters.CacheType.Null) {
        _ifStmtCacheStrategy = CacheStrategy.NoCache;
      } else {
        _ifStmtCacheStrategy = CacheStrategy.UseCache;
      }
    }

    /// <summary>
    /// シリアライズ時に行う処理
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    /// <remarks></remarks>
    public void GetObjectData(SerializationInfo info
                            , StreamingContext context) {
      //DBトランザクション中であれば、DBトランザクションを終了する
      if(_state == State.DbTransaction) {
        this.Suspend();
      }
      //固有のメンバ変数をシリアライズする
      info.AddValue("apTranId", _apTranId);
      info.AddValue("state", _state);
      info.AddValue("executedSqls", _executedSqls);
      //DbオブジェクトへのリファレンスをSerialize時に備えて退避する
      _dbRef.Add(_apTranId, _aDb);
    }

    /// <summary>
    /// トランザクションをCOMMITする
    /// </summary>
    /// <remarks>
    /// 以下の2つの呼出時状態を考慮する必要がある
    ///   1. Tranオブジェクトの状態(_state)
    ///   2. GCから呼ばれた場合(disposing)
    /// 複雑なコードになっているのでリファクタリング予定とする
    /// </remarks>
    protected override void DisposeImp(bool disposing) {
      if(_state == State.OutOfDbTransaction) {
        try {
          //_aDbが先にGCされた場合、Rollforwardしない
          if(_aDb != null) {
            //GCによる回収時にはCommitAtFinalizingの設定によりCOMMITまたはROLLBACKする
            if(disposing || _aDb.GetCommitAtFinalizing()) {
              //Transactionの開始
              _aDbConn = _aDb.GetDbConn();
              //ROLLFORWARD
              this.Rollforward();
              //Transactionの終了
              this.Commit();
            } else {
              this.Rollback();
            }
          }
          //_aLockManagerが先にGCされた場合、UnLockしない
          if(_aLockManager != null) {
            //UnLock
            _aLockManager.UnLock(_apTranId);
          }
        } catch(Exception ex) {
          //GCによる回収時には例外を送出しない
          if(disposing) {
            this.GoToError1AndThrow(ex, true);
          }
        } finally {
          //Dbオブジェクトへのリファレンスを削除する
          //_dbRef.Remove(_apTranId)
        }
        //次状態への遷移
        this.GoToNextState(State.End);

      } else if(_state == State.DbTransaction) {
        try {
          //Transactionの終了
          //GCによる回収時にはCommitAtFinalizingの設定によりCOMMITまたはROLLBACKする
          if(disposing || _aDb.GetCommitAtFinalizing()) {
            this.Commit();
          } else {
            this.Rollback();
          }
          //_aLockManagerが先にGCされた場合、UnLockしない
          if(_aLockManager != null) {
            //UnLock
            _aLockManager.UnLock(_apTranId);
          }
        } catch(Exception ex) {
          //GCによる回収時には例外を送出しない
          if(disposing) {
            this.GoToError1AndThrow(ex, true);
          }
        } finally {
          //Dbオブジェクトへのリファレンスを削除する
          //_dbRef.Remove(_apTranId)
        }
        //次状態への遷移
        this.GoToNextState(State.End);

      } else if(_state == State.Rollbacked) {
        //Dbオブジェクトへのリファレンスを削除する
        //_dbRef.Remove(_apTranId)
        //次状態への遷移
        this.GoToNextState(State.End);

      } else if(_state == State.Error1) {
        //Dbオブジェクトへのリファレンスを削除する
        //_dbRef.Remove(_apTranId)
        //次状態への遷移
        this.GoToNextState(State.Error2);

      } else if(_state == State.End || _state == State.Error2) {
        //処理なし

      } else if(disposing) {
        //状態遷移エラー(GCによる回収時には例外を送出しない)
        this.GoToError1AndThrow(
          new InvalidOperationException("Tranオブジェクトの無効な状態遷移が発生しました"), true);
      }
    }

    /// <summary>
    /// APトランザクションがCOMMIT又はROLLBACKされていればTrueを返す
    /// </summary>
    /// <returns></returns>
    /// <remarks></remarks>
    public bool IsClosed() {
      return _state == State.End    ||
             _state == State.Error1 ||
             _state == State.Error2;

      //以下の判定コードでは、State.OutOfDbTransactionの時に
      //APトランザクションが終了したと誤認識される
      //Return _aDbConn Is Nothing OrElse _aDbConn.IsClosed
    }

    private void Commit() {
      if(_aDbConn != null) {
        //_aDbConnのリファレンスカウンタをデクリメントする
        _aDbConn.Dispose();
        _aDbConn = null;
      }
    }

    private void RollbackImp() {
      if(_aDbConn != null) {
        //GCによりTranオブジェクトより先にDbConnオブジェクトが
        //RollBackされた場合(DbConnがEnd状態の場合)、DbConnのRollBackは行わない
        if(!_aDbConn.IsClosed()) {
          _aDbConn.Rollback();
        }
        //_aDbConnのリファレンスカウンタをデクリメントする
        _aDbConn.Dispose();
        _aDbConn = null;
      }
    }

    /// <summary>
    /// 更新系SQLの実行履歴にあるSQLを再実行する
    /// </summary>
    /// <remarks>トランザクションの中断によってROLLBACKされた更新系SQLを再実行する時に使用する</remarks>
    private void Rollforward() {
      if(_executedSqls == null || _aDbConn == null) {
        return;
      }

      foreach(ExecutedSql aExecutedSql in _executedSqls) {
        _aDbConn.ExecSql(aExecutedSql.Sql, new string[] { aExecutedSql.UpdateTable });
      }
    }

    /// <summary>
    /// トランザクションを取消す
    /// </summary>
    /// <remarks></remarks>
    public void Rollback() {
      if(_state == State.OutOfDbTransaction) {
        //次の遷移先状態が仕様通りでなければ例外を送出する
        this.ThrowIfInvalidNextState(State.Rollbacked);
        try {
          //UnLock
          _aLockManager.UnLock(_apTranId);
        } catch(Exception ex) {
          this.GoToError1AndThrow(ex, true);
        }
        //次状態への遷移
        this.GoToNextState(State.Rollbacked);

      } else if(_state == State.DbTransaction) {
        //次の遷移先状態が仕様通りでなければ例外を送出する
        this.ThrowIfInvalidNextState(State.Rollbacked);
        try {
          //ロールバックする
          this.RollbackImp();
          //Transactionの終了
          this.Commit();
          //UnLock
          _aLockManager.UnLock(_apTranId);
        } catch(Exception ex) {
          this.GoToError1AndThrow(ex, true);
        }
        //次状態への遷移
        this.GoToNextState(State.Rollbacked);

      } else if(_state == State.Rollbacked || _state == State.Error1) {
        //処理なし

      } else {
        //状態遷移エラー
        this.GoToError1AndThrow(
          new InvalidOperationException("Tranオブジェクトの無効な状態遷移が発生しました"), true);
      }
    }

    /// <summary>
    /// トランザクションを中断する
    /// </summary>
    /// <remarks></remarks>
    public void Suspend() {
      //次の遷移先状態が仕様通りでなければ例外を送出する
      this.ThrowIfInvalidNextState(State.OutOfDbTransaction);
      if(_state == State.DbTransaction) {
        //ロールバックする
        try {
          this.RollbackImp();
        } catch(Exception ex) {
          this.GoToError1AndThrow(ex, true);
        }
      }
      //次状態への遷移
      this.GoToNextState(State.OutOfDbTransaction);
    }

    /// <summary>
    /// 次の遷移先状態が仕様通りか確認する
    /// </summary>
    /// <param name="nextState">次の遷移先状態</param>
    /// <returns>True：仕様通り、False：仕様と異なる</returns>
    /// <remarks></remarks>
    private bool IsValidState(State nextState) {
      if((nextState == State.Start) ||
        (_state == State.Start && nextState == State.DbTransaction) ||
        (_state == State.Start && nextState == State.Rollbacked) ||
        (_state == State.Start && nextState == State.End) ||
        (_state == State.Start && nextState == State.Error2) ||
        (_state == State.OutOfDbTransaction && nextState == State.Error2) ||
        (_state == State.DbTransaction && nextState == State.Error2) ||
        (_state == State.Rollbacked && nextState == State.OutOfDbTransaction) ||
        (_state == State.Rollbacked && nextState == State.DbTransaction) ||
        (_state == State.Rollbacked && nextState == State.Error2) ||
        (_state == State.End && nextState == State.OutOfDbTransaction) ||
        (_state == State.End && nextState == State.DbTransaction) ||
        (_state == State.End && nextState == State.Rollbacked) ||
        (_state == State.End && nextState == State.Error2) ||
        (_state == State.Error1 && nextState == State.OutOfDbTransaction) ||
        (_state == State.Error1 && nextState == State.DbTransaction) ||
        (_state == State.Error1 && nextState == State.Rollbacked) ||
        (_state == State.Error1 && nextState == State.End) ||
        (_state == State.Error2 && nextState == State.OutOfDbTransaction) ||
        (_state == State.Error2 && nextState == State.DbTransaction) ||
        (_state == State.Error2 && nextState == State.Rollbacked) ||
        (_state == State.Error2 && nextState == State.End) ||
        (_state == State.Error2 && nextState == State.Error1)) {
        return false;
      } else {
        return true;
      }
    }

    /// <summary>
    /// 次の遷移先状態が仕様通りでなければ例外を送出する
    /// </summary>
    /// <param name="nextState">次の遷移先状態</param>
    /// <remarks></remarks>
    private void ThrowIfInvalidNextState(State nextState) {
      if(!this.IsValidState(nextState)) {
        throw new InvalidOperationException("Tranオブジェクトの無効な状態遷移が発生しました.");
      }
    }

    /// <summary>
    /// 次の遷移先状態に遷移する
    /// </summary>
    /// <param name="nextState">次の遷移先状態</param>
    /// <remarks></remarks>
    private void GoToNextState(State nextState) {
      //仕様通りの状態遷移でなければ例外を送出する
      if(!this.IsValidState(nextState)) {
        //トランザクション中であればロールバックする
        this.GoToError1AndThrow(
          new InvalidOperationException("Tranオブジェクトの無効な状態遷移が発生しました"),
          _state == State.DbTransaction);
      }

      //状態変数の設定
      _state = nextState;
    }

    /// <summary>
    /// Error1状態に遷移し、例外を送出する
    /// </summary>
    /// <param name="innerException">送出する例外</param>
    /// <param name="rollback">True：Error1状態への遷移後にROLLBACKする、False：ROLLBACKしない</param>
    /// <remarks></remarks>
    private void GoToError1AndThrow(Exception innerException, bool rollback = false) {
      //Error状態への遷移時に例外が発生しても、必ずError状態に遷移する
      this.GoToNextState(State.Error1);

      //rollbackフラグがTrueの場合、ROLLBACKする
      if(rollback) {
        try {
          this.RollbackImp();
        } catch {
          innerException = new DbAccessException("トランザクションの取消に失敗しました", innerException);
        }
      }

      //UnLockする
      if(_aLockManager != null) {
        _aLockManager.UnLock(_apTranId);
      }

      //例外を再送出する
      throw innerException;
    }

    /// <summary>
    /// 指定したSQL文から、データ抽出元テーブルの別名とその先頭主キーのペアをリストにして返す
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    /// <remarks></remarks>
    private Dictionary<SqlTable, string> MakeTableAndAliasNames<TRecord>(RecordViewTableMap<TRecord> aRecordViewTableMap
                                                                       , SqlBuilder sql)
    where TRecord: class, IRecord, new() {
      Dictionary<SqlTable, string> ret = new Dictionary<SqlTable, string>();

      //主キーリストを作成する
      Dictionary<string, IEnumerable<Tuple<string, bool>>> tableColumns = aRecordViewTableMap.GetTableColumns(sql);

      foreach(KeyValuePair<SqlTable, List<string>> kv in sql.RaisePrimaryKey(tableColumns)) {
        List<string> raisedPrimaryKeys = kv.Value;
        SqlTable tableAndAliasName = kv.Key;
        if(!ret.ContainsKey(tableAndAliasName)) {
          ret.Add(tableAndAliasName, raisedPrimaryKeys[0]);
        }
      }

      return ret;
    }

    //View列の型情報をViewColumnInfoに登録する
    //(ViewColumnInfoから呼び出すためのメソッド。卵が先か鶏が先か・・)
    internal void RegistToViewMapping(ViewInfo aViewInfo, SqlPod aSqlPod) {
      try {
        //次の遷移先状態が仕様通りでなければ例外を送出する
        this.ThrowIfInvalidNextState(State.DbTransaction);

        //初期化用SELECT文を取得する
        SqlBuilder sql = aSqlPod.GetInitSelectSql();

        //トランザクション中断状態の時は、トランザクションを開始しRollforwardを行う
        if(_state == State.OutOfDbTransaction) {
          _aDbConn = _aDb.GetDbConn();
          this.Rollforward();
        }

        //当然、キャッシュは使用しない
        CacheStrategy noCache = CacheStrategy.NoCache;

        //SQLを実行してResultsオブジェクトを取得する
        IResults aResults = _aDbConn.ExecSelect(sql.ToString(_aDb.GetSqlIndentType())
                                                           , sql.GetSrcTableNames()
                                                           , noCache);
        //RecordReaderを生成する
        using(IReader reader = new RegistColumnTypeReader(aResults, aViewInfo, this)) {
          //View列の型情報をViewColumnInfoに登録する
          object obj = reader.Current;
        }

        //次状態への遷移
        this.GoToNextState(State.DbTransaction);
      } catch(Exception ex) {
        this.GoToError1AndThrow(ex, true);
      }
    }

    private PlaceHolders CreatePlaceHolders<TRecord>(RecordViewTableMap<TRecord> aRecordViewTableMap
                                                   , PlaceHolders placeHolders
                                                   , SqlBuilder sql
                                                   , TRecord valueRecord) 
    where TRecord: class, IRecord, new() {
      //引数のplaceHoldersには既に引数指定のプレースホルダ値が指定されている
      //(レコードのプロパティ名と同じプレースホルダ名が指定された場合は引数で指定された値を優先する)

      PlaceHolders ret = null;
      //UPDATE/INSERT文の場合、SET句/VALUES句のプレースホルダ値に、
      //テーブルカラムの初期値またはDEFAULTキーワードを適用できるようにする
      if(sql.GetStatementType() == SqlBuilder.StatementType.Update ||
        sql.GetStatementType() == SqlBuilder.StatementType.InsertValue) {
        ret = aRecordViewTableMap.UnderwritePlaceHolders(placeHolders, sql.GetTargetTable().AliasName, valueRecord);
      } else {
        ret = aRecordViewTableMap.UnderwritePlaceHolders(placeHolders, valueRecord);
      }

      return ret;
    }

    /// <summary>
    /// APトランザクションIDを取得する
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
    public long ApTranId {
      get { return _apTranId; }
    }

    /// <summary>
    /// 保持しているロック情報の件数を取得する
    /// </summary>
    /// <returns></returns>
    /// <remarks></remarks>
    public int CountLock() {
      return _aLockManager.CountLock(_apTranId);
    }

    /// <summary>
    /// 最後に実行したFindOne()の処理状態を取得する
    /// </summary>
    /// <returns></returns>
    /// <remarks></remarks>
    public bool GetLastLoadState() {
      //(暫定実装)
      return true;
    }

    public string LastExecutedSql {
      get { return _lastExecutedSql; }
    }

    /// <summary>
    /// データベースからレコードを取得する
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="query">Queryオブジェクト</param>
    /// <param name="placeHolders">プレースホルダ名とその値</param>
    /// <param name="loadMode">読込モード</param>
    /// <param name="cacheStrategy">キャッシュ動作方式</param>
    /// <returns>取得したレコードのコレクション</returns>
    /// <remarks>aQueryがQuery(Of TRecord)型でないのは、
    /// オーバーロード解決時に、ジェネリックパラメータの制約が無視される為(VB2005以降で改善された?)</remarks>
    public IRecordReader<TRecord> Find<TRecord>(Query<TRecord> query
                                              , Dictionary<string, string> placeHolders
                                              , LoadMode loadMode = LoadMode.ReadOnly
                                              , CacheStrategy cacheStrategy = CacheStrategy.UseCache) 
    where TRecord: class, IRecord, new() {
      try {
        //次の遷移先状態が仕様通りでなければ例外を送出する
        this.ThrowIfInvalidNextState(State.DbTransaction);

        //TRecord型レコードとViewのマッピング情報を取得する
        RecordViewTableMap<TRecord> aRecordViewTableMap = _aRecordViewTableMapFactory.CreateRecordViewTableMap<TRecord>();
        //TRecord型に対応するViewInfoを取得する
        ViewInfo aViewInfo = aRecordViewTableMap.GetViewInfo();

        //SQL文を組み立てる
        SqlBuilder sql = _sqlMaker.GetSelectSql<TRecord>(query, loadMode);

        //トランザクション中断状態の時は、トランザクションを開始しRollforwardを行う
        if(_state == State.OutOfDbTransaction) {
          _aDbConn = _aDb.GetDbConn();
          this.Rollforward();
        }

        //DbParameters.CacheType=NULLの場合は必ずNoCacheとする
        if(_aDb.GetCacheType() == DbParameters.CacheType.Null) {
          cacheStrategy = Tran.CacheStrategy.NoCache;
        }

        //PlaceHolderオブジェクトを生成する
        placeHolders = placeHolders ?? new Dictionary<string, string>();
        PlaceHolders placeHoldersObj = new PlaceHolders(placeHolders);
        //プレースホルダ初期値コメントの値を設定する
        placeHoldersObj.Underwrite(sql.GetDefaultValuePlaceHolders());

        //IF文の場合は分岐条件を評価し、その評価結果となったSQL文を返す
        sql = IfStatement.Evaluate(_aDbConn, sql, placeHoldersObj, _ifStmtCacheStrategy).Sqls[0];

        //IF文の評価結果となったSQL文にPlaceHolderを適用する
        sql.SetPlaceHolder(placeHoldersObj.ToDictionary());

        //SQLを実行してResultsオブジェクトを取得する
        string sqlStr = sql.ToString(_aDb.GetSqlIndentType());
        IResults aResults = _aDbConn.ExecSelect(sqlStr
                                              , sql.GetSrcTableNames()
                                              , cacheStrategy);
        //発行したSQL文を記録しておく
        _lastExecutedSql = sqlStr;

        //RecordReaderを生成する
        IRecordReader<TRecord> ret = null;
        if(loadMode == Tran.LoadMode.ReadOnly) {
          ret = new ReadOnlyRecordReader<TRecord>(
            new RecordReaderImp<TRecord>(aResults, aViewInfo, _aCaster, _aDb.Dbms, this));
        } else {
          ret = new ReadWriteRecordReader<TRecord>(
            new RecordReaderImp<TRecord>(aResults, aViewInfo, _aCaster, _aDb.Dbms, this),
            _aLockManager, 
            _apTranId, 
            this.MakeTableAndAliasNames(aRecordViewTableMap, sql));
        }

        //次状態への遷移
        this.GoToNextState(State.DbTransaction);
        return ret;
      } catch(Exception ex) {
        this.GoToError1AndThrow(ex, true);
        //Warning対策
        throw;
      }
    }

    /// <summary>
    /// データベースからレコードを取得する
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="criteriaRec">抽出条件を格納したレコード</param>
    /// <param name="placeHolders">プレースホルダ名とその値</param>
    /// <param name="loadMode">読込モード</param>
    /// <param name="cacheStrategy">キャッシュ動作方式</param>
    /// <returns>取得したレコードのコレクション</returns>
    /// <remarks></remarks>
    public IRecordReader<TRecord> Find<TRecord>(TRecord criteriaRec
                                              , Dictionary<string, string> placeHolders
                                              , LoadMode loadMode = LoadMode.ReadOnly
                                              , CacheStrategy cacheStrategy = CacheStrategy.UseCache)
    where TRecord: class, IRecord, new() {
      //TRecord型レコードとViewのマッピング情報を取得する
      RecordViewTableMap<TRecord> aRecordViewTableMap = _aRecordViewTableMapFactory.CreateRecordViewTableMap<TRecord>();
      //RecordオブジェクトからQueryオブジェクトを生成する
      Query<TRecord> query = aRecordViewTableMap.CreateQuery(criteriaRec);

      return this.Find(query, placeHolders, loadMode, cacheStrategy);
    }

    /// <summary>
    /// データベースからレコードを取得する
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="query">Queryオブジェクト</param>
    /// <param name="loadMode">読込モード</param>
    /// <param name="cacheStrategy">キャッシュ動作方式</param>
    /// <returns>取得したレコードのコレクション</returns>
    /// <remarks></remarks>
    public IRecordReader<TRecord> Find<TRecord>(Query<TRecord> query
                                              , LoadMode loadMode = LoadMode.ReadOnly
                                              , CacheStrategy cacheStrategy = CacheStrategy.UseCache)
    where TRecord: class, IRecord, new() {
      return this.Find(query, null, loadMode, cacheStrategy);
    }

    /// <summary>
    /// データベースからレコードを取得する
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="criteriaRec">抽出条件を格納したレコード</param>
    /// <param name="loadMode">読込モード</param>
    /// <param name="cacheStrategy">キャッシュ動作方式</param>
    /// <returns>取得したレコードのコレクション</returns>
    /// <remarks></remarks>
    public IRecordReader<TRecord> Find<TRecord>(TRecord criteriaRec
                                              , LoadMode loadMode = LoadMode.ReadOnly
                                              , CacheStrategy cacheStrategy = CacheStrategy.UseCache) 
    where TRecord: class, IRecord, new() {
      return this.Find(criteriaRec, null, loadMode, cacheStrategy);
    }

    /// <summary>
    /// データベースからレコードを1件取得する
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="query">Queryオブジェクト</param>
    /// <param name="placeHolders">プレースホルダ名とその値</param>
    /// <param name="loadMode">読込モード</param>
    /// <param name="cacheStrategy">キャッシュ動作方式</param>
    /// <returns>取得したレコード</returns>
    /// <remarks>指定された抽出条件で2件以上のレコードが該当した場合は、例外を送出する</remarks>
    public TRecord FindOne<TRecord>(Query<TRecord> query
                                  , Dictionary<string, string> placeHolders
                                  , LoadMode loadMode = LoadMode.ReadOnly
                                  , CacheStrategy cacheStrategy = CacheStrategy.UseCache) 
    where TRecord: class, IRecord, new() {
      try {
        TRecord retRec = null;
        int i = 0;
        Reader<TRecord> reader = this.Find(query, placeHolders, loadMode, cacheStrategy);

        //結果レコードが2件以上在れば例外を送出する
        foreach(TRecord rec in reader) {
          retRec = rec;
          if(i > 0) {
            throw new MoreThanTwoRecordsException("FindOneメソッドに指定した抽出条件で2件以上のレコードが該当しました.");
          }
          i += 1;
        }

        return retRec;
      } catch(Exception ex) {
        this.GoToError1AndThrow(ex, true);
        throw;
      }
    }

    /// <summary>
    /// データベースからレコードを1件取得する
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="criteriaRec">抽出条件を格納したレコード</param>
    /// <param name="placeHolders">プレースホルダ名とその値</param>
    /// <param name="loadMode">読込モード</param>
    /// <param name="cacheStrategy">キャッシュ動作方式</param>
    /// <returns>取得したレコード</returns>
    /// <remarks>指定された抽出条件で2件以上のレコードが該当した場合は、例外を送出する</remarks>
    public TRecord FindOne<TRecord>(TRecord criteriaRec
                                  , Dictionary<string, string> placeHolders
                                  , LoadMode loadMode = LoadMode.ReadOnly
                                  , CacheStrategy cacheStrategy = CacheStrategy.UseCache) 
    where TRecord: class, IRecord, new() {
      //TRecord型レコードとViewのマッピング情報を取得する
      RecordViewTableMap<TRecord> aRecordViewTableMap = _aRecordViewTableMapFactory.CreateRecordViewTableMap<TRecord>();
      //RecordオブジェクトからQueryオブジェクトを生成する
      Query<TRecord> query = aRecordViewTableMap.CreateQuery(criteriaRec);

      return this.FindOne(query, placeHolders, loadMode, cacheStrategy);
    }

    /// <summary>
    /// データベースからレコードを1件取得する
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="query">Queryオブジェクト</param>
    /// <param name="loadMode">読込モード</param>
    /// <param name="cacheStrategy">キャッシュ動作方式</param>
    /// <returns>取得したレコード</returns>
    /// <remarks>指定された抽出条件で2件以上のレコードが該当した場合は、例外を送出する</remarks>
    public TRecord FindOne<TRecord>(Query<TRecord> query
                                  , LoadMode loadMode = LoadMode.ReadOnly
                                  , CacheStrategy cacheStrategy = CacheStrategy.UseCache) 
    where TRecord: class, IRecord, new() {
      return this.FindOne(query, null, loadMode, cacheStrategy);
    }

    /// <summary>
    /// データベースからレコードを1件取得する
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="criteriaRec">抽出条件を格納したレコード</param>
    /// <param name="loadMode">読込モード</param>
    /// <param name="cacheStrategy">キャッシュ動作方式</param>
    /// <returns>取得したレコード</returns>
    /// <remarks>指定された抽出条件で2件以上のレコードが該当した場合は、例外を送出する</remarks>
    public TRecord FindOne<TRecord>(TRecord criteriaRec
                                  , LoadMode loadMode = LoadMode.ReadOnly
                                  , CacheStrategy cacheStrategy = CacheStrategy.UseCache) 
    where TRecord: class, IRecord, new() {
      return this.FindOne(criteriaRec, null, loadMode, cacheStrategy);
    }

    /// <summary>
    /// データベースから抽出条件に一致するレコードの件数を返す
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="query">Queryオブジェクト</param>
    /// <param name="placeHolders">プレースホルダ名とその値</param>
    /// <returns>レコード件数</returns>
    /// <remarks>aQueryがQuery(Of TRecord)型でないのは、
    /// オーバーロード解決時に、ジェネリックパラメータの制約が無視される為</remarks>
    public int Count<TRecord>(Query<TRecord> query
                            , Dictionary<string, string> placeHolders = null) 
    where TRecord: class, IRecord, new() {
      try {
        //次の遷移先状態が仕様通りでなければ例外を送出する
        this.ThrowIfInvalidNextState(State.DbTransaction);

        //SQL文を組み立てる
        SqlBuilder sql = _sqlMaker.GetCountSql<TRecord>(query);

        //トランザクション中断状態の時は、トランザクションを開始しRollforwardを行う
        if(_state == State.OutOfDbTransaction) {
          _aDbConn = _aDb.GetDbConn();
          this.Rollforward();
        }

        //PlaceHolderオブジェクトを生成する
        placeHolders = placeHolders ?? new Dictionary<string, string>();
        PlaceHolders placeHoldersObj = new PlaceHolders(placeHolders);
        //プレースホルダ初期値コメントの値を設定する
        placeHoldersObj.Underwrite(sql.GetDefaultValuePlaceHolders());

        //IF文の場合は分岐条件を評価し、その評価結果となったSQL文を返す
        sql = IfStatement.Evaluate(_aDbConn, sql, placeHoldersObj, _ifStmtCacheStrategy).Sqls[0];

        //IF文の評価結果となったSQL文にPlaceHolderを適用する
        sql.SetPlaceHolder(placeHoldersObj.ToDictionary());

        //SQLを実行して、取得した値(件数)を返す
        string sqlStr = sql.ToString(_aDb.GetSqlIndentType());
        int ret = _aDbConn.ExecCount(sqlStr);

        //発行したSQL文を記録しておく
        _lastExecutedSql = sqlStr;

        //次状態への遷移
        this.GoToNextState(State.DbTransaction);
        return ret;
      } catch(Exception ex) {
        this.GoToError1AndThrow(ex, true);
        throw;
      }
    }

    /// <summary>
    /// データベースから抽出条件に一致するレコードの件数を返す
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="criteriaRec">抽出条件を格納したレコード</param>
    /// <param name="placeHolders">プレースホルダ名とその値</param>
    /// <returns>レコード件数</returns>
    /// <remarks></remarks>
    public int Count<TRecord>(TRecord criteriaRec
                            , Dictionary<string, string> placeHolders = null) 
    where TRecord: class, IRecord, new() {
      //TRecord型レコードとViewのマッピング情報を取得する
      RecordViewTableMap<TRecord> aRecordViewTableMap = _aRecordViewTableMapFactory.CreateRecordViewTableMap<TRecord>();
      //RecordオブジェクトからQueryオブジェクトを生成する
      Query<TRecord> query = aRecordViewTableMap.CreateQuery(criteriaRec);

      return this.Count(query, placeHolders);
    }

    /// <summary>
    /// SQLエントリを実行する
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="useRecordForCriteria">
    /// queryを無視してvalueRecordを抽出条件と見做す:True
    /// 見做さない:False
    /// </param>
    /// <param name="valueRecord">保存/更新値を格納したレコード</param>
    /// <param name="query">抽出条件を指定するQueryオブジェクト</param>
    /// <returns></returns>
    /// <remarks></remarks>
    private int CallImp<TRecord>(string sqlEntryName
                                , bool useRecordForCriteria
                                , TRecord valueRecord
                                , Query<TRecord> query
                                , Dictionary<string, string> placeHoldersDic = null) 
    where TRecord: class, IRecord, new() {
      try {
        //次の遷移先状態が仕様通りでなければ例外を送出する
        this.ThrowIfInvalidNextState(State.DbTransaction);

        //TRecord型レコードとテーブルのマッピング情報を取得する
        RecordViewTableMap<TRecord> aRecordViewTableMap = _aRecordViewTableMapFactory.CreateRecordViewTableMap<TRecord>();
        //SqlPodを取得する
        SqlPod aSqlPod = _aSqlPodFactory.CreateSqlPod<TRecord>();

        //引数のplaceHoldersDicで指定された値を持つプレースホルダオブジェクトを生成する
        placeHoldersDic = placeHoldersDic ?? new Dictionary<string, string>();
        PlaceHolders placeHolders = new PlaceHolders(placeHoldersDic);

        //RecordTableMap.GetPredicates()で参照するためのプレースホルダを生成する
        PlaceHolders nonPropertyPlaceHolders = (PlaceHolders)placeHolders.Clone();
        //レコードのプロパティ値をプレースホルダに格納する
        //(レコードのプロパティ名と同じプレースホルダ名が指定された場合は引数で指定された値を優先する)
        PlaceHolders placeHoldersForIf = (PlaceHolders)placeHolders.Clone();
        if(valueRecord != null) {
          placeHoldersForIf.Underwrite(aRecordViewTableMap.CreatePlaceHolders(valueRecord));
        }

        SqlBuilders sqls = aSqlPod.GetEntrySqls(sqlEntryName);

        //トランザクション中断状態の時は、トランザクションを開始しRollforwardを行う
        if(_state == State.OutOfDbTransaction) {
          _aDbConn = _aDb.GetDbConn();
          this.Rollforward();
        }

        int maxAffectedRows = 0;

        maxAffectedRows = this.CallImp(sqls
                                      , sqlEntryName
                                      , useRecordForCriteria
                                      , valueRecord
                                      , query
                                      , aRecordViewTableMap
                                      , nonPropertyPlaceHolders
                                      , placeHoldersForIf
                                      , new PlaceHolders()
                                      , 0).Item1;

        //次状態への遷移
        this.GoToNextState(State.DbTransaction);
        return maxAffectedRows;
      } catch(Exception ex) {
        this.GoToError1AndThrow(ex, true);
        throw;
      }
    }

    /// <summary>
    /// IF文があれば再帰的にSQL文を発行し指定されたSQLエントリを実行する
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="sqls"></param>
    /// <param name="sqlEntryName"></param>
    /// <param name="useRecordForCriteria"></param>
    /// <param name="valueRecord"></param>
    /// <param name="query"></param>
    /// <param name="aRecordViewTableMap"></param>
    /// <param name="nonPropertyPlaceHolders"></param>
    /// <param name="placeHoldersForIf"></param>
    /// <returns>(maxAffectedRows, currentSeq)</returns>
    /// <remarks></remarks>
    private Tuple<int, int> CallImp<TRecord>(SqlBuilders sqls
                                            , string sqlEntryName
                                            , bool useRecordForCriteria
                                            , TRecord valueRecord
                                            , Query<TRecord> query
                                            , RecordViewTableMap<TRecord> aRecordViewTableMap
                                            , PlaceHolders nonPropertyPlaceHolders
                                            , PlaceHolders placeHoldersForIf
                                            , PlaceHolders defaultValuePlaceHolders
                                            , int maxAffectedRows) 
    where TRecord: class, IRecord, new() {

      int lastAffectedRows = 0;
      int currentSeq = 0;

      //MoveNext()内でIF文の分岐条件を評価しているため、先にRollforward()を実行すること
      //While sqls.MoveNext(_aDbConn, lastAffectedRows)

      foreach(SqlBuilder sql in sqls) {
        //LAST_AFFECTED_ROWSの設定
        placeHoldersForIf["LAST_AFFECTED_ROWS"] = lastAffectedRows.ToString();
        //プレースホルダ初期値コメントの値を設定する
        defaultValuePlaceHolders.Overwrite(sql.GetDefaultValuePlaceHolders());
        //プレースホルダ初期値はSQL文の発行の度に値を追加設定するので、placeHoldersForIfは変更せず
        //placeHoldersForIfのクローンを用意しこれにプレースホルダ初期値を設定する
        PlaceHolders placeHoldersForIf_new = (PlaceHolders)placeHoldersForIf.Clone();
        placeHoldersForIf_new.Underwrite(defaultValuePlaceHolders);

        //IF文の場合
        if(sql.GetStatementType() == SqlBuilder.StatementType.If) {
          //IF文の場合は分岐条件を評価し、その評価結果となったSQL文を取得する
          IfStatement.SqlAndSeq sqlAndSeq = IfStatement.Evaluate(_aDbConn, sql, placeHoldersForIf_new, _ifStmtCacheStrategy);
          Tuple<int, int> t = this.CallImp(sqlAndSeq.Sqls
                                          , sqlEntryName
                                          , useRecordForCriteria
                                          , valueRecord
                                          , query
                                          , aRecordViewTableMap
                                          , nonPropertyPlaceHolders
                                          , placeHoldersForIf
                                          , defaultValuePlaceHolders
                                          , maxAffectedRows);
          maxAffectedRows = t.Item1;
          currentSeq = t.Item2;
        } else if(sql.GetStatementType() == SqlBuilder.StatementType.Null) {
          //NULL文の場合その実行処理をスキップする
          continue;
        } else {
          //SQL文末の";"を削除する
          sql.TrimRightSeparators();

          //WHERE句を持ち得ないSQL文は、WHERE句の自動付加処理をスキップする
          if(sql.GetStatementType() == SqlBuilder.StatementType.Update ||
             sql.GetStatementType() == SqlBuilder.StatementType.Delete ||
             sql.GetStatementType() == SqlBuilder.StatementType.Select) {
            //valueRecordを抽出条件と見做す場合
            if(useRecordForCriteria && valueRecord != null) {
              //UPDATE文に対しては特例的に、valueRecordの主キープロパティを抽出条件に、
              //それ以外のプロパティを更新値に見做す
              if(sql.GetStatementType() == SqlBuilder.StatementType.Update) {
                TRecord keyRecord = aRecordViewTableMap.SplitForKey2(valueRecord, sql.GetTargetTable())[0];
                query = aRecordViewTableMap.CreateQuery(keyRecord);
              } else {
                query = aRecordViewTableMap.CreateQuery(valueRecord);
              }
            }

            if(sql.GetStatementType() == SqlBuilder.StatementType.Select) {
              //
              // SELECT文の実行結果は返さないが、結果行数の制限はSELECT文の速度に影響するので
              // Queryに指定が在れば採用する
              //

              //Query.RowRange()で抽出条件が指定されている場合
              if(query != null && query.GetRowRange() != null && aRecordViewTableMap != null) {
                int begin = query.GetRowRange().Item1;
                int end = query.GetRowRange().Item2;
                sql.SetRowLimit(begin, end);
              }

              //最大抽出件数の設定
              //OracleのRowNumによる指定は、SELECT * FROMに指定する
              if(query != null && query.GetMaxRows() >= 0) {
                sql.SetMaxRows(query.GetMaxRows());
              }

            }

            // AutoWhere設定値がTrueの場合は抽出条件を付加する
            if(sql.GetAutoWhere()) {
              //WHERE句を作成する
              SqlPredicate aPredicate = aRecordViewTableMap.GetPredicates(sql
                                                                        , query
                                                                        , nonPropertyPlaceHolders
                                                                        , placeHoldersForIf_new
                                                                        , new SqlId(sqlEntryName, currentSeq)
                                                                        , _aDbConn
                                                                        , lastAffectedRows);
              sql.AddAndPredicate(aPredicate);
            }

          }


          //valueRecordプレースホルダ値の適用
          PlaceHolders placeHoldersForStmt = null;
          //プレースホルダのコピーを用いる
          PlaceHolders nonPropertyPlaceHolders_new = (PlaceHolders)nonPropertyPlaceHolders.Clone();

          if(valueRecord != null) {
            placeHoldersForStmt = this.CreatePlaceHolders(aRecordViewTableMap
                                                        , nonPropertyPlaceHolders_new
                                                        , sql
                                                        , valueRecord);
          } else {
            placeHoldersForStmt = nonPropertyPlaceHolders_new;
          }

          //プレースホルダ初期値コメントの値を設定する
          placeHoldersForStmt.Underwrite(defaultValuePlaceHolders);
          sql.SetPlaceHolder(placeHoldersForStmt.ToDictionary());

          //更新対象行をロックする
          if(!_aLockManager.Lock<TRecord>(_apTranId, sql)) {
            throw new WriteToLockedRecordException(
              "他トランザクションによってロックされているレコードを更新しようとしました", _apTranId);
          }
          //直前のSQLの処理行数は、次に実行するSQLにプレースホルダとして渡される
          string sqlStr = sql.ToString(_aDb.GetSqlIndentType());
          lastAffectedRows = _aDbConn.ExecSql(sqlStr, sql.GetSrcTableNames());

          //発行したSQL文を記録しておく
          _lastExecutedSql = sqlStr;

          //SQLの実行履歴に追加する
          if(lastAffectedRows != 0) {
            //SELECT文は更新対象テーブルを持たない
            string targetTableName = "";
            if(sql.GetTargetTable() != null) {
              targetTableName = sql.GetTargetTable().Name;
            }
            _executedSqls.Add(new ExecutedSql(sqlStr, targetTableName));
          }

          //SQL文のシーケンス番号をインクリメントする
          currentSeq += 1;

        }

        //最も大きい処理件数を、そのレコードに対する処理件数とする
        //(更新しないSQL文は処理件数0とする)
        if(sql.GetStatementType() != SqlBuilder.StatementType.Select) {
          maxAffectedRows = Math.Max(lastAffectedRows, maxAffectedRows);
        }

      }

      return Tuple.Create(maxAffectedRows, currentSeq);
    }

    public int Call<TRecord>(string sqlEntryName
                           , TRecord valueRecord, Query<TRecord> query
                           , Dictionary<string, string> placeHolders = null) 
    where TRecord: class, IRecord, new() {
      return this.CallImp(sqlEntryName, false, valueRecord, query, placeHolders);
    }

    public int Call<TRecord>(string sqlEntryName
                           , TRecord valueRecord
                           , Dictionary<string, string> placeHolders = null) 
    where TRecord: class, IRecord, new() {
      return this.CallImp(sqlEntryName, true, valueRecord, null, placeHolders);
    }

    public int Call<TRecord>(string sqlEntryName
                           , Query<TRecord> query
                           , Dictionary<string, string> placeHolders = null) 
    where TRecord: class, IRecord, new() {
      return this.CallImp(sqlEntryName, false, null, query, placeHolders);
    }

    public int Save<TRecord>(TRecord saveRecord
                           , Dictionary<string, string> placeHolders = null) 
    where TRecord: class, IRecord, new() {
      return this.Call("save", saveRecord, placeHolders);
    }

    public int Delete<TRecord>(TRecord criteriaRec
                             , Dictionary<string, string> placeHolders = null) 
    where TRecord: class, IRecord, new() {
      return this.Call("delete", criteriaRec, placeHolders);
    }
  }
}
