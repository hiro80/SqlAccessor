using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace SqlAccessor
{
  /// <summary>
  /// 乗法標準形を保持する
  /// </summary>
  /// <remarks></remarks>
  [Serializable()]
  internal class CNFofEquation: Reader<Equation>
  {
    private readonly List<Equation> _equations = new List<Equation>();
    private int _currentIndex = -1;

    public CNFofEquation() {
    }

    public void Dispose() {
    }

    //一致条件の乗法標準形文字列を、Equationオブジェクトのリストに変換する
    public CNFofEquation(string cnfStr) {
      if(cnfStr == null) {
        return;
      }

      //ANDキーワードで文字列を分割する
      foreach(string equationStr in new StringExtension(cnfStr).Split(" AND ", "'")) {
        //=キーワードで文字列を、変数名と値の2つに分割する
        string[] varAndValue = new StringExtension(equationStr).Split(" = ", "'");
        string variable = varAndValue[0];
        string value = varAndValue[1];
        //Equationオブジェクトのリストに追加する
        this.Add(new Equation(variable, value));
      }
    }

    private static int CompareByVarName(Equation x, Equation y) {
      return x.Variable.CompareTo(y.Variable);
    }

    public void Sort() {
      _equations.Sort(CompareByVarName);
    }

    public void Add(Equation eq) {
      _equations.Add(eq);
    }

    //保持している一致条件項の数
    public int Count {
      get { return _equations.Count; }
    }

    public IEnumerator<Equation> GetEnumerator() {
      return _equations.GetEnumerator();
    }

    public IEnumerator GetEnumerator1() {
      return this.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator1();
    }

    public bool MoveNext() {
      if(_currentIndex >= _equations.Count - 1) {
        return false;
      } else {
        _currentIndex += 1;
        return true;
      }
    }

    public void Reset() {
      _currentIndex = -1;
    }

    public Equation Current {
      get { return _equations[_currentIndex]; }
    }

    public object Current1 {
      get { return this.Current; }
    }
    object IEnumerator.Current {
      get { return Current1; }
    }

    public override string ToString() {
      return this.ToString(int.MaxValue);
    }

    public string ToString(int maxLength) {
      if(_equations.Count == 0) {
        return "";
      }

      StringBuilder ret = new StringBuilder();

      //最初の素論理式は"AND"を付加しない
      ret.Append(_equations[0].ToString());

      for(int i = 1; i <= _equations.Count - 1; i++) {
        //乗法標準形文字列の文字数がmaxLengthを超える場合は、途中までの結果を返す
        string equationStr = " AND " + _equations[i].ToString();
        if(ret.Length + equationStr.Length > maxLength) {
          break;
        }

        ret.Append(equationStr);
      }

      return ret.ToString();
    }

    public bool ContainsListCollection {
      get { return false; }
    }

    public System.Collections.IList GetList() {
      throw new NotSupportedException("CNFofEquationはGetList()を実装していません");
    }
  }
}
