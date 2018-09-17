using System.Reflection;
using System.Collections.Generic;
using System;

namespace SqlAccessor
{
  [Serializable()]
  internal class NotExp: IExp
  {
    private readonly IExp _operand;
    public NotExp(IExp operand) {
      _operand = operand;
    }
    public override object Clone() {
      return new NotExp((IExp)_operand.Clone());
    }
    public override string ToString(int orNestLevel = 0) {
      if(_operand == null) {
        return "";
      }
      return "NOT (" + _operand.ToString(orNestLevel) + ")";
    }
    internal override List<Tuple<string, string>> ToPropertyValuePairs() {
      return new List<Tuple<string, string>>();
    }
    internal override IExp RemoveEqualExp(EqualExp match) {
      return this;
    }
    internal override IExp RemoveExp(PropertyInfo[] matchProperties) {
      IExp exp = _operand.RemoveExp(matchProperties);
      if(exp == null) {
        return null;
      } else {
        return new NotExp(exp);
      }
    }
    internal override IExp RemoveExpIfNull(ICaster caster) {
      IExp exp = _operand.RemoveExpIfNull(caster);
      if(exp == null) {
        return null;
      } else {
        return new NotExp(exp);
      }
    }
    internal override void CastToSqlLiteralType(ICaster caster
                                              , IViewInfoGetter viewInfoGetter) {
      if(_operand != null) {
        _operand.CastToSqlLiteralType(caster, viewInfoGetter);
      }
    }
  }
}
