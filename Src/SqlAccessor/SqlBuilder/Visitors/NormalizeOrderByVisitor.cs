using System;
using System.Collections.Generic;
using System.ComponentModel;
using MiniSqlParser;

namespace MiniSqlParser
{
  /// <summary>
  /// ORDER BY句で用いるColumn名をAS別名に変換する
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class NormalizeOrderByVisitor: GetResultInfoListVisitor
  {
    // OrderingTerm句の削除で用いる情報
    private readonly Stack<OrderingTerm> _orderByInMainQuery;

    // ORDER BY句の変換候補のColumnオブジェクト
    private readonly List<Tuple<OrderingTerm, Column>> _candidateReplaceColumns;

    // メインクエリにおけるORDER BY句の入子階層
    private int _orderByLevelInMainQuery;

    public NormalizeOrderByVisitor(BestCaseDictionary<IEnumerable<string>> tableColumns
                                 , bool ignoreCase = true) 
    : base(tableColumns, ignoreCase)
    {
      _orderByInMainQuery = new Stack<OrderingTerm>();
      _candidateReplaceColumns = new List<Tuple<OrderingTerm, Column>>();
    }

    // メインクエリの抽出元にならないサブクエリの場合はTrueを返す
    protected override bool IsNotInMainResultsSource() {
      return _caseNestLevel > 0 ||
             _inNestLevel > 0 ||
             _anySomeAllNestLevel > 0 ||
             // メインクエリのOrderBy句内を走査対象にする
            (_queryExprNestLevel > 0 && _orderByLevelInMainQuery == 0);
    }

    public override void Visit(Column column) {
      if(_orderByLevelInMainQuery > 0) {
        if(this.IsDeclaredOutOfOrderBy(column)) {
          // 変換候補のColumnオブジェクトを集める
          var orderingTermInfo = _orderByInMainQuery.Peek();
          _candidateReplaceColumns.Add(Tuple.Create(orderingTermInfo, column));
        }
      }
    }

    public override void VisitBefore(OrderingTerm orderingTerm) {
      var orderBy = (OrderBy)orderingTerm.Parent;
      var query = (IQuery)orderBy.Parent;
      if(!query.IsSubQuery) {
        _orderByInMainQuery.Push(orderingTerm);
      }
    }

    public override void VisitBefore(OrderBy orderBy) {
      var query = (IQuery)orderBy.Parent;
      if(!query.IsSubQuery) {
        ++_orderByLevelInMainQuery;
        // OrderByサブクエリの場合はSELECT句情報を作成しない
        _subQueryStack.Push(SubQueryType.OrderBy);
      }
    }

    public override void VisitAfter(OrderBy orderBy) {
      var query = (IQuery)orderBy.Parent;
      if(!query.IsSubQuery) {
        --_orderByLevelInMainQuery;
        _subQueryStack.Pop();

        //
        // 変換候補のColumnのNameを処理する
        // (メインクエリのSELECT句情報が必要なのでこの時点で変換処理する)
        //
        this.ReplaceColumnNameInOrderBy(query);
      }
    }

    private void ReplaceColumnNameInOrderBy(IQuery query) {
      // queryのSELECT句情報を取得する
      var mainQueryResults = this.GetResults((IQueryClause)query);

      foreach(var candidateReplaceColumn in _candidateReplaceColumns) {
        Column replaceColumn = null;
        string newAliasName = null;
        bool aliasNameIsFound = false;

        foreach(var mainQueryResult in mainQueryResults) {
          var aliasName = mainQueryResult.Item2;

          // ORDER BY句でAS別名を参照するColumnはTableAliasNameを指定できない
          if(string.IsNullOrEmpty(candidateReplaceColumn.Item2.TableAliasName) &&
             aliasName == candidateReplaceColumn.Item2.Name) {
            // AS別名で参照しているOrderBy句は、そのまま置き換えない
            replaceColumn = null;
            aliasNameIsFound = true;
            break;

          } else if(mainQueryResult.Item1 != null) {
            var resultColumn = mainQueryResult.Item1;
            if(this.IsEqual(resultColumn, candidateReplaceColumn.Item2)) {
              // Column名で参照しているOrderBy句は、
              // AS別名で参照するように置き換える
              replaceColumn = candidateReplaceColumn.Item2;
              newAliasName = aliasName;
            }
          }
        } // foreach

        // ORDER BY句のColumnオブジェクトをAS別名に変換する
        if(replaceColumn != null) {
          replaceColumn.ServerName = null;
          replaceColumn.DataBaseName = null;
          replaceColumn.SchemaName = null;
          replaceColumn.TableAliasName = null;
          replaceColumn.Name = newAliasName;

        } else if(!aliasNameIsFound) {
          // CompoundQueryの場合SELECT句に無い列はORDER BY句で参照できないので、
          // SELECT句に補完しない
          var queryClause = (IQueryClause)query;
          while(queryClause.Type == QueryType.Bracketed) {
            queryClause = ((BracketedQueryClause)queryClause).Operand;
          }
          if(queryClause.Type == QueryType.Compound) {
            // 
            this.RemoveOrderingTerm(query.OrderBy, candidateReplaceColumn.Item1);
            continue;
          }
          var singleQueryClause = (SingleQueryClause)queryClause;

          // DISTINCTの指定がある場合、SELECT句に補完しない
          if(singleQueryClause.Quantifier == QuantifierType.Distinct) {
            this.RemoveOrderingTerm(query.OrderBy, candidateReplaceColumn.Item1);
            continue;
          }

          if(singleQueryClause.HasGroupBy) {
            // DISTINCTの指定が無く、GROUP BY句にORDER BY句で指定するColumnがある場合
            // そのColumnを参照するSELECT句を補完する
            var foundInGroupBy = false;
            foreach(var g in singleQueryClause.GroupBy) {
              if(g.GetType() == typeof(Column)) {
                if(this.IsEqual((Column)g, candidateReplaceColumn.Item2)) {
                  this.AppendResult(query, (Column)candidateReplaceColumn.Item2.Clone());
                  foundInGroupBy = true;
                  break;
                }
              }
            } // foreach

            // GROUP BY句にORDER BY句で指定するColumnがない場合、そのOrderingTerm句を削除する
            if(!foundInGroupBy) {
              this.RemoveOrderingTerm(query.OrderBy, candidateReplaceColumn.Item1);
            }

          } else {
            // GROUP BY句がなく集約関数のみの場合、SELECT句に補完しない
            var aggregateFinder = new FindAggregateExprVisitor();
            singleQueryClause.Results.Accept(aggregateFinder);
            if(aggregateFinder.ContainsAggregativeExpr) {
              this.RemoveOrderingTerm(query.OrderBy, candidateReplaceColumn.Item1);
              continue;
            }

            // SELECT句にORDER BY句が指定するAS別名またはColumnが無いが、
            // 抽出元にある場合は、その抽出元を参照するSELECT句を補完する
            foreach(var resultInfo in _stack.Peek()) {
              if(resultInfo.IsDirectSource(candidateReplaceColumn.Item2, _ignoreCase)) {
                this.AppendResult(query, (Column)candidateReplaceColumn.Item2.Clone());
                break;
              }
            } // foreach
          }

        } // if

      } // foreach
    }

    private void AppendResult(IQuery query, Column column) {
      var newResult = new ResultExpr(column);
      var addResultVisitor = new AddResultVisitor(newResult);
      query.Accept(addResultVisitor);
    }

    private void RemoveOrderingTerm(OrderBy orderBy, OrderingTerm orderingTerm) {
      for(int i = 0; i < orderBy.Count; ++i) {
        if(object.Equals(orderBy[i], orderingTerm)) {
          orderBy.RemoveAt(i);
          break;
        }
      }
    }

    private bool IsColumn(ResultInfo resultInfo) {
      return resultInfo.Node != null &&
            !resultInfo.Node.IsTableWildcard &&
            ((ResultExpr)resultInfo.Node).Value.GetType() == typeof(Column);
    }

    private bool IsEqual(Column result, Column orderBy) {
      if(!Identifier.Compare(result.Name, orderBy.Name, _ignoreCase)) {
        return false;
      } else if(Identifier.IsNullOrEmpty(result.TableAliasName) ||
                Identifier.IsNullOrEmpty(orderBy.TableAliasName)) {
        // どちらかのテーブル名が省略されている場合、trueを返す
        return true;
      } else if(!Identifier.Compare(result.TableAliasName, orderBy.TableAliasName, _ignoreCase)) {
        // テーブル別名が指定されている場合はテーブル別名を参照する
        return false;
      } else if(Identifier.IsNullOrEmpty(result.SchemaName) ||
                Identifier.IsNullOrEmpty(orderBy.SchemaName)) {
        // どちらかのスキーマ名が省略されている場合、trueを返す
        return true;
      } else if(!Identifier.Compare(result.SchemaName, orderBy.SchemaName, _ignoreCase)) {
        // テーブル別名が指定されている場合はスキーマ別名を参照する
        return false;
      } else if(Identifier.IsNullOrEmpty(result.DataBaseName) ||
                Identifier.IsNullOrEmpty(orderBy.DataBaseName)) {
        // どちらかのデータベース名が省略されている場合、trueを返す
        return true;
      } else if(!Identifier.Compare(result.DataBaseName, orderBy.DataBaseName, _ignoreCase)) {
        // データベース別名が指定されている場合はテーブル別名を参照する
        return false;
      } else if(Identifier.IsNullOrEmpty(result.ServerName) ||
                Identifier.IsNullOrEmpty(orderBy.ServerName)) {
        // どちらかのサーバ名が省略されている場合、trueを返す
        return true;
      } else if(!Identifier.Compare(result.ServerName, orderBy.ServerName, _ignoreCase)) {
        // サーバ別名が指定されている場合はテーブル別名を参照する
        return false;
      } else {
        // SELECT句とOrderBy句が全ての修飾子を指定され、全て一致する場合、tureを返す
        return true;
      }
    }

    // 指定したQueryClauseのSELECT句情報を取得する
    // (Column、AS別名)
    private List<Tuple<Column, Identifier>> GetResults(IQueryClause queryClause) {
      var rets = new List<Tuple<Column, Identifier>>();

      if(queryClause.Type == QueryType.Single) {
        var singleQueryClause = (SingleQueryClause)queryClause;
        if(singleQueryClause.HasWildcard) {
          foreach(var resultInfo in _stack.Peek()) {
            rets.Add(Tuple.Create<Column, Identifier>(null, resultInfo.ColumnAliasName));
          }

        } else {
          foreach(var result in singleQueryClause.Results) {
            if(result.IsTableWildcard) {
              var tableAliasName = ((TableWildcard)result).TableAliasName;
              foreach(var resultInfo in _stack.Peek()) {
                if(resultInfo.TableAliasName == tableAliasName) {
                  rets.Add(Tuple.Create<Column, Identifier>(null, resultInfo.ColumnAliasName));
                }
              } // foreach

            } else {
              var resultExpr = (ResultExpr)result;
              var columnAliasName = resultExpr.GetAliasOrColumnName();
              if(!string.IsNullOrEmpty(columnAliasName)) {
                Tuple<Column, Identifier> ret;
                if(resultExpr.Value.GetType() == typeof(Column)) {
                  ret = Tuple.Create((Column)resultExpr.Value, columnAliasName);
                } else {
                  ret = Tuple.Create<Column, Identifier>(null, columnAliasName);
                }
                rets.Add(ret);
              }
            }
          } // foreach
        }
        return rets;

      } else if(queryClause.Type == QueryType.Bracketed) {
        return this.GetResults(((BracketedQueryClause)queryClause).Operand);
      } else if(queryClause.Type == QueryType.Compound) {
        return this.GetResults(((CompoundQueryClause)queryClause).Left);
      } else {
        throw new InvalidEnumArgumentException("Undefined QueryType is used");
      }
    }

    // 指定したColumnがメインクエリのORDER BY句で用いられている場合Trueを返す
    private bool IsDeclaredOutOfOrderBy(Column column) {
      int depth = 1;
      foreach(var resultInfoList in _stack) {
        if(this.GetSourcesOf(column, resultInfoList)) {
          // _stackの最下層(=メインクエリのスコープ)がColumnの宣言場所の場合はTrue
          return depth == _stack.Count;
        }
        ++depth;
      }
      // Columnの宣言場所が分からない場合はTrueとする
      return true;
    }

    private bool GetSourcesOf(Column column, ResultInfoList resultInfoList) {
      foreach(var resultInfo in resultInfoList) {
        if(resultInfo.IsDirectSource(column, _ignoreCase)) {
          return true;
        }
      }
      return false;
    }

  }
}
