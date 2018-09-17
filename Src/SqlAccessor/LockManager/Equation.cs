namespace SqlAccessor
{
  /// <summary>
  /// 単純な一致条件を表す
  /// </summary>
  /// <remarks>スレッドセーフ</remarks>
  [System.Diagnostics.DebuggerDisplay("{_variable} = {_value}")]
  [System.Serializable()]
  internal class Equation
  {
    //変数名
    private readonly string _variable;
    //定数値
    private readonly string _value;

    public Equation(string variable, string value) {
      _variable = variable.ToUpper();
      _value = value;
    }

    public string Variable {
      get { return _variable; }
    }

    public string Value {
      get { return _value; }
    }

    public override string ToString() {
      return _variable + " = " + _value;
    }
  }
}

