using System;
using System.Collections.Generic;
using System.ComponentModel;
using MiniSqlParser;

namespace MiniSqlParser
{
  /// <summary>
  /// SQL文を表す
  /// </summary>
  public partial class SqlBuilder
  {
    /// <summary>
    /// DBMS種別
    /// </summary>
    public enum DbmsType
    {
      Unknown = 0,
      Oracle,
      Pervasive,
      Sqlite,
      MsSql
    }

    /// <summary>
    /// SQL文の種別を表す
    /// </summary>
    public enum StatementType
    {
      Unknown = 0,
      Null,
      Select,
      Update,
      InsertValue,
      InsertSelect,
      Delete,
      Merge,
      Call,
      Truncate,
      If,
      SqlitePragma
    }

    /// <summary>
    /// SQL文のインデント方式
    /// </summary>
    public enum IndentType
    {
      Compact,
      Beautiful
    }

    private readonly string _sqlStr;
    private readonly DBMSType _dbmsType;
    private readonly bool _forSqlAccessor;

    private Stmt _stmt;

    public SqlBuilder() : this("") { }

    public SqlBuilder(string sqlStr
                    , DbmsType dbmsType = DbmsType.Unknown
                    , bool forSqlAccessor = true) {
      _sqlStr = sqlStr;
      _dbmsType = SqlBuilder.ConvertDbmsType(dbmsType);
      _forSqlAccessor = forSqlAccessor;
    }

    internal SqlBuilder(Stmt stmt
                      , DbmsType dbmsType = DbmsType.Unknown
                      , bool forSqlAccessor = true) {
      _stmt = stmt;
      _dbmsType = SqlBuilder.ConvertDbmsType(dbmsType);
      this.Dbms = dbmsType;
      _forSqlAccessor = forSqlAccessor;
    }

    internal static DBMSType ConvertDbmsType(DbmsType dbmsType) {
      if(dbmsType == DbmsType.Oracle) {
        return DBMSType.Oracle;
      } else if(dbmsType == DbmsType.Pervasive) {
        return DBMSType.Pervasive;
      } else if(dbmsType == DbmsType.Sqlite) {
        return DBMSType.SQLite;
      } else if(dbmsType == DbmsType.MsSql) {
        return DBMSType.MsSql;
      } else if(dbmsType == DbmsType.Unknown) {
        return DBMSType.Unknown;
      } else {
        return DBMSType.Unknown;
      }
    }

    private static StatementType ConvertStmtType(StmtType stmtType) {
      if(stmtType == StmtType.Select) {
        return StatementType.Select;
      } else if(stmtType == StmtType.Update) {
        return StatementType.Update;
      } else if(stmtType == StmtType.InsertValue) {
        return StatementType.InsertValue;
      } else if(stmtType == StmtType.InsertSelect) {
        return StatementType.InsertSelect;
      } else if(stmtType == StmtType.Delete) {
        return StatementType.Delete;
      } else if(stmtType == StmtType.Merge) {
        return StatementType.Merge;
      } else if(stmtType == StmtType.Call) {
        return StatementType.Call;
      } else if(stmtType == StmtType.Truncate) {
        return StatementType.Truncate;
      } else if(stmtType == StmtType.If) {
        return StatementType.If;
      } else if(stmtType == StmtType.SqlitePragma) {
        return StatementType.SqlitePragma;
      } else if(stmtType == StmtType.Null) {
        return StatementType.Null;
      } else {
        return StatementType.Unknown;
      }
    }

    private Stmt GetStmt() {
      if(_stmt == null) {
        _stmt = MiniSqlParserAST.CreateStmt(_sqlStr, _dbmsType, _forSqlAccessor);
      }
      return _stmt;
    }

    private ResultInfoList GetResultInfoList(
        Dictionary<string, IEnumerable<Tuple<string, bool>>> tableColumnNamesList
      , ResultInfoAST.PrimaryKeyCompletion primaryKeyCompletion = ResultInfoAST.PrimaryKeyCompletion.None) {
      var stmt = this.GetStmt();

      // SELECT文以外は処理の対象外である
      if(stmt.Type != StmtType.Select) {
        return new ResultInfoList();
      }

      // ResultInfoASTに渡すためのテーブル列情報を作成する
      var tableColumns = new Dictionary<string, IEnumerable<TableResultInfo>>();
      foreach(var tableColumnNames in tableColumnNamesList){
        var tableName = tableColumnNames.Key;
        var tableResultInfoList = new List<TableResultInfo>();
        foreach(var tableColumnName in tableColumnNames.Value){
          var isPrimaryKey = tableColumnName.Item2;
          var tableResultInfo = new TableResultInfo(tableName, tableColumnName.Item1, !isPrimaryKey, isPrimaryKey);
          tableResultInfoList.Add(tableResultInfo);
        }
        tableColumns.Add(tableName, tableResultInfoList);
      }
      // SELECT句の解析を行う
      var resultInfoAST = new ResultInfoAST((SelectStmt)stmt, tableColumns, primaryKeyCompletion);
      return resultInfoAST.GetResultInfoList();
    }

    private ResultInfoList GetResultInfoList(
        string tableName
      , IEnumerable<Tuple<string, bool>> tableColumnNames
      , ResultInfoAST.PrimaryKeyCompletion primaryKeyCompletion = ResultInfoAST.PrimaryKeyCompletion.None) {
      var tableColumnNamesList = new Dictionary<string, IEnumerable<Tuple<string, bool>>>() {{tableName, tableColumnNames}};
      return this.GetResultInfoList(tableColumnNamesList, primaryKeyCompletion);
    }

    public DbmsType Dbms { get; private set; }

    public bool IsValidSyntax() {
      // SQL文の構文木が生成できれば文法エラーはないとする
      try {
        this.GetStmt();
        return true;
      } catch {
        return false;
      }
    }

    public StatementType GetStatementType() {
      return SqlBuilder.ConvertStmtType(this.GetStmt().Type);
    }

    public bool HasWildcard() {
      if(this.GetStmt().Type != StmtType.Select) {
        return false;
      }
      var query = ((SelectStmt)this.GetStmt()).Query;
      if(query.Type == QueryType.Single) {
        return ((SingleQuery)query).HasWildcard;
      } else if(query.Type == QueryType.Compound) {
        return this.HasWildcard(((CompoundQuery)query).Left);
      } else if(query.Type == QueryType.Bracketed) {
        return this.HasWildcard(((BracketedQuery)query).Operand);
      } else {
        throw new InvalidEnumArgumentException("Undefined QueryType is used"
                                              , (int)query.Type
                                              , typeof(QueryType));
      }
    }

    public bool HasTableWildcard() {
      if(this.GetStmt().Type != StmtType.Select) {
        return false;
      }
      var query = ((SelectStmt)this.GetStmt()).Query;
      if(query.Type == QueryType.Single) {
        return ((SingleQuery)query).Results.HasTableWildcard();
      } else if(query.Type == QueryType.Compound) {
        return this.HasTableWildcard(((CompoundQuery)query).Left);
      } else if(query.Type == QueryType.Bracketed) {
        return this.HasTableWildcard(((BracketedQuery)query).Operand);
      } else {
        throw new InvalidEnumArgumentException("Undefined QueryType is used"
                                              , (int)query.Type
                                              , typeof(QueryType));
      }
    }

    public Dictionary<string, string> GetDefaultValuePlaceHolders() {
      return this.GetStmt().PlaceHolderAssignComments;
    }

    public bool GetAutoWhere() {
      return this.GetStmt().AutoWhere;
    }

    public HashSet<string> GetAllTableNames() {
      var ret = new HashSet<string>();
      var visitor = new GetTablesVisitor();
      this.GetStmt().Accept(visitor);
      foreach(var table in visitor.Tables) {
        ret.Add(table.Name);
      }
      return ret;
    }

    public HashSet<SqlTable> GetSrcTables() {
      var ret = new HashSet<SqlTable>();
      var visitor = new GetSourceTablesVisitor();
      this.GetStmt().Accept(visitor);
      foreach(var table in visitor.Tables) {
        ret.Add(new SqlTable(table));
      }
      return ret;
    }

    public HashSet<string> GetSrcTableNames() {
      var ret = new HashSet<string>();
      var visitor = new GetSourceTablesVisitor();
      this.GetStmt().Accept(visitor);
      foreach(var table in visitor.Tables) {
        ret.Add(table.Name);
      }
      return ret;
    }

    public HashSet<string> GetSrcTableAliasNames() {
      var ret = new HashSet<string>();
      var visitor = new GetSourceTablesVisitor();
      this.GetStmt().Accept(visitor);
      foreach(var table in visitor.Tables) {
        ret.Add(table.GetAliasOrTableName2());
      }
      return ret;
    }

    public SqlTable GetTargetTable() {
      var visitor = new GetSourceTablesVisitor();
      this.GetStmt().Accept(visitor);
      if(visitor.TargetTable == null) {
        return null;
      } else {
        return new SqlTable(visitor.TargetTable);
      }
    }

    //public string GetTargetTableName() {
    //  var visitor = new GetSourceTablesVisitor();
    //  this.GetStmt().Accept(visitor);
    //  if(visitor.TargetTable == null) {
    //    return "";
    //  } else {
    //    return visitor.TargetTable.Name;
    //  }
    //}

    //public string GetTargetTableAliasName() {
    //  var visitor = new GetSourceTablesVisitor();
    //  this.GetStmt().Accept(visitor);
    //  if(visitor.TargetTable == null) {
    //    return "";
    //  } else {
    //    return visitor.TargetTable.GetAliasOrTableName2();
    //  }
    //}

    public SqlPredicate GetWhere() {
      var stmt = this.GetStmt();
      var visitor = new GetWherePredicateVisitor();
      stmt.Accept(visitor);
      return new SqlPredicate(visitor.GetWhere);
    }

    public List<string> GetSelectItems() {
      var stmt = this.GetStmt();
      if(stmt.Type == StmtType.Select) {
        return this.GetResultColumns(((SelectStmt)stmt).Query);
      } else if(stmt.Type == StmtType.SqlitePragma) {
        // SQLiteのPRAGMA TABLE_INFO()は列名の取得元となるSELECT句がないので、
        // この文が返す列名を決め打ちでここに記述する
        var sqlitePragmaColumnNames = new string[]{ "cid" 
                                                  , "name" 
                                                  , "type" 
                                                  , "notnull" 
                                                  , "dflt_value" 
                                                  , "pk"};
        return new List<string>(sqlitePragmaColumnNames);
      } else {
        return new List<string>();
      }
    }

    /// <summary>
    /// SELECT句の列名を取得する
    /// SELECT句に*が指定されている場合でも取得できるよう、抽出元テーブルの列情報を引数に渡す
    /// </summary>
    /// <returns></returns>
    public List<string> GetSelectItems(Dictionary<string, IEnumerable<Tuple<string, bool>>> tableColumns) {
      var ret = new List<string>();
      foreach(var resultInfo in this.GetSelectItemInfo(tableColumns)) {
        if(resultInfo.ExplicitDecl) {
          ret.Add(resultInfo.ColumnAliasName);
        }
      }
      return ret;
    }

    public ResultInfoList GetSelectItemInfo(Dictionary<string, IEnumerable<Tuple<string, bool>>> tableColumns) {
      return this.GetResultInfoList(tableColumns);
    }

    /// <summary>
    /// 指定したテーブル列を抽出元とするメインクエリのSELECT句情報を取得する
    /// </summary>
    /// <param name="tableColumns"></param>
    /// <param name="sourceTable">抽出元テーブル</param>
    /// <param name="sourceColumnName">抽出元テーブル列名</param>
    /// <returns></returns>
    public AbstractQueryResultInfo GetSelectItemInfo(Dictionary<string, IEnumerable<Tuple<string, bool>>> tableColumns
                                                   , SqlTable sourceTable
                                                   , string sourceColumnName) {
      foreach(var resultInfo in this.GetSelectItemInfo(tableColumns)){
        var queryResultInfo = (AbstractQueryResultInfo)resultInfo;
        if(queryResultInfo.SourceTable.AliasName == sourceTable.AliasName &&
           queryResultInfo.SourceTable.Name == sourceTable.Name &&
           queryResultInfo.SourceColumnName == sourceColumnName) {
          return queryResultInfo;
        }
      }
      return null;
    }

    /// <summary>
    /// 指定したSELECT句の抽出元の情報(SourceInfoプロパティ)を取得する
    /// </summary>
    /// <param name="resultInfo"></param>
    /// <returns></returns>
    public IResultInfo GetSourceInfo(AbstractSingleQueryResultInfo resultInfo) {
      return resultInfo.SourceInfo;
    }

    public List<Tuple<SqlPredicate, SqlBuilders>> GetIfBranches() {
      var ret = new List<Tuple<SqlPredicate, SqlBuilders>>();
      var stmt = this.GetStmt();
      var visitor = new GetIfConditionsVisitor();
      stmt.Accept(visitor);
      for(int i=0; i<visitor.Count; ++i){
        var sqlPredicate = new SqlPredicate(visitor.Conditions[i]);
        var sqlBuilders = new SqlBuilders(visitor.StmtsList[i]);
        ret.Add(Tuple.Create(sqlPredicate, sqlBuilders));
      }
      return ret;
    }

    public int CountIfBranches() {
      var ret = 0;
      var stmt = this.GetStmt();
      if(stmt.Type == StmtType.If) {
        var ifStmt = (IfStmt)stmt;
        ret = ifStmt.StatementsList.Count;
        if(ifStmt.HasElseStatements) {
          ++ret;
        }
      }
      return ret;
    }

    /// <summary>
    /// (テーブル別名, 列名, リテラル値)
    /// </summary>
    /// <returns></returns>
    public Dictionary<SqlTable, Dictionary<string, string>> 
    GetCNF(Dictionary<string, IEnumerable<string>> tableColumns){
      var ret = new Dictionary<SqlTable, Dictionary<string, string>>();

      var stmt = this.GetStmt();
      var bestcaseTableColumns = new BestCaseDictionary<IEnumerable<string>>(tableColumns);
      var visitor = new GetCNFVisitor(bestcaseTableColumns);
      stmt.Accept(visitor);

      foreach(var equalities in visitor.CNF) {
        var table = new SqlTable(equalities.Key);
        ret.Add(table, new Dictionary<string, string>());
        foreach(var equalitie in equalities.Value) {
          var columnName = equalitie.Key;
          var literalValue = equalitie.Value.Value;
          ret[table].Add(columnName, literalValue);
        }
      }

      return ret;
    }

    public Dictionary<string, IEnumerable<SqlExpr>> GetAssignments() {
      var stmtType = this.GetStatementType();

      if(stmtType == StatementType.Update) {
        var stmt = (UpdateStmt)this.GetStmt();
        return this.GetAssignments(stmt.Assignments);

      } else if(stmtType == StatementType.InsertValue) {
        var stmt = (InsertValuesStmt)this.GetStmt();
        var ret = new Dictionary<string, IEnumerable<SqlExpr>>();
        foreach(var column in stmt.Columns) {
          var sqlExprs = new List<SqlExpr>();
          foreach(var assignment in stmt.GetAssignments(column)) {
            sqlExprs.Add(new SqlExpr(assignment.Value));
          }
          ret[column.Name] = sqlExprs;
        }
        return ret;

      } else if(stmtType == StatementType.InsertSelect) {
        var stmt = (InsertSelectStmt)this.GetStmt();
        return this.GetAssignments(stmt.GetAssignments(0));

      } else if(stmtType == StatementType.Merge) {
        var stmt = (MergeStmt)this.GetStmt();

        //
        // Merge文の対応ができていない
        //
        throw new NotImplementedException();

      } else {
        return new Dictionary<string, IEnumerable<SqlExpr>>();
      }
    }

    private Dictionary<string, IEnumerable<SqlExpr>> GetAssignments(Assignments assignments) {
      var ret = new Dictionary<string, IEnumerable<SqlExpr>>();
      foreach(var assignment in assignments) {
        var values = new List<SqlExpr>();
        values.Add(new SqlExpr(assignment.Value));
        ret.Add(assignment.Column.Name, values);
      }
      return ret;
    }

    public override string ToString() {
      return this.ToString(IndentType.Compact);
    }

    public string ToString(IndentType indentType){
      var stmt = this.GetStmt();
      IVisitor stringifier;
      if(indentType == IndentType.Compact){
        stringifier = new CompactStringifier(144);
      } else if(indentType == IndentType.Beautiful){
        stringifier = new BeautifulStringifier(
            144
          , 4
          , BeautifulStringifier.KeywordCase.Upper
          , BeautifulStringifier.JoinIndentType.A, true);
      } else {
        throw new InvalidEnumArgumentException("Undefined IndentType is used"
                                              , (int)indentType
                                              , typeof(IndentType));
      }

      stmt.Accept(stringifier);
      return stringifier.ToString();
    }

    public void SetMaxRows(int maxRows){
      var stmt = this.GetStmt();
      if(stmt.Type != StmtType.Select) {
        return;
      }
      var stmtQuery = ((SelectStmt)stmt).Query;

      //if((_dbmsType == DBMSType.MsSql || _dbmsType == DBMSType.Pervasive) &&
      //    stmtQuery.Type == QueryType.Single) {
      //  var singleQuery = (SingleQuery)stmtQuery;
      //  singleQuery.HasTop = true;
      //  singleQuery.Top = maxRows;

      if(_dbmsType == DBMSType.MsSql || _dbmsType == DBMSType.Pervasive){
        stmtQuery.AcceptOnMainQuery(new SetMaxRowsVisitor(maxRows));
      } else {
        this.SetRowLimit(0, maxRows);
      }
    }

    public void SetRowLimit(int offset, int limit){
      var stmt = this.GetStmt();
      if(stmt.Type != StmtType.Select) {
        return;
      }

      var stmtQuery = ((SelectStmt)stmt).Query;
      var limitClause = stmtQuery.Limit;
      if(limitClause == null) {
        if(_dbmsType == DBMSType.Oracle || _dbmsType == DBMSType.MsSql) {
          stmtQuery.Limit = new OffsetFetchClause(offset, limit, false);
        } else {
          stmtQuery.Limit = new LimitClause(new UNumericLiteral(offset.ToString())
                                          , new UNumericLiteral(limit.ToString()), true);
        }
      } else {
        if(limitClause.Type == LimitClauseType.Limit) {
          ((LimitClause)limitClause).Offset = new UNumericLiteral(offset.ToString());
          ((LimitClause)limitClause).Limit = new UNumericLiteral(limit.ToString());
        } else if(limitClause.Type == LimitClauseType.OffsetFetch) {
          ((OffsetFetchClause)limitClause).Offset = offset;
          ((OffsetFetchClause)limitClause).Fetch = limit;
          ((OffsetFetchClause)limitClause).HasFetch = true;
          ((OffsetFetchClause)limitClause).FetchFromFirst = false;
          ((OffsetFetchClause)limitClause).FetchWithTies = false;
        } else {
          throw new InvalidEnumArgumentException("Undefined LimitType is used"
                                              , (int)limitClause.Type
                                              , typeof(LimitClauseType));
        }
      }
    }

    public void SetCount(){
      var stmt = this.GetStmt();
      if(stmt.Type == StmtType.Select) {
        this.ReplaceWithCountFunc((SelectStmt)stmt);
      }
    }

    public void SetConstant() {
      var stmt = this.GetStmt();
      if(stmt.Type == StmtType.Select) {
        this.ReplaceWithConstant(((SelectStmt)stmt).Query);
      }
    }

    public void WrapInSelectStar(Dictionary<string, IEnumerable<string>> tableColumns, string AliasName = "V0_") {
      var stmt = this.GetStmt();
      if(stmt.Type == StmtType.Select) {
        var selectStmt = (SelectStmt)stmt;

        var bestcaseTableColumns = new BestCaseDictionary<IEnumerable<string>>(tableColumns);
        var visitor = new NormalizeOrderByVisitor(bestcaseTableColumns);
        selectStmt.Accept(visitor);

        var singleQueryClause =
          SingleQuery.WrapInSelectStar(new AliasedQuery((IQueryClause)selectStmt.Query, false, AliasName));
        // Limit句にはColumnを指定できないので変換処理は必要ない
        selectStmt.Query =
          new SingleQuery(singleQueryClause, selectStmt.Query.OrderBy, selectStmt.Query.Limit);
      }
    }

    public void RenameTableAliasName(string oldName, string newName){
      var visitor = new RenameTableAliasVisitor(oldName, newName);
      this.GetStmt().Accept(visitor);
    }

    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="subQuery">相関サブクエリ文</param>
    ///// <param name="correlatedTableAliasName">相関対象テーブル別名</param>
    ///// <param name="primaryKeys">相関対象テーブルの主キー</param>
    //public void AddExistsExpr(SqlBuilder subQuery
    //                        , string correlatedTableAliasName
    //                        , IEnumerable<string> primaryKeys) {

    //  // subQueryがSELECT文であること
    //  if(subQuery.GetStatementType() != StatementType.Select) {
    //    throw new ArgumentException("相関サブクエリにSELECT文以外の文が指定されました", "subQuery");
    //  }

    //  var mainTableNames = this.GetSrcTableNames();
    //  var mainTableAliasNames = this.GetSrcTableAliasNames();
    //  var mainTables = this.GetSrcTables();

    //  // 引数で指定されたテーブル名が被相関クエリのテーブル別名に
    //  // 存在しない場合、処理を中断する
    //  var correlatedTableAliasNameFound = false;
    //  foreach(var mainTable in mainTables){
    //    if(mainTable.AliasName == correlatedTableAliasName) {
    //      correlatedTableAliasNameFound = true;
    //      break;
    //    }
    //  }
    //  if(!correlatedTableAliasNameFound){
    //    throw new ArgumentException("相関対象テーブル別名が被相関クエリに存在しません"
    //                              , "correlatedTableAliasName");
    //  }

    //  // 相関サブクエリにそのテーブル別名が存在しない場合、
    //  // また、存在した場合でもそのテーブル別名の実テーブル名が、
    //  // 被相関クエリの実テーブル名に一致しない場合、処理を中断する
    //  var correlatedTableFound = false;
    //  foreach(var subTable in subQuery.GetSrcTables()) {
    //    foreach(var mainTable in mainTables) {
    //      if(mainTable.Name == subTable.Name &&
    //         mainTable.AliasName == subTable.AliasName) {
    //        correlatedTableFound = true;
    //        break;
    //      }
    //    }
    //    if(correlatedTableFound) {
    //      break;
    //    }
    //  }
    //  if(!correlatedTableFound){
    //    throw new ArgumentException("相関対象テーブル別名が相関クエリに存在しない、" + 
    //                                "または相関対象テーブル別名が同じでも被相関クエリと相関クエリとで" + 
    //                                "実テーブルが異なっています", "correlatedTableAliasName");
    //  }

    //  // メインSQLがSELECT文の場合は相関対象テーブルの主キーが
    //  // 相関サブクエリから参照できるようにする
    //  if(this.GetStatementType() == StatementType.Select) {
    //    this.RaisePrimaryKey(new Dictionary<string, IEnumerable<string>> { { correlatedTableName, primaryKeys } });
    //  }

    //}

    /// <summary>
    /// SELECT文をEXISTS句に変換する
    /// </summary>
    /// <param name="subQuery">相関サブクエリ文</param>
    /// <param name="correlatedTableAliasName">相関対象テーブル別名</param>
    /// <param name="primaryKeys">相関対象テーブルの主キー</param>
    public SqlPredicate ConvertToExistsExpr2(SqlBuilder mainSql
                                            , SqlTable correlatedTable
                                            , IEnumerable<Tuple<string, bool>> tableColumnNames) {
      if(this.GetStatementType() != StatementType.Select) {
        throw new NotSupportedException("相関サブクエリに変形できるのはSELECT文のみです");
      }

      // 相関サブクエリにそのテーブル別名が存在しない場合、
      // また、存在した場合でもそのテーブル別名の実テーブル名が、
      // 被相関クエリの実テーブル名に一致しない場合、処理を中断する
      string correlatedTableName = null;
      foreach(var table in this.GetSrcTables()) {
        if(table.AliasName == correlatedTable.AliasName) {
          correlatedTableName = table.Name;
          break;
        }
      }
      if(string.IsNullOrEmpty(correlatedTableName)) {
        throw new ArgumentException(
          "相関対象テーブル別名が相関クエリに存在しません", "correlatedTableAliasName");
      }

      // (0) 相関クエリのSELECT句を定数値に変更する
      if((((SelectStmt)this.GetStmt()).Query).Type != QueryType.Compound) {
        this.SetConstant();
      }

      // (1) 相関クエリのOrderBy句を削除する
      this.ClearOrderBy();

      // (2) 相関クエリのメインクエリスコープに、相関対象テーブル名(又はテーブル別名)と
      //     同じテーブル名(又はテーブル別名)が存在する場合は、相関クエリのテーブル別名を変更する
      var tableAliasNameOfSub = correlatedTable.AliasName + "_";
      this.RenameTableAliasName(correlatedTable.AliasName, tableAliasNameOfSub);

      // 相関クエリが集合演算クエリの場合はWHERE句が付加できるようそのクエリをSELECT *で囲む
      if((((SelectStmt)this.GetStmt()).Query).Type == QueryType.Compound) {
        // ORDER BY句は削除済みなのでtableColumns引数は指定しなくても動作する
        this.WrapInSelectStar(new Dictionary<string, IEnumerable<string>>());
      }

      // (3) 相関クエリのSELECT句リストを取得する
      //     それと同時に相関対象テーブルの主キーを相関クエリのメインスコープに引き上げる
      var subResultInfoList = this.GetResultInfoList(correlatedTable.Name
                                                   , tableColumnNames
                                                   , ResultInfoAST.PrimaryKeyCompletion.SubQueryOnly);

      string tableAliasNameOfMain = "";
      IQueryClause subQueryClause = null;
      if(mainSql.GetStatementType() == StatementType.Select) {
        // 被相関クエリが集合演算クエリの場合はWHERE句が付加できるようそのクエリをSELECT *で囲む
        if((((SelectStmt)mainSql.GetStmt()).Query).Type == QueryType.Compound) {
          // ORDER BY句は削除済みなのでtableColumns引数は指定しなくても動作する
          mainSql.WrapInSelectStar(new Dictionary<string, IEnumerable<string>>());
        }

        // (3a) 被相関クエリのSELECT句リストを取得する
        //      それと同時に相関対象テーブルの主キーを相関クエリのメインスコープに引き上げる
        var mainResultInfoList = mainSql.GetResultInfoList(correlatedTable.Name
                                                         , tableColumnNames
                                                         , ResultInfoAST.PrimaryKeyCompletion.SubQueryOnly);

        // 被相関クエリのメインクエリスコープに、相関対象テーブル名(又はテーブル別名)を
        // 抽出元にもつSELECT句があればそのSELECT句のテーブル別名を取得する
        foreach(var resultInfo in mainResultInfoList) {
          if(resultInfo.SourceTable != null &&
             resultInfo.SourceTable.AliasName == correlatedTable.AliasName) {
            if(resultInfo.Type == ResultInfoType.Query) {
              tableAliasNameOfMain = ((AbstractSingleQueryResultInfo)resultInfo).SourceInfo.TableAliasName;
              break;
            } else if(resultInfo.Type == ResultInfoType.Compound) {
              tableAliasNameOfMain = ((CompoundQueryResultInfo)resultInfo).TableAliasName;
              break;
            }
          }
        } // foreach

        // (4) 相関クエリのSELECT句リストから、結合条件を作成し付加する
        subQueryClause = (IQueryClause)((SelectStmt)this.GetStmt()).Query;
        this.AddCorrelatedConditions(subQueryClause
                                   , subResultInfoList
                                   , mainResultInfoList
                                   , tableAliasNameOfMain
                                   , correlatedTable.AliasName);
      } else {
        // (4) 相関クエリのSELECT句リストから、結合条件を作成し付加する
        subQueryClause = (IQueryClause)((SelectStmt)this.GetStmt()).Query;
        this.AddCorrelatedConditions(subQueryClause
                                   , subResultInfoList
                                   , tableAliasNameOfSub
                                   , correlatedTable.ExplicitAliasName);
      }// if

      // (5) Existsで囲んで返す
      var query = ((SelectStmt)this.GetStmt()).Query;
      return new SqlPredicate(new ExistsPredicate(query));
    }

    //public SqlPredicate ConvertToExistsExpr(SqlTable correlatedTable
    //                                      , IEnumerable<Tuple<string, bool>> tableColumnNames) {
    //  if(this.GetStatementType() != StatementType.Select) {
    //    throw new NotSupportedException("相関サブクエリに変形できるのはSELECT文のみです");
    //  }

    //  // 相関サブクエリにそのテーブル別名が存在しない場合、
    //  // また、存在した場合でもそのテーブル別名の実テーブル名が、
    //  // 被相関クエリの実テーブル名に一致しない場合、処理を中断する
    //  string correlatedTableName = null;
    //  foreach(var table in this.GetSrcTables()) {
    //    if(table.AliasName == correlatedTable.AliasName) {
    //      correlatedTableName = table.Name;
    //      break;
    //    }
    //  }
    //  if(string.IsNullOrEmpty(correlatedTableName)) {
    //    throw new ArgumentException(
    //      "相関対象テーブル別名が相関クエリに存在しません", "correlatedTableAliasName");
    //  }

    //  // (0) 相関クエリのSELECT句を定数値に変更する
    //  this.SetConstant();

    //  // (1) 相関クエリのOrderBy句を削除する
    //  this.ClearOrderBy();

    //  // (2) 相関クエリのメインクエリスコープに、相関対象テーブル名(又はテーブル別名)と
    //  //     同じテーブル名(又はテーブル別名)が存在する場合は、相関クエリのテーブル別名を変更する
    //  var tableAliasNameSub = correlatedTable.AliasName + "_";
    //  this.RenameTableAliasName(correlatedTable.AliasName, tableAliasNameSub);

    //  // (3) 相関クエリのSELECT句リストを取得する
    //  //     それと同時に相関対象テーブルの主キーを相関クエリのメインスコープに引き上げる
    //  var resultInfoList = this.GetResultInfoList(correlatedTableName
    //                                             , tableColumnNames
    //                                             , ResultInfoAST.PrimaryKeyCompletion.SubQueryOnly);

    //  // (4) 相関クエリのSELECT句リストから、結合条件を作成し付加する
    //  var queryClause = (IQueryClause)((SelectStmt)this.GetStmt()).Query;
    //  this.AddCorrelatedConditions(queryClause, resultInfoList, null, tableAliasNameSub, correlatedTable.ExplicitAliasName);
    
    //  // (5) Existsで囲んで返す
    //  var query = ((SelectStmt)this.GetStmt()).Query;
    //  return new SqlPredicate(new ExistsPredicate(query));
    //}

    private void AddCorrelatedConditions(IQueryClause subQueryClause
                                       , ResultInfoList subResultInfoList
                                       , string oldTableAliasNameOfSub
                                       , string correlatedTableAliasName) {
      foreach(var subResultInfo in subResultInfoList) {
        // キーのみを結合条件にする
        if(subResultInfo.KeyType == KeyType.None) {
          continue;
        }

        if(subResultInfo.Type == ResultInfoType.Query) {
          var subQueryInfo = (QueryResultInfo)subResultInfo;
          if(oldTableAliasNameOfSub == subQueryInfo.SourceTable.AliasName) {
            var tableAliasNameOfSub = this.GetSourceInfoOfSingleQueryResultInfo(subResultInfo).TableAliasName;
            this.AddCorrelatedCondition(subQueryClause
                                      , subResultInfo
                                      , tableAliasNameOfSub
                                      , null
                                      , correlatedTableAliasName);
          }
        } else if(subResultInfo.Type == ResultInfoType.Compound) {


        }
      }
    }

    // 結合条件を作成し付加する
    private void AddCorrelatedConditions(IQueryClause subQueryClause
                                       , ResultInfoList subResultInfoList
                                       , ResultInfoList mainResultInfoList
                                       , string tableAliasNameOfMain
                                       , string correlatedTableAliasName) {
      foreach(var subResultInfo in subResultInfoList) {
        // キーのみを結合条件にする
        if(subResultInfo.KeyType == KeyType.None) {
          continue;
        }

        foreach(var mainResultInfo in mainResultInfoList) {
          // キーのみを結合条件にする
          if(mainResultInfo.KeyType == KeyType.None) {
            continue;
          }

          if(correlatedTableAliasName == mainResultInfo.SourceTable.AliasName &&
             subResultInfo.SourceColumnName == mainResultInfo.SourceColumnName) {
            // 相関クエリのメインクエリスコープに、相関対象テーブル名(又はテーブル別名)を
            // 抽出元にもつSELECT句があればそのSELECT句のテーブル別名を取得する
            var tableAliasNameOfSub = this.GetSourceInfoOfSingleQueryResultInfo(subResultInfo).TableAliasName;
            // 結合条件を作成し付加する
            this.AddCorrelatedCondition(subQueryClause
                                      , subResultInfo
                                      , tableAliasNameOfSub
                                      , mainResultInfo
                                      , tableAliasNameOfMain);
          }
        } // foreach
      } // foreach
    }

    // 結合条件を作成し付加する
    // 付加できた場合はtrueを返す
    private bool AddCorrelatedCondition(IQueryClause subQueryClause
                                      , IResultInfo subResultInfo
                                      , string tableAliasNameOfSub
                                      , IResultInfo mainResultInfo
                                      , string tableAliasNameOfMain) {

      if(subResultInfo.Type == ResultInfoType.Query) {
        var subQueryInfo = (QueryResultInfo)subResultInfo;
        //// SELECT句の抽出元テーブル情報を取得する
        // 結合条件を作成する
        string condition;
        string subTableAliasName = subQueryInfo.SourceInfo.TableAliasName;
        string primaryKeyOfSubTable = subQueryInfo.SourceInfo.ColumnAliasName;
        if(mainResultInfo == null) {
          var primaryKey = this.GetSourceTableResultInfo(subQueryInfo).ColumnAliasName;
          condition = tableAliasNameOfSub + "." + primaryKeyOfSubTable
                      + " = " +
                      tableAliasNameOfMain + "." + primaryKey;
        } else {
          string primaryKeyOfMainTable = mainResultInfo.ColumnAliasName;
          condition = tableAliasNameOfSub + "." + primaryKeyOfSubTable
                      + " = " +
                      tableAliasNameOfMain + "." + primaryKeyOfMainTable;
        }

        // 結合条件を追加する
        var visitor = new AddWherePredicateVisitor(condition, _dbmsType, _forSqlAccessor);
        subQueryClause.Accept(visitor);
        return true;

      } else if(subResultInfo.Type == ResultInfoType.Compound) {
        var compoundQueryInfo = (CompoundQueryResultInfo)subResultInfo;

        if(subQueryClause.Type != QueryType.Compound) {
          return false;
        }

        // 集合演算での抽出元の走査は左演算子を優先する
        var ret = this.AddCorrelatedCondition(((CompoundQuery)subQueryClause).Left
                                            , compoundQueryInfo.LeftResultInfo
                                            , tableAliasNameOfSub
                                            , mainResultInfo
                                            , tableAliasNameOfMain);
        if(!ret) {
          ret = this.AddCorrelatedCondition(((CompoundQuery)subQueryClause).Right
                                          , compoundQueryInfo.RightResultInfo
                                          , tableAliasNameOfSub
                                          , mainResultInfo
                                          , tableAliasNameOfMain);
        }
        return ret;

      } else if(subResultInfo.Type == ResultInfoType.Count) {
        return false;
      } else if(subResultInfo.Type == ResultInfoType.Table) {
        // selectItemがTableResultInfoの場合はない
        throw new ArgumentException(
          "GetCorrelatedCondition()にTableResultInfoは指定できません", "selectItem");
      } else {
        throw new InvalidEnumArgumentException("Undefined ResultInfoType is used"
                                              , (int)subResultInfo.Type
                                              , typeof(ResultInfoType));
      }
    }

    public void AddAndPredicate(string predicate) {
      var visitor = new AddWherePredicateVisitor(predicate, _dbmsType, _forSqlAccessor);
      this.GetStmt().Accept(visitor);
    }

    public void AddAndPredicate(SqlPredicate predicate){
      var visitor = new AddWherePredicateVisitor(predicate.Predicate);
      this.GetStmt().Accept(visitor);
    }

    public void AddOrderBy(IEnumerable<Tuple<string, bool>> orderByExprs){
      var visitor = new AddOrderByVisitor(orderByExprs, _dbmsType, _forSqlAccessor);
      this.GetStmt().Accept(visitor);
    }

    public void ClearWhere() {
      this.GetStmt().Accept(new ClearWherePredicateVisitor());
    }

    public void ClearOrderBy(){
      this.GetStmt().Accept(new ClearOrderByVisitor());
    }

    /// <summary>
    /// (テーブル別名、(テーブル名、主キーリスト))
    /// </summary>
    /// <param name="tableColumns">(テーブル名、主キーリスト)</param>
    /// <returns></returns>
    public Dictionary<SqlTable, List<string>>
    RaisePrimaryKey(Dictionary<string, IEnumerable<Tuple<string,bool>>> tableColumns) {
      var ret = new Dictionary<SqlTable, List<string>>();

      var resultInfoList = this.GetResultInfoList(tableColumns, ResultInfoAST.PrimaryKeyCompletion.AllQuery);
      foreach(IResultInfoInternal resultInfo in resultInfoList) {
        if(resultInfo.KeyType == KeyType.Table) {
          string tableAliasName = resultInfo.SourceTable.ExplicitAliasName;
          string tableName = resultInfo.SourceTable.Name;
          var table = resultInfo.SourceTable;
          if(!ret.ContainsKey(table)) {
            ret.Add(table, new List<string>());
          }
          ret[table].Add(resultInfo.ColumnAliasName);
        }
      }

      // Dictionary<string, Tuple<string, List<string>>>から
      // Dictionary<string, Tuple<string, IEnumerable<string>>>へ型を変換する
      //var ret = new Dictionary<string, Tuple<string, List<string>>>();
      //foreach(var p in ret0) {
      //  ret.Add(p.Key, Tuple.Create(p.Value.Item1, p.Value.Item2));
      //}

      return ret;
    }

    /// <summary>
    /// プレースホルダに値を適用する
    /// </summary>
    /// <param name="placeHolderName">プレースホルダ名(@を含まない)</param>
    /// <param name="value">プレースホルダに適用する値</param>
    public void SetPlaceHolder(string placeHolderName, string value){
      var placeHolders = new Dictionary<string, string>();
      placeHolders.Add(placeHolderName, value);
      this.SetPlaceHolder(placeHolders);
    }

    public void SetPlaceHolder(Dictionary<string, string> placeHolders){
      var visitor = new ReplacePlaceHolders(placeHolders);
      this.GetStmt().Accept(visitor);
    }

    public void SetAllPlaceHolderToNull() {
      var visitor = new ReplaceAllPlaceholdersToNull();
      this.GetStmt().Accept(visitor);
    }

    public void TrimRightSeparators() {
      this.GetStmt().TrimRightSeparators();
    }
  }
}
