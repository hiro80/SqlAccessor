namespace SqlAccessor
{
  internal interface IResults: System.IDisposable
  {
    bool IsDisposed();
    object GetValueOf(string columnName);
    object GetValueOf(int columnPos);
    System.Type GetDataType(int columnPos);
    string GetDbColumnTypeName(int columnPos);
    bool MoveNext();
  }
}


