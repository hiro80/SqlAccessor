using System.Collections.Generic;
namespace SqlAccessor
{

  internal interface IResultsCache: System.IDisposable
  {
    IResults Find(string selectSql, IEnumerable<string> usedTables);
    void Add(string selectSql, IEnumerable<string> usedTables, CachedResults aResults);
    IResults AddAndFind(string selectSql, IEnumerable<string> usedTables, CachedResults aResults);
    void Remove(IEnumerable<string> updateTables);
    void Clear();
    double HitRate { get; }

    bool IsNullCache { get; }
    //キャッシュオブジェクトをロックオブジェクトとする
    //Reader-WriterロックをDbConnとMarkovResultsCacheに提供する
    void AcquireReaderLock();
    void ReleaseReaderLock();
    void AcquireWriterLock();
    void ReleaseWriterLock();
  }
}
