using System;
using System.Data;
using System.Data.Common;
//using Pervasive.Data.SqlClient;

namespace SqlAccessor
{
  internal class LoggingDbConnection: IDbConnection
  {
    private readonly IDbConnection _aDbConnection;
    //乱数の種
    private static readonly Random _random = new Random();
    //ID
    private readonly long _id = _random.Next();

    public LoggingDbConnection(IDbConnection aDbConnection) {
      _aDbConnection = aDbConnection;
    }

    private void WriteDebugPrint(string msg) {
      System.Console.WriteLine("DbConnection " + msg + "(" + _id.ToString() + ")");
    }

    public System.Data.IDbTransaction BeginTransaction() {
      WriteDebugPrint("BeginTransaction");
      return _aDbConnection.BeginTransaction();
    }

    public System.Data.IDbTransaction BeginTransaction(System.Data.IsolationLevel il) {
      WriteDebugPrint("BeginTransaction2");
      return _aDbConnection.BeginTransaction(il);
    }

    public void ChangeDatabase(string databaseName) {
      WriteDebugPrint("ChangeDatabase");
      _aDbConnection.ChangeDatabase(databaseName);
    }

    public void Close() {
      WriteDebugPrint("Close");
      _aDbConnection.Close();
    }

    public string ConnectionString {
      get {
        WriteDebugPrint("ConnectionString GET");
        return _aDbConnection.ConnectionString;
      }
      set {
        WriteDebugPrint("ConnectionString SET");
        _aDbConnection.ConnectionString = value;
      }
    }

    public int ConnectionTimeout {
      get {
        WriteDebugPrint("ConnectionString GET");
        return _aDbConnection.ConnectionTimeout;
      }
    }

    public System.Data.IDbCommand CreateCommand() {
      WriteDebugPrint("CreateCommand");
      return _aDbConnection.CreateCommand();
    }

    public string Database {
      get {
        WriteDebugPrint("Database GET");
        return _aDbConnection.Database;
      }
    }

    public void Open() {
      WriteDebugPrint("Open");
      _aDbConnection.Open();
    }

    public System.Data.ConnectionState State {
      get {
        WriteDebugPrint("State GET");
        return _aDbConnection.State;
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
        WriteDebugPrint("Dispose");
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
