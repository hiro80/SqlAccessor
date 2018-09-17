using System;
using System.Collections.Generic;

namespace SqlAccessor
{
  partial class DbConn
  {
    private class Rollbacked: IDbConnState
    {
      private static readonly Rollbacked _me = new Rollbacked();

      private Rollbacked() {
      }

      public static Rollbacked GetInstance() {
        return _me;
      }

      public void DisposeImp(bool disposing, DbConn aDbConn) {
        //DBから切断する
        try {
          aDbConn.Close();
        } catch(Exception ex) {
          //Error1状態に遷移して例外を再送出する
          aDbConn.GoToError1AndThrow(ex);
        }
        //次の状態に遷移する
        aDbConn.GoToNextState(End.GetInstance());
      }

      public void SemiDispose(bool disposing, DbConn aDbConn) {
        //次の状態に遷移する
        aDbConn.GoToNextState(NoTransaction.GetInstance());
      }

      public void Rollback(DbConn aDbConn) {
        //処理なし
      }

      //Select文を実行する(結果を返す)
      public IResults ExecSelect(DbConn aDbConn
                               , string sql
                               , IEnumerable<string> usedTables
                               , Tran.CacheStrategy aCacheStrategy = Tran.CacheStrategy.UseCache) {
        aDbConn.GoToError1AndThrow(
          new InvalidOperationException("DbConnオブジェクトの無効な状態遷移が発生しました."));
        //Warning対策
        return null;
      }

      //Select文を実行してその結果をCachedResultsオブジェクトで取得する
      public CachedResults ExecSelectForCache(DbConn aDbConn
                                            , string sql
                                            , IEnumerable<string> usedTables) {
        aDbConn.GoToError1AndThrow(
          new InvalidOperationException("DbConnオブジェクトの無効な状態遷移が発生しました."));
        //Warning対策
        return null;
      }

      //SQLを実行する(件数の取得)
      public int ExecCount(DbConn aDbConn
                         , string sql) {
        aDbConn.GoToError1AndThrow(
          new InvalidOperationException("DbConnオブジェクトの無効な状態遷移が発生しました."));
        //Warning対策
        return 0;
      }

      //SQLを実行する(結果を返さない)
      public int ExecSql(DbConn aDbConn
                       , string sql
                       , IEnumerable<string> updateTables) {
        aDbConn.GoToError1AndThrow(
          new InvalidOperationException("DbConnオブジェクトの無効な状態遷移が発生しました."));
        //Warning対策
        return 0;
      }

      //論理式を評価する
      public bool ExecExp(DbConn aDbConn
                        , string expression
                        , IEnumerable<string> usedTables
                        , Tran.CacheStrategy aCacheStrategy = Tran.CacheStrategy.UseCache) {
        aDbConn.GoToError1AndThrow(
          new InvalidOperationException("DbConnオブジェクトの無効な状態遷移が発生しました."));
        //Warning対策
        return false;
      }
    }
  }
}
