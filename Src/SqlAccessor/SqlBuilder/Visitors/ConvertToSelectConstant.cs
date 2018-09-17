using System.ComponentModel;
using MiniSqlParser;

namespace MiniSqlParser
{
  /// <summary>
  /// メインクエリのSELECT句を定数1に置き換える
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class ConvertToSelectConstant: Visitor
  {
    //・以下の場合、*に置き換えることでSELECT文の結果が変わる可能性がある
    //  メインクエリのDISINTCT句がある場合
    //  メインクエリの集約関数がある場合
    //  メインクエリのGROUPBY句を持つ場合

    public override void VisitBefore(SingleQueryClause query) {
      if(!query.IsSubQuery) {
        ReplaceWithConstant(query);
      }
    }

    public override void VisitBefore(BracketedQueryClause bracketedQuery) {
      if(!bracketedQuery.IsSubQuery) {
        this.ReplaceWithConstant(bracketedQuery);
      }
    }

    public override void VisitBefore(CompoundQueryClause compoundQuery) {
      if(!compoundQuery.IsSubQuery) {
        ReplaceWithConstant(compoundQuery);
      }
    }

    private void ReplaceWithConstant(IQueryClause query) {
      if(query.Type == QueryType.Single) {
        // SELECT句を定数1に置き換える
        var queryClause = ((SingleQueryClause)query);
        var results = queryClause.Results;
        queryClause.HasWildcard = false;
        results.Clear();
        results.Add(new ResultExpr(new UNumericLiteral("1")));
      } else if(query.Type == QueryType.Bracketed) {
        this.ReplaceWithConstant(((BracketedQueryClause)query).Operand);
      } else if(query.Type == QueryType.Compound) {
        this.ReplaceWithConstant(((CompoundQueryClause)query).Left);
        this.ReplaceWithConstant(((CompoundQueryClause)query).Right);
      } else {
        throw new InvalidEnumArgumentException("Undefined QueryType is used");
      }
    }
  }
}
