using System.Collections.Generic;
using System.ComponentModel;

namespace MiniSqlParser
{
  /// <summary>
  /// 全てのPlaceHolder ExprにNULLを設定する
  /// 全てのPlaceHolder Predicateに0=1を設定する
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class ReplaceAllPlaceholdersByNull: ReplacePlaceHolders
  {
    public ReplaceAllPlaceholdersByNull() : base(new Dictionary<string, string>()) { 
    }

    protected override Expr Place(PlaceHolderExpr ph) {
      // NULL値は全てのPlaceHolderExprに適用できる
      return new NullLiteral();
    }

    protected override Predicate Place(PlaceHolderPredicate ph) {
      // 0=1は全てのPlaceHolderPredicateに適用できる
      // FALSE値に置き換えることでSELECT文の高速化を期待できる
      return new BinaryOpPredicate(new UNumericLiteral("0")
                                 , PredicateOperator.Equal
                                 , new UNumericLiteral("1"));
    }
  }
}
