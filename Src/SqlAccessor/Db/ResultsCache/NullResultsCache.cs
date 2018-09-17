using System.Collections.Generic;
namespace SqlAccessor
{

  /// <summary>
  /// キャッシ機能なし
  /// </summary>
  /// <remarks>スレッドセーフ</remarks>
  internal class NullResultsCache: Disposable, IResultsCache
  {

    protected override void DisposeImp(bool disposing) {
    }

    public void Add(string selectSql, IEnumerable<string> usedTables, CachedResults aResults) {
    }

    public IResults AddAndFind(string selectSql, IEnumerable<string> usedTables, CachedResults aResults) {
      //AddAndFind()は追加したaResultsを返すことを保証する

      //NullResultsCacheは受け取ったaResultsをProxyでラップする必要はないはず
      //Proxyでラップしないので、Dispose()されれば破棄される
      //Return New CachedResultsProxy(aResults)

      return aResults;
    }

    public IResults Find(string selectSql, IEnumerable<string> usedTables) {
      return null;
    }

    public void Remove(IEnumerable<string> updateTables) {
    }

    public void Clear() {
    }

    public double HitRate {
      get { return 0.0; }
    }

    public bool IsNullCache {
      get { return true; }
    }

    public void AcquireReaderLock() {
    }

    public void ReleaseReaderLock() {
    }

    public void AcquireWriterLock() {
    }

    public void ReleaseWriterLock() {
    }
  }
}
