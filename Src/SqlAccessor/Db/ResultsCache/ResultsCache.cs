using System.Threading;
using System.Collections.Generic;
namespace SqlAccessor
{

  /// <summary>
  /// SELECT文の抽出結果(Resultsオブジェクト)をキャッシュとして保持する
  /// </summary>
  /// <remarks>スレッドセーフ</remarks>
  internal class ResultsCache
  {
    //Resultsオブジェクトを格納するキャッシュ
    private readonly Dictionary<string, CachedResults> _cache = new Dictionary<string, CachedResults>();
    //ReadWriteLock
    private readonly ReaderWriterLock _rwLock = new ReaderWriterLock();
    //ReadWriteLockのタイムアウト時間

    private readonly int _timeout = Timeout.Infinite;
    public IResults Find(string selectSql) {
      try {
        //Readerロックを取得する
        _rwLock.AcquireReaderLock(_timeout);

        if(_cache.ContainsKey(selectSql)) {
          //キャッシュからCachedResultsオブジェクトを取出し、Proxyでラッピングして返す
          return new CachedResultsProxy(_cache[selectSql]);
        } else {
          return null;
        }
      } finally {
        //Readerロックを開放する
        _rwLock.ReleaseReaderLock();
      }
    }

    public void Add(string selectSql, CachedResults aResults) {
      try {
        //Writerロックを取得する
        _rwLock.AcquireWriterLock(_timeout);

        _cache.Add(selectSql, aResults);
      } finally {
        //Writerロックを開放する
        _rwLock.ReleaseWriterLock();
      }
    }
  }
}
