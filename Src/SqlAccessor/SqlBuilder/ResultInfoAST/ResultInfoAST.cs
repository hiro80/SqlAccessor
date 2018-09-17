using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace MiniSqlParser
{
  public class ResultInfoAST
  {
    public enum PrimaryKeyCompletion
    {
      // 主キーを補完しない
      None,
      // メインクエリ以外に主キーを補完する
      SubQueryOnly,
      // メインクエリを含む全てのクエリに主キーを補完する
      AllQuery
    }

    [System.Diagnostics.DebuggerDisplay("Current: {Current}")]
    private class ExplicitDeclIterator: IEnumerator<IResultInfoInternal>
    {
      private ResultInfoList _resultInfoList;
      private int _index;
      private int _explicitIndex;

      public bool MoveNext() {
        ++_index;
        for(var i = _index; i < _resultInfoList.Count; ++i) {
          if(_resultInfoList[i].ExplicitDecl) {
            _index = i;
            ++_explicitIndex;
            return true;
          }
        }
        return false;
      }
      public IResultInfoInternal Current {
        get { return _resultInfoList[_index]; }
      }
      object IEnumerator.Current {
        get { return _resultInfoList[_index]; }
      }
      public int CurrentIndex {
        get {
          return _explicitIndex;
        }
      }
      public void Reset() { _index = -1; }
      public void Dispose() { }
      public ExplicitDeclIterator(ResultInfoList resultInfoList) {
        _resultInfoList = resultInfoList;
        _index = -1;
        _explicitIndex = -1;
      }
    }

    private readonly ResultInfoList _resultInfoList;

    /// <summary>
    /// <テーブル名、テーブル列情報>
    /// </summary>
    private readonly BestCaseDictionary<IEnumerable<TableResultInfo>> _tableColumns;
    /// <summary>
    /// AS別名と列名の大文字小文字を区別しない場合、True
    /// </summary>
    private readonly bool _ignoreCase;

    public ResultInfoList GetResultInfoList() {
      return _resultInfoList;
    }

    public string Print(bool indent = false) {
      var ret = "";

      if(_resultInfoList == null) {
        return ret;
      }

      foreach(var resultInfo in _resultInfoList) {
        ret += this.Print(resultInfo, 0, indent);
        if(indent) {
          ret += Environment.NewLine;
        }
      }

      return ret;
    }

    private string Print(IResultInfo resultInfo, int level, bool indent) {
      var ret = "";

      if(resultInfo == null) {
        return ret;
      }

      if(indent && level > 0) {
        ret += Environment.NewLine;
        ret += new string(' ', level * 2);
      }

      ret += resultInfo.ExplicitDecl? "[" : "(";

      ret += resultInfo.TableAliasName + ".";
      ret += resultInfo.ColumnAliasName;

      if(!resultInfo.IsNullable) {
        ret += " notNull";
      }

      //if(resultInfo.IsUnique) {
      //  ret += " unique";
      //}

      if(resultInfo.KeyType == KeyType.Table) {
        ret += " tableKey";
      } else if(resultInfo.KeyType == KeyType.Group) {
        ret += " groupKey";
      } else if(resultInfo.KeyType == KeyType.Count) {
        ret += " countKey";
      }

      if(resultInfo.IsComplemented) {
        ret += " complemented";
      }

      if(resultInfo.Type == ResultInfoType.Query || resultInfo.Type == ResultInfoType.Count) {
        ret += " ";
        ret += this.Print(((AbstractSingleQueryResultInfo)resultInfo).SourceInfo, level+1, indent);
      } else if(resultInfo.Type == ResultInfoType.Compound) {
        var compoundQueryResultInfo = (CompoundQueryResultInfo)resultInfo;
        ret += " ";
        ret += this.Print(compoundQueryResultInfo.LeftResultInfo, level + 1, indent);
        ret += " ";
        ret += this.Print(compoundQueryResultInfo.RightResultInfo, level + 1, indent);
      }

      ret += resultInfo.ExplicitDecl ? "]" : ")";

      return ret;
    }

    public ResultInfoAST(SelectStmt selectStmt
                        , Dictionary<string, IEnumerable<TableResultInfo>> tableColumns
                        , PrimaryKeyCompletion primaryKeyCompletion = PrimaryKeyCompletion.None
                        , bool ignoreCase = true) {
      _tableColumns = new BestCaseDictionary<IEnumerable<TableResultInfo>>(tableColumns);
      _ignoreCase = ignoreCase;
      // ASTの構築を行う
      _resultInfoList = this.GetResultInfoList(selectStmt.Query, primaryKeyCompletion);
    }

    // 指定したQueryオブジェクトから列のメタ情報を取得する
    private ResultInfoList GetResultInfoList(IQuery query, PrimaryKeyCompletion pKeyCompletion) {
      if(query.Type == QueryType.Single) {
        var singleQuery = (SingleQuery)query;
        return this.GetResultInfoList((SingleQueryClause)singleQuery, pKeyCompletion);
      } else if(query.Type == QueryType.Bracketed) {
        var bracketedQuery = (BracketedQuery)query;
        return this.GetResultInfoList(bracketedQuery.Operand, pKeyCompletion);
      } else if(query.Type == QueryType.Compound) {
        var compoundQuery = (CompoundQuery)query;

        // UNION文への主キー補完は未実装なので暫定的にUNOINの被演算子であるSELECT文には
        // (そのサブクエリも含め)主キー補完をしない
        pKeyCompletion = 
          compoundQuery.Operator == CompoundType.Union ? PrimaryKeyCompletion.None : pKeyCompletion;

        var leftResultInfoList = this.GetResultInfoList(compoundQuery.Left, pKeyCompletion, true);
        var rightResultInfoList = this.GetResultInfoList(compoundQuery.Right, pKeyCompletion, true);
        if(pKeyCompletion == PrimaryKeyCompletion.AllQuery ||
           pKeyCompletion == PrimaryKeyCompletion.SubQueryOnly) {
          // 主キーかつSelect句項目から参照されていない項目をsingleQueryClauseオブジェクトに追加する
          this.ComplementPrimaryKeyNodeToQuery(compoundQuery, leftResultInfoList, rightResultInfoList);
        }
        return this.CreateResultInfoList(leftResultInfoList, rightResultInfoList, compoundQuery.Operator);
      } else{
        throw new InvalidEnumArgumentException("Undefined QueryType is used");
      }

    }

    // 指定したQueryClauseオブジェクトから列のメタ情報を取得する
    private ResultInfoList GetResultInfoList(IQueryClause queryClause
                                            , PrimaryKeyCompletion pKeyCompletion
                                            , bool explictOnly = false) {
      if(queryClause.Type == QueryType.Single) {
        var singleQueryClause = (SingleQueryClause)queryClause;

        // From句項目からResultInfoListを取得する
        ResultInfoLists sourceInfoLists = null;
        if(singleQueryClause.From == null) {
          sourceInfoLists = new ResultInfoLists();
        } else {
          // DISTINCTが指定されたSELECT文へ列の補完はできないので、そのFROM句内の全てのサブクエリへも補完をしない
          var fromPKeyCompletion =
            singleQueryClause.Quantifier == QuantifierType.Distinct ? PrimaryKeyCompletion.None : pKeyCompletion;
          sourceInfoLists = this.GetResultInfoLists(singleQueryClause.From, fromPKeyCompletion);
        }

        // From句項目のsourceInfoListとSingleQueryのSELECT句を紐付ける
        var resultInfoLists = this.CreateResultInfoList(singleQueryClause.Quantifier == QuantifierType.Distinct
                                                      , singleQueryClause.Results
                                                      , singleQueryClause.HasWildcard
                                                      , sourceInfoLists
                                                      , singleQueryClause.GroupBy);

        if(pKeyCompletion == PrimaryKeyCompletion.AllQuery ||
            (pKeyCompletion == PrimaryKeyCompletion.SubQueryOnly && singleQueryClause.IsSubQuery)) {
          // 主キーかつSelect句項目から参照されていない項目をsingleQueryClauseオブジェクトに追加する
          this.ComplementPrimaryKeyNodeToQuery(
            singleQueryClause, resultInfoLists[0], resultInfoLists[1], sourceInfoLists);
        }

        var ret = resultInfoLists[0];

        // 集約関数もGROUPBY句も持たないSELECT文の場合、
        // SELECT句で参照されない暗黙的に定義される列のResultInfoも含める
        if(!explictOnly) {
          var hasAggregateFunc = resultInfoLists[0].ContainsAggregateFunc();
          var hasGroupBy = singleQueryClause.GroupBy != null && singleQueryClause.GroupBy.Count > 0;
          if(!hasAggregateFunc && !hasGroupBy) {
            ret.AddRange(resultInfoLists[1].Items);
          }
        }

        return ret;

      } else if(queryClause.Type == QueryType.Bracketed) {
        return this.GetResultInfoList(((BracketedQueryClause)queryClause).Operand, pKeyCompletion);

      } else if(queryClause.Type == QueryType.Compound) {
        var compoundQueryClause = (CompoundQueryClause)queryClause;

        // UNION文への主キー補完は未実装なので暫定的にUNOINの被演算子であるSELECT文には
        // (そのサブクエリも含め)主キー補完をしない
        pKeyCompletion = 
          compoundQueryClause.Operator == CompoundType.Union ? PrimaryKeyCompletion.None : pKeyCompletion;

        var leftResultInfoList = this.GetResultInfoList(compoundQueryClause.Left, pKeyCompletion, true);
        var rightResultInfoList = this.GetResultInfoList(compoundQueryClause.Right, pKeyCompletion, true);
        if(pKeyCompletion == PrimaryKeyCompletion.AllQuery ||
           pKeyCompletion == PrimaryKeyCompletion.SubQueryOnly) {
          // 主キーかつSelect句項目から参照されていない項目をsingleQueryClauseオブジェクトに追加する
          this.ComplementPrimaryKeyNodeToQuery(compoundQueryClause, leftResultInfoList, rightResultInfoList);
        }
        return this.CreateResultInfoList(leftResultInfoList, rightResultInfoList, compoundQueryClause.Operator);
      } else {
        throw new InvalidEnumArgumentException("Undefined QueryType is used");
      }
    }

    // 指定したFromSourceオブジェクトから列のメタ情報を取得する
    private ResultInfoLists GetResultInfoLists(IFromSource fromSource, PrimaryKeyCompletion pKeyCompletion) {
      if(fromSource.Type == FromSourceType.Table) {
        var table = (Table)fromSource;
        var resultInfoList = new ResultInfoList();
        if(!_tableColumns.ContainsKey(table.Name)) {
          // テーブル列情報がない場合、空のResultInfoListsを返す
          return new ResultInfoLists(resultInfoList);
        }
        foreach(var tableResultInfo in _tableColumns[table.Name]) {
          var newTableResultInfo = tableResultInfo.Clone();
          newTableResultInfo.SourceTable = new SqlTable(table);
          newTableResultInfo.TableAliasName = table.AliasName;
          resultInfoList.Add(newTableResultInfo);
        }
        return new ResultInfoLists(resultInfoList);
      } else if(fromSource.Type == FromSourceType.AliasedQuery) {
        // FROM句のサブクエリのAS別名をResultInfoのテーブル名とする
        var aliasedQuery = (AliasedQuery)fromSource;
        var aliasName = aliasedQuery.AliasName;
        var resultInfoList = this.GetResultInfoList(aliasedQuery.Query, pKeyCompletion);
        foreach(var resultInfo in resultInfoList) {
          ((AbstractQueryResultInfo)resultInfo).TableAliasName = aliasName;
        }
        return new ResultInfoLists(resultInfoList);
      } else if(fromSource.Type == FromSourceType.Bracketed) {
        var bracketedSource = (BracketedSource)fromSource;
        var resultInfoLists = this.GetResultInfoLists(bracketedSource.Operand, pKeyCompletion);
        // FROM (T) t1  の場合や
        // FROM ((SELECT * FROM T) t1) t2 の場合最も外側のテーブル列名が有効となる
        if(!string.IsNullOrEmpty(bracketedSource.AliasName)) {
          foreach(var resultInfoList in resultInfoLists) {
            foreach(var resultInfo in resultInfoList) {
              if(resultInfo.Type == ResultInfoType.Query ||
                  resultInfo.Type == ResultInfoType.Compound) {
                ((AbstractQueryResultInfo)resultInfo).TableAliasName = bracketedSource.AliasName;
              } else if(resultInfo.Type == ResultInfoType.Table) {
                ((TableResultInfo)resultInfo).TableAliasName = bracketedSource.AliasName;
              } else {
                throw new InvalidEnumArgumentException("Undefined ResultInfoType is used");
              }
            }
          }
        }
        return resultInfoLists;
      } else if(fromSource.Type == FromSourceType.Join) {
        var joinSource = (JoinSource)fromSource;
        var leftSources = this.GetResultInfoLists(joinSource.Left, pKeyCompletion);
        var rightSources = this.GetResultInfoLists(joinSource.Right, pKeyCompletion);

        // 外部結合の左右または両方の被演算子のIsOuterJoinedをTrueにする
        // (このメソッドの処理の後、IsOuterJoined=TrueのResultInfoを
        //  抽出元とするResultInfoはNullable=Trueにされる)
        if(joinSource.Operator.JoinType == JoinType.Left) {
          this.SetIsOuterJoined(rightSources, true);
        } else if(joinSource.Operator.JoinType == JoinType.Right) {
          this.SetIsOuterJoined(leftSources, true);
        } else if(joinSource.Operator.JoinType == JoinType.Full) {
          this.SetIsOuterJoined(leftSources, true);
          this.SetIsOuterJoined(rightSources, true);
        }

        leftSources.AddRange(rightSources);
        return leftSources;
      } else if(fromSource.Type == FromSourceType.CommaJoin) {
        var commaJoinSource = (CommaJoinSource)fromSource;
        var leftSources = this.GetResultInfoLists(commaJoinSource.Left, pKeyCompletion);
        var rightSources = this.GetResultInfoLists(commaJoinSource.Right, pKeyCompletion);
        leftSources.AddRange(rightSources);
        return leftSources;
      } else {
        throw new InvalidEnumArgumentException("Undefined FromSourceType is used");
      }
    }

    // 指定したsourceInfoListsを参照するResultInfoListsを生成する
    // ResultInfoLists[0]: 明示的に参照する列情報
    // ResultInfoLists[1]: 暗黙的に参照する列情報
    private ResultInfoLists CreateResultInfoList(bool distinct
                                                , ResultColumns results
                                                , bool hasWildcard
                                                , ResultInfoLists sourceInfoLists
                                                , GroupBy groupBy) {
      var explicitList = new ResultInfoList();
      var implicitList = new ResultInfoList();

      if(hasWildcard) {
        // ワイルドカードの場合
        explicitList = this.CreateResultInfoList(sourceInfoLists);

      } else {
        foreach(var result in results) {
          if(result.IsTableWildcard) {
            // テーブルワイルドカードの場合
            var tableWildcard = (TableWildcard)result;
            var resultInfoList = this.CreateResultInfoList(tableWildcard, sourceInfoLists);
            explicitList.AddRange(resultInfoList.Items);

          } else {
            var resultExpr = (ResultExpr)result;
            var resultInfo = this.CreateResultInfo(resultExpr, sourceInfoLists);
            explicitList.Add(resultInfo);

          } // if
        } // foreach
      } // if


      if(distinct) {
        // DISTINCTを持つ場合、全てのSELECT句はUniqueである
        foreach(var resultInfo in explicitList) {
          var queryResultInfo = (AbstractQueryResultInfo)resultInfo;
          queryResultInfo.IsUnique = true;
          queryResultInfo.KeyType = KeyType.Group;
        }
        // SELECT句に定数値しか補完できないため、
        // 暗黙的に定義される列は用いないので以降の処理をスキップする
        return new ResultInfoLists(explicitList, implicitList);
      } // if

      for(var i = 0; i < sourceInfoLists.Count; ++i) {
        for(var j = 0; j < sourceInfoLists[i].Count; ++j) {
          var sourceInfo = sourceInfoLists[i][j];
          // SELECT句で参照されないExplicitな抽出元項目は、SELECT句で暗黙的に定義されていると見做す
          if(sourceInfo.ExplicitDecl && !this.GetIsReferenced(sourceInfo)) {
            var resultInfo = new QueryResultInfo( null
                                                , null
                                                , sourceInfo.ColumnAliasName
                                                , sourceInfo.IsNullable || this.GetIsOuterJoined(sourceInfo)
                                                , sourceInfo.KeyType
                                                , false // isExplicitDecl
                                                , sourceInfo.IsComplemented
                                                , sourceInfo.SourceTable
                                                , sourceInfo.SourceColumnName
                                                , null  // node
                                                , sourceInfoLists[i]
                                                , j);
            implicitList.Add(resultInfo);
          } //if
        } // for
      } // for

      var ret = new ResultInfoLists(explicitList, implicitList);

      #region "集約関数またはGROUPBY句を持つSELECT文のIsNullableとIsUnique属性の判定をする"
      var hasAggregateFunc = explicitList.ContainsAggregateFunc();
      var hasGroupBy = groupBy != null && groupBy.Count > 0;

      if(hasAggregateFunc || hasGroupBy) {
        int i = 0;
        foreach(var resultInfo in ret.GetAllResultInfo()) {
          if(resultInfo.Type != ResultInfoType.Query && resultInfo.Type != ResultInfoType.Count) {
            throw new ApplicationException(
              "Internal error. Type of resultInfo must be AbstractSingleQueryResultInfo type.");
          }
          var queryResultInfo = (AbstractSingleQueryResultInfo)resultInfo;

          if(hasAggregateFunc && !hasGroupBy) {
            // SELECT句に集約関数を含み、かつGroupBy句を持たない場合
            if(queryResultInfo.Type == ResultInfoType.Count) {
              // そのSELECT句がCOUNT関数の場合は、NOT NULLである
              queryResultInfo.IsNullable = false;
              // その抽出結果は必ず1行なので、Uniqueである
              queryResultInfo.IsUnique = true;
              // COUNT()の集計対象列がNotNull列の場合、KeyType=Countとする
              // また集計対象列がUnionAllまたは外部結合により発生するNullableなキー列の場合もKeyType=Countとする
              if(queryResultInfo.SourceInfo == null ||
                 queryResultInfo.SourceInfo.IsNullable && queryResultInfo.SourceInfo.KeyType == KeyType.None){
                queryResultInfo.KeyType = KeyType.None;
              } else {
                queryResultInfo.KeyType = KeyType.Count;
              }
            } else if(((QueryResultInfo)queryResultInfo).IsNotNullAggregateFunc()) {
              // そのSELECT句がNotNULL集約関数の場合は、NOT NULLである
              queryResultInfo.IsNullable = false;
              // その抽出結果は必ず1行なので、Uniqueである
              queryResultInfo.IsUnique = true;
              queryResultInfo.KeyType = KeyType.None;
            } else if(((QueryResultInfo)queryResultInfo).IsAggregative()) {
              // そのSELECT句がNotNULL以外の集約関数の場合は、以下の様にNULLを返す場合もあるため、
              // そのSELECT句はNOT NULLではない
              // ex) select id, sum(id) from [空テーブル]
              queryResultInfo.IsNullable = true;
              // その抽出結果は必ず1行なので、Uniqueである
              queryResultInfo.IsUnique = true;
              queryResultInfo.KeyType = KeyType.None;
            } else {
              // そのSELECT句が集約関数以外の場合は、以下の様にNULLを返す場合もあるため、
              // そのSELECT句はNOT NULLではない
              // ex) select id, sum(id) from [空テーブル]
              queryResultInfo.IsNullable = true;
              // その抽出結果は必ず1行なので、Uniqueである
              queryResultInfo.IsUnique = true;
              queryResultInfo.KeyType = KeyType.None;
            }

          } else if(!hasAggregateFunc && hasGroupBy) {
            // SELECT句に集約関数を含まず、かつGroupBy句を持つ場合、
            // そのSELECT句がGROUPBY句で指定されている場合、Uniqueである
            // またNotNull属性は抽出元のNotNull属性に従う
            if(this.FindInGroupByItems(groupBy, queryResultInfo, i)) {
              queryResultInfo.IsUnique = true;
              queryResultInfo.KeyType = KeyType.Group;
            } else {
              // そのSELECT句が集約関数でもなく、GROUPBY句で指定されてもいない場合、
              // 抽出元のNotNull属性とUnique属性に従う
              // (GROUPBYで集約する前に既にUniqueであるSELECT句は、GROUPBYの集約後もUniqueである)
              queryResultInfo.KeyType = KeyType.None;
            }

          } else if(hasAggregateFunc && hasGroupBy) {
            // SELECT句に集約関数を含み、かつGroupBy句を持つ場合、
            if(queryResultInfo.Type == ResultInfoType.Count ||
              ((QueryResultInfo)queryResultInfo).IsNotNullAggregateFunc()) {
              // そのSELECT句がNotNULL集約関数の場合は、NOT NULLである
              // また集約関数はUniqueでない
              queryResultInfo.IsNullable = false;
              queryResultInfo.IsUnique = false;
            } else if(((QueryResultInfo)queryResultInfo).IsAggregative()) {
              // そのSELECT句が集約関数の場合、
              // その集約関数がNULLを返す仕様の可能性もあるので、NOT NULLではない
              // (集約関数とGROUPBYが存在する場合は空テーブルであってもNULLにならない)
              queryResultInfo.IsNullable = true;
              queryResultInfo.IsUnique = false;
            } else {
              // そのSELECT句が集約関数でもなく、GROUPBY句で指定されてもいない場合、
              // 抽出元のNotNull属性とUnique属性に従う
              // (GROUPBYで集約する前に既にUniqueであるSELECT句は、GROUPBYの集約後もUniqueである)
              queryResultInfo.KeyType = KeyType.None;
            }
            if(this.FindInGroupByItems(groupBy, queryResultInfo, i)) {
              // そのSELECT句がGROUPBY句で指定されている場合、Uniqueである
              // またNotNull属性は抽出元のNotNull属性に従う
              // (GROUPBY句に集約関数が指定されている場合も含む)
              queryResultInfo.IsUnique = true;
              queryResultInfo.KeyType = KeyType.Group;
            }
          }

          ++i;
        } // foreach

      }// if
      #endregion

      return ret;
    }

    // 指定したResultInfoListを参照するResultInfoListを生成する
    private ResultInfoList CreateResultInfoList(ResultInfoList leftResultInfoList
                                              , ResultInfoList rightResultInfoList
                                              , CompoundType compoundOperator) {
      var ret = new ResultInfoList();

      // Compound Queryに暗黙的な列は無いので除外する
      var leftSourceItr = new ExplicitDeclIterator(leftResultInfoList);
      var rightSourceItr = new ExplicitDeclIterator(rightResultInfoList);

      while(leftSourceItr.MoveNext()) {
        if(!rightSourceItr.MoveNext()) {
          throw new InvalidASTStructureError(
            "集合演算の被演算子のSELECT句の数は同じでなければなりません");
        }

        var leftResultInfo = (AbstractQueryResultInfo)leftSourceItr.Current;
        var rightResultInfo = (AbstractQueryResultInfo)rightSourceItr.Current;
        var compoundResultInfo = this.CreateResultInfo(leftResultInfo, rightResultInfo, compoundOperator);

        ret.Add(compoundResultInfo);
      }

      return ret;
    }

    // 指定したResultInfoを参照するResultInfoを生成する
    private CompoundQueryResultInfo CreateResultInfo(AbstractQueryResultInfo leftSourceInfo
                                                    , AbstractQueryResultInfo rightSourceInfo
                                                    , CompoundType compoundOperator) {
      CompoundQueryResultInfo ret;
      if(compoundOperator == CompoundType.UnionAll) {
        // ・AS別名は先頭サブクエリのSELECT句のAS別名を使う
        // ・重複する行が存在し得るためUniqueではない
        //   ただし、サブクエリの一方がNULLリテラルで、もう一方がUniqueの場合はUniqueとみなす
        //var isUnique = leftSourceInfo.IsUnique && rightSourceInfo.IsNullLiteral ||
        //               rightSourceInfo.IsUnique && leftSourceInfo.IsNullLiteral;
        var keyType = leftSourceInfo.IsNullLiteral ? rightSourceInfo.KeyType :
                      rightSourceInfo.IsNullLiteral ? leftSourceInfo.KeyType :
                      KeyType.None;
        var sourceTable = leftSourceInfo.IsNullLiteral ? rightSourceInfo.SourceTable :
                          rightSourceInfo.IsNullLiteral ? leftSourceInfo.SourceTable :
                          null;
        var sourceColumnName = leftSourceInfo.IsNullLiteral ? rightSourceInfo.SourceColumnName :
                               rightSourceInfo.IsNullLiteral ? leftSourceInfo.SourceColumnName :
                               null;
        ret = new CompoundQueryResultInfo(
                      null
                    , leftSourceInfo.ColumnAliasName
                    , leftSourceInfo.IsNullable || rightSourceInfo.IsNullable
                    //, isUnique
                    , keyType
                    , leftSourceInfo.IsComplemented || rightSourceInfo.IsComplemented
                    , sourceTable
                    , sourceColumnName
                    , leftSourceInfo
                    , rightSourceInfo
                  );
      } else if(compoundOperator == CompoundType.Union) {
        // ・AS別名は先頭サブクエリのSELECT句のAS別名を使う
        // ・SELECT句全てUniqueである
        var keyType = leftSourceInfo.IsNullLiteral ? rightSourceInfo.KeyType :
                      rightSourceInfo.IsNullLiteral ? leftSourceInfo.KeyType :
                      KeyType.None;
        var sourceTable = leftSourceInfo.IsNullLiteral ? rightSourceInfo.SourceTable :
                          rightSourceInfo.IsNullLiteral ? leftSourceInfo.SourceTable :
                          null;
        var sourceColumnName = leftSourceInfo.IsNullLiteral ? rightSourceInfo.SourceColumnName :
                               rightSourceInfo.IsNullLiteral ? leftSourceInfo.SourceColumnName :
                               null;
        ret = new CompoundQueryResultInfo(
                      null
                    , leftSourceInfo.ColumnAliasName
                    , leftSourceInfo.IsNullable || rightSourceInfo.IsNullable
                    //, true // isUnique
                    , keyType
                    , leftSourceInfo.IsComplemented || rightSourceInfo.IsComplemented
                    , sourceTable
                    , sourceColumnName
                    , leftSourceInfo
                    , rightSourceInfo
                  );
      } else if(compoundOperator == CompoundType.Intersect) {
        // ・AS別名は先頭サブクエリのSELECT句のAS別名を使う
        // ・両方のサブクエリのSELECT句がUniqueであればUniqueである
        // ・Intersect句で連結された両方のサブクエリが抽出元になる

        // KeyType.TableとKeyType.GroupByが重なった場合、GroupByを返す
        // (TableでもGroupByでもどちらでもいいと思うが)
        var keyType = leftSourceInfo.KeyType == rightSourceInfo.KeyType ? leftSourceInfo.KeyType :
                      leftSourceInfo.KeyType == KeyType.None ? KeyType.None :
                      rightSourceInfo.KeyType == KeyType.None ? KeyType.None : 
                      KeyType.Group  ;

        ret = new CompoundQueryResultInfo(
                      null
                    , leftSourceInfo.ColumnAliasName
                    , leftSourceInfo.IsNullable || rightSourceInfo.IsNullable
                    //, leftSourceInfo.IsUnique && rightSourceInfo.IsUnique
                    , keyType
                    , leftSourceInfo.IsComplemented || rightSourceInfo.IsComplemented
                    , null
                    , null
                    , leftSourceInfo
                    , rightSourceInfo
                  );
      } else if(compoundOperator == CompoundType.Except ||
                compoundOperator == CompoundType.Minus) {
        // ・AS別名は先頭サブクエリのSELECT句のAS別名を使う
        // ・先頭サブクエリがUniqueであればUniqueである
        // ・Except/Minus句で連結された先頭サブクエリだけが抽出元になる
        ret = new CompoundQueryResultInfo(
                      null
                    , leftSourceInfo.ColumnAliasName
                    , leftSourceInfo.IsNullable
                    //, leftSourceInfo.IsUnique
                    , leftSourceInfo.KeyType
                    , leftSourceInfo.IsComplemented
                    , leftSourceInfo.SourceTable
                    , leftSourceInfo.SourceColumnName
                    , leftSourceInfo
                    , null
                  );
      } else {
        throw new InvalidEnumArgumentException("Undefined CompoundType is used");
      }

      return ret;
    }

    // 指定したResultInfoを参照するQueryResultInfoを生成する
    private AbstractSingleQueryResultInfo CreateResultInfo(ResultExpr resultExpr
                                                         , ResultInfoLists sourceInfoLists) {
      var exprValue = resultExpr.Value;

      // 指定したSELECT句が括弧で包まれている場合は、括弧を剥く
      while(exprValue.GetType() == typeof(BracketedExpr)) {
        exprValue = ((BracketedExpr)exprValue).Operand;
      }

      if(exprValue.GetType() == typeof(Column)) {
        QueryResultInfo resultInfo = null;
        var column = (Column)exprValue;

        foreach(var sourceInfoList in sourceInfoLists) {
          // From句項目にSelect句項目の抽出元SELECT項目が存在するか否か
          var sourceInfoListIndex = sourceInfoList.FindResultInfo(column, true, _ignoreCase);

          if(sourceInfoListIndex >= 0){

            if(resultInfo != null) {
              throw new InvalidASTStructureError(
                "Ambiguous column name: " + resultInfo.ColumnAliasName);
            }

            var sourceInfo = sourceInfoList[sourceInfoListIndex];
            // SELECT句から参照されたことを示す目印をつける
            this.SetIsReferenced(sourceInfo, true);
            // ResultInfoオブジェクトを生成する
            resultInfo = new QueryResultInfo( null
                                            , column.Name
                                            , resultExpr.AliasName
                                            , sourceInfo.IsNullable || this.GetIsOuterJoined(sourceInfo)
                                            //, sourceInfo.IsUnique
                                            , sourceInfo.KeyType
                                            , true  // isExplicitDesl
                                            , sourceInfo.IsComplemented
                                            , sourceInfo.SourceTable
                                            , sourceInfo.SourceColumnName
                                            , resultExpr
                                            , sourceInfoList
                                            , sourceInfoListIndex);
          } // if
        } // foreach

        if(resultInfo == null) {
            // SELECT句がColumn型だが抽出元SELECT句が存在しない場合
            // 抽出元無しのResultInfoオブジェクトを生成する
            resultInfo = new QueryResultInfo( null
                                            , column.Name
                                            , resultExpr.AliasName
                                            , true  // isNullable
                                            //, false // isUnique
                                            , KeyType.None // keyType
                                            , true  // isExplicitDesl
                                            , false // isComplemented
                                            , resultExpr);
        } // if

        return resultInfo;

      } else if(exprValue.GetType() == typeof(AggregateFuncExpr) && 
                ((AggregateFuncExpr)exprValue).Name.ToUpper() == "COUNT") {
        // COUNT()の場合
        CountQueryResultInfo resultInfo = null;
        var count = (AggregateFuncExpr)exprValue;

        foreach(var sourceInfoList in sourceInfoLists) {
          // From句項目にSelect句項目の抽出元SELECT項目が存在するか否か
          var sourceInfoListIndex = -1;
          if(count.Wildcard || count.Argument1 == null || count.Argument1.GetType() != typeof(Column)) {
            // 抽出元SELECT項目は存在しない 
          } else {
            sourceInfoListIndex = sourceInfoList.FindResultInfo((Column)count.Argument1, true, _ignoreCase);
          }

          if(sourceInfoListIndex >= 0) {

            if(resultInfo != null) {
              throw new InvalidASTStructureError(
                "Ambiguous column name: " + resultInfo.ColumnAliasName);
            }

            var sourceInfo = sourceInfoList[sourceInfoListIndex];

            // 以下のSELECT文に対しT.yを補完するため、COUNT()からのみ参照された列は参照されたと見做さない
            // select count(y) from T group by y
            // this.SetIsReferenced(sourceInfo, true);

            // COUNT()の集計対象列がNotNull列の場合、KeyType=Countとする
            // また集計対象列がUnionAllまたは外部結合により発生するNullableなキー列の場合もKeyType=Countとする
            KeyType keyType;
            if(sourceInfo.IsNullable && sourceInfo.KeyType == KeyType.None) {
              keyType = KeyType.None;
            } else {
              keyType = KeyType.Count;
            }

            // ResultInfoオブジェクトを生成する
            resultInfo = new CountQueryResultInfo(null
                                                , resultExpr.AliasName
                                                //, sourceInfo.IsNullable ? KeyType.None : KeyType.Count
                                                , keyType
                                                , true  // isExplicitDesl
                                                , sourceInfo.IsComplemented
                                                , sourceInfo.SourceTable
                                                , sourceInfo.SourceColumnName
                                                , resultExpr
                                                , sourceInfoList
                                                , sourceInfoListIndex);
          } // if
        } // foreach

        if(resultInfo == null) {
          // SELECT句がCOUNT()だが抽出元SELECT句が存在しない場合
          // 抽出元無しのResultInfoオブジェクトを生成する
          resultInfo = new CountQueryResultInfo(null
                                              , resultExpr.AliasName
                                              , KeyType.None // keyType
                                              , true  // isExplicitDesl
                                              , false // isComplemented
                                              , resultExpr);
        } // if

        return resultInfo;

      } else if(exprValue.GetType() == typeof(SubQueryExp)) {
        // SELECT句サブクエリの場合
        var subQueryExp = (SubQueryExp)exprValue;
        // SELECT句サブクエリ以下を走査する
        // (SELECT句サブクエリにはSELECT句を補完しない)
        var resultInfoList = this.GetResultInfoList(subQueryExp.Query, PrimaryKeyCompletion.None);
        // SELECT句サブクエリが列を返さない場合
        // ( select (select *) from ...の場合 )
        if(resultInfoList == null || resultInfoList.Count == 0) {
          throw new InvalidASTStructureError("SELECT句サブクエリの列情報を取得できません");
        }
        // ResultInfoオブジェクトを生成する
        var resultInfo = new QueryResultInfo( null
                                            , null
                                            , resultExpr.AliasName
                                            , resultInfoList[0].IsNullable || this.GetIsOuterJoined(resultInfoList[0])
                                            //, resultInfoList[0].IsUnique
                                            , resultInfoList[0].KeyType
                                            , true  // explicitDecl
                                            , resultInfoList[0].IsComplemented
                                            , resultInfoList[0].SourceTable
                                            , resultInfoList[0].SourceColumnName
                                            , resultExpr
                                            , resultInfoList
                                            , 0);
        return resultInfo;

      } else if((exprValue is Literal && exprValue.GetType() != typeof(NullLiteral)) ||
                exprValue.GetType() == typeof(SignedNumberExpr)) {
        // NULL以外のリテラル値はNullable=falseである
        var resultInfo = new QueryResultInfo( null
                                            , null
                                            , resultExpr.AliasName
                                            , false // isNullable
                                            //, false // isUnique
                                            , KeyType.None // keyType
                                            , true  // explicitDecl
                                            , false // isComplemented
                                            , resultExpr);
        return resultInfo;

      } else {
        // 抽出元無しのResultInfoオブジェクトを生成する
        var resultInfo = new QueryResultInfo( null
                                            , null
                                            , resultExpr.AliasName
                                            , true  // isNullable
                                            //, false // isUnique
                                            , KeyType.None // keyType
                                            , true  // explicitDecl
                                            , false // isComplemented
                                            , resultExpr);
        return resultInfo;
      }
    }

    // 指定したResultInfoListsを参照するResultInfoListを生成する(SELECT句がワイルドカードの場合)
    private ResultInfoList CreateResultInfoList(ResultInfoLists sourceInfoLists) {
      var ret = new ResultInfoList();
      foreach(var sourceInfoList in sourceInfoLists) {
        ret.AddRange(this.CreateResultInfoList(sourceInfoList).Items);
      }
      return ret;
    }

    // 指定したResultInfoListを参照するResultInfoListを生成する(SELECT句がワイルドカードの場合)
    private ResultInfoList CreateResultInfoList(ResultInfoList sourceInfoList) {
      var ret = new ResultInfoList();

      // SELECT句がワイルドカードの場合、From句で定義された
      // 全てのExplicitな項目が明示的にSELECT句で定義されていると見做す
      var explicitSourceItr = new ExplicitDeclIterator(sourceInfoList);
      while(explicitSourceItr.MoveNext()) {
        var explicitSourceInfo = explicitSourceItr.Current;

        this.SetIsReferenced(explicitSourceInfo, true);
        var resultInfo = new QueryResultInfo( null
                                            , null
                                            , explicitSourceInfo.ColumnAliasName
                                            , explicitSourceInfo.IsNullable || this.GetIsOuterJoined(explicitSourceInfo)
                                            //, sourceInfo.IsUnique
                                            , explicitSourceInfo.KeyType
                                            , true  // isExplicitDesc
                                            , explicitSourceInfo.IsComplemented
                                            , explicitSourceInfo.SourceTable
                                            , explicitSourceInfo.SourceColumnName
                                            , null
                                            , sourceInfoList
                                            , explicitSourceItr.CurrentIndex);
        ret.Add(resultInfo);
      }

      return ret;
    }

    // 指定したResultInfoListsを参照するResultInfoListを生成する(SELECT句がテーブルワイルドカードの場合)
    private ResultInfoList CreateResultInfoList(TableWildcard tableWildcard
                                              , ResultInfoLists sourceInfoLists) {
      var ret = new ResultInfoList();
      ResultInfoList resultInfoList = null;

      foreach(var sourceInfoList in sourceInfoLists) {
        resultInfoList = this.CreateResultInfoList(tableWildcard, sourceInfoList);
        if(resultInfoList.Count > 0 && ret.Count > 0) {
          throw new InvalidASTStructureError(
            "Ambiguous table wildcard name: " + tableWildcard.TableAliasName);
        }
        ret.AddRange(resultInfoList.Items);
      }
      return ret;
    }

    // 指定したResultInfoListを参照するResultInfoListを生成する(SELECT句がテーブルワイルドカードの場合)
    private ResultInfoList CreateResultInfoList(TableWildcard tableWildcard
                                              , ResultInfoList sourceInfoList) {
      var ret = new ResultInfoList();
      var explicitSourceItr = new ExplicitDeclIterator(sourceInfoList);

      while(explicitSourceItr.MoveNext()) {
        var explicitSourceInfo = explicitSourceItr.Current;

        if(string.Compare(explicitSourceInfo.TableAliasName, tableWildcard.TableAliasName, _ignoreCase) == 0) {
          // SELECT句から参照されたことを示す目印をつける
          this.SetIsReferenced(explicitSourceInfo, true);
          // ResultInfoオブジェクトを生成する
          var resultInfo = new QueryResultInfo( null
                                              , null
                                              , explicitSourceInfo.ColumnAliasName
                                              , explicitSourceInfo.IsNullable || this.GetIsOuterJoined(explicitSourceInfo)
                                              //, sourceInfo.IsUnique
                                              , explicitSourceInfo.KeyType
                                              , true  // isExplicitDecl
                                              , explicitSourceInfo.IsComplemented
                                              , explicitSourceInfo.SourceTable
                                              , explicitSourceInfo.SourceColumnName
                                              , tableWildcard
                                              , sourceInfoList
                                              , explicitSourceItr.CurrentIndex);
          ret.Add(resultInfo);
        } // if
      } // while

      return ret;
    }

    private IQueryClause ConvertUnionToUnionAll(CompoundQueryClause compoundQueryClause) {
      return SingleQueryClause.WrapInSelectStar(
                new AliasedQuery(compoundQueryClause, false, "V2_"));
    }

    private void ComplementPrimaryKeyNodeToQuery(SingleQueryClause query
                                               , ResultInfoList explicitInfoList
                                               , ResultInfoList implicitInfoList
                                               , ResultInfoLists sourceInfoLists) {
      // SELECT句がワイルドカードの場合は列を補完する必要はない
      if(query.HasWildcard) {
        return;
      }

      var complementedResultInfoList = new ResultInfoList();

      // Distinctが指定され、かつ全てのSELECT句がNotNullでない場合、固定値1をSELECT句に追加する
      if(query.Quantifier == QuantifierType.Distinct) {
        if(!explicitInfoList.ContainsNotNull()) {
          // リテラル値を追加する
          var columnAliasName = "RESULT_EXISTS_";
          var resultExpr = new ResultExpr(new UNumericLiteral("1"), true, columnAliasName);
          var queryResultInfo = new QueryResultInfo(null
                                                  , null
                                                  , columnAliasName
                                                  , false // isNullable
                                                  , KeyType.Count
                                                  , true  // explicitDecl
                                                  , true  // isComplemented
                                                  , resultExpr);
          // 追加する参照列内でAs別名が重複する場合、重複しないAs別名にリネームする
          var renamedQueryResultInfo =
              this.RenameCollisionResultInfoAndNode(complementedResultInfoList, queryResultInfo);

          explicitInfoList.Add(renamedQueryResultInfo);
          complementedResultInfoList.Add(renamedQueryResultInfo);

          // SELECT句に補完列を追加する
          this.ComplementPrimaryKeyNodeToQuerySub(query, explicitInfoList, complementedResultInfoList);
        }
        // Distinctが指定されている場合、SELECT句に定数値しか補完できないので以降の処理は必要ない
        return;
      } // if  

      var hasAggregateFunc = explicitInfoList.ContainsAggregateFunc();
      var hasGroupBy = query.GroupBy != null && query.GroupBy.Count > 0;

      // 主キーかつSelect句項目から参照されていない列を補完する
      var columnIndex = explicitInfoList.Count - 1;
      for(int i = 0; i < implicitInfoList.Count; ++i) {
        var implicitQueryResultInfo = (AbstractSingleQueryResultInfo)implicitInfoList[i];

        // 主キーでない列は補完しない
        if(implicitQueryResultInfo.KeyType == KeyType.None) {
          continue;
        }

        // GroupBy句で指定された列か?
        bool findInGroupBy = false;
        if(hasGroupBy){
          findInGroupBy = this.FindInGroupByItems(query.GroupBy, implicitQueryResultInfo, -1);
        }

        AbstractSingleQueryResultInfo complementResultInfo = null;
        if((hasAggregateFunc || hasGroupBy) && !findInGroupBy) {
          // 集約関数またはGroupBy句を持つSelect文の場合、GroupBy句で指定されていない参照先の列を
          // COUNT()でラッピングして補完するため、以降でその処理を行う
          continue;
        }

        // 参照先の列に列名がない場合、別名を付与する
        var sourceInfo = implicitQueryResultInfo.SourceInfo;
        sourceInfo = this.RenameColumnAliasName(sourceInfo, columnIndex);

        // 補完列を作成する
        var v = sourceInfo.TableAliasName;
        var c = sourceInfo.ColumnAliasName;
        var column = new Column(v, c);

        ResultExpr resultExpr;
        if(sourceInfo.IsComplemented) {
          // 補完列を参照する列にはAS別名を付けない
          resultExpr = new ResultExpr(column);
        } else {
          var aliasName = "";
          if(!string.IsNullOrEmpty(v)) {
            aliasName = v + "_";
          }
          aliasName += column.Name + "_";
          resultExpr = new ResultExpr(column, true, aliasName);
        }

        complementResultInfo = new QueryResultInfo(implicitQueryResultInfo.TableAliasName
                                                  , c
                                                  , resultExpr.AliasName
                                                  , implicitQueryResultInfo.IsNullable
                                                  , findInGroupBy ? KeyType.Group : sourceInfo.KeyType
                                                  , true // ExplicitDecl
                                                  , true // IsComplemented
                                                  , sourceInfo.SourceTable
                                                  , sourceInfo.SourceColumnName
                                                  , resultExpr
                                                  , implicitQueryResultInfo.SourceInfoList
                                                  , implicitQueryResultInfo.SourceInfoListIndex);
       
        // SELECT句から参照されたことを示す目印をつける
        this.SetIsReferenced(sourceInfo, true);

        // 暗黙列を明示列にしたのでimplicitListから対象列を削除する
        implicitInfoList.RemoveAt(i);
        --i;

        // 追加する参照列内でAs別名が重複する場合、重複しないAs別名にリネームする
        var renamedComplementResultInfo =
            this.RenameCollisionResultInfoAndNode(complementedResultInfoList, complementResultInfo);

        explicitInfoList.Add(renamedComplementResultInfo);
        complementedResultInfoList.Add(renamedComplementResultInfo);

        ++columnIndex;
      } // for

      // 集約関数もGroupBy句も持たないSelect文の場合、RESULT_COUNT_は付加しないので以降の処理は必要ない
      if(!hasAggregateFunc && !hasGroupBy) {
        // SELECT句に補完列を追加する
        this.ComplementPrimaryKeyNodeToQuerySub(query, explicitInfoList, complementedResultInfoList);
        return;
      }

      // GroupByキーがNullableの場合は同じ抽出元テーブルの主キーを参照するCOUNT()列を補完する
      // そのために、SELECT句から直接/間接的にNotNull列を参照された抽出元テーブルの別名を記録する
      //
      // (SELECT文の異なる箇所で定義されている場合は、抽出元テーブルがAliasNameも含め同じ名称でも、
      //  異なるテーブルとみなすためSqlTableではなくTableオブジェクトのリファレンス値を用いる)
      //
      var referencedSourceTables = new HashSet<Table>();
      foreach(var explicitInfo in explicitInfoList) {
        var explicitQueryInfo = (AbstractSingleQueryResultInfo)explicitInfo;
        var sourceInfo = explicitQueryInfo.SourceInfo;
        // SELECT句が(GroupBy句によるなどの)キー列で、
        // その参照先の列が抽出元テーブルのNotNull列を参照する場合は、抽出元テーブルを参照すると見做す
        if(explicitInfo.KeyType != KeyType.None && sourceInfo != null &&
          (sourceInfo.KeyType != KeyType.None || !sourceInfo.IsNullable)) {
          if(sourceInfo.SourceTable != null) {
            referencedSourceTables.Add(sourceInfo.SourceTable.Table);
          }
        }
      }

      // 集約関数またはGROUPBYを持つ場合、
      // 主キーかつSelect句項目から参照されていない列をCOUNT()でラッピングして補完する
      foreach(var sourceInfoList in sourceInfoLists) {
        for(var i = 0; i < sourceInfoList.Count; ++i) {
          var sourceInfo = sourceInfoList[i];
          if(sourceInfo.KeyType != KeyType.None && !this.GetIsReferenced(sourceInfo)) {
            if(!referencedSourceTables.Contains(sourceInfo.SourceTable.Table)) {
              // RESULT_COUNT_列の追加により対象列の抽出元テーブルは参照済みとなる
              referencedSourceTables.Add(sourceInfo.SourceTable.Table);

              // 参照先の列に列名がない場合、別名を付与する
              sourceInfo = this.RenameColumnAliasName(sourceInfo, columnIndex);
              // 補完するCOUNT()列を作成する
              var column = new Column(sourceInfo.TableAliasName, sourceInfo.ColumnAliasName);
              var countFuncExpr = new AggregateFuncExpr("COUNT", QuantifierType.None, false, column, null);
              var columnAliasName = "RESULT_COUNT_";
              var resultExpr = new ResultExpr(countFuncExpr, true, columnAliasName);

              var countResultInfo = new CountQueryResultInfo(null
                                                            , columnAliasName
                                                            , KeyType.Count
                                                            , true  // explicitDecl
                                                            , true  // isComplemented
                                                            , sourceInfo.SourceTable
                                                            , sourceInfo.SourceColumnName 
                                                            , resultExpr
                                                            , sourceInfoList
                                                            , i);
              // SELECT句から参照されたことを示す目印をつける
              this.SetIsReferenced(sourceInfo, true);

              // 追加する参照列内でAs別名が重複する場合、重複しないAs別名にリネームする
              var renamedQueryResultInfo =
                  this.RenameCollisionResultInfoAndNode(complementedResultInfoList, countResultInfo);

              explicitInfoList.Add(renamedQueryResultInfo);
              complementedResultInfoList.Add(renamedQueryResultInfo);

              ++columnIndex;
            } // if
          } // if
        } // for
      } // foreach

      // SELECT句に補完列を追加する
      this.ComplementPrimaryKeyNodeToQuerySub(query, explicitInfoList, complementedResultInfoList);
    }

    private void ComplementPrimaryKeyNodeToQuerySub(SingleQueryClause query
                                                  , ResultInfoList resultInfoList
                                                  , ResultInfoList complementedResultInfoList) {
      foreach(var complementResultInfo in complementedResultInfoList) {
        var complementQueryResultInfo = (AbstractSingleQueryResultInfo)complementResultInfo;

        // 既存列と追加する参照列のAs別名が重複する場合、重複しないAs別名にリネームする
        // (リネームしないとresultInfoList.FindResultInfo()で重複判定となってしまう)
        // (既存の参照列同士でAs別名が重複していてもAs別名は変更しない)
        var renamedComplementResultInfo =
            this.RenameCollisionResultInfoAndNode(resultInfoList, complementQueryResultInfo);

        // Select句に主キー項目の参照列を追加する
        var node = (ResultExpr)complementQueryResultInfo.Node;
        query.Accept(new AddResultVisitor(node));
      } // foreach
    }

    private void ComplementPrimaryKeyNodeToQuery(CompoundQueryClause compoundQueryClause
                                                , ResultInfoList leftResultInfoList
                                                , ResultInfoList rightResultInfoList) {
      if(compoundQueryClause.Operator == CompoundType.UnionAll) {
        // UNION ALLの左右のSELECT句の補完列について、これに対応するNULL項目を追加する
        this.ComplementPrimaryKeyNodeToQuerySub(compoundQueryClause.Left
                                              , compoundQueryClause.Right
                                              , leftResultInfoList
                                              , rightResultInfoList);
      } else if(compoundQueryClause.Operator == CompoundType.Union) {
        // UNION ALLの結果をGROUP BYする。補完列はMAX()で集約する
        //throw new NotImplementedException("sorry, now union all only");

      } else if(compoundQueryClause.Operator == CompoundType.Intersect) {
        // SELECT ... WHERE EXISTSに変換してから、補完列を追加する
        throw new NotImplementedException("sorry, now union all only");

      } else if(compoundQueryClause.Operator == CompoundType.Except ||
                compoundQueryClause.Operator == CompoundType.Minus) {
        // SELECT ... WHERE NOT EXISTSに変換してから、左SELECT文の補完列を追加する
        throw new NotImplementedException("sorry, now union all only");

      } else {
        throw new InvalidEnumArgumentException("Undefined CompoundType is used");
      }
    }

    private void ComplementPrimaryKeyNodeToQuerySub(IQueryClause leftClause
                                                  , IQueryClause rightClause
                                                  , ResultInfoList leftResultInfoList
                                                  , ResultInfoList rightResultInfoList) {
      //
      //●処理概要
      //  SELECT句の補完や列名変更の操作はResultInfoListに対して行う
      //  全ての操作を終えた後に、QueryClause.ResultsをResultInfoListに同期させる
      //

      // 明示されているSELECT句のリスト
      var allExplicitDeclLeftResultInfoList = new ResultInfoList();
      var allExplicitDeclRightResultInfoList = new ResultInfoList();
      
      // 補完されたSELECT句のリスト
      var complementedLeftResultInfoList = new ResultInfoList();
      var complementedRightResultInfoList = new ResultInfoList();

      // 明示されているSELECT句のイテレータ
      var leftItr = new ExplicitDeclIterator(leftResultInfoList);
      var rightItr = new ExplicitDeclIterator(rightResultInfoList);

      // 次にイテレートするSELECT句がある場合はTrue
      bool nextLeftExists;
      bool nextRightExists;

      // 新たなSELECT句(NULL列)を追加した場合はtrue
      bool newLeftInfoOccurs = false;
      bool newRightInfoOccurs = false;

      while(true){

        // 左の被演算クエリが主キー補完列の場合
        while((nextLeftExists = leftItr.MoveNext()) && leftItr.Current.IsComplemented) {
          var leftResultInfo = (AbstractQueryResultInfo)leftItr.Current;

          // UNION ALLの左の被演算クエリ内で同じ列名が使用されている場合は列名を変更する
          leftResultInfo = this.RenameCollisionResultInfo(allExplicitDeclLeftResultInfoList
                                                        , leftResultInfo);
          leftResultInfo = this.RenameCollisionResultInfo(complementedLeftResultInfoList
                                                        , leftResultInfo);

          // UNION ALLの左の被演算クエリの主キー補完列に対応するNULL列を新たに生成する
          var rightCounterNull = this.CreateNullResultInfo(leftResultInfo.ColumnAliasName);

          // 主キー補完列を、左の被演算クエリのリストに追加する
          complementedLeftResultInfoList.Add(leftResultInfo);
          // 主キー補完列に対応するNULL列を、右の被演算クエリのリストに追加する
          complementedRightResultInfoList.Add(rightCounterNull);

          newRightInfoOccurs = true;
        }

        // 右の被演算クエリが主キー補完列の場合
        while((nextRightExists = rightItr.MoveNext()) && rightItr.Current.IsComplemented) {
          var rightResultInfo = (AbstractQueryResultInfo)rightItr.Current;

          rightResultInfo = this.RenameCollisionResultInfo(allExplicitDeclRightResultInfoList
                                                         , rightResultInfo);
          rightResultInfo = this.RenameCollisionResultInfo(complementedRightResultInfoList
                                                         , rightResultInfo);

          var leftCounterNull = this.CreateNullResultInfo(rightResultInfo.ColumnAliasName);

          complementedLeftResultInfoList.Add(leftCounterNull);
          complementedRightResultInfoList.Add(rightResultInfo);

          newLeftInfoOccurs = true;
        }

        // 補完列以外の列(既存列)の場合
        if(nextLeftExists && nextRightExists) {
          // 既存列の列名は(SELECT句内で衝突していても)変更しない
          // (リテラル列等の列名が取得できないため衝突判定ができないため)
          allExplicitDeclLeftResultInfoList.Add(leftItr.Current);
          allExplicitDeclRightResultInfoList.Add(rightItr.Current);
        } else if(!nextLeftExists && !nextRightExists) {
          // 左右のサブクエリのSELECT句をを全てイテレートした場合、while(true)を抜ける
          break;
        } else {
          throw new InvalidASTStructureError(
            "集合演算の被演算子のSELECT句の数は同じでなければなりません.");
        }

      } // End while(true)


      for(var i = 0; i < allExplicitDeclLeftResultInfoList.Count; ++i) {
        var explicitDeclLeft = allExplicitDeclLeftResultInfoList[i];
        var explicitDeclRight = allExplicitDeclRightResultInfoList[i];

        // 主キー列がNULLリテラル以外の列に対応していると、主キー値を取得できないので、列を追加する
        if(explicitDeclLeft.KeyType != KeyType.None && !((AbstractQueryResultInfo)explicitDeclRight).IsNullLiteral) {
          // 追加する列と同じ列が既にあれば追加しない
          if(this.HasSameResultInfoAndNullPairIn(allExplicitDeclLeftResultInfoList
                                                , allExplicitDeclRightResultInfoList
                                                , explicitDeclLeft)) {
            continue;
          }

          // 主キー列を複製し、これを新たに追加する列とする
          var leftComplementedItem = ((AbstractQueryResultInfo)explicitDeclLeft).Clone();

          // 追加する列は補完列である
          leftComplementedItem.IsComplemented = true;

          // 可能であれば追加する列のAS別名の接頭詞に抽出元TableAliasNameをつける
          var tableAliasName = this.GetSrcTableAliasName(leftComplementedItem);
          if(!string.IsNullOrEmpty(tableAliasName)) {
            tableAliasName += "_";
          }

          // AS別名として列名またはそのAS別名をつける、COUNT(*)の場合は"RESULT_COUNT_"をつける
          var columnAliasName = leftComplementedItem.ColumnAliasName + "_";
          if(columnAliasName == "_" &&
             leftComplementedItem.Type == ResultInfoType.Count) {
            columnAliasName = "RESULT_COUNT_";
          }

          this.SetAliasName(leftComplementedItem, tableAliasName + columnAliasName);

          // UNION ALLの左の被演算クエリ内で同じ列名が使用されている場合は列名を変更する
          leftComplementedItem = this.RenameCollisionResultInfo(allExplicitDeclLeftResultInfoList
                                                              , leftComplementedItem);
          leftComplementedItem = this.RenameCollisionResultInfo(complementedLeftResultInfoList
                                                              , leftComplementedItem);

          // UNION ALLの左の被演算クエリの主キー補完列に対応するNULL列を新たに生成する
          var rightCounterNull = this.CreateNullResultInfo(leftComplementedItem.ColumnAliasName);

          // 主キー列の複製を、左の被演算クエリのリストに追加する
          complementedLeftResultInfoList.Add(leftComplementedItem);
          // 主キー列の複製に対応するNULL列を、右の被演算クエリのリストに追加する
          complementedRightResultInfoList.Add(rightCounterNull);

          newLeftInfoOccurs = true;
          newRightInfoOccurs = true;
        } // End if
      } // End for

      for(var i = 0; i < allExplicitDeclRightResultInfoList.Count; ++i) {
        var explicitDeclLeft = allExplicitDeclLeftResultInfoList[i];
        var explicitDeclRight = allExplicitDeclRightResultInfoList[i];

        if(explicitDeclRight.KeyType != KeyType.None && !((AbstractQueryResultInfo)explicitDeclLeft).IsNullLiteral) {

          if(this.HasSameNullAndResultInfoPairIn(allExplicitDeclLeftResultInfoList
                                                , allExplicitDeclRightResultInfoList
                                                , explicitDeclRight)){
            continue;
          }
                                         
          var rightComplementedItem = ((AbstractQueryResultInfo)explicitDeclRight).Clone();

          rightComplementedItem.IsComplemented = true;

          var tableAliasName = this.GetSrcTableAliasName(rightComplementedItem);
          if(!string.IsNullOrEmpty(tableAliasName)) {
            tableAliasName += "_";
          }

          var columnAliasName = rightComplementedItem.ColumnAliasName + "_";
          if(columnAliasName == "_" &&
             rightComplementedItem.Type == ResultInfoType.Count) {
            columnAliasName = "RESULT_COUNT_";
          }

          this.SetAliasName(rightComplementedItem, tableAliasName + columnAliasName);

          rightComplementedItem = this.RenameCollisionResultInfo(allExplicitDeclRightResultInfoList
                                                               , rightComplementedItem);
          rightComplementedItem = this.RenameCollisionResultInfo(complementedRightResultInfoList
                                                               , rightComplementedItem);

          var leftCounterNull = this.CreateNullResultInfo(rightComplementedItem.ColumnAliasName);

          complementedLeftResultInfoList.Add(leftCounterNull);
          complementedRightResultInfoList.Add(rightComplementedItem);

          newLeftInfoOccurs = true;
          newRightInfoOccurs = true;
        } // End if
      } // End for


      // 被演算クエリの補完列を既存列の後に追加する
      allExplicitDeclLeftResultInfoList.AddRange(complementedLeftResultInfoList.Items);
      allExplicitDeclRightResultInfoList.AddRange(complementedRightResultInfoList.Items);


      if(newLeftInfoOccurs){
        // 列を新たに追加した場合、被演算クエリのWildcardを展開する
        this.ExpandWildcard(leftClause, allExplicitDeclLeftResultInfoList);

        // ResultInfoの追加、またはAS別名の変更/追加が発生した場合、
        // QueryClauseのResultsとResultInfoListが正しく対応するようResultsを修正する
        for(var i = 0; i < allExplicitDeclLeftResultInfoList.Count; ++i) {
          allExplicitDeclLeftResultInfoList[i] =
            this.InsertResultInfoToCorrespondingResults(
                  (AbstractQueryResultInfo)allExplicitDeclLeftResultInfoList[i]
                , leftClause
                , i);
        }
      }
      if(newRightInfoOccurs){
        this.ExpandWildcard(rightClause, allExplicitDeclRightResultInfoList);

        for(var i = 0; i < allExplicitDeclRightResultInfoList.Count; ++i) {
          allExplicitDeclRightResultInfoList[i] =
            this.InsertResultInfoToCorrespondingResults(
                  (AbstractQueryResultInfo)allExplicitDeclRightResultInfoList[i]
                , rightClause
                , i);
        }
      }

      // ResultInfoListを一旦クリアして格納し直す
      leftResultInfoList.Clear();
      leftResultInfoList.AddRange(allExplicitDeclLeftResultInfoList.Items);
      rightResultInfoList.Clear();
      rightResultInfoList.AddRange(allExplicitDeclRightResultInfoList.Items);
      
    }

    private bool HasSameResultInfoAndNullPairIn(ResultInfoList lResultInfoList, ResultInfoList rResultInfoList,
                                                IResultInfo lResultInfo) {
      for(var i = 0; i < lResultInfoList.Count; ++i) {
        if(rResultInfoList[i].IsNullLiteral && this.IsSameResultInfo(lResultInfoList[i], lResultInfo)) {
          return true;
        }
      }
      return false;
    }

    private bool HasSameNullAndResultInfoPairIn(ResultInfoList lResultInfoList, ResultInfoList rResultInfoList,
                                                IResultInfo rResultInfo) {
      for(var i = 0; i < rResultInfoList.Count; ++i) {
        if(lResultInfoList[i].IsNullLiteral && this.IsSameResultInfo(rResultInfoList[i], rResultInfo)) {
          return true;
        }
      }
      return false;
    }

    // 同じ抽出元を持つResultInfo同士であればTrueを返す
    private bool IsSameResultInfo(IResultInfo lResultInfo, IResultInfo rResultInfo) {
      if(lResultInfo == null || rResultInfo == null) {
        return false;
      }
      if(lResultInfo.Type != rResultInfo.Type) {
        return false;
      }

      if(lResultInfo.Type == ResultInfoType.Table) {
        // TableResultInfoは呼び出し側がGetresultInfoListVisitorのコンストラクタの引数で指定するので
        // 同値性は各プロパティが完全一致するか否かで判定する
        return (TableResultInfo)lResultInfo == (TableResultInfo)rResultInfo;
      }else if(lResultInfo.Type == ResultInfoType.Query || lResultInfo.Type == ResultInfoType.Count) {
        var lQueryResultInfo = (AbstractSingleQueryResultInfo)lResultInfo;
        var rQueryResultInfo = (AbstractSingleQueryResultInfo)rResultInfo;
        if(lQueryResultInfo.SourceInfo == null && rQueryResultInfo.SourceInfo == null) {

          // NodeプロパティのNodeオブジェクトが同じか否かを判定したいが、
          // IsSameNodeVisitorを作成するまで、暫定的にこの実装とする
          //return lQueryResultInfo.IsNullLiteral && rQueryResultInfo.IsNullLiteral;
          return true;
        } else {
          return this.IsSameResultInfo(lQueryResultInfo.SourceInfo, rQueryResultInfo.SourceInfo);
        }
      } else if(lResultInfo.Type == ResultInfoType.Compound) {
        var lCompoundResultInfo = (CompoundQueryResultInfo)lResultInfo;
        var rCompoundResultInfo = (CompoundQueryResultInfo)rResultInfo;
        return this.IsSameResultInfo(lCompoundResultInfo.LeftResultInfo, rCompoundResultInfo.LeftResultInfo)
            && this.IsSameResultInfo(lCompoundResultInfo.RightResultInfo, rCompoundResultInfo.RightResultInfo);
      } else {
        return false;
      }
    }

    // resultInfoに対応するqueryClauseのResultノードがない場合、resultInfo.NodeをResultsに追加する
    private AbstractQueryResultInfo InsertResultInfoToCorrespondingResults(AbstractQueryResultInfo resultInfo
                                                                          , IQueryClause queryClause 
                                                                          , int resultIndex) {
      // Queryを囲む括弧は無視する
      if(queryClause.Type == QueryType.Bracketed) {
        return this.InsertResultInfoToCorrespondingResults(resultInfo
                                                        , ((BracketedQueryClause)queryClause).Operand
                                                        , resultIndex);
      }

      if(resultInfo.Type == ResultInfoType.Query || resultInfo.Type == ResultInfoType.Count) {
        var queryResultInfo = (AbstractSingleQueryResultInfo)resultInfo;
        var queryResultInfo_Node = queryResultInfo.Node;

        if(queryClause.Type == QueryType.Single){
          if(queryResultInfo_Node == null) {
            throw new ApplicationException("Expand wildcard before inserting");
          }

          // queryResultInfoとこれに対応するResultColumnについて、これらのAliasNameが異なる場合には、
          // queryResultInfoのAliasNameに合わせる
          if(!string.IsNullOrEmpty(queryResultInfo.ColumnAliasName) &&
            ((ResultExpr)queryResultInfo_Node).GetAliasOrColumnName() != queryResultInfo.ColumnAliasName) {
            if(queryResultInfo_Node.IsTableWildcard) {
              throw new ApplicationException("Expand wildcard before rename");
            }
            ((ResultExpr)queryResultInfo_Node).AliasName = queryResultInfo.ColumnAliasName;
            ((ResultExpr)queryResultInfo_Node).HasAs = true;
          }

          // singleQeryClause.Resultsの所定位置(resultIndex)に、
          // queryResultInfoに対応するResultColumnがない場合、ResultColumnを挿入する
          var singleQeryClause = (SingleQueryClause)queryClause;
          if(singleQeryClause.Results.Count <= resultIndex ||
             !Object.ReferenceEquals(queryResultInfo_Node, singleQeryClause.Results[resultIndex])) {
            // queryResultInfo.NodeをsingleQeryClause.Resultsに挿入する
            return this.InsertResultInfoToResults(singleQeryClause, resultIndex, queryResultInfo);
          } else {
            return resultInfo;
          }

        } else if(queryClause.Type == QueryType.Compound) {
          // ComplementPrimaryKeyNodeToQuerySub()において、NULLリテラルを追加する対象が
          // CompoundQuery型オブジェクトであっても、一旦SingleQuery型用の静解析木ノード(QueryResultInfo)
          // を対応させているため、ここでCompoundQuery型用の静解析木ノードに置き換える
          var compoundQueryClause = (CompoundQueryClause)queryClause;

          var leftResultInfo = resultInfo;
          var rightResultInfo = resultInfo.Clone();

          this.InsertResultInfoToCorrespondingResults(leftResultInfo, compoundQueryClause.Left, resultIndex);
          this.InsertResultInfoToCorrespondingResults(rightResultInfo, compoundQueryClause.Right, resultIndex);

          // 暫定的にUNION ALLにのみ対応する
          return this.CreateResultInfo(leftResultInfo, rightResultInfo, CompoundType.UnionAll);

        } else if(queryClause.Type == QueryType.Bracketed) {
          // ここの分岐が実行されることはない
          var bracketedQueryClause = (BracketedQueryClause)queryClause;
          return this.InsertResultInfoToCorrespondingResults(resultInfo
                                                            , bracketedQueryClause.Operand
                                                            , resultIndex);

        } else {
          throw new InvalidEnumArgumentException("Undefined QueryClauseType is used");
        }
        
      } else if(resultInfo.Type == ResultInfoType.Compound) {
        var compoundResultInfo = (CompoundQueryResultInfo)resultInfo;

        if(queryClause.Type != QueryType.Compound) {
          throw new ApplicationException("ResultInfoに対応しないQueryClauseの型が指定されました");
        }

        this.InsertResultInfoToCorrespondingResults(compoundResultInfo.LeftResultInfo
                                                  , ((CompoundQueryClause)queryClause).Left
                                                  , resultIndex);
        this.InsertResultInfoToCorrespondingResults(compoundResultInfo.RightResultInfo
                                                  , ((CompoundQueryClause)queryClause).Right
                                                  , resultIndex);
        return compoundResultInfo;
      } else if(resultInfo.Type == ResultInfoType.Table){
        throw new ApplicationException("TableSource Type is never used");
      } else {
        throw new InvalidEnumArgumentException("Undefined ResultInfoType is used");
      }
      
    }

    private AbstractQueryResultInfo InsertResultInfoToResults(IQueryClause queryClause
                                                            , int resultIndex
                                                            , AbstractQueryResultInfo resultInfo) {
      if(queryClause.Type == QueryType.Single) {
        var singleQueryClause = (SingleQueryClause)queryClause;
        var resultColumn = ((AbstractSingleQueryResultInfo)resultInfo).Node;
        singleQueryClause.Accept(new InsertResultVisitor(resultIndex, resultColumn));
        return resultInfo;
      } else if(queryClause.Type == QueryType.Compound) {
        // 挿入先のSELECT文が集合演算の場合は、その被演算子のSELECT文に挿入する
        var compoundQueryClause = (CompoundQueryClause)queryClause;
        var leftResultInfo = this.InsertResultInfoToResults(compoundQueryClause.Left
                                                          , resultIndex
                                                          , resultInfo);
        // resultInfo.Clone()によってresultInfo.Nodeもコピーされる
        var rightResultInfo = this.InsertResultInfoToResults(compoundQueryClause.Right
                                                            , resultIndex
                                                            , (AbstractQueryResultInfo)resultInfo.Clone());
        return new CompoundQueryResultInfo( null
                                          , resultInfo.ColumnAliasName
                                          , resultInfo.IsNullable
                                          //, resultInfo.IsUnique
                                          , resultInfo.KeyType
                                          , resultInfo.IsComplemented
                                          , null
                                          , null
                                          , leftResultInfo
                                          , rightResultInfo);
      } else if(queryClause.Type == QueryType.Bracketed) {
        return this.InsertResultInfoToResults(((BracketedQueryClause)queryClause).Operand
                                            , resultIndex
                                            , resultInfo);
      } else {
        throw new InvalidEnumArgumentException("Undefined QueryType is used");
      }
    }

    private void RenameResultAndResultInfo(AbstractQueryResultInfo resultInfo
                                         , string aliasName) {
      if(resultInfo.Type == ResultInfoType.Query || resultInfo.Type == ResultInfoType.Count) {
        var queryResultInfo = (AbstractSingleQueryResultInfo)resultInfo;
        var resultColumn = queryResultInfo.Node;

        if(resultColumn == null || resultColumn.IsTableWildcard) {
          throw new ApplicationException("Expand wildcard before rename");
        }
        // SQL構文木のAS別名を変更する
        var resultExpr = (ResultExpr)resultColumn;
        resultExpr.HasAs = true;
        resultExpr.AliasName = aliasName;
        // 静解析木の列名を変更する
        queryResultInfo.ColumnAliasName = aliasName;
      } else if(resultInfo.Type == ResultInfoType.Compound) {
        var compoundResultInfo = (CompoundQueryResultInfo)resultInfo;
        this.RenameResultAndResultInfo(compoundResultInfo.LeftResultInfo, aliasName);
        this.RenameResultAndResultInfo(compoundResultInfo.RightResultInfo, aliasName);
      } else if(resultInfo.Type == ResultInfoType.Table) {
        throw new ApplicationException("TableResultInfo type doesn't come here");
      } else {
        throw new InvalidEnumArgumentException("Undefined ResultInfoType is used");
      }
    }

    private void SetAliasName(AbstractQueryResultInfo absQueryResultInfo, string aliasName) {
      if(absQueryResultInfo.Type == ResultInfoType.Query || absQueryResultInfo.Type == ResultInfoType.Count) {
        var queryResultInfo = (AbstractSingleQueryResultInfo)absQueryResultInfo;
        queryResultInfo.ColumnAliasName = aliasName;
      } else if(absQueryResultInfo.Type == ResultInfoType.Compound) {
        var compoundResultInfo = (CompoundQueryResultInfo)absQueryResultInfo;
        this.SetAliasName(compoundResultInfo.LeftResultInfo, aliasName);
        this.SetAliasName(compoundResultInfo.RightResultInfo, aliasName);
        // 参照元の列名も変更する
        compoundResultInfo.ColumnAliasName = aliasName;
      } else if(absQueryResultInfo.Type == ResultInfoType.Table) {
        throw new ApplicationException("TableResultInfo type doesn't come here");
      } else {
        throw new InvalidEnumArgumentException("Undefined ResultInfoType is used");
      }
    }

    private AbstractQueryResultInfo CreateNullResultInfo(string aliasNameOfNull) {
      // 追加するNULL項目を作成する
      var nullResult = new ResultExpr(new NullLiteral(), true, aliasNameOfNull);

      // leftResultInfoListに追加するNULL項目のResultInfoオブジェクトを作成する
      return new QueryResultInfo( null
                                , null
                                , aliasNameOfNull
                                , true  // isNullable
                                //, false // isUnique
                                , KeyType.None // keyType
                                , true  // explicitDecl
                                , true  // isComplemented
                                , nullResult);
    }

    private string GetNoCollisionName(ResultInfoList resultInfoList
                                    , AbstractQueryResultInfo resultInfo) {
      var ret = resultInfo.ColumnAliasName;
      foreach(var item in resultInfoList) {
        if(string.Compare(item.ColumnAliasName, ret, _ignoreCase) == 0 && 
           !Object.ReferenceEquals(item, resultInfo)) {
          ret += '_';
        }
      }
      return ret;
    }

    private AbstractSingleQueryResultInfo RenameCollisionResultInfoAndNode(ResultInfoList resultInfoList
                                                                         , AbstractSingleQueryResultInfo resultInfo) {
      var renamedResultInfo = this.RenameCollisionResultInfo(resultInfoList, resultInfo);
      //if(renamedResultInfo.ColumnAliasName == resultInfo.ColumnAliasName) {
      //  return (QueryResultInfo)renamedResultInfo;
      //}
      var node = ((AbstractSingleQueryResultInfo)renamedResultInfo).Node;
      if(!node.IsTableWildcard) {
        var resultExpr = (ResultExpr)node;
        resultExpr.HasAs = true;
        resultExpr.AliasName = renamedResultInfo.ColumnAliasName;
      }
      return (AbstractSingleQueryResultInfo)renamedResultInfo;
    }

    private AbstractQueryResultInfo RenameCollisionResultInfo(ResultInfoList resultInfoList
                                                            , AbstractQueryResultInfo resultInfo) {
      // resultInfoList内にresultInfoと同じ列名がある場合は、新しい列名を取得する
      var aliasName = this.GetNoCollisionName(resultInfoList, resultInfo);
      if(string.Compare(aliasName, resultInfo.ColumnAliasName, _ignoreCase) != 0) {
        // 列名を変更する
        this.RenameResultInfo(resultInfo, aliasName);
      }
      return resultInfo;
    }

    private void RenameResultInfo(AbstractQueryResultInfo resultInfo
                                , string aliasName) {
      if(resultInfo.Type == ResultInfoType.Query || resultInfo.Type == ResultInfoType.Count ) {
        var queryResultInfo = (AbstractSingleQueryResultInfo)resultInfo;
        // 静解析木の列名を変更する
        queryResultInfo.ColumnAliasName = aliasName;
      } else if(resultInfo.Type == ResultInfoType.Compound) {
        var compoundResultInfo = (CompoundQueryResultInfo)resultInfo;
        this.RenameResultInfo(compoundResultInfo.LeftResultInfo, aliasName);
        this.RenameResultInfo(compoundResultInfo.RightResultInfo, aliasName);
        compoundResultInfo.ColumnAliasName = aliasName;
      } else if(resultInfo.Type == ResultInfoType.Table) {
        throw new ApplicationException("TableResultInfo type doesn't come here");
      } else {
        throw new InvalidEnumArgumentException("Undefined ResultInfoType is used");
      }
    }

    private void ExpandWildcard(IQueryClause queryClause, ResultInfoList resultInfoList) {
      if(queryClause.Type == QueryType.Single) {
        var singleQueryClause = (SingleQueryClause)queryClause;
        if(singleQueryClause.HasWildcard || singleQueryClause.Results.HasTableWildcard()) {
          this.ExpandWildcard(singleQueryClause, resultInfoList);
        }
      } else if(queryClause.Type == QueryType.Compound) {
        this.ExpandWildcard(((CompoundQueryClause)queryClause).Left, resultInfoList);
        this.ExpandWildcard(((CompoundQueryClause)queryClause).Right, resultInfoList);
      } else if(queryClause.Type == QueryType.Bracketed) {
        this.ExpandWildcard(((BracketedQueryClause)queryClause).Operand, resultInfoList);
      } else {
        throw new InvalidEnumArgumentException("Undefined QueryClauseType is used");
      }
    }

    private void ExpandWildcard(SingleQueryClause queryClause, ResultInfoList resultInfoList) {
      // queryClauseのSelect句をクリアする
      queryClause.Results.Clear();
      queryClause.HasWildcard = false;

      // resultInfoListからSelect句を新たに作成する
      var columnIndex = 0;
      var newResultInfoList = new ResultInfoList();
      foreach(var resultInfo in resultInfoList){
        // SELECT句で明示されていない項目は作成しない
        if(!resultInfo.ExplicitDecl) {
          continue;
        }

        var queryResultInfo = (AbstractSingleQueryResultInfo)resultInfo;
        var sourceInfo = queryResultInfo.SourceInfo;
        // 生成するQueryResultInfoのColumnName
        string newColumnName = null;

        ResultColumn expandNode;
        if(sourceInfo == null) {
          expandNode = queryResultInfo.Node.Clone();
          if(queryResultInfo.Type == ResultInfoType.Query) {
            newColumnName = ((QueryResultInfo)queryResultInfo).ColumnName;
          }
        } else {
          // GetAndSetColumnAliasName()内の処理によってquerySourceInfo.SourceInfoも変更されうるため、
          // resultInfo.ColumnAliasNameの参照より先にGetAndSetColumnAliasName()を実行する必要がある
          queryResultInfo = (AbstractSingleQueryResultInfo)this.RenameColumnAliasName(queryResultInfo, columnIndex++);
          var a = queryResultInfo.ColumnAliasName;
          // 抽出元SELECT句のViewNameを取得する
          var v = sourceInfo.TableAliasName;
          // 抽出元SELECT句のAliasNameを取得する
          var c = sourceInfo.ColumnAliasName;
          // 抽出元SELECT句を参照するColumnオブジェクトを作成する
          expandNode = new ResultExpr(new Column(v, c));

          // 1.補完列を参照する列には必ずAS別名を付ける
          // 2.SELECT句のAliasNameと、抽出元の(参照される)SELECT句の
          //   AliasNameが同じ場合は、AS別名を設定しない
          if(sourceInfo.IsComplemented || a != c) {
            ((ResultExpr)expandNode).HasAs = true;
            ((ResultExpr)expandNode).AliasName = a;
          }
          // ColumnNameはColumnオブジェクトのNameプロパティ値である
          newColumnName = c;
        }

        // queryClauseにResultExprを追加する
        queryClause.Results.Add(expandNode);

        // resultInfoのNodeプロパティを変更するため、新たにQueryResultInfoを生成する
        var newQueryResultInfo = new QueryResultInfo(queryResultInfo.TableAliasName
                                                    , newColumnName
                                                    , queryResultInfo.ColumnAliasName
                                                    , queryResultInfo.IsNullable
                                                    //, querySourceInfo.IsUnique
                                                    , queryResultInfo.KeyType
                                                    , queryResultInfo.ExplicitDecl
                                                    , queryResultInfo.IsComplemented
                                                    , queryResultInfo.SourceTable
                                                    , queryResultInfo.SourceColumnName
                                                    , expandNode // 新規作成したResultExprを設定する
                                                    , queryResultInfo.SourceInfoList
                                                    , queryResultInfo.SourceInfoListIndex);
        newResultInfoList.Add(newQueryResultInfo);
      }

      // queryClauseのResultInfoListを全て破棄し、新規作成したnewResultInfoListに入れ替える
      resultInfoList.Clear();
      resultInfoList.AddRange(newResultInfoList.Items);
    }

    private bool FindInGroupByItems(GroupBy groupBy, AbstractSingleQueryResultInfo resultInfo, int resultInfoIndex) {
      foreach(var groupByExpr in groupBy) {
        if(groupByExpr.GetType() == typeof(Column)) {
          var groupByColumn = (Column)groupByExpr;

          // SELECT句がGroupByキーか判定するため、SELECT句がColumn型であり、かつそのColumnの
          // テーブル別名と列名がGROUPBYキーと一致するかを調べる
          if(resultInfo.SourceInfo != null && resultInfo.Type == ResultInfoType.Query) {
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

    // 指定した参照名を持つTableWildcard項目を返す
    private bool FindTableWildcard(ResultInfoList resultInfoList, string tableAliasName) {
      foreach(var resultInfo in resultInfoList) {
        if(resultInfo.Type == ResultInfoType.Query || resultInfo.Type == ResultInfoType.Count) {
          var queryResultInfo = (AbstractSingleQueryResultInfo)resultInfo;
          if(queryResultInfo.Node != null && queryResultInfo.Node.IsTableWildcard) {
            if(((TableWildcard)queryResultInfo.Node).TableAliasName == tableAliasName) {
              return true;
            }
          }
        }
      }
      return false;
    }

    private string GetSrcTableAliasName(IResultInfo resultInfo) {
      if(resultInfo == null){
        return "";
      } else if(!string.IsNullOrEmpty(resultInfo.TableAliasName)) {
        return resultInfo.TableAliasName;
      } else if(resultInfo.Type == ResultInfoType.Query || resultInfo.Type == ResultInfoType.Count) {
        return this.GetSrcTableAliasName(((AbstractSingleQueryResultInfo)resultInfo).SourceInfo);
      } else if(resultInfo.Type == ResultInfoType.Compound) {
        // Select句サブクエリにCompoundQueryがある場合は、この分岐が処理されるだろう
        return "";
      } else if(resultInfo.Type == ResultInfoType.Table) {
        return ((TableResultInfo)resultInfo).TableAliasName;
      } else {
        throw new InvalidEnumArgumentException("Undefined ResultInfoType is used");
      }
    }

    private IResultInfoInternal RenameColumnAliasName(IResultInfoInternal resultInfo, int columnIndex) {
      if(resultInfo.Type == ResultInfoType.Query || resultInfo.Type == ResultInfoType.Count) {
        var queryResultInfo = (AbstractSingleQueryResultInfo)resultInfo;

        if(!string.IsNullOrEmpty(queryResultInfo.ColumnAliasName)) {
          // Column列の場合
          //return queryResultInfo.ColumnAliasName;
          return queryResultInfo;
        } else if(queryResultInfo.SourceInfo != null && 
                 (queryResultInfo.Node == null || queryResultInfo.Node.IsTableWildcard)){
          // WildcardまたはTableWildcardの場合
          var ret = this.RenameColumnAliasName(queryResultInfo.SourceInfo, columnIndex);
          // 参照元の列名も変更する
          queryResultInfo.ColumnAliasName = ret.ColumnAliasName;
          //return ret;
          return queryResultInfo;
        } else if(queryResultInfo.Node       != null &&
                 !queryResultInfo.Node.IsTableWildcard) {
          var aliasName = ((ResultExpr)queryResultInfo.Node).GetAliasOrColumnName();
          if(string.IsNullOrEmpty(aliasName)) {
            // 参照可能な列名が無い場合は、As別名を付加する
            if(!string.IsNullOrEmpty(queryResultInfo.TableAliasName)) {
              aliasName = queryResultInfo.TableAliasName + "_";
            }
            aliasName += "col" + columnIndex.ToString() + "_";
            ((ResultExpr)queryResultInfo.Node).AliasName = aliasName;
            ((ResultExpr)queryResultInfo.Node).HasAs = true;
            queryResultInfo.ColumnAliasName = aliasName;
          } else {
            throw new Exception("このパス通るときqueryResultInfo.ColumnAliasNameに値入っている?");
          }
          //return aliasName;
          return queryResultInfo;

        } else {
          // 以下の論理式の場合、抽出元ResultInfoがないのにWildcardまたはTableWildcardを
          // Select句に持つ場合の条件となるが、そのような条件を満たすSELECT文は無いだろう
          // 
          // (queryResultInfo.SourceInfo == null  &&
          //  queryResultInfo.Node       == null)
          // ||
          // (queryResultInfo.SourceInfo == null  &&
          //  queryResultInfo.Node       != null  &&
          //  queryResultInfo.Node.IsTableWildcard)
          // 
          throw new ApplicationException("This 'if branch' will not processed.");
        }

      } else if(resultInfo.Type == ResultInfoType.Compound) {
        var compoundResultInfo = (CompoundQueryResultInfo)resultInfo;
        // Compound Queryの列名は左のSELECT文から取得する
        var ret = this.RenameColumnAliasName(compoundResultInfo.LeftResultInfo, columnIndex);
        // 参照元の列名も変更する
        compoundResultInfo.ColumnAliasName = ret.ColumnAliasName;
        //return ret;
        return compoundResultInfo;
      } else if(resultInfo.Type == ResultInfoType.Table) {
        // return ((TableResultInfo)resultInfo).ColumnAliasName;
        return resultInfo;
      } else {
        throw new InvalidEnumArgumentException("Undefined ResultInfoType is used");
      }
    }

    //private bool HasTablewildCard(ResultColumns results) {
    //  foreach(var result in results) {
    //    if(result.IsTableWildcard) {
    //      return true;
    //    }
    //  }
    //  return false;
    //}
      
    private bool GetIsReferenced(IResultInfo resultInfo){
      if(resultInfo.Type == ResultInfoType.Query) {
        return ((QueryResultInfo)resultInfo).IsReferenced;
      } else if(resultInfo.Type == ResultInfoType.Count) {
        return ((CountQueryResultInfo)resultInfo).IsReferenced;
      } else if(resultInfo.Type == ResultInfoType.Table) {
        return ((TableResultInfo)resultInfo).IsReferenced;
      } else if(resultInfo.Type == ResultInfoType.Compound) {
        return ((CompoundQueryResultInfo)resultInfo).IsReferenced;
      } else {
        throw new InvalidEnumArgumentException("Undefined ResultInfoType is used");
      }
    }

    private void SetIsReferenced(IResultInfo resultInfo, bool isReferenced){
      if(resultInfo.Type == ResultInfoType.Query) {
        ((QueryResultInfo)resultInfo).IsReferenced = isReferenced;
      } else if(resultInfo.Type == ResultInfoType.Count) {
        ((CountQueryResultInfo)resultInfo).IsReferenced = isReferenced;
      } else if(resultInfo.Type == ResultInfoType.Table) {
        ((TableResultInfo)resultInfo).IsReferenced = isReferenced;
      } else if(resultInfo.Type == ResultInfoType.Compound) {
        ((CompoundQueryResultInfo)resultInfo).IsReferenced = isReferenced;
      } else {
        throw new InvalidEnumArgumentException("Undefined ResultInfoType is used");
      }
    }

    private bool GetIsOuterJoined(IResultInfo resultInfo) {
      if(resultInfo.Type == ResultInfoType.Query) {
        return ((QueryResultInfo)resultInfo).IsOuterJoined;
      } else if(resultInfo.Type == ResultInfoType.Count) {
        return ((CountQueryResultInfo)resultInfo).IsOuterJoined;
      } else if(resultInfo.Type == ResultInfoType.Table) {
        return ((TableResultInfo)resultInfo).IsOuterJoined;
      } else if(resultInfo.Type == ResultInfoType.Compound) {
        return ((CompoundQueryResultInfo)resultInfo).IsOuterJoined;
      } else {
        throw new InvalidEnumArgumentException("Undefined ResultInfoType is used");
      }
    }

    private void SetIsOuterJoined(ResultInfoLists resultInfoLists, bool isOuterJoined) {
      foreach(var resultInfolist in resultInfoLists) {
        this.SetIsOuterJoined(resultInfolist, isOuterJoined);
      }
    }

    private void SetIsOuterJoined(ResultInfoList resultInfoList, bool isOuterJoined) {
      foreach(var resultInfo in resultInfoList) {
        this.SetIsOuterJoined(resultInfo, isOuterJoined);
      }
    }

    private void SetIsOuterJoined(IResultInfo resultInfo, bool isOuterJoined) {
      if(resultInfo.Type == ResultInfoType.Query) {
        ((QueryResultInfo)resultInfo).IsOuterJoined = isOuterJoined;
      } else if(resultInfo.Type == ResultInfoType.Count) {
        ((CountQueryResultInfo)resultInfo).IsOuterJoined = isOuterJoined;
      } else if(resultInfo.Type == ResultInfoType.Table) {
        ((TableResultInfo)resultInfo).IsOuterJoined = isOuterJoined;
      } else if(resultInfo.Type == ResultInfoType.Compound) {
        ((CompoundQueryResultInfo)resultInfo).IsOuterJoined = isOuterJoined;
      } else {
        throw new InvalidEnumArgumentException("Undefined ResultInfoType is used");
      }
    }

  }
}
