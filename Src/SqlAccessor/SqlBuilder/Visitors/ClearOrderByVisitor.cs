using System.ComponentModel;
using MiniSqlParser;

namespace MiniSqlParser
{
  /// <summary>
  /// ORDER BY句を削除する
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class ClearOrderByVisitor: Visitor
  {
    public override void VisitBefore(SingleQueryClause query) {
      if(!query.IsSubQuery && query.GetType() == typeof(SingleQuery)) {
        ((SingleQuery)query).OrderBy.Clear();
      }
    }

    public override void VisitBefore(BracketedQueryClause bracketedQuery) {
      if(!bracketedQuery.IsSubQuery && bracketedQuery.GetType() == typeof(BracketedQuery)) {
        ((BracketedQuery)bracketedQuery).OrderBy.Clear();
      }
    }

    public override void VisitBefore(CompoundQueryClause compoundQuery) {
      if(!compoundQuery.IsSubQuery && compoundQuery.GetType() == typeof(CompoundQuery)) {
        ((CompoundQuery)compoundQuery).OrderBy.Clear();
      }
    }
  }
}
