using System;
using System.Collections;
using System.Collections.Generic;

namespace SqlAccessor
{
  /// <summary>
  /// プレースホルダ名をキーとして値(文字列)を格納したハッシュ
  /// </summary>
  /// <remarks></remarks>
  [System.Serializable(), System.Diagnostics.DebuggerDisplay("_hash:{\"}")]
  internal class PlaceHolders: IEnumerable<KeyValuePair<string, string>>, ICloneable
  {

    //(プレースホルダ名、値)
    private readonly Dictionary<string, string> _hash = new Dictionary<string, string>();
    
    public PlaceHolders() {
      //定義済みプレースホルダを設定する
      this.SetPredefinedPlaceHolders();
    }

    public PlaceHolders(Dictionary<string, string> placeHolderNameAndValues)
      : this() {
      foreach(KeyValuePair<string, string> kv in placeHolderNameAndValues) {
        this.Add(kv.Key, kv.Value);
      }
    }

    private PlaceHolders(Dictionary<string, string> placeHolderNameAndValues
                       , bool noPredefined) {
      if(!noPredefined) {
        this.SetPredefinedPlaceHolders();
      }

      foreach(KeyValuePair<string, string> kv in placeHolderNameAndValues) {
        this.Add(kv.Key, kv.Value);
      }
    }

    private void SetPredefinedPlaceHolders() {
      //@CURRENT_DATE、@CURRENT_TIME、@CURRENT_DATETIMEに現在日付時刻を設定する
      System.DateTime current = System.DateTime.Now;
      this.Add("CURRENT_DATE", current.ToString("yyyyMMdd"));
      this.Add("CURRENT_TIME", current.ToString("HHmmss"));
      this.Add("CURRENT_DATETIME", current.ToString("yyyyMMddHHmmss"));
      this.Add("CURRENT_TIMESTAMP", current.ToString("HHmmss") +
                                    current.Millisecond.ToString().PadLeft(3, '0'));
      this.Add("CURRENT_DATETIMESTAMP", current.ToString("yyyyMMddHHmmss") + 
                                        current.Millisecond.ToString().PadLeft(3, '0'));
    }

    public object Clone() {
      return new PlaceHolders(new Dictionary<string, string>(_hash), true);
    }

    public void Add(string placeHolderName, string value) {
      if(_hash.ContainsKey(placeHolderName)) {
        throw new ArgumentException(
          "既に格納済みのプレースホルダと同名のプレースホルダを格納しようとしました"
          , placeHolderName);
      } else if(string.IsNullOrEmpty(placeHolderName)) {
        throw new ArgumentNullException(
          "placeHolderName", "プレースホルダ名がNULL又は空文字です");
      } else if(value == null) {
        throw new ArgumentNullException("value", 
          "プレースホルダ値がNULLです");
      }
      _hash.Add(placeHolderName, value);
    }

    public void Overwrite(string placeHolderName, string value) {
      if(_hash.ContainsKey(placeHolderName)) {
        //既に格納済みのプレースホルダと同名のプレースホルダを格納する場合は、
        //プレースホルダ値を上書きする
        _hash.Remove(placeHolderName);
      } else if(string.IsNullOrEmpty(placeHolderName)) {
        throw new ArgumentNullException("placeHolderName"
                                      , "プレースホルダ名がNULL又は空文字です");
      } else if(value == null) {
        throw new ArgumentNullException("value"
                                      , "プレースホルダ値がNULLです");
      }
      _hash.Add(placeHolderName, value);
    }

    public void Underwrite(string placeHolderName, string value) {
      if(_hash.ContainsKey(placeHolderName)) {
        //既に格納済みのプレースホルダと同名のプレースホルダが存在する場合は、
        //プレースホルダ値の適用をしない
        return;
      } else if(string.IsNullOrEmpty(placeHolderName)) {
        throw new ArgumentNullException("placeHolderName"
                                      , "プレースホルダ名がNULL又は空文字です");
      } else if(value == null) {
        throw new ArgumentNullException("value"
                                      , "プレースホルダ値がNULLです");
      }
      _hash.Add(placeHolderName, value);
    }

    public void Add(Dictionary<string, string> placeHoldersDic) {
      foreach(KeyValuePair<string, string> kv in placeHoldersDic) {
        this.Add(kv.Key, kv.Value);
      }
    }

    public void Overwrite(Dictionary<string, string> placeHoldersDic) {
      foreach(KeyValuePair<string, string> kv in placeHoldersDic) {
        this.Overwrite(kv.Key, kv.Value);
      }
    }

    public void Underwrite(Dictionary<string, string> placeHoldersDic) {
      foreach(KeyValuePair<string, string> kv in placeHoldersDic) {
        this.Underwrite(kv.Key, kv.Value);
      }
    }

    public void Add(PlaceHolders placeHolders) {
      foreach(KeyValuePair<string, string> kv in placeHolders) {
        this.Add(kv.Key, kv.Value);
      }
    }

    public void Overwrite(PlaceHolders placeHolders) {
      foreach(KeyValuePair<string, string> kv in placeHolders) {
        this.Overwrite(kv.Key, kv.Value);
      }
    }

    public void Underwrite(PlaceHolders placeHolders) {
      foreach(KeyValuePair<string, string> kv in placeHolders) {
        this.Underwrite(kv.Key, kv.Value);
      }
    }

    public void Remove(string placeHolderName) {
      _hash.Remove(placeHolderName);
    }

    public void Remove(IEnumerable<string> placeHolderNames) {
      foreach(string placeHolderName in placeHolderNames) {
        _hash.Remove(placeHolderName);
      }
    }

    public void RemoveByValue(string value) {
      List<string> removeKeys = new List<string>();

      foreach(KeyValuePair<string, string> phValue in _hash) {
        if(phValue.Value == value) {
          removeKeys.Add(phValue.Value);
        }
      }

      foreach(string removeKey in removeKeys) {
        _hash.Remove(removeKey);
      }
    }

    public bool Contains(string placeHolderName) {
      return _hash.ContainsKey(placeHolderName);
    }

    public string this[string placeHolderName] {
      get {
        if(!_hash.ContainsKey(placeHolderName)) {
          return "";
        }
        return _hash[placeHolderName];
      }
      set { _hash[placeHolderName] = value; }
    }

    /// <summary>
    /// 保持している全てのプレースホルダを返す
    /// </summary>
    /// <returns></returns>
    /// <remarks></remarks>
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
      return _hash.GetEnumerator();
    }

    public System.Collections.IEnumerator GetEnumerator1() {
      return this.GetEnumerator();
    }
    System.Collections.IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator1();
    }

    public Dictionary<string, string> ToDictionary() {
      return _hash;
    }
  }
}
