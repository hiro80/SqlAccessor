using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using MiniSqlParser;

namespace SqlAccessor
{
  /// <summary>
  /// レコードとViewとテーブルのマッピングを表す
  /// </summary>
  /// <typeparam name="TRecord"></typeparam>
  /// <remarks></remarks>
  internal class RecordViewTableMap<TRecord>: IViewInfoGetter
  where TRecord: class, IRecord, new()
  {
    private class Maps: IEnumerable<Map>
    {
      //(テーブル別名(テーブル別名がない場合はテーブル名)、Mapオブジェクトのリスト)
      // Dictionaryキーのテーブル別名は大文字で登録する
      private readonly Dictionary<string, List<Map>> _mapList = new Dictionary<string, List<Map>>();
      /// <summary>
      /// マッピング情報を一つ追加する
      /// </summary>
      /// <param name="newMap"></param>
      /// <remarks></remarks>
      public void Add(Map newMap) {
        string upperCaseTableAliasName = newMap.Table.AliasName.ToUpper();
        if(_mapList.ContainsKey(upperCaseTableAliasName)) {
          _mapList[upperCaseTableAliasName].Add(newMap);
        } else {
          List<Map> newMapList = new List<Map>();
          newMapList.Add(newMap);
          _mapList.Add(upperCaseTableAliasName, newMapList);
        }
      }
      public void AddRange(Maps newMaps) {
        foreach(Map newMap in newMaps) {
          this.Add(newMap);
        }
      }
      public void Remove(Map map) {
        string upperCaseTableAliasName = map.Table.AliasName.ToUpper();
        if(!_mapList.ContainsKey(upperCaseTableAliasName)) {
          return;
        }
        Map removeMap = null;
        List<Map> mapSubList = _mapList[upperCaseTableAliasName];
        foreach(Map aMap in mapSubList) {
          if(aMap == map) {
            if(mapSubList.Count == 1) {
              _mapList.Remove(upperCaseTableAliasName);
            } else {
              removeMap = aMap;
            }
            break;
          }
        }
        //ForEach内ではイテレート対象を削除できないため、ここで削除する
        if(removeMap != null) {
          mapSubList.Remove(removeMap);
        }
      }
      public List<Map> Find() {
        List<Map> ret = new List<Map>();
        foreach(KeyValuePair<string, List<Map>> kv in _mapList) {
          ret.AddRange(kv.Value);
        }
        return ret;
      }
      public List<Map> Find(string tableAliasName) {
        string tableAliasUpperName = tableAliasName.ToUpper();
        if(_mapList.ContainsKey(tableAliasUpperName)) {
          return _mapList[tableAliasUpperName];
        } else {
          //指定したテーブル別名に対応するMapがなければ空Listを返す
          return new List<Map>();
        }
      }
      //テーブルカラム名が一致する最初のMapを返す
      public Map FindFirstByColumnName(string columnName) {
        foreach(KeyValuePair<string, List<Map>> kv in _mapList) {
          foreach(Map aMap in kv.Value) {
            if(aMap.ColumnName == columnName) {
              return aMap;
            }
          }
        }
        return null;
      }

      //プロパティ名に対して、一対一で対応するテーブル列が存在すれば、そのMapオブジェクトを返す
      public Map Find1to1Column(string propertyName) {
        List<Map> aMaps = this.Find();
        foreach(Map aMap in aMaps) {
          if(aMap.PropPlaceHolders.Count == 1 &&
             aMap.PropPlaceHolders[0].Item1 == propertyName &&
             aMap.Expression.IsPlaceHolderOnly) {
            return aMap;
          }
        }

        return null;
      }

      //テーブル別名とプロパティ名に対して、一対一で対応するテーブル列が存在すれば、そのMapオブジェクトを返す
      public Map Find1to1Column(string tableAliasName
                              , string propertyName) {
        List<Map> aMaps = this.Find(tableAliasName);
        foreach(Map aMap in aMaps) {
          if(aMap.PropPlaceHolders.Count == 1 &&
             aMap.PropPlaceHolders[0].Item1 == propertyName &&
             aMap.Expression.IsPlaceHolderOnly) {
            return aMap;
          }
        }

        return null;
      }

      public SqlTable GetTableAndAliasName(SqlId sqlId) {
        foreach(KeyValuePair<string, List<Map>> recordNameAndList in _mapList) {
          foreach(Map map in recordNameAndList.Value) {
            if(map.SrcSqlId == sqlId) {
              return new SqlTable(map.Table.Name, map.Table.AliasName);
            }
          }
        }
        throw new ArgumentOutOfRangeException(
            "sqlId"
          , "指定したsqlId=" + sqlId.ToString() + "に一致するMap情報が見つかりませんでした");
      }
      public List<Tuple<string, string>> GetPlacedPropPlaceHolders() {
        List<Tuple<string, string>> ret = new List<Tuple<string, string>>();
        foreach(Map aMap in this) {
          foreach(Tuple<string, string> propPlaceHolder in aMap.PropPlaceHolders) {
            if(!ret.Contains(propPlaceHolder)) {
              ret.Add(propPlaceHolder);
            }
          }
        }
        return ret;
      }

      public int Count {
        get { return _mapList.Count; }
      }

      public IEnumerator<Map> GetEnumerator() {
        List<Map> ret = new List<Map>();

        foreach(KeyValuePair<string, List<Map>> kv in _mapList) {
          ret.AddRange(kv.Value);
        }

        return ret.GetEnumerator();
      }
      public IEnumerator GetEnumerator1() {
        return this.GetEnumerator();
      }
      IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator1();
      }
    }

    private class Map
    {
      //抽出元SQL文のID
      public readonly SqlId SrcSqlId;
      //抽出元SQL文の種別
      public readonly SqlBuilder.StatementType StatementType;
      //テーブル名
      public readonly SqlTable Table;
      //テーブルカラム名
      public readonly string ColumnName;
      //テーブルカラムが主キーの場合True
      public readonly bool IsPrimaryKey;
      //Mapping採用条件
      public readonly SqlPredicate Condition;
      //テーブルカラムに格納する値を生成する式
      public readonly SqlExpr Expression;
      //プロパティ名プレースホルダとその適用値
      public readonly List<Tuple<string, string>> PropPlaceHolders;
      public Map(SqlId srcSqlId
                , SqlBuilder.StatementType statementType
                , SqlTable table
                , string columnName
                , bool isPrimaryKey
                , SqlPredicate condition
                , SqlExpr expression
                , List<Tuple<string, string>> propPlaceHolders) {
        this.SrcSqlId = srcSqlId;
        this.StatementType = statementType;
        this.Table = table;
        this.ColumnName = columnName;
        this.IsPrimaryKey = isPrimaryKey;
        this.Condition = condition;
        this.Expression = expression;
        this.PropPlaceHolders = propPlaceHolders;
      }

      public Map PlaceExpression(string placeHolderName, string value) {
        SqlExpr newExpression = this.Expression.Clone();
        newExpression.Place(placeHolderName, value);
        Dictionary<string, string> dic = new Dictionary<string, string>();
        dic.Add(placeHolderName, value);
        List<Tuple<string, string>> newPropPlaceHolders 
          = this.SetPropPlaceHolders(this.PropPlaceHolders, new PlaceHolders(dic));
        return new Map(this.SrcSqlId
                      , this.StatementType
                      , this.Table
                      , this.ColumnName
                      , this.IsPrimaryKey
                      , this.Condition
                      , newExpression
                      , newPropPlaceHolders);
      }
      public Map PlaceExpression(PlaceHolders aPlaceHolders, int lastAffectedRows) {
        SqlExpr newExpression 
          = this.SetPlaceHolders(this.Expression, aPlaceHolders, lastAffectedRows);
        List<Tuple<string, string>> newPropPlaceHolders 
          = this.SetPropPlaceHolders(this.PropPlaceHolders, aPlaceHolders);
        return new Map(this.SrcSqlId
                      , this.StatementType
                      , this.Table
                      , this.ColumnName
                      , this.IsPrimaryKey
                      , this.Condition
                      , newExpression
                      , newPropPlaceHolders);
      }
      public Map PlaceCondition(PlaceHolders aPlaceHolders, int lastAffectedRows) {
        SqlPredicate newCondition = this.SetPlaceHolders(this.Condition, aPlaceHolders, lastAffectedRows);
        return new Map(this.SrcSqlId
                      , this.StatementType
                      , this.Table
                      , this.ColumnName
                      , this.IsPrimaryKey
                      , newCondition
                      , this.Expression
                      , this.PropPlaceHolders);
      }
      private SqlExpr SetPlaceHolders(SqlExpr expression
                                    , PlaceHolders aPlaceHolders
                                    , int lastAffectedRows) {
        if(expression == null) {
          return new SqlExpr();
        }

        // Place()はオブジェクトの状態を変更するので複製に対して操作する
        SqlExpr newExpression = expression.Clone();

        //PlaceHoldersオブジェクトで指定されているプレースホルダを適用する
        foreach(KeyValuePair<string, string> aKeyValuePair in aPlaceHolders) {
          string placeHolder = aKeyValuePair.Key;
          string value = aKeyValuePair.Value;
          newExpression.Place(placeHolder, value);
        }

        //プレースホルダ"LAST_AFFECTED_ROWS"を適用する
        newExpression.Place("LAST_AFFECTED_ROWS", lastAffectedRows.ToString());

        return newExpression;
      }
      private SqlPredicate SetPlaceHolders(SqlPredicate condition
                                         , PlaceHolders aPlaceHolders
                                         , int lastAffectedRows) {
        if(condition == null) {
          return new SqlPredicate();
        }

        // Place()はオブジェクトの状態を変更するので複製に対して操作する
        SqlPredicate newCondition = condition.Clone();

        //PlaceHoldersオブジェクトで指定されているプレースホルダを適用する
        foreach(KeyValuePair<string, string> aKeyValuePair in aPlaceHolders) {
          string placeHolder = aKeyValuePair.Key;
          string value = aKeyValuePair.Value;
          newCondition.Place(placeHolder, value);
        }

        //プレースホルダ"LAST_AFFECTED_ROWS"を適用する
        newCondition.Place("LAST_AFFECTED_ROWS", lastAffectedRows.ToString());

        return newCondition;
      }
      private List<Tuple<string, string>> SetPropPlaceHolders(List<Tuple<string, string>> propPlaceHolders
                                                            , PlaceHolders aPlaceHolders) {
        List<Tuple<string, string>> ret = new List<Tuple<string, string>>(propPlaceHolders.Count);

        //propPlaceHoldersからretへ1要素ずつコピーする
        foreach(Tuple<string, string> propPlaceHolder in propPlaceHolders) {
          string propPlaceHolderName = propPlaceHolder.Item1;
          if(aPlaceHolders.Contains(propPlaceHolderName)) {
            //PlaceHolderにプロパティプレースホルダの値が指定されていれば、
            //その値をretに適用する
            ret.Add(new Tuple<string, string>(propPlaceHolderName
                                            , aPlaceHolders[propPlaceHolderName]));
          } else {
            //Pairオブジェクトは不変オブジェクトなので、propPlaceHoldersとretで共有可能
            ret.Add(propPlaceHolder);
          }
        }

        return ret;
      }
      public void AddPlacedPropPlaceHolders(List<Tuple<string, string>> placedPropPlaceHolders) {
        this.PropPlaceHolders.AddRange(placedPropPlaceHolders);
      }
      public static bool operator ==(Map map1, Map map2) {
        //テーブル別名の大文字小文字の区別をしない
        if((object)map1 != null && (object)map2 != null &&
           map1.SrcSqlId == map2.SrcSqlId &&
           map1.Table.AliasName.ToUpper() == map2.Table.AliasName.ToUpper() &&
           map1.ColumnName == map2.ColumnName) {
          return true;
        } else {
          return (object)map1 == null && (object)map2 == null;
        }
      }
      public static bool operator !=(Map map1, Map map2) {
        return !(map1 == map2);
      }
    }

    private static RecordViewTableMap<TRecord> _recordViewTableMap;
    //ロック用オブジェクト
    private static readonly object _lock = new object();

    private readonly ICaster _aCaster;
    private readonly TableInfoSet _aTableInfoSet;
    private readonly Tran.CacheStrategy _ifStmtCacheStrategy;
    private readonly SqlPod _sqlPod;
    private readonly Maps _mapSet = new Maps();

    //TRecordにマッピングするView
    private readonly ViewInfo _viewInfo;

    private RecordViewTableMap(TableInfoSet aTableInfoSet
                              , ICaster aCaster
                              , SqlPodFactory aSqlPodFactory
                              , Tran.CacheStrategy ifStmtCacheStrategy, Db aDb) {
      _aCaster = aCaster;
      _aTableInfoSet = aTableInfoSet;
      _ifStmtCacheStrategy = ifStmtCacheStrategy;
      _sqlPod = aSqlPodFactory.CreateSqlPod<TRecord>();
      _viewInfo = new ViewInfo(this.GetRecordInfo().Name, _sqlPod, _aTableInfoSet, aDb);
      this.LoadMapping(_sqlPod);
    }

    public static RecordViewTableMap<TRecord> GetInstance(TableInfoSet aTableInfoSet
                                                        , ICaster aCaster
                                                        , SqlPodFactory aSqlPodFactory
                                                        , Tran.CacheStrategy ifStmtCacheStrategy
                                                        , Db aDb) {
      //Double-Checked Lockingパターン
      if(_recordViewTableMap == null) {
        lock(_lock) {
          if(_recordViewTableMap == null) {
            _recordViewTableMap = new RecordViewTableMap<TRecord>(aTableInfoSet
                                                                , aCaster
                                                                , aSqlPodFactory
                                                                , ifStmtCacheStrategy
                                                                , aDb);
          }
        }
      }

      return _recordViewTableMap;
    }

    private void LoadMapping(SqlPod aSqlPod) {
      //'find','count'以外の全てのSQLエントリからSQL文を取得する
      foreach(string entryName in aSqlPod.GetAllEntryNames()) {
        // Queryオブジェクトで指定されたプレースホルダはMe.SplitFotTables()で適用するので、ここでは適用しない
        // 定義済みプレースホルダはPlaceHolderのコンストラクタ内で設定される
        SqlId sqlId = new SqlId(entryName, 0);
        SqlBuilders sqls = aSqlPod.GetEntrySqlsForMapping(entryName);
        //SQL文の取得処理
        this.LoadMappingSub(sqls, sqlId);
      }
    }

    private void LoadMappingSub(SqlBuilders sqls
                              , SqlId sqlId
                              , SqlPredicate condition = null) {
      int i = 0;

      foreach(SqlBuilder sql in sqls) {
        if(sql.GetStatementType() == SqlBuilder.StatementType.InsertValue) {
          //INSERT-SELECT文には暫定的に対応しない
          this.AddMap(sql, sqlId + i, condition);

        } else if(sql.GetStatementType() == SqlBuilder.StatementType.Update) {
          //UPDATE文の場合は、WHERE句の条件式を"EXISTS (SELECT ..."の条件部に書き出し、
          //Mapping採用条件にAND連結する
          SqlPredicate updateWhereCondition = null;
          if(!sql.GetWhere().IsEmpty) {
            SqlBuilder dummySql = new SqlBuilder("SELECT * FROM T WHERE EXISTS (SELECT * FROM " + 
                                                  sql.GetTargetTable().Name + " WHERE " + sql.GetWhere().ToString() + " )"
                                                , sql.Dbms);
            updateWhereCondition = dummySql.GetWhere();
          }

          if(condition == null) {
            this.AddMap(sql, sqlId + i, updateWhereCondition);
          } else {
            this.AddMap(sql, sqlId + i, condition.And(updateWhereCondition));
          }

        } else if(sql.GetStatementType() == SqlBuilder.StatementType.If) {
          //IF文内のSQL文を全て登録する
          foreach(Tuple<SqlPredicate, SqlBuilders> branch in sql.GetIfBranches()) {
            //sqlIdはIF文内の順序を加算する
            this.LoadMappingSub(branch.Item2, sqlId + i, branch.Item1);
            i += 1;
          }

        }

        //SqlIdのインクリメント
        i += 1;
      }
    }

    private void AddMap(SqlBuilder sql
                      , SqlId sqlId
                      , SqlPredicate condition = null) {
      string recordName = RecordInfo<TRecord>.GetInstance().Name;
      SqlTable targetTable = sql.GetTargetTable();
      SqlBuilder.StatementType statementType = sql.GetStatementType();

      foreach(KeyValuePair<string, IEnumerable<SqlExpr>> assignment in sql.GetAssignments()) {
        string columnName = assignment.Key;
        ColumnInfo columnInfo = this.GetTableColumnInfo(targetTable.Name, columnName);
        if(columnInfo == null) {
          throw new NotExistsTableColumnException(
            "テーブル" + targetTable.Name + "に列" + columnName + "が存在しません");
        }

        bool isPrimaryKey = columnInfo.PrimaryKey ?? false;
        foreach(SqlExpr expression in assignment.Value) {
          _mapSet.Add(
            new Map(sqlId
                  , statementType
                  , targetTable
                  , columnName
                  , isPrimaryKey
                  , condition
                  , expression
                  , this.GetAllPropPlaceHolders(expression)));
        }
      }
    }

    //式に含まれる全てのプロパティプレースホルダを取得する
    private List<Tuple<string, string>> GetAllPropPlaceHolders(SqlExpr expression) {
      List<Tuple<string, string>> ret = new List<Tuple<string, string>>();
      foreach(KeyValuePair<string, string> aPlaceHolder in expression.GetAllPlaceHolders()) {
        //ViewColumnInfoに登録されているプレースホルダ名であれば、
        //プロパティプレースホルダである
        if(this.GetViewColumnInfo(aPlaceHolder.Key) != null) {
          ret.Add(new Tuple<string, string>(aPlaceHolder.Key, aPlaceHolder.Value));
        }
      }
      return ret;
    }

    public Query<TRecord> CreateQuery(TRecord aRecord) {
      Query<TRecord> aQuery = new Query<TRecord>();
      RecordInfo<TRecord> recordInfo = this.GetRecordInfo();
      PropertyInfo[] properties = recordInfo.Properties;

      foreach(PropertyInfo aProperty in properties) {
        //Viewカラムに紐付いていないプロパティは一致条件と見做さない
        ViewColumnInfo aViewColumnInfo = this.GetViewColumnInfo(aProperty.Name);
        if(aViewColumnInfo == null) {
          continue;
        }

        //Queryオブジェクトに一致条件を一つ追加する
        string propertyname = aProperty.Name;
        object propertyValue = aProperty.GetValue(aRecord, null);
        aQuery = this.AddMatchingCondition(aQuery, aViewColumnInfo.ViewColumnName, propertyValue);
      }

      return aQuery;
    }

    /// <summary>
    /// Queryオブジェクトに一致条件を一つ追加する
    /// </summary>
    /// <param name="aQuery">条件の追加対象(Queryオブジェクト)</param>
    /// <param name="propertyName">一致条件の対象プロパティ名</param>
    /// <param name="propertyValue">一致条件の値</param>
    /// <returns>Queryオブジェクト</returns>
    /// <remarks></remarks>
    private Query<TRecord> AddMatchingCondition(Query<TRecord> aQuery
                                              , string propertyName
                                              , object propertyValue) {
      //プロパティがNULL表現値ならば一致条件と見做さない
      if(propertyValue == null || _aCaster.IsNullPropertyValue(propertyValue)) {
        return aQuery;
      }

      aQuery.And(val.of(propertyName) == propertyValue);
      return aQuery;
    }

    public Query<TRecord> CastQuery(Query<TRecord> aQuery) {
      ((IQueryExtention<TRecord>)aQuery).CastToSqlLiteralType(_aCaster, this);
      return aQuery;
    }

    /// <summary>
    /// レコードの更新によって変更されうるテーブルを取得する
    /// </summary>
    /// <returns></returns>
    /// <remarks>テーブル別名、又は実テーブル名</remarks>
    private List<string> GetTableNames() {
      List<string> ret = new List<string>();
      foreach(Map aMap in _mapSet.Find()) {
        string tableName = aMap.Table.AliasName;
        if(!ret.Contains(tableName)) {
          ret.Add(tableName);
        }
      }
      return ret;
    }

    public ViewInfo GetViewInfo() {
      return _viewInfo;
    }

    public ViewColumnInfo GetViewColumnInfo(string propertyName) {
      ViewColumnInfo ret = this.GetViewInfo()[propertyName];

      //Viewカラムに対応するプロパティだがaProperty.GetValue()で値を取得できないプロパティが
      //存在する場合はエラーを送出する
      if(ret != null) {
        PropertyInfo aPropetyInfo = this.GetRecordInfo().GetPropertyInfo(propertyName);
        if(aPropetyInfo != null &&
          !aPropetyInfo.CanRead &&
          !aPropetyInfo.CanWrite) {
          throw new BadFormatRecordException(
            "Viewカラムに対応するプロパティは読み書き可能にする必要があります.");
        }
      }

      return ret;
    }

    public TableInfo GetTableInfo(string tableName) {
      TableInfo tableInfo = _aTableInfoSet[tableName];
      if(tableInfo == null) {
        throw new NotExistsTableException("テーブル" + tableName + "がデータベースに存在しません");
      }
      return tableInfo;
    }

    /// <summary>
    /// テーブル別名とプロパティ名から、一対一で対応するテーブル列を取得する
    /// </summary>
    /// <param name="tableAliasName">更新対象テーブル別名</param>
    /// <param name="propertyName">プロパティ名</param>
    /// <returns></returns>
    /// <remarks>一対一で対応するテーブル列がない場合はNothingを返す</remarks>
    public ColumnInfo GetTableColumnInfoByProp(string tableAliasName
                                             , string propertyName) {
      Map aMap = _mapSet.Find1to1Column(tableAliasName, propertyName);
      if(aMap != null) {
        return this.GetTableColumnInfo(aMap.Table.Name, aMap.ColumnName);
      }

      //一対一で対応するテーブルカラムが無ければNothingを返す
      return null;
    }

    /// <summary>
    /// テーブル別名とプロパティ名から、一対一で対応するテーブル列を取得する
    /// </summary>
    /// <param name="preferredTableAliasName">優先する更新対象テーブル別名</param>
    /// <param name="propertyName">プロパティ名</param>
    /// <returns></returns>
    /// <remarks>一対一で対応するテーブル列がない場合はNothingを返す</remarks>
    public ColumnInfo GetTableColumnInfoByProp2(string preferredTableAliasName
                                              , string propertyName) {
      Map aMap = _mapSet.Find1to1Column(preferredTableAliasName, propertyName);
      if(aMap != null) {
        return this.GetTableColumnInfo(aMap.Table.Name, aMap.ColumnName);
      }

      aMap = _mapSet.Find1to1Column(propertyName);
      if(aMap != null) {
        //一致する更新対象テーブル別名が無い場合、
        //更新対象テーブル別名を条件から除外して再取得を試みる
        return this.GetTableColumnInfo(aMap.Table.Name, aMap.ColumnName);
      }

      //一対一で対応するテーブルカラムが無ければNothingを返す
      return null;
    }

    public ColumnInfo GetTableColumnInfo(string tableName, string columnName) {
      return this.GetTableInfo(tableName)[columnName];
    }

    public RecordInfo<TRecord> GetRecordInfo() {
      return RecordInfo<TRecord>.GetInstance();
    }

    public PlaceHolders UnderwritePlaceHolders(PlaceHolders aPlaceHolders
                                             , string aTableAliasName
                                             , TRecord aRecord) {
      if(aPlaceHolders == null) {
        throw new ArgumentNullException("aPlaceHolders", "aPlaceHoldersがNullです");
      }

      //aRecordオブジェクトのプロパティ値をプレースホルダに格納する
      //(aRecordがNothingであれば、処理をスキップする)
      if(aRecord == null) {
        return aPlaceHolders;
      }

      RecordInfo<TRecord> recordInfo = this.GetRecordInfo();
      string recordName = recordInfo.Name;
      PropertyInfo[] properties = recordInfo.Properties;
      foreach(PropertyInfo aProperty in properties) {
        string propertyName = aProperty.Name;

        //Viewカラムに対応しないプロパティはスキップする
        ViewColumnInfo aViewColumnInfo = this.GetViewColumnInfo(propertyName);
        if(aViewColumnInfo == null) {
          continue;
        }

        object propertyValue = aProperty.GetValue(aRecord, null);

        if(!string.IsNullOrEmpty(aTableAliasName)) {
          //プロパティに一対一対応するテーブルカラムが存在するか確認する
          ColumnInfo aColumnInfo = this.GetTableColumnInfoByProp(aTableAliasName, propertyName);
          if(aColumnInfo != null) {
            aPlaceHolders.Underwrite(propertyName, _aCaster.CastToSqlLiteralType(propertyValue, aViewColumnInfo, aColumnInfo));
          } else {
            //プロパティ名に紐づくテーブルカラムが2つ以上存在する場合
            //CastToSqlLiteralType()にテーブルカラムのメタ情報は指定しない
            aPlaceHolders.Underwrite(propertyName, _aCaster.CastToSqlLiteralType(propertyValue, aViewColumnInfo));
          }
        } else {
          aPlaceHolders.Underwrite(propertyName, _aCaster.CastToSqlLiteralType(propertyValue, aViewColumnInfo));
        }
      }

      return aPlaceHolders;
    }

    public PlaceHolders CreatePlaceHolders(string aTableAliasName
                                         , TRecord aRecord) {
      return this.UnderwritePlaceHolders(new PlaceHolders(), aTableAliasName, aRecord);
    }

    public PlaceHolders UnderwritePlaceHolders(PlaceHolders aPlaceHolders
                                             , TRecord aRecord) {
      return this.UnderwritePlaceHolders(aPlaceHolders, null, aRecord);
    }

    public PlaceHolders CreatePlaceHolders(TRecord aRecord) {
      return this.CreatePlaceHolders(null, aRecord);
    }

    /// <summary>
    ///引数で指定したレコードを、各テーブルの主キーに対応するプロパティと、
    ///キー以外に対応するプロパティに分類してそれぞれを別レコードとして返す。
    /// </summary>
    /// <param name="aRecord">対象レコード</param>
    /// <param name="targetTable">UPDATE文の更新対象テーブル</param>
    /// <returns></returns>
    /// <remarks>返される配列の要素数は常に2要素である</remarks>
    public TRecord[] SplitForKey2(TRecord aRecord, SqlTable targetTable) {
      SqlBuilder selectSql = _sqlPod.GetSelectSql(new Query<TRecord>(), _recordViewTableMap);
      PropertyInfo[] properties = this.GetRecordInfo().Properties;
      TRecord keyRecord = new TRecord();
      TRecord nonKeyRecord = new TRecord();

      //消込用の更新対象テーブルの主キーリストを作成する
      HashSet<ColumnInfo> noValuePrimaryKeys = new HashSet<ColumnInfo>(this.GetTableInfo(targetTable.Name).GetPrimaryKeys());

      //
      //相関テーブルの主キーリストを作成する
      //
      List<Tuple<string, bool>> tableColumns = null;
      Dictionary<string, IEnumerable<Tuple<string, bool>>> allTables = new Dictionary<string, IEnumerable<Tuple<string, bool>>>();
      //SELECT文で用いられているすべてのテーブル名を取得する
      foreach(string srcTableName in selectSql.GetSrcTableNames()) {
        tableColumns = new List<Tuple<string, bool>>();
        allTables.Add(srcTableName, tableColumns);
        //テーブルの全ての列情報をtableColumnsに格納する
        foreach(ColumnInfo columnInfo in _aTableInfoSet[srcTableName].GetAllColumns()) {
          bool isPrimaryKey = columnInfo.PrimaryKey.HasValue && columnInfo.PrimaryKey.Value;
          tableColumns.Add(Tuple.Create(columnInfo.ColumnName, isPrimaryKey));
        }
      }

      //SELECT句の列情報リストを取得する
      ResultInfoList resultInfoList = selectSql.GetSelectItemInfo(allTables);

      foreach(PropertyInfo aProperty in properties) {
        //プロパティに対応するSELECT句の列情報を取得する
        foreach(IResultInfo resultInfo in resultInfoList) {
          if(resultInfo.ColumnAliasName != aProperty.Name) {
            continue;
          }

          if(!aProperty.CanRead || !aProperty.CanWrite) {
            throw new BadFormatRecordException("テーブルカラムに対応するプロパティは読み書き可能にする必要があります");
          }

          //レコードのプロパティ値を取得する
          object columnValue = aProperty.GetValue(aRecord, null);

          //SELECT句の列がキー列か否かでプロパティ値を振り分ける
          if(resultInfo.SourceTable != null &&
             resultInfo.SourceTable.AliasName == targetTable.AliasName &&
             resultInfo.KeyType == KeyType.Table) {
            //更新対象テーブル別名でキー列
            aProperty.SetValue(keyRecord, columnValue, null);
            // 値が設定されていれば更新対象テーブルの主キーを消し込む
            this.SplitForKey2Sub(noValuePrimaryKeys, targetTable, aProperty, columnValue);

          } else if(resultInfo.KeyType == KeyType.Group) {
            //Viewのキー列
            aProperty.SetValue(keyRecord, columnValue, null);
            // 値が設定されていれば更新対象テーブルの主キーを消し込む
            this.SplitForKey2Sub(noValuePrimaryKeys, targetTable, aProperty, columnValue);

          } else {
            aProperty.SetValue(nonKeyRecord, columnValue, null);

          }
        }
      }

      // keyRecordに更新対象テーブルの主キーが全て含まれていない場合
      // keyRecordには相関するテーブルの主キーを設定する
      if(noValuePrimaryKeys.Count > 0) {
        keyRecord = new TRecord();
        nonKeyRecord = new TRecord();

        foreach(PropertyInfo aProperty in properties) {
          //プロパティに対応するSELECT句の列情報を取得する
          foreach(IResultInfo resultInfo in resultInfoList) {
            if(resultInfo.ColumnAliasName != aProperty.Name) {
              continue;
            }

            if(!aProperty.CanRead || !aProperty.CanWrite) {
              throw new BadFormatRecordException(
                "テーブルカラムに対応するプロパティは読み書き可能にする必要があります");
            }

            //レコードのプロパティ値を取得する
            object columnValue = aProperty.GetValue(aRecord, null);

            //SELECT句の列がキー列か否かでプロパティ値を振り分ける
            // 自己結合(テーブル名は同じだがテーブル別名は異なる)の場合と
            // 他者結合(テーブル名もテーブル別名も異なる)場合には
            // キー列か否かのみで判定する

            if(resultInfo.KeyType == KeyType.Table) {
              //更新対象テーブル別名でキー列
              aProperty.SetValue(keyRecord, columnValue, null);
              // 値が設定されていれば更新対象テーブルの主キーを消し込む
              this.SplitForKey2Sub(noValuePrimaryKeys, targetTable, aProperty, columnValue);

            } else if(resultInfo.KeyType == KeyType.Group) {
              //Viewのキー列
              aProperty.SetValue(keyRecord, columnValue, null);
              // 値が設定されていれば更新対象テーブルの主キーを消し込む
              this.SplitForKey2Sub(noValuePrimaryKeys, targetTable, aProperty, columnValue);

            } else {
              aProperty.SetValue(nonKeyRecord, columnValue, null);

            }
          }
        }
      }

      return new TRecord[] {keyRecord, nonKeyRecord};
    }

    private void SplitForKey2Sub(HashSet<ColumnInfo> noValuePrimaryKeys
                                , SqlTable targetTable
                                , PropertyInfo aProperty
                                , object columnValue) {
      // 値が設定されていれば更新対象テーブルの主キーを消し込む
      if(!_aCaster.IsNullPropertyValue(columnValue)) {
        ColumnInfo columnInfo = this.GetTableColumnInfoByProp(targetTable.AliasName, aProperty.Name);
        noValuePrimaryKeys.Remove(columnInfo);
      }
    }

    /// <summary>
    /// レコードからテーブル別名ごとのCNFを取得する
    /// </summary>
    /// <param name="record"></param>
    /// <returns></returns>
    /// <remarks>(TableAndAliasName、CNF)</remarks>
    public Dictionary<SqlTable, Dictionary<string, SqlExpr>> GetCNFExpression(TRecord record
                                                                            , IEnumerable<SqlTable> usedTableNames) {
      Dictionary<SqlTable, Dictionary<string, SqlExpr>> ret = new Dictionary<SqlTable, Dictionary<string, SqlExpr>>();

      RecordInfo<TRecord> recordInfo = this.GetRecordInfo();
      PropertyInfo[] propertiesInfo = recordInfo.Properties;

      foreach(SqlTable usedTableName in usedTableNames) {
        Dictionary<string, SqlExpr> aCNF = new Dictionary<string, SqlExpr>();

        foreach(PropertyInfo aProperty in propertiesInfo) {
          string propertyname = aProperty.Name;

          //Viewカラムに対応しないプロパティはスキップする
          ViewColumnInfo aViewColumnInfo = this.GetViewColumnInfo(propertyname);
          if(aViewColumnInfo == null) {
            continue;
          }

          object propertyValue = aProperty.GetValue(record, null);

          //プロパティに一対一対応するテーブルカラムが存在するか確認する
          Map aMap = _mapSet.Find1to1Column(usedTableName.AliasName, propertyname);
          if(aMap != null) {
            //1つのテーブルカラムと1つのSQLリテラルの等式のみをCNFの素論理式とする
            SqlExpr exp = aMap.Expression.Clone();
            ColumnInfo aColumnInfo = this.GetTableColumnInfo(aMap.Table.Name, aMap.ColumnName);
            string castedValue = _aCaster.CastToSqlLiteralType(propertyValue, aViewColumnInfo, aColumnInfo);
            exp.Place(propertyname, castedValue);
            aCNF.Add(aMap.ColumnName, exp);
          }
        }
        ret.Add(usedTableName, aCNF);
      }

      return ret;
    }

    //aCNFに素論理式を追加する
    private SqlPredicate Add(SqlPredicate aCNF, SqlPredicate exp) {
      if(aCNF == null) {
        return exp;
      } else {
        return aCNF.And(exp);
      }
    }

    public Dictionary<string, IEnumerable<Tuple<string, bool>>> GetTableColumns(SqlBuilder sql) {
      //主キーリストを作成する
      Dictionary<string, IEnumerable<Tuple<string, bool>>> tableColumns = new Dictionary<string, IEnumerable<Tuple<string, bool>>>();
      foreach(string tableName in sql.GetSrcTableNames()) {
        if(tableColumns.ContainsKey(tableName)) {
          continue;
        }

        List<Tuple<string, bool>> tableColumnList = new List<Tuple<string, bool>>();
        foreach(ColumnInfo columnInfo in this.GetTableInfo(tableName).GetAllColumns()) {
          bool isPrimaryKey = columnInfo.PrimaryKey.HasValue && columnInfo.PrimaryKey.Value;
          tableColumnList.Add(Tuple.Create(columnInfo.ColumnName, isPrimaryKey));
        }
        tableColumns.Add(tableName, tableColumnList);
      }
      return tableColumns;
    }

    /// <summary>
    /// Queryをテーブル毎に分割し、分割した結果をSQL文として返す
    /// </summary>
    /// <param name="aQuery"></param>
    /// <param name="nonPropertyPlaceHolders"></param>
    /// <param name="sqlId"></param>
    /// <param name="aDbConn"></param>
    /// <param name="lastAffectedRows"></param>
    /// <returns></returns>
    /// <remarks></remarks>
    public SqlPredicate GetPredicates(SqlBuilder mainSql
                                    , Query<TRecord> aQuery
                                    , PlaceHolders nonPropertyPlaceHolders
                                    , PlaceHolders placeHoldersForIf
                                    , SqlId sqlId
                                    , IDbConn aDbConn
                                    , int lastAffectedRows) {
      //プレースホルダのコピーを用いる
      PlaceHolders aNonPropertyPlaceHolders = (PlaceHolders)nonPropertyPlaceHolders.Clone();
      PlaceHolders aPlaceHoldersForIf = (PlaceHolders)placeHoldersForIf.Clone();

      //nonPropertyPlaceHoldersからプロパティに紐づくプレースホルダを除外する
      List<string> removePlaceHolders = new List<string>();
      foreach(KeyValuePair<string, string> ph in aNonPropertyPlaceHolders) {
        ViewColumnInfo aViewColumnInfo = this.GetViewColumnInfo(ph.Key);
        if(aViewColumnInfo != null) {
          removePlaceHolders.Add(ph.Key);
        }
      }
      aNonPropertyPlaceHolders.Remove(removePlaceHolders);

      SqlPredicate ret = new SqlPredicate();

      Maps allAdoptedMaps = new Maps();

      //QueryオブジェクトはCastToSqlLiteralType()によって変更されるのでコピーを用いる
      Query<TRecord> query = (Query<TRecord>)aQuery.Clone();
      //Queryオブジェクトが保持するリテラル値をDB列型にキャストする
      ((IQueryExtention<TRecord>)query).CastToSqlLiteralType(_aCaster, this);

      foreach(SqlTable correlatedTable in mainSql.GetSrcTables()) {
        //aQueryオブジェクトに格納されたプロパティとの一致条件を
        //テーブル列との一致条件に変換するためのMapを取得する
        Maps adoptedMaps = this.GetAdoptedMaps(query
                                              , aNonPropertyPlaceHolders
                                              , aPlaceHoldersForIf
                                              , sqlId
                                              , correlatedTable.AliasName
                                              , aDbConn
                                              , lastAffectedRows);

        //採用済み一致条件は、一致条件として付加する

        foreach(Map adoptedMap in adoptedMaps) {
          string columnName = adoptedMap.ColumnName;
          if(mainSql.GetStatementType() == SqlBuilder.StatementType.Update ||
             mainSql.GetStatementType() == SqlBuilder.StatementType.Delete) {
            if(mainSql.GetSrcTables().Count > 1) {
              //FROM句のあるUPDATE/DELETE文に付加する一致条件のカラム名にはテーブル別名を修飾する
              columnName = correlatedTable.ExplicitAliasName + "." + columnName;
            }

          } else if(mainSql.GetStatementType() == SqlBuilder.StatementType.Select) {
            //採用済み一致条件のColumnNameの値がメインSELECT句で取得可能であれば
            //それに対する一致条件を作成する

            //主キーリストを作成する
            Dictionary<string, IEnumerable<Tuple<string, bool>>> tableColumns = this.GetTableColumns(mainSql);

            //主キーをメインクエリまで引き上げる
            mainSql.RaisePrimaryKey(tableColumns);


            //columnNameに対応するプロパティ名を取得する
            //--> テーブル(correlatedTable)の列(columnName)がダイレクトに出力される列を
            //    メインクエリのSELECT句から探す、ない場合は列(ColumnName)をメインクエリまで引き上げる



            //テーブルとその列名がメインクエリで取得可能であればメインクエリでの列名を取得する
            AbstractQueryResultInfo queryResultInfo = mainSql.GetSelectItemInfo(tableColumns, correlatedTable, columnName);
            if(queryResultInfo == null ||
               queryResultInfo.Type == ResultInfoType.Compound ||
               queryResultInfo.Type == ResultInfoType.Count) {
              continue;
            }
            IResultInfo sourceInfo = mainSql.GetSourceInfo((AbstractSingleQueryResultInfo)queryResultInfo);

            columnName = sourceInfo.TableAliasName + "." + sourceInfo.ColumnAliasName;

          }

          //一致条件を付加する
          SqlPredicate p = SqlPredicate.CreateEqualExpr(columnName, adoptedMap.Expression);
          ret = ret.And(p);

        }

        //適用済みMapを記録しておく
        allAdoptedMaps.AddRange(adoptedMaps);
      }

      foreach(SqlTable correlatedTable in mainSql.GetSrcTables()) {
        //Exists句へはプロパティプレースホルダにも値を適用する
        SqlPredicate existsClause = this.MakeExistsExp(mainSql
                                                      , query
                                                      , allAdoptedMaps
                                                      , aPlaceHoldersForIf
                                                      , correlatedTable
                                                      , aDbConn);
        //Exists句を付加する
        ret = ret.And(existsClause);

        //UPDATE/DELETE文のFROM句が記述できるDBMSは限られているため、FROM句は
        //無いという前提で処理をする。従って付加するExists句は1つである
        break;
      }

      return ret;
    }

    /// <summary>
    /// Queryオブジェクトで指定された一致条件の変換に必要なMapをテーブルカラム表から取得する
    /// </summary>
    /// <param name="aQuery"></param>
    /// <param name="nonPropertyPlaceHolders">プロパティプレースホルダ以外のプレースホルダ</param>
    /// <param name="tableAliasName"></param>
    /// <param name="lastAffectedRows"></param>
    /// <returns></returns>
    /// <remarks></remarks>
    private Maps GetAdoptedMaps(Query<TRecord> aQuery
                              , PlaceHolders nonPropertyPlaceHolders
                              , PlaceHolders placeHoldersForIf
                              , SqlId sqlId
                              , string tableAliasName
                              , IDbConn aDbConn
                              , int lastAffectedRows) {
      //'QueryオブジェクトはCastToSqlLiteralType()によって変更されるのでコピーを用いる
      //Dim query As Query(Of TRecord) = DirectCast(aQuery.Clone(), Query(Of TRecord))
      //'Queryオブジェクトが保持するリテラル値をDB列型にキャストする
      //query.CastToSqlLiteralType(_aCaster, _aRecordViewMap)

      //プロパティプレースホルダ以外の全てのプレースホルダに、
      //値を適用しても未摘要なプレースホルダが残るMapを抽出する
      Maps candidateMaps = new Maps();
      foreach(Map aMap in _mapSet.Find(tableAliasName)) {
        Map placedMap = aMap.PlaceExpression(nonPropertyPlaceHolders, lastAffectedRows);
        if(placedMap.Expression.HasUnplacedHolders()) {
          //Mapping採用条件へはプロパティプレースホルダにも値を適用する
          candidateMaps.Add(placedMap.PlaceCondition(placeHoldersForIf, lastAffectedRows));
        }
      }

      Maps adoptedMaps = new Maps();
      foreach(Map candidateMap in candidateMaps) {
        //Queryオブジェクトから、プロパティとの一致条件を取得する
        foreach(Tuple<string, string> propValuePair in aQuery.GetMatchingConditions()) {
          string propertyName = propValuePair.Item1;
          string propertyValue = propValuePair.Item2;

          //プロパティとの一致条件値を適用する
          RecordViewTableMap<TRecord>.Map placedCandidateMap = candidateMap.PlaceExpression(propertyName, propertyValue);

          //全てのプレースホルダにプロパティとの一致条件を適用できたMapは
          //WHERE句の一致条件として採用する
          if(!placedCandidateMap.Expression.HasUnplacedHolders()) {
            //同じテーブルカラムに既に候補が存在する場合、優先する方のMapを採用する
            Map conflictMap = adoptedMaps.FindFirstByColumnName(placedCandidateMap.ColumnName);
            if(conflictMap != null) {
              adoptedMaps.Remove(conflictMap);
              placedCandidateMap = this.SelectPreferMap(placedCandidateMap, conflictMap, aDbConn, sqlId, tableAliasName);
            }
            //解決済みのMapを記録する
            adoptedMaps.Add(placedCandidateMap);
            break;
          }
        }
      }

      return adoptedMaps;
    }

    private SqlPredicate MakeExistsExp(SqlBuilder mainSql
                                      , Query<TRecord> subQuery
                                      , Maps adoptedMaps
                                      , PlaceHolders aPlaceHolders
                                      , SqlTable table
                                      , IDbConn aDbConn) {
      //適用されたプロパティをaQueryオブジェクトから削除する
      foreach(Tuple<string, string> propValuePair in adoptedMaps.GetPlacedPropPlaceHolders()) {
        subQuery.RemoveEqualExp(propValuePair);
      }

      //相関サブクエリに含める抽出条件がない場合は空文字を返す
      if(!subQuery.HasCriteria()) {
        return new SqlPredicate();
      }

      //適用されなかったプロパティだけから成るQueryオブジェクトの抽出条件は
      //EXISTS (SELECT...に書き出す
      SqlBuilder selectSql = _sqlPod.GetSelectSql(subQuery, this);

      //IF文の場合は分岐条件を評価し、その評価結果となったSQL文を返す
      selectSql = IfStatement.Evaluate(aDbConn, selectSql, aPlaceHolders, _ifStmtCacheStrategy).Sqls[0];

      //IF文の評価結果となったSQL文にPlaceHolderを適用する
      selectSql.SetPlaceHolder(aPlaceHolders.ToDictionary());

      //相関テーブルの主キーリストを作成する
      List<Tuple<string, bool>> tableColumns = new List<Tuple<string, bool>>();
      foreach(ColumnInfo columnInfo in _aTableInfoSet[table.Name].GetAllColumns()) {
        bool isPrimaryKey = columnInfo.PrimaryKey.HasValue && columnInfo.PrimaryKey.Value;
        tableColumns.Add(Tuple.Create(columnInfo.ColumnName, isPrimaryKey));
      }
      //相関サブクエリを作成する
      return selectSql.ConvertToExistsExpr2(mainSql, table, tableColumns);
    }

    //2つのMapのうち、指定したSqlIdと同じMapを返す
    //どちらも指定したSqlIdと異なる場合、指定したテーブル別名等から優先するMapを返す
    //2つのMapはプレースホルダの適用処理が終わっていること
    private Map SelectPreferMap(Map placedMap1
                              , Map placedMap2
                              , IDbConn aDbConn
                              , SqlId sqlId
                              , string tableAliasName) {
      //抽出条件を付加するSQL文と同じsqlIdをもつMapを採用する
      if(placedMap1.SrcSqlId == sqlId) {
        return placedMap1;
      } else if(placedMap2.SrcSqlId == sqlId) {
        return placedMap2;
      }

      //同じsqlIdのMapがなければ、同じ"テーブル別名"のMapを採用する
      if(placedMap1.Table.AliasName == tableAliasName &&
         placedMap2.Table.AliasName != tableAliasName) {
        return placedMap1;
      } else if(placedMap1.Table.AliasName != tableAliasName &&
                placedMap2.Table.AliasName == tableAliasName) {
        return placedMap2;
      } else if(placedMap1.Table.AliasName != tableAliasName &&
                placedMap2.Table.AliasName != tableAliasName) {
        //同じ"テーブル別名"のマッピング情報がなければ、UPDATE文から抽出したマッピング情報を採用する
        return this.GetPreferMapSub(placedMap1, placedMap2);
      }

      //2つのMapのうち、どちらも同じ"テーブル別名"の場合、Mapping採用条件がTrueのMapを採用する
      bool map1Condition = this.CheckCondition(placedMap1.Condition, aDbConn);
      bool map2Condition = this.CheckCondition(placedMap2.Condition, aDbConn);

      if(map1Condition && !map2Condition) {
        return placedMap1;
      } else if(!map1Condition && map2Condition) {
        return placedMap2;
      } else {
        //2つのMapのうち、どちらのMapping採用条件もTrueの場合、
        //または、どちらのMapping採用条件もFalseの場合、
        //Update文等から優先するMapを判定する
        return this.GetPreferMapSub(placedMap1, placedMap2);
      }
    }

    //2つのMapのうち、Update文から取得したMapを返す
    //どちらもUpdate文から取得したMapの場合、SqlIdが大きいMapを返す
    private Map GetPreferMapSub(Map map1, Map map2) {
      if(map1.StatementType == SqlBuilder.StatementType.Update &&
         map2.StatementType != SqlBuilder.StatementType.Update) {
        return map1;
      } else if(map1.StatementType != SqlBuilder.StatementType.Update &&
                map2.StatementType == SqlBuilder.StatementType.Update) {
        return map2;
      } else {
        //2つのMapのうち、どちらもUpdate文から取得したMapの場合
        //または、どちらもUpdate文以外から取得したMapの場合
        //SqlIdが大きいMapを返す
        if(map1.SrcSqlId > map2.SrcSqlId) {
          return map1;
        } else {
          return map2;
        }
      }
    }

    private bool CheckCondition(SqlPredicate condition, IDbConn aDbConn) {
      if(condition == null) {
        //Mapping採用条件がない場合はTrueを返す
        return true;
      } else if(condition.IsEmpty) {
        return true;
      } else if(condition.HasUnplacedHolders()) {
        //適用済みでないプレースホルダが存在する場合はFalseを返す
        return false;
      }
      return IfStatement.EvaluateCondition(aDbConn, condition.ToString(), _ifStmtCacheStrategy);
    }

    //(テーブル名、List(Of 主キー列名))
    private Dictionary<string, IEnumerable<string>> _allColumns;
    //AllColumnsオブジェクトを作成して返す
    internal Dictionary<string, IEnumerable<string>> GetAllColumns(SqlBuilder sql) {
      //一度PrimaryKeysオブジェクトを作成していれば、それを返す
      if(_allColumns != null) {
        return _allColumns;
      }
      _allColumns = new Dictionary<string, IEnumerable<string>>();

      foreach(string tableName in sql.GetSrcTableNames()) {
        if(_allColumns.ContainsKey(tableName)) {
          continue;
        }

        //_primaryKeys.Add(tableName, New List(Of String))
        List<string> primaryKeyList = new List<string>();
        foreach(ColumnInfo aColumnInfo in this.GetTableInfo(tableName).GetAllColumns()) {
          //_primaryKeys(tableName).Add(aColumnInfo.ColumnName)
          primaryKeyList.Add(aColumnInfo.ColumnName);
        }
        _allColumns.Add(tableName, primaryKeyList);
      }

      return _allColumns;
    }
  }
}
