using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using MiniSqlParser;

namespace MiniSqlParser
{
  /// <summary>
  /// SELECT句の情報を取得する
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class GetResultInfoListVisitor: Visitor
  {
    protected enum SubQueryType
    {
      Exists,
      InResults,
      OrderBy,
      From   // 20180420 追加
    }

    [System.Diagnostics.DebuggerDisplay("{Table.Name}.{ColumnName}")]
    protected class TableAndColumn
    {
      public Table Table { get; private set; }
      public string ColumnName { get; private set; }
      public TableAndColumn(Table table, string columnName) {
        this.Table = table;
        this.ColumnName = columnName;
      }
      public override int GetHashCode() {
        // XOR
        return this.Table.GetHashCode() ^ this.ColumnName.GetHashCode();
      }
      public override bool Equals(object obj) {
        return obj.GetType() == typeof(TableAndColumn) && this == (TableAndColumn)obj;
      }
      public static bool operator ==(TableAndColumn tableColumn1, TableAndColumn tableColumn2) {
        return tableColumn1.Table == tableColumn2.Table && tableColumn1.ColumnName == tableColumn2.ColumnName;
      }
      public static bool operator !=(TableAndColumn tableColumn1, TableAndColumn tableColumn2) {
        return !(tableColumn1 == tableColumn2);
      }
    }

    [System.Diagnostics.DebuggerDisplay("{TableAliasName}.{ColumnAliasName}")]
    protected class ResultInfo
    {
      public Identifier TableAliasName { get; set; }
      public Identifier ColumnAliasName { get; private set; }
      /// <summary>
      /// 参照元のResultInfo(FindInGroupByItems()で使用する)
      /// </summary>
      public ResultInfo SourceInfo { get; set; }
      /// <summary>
      /// 抽出元テーブルとその列
      /// </summary>
      public List<TableAndColumn> Sources { get; private set; }
      public ResultColumn Node { get; set; }
      public ResultInfo(string tableAliasName
                      , string columnAliasName
                      , ResultColumn node
                      , ResultInfo sourceInfo = null) {
        this.TableAliasName = tableAliasName;
        this.ColumnAliasName = columnAliasName;
        this.Node = node;
        if(sourceInfo == null) {
          this.Sources = new List<TableAndColumn>();
        } else {
          this.SourceInfo = sourceInfo;
          this.Sources = sourceInfo.Sources;
        }
      }
      public ResultInfo(Identifier tableAliasName, Identifier columnAliasName, TableAndColumn source) {
        this.TableAliasName = tableAliasName;
        this.ColumnAliasName = columnAliasName;
        this.Sources = new List<TableAndColumn>() { source };
      }
      public bool IsDirectSource(Column destItem, bool ignoreCase = true) {
        if(!Identifier.Compare(destItem.Name, this.ColumnAliasName, ignoreCase)) {
          return false;
        } else if(Identifier.IsNullOrEmpty(destItem.TableAliasName)) {
          // SELECT句でテーブル名が省略されている場合、trueを返す
          return true;
        } else if(Identifier.IsNullOrEmpty(this.TableAliasName)) {
          // SELECT句でテーブル名が指定され、抽出元Queryの別名が省略されている場合、falseを返す
          return false;
        } else if(!Identifier.Compare(destItem.TableAliasName, this.TableAliasName, ignoreCase)) {
          // テーブル別名が指定されている場合はテーブル別名を参照する
          return false;
        } else {
          // SELECT句とQueryがテーブル別名を指定され、全て一致する場合、tureを返す
          return true;
        }
      }
    }

    protected class ResultInfoList: IEnumerable<ResultInfo>
    {
      /// <summary>
      /// (テーブル別名, その列情報)
      /// </summary>
      private Dictionary<Identifier, List<ResultInfo>> Items { get; set; }
      /// <summary>
      /// 列情報を定義順に並べたリスト
      /// </summary>
      private List<ResultInfo> _sequencialList;

      internal void Add(Identifier tableAliasName) {
        if(!this.Items.ContainsKey(tableAliasName)) {
          this.Items.Add(tableAliasName, new List<ResultInfo>());
        }
      }

      internal void Add(ResultInfo item) {
        if(!this.Items.ContainsKey(item.TableAliasName)) {
          this.Items.Add(item.TableAliasName, new List<ResultInfo>());
        }
        this.Items[item.TableAliasName].Add(item);
        _sequencialList.Add(item);
      }

      internal void AddRange(IEnumerable<ResultInfo> items) {
        foreach(var item in items) {
          this.Add(item);
        }
      }

      IEnumerator<ResultInfo> IEnumerable<ResultInfo>.GetEnumerator() {
        return _sequencialList.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator() {
        return _sequencialList.GetEnumerator();
      }

      public int Count {
        get {
          return _sequencialList.Count;
        }
      }

      public bool IsSubQueryInResults { get; set; }

      public bool IsLeftOperandOfJoinOrCompoundOp { get; set; }

      public bool HasTableAliasName(Identifier identifier) {
        return this.Items.ContainsKey(identifier);
      }

      public ResultInfo this[int i] {
        get {
          return _sequencialList[i];
        }
        internal set {
          _sequencialList[i] = value;
        }
      }

      public ResultInfoList() {
        this.Items = new Dictionary<Identifier, List<ResultInfo>>();
        _sequencialList = new List<ResultInfo>();
      }
    }

    /// <summary>
    /// AS別名と列名の大文字小文字を区別しない場合、True
    /// </summary>
    protected readonly bool _ignoreCase;

    /// <summary>
    /// (テーブル名, 列名リスト)
    /// </summary>
    protected readonly BestCaseDictionary<IEnumerable<string>> _tableColumns;

    /// <summary>
    /// SELECT句の列情報リスト
    /// </summary>
    protected readonly Stack<ResultInfoList> _stack;

    /// <summary>
    /// サブクエリの入子階層
    /// </summary>
    protected readonly Stack<SubQueryType> _subQueryStack;

    // Visitorの操作ノードでのIN、ANY、SOME、ALLのPredicateの入子階層
    protected int _inNestLevel;
    protected int _anySomeAllNestLevel;
    // Visitorの操作ノードでのCASE式の入子階層
    protected int _caseNestLevel;
    // Visitorの操作ノードでのSubQueryExprの入子階層
    protected int _queryExprNestLevel;

    public GetResultInfoListVisitor(BestCaseDictionary<IEnumerable<string>> tableColumns
                                  , bool ignoreCase = true) {
      _tableColumns = tableColumns;
      _stack = new Stack<ResultInfoList>();
      _subQueryStack = new Stack<SubQueryType>();
      _ignoreCase = ignoreCase;
    }

    public sealed override bool VisitOnFromFirstInQuery {
      get {
        // Queryを走査する時はFROM句を最初に走査し、その後にWHERE句、SELECT句の順に走査する
        return true;
      }
    }

    public override void Visit(Table table) {
      if(this.IsNotInMainResultsSource()) {
        return;
      }

      if(_tableColumns.ContainsKey(table.Name)) {
        var tableResultInfoList = new ResultInfoList();
        foreach(string tableColumnName in _tableColumns[table.Name]) {
          tableResultInfoList.Add(
            new ResultInfo(table.GetAliasOrTableName()
                         , tableColumnName
                         , new TableAndColumn(table, tableColumnName))
          );
        };
        _stack.Push(tableResultInfoList);
      } else {;
        _stack.Push(new ResultInfoList());
      }
    }

    public sealed override void Visit(JoinOperator joinOperator) {
      _stack.Peek().IsLeftOperandOfJoinOrCompoundOp = true;
    }

    public sealed override void Visit(JoinSource joinSource) {
      if(this.IsNotInMainResultsSource()) {
        return;
      }
      var rightSources = _stack.Pop();
      var leftSources = _stack.Peek();
      leftSources.AddRange(rightSources);
    }

    public sealed override void VisitOnSeparator(CommaJoinSource commaJoinSource, int offset, int i) {
      _stack.Peek().IsLeftOperandOfJoinOrCompoundOp = true;
    }

    public sealed override void VisitAfter(CommaJoinSource commaJoinSource) {
      if(this.IsNotInMainResultsSource()) {
        return;
      }
      var rightSources = _stack.Pop();
      var leftSources = _stack.Peek();
      leftSources.AddRange(rightSources);
    }

    public sealed override void VisitOnCompoundOp(CompoundQueryClause compoundQuery, int offset) {
      _stack.Peek().IsLeftOperandOfJoinOrCompoundOp = true;
    }

    public sealed override void VisitAfter(CompoundQueryClause compoundQuery) {
      if(this.IsNotInMainResultsSource()) {
        return;
      }
      // 集合演算がEXISTS句内の場合以降の処理は行わない(2018-04-12 Added)
      if(_subQueryStack.Contains(SubQueryType.Exists)) {
        return;
      }
      var rightSources = _stack.Pop();
      var leftSources = _stack.Peek();
      if(leftSources.Count != rightSources.Count) {
        throw new ApplicationException("The count of left and right compound operands differ");
      }
      for(int i = 0; i < leftSources.Count; ++i) {
        leftSources[i].Sources.AddRange(rightSources[i].Sources);
      }
    }

    public sealed override void VisitBefore(SingleQueryClause query) {
      if(!query.HasFrom) {
        // FROM句がない場合はSELECT句サブクエリのResultInfoListが
        // Stackに積まれる前に空のResultInfoListを積んでおく
        _stack.Push(new ResultInfoList());
      }
    }

    public sealed override void VisitAfter(SingleQueryClause query) {
      // ReadロックはReadするテーブル行をロックするのが目的なので、その行を抽出するための条件
      // (EXISTS, IN, ..)で用いられるサブクエリ内のテーブル行はロックしない
      if(this.IsNotInMainResultsSource()) {
        return;
      }

      //
      // このQueryの列情報(ResultInfoList)を作成する
      //

      // 作成するこのQueryの列情報
      var resultInfoList = new ResultInfoList();
      // SELECT句サブクエリ
      var subQueries = new Stack<ResultInfoList>();
      // FROM句の抽出元テーブル列
      ResultInfoList sources;

      while(true) {
        ResultInfoList subQuery = _stack.Pop();
        if(subQuery.IsSubQueryInResults) {
          subQueries.Push(subQuery);
        } else if(this.CurrentSubQueryIs(SubQueryType.Exists)) {
          // Existsサブクエリの場合はSELECT句情報を作成しない
          return;
        } else if(this.CurrentSubQueryIs(SubQueryType.OrderBy)) {
          // OrderByサブクエリの場合はSELECT句情報を作成しない
          return;
        } else {
          sources = subQuery;
          break;
        }
      }

      if(query.HasWildcard) {
        foreach(var source in sources) {
          resultInfoList.Add(new ResultInfo("", source.ColumnAliasName, null, source));
        }
        _stack.Push(resultInfoList);
        return;
      }

      foreach(var resultColumn in query.Results) {
        if(resultColumn.IsTableWildcard) {
          var tableWildcard = (TableWildcard)resultColumn;
          foreach(var source in sources) {
            if(source.TableAliasName == tableWildcard.TableAliasName) {
              resultInfoList.Add(new ResultInfo("", source.ColumnAliasName, tableWildcard, source));
            }
          }
        } else {
          var resultExpr = (ResultExpr)resultColumn;
          if(resultExpr.Value.GetType() == typeof(Column)) {
            // sourcesから参照元のResultInfoを見つける
            var column = (Column)resultExpr.Value;
            var columnAliasName = string.IsNullOrEmpty(resultExpr.AliasName) ? column.Name : resultExpr.AliasName;
            var foundInSources = false;
            foreach(var source in sources) {
              if(source.IsDirectSource(column, _ignoreCase)) {
                resultInfoList.Add(new ResultInfo("", columnAliasName, resultExpr, source));
                foundInSources = true;
                break;
              }
            }
            // 参照元のResultInfoが見つからない場合(抽出元テーブルの列名が不明な場合等)
            // UNIONでResultInfoをマージするためSELECT句には必ずResultInfoを用意する
            if(!foundInSources) {
              resultInfoList.Add(new ResultInfo("", columnAliasName, resultExpr));
            }

          } else if(resultExpr.Value.GetType() == typeof(SubQueryExp)) {
            var subQueryExp = (SubQueryExp)resultExpr.Value;
            var subQueryResultInfo = subQueries.Pop();
            if(subQueryResultInfo.Count == 0) {
              resultInfoList.Add(new ResultInfo("", resultExpr.AliasName, resultExpr, null));
            } else {
              resultInfoList.Add(new ResultInfo("", resultExpr.AliasName, resultExpr, subQueryResultInfo[0]));
            }

          } else {
            // UNIONでResultInfoをマージするためSELECT句には必ずResultInfoを用意する
            resultInfoList.Add(new ResultInfo("", resultExpr.AliasName, resultColumn));
          }
        }  // if
      } // for

      if(query.HasGroupBy) {
        // GROUP BY句がある場合は、そのGROUPBY句で指定されていないSELECT句は
        // 一致条件の被演算子に指定されてもテーブル列への一致条件とは見做さない
        for(var i = resultInfoList.Count - 1; i >= 0; --i) {
          if(!this.FindInGroupByItems(query.GroupBy, resultInfoList[i], i)) {
            resultInfoList[i] = new ResultInfo(""
                                             , resultInfoList[i].ColumnAliasName
                                             , resultInfoList[i].Node);
          }
        }
      } else {
        var aggregateFinder = new FindAggregateExprVisitor();
        query.Results.Accept(aggregateFinder);
        if(aggregateFinder.ContainsAggregativeExpr) {
          // GROUP BY句がなく集約関数がある場合は、全てのSELECT句は
          // 一致条件の被演算子に指定されてもテーブル列への一致条件とは見做さない
          for(var i = resultInfoList.Count - 1; i >= 0; --i) {
            resultInfoList[i] = new ResultInfo(""
                                              , resultInfoList[i].ColumnAliasName
                                              , resultInfoList[i].Node);
          }
        }
      } // if

      _stack.Push(resultInfoList);
    }

    // 2018/04/20追加
    public sealed override void VisitBefore(AliasedQuery aliasedQuery) {
      if(aliasedQuery.GetType() != typeof(MergeStmt)) {
        // FROM句のAliasedQueryの場合
        _subQueryStack.Push(SubQueryType.From);
      }
    }

    public sealed override void VisitAfter(AliasedQuery aliasedQuery) {
      if(aliasedQuery.GetType() != typeof(MergeStmt)) {
        // FROM句のAliasedQueryの場合
        _subQueryStack.Pop();
      }

      if(this.IsNotInMainResultsSource()) {
        return;
      }
      foreach(var resultInfo in _stack.Peek()) {
        resultInfo.TableAliasName = aliasedQuery.AliasName ?? "";
      }
    }


    #region count sub query nest level

    public sealed override void VisitBefore(ExistsPredicate predicate) {
      _subQueryStack.Push(SubQueryType.Exists);
    }
    public sealed override void VisitAfter(ExistsPredicate predicate) {
      _subQueryStack.Pop();
    }
    public sealed override void VisitBefore(InPredicate predicate) {
      if(predicate.HasSubQuery) {
        ++_inNestLevel;
      }
    }
    public sealed override void VisitAfter(InPredicate predicate) {
      if(predicate.HasSubQuery) {
        --_inNestLevel;
      }
    }
    public sealed override void VisitBefore(SubQueryPredicate predicate) {
      ++_anySomeAllNestLevel;
    }
    public sealed override void VisitAfter(SubQueryPredicate predicate) {
      --_anySomeAllNestLevel;
    }
    public sealed override void VisitBefore(CaseExpr expr) {
      ++_caseNestLevel;
    }
    public sealed override void VisitAfter(CaseExpr expr) {
      --_caseNestLevel;
    }
    public sealed override void VisitBefore(SubQueryExp expr) {
      if(!expr.IsUsedInResultColumn()) {
        ++_queryExprNestLevel;
      } else {
        _subQueryStack.Push(SubQueryType.InResults);
      }
    }
    public sealed override void VisitAfter(SubQueryExp expr) {
      if(!expr.IsUsedInResultColumn()) {
        --_queryExprNestLevel;
      } else {
        // SELECT句サブクエリの目印をつける
        _stack.Peek().IsSubQueryInResults = true;
        _subQueryStack.Pop();
      }
    }

    #endregion


    private string GetTableAliasName(Table table) {
      // テーブル別名、テーブル別名コメント、テーブル名の順に優先して名称を取得する
      if(!Identifier.IsNullOrEmpty(table.AliasName)) {
        return table.AliasName;
      } else if(!string.IsNullOrEmpty(table.ImplicitAliasName)) {
        return table.ImplicitAliasName;
      } else {
        return table.Name;
      }
    }

    // メインクエリの抽出元にならないサブクエリの場合はTrueを返す
    virtual protected bool IsNotInMainResultsSource() {
      return _caseNestLevel > 0 ||
             _inNestLevel > 0 ||
             _anySomeAllNestLevel > 0 ||
             _queryExprNestLevel > 0;
    }

    private bool CurrentSubQueryIs(SubQueryType subQueryType) {
      return _subQueryStack.Count != 0 && _subQueryStack.Peek() == subQueryType;
    }

    private bool FindInGroupByItems(GroupBy groupBy, ResultInfo resultInfo, int resultInfoIndex) {
      foreach(var groupByExpr in groupBy) {
        if(groupByExpr.GetType() == typeof(Column)) {
          var groupByColumn = (Column)groupByExpr;
          // SELECT句がGroupByキーか判定するため、SELECT句がColumn型であり、かつそのColumnの
          // テーブル別名と列名がGROUPBYキーと一致するかを調べる
          if(resultInfo.SourceInfo != null) {
            if(resultInfo.SourceInfo.IsDirectSource(groupByColumn, _ignoreCase)) {
              return true;
            }
          }
        } else if(groupByExpr.GetType() == typeof(UNumericLiteral) &&
                  ((UNumericLiteral)groupByExpr).TryParseToLong() > 0) {
          if(((UNumericLiteral)groupByExpr).TryParseToLong() == resultInfoIndex + 1) {
            return true;
          }
        } else {
          // GroupBy句がExpressionの場合、SELECTに同等な論理式が存在すればTrueを返す
          // 暫定的に同じ文字列か否かで判定する
          var stringifier1 = new CompactStringifier(1024);
          var stringifier2 = new CompactStringifier(1024);
          groupByExpr.Accept(stringifier1);
          if(resultInfo.Node != null) {
            resultInfo.Node.Accept(stringifier2);
          }
          var groupByExprStr = stringifier1.ToString();
          var selectItemStr = stringifier2.ToString();
          if(string.Compare(groupByExprStr, selectItemStr, _ignoreCase) == 0) {
            return true;
          }
        }
      }
      return false;
    }

  }
}
