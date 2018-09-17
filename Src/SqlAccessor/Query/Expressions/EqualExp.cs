using System.Reflection;
using System.Collections.Generic;
using System;

namespace SqlAccessor
{
  [Serializable()]
  internal class EqualExp: IExp
  {
    private IElement _lOperand;
    private IElement _rOperand;
    public EqualExp(IElement lOperand, IElement rOperand) {
      _lOperand = lOperand;
      _rOperand = rOperand;
    }
    public override object Clone() {
      return new EqualExp((IElement)_lOperand.Clone(), (IElement)_rOperand.Clone());
    }
    public override string ToString(int orNestLevel = 0) {
      return _lOperand.ToString() + " = " + _rOperand.ToString();
    }
    internal override List<Tuple<string, string>> ToPropertyValuePairs() {
      List<Tuple<string, string>> ret = new List<Tuple<string, string>>();
      string propertyName = null;
      string propertyValue = null;

      if(_lOperand is val && (_rOperand is Literal || _rOperand is SqlLiteral)) {
        propertyName = _lOperand.ToString();
        propertyValue = _rOperand.ToString();
      } else if((_lOperand is Literal || _lOperand is SqlLiteral) && _rOperand is val) {
        propertyName = _rOperand.ToString();
        propertyValue = _lOperand.ToString();
      } else {
        return ret;
      }
      ret.Add(new Tuple<string, string>(propertyName, propertyValue));
      return ret;
    }
    internal override IExp RemoveEqualExp(EqualExp match) {
      if(match == null) {
        return this;
      }

      if(match._lOperand.ToString() == _lOperand.ToString() &&
         match._rOperand.ToString() == _rOperand.ToString()) {
        return null;
      } else {
        return this;
      }
    }
    internal override IExp RemoveExp(PropertyInfo[] matchProperties) {
      if(_lOperand is val && 
         !this.Contains(matchProperties, ((val)_lOperand).PropertyName)) {
        return null;
      }

      if(_rOperand is val && 
         !this.Contains(matchProperties, ((val)_rOperand).PropertyName)) {
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
