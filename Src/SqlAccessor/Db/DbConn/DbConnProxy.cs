using System.Collections.Generic;

namespace SqlAccessor
{
  partial class DbConn
  {
    /// <summary>
    /// コネクションプールで使用するProxy
    /// </summary>
    /// <remarks>DbConn.SemiDispose()はDbConnProxyにだけ公開したいので、
    /// DbConnのインナークラスにする</remarks>
    internal class DbConnProxy: Disposable, IDbConn
    {
      private readonly DbConn _dbConn;
      private int _reUseCount;

      public DbConnProxy(DbConn aDbConn) {
        _dbConn = aDbConn;
        _reUseCount = 1;
      }

      protected override void DisposeImp(bool disposing) {
        if(_reUseCount > 0) {
          _reUseCount -= 1;
          _dbConn.SemiDispose(disposing);
        } else {
          _dbConn.Dispose();
        }
      }

      public bool IsClosed() {
        return _dbConn.IsClosed();
      }

      public void Rollback() {
        _dbConn.Rollback();
      }

      public IResults ExecSelect(string sql
                               , IEnumerable<string> usedTables
                               , Tran.CacheStrategy aCacheStrategy = Tran.CacheStrategy.UseCache) {
        return _dbConn.ExecSelect(sql, usedTables, aCacheStrategy);
      }

      public CachedResults ExecSelectForCache(string sql
                                            , IEnumerable<string> usedTables) {
        return _dbConn.ExecSelectForCache(sql, usedTables);
      }

      public int ExecCount(string sql) {
        return _dbConn.ExecCount(sql);
      }

      public int ExecSql(string sql
                       , IEnumerable<string> updateTables) {
        return _dbConn.ExecSql(sql, updateTables);
      }

      public bool ExecExp(string expression
                        , IEnumerable<string> usedTables
                        , Tran.CacheStrategy aCacheStrategy = Tran.CacheStrategy.UseCache) {
        return _dbConn.ExecExp(expression, usedTables, aCacheStrategy);
      }

    }
  }
}
