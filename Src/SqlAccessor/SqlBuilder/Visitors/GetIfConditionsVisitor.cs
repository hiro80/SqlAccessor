using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using MiniSqlParser;

namespace MiniSqlParser
{
  /// <summary>
  /// IF文の条件式とSQL文を取得する
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class GetIfConditionsVisitor: Visitor
  {
    private readonly List<Predicate> _conditions;
    private readonly List<Stmts> _stmtsList;

    private IfStmt _topIfStmt;
    private readonly Stack<Predicate> _conditionStack;

    public GetIfConditionsVisitor() {
      _conditions = new List<Predicate>();
      _stmtsList = new List<Stmts>();
      _conditionStack = new Stack<Predicate>();
    }

    public ReadOnlyCollection<Predicate> Conditions {
      get { return _conditions.AsReadOnly(); }
    }

    public ReadOnlyCollection<Stmts> StmtsList {
      get { return _stmtsList.AsReadOnly(); }
    }

    public int Count {
      get {
        return _conditions.Count;
      }
    }

    public override void VisitBefore(IfStmt ifStmt) {
      // 最上位階層のIF文のみを解析対象にする
      if(_topIfStmt == null) {
        _topIfStmt = ifStmt;
      } else if(_topIfStmt != ifStmt) {
        return;
      }
      _conditionStack.Push(ifStmt.Conditions[0]);
    }

    public override void VisitOnElsIf(IfStmt ifStmt, int ifThenIndex, int offset) {
      if(_topIfStmt != ifStmt) {
        return;
      }

      // ELSE(ELSIF)において、スタックの1番上の式を否定し、これをスタックに戻す
      var lastCondition = _conditionStack.Pop();
      var lastNotCondition = new NotPredicate(this.EncloseIfNecessary(lastCondition));
      _conditionStack.Push(lastNotCondition);

      // IFにおいて条件式をスタックにPUSHする
      _conditionStack.Push(this.EncloseIfNecessary(ifStmt.Conditions[ifThenIndex]));
    }

    public override void VisitOnThen(IfStmt ifStmt, int ifThenIndex, int offset) {
      if(_topIfStmt != ifStmt) {
        return;
      }

      // 条件式とSQL文をリストに格納する
      _conditions.Add(this.ConcatConditions(_conditionStack));
      _stmtsList.Add(this.RemoveNullStmt(ifStmt.StatementsList[ifThenIndex]));
    }

    public override void VisitOnElse(IfStmt ifStmt, int offset) {
      if(_topIfStmt != ifStmt) {
        return;
      }

      // ELSE(ELSIF)において、スタックの1番上の式を否定し、これをスタックに戻す
      var lastCondition = _conditionStack.Pop();
      var lastNotCondition = new NotPredicate(this.EncloseIfNecessary(lastCondition));
      _conditionStack.Push(lastNotCondition);

      // 条件式とSQL文をリストに格納する
      _conditions.Add(this.ConcatConditions(_conditionStack));
      _stmtsList.Add(this.RemoveNullStmt(ifStmt.ElseStatements));
    }

    public override void VisitAfter(IfStmt ifStmt) {
      if(_topIfStmt != ifStmt) {
        return;
      }

      // IF部分の条件式をPOPする
      _conditionStack.Pop();
      // ELSIF部分の条件式をPOPする
      for(int i = 0; i < ifStmt.CountElsIfStatements; ++i) {
        _conditionStack.Pop();
      }
    }

    private Predicate EncloseIfNecessary(Predicate predicate) {
      if(predicate.GetType() == typeof(AndPredicate) ||
         predicate.GetType() == typeof(OrPredicate) ||
         predicate.GetType() == typeof(CollatePredicate)) {
        return new BracketedPredicate(predicate);
      } else {
        return predicate;
      }
    }

    // 空SQL文は構文木ではNullStmtオブジェクトが割り当てられるが、
    // IF文の結果には含めないよう削除する
    private Stmts RemoveNullStmt(Stmts stmts) {
      for(int i = stmts.Count - 1; i >= 0; --i) {
        if(stmts[i].Type == StmtType.Null) {
          stmts.RemoveAt(i);
        }
      }
      return stmts;
    }

    private Predicate ConcatConditions(Stack<Predicate> conditionStack) {
      var e = conditionStack.GetEnumerator();
      if(!e.MoveNext()) {
        return null;
      }
      Predicate ret = e.Current;
      while(e.MoveNext()) {
        ret = new AndPredicate(e.Current, ret);
      }
      return ret;
    }
  }
}
