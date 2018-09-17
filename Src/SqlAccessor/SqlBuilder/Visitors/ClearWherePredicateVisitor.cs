using System.ComponentModel;
using MiniSqlParser;

namespace MiniSqlParser
{
  /// <summary>
  /// WHERE句を削除する
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class ClearWherePredicateVisitor: Visitor
  {
    // VisitOnWhere()内でWhereプロパティをNullにすると
    // その後のWhere.Accept(visitor)でNULL例外が発生する

    public override void VisitBefore(SingleQueryClause query) {
      if(!query.IsSubQuery) {
        query.Where = null;
      }
    }

    public override void VisitBefore(UpdateStmt updateStmt) {
      updateStmt.Where = null;
    }

    public override void VisitBefore(DeleteStmt deleteStmt) {
      deleteStmt.Where = null;
    }
  }
}
