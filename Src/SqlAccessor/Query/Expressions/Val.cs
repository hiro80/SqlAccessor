using System;
using System.Collections.Generic;

namespace SqlAccessor
{
  /// <summary>
  /// Queryオブジェクトに格納する抽出条件においてレコードのプロパティを表す
  /// </summary>
  /// <remarks></remarks>
  /// 
  [Serializable()]
  //Inherits IExp
  public class val: IElement
  {
    private readonly string _propertyName;
    private val(string propertyName) {
      _propertyName = propertyName;
    }

    public object Clone() {
      return new val(_propertyName);
    }

    public static val of(string propertyName) {
      return new val(propertyName);
    }

    public string PropertyName {
      get { return _propertyName; }
    }

    public string ToString(int orNestLevel = 0) {
      //Column名が予約語と同じ、又はハイフンを含む場合はエスケープする
      //
      //暫定版コード!
      //
      if(string.Equals(_propertyName, "Password", System.StringComparison.CurrentCultureIgnoreCase) ||
         string.Equals(_propertyName, "Key", System.StringComparison.CurrentCultureIgnoreCase)) {
        return "\"" + _propertyName + "\"";
      } else if(_propertyName.Contains("-")) {
        return "\"" + _propertyName + "\"";
      } else {
        return _propertyName;
      }
    }

    #region "Equal Operators Definition"
    public static IExp operator ==(val lOperand, val rOperand) {
      return new EqualExp(lOperand, rOperand);
    }

    public static IExp operator ==(val lOperand, object rOperand) {
      return new EqualExp(lOperand, new Literal(rOperand));
    }

    public static IExp operator ==(object lOperand, val rOperand) {
      return new EqualExp(new Literal(lOperand), rOperand);
    }
    #endregion

    #region "NotEqual Operators Definition"
    public static IExp operator !=(val lOperand, val rOperand) {
      return new NotEqualExp(lOperand, rOperand);
    }

    public static IExp operator !=(val lOperand, object rOperand) {
      return new NotEqualExp(lOperand, new Literal(rOperand));
    }

    public static IExp operator !=(object lOperand, val rOperand) {
      return new NotEqualExp(new Literal(lOperand), rOperand);
    }
    #endregion

    #region "LessThan Operators Definition"
    public static IExp operator <(val lOperand, val rOperand) {
      return new LessThan(lOperand, rOperand);
    }

    public static IExp operator <(val lOperand, object rOperand) {
      return new LessThan(lOperand, new Literal(rOperand));
    }

    public static IExp operator <(object lOperand, val rOperand) {
      return new LessThan(new Literal(lOperand), rOperand);
    }
    #endregion

    #region "LessThanOrEqual Operators Definition"
    public static IExp operator <=(val lOperand, val rOperand) {
      return new LessThanOrEqual(lOperand, rOperand);
    }

    public static IExp operator <=(val lOperand, object rOperand) {
      return new LessThanOrEqual(lOperand, new Literal(rOperand));
    }

    public static IExp operator <=(object lOperand, val rOperand) {
      return new LessThanOrEqual(new Literal(lOperand), rOperand);
    }
    #endregion

    #region "GreaterThan Operators Definition"
    public static IExp operator >(val lOperand, val rOperand) {
      return new GreaterThan(lOperand, rOperand);
    }

    public static IExp operator >(val lOperand, object rOperand) {
      return new GreaterThan(lOperand, new Literal(rOperand));
    }

    public static IExp operator >(object lOperand, val rOperand) {
      return new GreaterThan(new Literal(lOperand), rOperand);
    }
    #endregion

    #region "GreaterThanOrEqual Operators Definition"
    public static IExp operator >=(val lOperand, val rOperand) {
      return new GreaterThanOrEqual(lOperand, rOperand);
    }

    public static IExp operator >=(val lOperand, object rOperand) {
      return new GreaterThanOrEqual(lOperand, new Literal(rOperand));
    }

    public static IExp operator >=(object lOperand, val rOperand) {
      return new GreaterThanOrEqual(new Literal(lOperand), rOperand);
    }
    #endregion

    #region "LIKE Operators Definition"
    public IExp Like(string rOperand) {
      return new LikeExp(this, new Literal(rOperand));
    }

    public IExp Like(val rOperand) {
      return new LikeExp(new Literal(this), rOperand);
    }
    #endregion

    #region "IN Operators Definition"
    public IExp In(val operand, params val[] operands) {
      IElement[] literals = new IElement[operands.Length + 1];
      literals[0] = operand;
      for(int i = 1; i <= operands.Length; i++) {
        literals[i] = operands[i - 1];
      }
      return new InExp(this, literals);
    }

    public IExp In(IEnumerable<val> operands) {
      List<IElement> literals = new List<IElement>();
      foreach(val operand in operands) {
        literals.Add(operand);
      }
      return new InExp(this, literals.ToArray());
    }

    public IExp In(object operand, params object[] operands) {
      IElement[] literals = new IElement[operands.Length + 1];
      if(operand is val) {
        literals[0] = (val)operand;
      } else {
        literals[0] = new Literal(operand);
      }
      for(int i = 1; i <= operands.Length; i++) {
        if(operands[i - 1] is val) {
          literals[i] = (val)operands[i - 1];
        } else {
          literals[i] = new Literal(operands[i - 1]);
        }
      }
      return new InExp(this, literals);
    }

    public IExp In(System.Collections.IEnumerable operands) {
      List<IElement> literals = new List<IElement>();
      foreach(object operand in operands) {
        if(operand is val) {
          literals.Add((val)operand);
        } else {
          literals.Add(new Literal(operand));
        }
      }
      return new InExp(this, literals.ToArray());
    }
    #endregion

    #region "BETWEEN Operators Definition"
    public IExp Between(val fromOperand, val toOperand) {
      return new Between(this, fromOperand, toOperand);
    }

    public IExp Between(val fromOperand, object toOperand) {
      return new Between(this, fromOperand, new Literal(toOperand));
    }

    public IExp Between(object fromOperand, val toOperand) {
      return new Between(this, new Literal(fromOperand), toOperand);
    }

    public IExp Between(object fromOperand, object toOperand) {
      return new Between(this, new Literal(fromOperand), new Literal(toOperand));
    }
    #endregion

    #region "IS NULL Operator Definition"
    public IExp IsNull {
      get { return new IsNull(this); }
    }
    #endregion
  }
}
