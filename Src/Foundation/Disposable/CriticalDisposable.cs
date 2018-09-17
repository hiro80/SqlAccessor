using System.Threading;

namespace SqlAccessor
{
  /// <summary>
  /// Dispose()とFinalize()で同じ終了処理を実装するためのTemplate Methodをサブクラスに提供する
  /// このクラスのサブクラスは確実にFinalize()を実行されるための機能が付加される
  /// </summary>
  /// <remarks>
  /// Disposableクラスに以下の機能が加わる(プログラミング.NET Framework P508より)
  ///   1. 確実にFinalize()が実行されるよう、このクラスのサブクラスがインスタンス化される時に
  ///      Finalize()がJITコンパイルされる
  ///   2. CriticalFinalizerObjectを継承していない全クラスのFinalize()が実行された後に、
  ///      このクラスのFinalize()が実行される
  ///   3. ホストアプリケーション(SQL ServerやASP.NETなど)が異常終了した場合でもFinalize()が実行される
  /// </remarks>
  public abstract class CriticalDisposable
    : System.Runtime.ConstrainedExecution.CriticalFinalizerObject, System.IDisposable
  {

    //サブクラスで明示的にDispose()が呼ばれている間に、GCによるFinalize()の呼出が発生しうる
    //そのため、DisposeImp()が同時に実行されないよう排他制御を行う
    //DisposeImp()が実行される前は0、実行されると1になる
    private int _disposed;

    //サブクラスのDisposeImp()内でMyBase.DisposeImp()を実行させるため、Mustoverrideで修飾しない。
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

    protected override void Finalize() {
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
