using System.Collections.Generic;

namespace SqlAccessor
{
  /// <summary>
  /// 先読み機能付きEnumerator
  /// </summary>
  /// <remarks>Enumeratorに先読み機能を付加するデコレータ</remarks>
  public class LookaheadEnumerator<T>: IEnumerator<T>
  {

    private readonly IEnumerator<T> _itr;
    private T _current;
    private T _next;

    public LookaheadEnumerator(IEnumerator<T> aEnumerator) {
      _itr = aEnumerator;

      //最初に要素を一つ先読みしておく
      if(_itr.MoveNext()) {
        _next = _itr.Current;
      }
    }

    // 重複する呼び出しを検出するには
    private bool disposedValue = false;

    // IDisposable
    protected virtual void Dispose(bool disposing) {
      if(!this.disposedValue) {
        if(disposing) {
          // TODO: 明示的に呼び出されたときにマネージ リソースを解放します
        }

        // TODO: 共有のアンマネージ リソースを解放します
        _itr.Dispose();
      }
      this.disposedValue = true;
    }

    #region " IDisposable Support "
    // このコードは、破棄可能なパターンを正しく実装できるように Visual Basic によって追加されました。
    public void Dispose() {
      // このコードを変更しないでください。クリーンアップ コードを上の Dispose(ByVal disposing As Boolean) に記述します。
      Dispose(true);
      System.GC.SuppressFinalize(this);
    }
    #endregion

    /// <summary>
    /// 列挙子をコレクションの次の要素に進めます
    /// </summary>
    /// <returns></returns>
    /// <remarks></remarks>
    public bool MoveNext() {
      _current = _next;

      if(_current == null) {
        return false;
      }

      if(_itr.MoveNext()) {
        _next = _itr.Current;
      } else {
        _next = default(T);
      }

      return true;
    }

    /// <summary>
    /// コレクション内の現在の要素を取得します
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
    public T Current {
      get { return _current; }
    }

    object System.Collections.IEnumerator.Current {
      get { return _current; }
    }

    /// <summary>
    /// コレクション内の現在の次の要素を取得します
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
    public T Next {
      get { return _next; }
    }

    /// <summary>
    /// 列挙子を初期位置、つまりコレクションの最初の要素の前に設定します
    /// </summary>
    /// <remarks></remarks>
    public void Reset() {
      _itr.Reset();
      _current = default(T);

      //最初に要素を一つ先読みしておく
      if(_itr.MoveNext()) {
        _next = _itr.Current;
      }
    }

  }
}
