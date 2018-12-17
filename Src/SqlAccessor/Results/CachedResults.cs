using System;
using System.Data;

namespace SqlAccessor
{
  internal class CachedResults: Disposable, IResults
  {
    private DataAndInfoTable _aDataTable;
    private readonly int _maxRows;
    private int _rowPos;

    public CachedResults(DataAndInfoTable aDataTable) {
      _aDataTable = aDataTable;
      _maxRows = _aDataTable.Rows.Count;
      _rowPos = -1;
    }

    protected override void DisposeImp(bool disposing) {
      if(_aDataTable != null) {
        _aDataTable.Dispose();
        _aDataTable = null;
      }
    }

    public new bool IsDisposed() {
      //Return _aDataTable Is null
      return base.IsDisposed;
    }

    private void ThrowIfDisposed() {
      if(_aDataTable == null) {
        throw new ObjectDisposedException("Results"
          , "既にDisposeを呼出されたオブジェクトに対して、Dispose以外のメソッドを呼出そうとしました");
      }
    }

    public DataRowCollection Rows {
      get {
        this.ThrowIfDisposed();
        return _aDataTable.Rows;
      }
    }

    public object GetValueOf(string columnName) {
      this.ThrowIfDisposed();
      //ResultsではMoveNext()せずにGetValueOf()を実行するとDbNull値を返すため、その挙動に合わせる
      if(_rowPos < 0) {
        return DBNull.Value;
      }
      return _aDataTable.Rows[_rowPos][columnName];
    }

    public object GetValueOf(int columnPos) {
      this.ThrowIfDisposed();
      //ResultsではMoveNext()せずにGetValueOf()を実行するとDbNull値を返すため、その挙動に合わせる
      if(_rowPos < 0) {
        return DBNull.Value;
      }
      return _aDataTable.Rows[_rowPos][columnPos];
    }

    public System.Type GetDataType(int columnPos) {
      this.ThrowIfDisposed();
      //ResultsではMoveNext()せずにGetDataType()を実行するとnullを返す?(未確認)

      //Results.GetDataType()と挙動を合わせるため、MoveNext()なきGetDataType()の返り値はNullにしていたが、
      //CachedResultsProxyでラッピングして使用するので、MoveNext()が呼ばれることはない.
      //(CachedResultsオブジェクトはトランザクション間で共有するので、状態を変更してはいけない
      // 現在行はCachedResultsProxyがトランザクション毎に記憶する仕組みになっている)
      //よって _rowPosは常に-1となり、常にデータ型が取得できなくなるので、このチェックを外す
      //
      //if(_rowPos < 0) {
      //  return null
      //}

      return _aDataTable.GetDataType(columnPos);
    }

    public string GetDbColumnTypeName(int columnPos) {
      this.ThrowIfDisposed();
      //ResultsではMoveNext()せずにGetDataType()を実行するとnullを返す?(未確認)

      //if(_rowPos < 0) {
      //  return null
      //}

      return _aDataTable.GetDbColumnTypeName(columnPos);
    }

    public bool MoveNext() {
      this.ThrowIfDisposed();

      if(_rowPos >= _maxRows - 1) {
        return false;
      } else {
        _rowPos += 1;
        return true;
      }
    }

    public void Reset() {
      this.ThrowIfDisposed();
      _rowPos = -1;
    }
  }
}
