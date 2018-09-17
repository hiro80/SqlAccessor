using System.Collections.Generic;
using System.ComponentModel;
using MiniSqlParser;

namespace MiniSqlParser
{
  /// <summary>
  /// 主SQL文のWHERE句を文字列として取得する
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class GetWherePredicateVisitor: Visitor
  {
    private Predicate _predicate;

    public Predicate GetWhere {
      get {
        return _predicate;
      }
    }

    public override void VisitOnWhere(SingleQueryClause query, int offset) {
      if(!query.IsSubQuery) {
        _predicate = query.Where;
      }
    }

    public override void VisitOnWhere(UpdateStmt updateStmt, int offset) {
      _predicate = updateStmt.Where;
    }

    public override void VisitOnWhere(DeleteStmt deleteStmt, int offset) {
      _predicate = deleteStmt.Where;
    }
  }
}
