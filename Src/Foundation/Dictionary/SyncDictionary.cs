using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace SqlAccessor
{
  /// <summary>
  /// スレッドセーフ版Dictionaryクラス
  /// </summary>
  /// <typeparam name="TKey"></typeparam>
  /// <remarks>スレッドセーフ</remarks>
  public class SyncDictionary<TKey, TValue>: IDictionary<TKey, TValue>
  {
    private readonly IDictionary<TKey, TValue> _aDictionary;
    //ReadWriteLock
    private readonly ReaderWriterLock _rwLock = new ReaderWriterLock();
    //ReadWriteLockのタイムアウト時間
    private readonly int _timeout = Timeout.Infinite;

    public SyncDictionary() {
      _aDictionary = new Dictionary<TKey, TValue>();
    }

#region "Read Write Methods"
    public void Add(KeyValuePair<TKey, TValue> item) {
      try {
        _rwLock.AcquireWriterLock(_timeout);
        _aDictionary.Add(item);
      } finally {
        _rwLock.ReleaseWriterLock();
      }
    }

    public void Add(TKey key, TValue value) {
      try {
        _rwLock.AcquireWriterLock(_timeout);
        _aDictionary.Add(key, value);
      } finally {
        _rwLock.ReleaseWriterLock();
      }
    }

    public bool Remove(KeyValuePair<TKey, TValue> item) {
      try {
        _rwLock.AcquireWriterLock(_timeout);
        return _aDictionary.Remove(item);
      } finally {
        _rwLock.ReleaseWriterLock();
      }
    }

    public bool Remove(TKey key) {
      try {
        _rwLock.AcquireWriterLock(_timeout);
        return _aDictionary.Remove(key);
      } finally {
        _rwLock.ReleaseWriterLock();
      }
    }

    public void Clear() {
      try {
        _rwLock.AcquireWriterLock(_timeout);
        _aDictionary.Clear();
      } finally {
        _rwLock.ReleaseWriterLock();
      }
    }

    public TValue this[TKey key] {
      get {
        try {
          _rwLock.AcquireReaderLock(_timeout);
          return _aDictionary[key];
        } finally {
          _rwLock.ReleaseReaderLock();
        }
      }
      set {
        try {
          _rwLock.AcquireWriterLock(_timeout);
          _aDictionary[key] = value;
        } finally {
          _rwLock.ReleaseWriterLock();
        }
      }
    }
#endregion


#region "Read Only Methods"
    public int Count {
      get {
        try {
          _rwLock.AcquireReaderLock(_timeout);
          return _aDictionary.Count;
        } finally {
          _rwLock.ReleaseReaderLock();
        }
      }
    }

    public bool IsReadOnly {
      get { return _aDictionary.IsReadOnly; }
    }

    public bool Contains(KeyValuePair<TKey, TValue> item) {
      try {
        _rwLock.AcquireReaderLock(_timeout);
        return _aDictionary.Contains(item);
      } finally {
        _rwLock.ReleaseReaderLock();
      }
    }

    public bool ContainsKey(TKey key) {
      try {
        _rwLock.AcquireReaderLock(_timeout);
        return _aDictionary.ContainsKey(key);
      } finally {
        _rwLock.ReleaseReaderLock();
      }
    }

    public ICollection<TKey> Keys {
      get {
        try {
          _rwLock.AcquireReaderLock(_timeout);
          return _aDictionary.Keys;
        } finally {
          _rwLock.ReleaseReaderLock();
        }
      }
    }

    public ICollection<TValue> Values {
      get {
        try {
          _rwLock.AcquireReaderLock(_timeout);
          return _aDictionary.Values;
        } finally {
          _rwLock.ReleaseReaderLock();
        }
      }
    }

    public bool TryGetValue(TKey key, out TValue value) {
      try {
        _rwLock.AcquireReaderLock(_timeout);
        return _aDictionary.TryGetValue(key, out value);
      } finally {
        _rwLock.ReleaseReaderLock();
      }
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
      try {
        _rwLock.AcquireReaderLock(_timeout);
        _aDictionary.CopyTo(array, arrayIndex);
      } finally {
        _rwLock.ReleaseReaderLock();
      }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
      try {
        _rwLock.AcquireReaderLock(_timeout);
        return _aDictionary.GetEnumerator();
      } finally {
        _rwLock.ReleaseReaderLock();
      }
    }

    IEnumerator IEnumerable.GetEnumerator() {
      try {
        _rwLock.AcquireReaderLock(_timeout);
        return _aDictionary.GetEnumerator();
      } finally {
        _rwLock.ReleaseReaderLock();
      }
    }
#endregion

  }
}
