using System.Collections.Generic;

namespace SqlAccessor
{
  /// <summary>
  /// データベースへの接続を表す
  /// </summary>
  /// <remarks></remarks>
  internal interface IDbConn: System.IDisposable
  {
    bool IsClosed();
    void Rollback();
    //Select文を実行する(結果を返す)
    IResults ExecSelect(string sql
                      , IEnumerable<string> usedTables
                      , Tran.CacheStrategy aCacheStrategy = Tran.CacheStrategy.UseCache);
    //Select文を実行してその結果をCachedResultsオブジェクトで取得する
    CachedResults ExecSelectForCache(string sql
                                   , IEnumerable<string> usedTables);
    //SQLを実行する(件数の取得)
    int ExecCount(string sql);
    //SQLを実行する(結果を返さない)
    int ExecSql(string sql
              , IEnumerable<string> updateTables);
    //論理式を評価する
    bool ExecExp(string expression
               , IEnumerable<string> usedTables
               , Tran.CacheStrategy aCacheStrategy = Tran.CacheStrategy.UseCache);
  }
}
