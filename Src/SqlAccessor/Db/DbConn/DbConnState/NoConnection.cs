using System;
using System.Collections.Generic;

namespace SqlAccessor
{
  partial class DbConn
  {
    private class NoConnection: IDbConnState
    {
      private static readonly NoConnection _me = new NoConnection();

      private NoConnection() {
      }

      public static NoConnection GetInstance() {
        return _me;
      }

      public void DisposeImp(bool disposing, DbConn aDbConn) {
        //次の状態に遷移する
        aDbConn.GoToNextState(End.GetInstance());
      }

      public void SemiDispose(bool disposing, DbConn aDbConn) {
        //処理なし
      }

      public void Rollback(DbConn aDbConn) {
        //次の状態に遷移する
        aDbConn.GoToNextState(Rollbacked.GetInstance());
      }

      //Select文を実行する(結果を返す)
      public IResults ExecSelect(DbConn aDbConn, string sql
                               , IEnumerable<string> usedTables
                               , Tran.CacheStrategy aCacheStrategy = Tran.CacheStrategy.UseCache) {
        //キャッシュが利用可能か判定する
        IResults aResults = null;
        if(aCacheStrategy == Tran.CacheStrategy.UseCache ||
           aCacheStrategy == Tran.CacheStrategy.UseCacheIfExists) {
          aResults = aDbConn._aResultsCache.Find(sql, usedTables);
        }

        //キャッシュが利用できない場合、DBに接続してトランザクションを開始する
        try {
          if(aResults == null) {
            //DBに接続する
            aDbConn.Open();
            //トランザクションを開始する
            aDbConn.BeginTran();
          }
        } catch(Exception ex) {
          //Error1状態に遷移して例外を再送出する
          aDbConn.GoToError1AndThrow(ex);
        }

        try {
          if(aCacheStrategy == Tran.CacheStrategy.UseCache) {
            if(aResults == null) {
              //キャッシュの追加
              try {
                aDbConn._aResultsCache.AcquireReaderLock();
                //SELECT結果の取得からキャッシュへの格納までを排他制御する
                aResults = aDbConn._aResultsCache.AddAndFind(sql, usedTables, aDbConn.CreateCachedResults(sql));
              } finally {
                aDbConn._aResultsCache.ReleaseReaderLock();
              }
              //次の状態に遷移する
              aDbConn.GoToNextState(Transaction.GetInstance());
            }
            return aResults;

          } else if(aCacheStrategy == Tran.CacheStrategy.UseCacheIfExists) {
            if(aResults == null) {
              //SELECT文を発行する
              aResults = aDbConn.CreateResults(sql);
              //次の状態に遷移する
              aDbConn.GoToNextState(Transaction.GetInstance());
            }
            return aResults;

          } else if(aCacheStrategy == Tran.CacheStrategy.NoCache) {
            //SELECT文を発行する
            aResults = aDbConn.CreateResults(sql);
            //次の状態に遷移する
            aDbConn.GoToNextState(Transaction.GetInstance());
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
        //トランザクションを開始する
        try {
          //DBに接続する
          aDbConn.Open();
          //トランザクションを開始する
          aDbConn.BeginTran();
        } catch(Exception ex) {
          //Error1状態に遷移して例外を再送出する
          aDbConn.GoToError1AndThrow(ex);
        }

        try {
          //
          //ExecSelectForCache()内ではキャッシュの更新はしていないので、排他制御をしない.
          //
          //MarkovResultsCacheクラス内でExecSelectForCache()の抽出結果をキャッシュに格納しているので、
          //MarkovResultsCacheクラス内でロックする.
          //
          //SELECT文を発行する
          CachedResults aResults = aDbConn.CreateCachedResults(sql);
          //次の状態に遷移する
          aDbConn.GoToNextState(Transaction.GetInstance());
          //抽出結果を返す
          return aResults;
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
          //DBに接続する
          aDbConn.Open();
          //トランザクションを開始する
          aDbConn.BeginTran();
        } catch(Exception ex) {
          //Error1状態に遷移して例外を再送出する
          aDbConn.GoToError1AndThrow(ex);
          throw;
        }

        try {
          //SELECT COUNT(*)文を発行する
          int i = aDbConn.ExecCountImp(sql);
          //次の状態に遷移する
          aDbConn.GoToNextState(Transaction.GetInstance());
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
          //DBに接続する
          aDbConn.Open();
          //トランザクションを開始する
          aDbConn.BeginTran();
        } catch(Exception ex) {
          //Error1状態に遷移して例外を再送出する
          aDbConn.GoToError1AndThrow(ex);
          throw;
        }

        try {
          //SQL文を発行する
          int i = aDbConn.ExecSqlImp(sql, updateTables);
          //次の状態に遷移する
          aDbConn.GoToNextState(Transaction.GetInstance());
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
                        , string expression
                        , IEnumerable<string> usedTables
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
