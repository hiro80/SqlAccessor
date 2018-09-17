using System.Collections.Generic;
using System.ComponentModel;
using MiniSqlParser;

namespace MiniSqlParser
{
  /// <summary>
  /// SELECT文の抽出結果の行数を取得するためのSELECT文に書き換える
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class ConvertToCountStmtVisitor: Visitor
  {
    //・以下の場合、SELECT COUNT(*) FROM で囲む
    //  メインクエリがUNION等の複合クエリ
    //  メインクエリがGROUPBY句を持つ
    //  メインクエリがDISTINCTを持つ
    //  メインクエリがLIMIT句を持つ

    //・以下の場合、SELECT文全体をSELECT 0 に置き換える
    //  メインクエリがTOP 0を持つ場合

    //・以下の場合、メインのSELECT句をCOUNT(*)に置き換える
    // メインクエリが集約関数を持ち、GROUPBYやDISTINCTを持たない場合
    // メインクエリがTOP n (n>0)を持つ場合
    // 上記の何れの条件にも該当しない場合

    // (*)BracketedQueryの場合は括弧で囲む必要なし
    // (*)メインクエリのORDERBY句は削除する

    // Visitorで処理をせずSqlBuilderのCmd内で処理をすることにする
    //   GetMainQueryVisitorでメインクエリを取得し、
    //   これに対して操作を行う

  }
}
