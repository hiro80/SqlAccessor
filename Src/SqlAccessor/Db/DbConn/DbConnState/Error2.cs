using System;
using System.Collections.Generic;

namespace SqlAccessor
{
  partial class DbConn
  {
    private class Error2: IDbConnState
    {
      private static readonly Error2 _me = new Error2();

      private Error2() {
      }

      public static Error2 GetInstance() {
        return _me;
      }

      public void DisposeImp(bool disposing, DbConn aDbConn) {
        //処理なし
      }

      public void SemiDispose(bool disposing, DbConn aDbConn) {
        throw new InvalidOperationException("DbConnオブジェクトの無効な状態遷移が発生しました.");
      }

      public void Rollback(DbConn aDbConn) {
        throw new InvalidOperationException("DbConnオブジェクトの無効な状態遷移が発生しました.");
      }

      //Select文を実行する(結果を返す)
      public IResults ExecSelect(DbConn aDbConn
                               , string sql
                               , IEnumerable<string> usedTables
                               , Tran.CacheStrategy aCacheStrategy = Tran.CacheStrategy.UseCache) {
        throw new InvalidOperationException("DbConnオブジェクトの無効な状態遷移が発生しました.");
      }

      //Select文を実行してその結果をCachedResultsオブジェクトで取得する
      public CachedResults ExecSelectForCache(DbConn aDbConn
                                            , string sql
                                            , IEnumerable<string> usedTables) {
        throw new InvalidOperationException("DbConnオブジェクトの無効な状態遷移が発生しました.");
      }

      //SQLを実行する(件数の取得)
      public int ExecCount(DbConn aDbConn
                         , string sql) {
        throw new InvalidOperationException("DbConnオブジェクトの無効な状態遷移が発生しました.");
      }

      //SQLを実行する(結果を返さない)
      public int ExecSql(DbConn aDbConn
                       , string sql
                       , IEnumerable<string> updateTables) {
        throw new InvalidOperationException("DbConnオブジェクトの無効な状態遷移が発生しました.");
      }

      //論理式を評価する
      public bool ExecExp(DbConn aDbConn
                        , string expression
                        , IEnumerable<string> usedTables
                        , Tran.CacheStrategy aCacheStrategy = Tran.CacheStrategy.UseCache) {
        aDbConn.GoToError1AndThrow(
          new InvalidOperationException("DbConnオブジェクトの無効な状態遷移が発生しました."));
        return false;
      }
    }
  }
}
