using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace SqlAccessor
{
  /// <summary>
  /// ReaderクラスのNull Object
  /// </summary>
  /// <remarks></remarks>
  public class NullReader<T>: Disposable, Reader<T>
  {
    object IEnumerator.Current {
      get { return this.Current; }
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return this.GetEnumerator();
    }

    public bool MoveNext() {
      return false;
    }

    public T Current {
      get { return default(T); }
    }

    public void Reset() {
    }

    public IEnumerator<T> GetEnumerator() {
      return this;
    }

    public bool ContainsListCollection {
      get { return false; }
    }

    public System.Collections.IList GetList() {
      return (new List<T>()).AsReadOnly();
    }

  }
}
