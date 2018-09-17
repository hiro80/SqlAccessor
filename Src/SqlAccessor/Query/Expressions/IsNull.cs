using System.Reflection;
using System.Collections.Generic;
using System;

namespace SqlAccessor
{
  [Serializable()]
  internal class IsNull: IExp
  {
    private val _operand;
    public IsNull(val operand) {
      _operand = operand;
    }
    public override object Clone() {
      return new IsNull((val)_operand.Clone());
    }
    public override string ToString(int orNestLevel = 0) {
      return _operand.ToString() + " IS NULL ";
    }
    internal override List<Tuple<string, string>> ToPropertyValuePairs() {
      return new List<Tuple<string, string>>();
    }
    internal override IExp RemoveEqualExp(EqualExp match) {
      return this;
    }
    internal override IExp RemoveExp(PropertyInfo[] matchProperties) {
      if(!this.Contains(matchProperties, _operand.PropertyName)) {
        return null;
      }

      return this;
    }
    internal override IExp RemoveExpIfNull(ICaster caster) {
      return this;
    }
    internal override void CastToSqlLiteralType(ICaster caster
                                              , IViewInfoGetter viewInfoGetter) {
      return;
    }
  }
}
