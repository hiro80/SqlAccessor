using System.Collections.Generic;
using System.ComponentModel;
using MiniSqlParser;

namespace MiniSqlParser
{
  /// <summary>
  /// メインクエリのSELECT句を*に置き換える
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class ConvertToSelectAsterisk: Visitor
  {
    //・以下の場合、*に置き換えることでSELECT文がエラーになる可能性がある
    //  メインクエリがUNION等の複合クエリの場合、エラーを送出する
    //  メインクエリのFROM句がない場合、エラーを送出する
    //  メインクエリのGROUPBY句を持つ場合、エラーを送出する
    
    //・以下の場合、*に置き換えることでSELECT文の結果が変わる可能性がある
    //  メインクエリのDISINTCT句がある場合
    //  メインクエリの集約関数がある場合

    // → ConvertToSelectConstantで十分に代用可能なので、
    //    ConvertToSelectAsteriskは実装しないことにした
  }
}
