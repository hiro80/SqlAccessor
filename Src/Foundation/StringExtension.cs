using System.Collections.Generic;
using System.Text;

namespace SqlAccessor
{
  /// <summary>
  /// System.Stringで提供されていない文字列操作機能を定義する
  /// </summary>
  /// <remarks></remarks>
  public class StringExtension
  {
    private readonly string _str;

    public StringExtension(string str) {
      if(str == null) {
        throw new System.ArgumentNullException("str", "strにはNULLを指定できません");
      }
      _str = str;
    }

    public override string ToString() {
      return _str;
    }

    /// <summary>
    /// 最大文字列長以降の文字列をカットする
    /// </summary>
    /// <param name="index">カット位置</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public string SubstrBefore(int index) {
      if(_str != null && _str.Length > index) {
        return _str.Substring(0, index);
      }

      return _str;
    }

    /// <summary>
    /// 最大文字列長以前の文字列をカットする
    /// </summary>
    /// <param name="index">カット位置</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public string SubstrAfter(int index) {
      if(_str != null && _str.Length > index) {
        return _str.Substring(index);
      }

      return "";
    }

    /// <summary>
    /// 区切文字列で分割した文字列配列を返す
    /// </summary>
    /// <param name="delimiter">区切文字列</param>
    /// <param name="qualifier">エスケープ文字列</param>
    /// <param name="ignoreCase">区切文字列及びエスケープ文字列の大小文字を区別するか</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public string[] Split(string delimiter
                        , string qualifier
                        , bool ignoreCase = false) {
      bool qualifierState = false;
      int startIndex = 0;
      List<string> values = new List<string>();

      for(int charIndex = 0; charIndex <= _str.Length - 1; charIndex++) {
        if(qualifier != null && 
           qualifier.Length <= _str.Length - charIndex && 
           string.Compare(_str.Substring(charIndex, qualifier.Length), qualifier, ignoreCase) == 0) {
          qualifierState = !qualifierState;
        } else if(!qualifierState && 
                  delimiter != null && 
                  delimiter.Length <= _str.Length - charIndex && 
                  string.Compare(_str.Substring(charIndex, delimiter.Length), delimiter, ignoreCase) == 0) {
          if(charIndex - startIndex > 0) {
            values.Add(_str.Substring(startIndex, charIndex - startIndex));
          }
          startIndex = charIndex + delimiter.Length;
        }
      }

      if(startIndex <= _str.Length && _str.Length - startIndex > 0) {
        values.Add(_str.Substring(startIndex, _str.Length - startIndex));
      }

      string[] returnValues = new string[values.Count];
      values.CopyTo(returnValues);
      return returnValues;
    }

    /// <summary>
    /// 指定した位置の文字が、エスケープ文字で囲まれているか調べる
    /// </summary>
    /// <param name="index">位置</param>
    /// <param name="qualifier">エスケープ文字列</param>
    /// <returns>True: 囲まれている、False: 囲まれていない</returns>
    /// <remarks>indexがエスケープ文字部分を指している場合は囲まれているとする
    ///          エスケープ文字の終了文字列がない場合は、文字列の最後まで囲まれていると見做す</remarks>
    public bool IsInComment(int index, string qualifier) {
      List<int> indexesOfQualifier = new List<int>();
      int lengthOfQualifier = qualifier.Length;

      //エスケープ文字の位置を全て調べる
      int i = 0;
      do {
        i = _str.IndexOf(qualifier, i + lengthOfQualifier - 1);
        if(i < 0) {
          break;
        }
        indexesOfQualifier.Add(i);
      } while(true);

      //エスケープ区間      |←      →|
      //文字列          ----++++----++++----
      //    j=           0   1   2   3   4 
      //index値がJ=偶数の位置にある場合は、エスケープ文字に囲まれていない
      //          =奇数の位置にある場合は、囲まれている
      int j = 0;
      for(j = 0; j <= indexesOfQualifier.Count - 1; j++) {
        if(index < (indexesOfQualifier[j] + lengthOfQualifier * (j % 2))) {
          break;
        }
      }

      return j % 2 != 0;
    }

    /// <summary>
    /// 指定した位置の文字が、指定文字列で囲まれているか調べる
    /// </summary>
    /// <param name="index"></param>
    /// <param name="qualifierStart"></param>
    /// <param name="qualifierEnd"></param>
    /// <returns></returns>
    /// <remarks></remarks>
    public bool IsInComment(int index
                          , string qualifierStart
                          , string qualifierEnd) {
      if(index > _str.Length - 1) {
        throw new System.ArgumentOutOfRangeException("index", "indexの値が対象文字列長を超えています");
      } else if(qualifierStart == qualifierEnd) {
        throw new System.ArgumentException("qualifierEnd", "2つの指定文字列に同じ文字列は指定できません");
      }

      Stack<string> qualifierStack = new Stack<string>();

      for(int charIndex = 0; charIndex <= index; charIndex++) {
        if(qualifierStart != null && 
           qualifierStart.Length <= _str.Length - charIndex && 
           string.Compare(_str.Substring(charIndex, qualifierStart.Length), qualifierStart) == 0) {
          //コメント開始文字列
          qualifierStack.Push("START");
        } else if(qualifierEnd != null && 
                  qualifierEnd.Length <= _str.Length - charIndex && 
                  qualifierEnd.Length <= charIndex && 
                  string.Compare(_str.Substring(charIndex - qualifierEnd.Length, qualifierEnd.Length), qualifierEnd) == 0) {
          //コメント終了文字列
          if(qualifierStack.Count > 0 && qualifierStack.Peek() == "START") {
            //ENDに対するSTARTを相殺する
            qualifierStack.Pop();
          } else {
            qualifierStack.Push("END");
          }
        }
      }

      if(qualifierStack.Count == 0) {
        //コメント区間外
        return false;
      } else if(qualifierStack.Peek() == "START") {
        //コメント区間内
        return true;
      } else {
        //エラー状態(区間外と見做す)
        return false;
      }
    }

    /// <summary>
    /// 文字列の先頭文字
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
    public char HeadChar {
      get {
        if(string.IsNullOrEmpty(_str)) {
          return '\0';
        }
        return _str[0];
      }
    }

    /// <summary>
    /// 文字列の最後尾文字
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
    public char TailChar {
      get {
        if(string.IsNullOrEmpty(_str)) {
          return '\0';
        }
        return _str[_str.Length - 1];
      }
    }

    /// <summary>
    /// 文字列の先頭文字を削除する
    /// </summary>
    /// <returns></returns>
    /// <remarks></remarks>
    public string RemoveHeadChar() {
      if(string.IsNullOrEmpty(_str)) {
        return _str;
      }
      return _str.Remove(0, 1);
    }

    /// <summary>
    /// 文字列の先頭文字が指定した文字の場合に削除する
    /// </summary>
    /// <param name="c">削除文字</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public string RemoveHeadCharIf(char c) {
      if(string.IsNullOrEmpty(_str)) {
        return _str;
      }
      if(_str[0] == c) {
        return _str.Remove(0, 1);
      } else {
        return _str;
      }
    }

    /// <summary>
    /// 文字列の最後尾文字を削除する
    /// </summary>
    /// <returns></returns>
    /// <remarks></remarks>
    public string RemoveTailChar() {
      if(string.IsNullOrEmpty(_str)) {
        return _str;
      }
      return _str.Remove(_str.Length - 1, 1);
    }

    /// <summary>
    /// 文字列の最後尾文字が指定した文字の場合に削除する
    /// </summary>
    /// <param name="c">削除文字</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public string RemoveTailCharIf(char c) {
      if(string.IsNullOrEmpty(_str)) {
        return _str;
      }
      if(_str[_str.Length - 1] == c) {
        return _str.Remove(_str.Length - 1, 1);
      } else {
        return _str;
      }
    }

    /// <summary>
    /// 単語単位で文字列を置換する
    /// </summary>
    /// <param name="oldStr">検索文字列</param>
    /// <param name="newStr">置換文字列</param>
    /// <remarks></remarks>
    public string ReplaceByWord(string oldStr, string newStr) {
      StringBuilder ret = new StringBuilder(_str);
      //置換対象文字列における検索文字列の開始位置
      int idx = 0;
      //前回検索後のidxの値
      int preIdx = 0;
      //検索文字列の終了位置の次の文字
      char postChar = '\0';
      //_strに文字列置換処理を行った後のstrとの位置調整
      int diff = 0;

      //検索文字列が複数存在する場合、全て置換する
      for(int i = 0; i <= _str.Length - 1; i++) {
        //検索文字列が無ければ終了する
        idx = _str.IndexOf(oldStr, preIdx, System.StringComparison.InvariantCultureIgnoreCase);
        if(idx < 0) {
          return ret.ToString();
        }
        preIdx = idx + oldStr.Length;
        postChar = (_str.Substring(preIdx, 1).ToCharArray()[0]);
        //英数字と'_'以外の文字をを単語の区切りと見なす
        if(!(char.IsLetterOrDigit(postChar) || postChar == '_')) {
          //aStringBuilder.Replace(oldStr, newStr, idx + diff, oldStr.Length)
          ret.Remove(idx + diff, oldStr.Length);
          ret.Insert(idx + diff, newStr);
          diff += newStr.Length - oldStr.Length;
        }
      }

      return ret.ToString();
    }

    public static bool operator ==(StringExtension lOperand
                                 , StringExtension rOperand) {
      return lOperand._str == rOperand._str;
    }

    public static bool operator !=(StringExtension lOperand
                                 , StringExtension rOperand) {
      return lOperand._str != rOperand._str;
    }

  }
}
