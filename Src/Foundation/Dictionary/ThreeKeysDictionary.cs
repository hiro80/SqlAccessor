using System;
using System.Collections.Generic;

namespace SqlAccessor
{

  /// <summary>
  /// 3つのキーを持つDictionaryクラス
  /// </summary>
  /// <typeparam name="TKey1"></typeparam>
  /// <typeparam name="TKey2"></typeparam>
  /// <typeparam name="TKey3"></typeparam>
  /// <remarks></remarks>
  [Serializable()]
  public class ThreeKeysDictionary<TKey1, TKey2, TKey3, TValue>: ICloneable
  {
    private readonly Dictionary<TKey1, TwoKeysDictionary<TKey2, TKey3, TValue>> _hash;
    public ThreeKeysDictionary() {
      _hash = new Dictionary<TKey1, TwoKeysDictionary<TKey2, TKey3, TValue>>();
    }

    public object Clone() {
      var ret = new ThreeKeysDictionary<TKey1, TKey2, TKey3, TValue>();

      foreach(var kv in _hash) {
        TKey1 key = kv.Key;
        var value = (TwoKeysDictionary<TKey2, TKey3, TValue>)kv.Value.Clone();
        ret._hash.Add(key, value);
      }

      return ret;
    }

    public TValue this[TKey1 key1, TKey2 key2, TKey3 key3] {
      get {
        if(key1 == null || key2 == null || key3 == null) {
          throw new ArgumentNullException("key1/key2/key3", "キーにはNULLを指定できません");
        }
        if(_hash[key1] == null) {
          throw new KeyNotFoundException("key1をキーとする値が存在しません");
        }
        return _hash[key1][key2, key3];
      }
      set {
        if(key1 == null || key2 == null || key3 == null) {
          throw new ArgumentNullException("key1/key2/key3", "キーにはNULLを指定できません");
        }
        if(_hash[key1] == null) {
          throw new KeyNotFoundException("key1をキーとする値が存在しません");
        }
        _hash[key1][key2, key3] = value;
      }
    }

    public Dictionary<TKey3, TValue> Items(TKey1 key1, TKey2 key2) {
      if(key1 == null || key2 == null) {
        throw new ArgumentNullException("key1/key2", "キーにはNULLを指定できません");
      }
      if(_hash[key1] == null) {
        throw new KeyNotFoundException("key1をキーとする値が存在しません");
      }
      return _hash[key1].Items(key2);
    }

    public TwoKeysDictionary<TKey2, TKey3, TValue> Items(TKey1 key1) {
      if(_hash[key1] == null) {
        throw new KeyNotFoundException("key1をキーとする値が存在しません");
      }
      return _hash[key1];
    }

    public Dictionary<TKey1, TwoKeysDictionary<TKey2, TKey3, TValue>> Items() {
      return _hash;
    }

    public void Add(TKey1 key1, TKey2 key2, TKey3 key3, TValue value) {
      if(key1 == null || key2 == null || key3 == null) {
        throw new ArgumentNullException("key1/key2/key3", "キーにはNULLを指定できません");
      }
      if((!_hash.ContainsKey(key1)) || _hash[key1] == null) {
        //key1に対するハッシュオブジェクトが存在しなければ、ハッシュオブジェクトがを生成する
        _hash.Add(key1, new TwoKeysDictionary<TKey2, TKey3, TValue>());
      }

      _hash[key1].Add(key2, key3, value);
    }

    public bool ContainsKey(TKey1 key1) {
      return _hash.ContainsKey(key1);
    }

    public bool ContainsKey(TKey1 key1, TKey2 key2) {
      if(key1 == null || key2 == null) {
        throw new ArgumentNullException("key1/key2", "キーにはNULLを指定できません");
      }
      if(_hash.ContainsKey(key1)) {
        if(_hash[key1].ContainsKey(key2)) {
          return true;
        }
      }

      return false;
    }

    public bool ContainsKey(TKey1 key1, TKey2 key2, TKey3 key3) {
      if(key1 == null || key2 == null || key3 == null) {
        throw new ArgumentNullException("key1/key2/key3", "キーにはNULLを指定できません");
      }
      if(_hash.ContainsKey(key1)) {
        if(_hash[key1].ContainsKey(key2, key3)) {
          return true;
        }
      }

      return false;
    }
  }
}
