using System.Reflection;
using System.Collections.Generic;
using System;

namespace SqlAccessor
{
  [Serializable()]
  internal class LikeExp: IExp
  {
    private IElement _lOperand;
    private IElement _rOperand;
    public LikeExp(IElement lOperand, IElement rOperand) {
      _lOperand = lOperand;
      _rOperand = rOperand;
    }
    public override object Clone() {
      return new LikeExp((IElement)_lOperand.Clone(), (IElement)_rOperand.Clone());
    }
    public override string ToString(int orNestLevel = 0) {
      return _lOperand.ToString() + " LIKE " + _rOperand.ToString();
    }
    internal override List<Tuple<string, string>> ToPropertyValuePairs() {
      return new List<Tuple<string, string>>();
    }
    internal override IExp RemoveEqualExp(EqualExp match) {
      return this;
    }
    internal override IExp RemoveExp(PropertyInfo[] matchProperties) {
      if(_lOperand is val && !this.Contains(matchProperties, ((val)_lOperand).PropertyName)) {
        return null;
      }

      if(_rOperand is val && !this.Contains(matchProperties, ((val)_rOperand).PropertyName)) {
        return null;
      }

      return this;
    }
    internal override IExp RemoveExpIfNull(ICaster caster) {
      if(_rOperand == null || _lOperand == null) {
        return null;
      }

      if(_rOperand is Literal) {
        if(caster.IsNullPropertyValue(((Literal)_rOperand).Value)) {
          return null;
        }
      }

      if(_lOperand is Literal) {
        if(caster.IsNullPropertyValue(((Literal)_lOperand).Value)) {
          return null;
        }
      }

      return this;
    }
    internal override void CastToSqlLiteralType(ICaster caster
                                              , IViewInfoGetter viewInfoGetter) {
      if(_lOperand is val && _rOperand is Literal) {
        _rOperand = CreateSqlLiteral(caster
                                   , viewInfoGetter
                                   , (val)_lOperand
                                   , ((Literal)_rOperand).Value);
      } else if(_lOperand is Literal && _rOperand is val) {
        _lOperand = CreateSqlLiteral(caster
                                   , viewInfoGetter
                                   , (val)_rOperand
                                   , ((Literal)_lOperand).Value);
      } else if(_lOperand is val && _rOperand is val) {
        return;
      }
    }
  }
}
