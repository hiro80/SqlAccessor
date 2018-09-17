using System;
using System.Collections;
using System.Collections.Generic;

namespace SqlAccessor
{
  /// <summary>
  /// 2つのキーを持つDictionaryクラス
  /// </summary>
  /// <typeparam name="TKey1"></typeparam>
  /// <typeparam name="TKey2"></typeparam>
  /// <remarks></remarks>
  [Serializable()]
  public class TwoKeysDictionary<TKey1, TKey2, TValue>
    : IEnumerable<KeyValueTriplet<TKey1, TKey2, TValue>>, ICloneable
  {
    private readonly Dictionary<TKey1, Dictionary<TKey2, TValue>> _hash;
    public TwoKeysDictionary() {
      _hash = new Dictionary<TKey1, Dictionary<TKey2, TValue>>();
    }

    private TwoKeysDictionary(Dictionary<TKey1, Dictionary<TKey2, TValue>> hash) {
      _hash = hash;
    }

    public object Clone() {
      Dictionary<TKey1, Dictionary<TKey2, TValue>> newHash = new Dictionary<TKey1, Dictionary<TKey2, TValue>>(_hash);
      return new TwoKeysDictionary<TKey1, TKey2, TValue>(newHash);
    }

    public TValue this[TKey1 key1, TKey2 key2] {
      get {
        if(key1 == null || key2 == null) {
          throw new ArgumentNullException("key1/key2", "キーにはNULLを指定できません");
        }
        if(_hash[key1] == null) {
          throw new KeyNotFoundException("key1をキーとする値が存在しません");
        }
        return _hash[key1][key2];
      }
      set {
        if(key1 == null || key2 == null) {
          throw new ArgumentNullException("key1/key2", "キーにはNULLを指定できません");
        }
        if(_hash[key1] == null) {
          throw new KeyNotFoundException("key1をキーとする値が存在しません");
        }
        _hash[key1][key2] = value;
      }
    }

    public Dictionary<TKey2, TValue> Items(TKey1 key1) {
      if(_hash[key1] == null) {
        throw new KeyNotFoundException("key1をキーとする値が存在しません");
      }
      return _hash[key1];
    }

    public Dictionary<TKey1, Dictionary<TKey2, TValue>> Items() {
     return _hash;
    }

    public void Add(TKey1 key1, TKey2 key2, TValue value) {
      if(key1 == null || key2 == null) {
        throw new ArgumentNullException("key1/key2", "キーにはNULLを指定できません");
      }
      if((!_hash.ContainsKey(key1)) || _hash[key1] == null) {
        //key1に対するハッシュオブジェクトが存在しなければ、ハッシュオブジェクトがを生成する
        _hash.Add(key1, new Dictionary<TKey2, TValue>());
      }

      _hash[key1].Add(key2, value);
    }

    public void Remove(TKey1 key1) {
      if(key1 == null) {
        throw new ArgumentNullException("key1", "キーにはNULLを指定できません");
      }

      if(_hash.ContainsKey(key1)) {
        _hash.Remove(key1);
      }
    }

    public void Remove(TKey1 key1, TKey2 key2) {
      if(key1 == null || key2 == null) {
        throw new ArgumentNullException("key1/key2", "キーにはNULLを指定できません");
      }

      if(_hash.ContainsKey(key1) && _hash[key1].ContainsKey(key2)) {
        _hash[key1].Remove(key2);
        if(_hash[key1].Count <= 0) {
          _hash.Remove(key1);
        }
      }
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

    public Dictionary<TKey1, Dictionary<TKey2, TValue>>.KeyCollection Keys {
      get { return _hash.Keys; }
    }

    public Dictionary<TKey1, Dictionary<TKey2, TValue>>.ValueCollection Values {
      get { return _hash.Values; }
    }

    public IEnumerator<KeyValueTriplet<TKey1, TKey2, TValue>> GetEnumerator() {
      List<KeyValueTriplet<TKey1, TKey2, TValue>> ret = new List<KeyValueTriplet<TKey1, TKey2, TValue>>();
      foreach(KeyValuePair<TKey1, Dictionary<TKey2, TValue>> kv in _hash) {
        foreach(KeyValuePair<TKey2, TValue> kv2 in kv.Value) {
          ret.Add(new KeyValueTriplet<TKey1, TKey2, TValue>(kv.Key, kv2.Key, kv2.Value));
        }
      }

      return ret.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return this.GetEnumerator();
    }
  }
}
