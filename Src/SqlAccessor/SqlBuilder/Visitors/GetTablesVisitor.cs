using System.Collections.Generic;
using System.ComponentModel;
using MiniSqlParser;

namespace MiniSqlParser
{
  /// <summary>
  /// SQL文で使用されている全てのテーブルを取得する
  /// テーブル名が重複する場合はひとつに纏めて返す
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class GetTablesVisitor: Visitor
  {
    private HashSet<Table> _tables;

    public GetTablesVisitor() {
      _tables = new HashSet<Table>();
    }

    public HashSet<Table> Tables {
      get {
        return _tables;
      }
    }

    public override void Visit(Table table) {
      _tables.Add(table);
    }
  }
}
