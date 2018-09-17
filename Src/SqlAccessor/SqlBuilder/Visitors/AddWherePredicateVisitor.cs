using System.ComponentModel;
using MiniSqlParser;

namespace MiniSqlParser
{
  /// <summary>
  /// SQL文のwhere句にAND条件を追加する
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class AddWherePredicateVisitor: Visitor
  {
    private readonly Predicate _predicate;
    private int _queryNestLevel = 0;

    public AddWherePredicateVisitor(string predicateStr
                                  , DBMSType dbmsType = DBMSType.Unknown
                                  , bool forSqlAccessor = false) {
      if(string.IsNullOrEmpty(predicateStr)) {
        _predicate = null;
      } else {
        _predicate = MiniSqlParserAST.CreatePredicate(predicateStr, dbmsType, forSqlAccessor);
      }
    }

    public AddWherePredicateVisitor(Predicate predicate) {
      _predicate = predicate;
    }

    public override void VisitBefore(SingleQueryClause query) {
      ++_queryNestLevel;
    }

    // 若干の速度改善のため、
    // 追加したWhere句へのAccept()を軽減するため、VisitAfterでWhere句の操作を行う
    public override void VisitAfter(SingleQueryClause query) {
      // if(!query.IsSubQuery) {
      // サブクエリへの追加にも対応するためIsSubQueryは用いない
      if(_queryNestLevel == 1) {
        query.Where = this.AddPredicate(query.Where);
      }
      --_queryNestLevel;
    }

    public override void VisitAfter(BracketedQueryClause query) {
      if(_queryNestLevel == 1 && query.Operand.GetType() == typeof(SingleQueryClause)) {
        var singleQueryClause = (SingleQueryClause)query.Operand;
        singleQueryClause.Where = this.AddPredicate(singleQueryClause.Where);
      }
    }

    public override void VisitBefore(CompoundQueryClause compoundQuery) {
      ++_queryNestLevel;
    }

    public override void VisitAfter(CompoundQueryClause compoundQuery) {
      --_queryNestLevel;
    }

    public override void VisitBefore(UpdateStmt updateStmt) {
      ++_queryNestLevel;
    }

    public override void VisitAfter(UpdateStmt updateStmt) {
      updateStmt.Where = this.AddPredicate(updateStmt.Where);
      --_queryNestLevel;
    }

    public override void VisitBefore(DeleteStmt deleteStmt) {
      ++_queryNestLevel;
    }

    public override void VisitAfter(DeleteStmt deleteStmt) {
      deleteStmt.Where = this.AddPredicate(deleteStmt.Where);
      --_queryNestLevel;
    }

    private Predicate AddPredicate(Predicate where) {
      //if(_predicate == null || _predicate.GetType() == typeof(NullPredicate)) {
      if(_predicate == null) {
        // 空文字、又はNothingであれば構文木に何もしない
        return where;
      }

      if(where == null) {
        where = _predicate;
      } else {
        Predicate leftPredicate;
        //if(AddWherePredicateVisitor.IsAndOnlyPredicate(where)) {
        //  // AND連言の場合
        //  leftPredicate = where;
        //} else {
        //  // AND連言でない場合はWhere句の式を括弧で囲んでAND節をつなげる
        //  leftPredicate = new BracketedPredicate(where);
        //}
        if(where.GetType() == typeof(OrPredicate)) {
          // WHERE句が、A OR Bの場合(A OR B)のように括弧で囲む
          leftPredicate = new BracketedPredicate(where);
        } else {
          // WHERE句が、A AND Bの場合またはAのように2項演算でない場合、括弧で囲まない
          leftPredicate = where;
        }

        // 追加するPredicateがAND連言でない場合は括弧で囲む
        Predicate rightPredicate;
        //if(AddWherePredicateVisitor.IsAndOnlyPredicate(_predicate)) {
        //  rightPredicate = _predicate;
        //} else {
        //  rightPredicate = new BracketedPredicate(_predicate);
        //}
        if(_predicate.GetType() == typeof(OrPredicate)) {
          // OR演算式の場合は式全体を括弧で囲んでAND節をつなげる
          rightPredicate = new BracketedPredicate(_predicate);
        } else {
          rightPredicate = _predicate;
        }

        where = new AndPredicate(leftPredicate, rightPredicate);
      }
      return where;
    }

    //internal static bool IsAndOnlyPredicate(Predicate predicate) {
    //  if(predicate.GetType() == typeof(AndPredicate) &&
    //     AddWherePredicateVisitor.IsAndOnlyPredicate(((AndPredicate)predicate).Left)) {
    //    // 引数で渡された式木の左部分木のみを葉ノードまで探索する
    //    // 探索途中でORノードが無ければその式木はAND連言である
    //    return true;
    //  } else if(predicate.GetType() == typeof(OrPredicate)) {
    //    return false;
    //  } else if(predicate.GetType() == typeof(NotPredicate)) {
    //    return AddWherePredicateVisitor.IsAndOnlyPredicate(((NotPredicate)predicate).Operand);
    //  } else if(predicate.GetType() == typeof(CollatePredicate)) {
    //    return AddWherePredicateVisitor.IsAndOnlyPredicate(((CollatePredicate)predicate).Operand);
    //  } else {
    //    return true;
    //  }
    //}

  }

}
