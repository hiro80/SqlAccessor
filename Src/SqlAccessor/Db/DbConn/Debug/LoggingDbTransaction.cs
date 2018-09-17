using System;
using System.Data;
using System.Data.Common;

namespace SqlAccessor
{

  internal class LoggingDbTransaction: IDbTransaction
  {
    public void Commit() {
    }

    public System.Data.IDbConnection Connection {
      get { throw new NotImplementedException(); }
    }

    public System.Data.IsolationLevel IsolationLevel {
      get { throw new NotImplementedException(); }
    }


    public void Rollback() {
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
      }
      this.disposedValue = true;
    }

#region " IDisposable Support "
    // このコードは、破棄可能なパターンを正しく実装できるように Visual Basic によって追加されました。
    public void Dispose() {
      // このコードを変更しないでください。クリーンアップ コードを上の Dispose(ByVal disposing As Boolean) に記述します。
      Dispose(true);
      GC.SuppressFinalize(this);
    }
#endregion

  }
}
