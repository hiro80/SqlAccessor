using System;
using System.Reflection;
using System.Collections.Generic;

namespace SqlAccessor
{
  [Serializable()]
  internal class AndExp: IExp
  {
    private readonly IExp _lOperand;
    private readonly IExp _rOperand;
    public AndExp(IExp lOperand, IExp rOperand) {
      _lOperand = lOperand;
      _rOperand = rOperand;
    }
    public override object Clone() {
      return new AndExp((IExp)_lOperand.Clone(), (IExp)_rOperand.Clone());
    }
    public override string ToString(int orNestLevel = 0) {
      //被演算子の両方、又はどちらか片方がNothingの場合
      if(_lOperand == null && _rOperand == null) {
        return "";
      } else if(_lOperand == null) {
        return _rOperand.ToString(0);
      } else if(_rOperand == null) {
        return _lOperand.ToString(0);
      }

      string retStr = "";
      retStr += _lOperand.ToString(0) + " AND " + _rOperand.ToString(0);
      return retStr;
    }
    internal override List<Tuple<string, string>> ToPropertyValuePairs() {
      List<Tuple<string, string>> ret = new List<Tuple<string, string>>();

      if(_lOperand != null) {
        ret.AddRange(_lOperand.ToPropertyValuePairs());
      }

      if(_rOperand != null) {
        ret.AddRange(_rOperand.ToPropertyValuePairs());
      }

      return ret;
    }
    internal override IExp RemoveEqualExp(EqualExp match) {
      IExp lExp = _lOperand.RemoveEqualExp(match);
      IExp rExp = _rOperand.RemoveEqualExp(match);

      if(lExp == null && rExp == null) {
        return null;
      } else if(lExp == null) {
        return rExp;
      } else if(rExp == null) {
        return lExp;
      } else {
        return new AndExp(lExp, rExp);
      }
    }
    internal override IExp RemoveExp(PropertyInfo[] matchProperties) {
      IExp lExp = _lOperand.RemoveExp(matchProperties);
      IExp rExp = _rOperand.RemoveExp(matchProperties);

      if(lExp == null && rExp == null) {
        return null;
      } else if(lExp == null) {
        return rExp;
      } else if(rExp == null) {
        return lExp;
      } else {
        return new AndExp(lExp, rExp);
      }
    }
    internal override IExp RemoveExpIfNull(ICaster caster) {
      IExp lExp = _lOperand.RemoveExpIfNull(caster);
      IExp rExp = _rOperand.RemoveExpIfNull(caster);

      if(lExp == null && rExp == null) {
        return null;
      } else if(lExp == null) {
        return rExp;
      } else if(rExp == null) {
        return lExp;
      } else {
        return new AndExp(lExp, rExp);
      }
    }
    internal override void CastToSqlLiteralType(ICaster caster
                                              , IViewInfoGetter viewInfoGetter) {
      if(_lOperand != null) {
        _lOperand.CastToSqlLiteralType(caster, viewInfoGetter);
      }
      if(_rOperand != null) {
        _rOperand.CastToSqlLiteralType(caster, viewInfoGetter);
      }
    }
  }
}
