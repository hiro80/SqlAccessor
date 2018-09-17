using System;
using System.Collections.Generic;

namespace SqlAccessor
{
  partial class DbConn
  {
    private class Transaction: IDbConnState
    {
      private static readonly Transaction _me = new Transaction();

      private Transaction() {
      }

      public static Transaction GetInstance() {
        return _me;
      }

      public void DisposeImp(bool disposing, DbConn aDbConn) {
        //トランザクションを終了し、DBから切断する
        try {
          //GCによる回収時にはCommitAtFinalizingの設定によりCOMMITまたはROLLBACKする
          if(disposing || aDbConn._commitAtFinalizing) {
            aDbConn.Commit();
          } else {
            aDbConn.Rollback();
          }
          aDbConn.Close();
        } catch(Exception ex) {
          //GCによる回収時には例外を送出しない
          if(disposing) {
            //Error1状態に遷移して例外を再送出する
            aDbConn.GoToError1AndThrow(ex, true);
          }
        }
        //次の状態に遷移する
        aDbConn.GoToNextState(End.GetInstance());
      }

      public void SemiDispose(bool disposing, DbConn aDbConn) {
        //トランザクションを終了する
        try {
          //GCによる回収時にはCommitAtFinalizingの設定によりCOMMITまたはROLLBACKする
          if(disposing || aDbConn._commitAtFinalizing) {
            aDbConn.Commit();
          } else {
            aDbConn.Rollback();
          }
        } catch(Exception ex) {
          //GCによる回収時には例外を送出しない
          if(disposing) {
            //Error1状態に遷移して例外を再送出する
            aDbConn.GoToError1AndThrow(ex, true);
          }
        }
        //次の状態に遷移する
        aDbConn.GoToNextState(NoTransaction.GetInstance());
      }

      public void Rollback(DbConn aDbConn) {
        //トランザクションを取り消す
        try {
          aDbConn.RollbackImp();
        } catch(Exception ex) {
          //ROLLBACKに失敗した場合は再度ROLLBACKを行い、
          //それが成功した場合は正常な処理に復帰する
          try {
            aDbConn.RollbackImp();
          } catch(Exception ex2) {
            //Error1状態に遷移して例外を再送出する
            aDbConn.GoToError1AndThrow(ex2, true);
          }
        }
        //次の状態に遷移する
        aDbConn.GoToNextState(Rollbacked.GetInstance());
      }

      //Select文を実行する(結果を返す)
      public IResults ExecSelect(DbConn aDbConn
                               , string sql
                               , IEnumerable<string> usedTables
                               , Tran.CacheStrategy aCacheStrategy = Tran.CacheStrategy.UseCache) {
        try {
          if(aCacheStrategy == Tran.CacheStrategy.UseCache) {
            //キャッシュが利用可能か判定する
            IResults aResults = aDbConn._aResultsCache.Find(sql, usedTables);
            if(aResults == null) {
              //キャッシュの追加
              try {
                aDbConn._aResultsCache.AcquireReaderLock();
                //SELECT結果の取得からキャッシュへの格納までを排他制御する
                aResults = aDbConn._aResultsCache.AddAndFind(sql, usedTables, aDbConn.CreateCachedResults(sql));
              } finally {
                aDbConn._aResultsCache.ReleaseReaderLock();
              }
            }
            return aResults;
          } else if(aCacheStrategy == Tran.CacheStrategy.UseCacheIfExists) {
            //キャッシュが利用可能か判定する
            IResults aResults = aDbConn._aResultsCache.Find(sql, usedTables);
            if(aResults == null) {
              //SELECT文を発行する
              aResults = aDbConn.CreateResults(sql);
            }
            return aResults;
          } else if(aCacheStrategy == Tran.CacheStrategy.NoCache) {
            //SELECT文を発行する
            IResults aResults = aDbConn.CreateResults(sql);
            //抽出結果を返す
            return aResults;
          } else {
            throw new ArgumentOutOfRangeException("aCacheStrategy", "Tran.CacheStrategy型の予期しない値です");
          }
        } catch(Exception ex) {
          //Error1状態に遷移して例外を再送出する
          aDbConn.GoToError1AndThrow(ex, true);
          //Warning対策
          return null;
        }
      }

      //Select文を実行してその結果をCachedResultsオブジェクトで取得する
      public CachedResults ExecSelectForCache(DbConn aDbConn
                                            , string sql
                                            , IEnumerable<string> usedTables) {
        try {
          //
          //ExecSelectForCache()内ではキャッシュの更新はしていないので、排他制御をしない.
          //
          //MarkovResultsCacheクラス内でExecSelectForCache()の抽出結果をキャッシュに格納しているので、
          //MarkovResultsCacheクラス内でロックする.
          //
          //SELECT文を発行する
          return aDbConn.CreateCachedResults(sql);
        } catch(Exception ex) {
          //Error1状態に遷移して例外を再送出する
          aDbConn.GoToError1AndThrow(ex, true);
          //Warning対策
          return null;
        }
      }

      //SQLを実行する(件数の取得)
      public int ExecCount(DbConn aDbConn
                         , string sql) {
        try {
          //SELECT COUNT(*)文を発行する
          int i = aDbConn.ExecCountImp(sql);
          //件数を返す
          return i;
        } catch(Exception ex) {
          //Error1状態に遷移して例外を再送出する
          aDbConn.GoToError1AndThrow(ex, true);
          throw;
        }
      }

      //SQLを実行する(結果を返さない)
      public int ExecSql(DbConn aDbConn
                       , string sql
                       , IEnumerable<string> updateTables) {
        try {
          //SQL文を発行する
          int i = aDbConn.ExecSqlImp(sql, updateTables);
          //件数を返す
          return i;
        } catch(Exception ex) {
          //Error1状態に遷移して例外を再送出する
          aDbConn.GoToError1AndThrow(ex, true);
          throw;
        }
      }

      //論理式を評価する
      public bool ExecExp(DbConn aDbConn
                        , string expression, IEnumerable<string> usedTables
                        , Tran.CacheStrategy aCacheStrategy = Tran.CacheStrategy.UseCache) {
        //式評価用SELECT文を作成する
        string sql = aDbConn.MakeExpEvalSql(expression);
        //トランザクション、状態遷移、エラー処理は、ExecSelect()で処理される
        using(IResults aResults = this.ExecSelect(aDbConn, sql, usedTables, aCacheStrategy)) {
          return aResults.MoveNext() && aResults.GetValueOf(0).ToString() == "1";
        }
      }
    }
  }
}
