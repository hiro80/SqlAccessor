using System.Collections.Generic;
using System.ComponentModel;

namespace MiniSqlParser
{
  /// <summary>
  /// 全てのPlaceHolder ExprにNULLを設定する
  /// 全てのPlaceHolder Predicateに0=1を設定する
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class ReplaceAllPlaceholdersToNull: ReplacePlaceHolders
  {
    private bool _isSqlitePragmaStmt;

    public ReplaceAllPlaceholdersToNull() : base(new Dictionary<string, string>()) { 
    }

    protected override Expr Place(PlaceHolderExpr ph) {
      if(_isSqlitePragmaStmt) {
        // SQLiteのPRAGMA文のテーブル名プレースホルダはNULLに置き換えない
        return ph;
      } else {
        // NULL値は全てのPlaceHolderExprに適用できる
        return new NullLiteral();
      }
    }

    protected override IValue PlaceValue(PlaceHolderExpr ph) {
      // Value句にもNULL値を適用する
      return new NullLiteral();
    }

    protected override Predicate Place(PlaceHolderPredicate ph) {
      // 0=1は全てのPlaceHolderPredicateに適用できる
      // FALSE値に置き換えることでSELECT文の高速化を期待できる
      return new BinaryOpPredicate(new UNumericLiteral("0")
                                 , PredicateOperator.Equal
                                 , new UNumericLiteral("1"));
    }

    public override void VisitBefore(SqlitePragmaStmt pragmaStmt) {
      if(pragmaStmt.HasPlaceHolder) {
        _isSqlitePragmaStmt = true;
      }
    }

    public override void VisitAfter(SqlitePragmaStmt pragmaStmt) {
      _isSqlitePragmaStmt = false;
    }
  }
}
