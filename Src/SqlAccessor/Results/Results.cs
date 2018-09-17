using System;
using System.Runtime.InteropServices;

namespace SqlAccessor
{
  internal class Results: Disposable, IResults
  {
    private System.Data.IDataReader _aDbDataReader;
    private bool _moveNexted;
    private readonly GCHandle _h_DbDataReader;

    public Results(System.Data.IDataReader aDbDataReader) {
      _aDbDataReader = aDbDataReader;

      //DbDataReaderをGCされないように設定する(GCの実装に依存するため暫定実装とする)
      _h_DbDataReader = GCHandle.Alloc(_aDbDataReader, GCHandleType.Normal);
    }

    protected override void DisposeImp(bool disposing) {
      if((_aDbDataReader != null)) {
        try {
          _aDbDataReader.Close();
          _aDbDataReader.Dispose();
        } catch(Exception ex) {
          //GCによる回収時には例外を送出しない
          if(disposing) {
            throw new DbAccessException("Resultsオブジェクトが終了しました", ex);
          }
        } finally {
          //DbDataReaderを開放する
          _h_DbDataReader.Free();
          _aDbDataReader = null;
        }
      }
    }

    public new bool IsDisposed() {
      //return _aDbDataReader == null
      return base.IsDisposed;
    }

    public object GetValueOf(string columnName) {
      if(_aDbDataReader == null) {
        throw new ObjectDisposedException("Results"
          , "既にDisposeを呼出されたオブジェクトに対して、Dispose以外のメソッドを呼出そうとしました");
      } else if(!_moveNexted) {
        throw new InvalidOperationException("MoveNext()を呼出す前に値を取得しようとしました");
      }

      return _aDbDataReader[columnName];
    }

    public object GetValueOf(int columnPos) {
      if(_aDbDataReader == null) {
        throw new ObjectDisposedException("Results"
          , "既にDisposeを呼出されたオブジェクトに対して、Dispose以外のメソッドを呼出そうとしました");
      } else if(!_moveNexted) {
        throw new InvalidOperationException("MoveNext()を呼出す前に値を取得しようとしました");
      }

      return _aDbDataReader[columnPos];
    }

    public System.Type GetDataType(int columnPos) {
      if(_aDbDataReader == null) {
        throw new ObjectDisposedException("Results"
          , "既にDisposeを呼出されたオブジェクトに対して、Dispose以外のメソッドを呼出そうとしました");
      }

      return _aDbDataReader.GetFieldType(columnPos);
    }

    public string GetDbColumnTypeName(int columnPos) {
      if(_aDbDataReader == null) {
        throw new ObjectDisposedException("Results"
          , "既にDisposeを呼出されたオブジェクトに対して、Dispose以外のメソッドを呼出そうとしました");
      }

      return _aDbDataReader.GetDataTypeName(columnPos);
    }

    public bool MoveNext() {
      if(_aDbDataReader == null) {
        throw new ObjectDisposedException("Results"
          , "既にDisposeを呼出されたオブジェクトに対して、Dispose以外のメソッドを呼出そうとしました");
      }

#if DEBUG
	  if (_aDbDataReader.IsClosed) {
		  Console.WriteLine("Is DataReader Closed ? >> " + _aDbDataReader.IsClosed.ToString());
		  return false;
	  }
#endif
      _moveNexted = true;
      return _aDbDataReader.Read();
    }
  }
}
