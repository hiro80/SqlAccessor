using System.Reflection;
using System.Collections.Generic;
using System;

namespace SqlAccessor
{
  [Serializable()]
  internal class XOrExp: IExp
  {
    private readonly IExp _lOperand;
    private readonly IExp _rOperand;
    public XOrExp(IExp lOperand, IExp rOperand) {
      _lOperand = lOperand;
      _rOperand = rOperand;
    }
    public override object Clone() {
      return new XOrExp((IExp)_lOperand.Clone(), (IExp)_rOperand.Clone());
    }
    public override string ToString(int orNestLevel = 0) {
      //被演算子の両方、又はどちらか片方がnullの場合
      if(_lOperand == null && _rOperand == null) {
        return "";
      } else if(_lOperand == null) {
        return _rOperand.ToString(0);
      } else if(_rOperand == null) {
        return _lOperand.ToString(0);
      }

      string retStr = "";

      if(!(_lOperand is OrExp)) {
        retStr = "(";
      }
      retStr += new AndExp(new OrExp(_lOperand, _rOperand), 
                          !new AndExp(_lOperand, _rOperand)).ToString();
      if(!(_rOperand is OrExp)) {
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
        return new XOrExp(lExp, rExp);
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
        return new XOrExp(lExp, rExp);
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
        return new XOrExp(lExp, rExp);
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
