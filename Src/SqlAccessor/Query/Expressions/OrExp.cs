using System.Reflection;
using System.Collections.Generic;
using System;

namespace SqlAccessor
{
  [Serializable()]
  internal class OrExp: IExp
  {
    private readonly IExp _lOperand;
    private readonly IExp _rOperand;
    public OrExp(IExp lOperand, IExp rOperand) {
      _lOperand = lOperand;
      _rOperand = rOperand;
    }
    public override object Clone() {
      return new OrExp((IExp)_lOperand.Clone(), (IExp)_rOperand.Clone());
    }
    public override string ToString(int orNestLevel = 0) {
      //被演算子の両方、又はどちらか片方がNothingの場合
      if(_lOperand == null && _rOperand == null) {
        return "";
      } else if(_lOperand == null) {
        return _rOperand.ToString(orNestLevel + 1);
      } else if(_rOperand == null) {
        return _lOperand.ToString(orNestLevel + 1);
      }

      string retStr = "";
      if(orNestLevel == 0) {
        retStr = "(";
      }
      retStr += _lOperand.ToString(orNestLevel + 1) + " OR " +
                _rOperand.ToString(orNestLevel + 1);
      if(orNestLevel == 0) {
        retStr += ")";
      }
      return retStr;
    }
    internal override List<Tuple<string, string>> ToPropertyValuePairs() {
      return new List<Tuple<string, string>>();
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
        return new OrExp(lExp, rExp);
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
        return new OrExp(lExp, rExp);
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
        return new OrExp(lExp, rExp);
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
