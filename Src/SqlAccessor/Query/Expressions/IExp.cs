using System;
using System.Reflection;
using System.Collections.Generic;

namespace SqlAccessor
{
  [Serializable()]
  public abstract class IExp: ICloneable
  {

    public static IExp operator &(IExp lOperand, IExp rOperand) {
      return new AndExp(lOperand, rOperand);
    }

    public static IExp operator &(bool lOperand, IExp rOperand) {
      if(lOperand) {
        return rOperand;
      } else {
        return new NotEqualExp(new Literal(1), new Literal(1));
      }
    }

    public static IExp operator &(IExp lOperand, bool rOperand) {
      if(rOperand) {
        return lOperand;
      } else {
        return new NotEqualExp(new Literal(1), new Literal(1));
      }
    }

    public static IExp operator |(IExp lOperand, IExp rOperand) {
      return new OrExp(lOperand, rOperand);
    }

    public static IExp operator |(bool lOperand, IExp rOperand) {
      if(lOperand) {
        return new EqualExp(new Literal(1), new Literal(1));
      } else {
        return rOperand;
      }
    }

    public static IExp operator |(IExp lOperand, bool rOperand) {
      if(rOperand) {
        return new EqualExp(new Literal(1), new Literal(1));
      } else {
        return lOperand;
      }
    }

    public static IExp operator ^(IExp lOperand, IExp rOperand) {
      return new XOrExp(lOperand, rOperand);
    }

    public static IExp operator ^(bool lOperand, IExp rOperand) {
      if(lOperand) {
        return new XOrExp(new EqualExp(new Literal(1), new Literal(1)), rOperand);
      } else {
        return new XOrExp(new NotEqualExp(new Literal(1), new Literal(1)), rOperand);
      }
    }

    public static IExp operator ^(IExp lOperand, bool rOperand) {
      if(rOperand) {
        return new XOrExp(new EqualExp(new Literal(1), new Literal(1)), lOperand);
      } else {
        return new XOrExp(new NotEqualExp(new Literal(1), new Literal(1)), lOperand);
      }
    }

    public static IExp operator !(IExp operand) {
      return new NotExp(operand);
    }

    internal SqlLiteral CreateSqlLiteral(ICaster caster
                                       , IViewInfoGetter viewInfoGetter
                                       , val val
                                       , object literal) {
      string propertyName = val.PropertyName;
      ViewColumnInfo viewColumnInfo = viewInfoGetter.GetViewColumnInfo(propertyName);
      if(viewColumnInfo == null) {
        throw new NotExistsViewColumnException(
          propertyName + "はレコード" + viewInfoGetter.GetViewInfo().Name + 
          "に存在しない、" + "またはSELECT句と紐付いていません");
      }
      string sqlLiteral = caster.CastToSqlLiteralType(literal, viewColumnInfo);
      return new SqlLiteral(sqlLiteral);
    }

    internal bool Contains(PropertyInfo[] matchProperties, string propertyName) {
      foreach(PropertyInfo aPropertyInfo in matchProperties) {
        if(aPropertyInfo.Name == propertyName) {
          return true;
        }
      }
      return false;
    }

    public abstract string ToString(int orNestLevel = 0);

    internal abstract List<Tuple<string, string>> ToPropertyValuePairs();

    public abstract object Clone();

    internal abstract IExp RemoveEqualExp(EqualExp match);

    internal abstract IExp RemoveExp(PropertyInfo[] matchProperties);

    internal abstract IExp RemoveExpIfNull(ICaster caster);

    internal abstract void CastToSqlLiteralType(ICaster caster
                                              , IViewInfoGetter viewInfoGetter);
  }
}
