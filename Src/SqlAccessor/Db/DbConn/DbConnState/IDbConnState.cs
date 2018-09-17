using System.Collections.Generic;

namespace SqlAccessor
{
  partial class DbConn
  {
    /// <summary>
    /// DbConnオブジェクトの状態を表す
    /// </summary>
    /// <remarks></remarks>
    private interface IDbConnState
    {
      void DisposeImp(bool disposing, DbConn aDbConn);
      void SemiDispose(bool disposing, DbConn aDbConn);
      void Rollback(DbConn aDbConn);
      //Select文を実行する(結果を返す)
      IResults ExecSelect(DbConn aDbConn
                        , string sql, IEnumerable<string> usedTables
                        , Tran.CacheStrategy aCacheStrategy = Tran.CacheStrategy.UseCache);
      //Select文を実行してその結果をCachedResultsオブジェクトで取得する
      CachedResults ExecSelectForCache(DbConn aDbConn
                                     , string sql
                                     , IEnumerable<string> usedTables);
      //SQLを実行する(件数の取得)
      int ExecCount(DbConn aDbConn
                  , string sql);
      //SQLを実行する(結果を返さない)
      int ExecSql(DbConn aDbConn
                , string sql
                , IEnumerable<string> updateTables);

      //論理式を評価する
      bool ExecExp(DbConn aDbConn
                 , string expression
                 , IEnumerable<string> usedTables
                 , Tran.CacheStrategy aCacheStrategy = Tran.CacheStrategy.UseCache);
    }
  }
}
