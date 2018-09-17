using System;

namespace SqlAccessor
{
  [Serializable()]
  //Inherits IExp
  internal class Literal: IElement
  {
    private readonly object _literal;
    public Literal(object literal) {
      _literal = literal;
    }
    public object Value {
      get { return _literal; }
    }
    public object Clone() {
      return new Literal((object)_literal);
    }
    public string ToString(int orNestLevel = 0) {
      if(_literal == null) {
        return "NULL";
      }
      return _literal.ToString();
    }
  }
}
