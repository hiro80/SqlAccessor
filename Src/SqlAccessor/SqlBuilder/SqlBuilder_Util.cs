using System;
using System.Collections.Generic;
using System.ComponentModel;
using MiniSqlParser;

namespace MiniSqlParser
{
  public partial class SqlBuilder{

    private List<string> GetResultColumns(IQuery query) {
      if(!query.IsSubQuery) {
        return this.GetResultColumns((IQueryClause)query);
      } else {
        return new List<string>();
      }
    }

    private List<string> GetResultColumns(IQueryClause queryClause) {
      var ret = new List<string>();
      if(queryClause.Type == QueryType.Single) {
        var singleQueryClause = (SingleQueryClause)queryClause;
        foreach(var result in singleQueryClause.Results) {
          if(!result.IsTableWildcard) {
            ret.Add(((ResultExpr)result).GetAliasOrColumnName());
          }
        }
        return ret;
      } else if(queryClause.Type == QueryType.Bracketed) {
        return this.GetResultColumns(((BracketedQueryClause)queryClause).Operand);
      } else if(queryClause.Type == QueryType.Compound) {
        return this.GetResultColumns(((CompoundQueryClause)queryClause).Left);
      } else {
        throw new InvalidEnumArgumentException("Undefined QueryType is used");
      } 
    }

    private void ReplaceWithCountFunc(SelectStmt stmt) {
      if(stmt.Query.Type == QueryType.Compound) {
        // メインクエリが複合クエリの場合はSELECT COUNT(*)文でラッピングする
        // (複合クエリのOrderBy句は変換処理なしにSELECT COUNT(*)のOrderByに使える)
        var singleQueryClause = SingleQuery.WrapInSelectStar(new AliasedQuery((IQueryClause)stmt.Query, false, "COUNT_"));
        // Limit句にはColumnを指定できないので変換処理は必要ない
        var singleQuery = new SingleQuery(singleQueryClause, stmt.Query.OrderBy, stmt.Query.Limit);
        stmt.Query = singleQuery;
      }
      this.ReplaceWithCountFunc((IQueryClause)stmt.Query);
    }

    private void ReplaceWithCountFunc(IQueryClause queryClause) {
      if(queryClause.Type == QueryType.Single) {
        // SELECT句をCOUNT(*)に置き換える
        var singleQueryClause = ((SingleQueryClause)queryClause);
        var results = singleQueryClause.Results;
        singleQueryClause.HasWildcard = false;
        results.Clear();
        results.Add(new ResultExpr(new AggregateFuncExpr("COUNT", QuantifierType.None, true, null, null)));
      } else if(queryClause.Type == QueryType.Bracketed) {
        this.ReplaceWithCountFunc(((BracketedQueryClause)queryClause).Operand);
      } else if(queryClause.Type == QueryType.Compound) {
        throw new ApplicationException("Can't replace with count(*) compound query.");
      } else {
        throw new InvalidEnumArgumentException("Undefined QueryType is used");
      }
    }

    private void ReplaceWithConstant(IQuery query) {
      if(!query.IsSubQuery) {
        this.ReplaceWithConstant((IQueryClause)query);
      }
    }

    private void ReplaceWithConstant(IQueryClause queryClause) {
      if(queryClause.Type == QueryType.Single) {
        // SELECT句を定数1に置き換える
        var singleQueryClause = ((SingleQueryClause)queryClause);
        var results = singleQueryClause.Results;
        singleQueryClause.HasWildcard = false;
        results.Clear();
        results.Add(new ResultExpr(new UNumericLiteral("1")));
      } else if(queryClause.Type == QueryType.Bracketed) {
        this.ReplaceWithConstant(((BracketedQueryClause)queryClause).Operand);
      } else if(queryClause.Type == QueryType.Compound) {
        this.ReplaceWithConstant(((CompoundQueryClause)queryClause).Left);
        this.ReplaceWithConstant(((CompoundQueryClause)queryClause).Right);
      } else {
        throw new InvalidEnumArgumentException("Undefined QueryType is used");
      }
    }

    // 指定したResultInfoの抽出元のTableResultInfoを取得する
    private TableResultInfo GetSourceTableResultInfo(IResultInfo resultInfo) {
      if(resultInfo.Type == ResultInfoType.Query) {
        var queryInfo = (QueryResultInfo)resultInfo;
        if(queryInfo.SourceInfo == null) {
          return null;
        }
        return this.GetSourceTableResultInfo(queryInfo.SourceInfo);
      } else if(resultInfo.Type == ResultInfoType.Compound) {
        var compoundQueryInfo = (CompoundQueryResultInfo)resultInfo;
        // LeftとRight(RightとLeft)がNullリテラルとテーブル参照列の組合わせの場合に
        // SourceTableプロパティに値が設定される
        if(compoundQueryInfo.SourceTable == null) {
          return null;
        }
        if(compoundQueryInfo.LeftResultInfo.IsNullLiteral) {
          return this.GetSourceTableResultInfo(compoundQueryInfo.RightResultInfo);
        } else {
          return this.GetSourceTableResultInfo(compoundQueryInfo.LeftResultInfo);
        }
      } else if(resultInfo.Type == ResultInfoType.Count) {
        var countInfo = (CountQueryResultInfo)resultInfo;
        return this.GetSourceTableResultInfo(countInfo.SourceInfo);
      } else if(resultInfo.Type == ResultInfoType.Table) {
        return (TableResultInfo)resultInfo;
      } else {
        throw new InvalidEnumArgumentException("Undefined QueryType is used");
      }
    }

    private IResultInfo GetSourceInfoOfSingleQueryResultInfo(IResultInfo resultInfo) {
      if(resultInfo.Type == ResultInfoType.Query) {
        var sourceInfo = ((AbstractSingleQueryResultInfo)resultInfo).SourceInfo;
        if(sourceInfo == null){
          return null;
        }else{
          return sourceInfo;
        }
      } else if(resultInfo.Type == ResultInfoType.Compound) {
        var compoundQueryInfo = (CompoundQueryResultInfo)resultInfo;
        var leftInfo = this.GetSourceInfoOfSingleQueryResultInfo(compoundQueryInfo.LeftResultInfo);
        if(leftInfo == null) {
          return this.GetSourceInfoOfSingleQueryResultInfo(compoundQueryInfo.RightResultInfo);
        } else {
          return leftInfo;
        }
      } else if(resultInfo.Type == ResultInfoType.Count){
        throw new ApplicationException("");
      } else {
        throw new InvalidEnumArgumentException("Undefined ResultInfoType is used");
      }
    }

    private bool HasWildcard(IQueryClause query) {
      if(query.Type == QueryType.Single) {
        return ((SingleQueryClause)query).HasWildcard;
      } else if(query.Type == QueryType.Compound) {
        return this.HasWildcard(((CompoundQueryClause)query).Left);
      } else if(query.Type == QueryType.Bracketed) {
        return this.HasWildcard(((BracketedQueryClause)query).Operand);
      } else {
        throw new InvalidEnumArgumentException("Undefined QueryType is used");
      }
    }

    private bool HasTableWildcard(IQueryClause query) {
      if(query.Type == QueryType.Single) {
        return ((SingleQueryClause)query).Results.HasTableWildcard();
      } else if(query.Type == QueryType.Compound) {
        return this.HasTableWildcard(((CompoundQueryClause)query).Left);
      } else if(query.Type == QueryType.Bracketed) {
        return this.HasTableWildcard(((BracketedQueryClause)query).Operand);
      } else {
        throw new InvalidEnumArgumentException("Undefined QueryType is used");
      }
    }

  }
}
