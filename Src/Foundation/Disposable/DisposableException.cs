using System.Threading;

namespace SqlAccessor
{
  /// <summary>
  /// Dispose()とFinalize()で同じ終了処理を実装するためのTemplate Methodをサブクラス(例外クラス)に提供する
  /// </summary>
  /// <remarks></remarks>
  public abstract class DisposableException
    : System.ApplicationException
    , System.Runtime.Serialization.ISerializable
    , System.IDisposable
  {

    //サブクラスで明示的にDispose()が呼ばれている間に、GCによるFinalize()の呼出が発生しうる
    //そのため、DisposeImp()が同時に実行されないよう排他制御を行う
    //DisposeImp()が実行される前は0、実行されると1になる
    private int _disposed;

    //disposing: Dispose()が明示的に呼出されている場合: True
    //           Finalize()から呼出されている場合     : False
    protected virtual void DisposeImp(bool disposing) {
    }

    public bool IsDisposed {
      get { return Thread.VolatileRead(ref _disposed) == 1; }
    }

    public void Dispose() {
      if(Interlocked.CompareExchange(ref _disposed, 1, 0) == 0) {
        this.DisposeImp(true);
        System.GC.SuppressFinalize(this);
      }
    }

    ~DisposableException() {

      if(Interlocked.CompareExchange(ref _disposed, 1, 0) == 0) {
#if DEBUG
        System.Diagnostics.Debug.WriteLine("Finalize of " + this.GetType().Name + " is start");
#endif
        this.DisposeImp(false);
#if DEBUG
        System.Diagnostics.Debug.WriteLine("Finalize of " + this.GetType().Name + " is completed");
#endif
      }
    }
  }
}
