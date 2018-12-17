using System;

namespace SqlAccessor
{
  internal class CachedResultsProxy: IResults
  {
    //Proxyパターンで代理されるオブジェクト
    private readonly CachedResults _aCachedResults;
    //行数
    private readonly int _maxRows;
    //現在の読込位置
    private int _rowPos;

    public CachedResultsProxy(CachedResults aCachedResults) {
      _aCachedResults = aCachedResults;
      _maxRows = _aCachedResults.Rows.Count;
      _rowPos = -1;
    }

    public void Dispose() {
    }

    public bool IsDisposed() {
      //被代理オブジェクトがDisposeされている場合はTrue
      return _aCachedResults == null;
    }

    public object GetValueOf(int columnPos) {
      //ResultsではMoveNext()せずにGetValueOf()を実行するとDbNull値を返すため、その挙動に合わせる
      if(_rowPos < 0) {
        return DBNull.Value;
      }
      return _aCachedResults.Rows[_rowPos][columnPos];
    }

    public object GetValueOf(string columnName) {
      //ResultsではMoveNext()せずにGetValueOf()を実行するとDbNull値を返すため、その挙動に合わせる
      if(_rowPos < 0) {
        return DBNull.Value;
      }
      return _aCachedResults.Rows[_rowPos][columnName];
    }

    public System.Type GetDataType(int columnPos) {
      //ResultsではMoveNext()せずにGetDataType()を実行するとnullを返す?(未確認)
      if(_rowPos < 0) {
        return null;
      }
      return _aCachedResults.GetDataType(columnPos);
    }

    public string GetDbColumnTypeName(int columnPos) {
      //ResultsではMoveNext()せずにGetDataType()を実行するとnullを返す?(未確認)
      if(_rowPos < 0) {
        return null;
      }
      return _aCachedResults.GetDbColumnTypeName(columnPos);
    }

    public bool MoveNext() {
      if(_rowPos >= _maxRows - 1) {
        return false;
      } else {
        _rowPos += 1;
        return true;
      }
    }

    public void Reset() {
      _rowPos = -1;
    }
  }
}
