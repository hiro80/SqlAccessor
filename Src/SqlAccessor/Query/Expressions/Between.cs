using System.Reflection;
using System.Collections.Generic;
using System;

namespace SqlAccessor
{
  [Serializable()]
  internal class Between: IExp
  {
    private readonly val _operand;
    private IElement _fromOperand;
    private IElement _toOperand;
    public Between(val operand, IElement fromOperand, IElement toOperand) {
      _operand = operand;
      _fromOperand = fromOperand;
      _toOperand = toOperand;
    }
    public override object Clone() {
      return new Between((val)_operand.Clone()
                       , (IElement)_fromOperand.Clone()
                       , (IElement)_toOperand.Clone());
    }
    public override string ToString(int orNestLevel = 0) {
      return _operand.ToString() + " BETWEEN " + 
             _fromOperand.ToString() + " AND " + _toOperand.ToString();
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

      if(_fromOperand is val && 
         !this.Contains(matchProperties, ((val)_fromOperand).PropertyName)) {
        return null;
      }

      if(_toOperand is val && 
         !this.Contains(matchProperties, ((val)_toOperand).PropertyName)) {
        return null;
      }

      return this;
    }
    internal override IExp RemoveExpIfNull(ICaster caster) {
      if(_fromOperand == null && _toOperand == null) {
        return null;
      } else if(_fromOperand == null) {
        return new LessThanOrEqual(_operand, _toOperand);
      } else if(_toOperand == null) {
        return new GreaterThanOrEqual(_operand, _fromOperand);
      } else if(_fromOperand is Literal && _toOperand is Literal) {
        if(caster.IsNullPropertyValue(((Literal)_fromOperand).Value) && 
           caster.IsNullPropertyValue(((Literal)_toOperand).Value)) {
          return null;
        } else if(caster.IsNullPropertyValue(((Literal)_fromOperand).Value)) {
          return new LessThanOrEqual(_operand, _toOperand);
        } else if(caster.IsNullPropertyValue(((Literal)_toOperand).Value)) {
          return new GreaterThanOrEqual(_operand, _fromOperand);
        }
      } else if(_fromOperand is Literal && _toOperand is val) {
        if(caster.IsNullPropertyValue(((Literal)_fromOperand).Value)) {
          return new LessThanOrEqual(_operand, _toOperand);
        }
      } else if(_fromOperand is val && _toOperand is Literal) {
        if(caster.IsNullPropertyValue(((Literal)_toOperand).Value)) {
          return new GreaterThanOrEqual(_operand, _fromOperand);
        }
      }

      return this;
    }
    internal override void CastToSqlLiteralType(ICaster caster
      , IViewInfoGetter viewInfoGetter) {
      if(_fromOperand is Literal) {
        _fromOperand = CreateSqlLiteral(caster
                                      , viewInfoGetter
                                      , _operand
                                      , ((Literal)_fromOperand).Value);
      }
      if(_toOperand is Literal) {
        _toOperand = CreateSqlLiteral(caster
                                    , viewInfoGetter
                                    , _operand
                                    , ((Literal)_toOperand).Value);
      }
    }
  }
}
