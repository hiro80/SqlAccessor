using System.Reflection;
using System.Collections.Generic;
using System;

namespace SqlAccessor
{
  [Serializable()]
  internal class InExp: IExp
  {
    private readonly val _lOperand;
    private IElement[] _operands;
    public InExp(val lOperand, params IElement[] operands) {
      _lOperand = lOperand;
      _operands = operands;
    }
    public override object Clone() {
      return new InExp((val)_lOperand.Clone(), (IElement[])_operands.Clone());
    }
    public override string ToString(int orNestLevel = 0) {
      string retStr = null;
      retStr = _lOperand.ToString() + " IN (";
      for(int i = 0; i <= _operands.Length - 1; i++) {
        retStr += _operands[i].ToString();
        if(i != _operands.Length - 1) {
          retStr += ", ";
        }
      }
      retStr += ")";
      return retStr;
    }
    internal override List<Tuple<string, string>> ToPropertyValuePairs() {
      return new List<Tuple<string, string>>();
    }
    internal override IExp RemoveEqualExp(EqualExp match) {
      return this;
    }
    internal override IExp RemoveExp(PropertyInfo[] matchProperties) {
      if(!this.Contains(matchProperties, _lOperand.PropertyName)) {
        return null;
      }

      List<IElement> newOperands = new List<IElement>();
      for(int i = 0; i <= _operands.Length - 1; i++) {
        if(_operands[i] is val && 
           !this.Contains(matchProperties, ((val)_operands[i]).PropertyName)) {
          continue;
        }
        newOperands.Add(_operands[i]);
      }

      if(newOperands.Count == 0) {
        return null;
      }

      return new InExp(_lOperand, newOperands.ToArray());
    }
    internal override IExp RemoveExpIfNull(ICaster caster) {
      List<IElement> newOperands = new List<IElement>();

      for(int i = 0; i <= _operands.Length - 1; i++) {
        if(_operands[i] == null) {
          continue;
        } else if(_operands[i] is Literal && 
                  caster.IsNullPropertyValue(((Literal)_operands[i]).Value)) {
          continue;
        }
        newOperands.Add(_operands[i]);
      }

      if(newOperands.Count == 0) {
        return null;
      }

      return new InExp(_lOperand, newOperands.ToArray());
    }
    internal override void CastToSqlLiteralType(ICaster caster
                                              , IViewInfoGetter viewInfoGetter) {
      for(int i = 0; i <= _operands.Length - 1; i++) {
        if(_operands[i] is Literal) {
          _operands[i] = CreateSqlLiteral(caster
                                        , viewInfoGetter
                                        , _lOperand
                                        , ((Literal)_operands[i]).Value);
        }
      }
    }
  }
}
