using System;
using System.Data.Common;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SqlAccessor
{
  /// <summary>
  /// データベースへの接続を表す
  /// </summary>
  /// <remarks></remarks>
  internal abstract partial class DbConn: Disposable, IDbConn
  {
    //現在の状態
    private IDbConnState _state;
    //接続文字列
    private readonly string _connStr;
    //キャッシュテーブルへのリファレンス
    private readonly IResultsCache _aResultsCache;
    //このDbConn内で更新対象となったテーブルのリスト
    private readonly List<string> _updatedTables = new List<string>();
    //ExecSelectが生成するResultsオブジェクトへのリファレンス
    private IResults _aResults;
    //ADO.NET
    private DbConnection _aDbConnection;
    private DbTransaction _aDbTransaction = null;
    //_aDbConnectionオブジェクトへのGCハンドル
    private GCHandle _h_DbConnection;
    //_aDbTransactionオブジェクトへのGCハンドル
    private GCHandle _h_DbTransaction;

    //GCによる回収時にROLLBACKするか否か
    protected readonly bool _commitAtFinalizing;
    //発行するSQL文をログ出力するか否か
    protected readonly bool _debugPrint;
    //発行するSQL文のログ出力先
    protected readonly ILogger _logger;
    //時間計測
    protected readonly Stopwatch _stopwatch = new Stopwatch();

    //
    //●キャッシュの整合性維持のための排他制御
    //
    // トランザクションAとBが同じテーブルを対象に以下の順に処理を行った時、
    // トランザクションBの更新より前の抽出結果がキャッシュに残ってしまう
    // 
    // Tran(A) SELECT文が抽出結果を取得する
    //         * Tran(B)のCOMMIT前なのでTran(B)の更新前の抽出結果を得る
    // Tran(B) 更新をCOMMITする
    // Tran(B) 対象テーブルのキャッシュをクリアする
    // Tran(A) 抽出結果をキャッシュに格納する
    // 
    // これを回避するため、キャッシュ使用時は
    // SELECT文による抽出結果の取得から、キャッシュへの格納までと
    // COMMITから、キャッシュのクリアまでをオーバラップしないよう排他制御する
    //
    // Rader-Writer Lock方式を用いることにより、COMMIT()やRollBack()同士でも
    // 排他制御されてしまうが、その必要性はない
    //

    #region "Must Override Methods"
    protected abstract DbConnection CreateDbConnection();

    protected abstract DbCommand CreateDbCommand(string sql
                                               , DbConnection aDbConnection
                                               , DbTransaction aDbTransaction);

    protected abstract DataAdapter CreateDataAdapter(DbCommand aDbCommand);

    protected abstract string MakeExpEvalSql(string expression);

    protected abstract bool IsDuplicateKeyException(Exception ex);
    #endregion

    public DbConn(string connStr
                , IResultsCache aResultsCache
                , bool commitAtFinalizing
                , bool debugPrint
                , ILogger logger) {
      _connStr = connStr;
      _aResultsCache = aResultsCache;
      _commitAtFinalizing = commitAtFinalizing;
      _debugPrint = debugPrint;
      _logger = logger;
      //次状態への遷移
      this.GoToNextState(NoConnection.GetInstance());
    }

    private void Open() {
      //DBに接続する
      _aDbConnection = this.CreateDbConnection();

      //DbConnectionをGCされないように設定する(GCの実装に依存するため暫定実装とする)
      _h_DbConnection = GCHandle.Alloc(_aDbConnection, GCHandleType.Normal);

      _aDbConnection.ConnectionString = _connStr;
      _aDbConnection.Open();
    }

    private void Close() {
      //aResultsを破棄する
      if(_aResults != null) {
        _aResults.Dispose();
        _aResults = null;
      }
      //トランザクションを終了する(Dispose()のみ行う)
      if(_aDbTransaction != null) {
        _aDbTransaction.Dispose();
        //DbTransactionを解放する
        _h_DbTransaction.Free();
        _aDbTransaction = null;
      }
      //DBから切断する
      if(_aDbConnection != null) {
        _aDbConnection.Close();
        _aDbConnection.Dispose();
        //DbConnectionを解放する
        _h_DbConnection.Free();
        _aDbConnection = null;
      }
    }

    private void BeginTran() {
      //トランザクションを開始する
      if(_aDbTransaction == null) {
        _aDbTransaction = _aDbConnection.BeginTransaction();

        //DbTransactionをGCされないように設定する(GCの実装に依存するため暫定実装とする)
        _h_DbTransaction = GCHandle.Alloc(_aDbTransaction, GCHandleType.Normal);
      }
    }

    private void Commit() {
      //aResultsを破棄する
      if(_aResults != null) {
        _aResults.Dispose();
        _aResults = null;
      }

      //トランザクションを終了する

      if(_aDbTransaction != null) {
        if(!_aResultsCache.IsNullCache && _updatedTables.Count > 0) {
          try {
            _aResultsCache.AcquireWriterLock();
            //ロック対象処理 START
            _aDbTransaction.Commit();
            _aDbTransaction.Dispose();
            _aResultsCache.Remove(_updatedTables);
            //ロック対象処理 END
          } finally {
            _aResultsCache.ReleaseWriterLock();
          }
        } else {
          //キャッシュの更新が発生しない場合はロックしない
          _aDbTransaction.Commit();
          _aDbTransaction.Dispose();
        }

        //DbTransactionを解放する
        _h_DbTransaction.Free();
        _aDbTransaction = null;
        Debug.WriteLineIf(_debugPrint, "COMMIT" + Environment.NewLine);
      }
    }

    private void RollbackImp() {
      //テーブル更新 → そのテーブルからSELECTした結果をキャッシュに保存 → ROLLBACK
      //上記処理の結果、キャッシュした結果とDBとで差異が生じるためROLLBACK時には
      //更新対象となったテーブルに紐付くキャッシュを削除する

      //aResultsを破棄する
      if(_aResults != null) {
        _aResults.Dispose();
        _aResults = null;
      }

      //ロールバックする
      if(_aDbTransaction != null) {
        if(!_aResultsCache.IsNullCache) {
          try {
            _aResultsCache.AcquireWriterLock();
            //ロック対象処理 START
            _aDbTransaction.Rollback();
            _aDbTransaction.Dispose();
            _aResultsCache.Remove(_updatedTables);
            //ロック対象処理 END
          } finally {
            _aResultsCache.ReleaseWriterLock();
          }
        } else {
          //If _aDbTransaction.Connection IsNot Nothing AndAlso
          //   _aDbTransaction.Connection.State = Data.ConnectionState.Connecting Then
          _aDbTransaction.Rollback();
          _aDbTransaction.Dispose();
          //End If
        }

        //DbTransactionを解放する
        _h_DbTransaction.Free();
        _aDbTransaction = null;
        Debug.WriteLineIf(_debugPrint, "ROLLBACK!" + Environment.NewLine);
      }
    }

    private void GoToNextState(IDbConnState nextState) {
      //次の状態に設定する
      _state = nextState;
    }

    private void GoToError1AndThrow(Exception innerException, bool rollback = false) {
      //rollbackフラグがTrueの場合、ROLLBACKする
      if(rollback) {
        try {
          this.RollbackImp();
        } catch {
          innerException = new DbAccessException("トランザクションの取消に失敗しました", innerException);
        }
      }

      //DBから切断する
      //(Open()前にClose()を実行してもエラーにはならない)
      try {
        this.Close();
      } catch {
        innerException = new DbAccessException("DBからの切断に失敗しました", innerException);
      }

      //例外を再送出する
      throw innerException;
    }

    //生成したResultsオブジェクトがDisposeされていなければ、Disposeする
    private void DisposeResults() {

      if(_aResults != null && !_aResults.IsDisposed()) {
#if DEBUG
	    System.Diagnostics.Debug.WriteLine("A Results is disposed by DbConn");
#endif
        _aResults.Dispose();
      }
    }

    private IResults CreateResults(string sql) {
      try {
        //Commandオブジェクトを生成する
        DbCommand aDbCommand = this.CreateDbCommand(sql, _aDbConnection, _aDbTransaction);
        //ResultsオブジェクトがDisposeされていなければ、Disposeする
        this.DisposeResults();

        //時間計測開始
        if(_debugPrint) {
          _stopwatch.Reset();
          _stopwatch.Start();
        }

        //SQLを実行しDataReaderオブジェクトをaResultsオブジェクトに格納して返す
        _aResults = new Results(aDbCommand.ExecuteReader());

        //時間計測終了
        if(_debugPrint) {
          _stopwatch.Stop();
          Trace.WriteLine("[" + _stopwatch.ElapsedMilliseconds.ToString() + "ms]" + Environment.NewLine);
        }

        aDbCommand.Dispose();
        return _aResults;
      } catch(Exception ex) {
        throw new DbAccessException("SELECT文の実行に失敗しました", sql, ex);
      }
    }

    private CachedResults CreateCachedResults(string sql) {
      try {
        //Commandオブジェクトを生成する
        DbCommand aDbCommand = this.CreateDbCommand(sql, _aDbConnection, _aDbTransaction);
        //ResultsオブジェクトがDisposeされていなければ、Disposeする
        this.DisposeResults();

        //時間計測開始
        if(_debugPrint) {
          _stopwatch.Reset();
          _stopwatch.Start();
        }

        //データセットに結果レコードを格納する
        DataAndInfoTable aDataTable = new DataAndInfoTable(aDbCommand.ExecuteReader());

        //時間計測終了
        if(_debugPrint) {
          _stopwatch.Stop();
          Trace.WriteLine("[" + _stopwatch.ElapsedMilliseconds.ToString() + "ms]" + Environment.NewLine);
        }

        //SQLを実行しDataTableオブジェクトをCachedResultsオブジェクトに格納して返す
        return new CachedResults(aDataTable);
      } catch(Exception ex) {
        throw new DbAccessException("SELECT文の実行に失敗しました.", sql, ex);
      }
    }

    private int ExecCountImp(string sql) {
      try {
        //Commandオブジェクトを生成する
        DbCommand aDbCommand = this.CreateDbCommand(sql, _aDbConnection, _aDbTransaction);
        //ResultsオブジェクトがDisposeされていなければ、Disposeする
        this.DisposeResults();

        //時間計測開始
        if(_debugPrint) {
          _stopwatch.Reset();
          _stopwatch.Start();
        }

        //SQLを実行し結果件数を返す
        //(ExecuteScalarの返す値がIntegerであったりShortであったりするためCTypeを使用する)
        int i = Convert.ToInt32(aDbCommand.ExecuteScalar());

        //時間計測終了
        if(_debugPrint) {
          _stopwatch.Stop();
          Trace.WriteLine("[" + _stopwatch.ElapsedMilliseconds.ToString() + "ms]" + Environment.NewLine);
        }

        return i;
      } catch(Exception ex) {
        throw new DbAccessException("SELECT COUNT(*)文の実行に失敗しました", sql, ex);
      }
    }

    //SQLを実行する(結果を返さない)
    private int ExecSqlImp(string sql, IEnumerable<string> updateTables) {
      try {
        //Commandオブジェクトを生成する
        DbCommand aDbCommand = this.CreateDbCommand(sql, _aDbConnection, _aDbTransaction);
        //ResultsオブジェクトがDisposeされていなければ、Disposeする
        this.DisposeResults();

        //時間計測開始
        if(_debugPrint) {
          _stopwatch.Reset();
          _stopwatch.Start();
        }

        //SQLを実行し結果件数を返す
        int i = aDbCommand.ExecuteNonQuery();

        //時間計測終了
        if(_debugPrint) {
          _stopwatch.Stop();
          Trace.WriteLine("[" + _stopwatch.ElapsedMilliseconds.ToString() + "ms, " +
                          i.ToString() + " rows were affected]" + Environment.NewLine);
        }

        //更新対象テーブルを記録する
        this.AddToUpdatedTables(updateTables);

        return i;
      } catch(Exception ex) {
        //SQL文の発行により例外が送出された場合、Error1ではなくRollbackedに遷移する
        this.Rollback();
        if(this.IsDuplicateKeyException(ex)) {
          throw new DuplicateKeyException("一意性制約に違反したため、更新系SQL文の実行に失敗しました", sql, ex);
        } else {
          throw new DbAccessException("更新系SQL文の実行に失敗しました", sql, ex);
        }
      }
    }

    public bool IsAvailable() {
      return object.ReferenceEquals(_state, NoTransaction.GetInstance());
    }

    public bool IsClosed() {
      //DBと接続していない場合はTrue
      //Return _state = State.End OrElse _
      //       _state = State.Start
      return _aDbConnection == null;
    }

    protected override void DisposeImp(bool disposing) {
      //GCによりオブジェクトが破棄される時に(disposing==Falseの場合)
      //例外を送出すると、正常に破棄されないらしい。
      try {
        //GCによる回収時にはCommitAtFinalizingの設定によりCOMMITまたはROLLBACKする
        if(!disposing && !_commitAtFinalizing) {
          _state.Rollback(this);
        }
        _state.DisposeImp(disposing, this);
      } catch {
        if(disposing) {
          throw;
        }
      }
    }

    protected void SemiDispose(bool disposing) {
      _state.SemiDispose(disposing, this);
    }

    public void Rollback() {
      _state.Rollback(this);
    }

    //Select文を実行する(結果を返す)
    public IResults ExecSelect(string sql
                            ,  IEnumerable<string> usedTables
                             , Tran.CacheStrategy aCacheStrategy = Tran.CacheStrategy.UseCache) {
      //自身のTransaction内で更新したテーブルのキャッシュは
      //更新前の結果なので参照しないようにする
      if(aCacheStrategy != Tran.CacheStrategy.NoCache &&
         !_aResultsCache.IsNullCache &&
         this.UpdatedTablesContains(usedTables)) {
        aCacheStrategy = Tran.CacheStrategy.NoCache;
      }

      return _state.ExecSelect(this, sql, usedTables, aCacheStrategy);
    }

    //Select文を実行してその結果をCachedResultsオブジェクトで取得する
    public CachedResults ExecSelectForCache(string sql
                                          , IEnumerable<string> usedTables) {
      return _state.ExecSelectForCache(this, sql, usedTables);
    }

    //SQLを実行する(件数の取得)
    public int ExecCount(string sql) {
      return _state.ExecCount(this, sql);
    }

    //SQLを実行する(結果を返さない)
    public int ExecSql(string sql
                     , IEnumerable<string> updateTables) {
      return _state.ExecSql(this, sql, updateTables);
    }

    //論理式を評価する
    public bool ExecExp(string expression
                      , IEnumerable<string> usedTables
                      , Tran.CacheStrategy aCacheStrategy = Tran.CacheStrategy.UseCache) {
      //自身のTransaction内で更新したテーブルのキャッシュは
      //更新前の結果なので参照しないようにする
      if(aCacheStrategy != Tran.CacheStrategy.NoCache &&
         !_aResultsCache.IsNullCache &&
         this.UpdatedTablesContains(usedTables)) {
        aCacheStrategy = Tran.CacheStrategy.NoCache;
      }

      return _state.ExecExp(this, expression, usedTables, aCacheStrategy);
    }

    private void AddToUpdatedTables(IEnumerable<string> updateTables) {
      foreach(string updateTable in updateTables) {
        if(!_updatedTables.Contains(updateTable)) {
          _updatedTables.Add(updateTable);
        }
      }
    }

    private bool UpdatedTablesContains(IEnumerable<string> usedTables) {
      foreach(string usedTable in usedTables) {
        if(_updatedTables.Contains(usedTable)) {
          return true;
        }
      }
      return false;
    }
  }
}