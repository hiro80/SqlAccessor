using System;
using System.Collections.Generic;
namespace SqlAccessor
{

  internal partial class MarkovResultsCache: Disposable, IResultsCache
  {

    //非同期で呼び出すメソッドと同じシグネチャのデリゲート
    public delegate void ExecSelectFunc(string sql, IEnumerable<string> usedTables);

    private readonly IResultsCache _resultsCache;
    private readonly Db _aDb;
    private readonly MarkovPredictors _aSqlPredictor = new MarkovPredictors(6, 1);
    //予測確度の上位何位までのSQL文を採用するか

    private readonly int _predictNum = 3;
    public MarkovResultsCache(IResultsCache resultsCache, Db aDb) {
      _resultsCache = resultsCache;
      _aDb = aDb;
    }

    protected override void DisposeImp(bool disposing) {
      _resultsCache.Dispose();
    }

    private void ExecSelect(string selectSql, IEnumerable<string> usedTables) {
      bool NoCache = _resultsCache.Find(selectSql, usedTables) == null;
      if(NoCache) {
        using(IDbConn aDbConn = _aDb.GetDbConn()) {
          try {
            _resultsCache.AcquireReaderLock();
            //ロック対象処理 START
            _resultsCache.Add(selectSql, usedTables, aDbConn.ExecSelectForCache(selectSql, usedTables));
            //ロック対象処理 END
          } finally {
            _resultsCache.ReleaseReaderLock();
          }
        }
        _hitCount += 1;
      }
    }

    private void AsyncExecSelect(string selectSql, IEnumerable<string> usedTables) {
      //デリゲートオブジェクトの作成
      ExecSelectFunc execSelectFunc = new ExecSelectFunc(this.ExecSelect);
      //非同期呼び出しを開始
      System.IAsyncResult ar = execSelectFunc.BeginInvoke(selectSql, usedTables, null, null);
    }

    private void MemorizeAndPredict(string selectSql, IEnumerable<string> usedTables) {
      List<MarkovPredictor.ExpectedState> expectedSqls = _aSqlPredictor.MemorizeAndPredict(selectSql, usedTables);

      //予測確度の上位3位までのSQL文を採用する
      int i = 0;
      for(i = 0; i <= Math.Min(expectedSqls.Count - 2, _predictNum - 2); i++) {
        //予測SQL文を発行し、その結果をキャッシュに追加する
        this.AsyncExecSelect(expectedSqls[i].State, (IEnumerable<string>)expectedSqls[i].Appended);
      }

      //3位のSQL文は自身のスレッドで実行する
      if(i <= expectedSqls.Count - 1) {
        //予測SQL文を発行し、その結果をキャッシュに追加する
        this.ExecSelect(expectedSqls[i].State, (IEnumerable<string>)expectedSqls[i].Appended);
      }
    }

    private void AsyncMemorizeAndPredict(string selectSql, IEnumerable<string> usedTables) {
      //デリゲートオブジェクトの作成
      ExecSelectFunc memorizeAndPredictFunc = new ExecSelectFunc(this.MemorizeAndPredict);
      //非同期呼び出しを開始
      System.IAsyncResult ar = memorizeAndPredictFunc.BeginInvoke(selectSql, usedTables, null, null);
    }

    public IResults Find(string selectSql, IEnumerable<string> usedTables) {
      this.AsyncMemorizeAndPredict(selectSql, usedTables);

      return _resultsCache.Find(selectSql, usedTables);
    }

    public void Add(string selectSql, IEnumerable<string> usedTables, CachedResults aResults) {
      this.AsyncMemorizeAndPredict(selectSql, usedTables);

      _resultsCache.Add(selectSql, usedTables, aResults);
    }

    public IResults AddAndFind(string selectSql, IEnumerable<string> usedTables, CachedResults aResults) {
      _predictCount += 1;

      this.AsyncMemorizeAndPredict(selectSql, usedTables);

      return _resultsCache.AddAndFind(selectSql, usedTables, aResults);
    }

    public void Remove(IEnumerable<string> updateTables) {
      _resultsCache.Remove(updateTables);
    }

    public void Clear() {
      _resultsCache.Clear();
    }

    private long _hitCount;
    private long _predictCount;
    public double HitRate {
      get { return (double)_hitCount / (double)_predictCount; }
    }

    public bool IsNullCache {
      get { return false; }
    }

    public void AcquireReaderLock() {
      _resultsCache.AcquireReaderLock();
    }

    public void ReleaseReaderLock() {
      _resultsCache.ReleaseReaderLock();
    }

    public void AcquireWriterLock() {
      _resultsCache.AcquireWriterLock();
    }

    public void ReleaseWriterLock() {
      _resultsCache.ReleaseWriterLock();
    }
  }
}
