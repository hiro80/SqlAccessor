using System.Threading;

namespace SqlAccessor
{
  /// <summary>
  /// Dispose()とFinalize()で同じ終了処理を実装するためのTemplate Methodをサブクラスに提供する
  /// </summary>
  /// <remarks>
  /// 以下の理由により、必要が無ければこのクラスは継承しないこと
  /// 1. GCはマルチスレッドで動作する場合もあるため、DisposeImp()はスレッドセーフにする必要がある
  /// 2. GCされるオブジェクトの順序は不定であるため、DisposeImp()の実行時、メンバ変数が参照してい
  ///    るオブジェクトが既にGCされている場合がある
  /// 3. Finalize()の実装は微小な速度低下を招く(プログラミング.NET Framework P516より)
  /// </remarks>
  public abstract class Disposable: object, System.IDisposable
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

    ~Disposable() {
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
