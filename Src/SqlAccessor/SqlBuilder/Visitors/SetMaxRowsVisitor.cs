using System.ComponentModel;

namespace MiniSqlParser
{
  /// <summary>
  /// SELECT文の抽出件数を設定する
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class SetMaxRowsVisitor: Visitor
  {
    private readonly int _maxRows;
    public SetMaxRowsVisitor(int maxRows) {
      _maxRows = maxRows;
    }
    public override void VisitBefore(SingleQueryClause query) {
      query.HasTop = true;
      query.Top = _maxRows;
    }
  }
}
