using System;

namespace SqlAccessor
{
  [Serializable()]
  //Inherits IExp
  internal class SqlLiteral: IElement
  {
    private readonly string _sqlLiteral;
    public SqlLiteral(string sqlLiteral) {
      _sqlLiteral = sqlLiteral;
    }
    public object Clone() {
      return new SqlLiteral(_sqlLiteral);
    }
    public string ToString(int orNestLevel = 0) {
      if(_sqlLiteral == null) {
        return "NULL";
      }
      return _sqlLiteral;
    }
  }
}
