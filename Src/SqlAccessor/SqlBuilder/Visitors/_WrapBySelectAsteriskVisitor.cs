using System.Collections.Generic;
using System.ComponentModel;
using MiniSqlParser;

namespace MiniSqlParser
{
  /// <summary>
  /// SELECT文を"SELECT * FROM"でラッピングする
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class WrapBySelectAsteriskVisitor: Visitor
  {
    // ResultsInfo木を解析して、ORDERBY句を新たなメインクエリに移動する


    // Visitorで処理をせずSqlBuilderのCmd内で処理をすることにする
  }
}
