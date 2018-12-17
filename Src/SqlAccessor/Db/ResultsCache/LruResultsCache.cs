using System.Threading;
using System.Collections.Generic;
namespace SqlAccessor
{

  /// <summary>
  /// SELECT文の抽出結果(Resultsオブジェクト)をLRUキャッシュとして保持する
  /// </summary>
  /// <remarks>スレッドセーフ
  /// iBATIS.NETのLRUキャッシュを参考にしている</remarks>
  internal class LruResultsCache: Disposable, IResultsCache
  {

    //キャッシュデータとそのメタ情報
    private class CacheInfo: Disposable
    {
      public CacheInfo(string aSelectSql, IEnumerable<string> aUsedTables, CachedResults aCache) {
        SelectSql = aSelectSql;
        UsedTables = aUsedTables;
        Cache = aCache;
      }
      protected override void DisposeImp(bool disposing) {
        this.Cache.Dispose();
      }
      public readonly string SelectSql;
      public readonly IEnumerable<string> UsedTables;
      public readonly CachedResults Cache;
    }

    //キャッシュサイズ
    private readonly int _maxCacheSlot;
    //Resultsオブジェクトを格納するキャッシュ
    private readonly Dictionary<string, CacheInfo> _cache;
    //Resultsオブジェクトの最終アクセス順を保持するリスト
    private readonly LinkedList<CacheInfo> _lruList = new LinkedList<CacheInfo>();
    //使用テーブルとSELECT文の対応辞書(テーブル名、SELECT文)

    private readonly Dictionary<string, List<string>> _usedTableDic;

    private readonly object _lock = new object();
    //ReadWriteLock
    private readonly ReaderWriterLock _rwLock = new ReaderWriterLock();
    //ReadWriteLockのタイムアウト時間

    private readonly int _timeout = Timeout.Infinite;

    public LruResultsCache(int maxCacheSlot = 32) {
      _maxCacheSlot = maxCacheSlot;
      _cache = new Dictionary<string, CacheInfo>(_maxCacheSlot + 1);
      _usedTableDic = new Dictionary<string, List<string>>(_maxCacheSlot + 1);
    }

    protected override void DisposeImp(bool disposing) {
      //保持しているCachedResultsオブジェクトをDisposeする
      foreach(CacheInfo aCacheInfo in _lruList) {
        aCacheInfo.Dispose();
      }
      _lruList.Clear();
      _cache.Clear();
    }

    private IResults FindImp(string selectSql) {
      //キャッシュへの問合せ回数を記録
      _findCount += 1;

      if(_cache.ContainsKey(selectSql)) {
        //キャッシュヒットの回数を記録
        _hitCount += 1;

        CacheInfo findCache = _cache[selectSql];
        //アクセス時系列の更新
        _lruList.Remove(findCache);
        _lruList.AddLast(findCache);

        //キャッシュからCachedResultsオブジェクトを取出し、Proxyでラッピングして返す
        return new CachedResultsProxy(findCache.Cache);
      }

      //キャッシュが見つからなければnullを返す
      return null;
    }

    private void AddImp(string selectSql, IEnumerable<string> usedTables, CachedResults aResults) {
      //キャッシュが既に在れば、キャッシュは追加しない
      if(_cache.ContainsKey(selectSql)) {
        return;
      }

      //キャッシュを追加する
      CacheInfo newCache = new CacheInfo(selectSql, usedTables, aResults);
      _cache.Add(selectSql, newCache);

      //使用テーブルとSELECT文の対応辞書を更新する
      foreach(string usedTable in usedTables) {
        if(!_usedTableDic.ContainsKey(usedTable)) {
          _usedTableDic.Add(usedTable, new List<string>());
        }
        _usedTableDic[usedTable].Add(selectSql);
      }

      //最終アクセス順リストに要素を追加する
      _lruList.AddLast(newCache);

      //キャッシュサイズを超えた場合は、最終アクセスが最も過去のキャッシュを削除する
      if(_lruList.Count > _maxCacheSlot) {
        CacheInfo oldestCache = _lruList.First.Value;
        _cache.Remove(oldestCache.SelectSql);
        _lruList.RemoveFirst();
        foreach(string usedTable in oldestCache.UsedTables) {
          //_usedTableDic.Remove(usedTable)

          //テーブル名に紐付く全てのSELECT文が無くなれば、
          //_usedTableDicからそのテーブル名を削除する
          if(_usedTableDic.ContainsKey(usedTable) && _usedTableDic[usedTable].Remove(oldestCache.SelectSql) && _usedTableDic[usedTable].Count == 0) {
            _usedTableDic.Remove(usedTable);
          }
        }
      }
    }

    public IResults Find(string selectSql, IEnumerable<string> usedTables) {
      lock(_lock) {
        return this.FindImp(selectSql);
      }
    }

    public void Add(string selectSql, IEnumerable<string> usedTables, CachedResults aResults) {
      lock(_lock) {
        this.AddImp(selectSql, usedTables, aResults);
      }
    }

    public IResults AddAndFind(string selectSql, IEnumerable<string> usedTables, CachedResults aResults) {
      lock(_lock) {
        this.AddImp(selectSql, usedTables, aResults);
        return this.FindImp(selectSql);
      }
    }

    public void Remove(IEnumerable<string> updateTables) {
      lock(_lock) {
        //更新対象テーブルから抽出するSELECT文
        foreach(string updateTable in updateTables) {
          if(!_usedTableDic.ContainsKey(updateTable)) {
            continue;
          }

          foreach(string selectSql in _usedTableDic[updateTable]) {
            //_cacheのデータは複数のテーブル名に紐付いているので、
            //既に削除されている可能性がある
            if(_cache.ContainsKey(selectSql)) {
              CacheInfo updateCache = _cache[selectSql];
              _cache.Remove(selectSql);
              _lruList.Remove(updateCache);
            }
          }

          _usedTableDic.Remove(updateTable);
        }
      }
    }

    public void Clear() {
      lock(_lock) {
        _cache.Clear();
        _lruList.Clear();
        _usedTableDic.Clear();
      }
    }

    private long _hitCount;
    private long _findCount;
    public double HitRate {
      get {
        if(_findCount == 0) {
          return 0;
        }
        return (double)_hitCount / (double)_findCount;
      }
    }

    public bool IsNullCache {
      get { return false; }
    }

    public void AcquireReaderLock() {
      _rwLock.AcquireReaderLock(_timeout);
    }

    public void ReleaseReaderLock() {
      _rwLock.ReleaseReaderLock();
    }

    public void AcquireWriterLock() {
      _rwLock.AcquireWriterLock(_timeout);
    }

    public void ReleaseWriterLock() {
      _rwLock.ReleaseWriterLock();
    }

  }
}
