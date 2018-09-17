using System;
using System.Data;
using System.Data.Common;

namespace SqlAccessor
{
  internal class LoggingDataReader: IDataReader
  {
    private readonly IDataReader _aDataReader;
    //乱数の種
    private static readonly Random _random = new Random();
    //ID
    private readonly long _id = _random.Next();

    public LoggingDataReader(IDataReader aDataReader) {
      WriteDebugPrint("New");
      _aDataReader = aDataReader;
    }

    private void WriteDebugPrint(string msg) {
      System.Console.WriteLine(_id.ToString() + " DataReader " + msg);
    }

    public void Close() {
      WriteDebugPrint("Close");
      _aDataReader.Close();
    }

    public int Depth {
      get {
        WriteDebugPrint("Depth");
        return _aDataReader.Depth;
      }
    }

    public System.Data.DataTable GetSchemaTable() {
      WriteDebugPrint("GetSchemaTable");
      return _aDataReader.GetSchemaTable();
    }

    public bool IsClosed {
      get {
        WriteDebugPrint("IsClosed");
        return _aDataReader.IsClosed;
      }
    }

    public bool NextResult() {
      WriteDebugPrint("NextResult");
      return _aDataReader.NextResult();
    }

    public bool Read() {
      WriteDebugPrint("Read");
      return _aDataReader.Read();
    }

    public int RecordsAffected {
      get {
        WriteDebugPrint("RecordsAffected");
        return _aDataReader.RecordsAffected;
      }
    }

    public int FieldCount {
      get {
        WriteDebugPrint("FieldCount");
        return _aDataReader.FieldCount;
      }
    }

    public bool GetBoolean(int i) {
      WriteDebugPrint("GetBoolean");
      return _aDataReader.GetBoolean(i);
    }

    public byte GetByte(int i) {
      WriteDebugPrint("GetByte");
      return _aDataReader.GetByte(i);
    }

    public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) {
      WriteDebugPrint("GetBytes");
      return _aDataReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
    }

    public char GetChar(int i) {
      WriteDebugPrint("GetChar");
      return _aDataReader.GetChar(i);
    }

    public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) {
      WriteDebugPrint("GetChars");
      return _aDataReader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
    }

    public System.Data.IDataReader GetData(int i) {
      WriteDebugPrint("GetData");
      return _aDataReader.GetData(i);
    }

    public string GetDataTypeName(int i) {
      WriteDebugPrint("GetDataTypeName");
      return _aDataReader.GetDataTypeName(i);
    }

    public System.DateTime GetDateTime(int i) {
      WriteDebugPrint("GetDateTime");
      return _aDataReader.GetDateTime(i);
    }

    public decimal GetDecimal(int i) {
      WriteDebugPrint("GetDecimal");
      return _aDataReader.GetDecimal(i);
    }

    public double GetDouble(int i) {
      WriteDebugPrint("GetDouble");
      return _aDataReader.GetDouble(i);
    }

    public System.Type GetFieldType(int i) {
      WriteDebugPrint("GetFieldType");
      return _aDataReader.GetFieldType(i);
    }

    public float GetFloat(int i) {
      WriteDebugPrint("GetFloat");
      return _aDataReader.GetFloat(i);
    }

    public System.Guid GetGuid(int i) {
      WriteDebugPrint("GetGuid");
      return _aDataReader.GetGuid(i);
    }

    public short GetInt16(int i) {
      WriteDebugPrint("GetInt16");
      return _aDataReader.GetInt16(i);
    }

    public int GetInt32(int i) {
      WriteDebugPrint("GetInt32");
      return _aDataReader.GetInt32(i);
    }

    public long GetInt64(int i) {
      WriteDebugPrint("GetInt64");
      return _aDataReader.GetInt64(i);
    }

    public string GetName(int i) {
      WriteDebugPrint("GetName");
      return _aDataReader.GetName(i);
    }

    public int GetOrdinal(string name) {
      WriteDebugPrint("GetOrdinal");
      return _aDataReader.GetOrdinal(name);
    }

    public string GetString(int i) {
      WriteDebugPrint("GetString");
      return _aDataReader.GetString(i);
    }

    public object GetValue(int i) {
      WriteDebugPrint("GetValue(i)");
      return _aDataReader.GetValue(i);
    }

    public int GetValues(object[] values) {
      WriteDebugPrint("GetValues(values)");
      return _aDataReader.GetValues(values);
    }

    public bool IsDBNull(int i) {
      WriteDebugPrint("IsDBNull");
      return _aDataReader.IsDBNull(i);
    }

    public object this[int i] {
      get {
        WriteDebugPrint("Item(i)");
        return _aDataReader[i];
      }
    }

    public object this[string name] {
      get {
        WriteDebugPrint("Item(name)");
        return _aDataReader[name];
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
