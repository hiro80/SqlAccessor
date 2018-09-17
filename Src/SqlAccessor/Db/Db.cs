using System;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using MiniSqlParser;

namespace SqlAccessor
{
  /// <summary>
  /// データベースへの接続を表す
  /// </summary>
  /// <remarks>スレッドセーフにしたい</remarks>
  //Tranオブジェクトの終了処理が正常に行われるように、
  //DbオブジェクトのFinalizeは最後に行われるようにする
  public class Db: CriticalDisposable
  {
    //データベースの種別
    private readonly SqlBuilder.DbmsType _dbms;
    //データベースへの接続文字列
    private readonly string _connStr;
    //設定情報
    private readonly DbParameters _params;
    //生成したTranオブジェクトへの弱いリファレンス
    private List<WeakReference> _tranList = new List<WeakReference>();
    //コネクションプール
    private readonly List<DbConn> _connections;
    //結果レコードのキャッシュテーブル
    private readonly IResultsCache _aResultsCache;
    //レコードとテーブルのマッピング情報
    private readonly RecordViewTableMapFactory _aRecordViewTableMapFactory;
    //テーブル情報
    private readonly TableInfoSet _aTableInfoSet;
    //SqlPod
    private readonly SqlPodFactory _aSqlPodFactory;
    //キャスター
    private readonly ICaster _aCaster;
    //ロックマネージャ
    private readonly ILockManager _aLockManager;
    //発行直前のSQL文ログの出力方法
    private readonly ILogger _aLogger;

    //ReadWriteLock
    private readonly ReaderWriterLock _rwLock = new ReaderWriterLock();
    //ReadWriteLockのタイムアウト時間
    private readonly int _timeout = Timeout.Infinite;

    /// <summary>
    /// データベースに接続する
    /// </summary>
    /// <param name="dbms">DBMSの種別</param>
    /// <param name="connStr">接続文字列</param>
    /// <param name="dbParams">設定情報</param>
    public Db(SqlBuilder.DbmsType dbms
            , string connStr
            , DbParameters dbParams = null) {
      _dbms = dbms;

      _connStr = connStr;
      if(dbParams == null) {
        _params = new DbParameters();
      } else {
        _params = (DbParameters)dbParams.Clone();
      }

      //プールするDB接続を用意する
      _connections = new List<DbConn>();
      for(int i = 0; i <= _params.ConnectionPool; i++) {
        _connections.Add(this.CreateDbConn());
      }

      //キャッシュテーブルの作成
      if(_params.Cache == DbParameters.CacheType.Null) {
        _aResultsCache = new NullResultsCache();
      } else if(_params.Cache == DbParameters.CacheType.LRU) {
        _aResultsCache = new LruResultsCache(_params.CacheSize);
      } else if(_params.Cache == DbParameters.CacheType.MarkovLRU) {
        _aResultsCache = new MarkovResultsCache(
                            new LruResultsCache(_params.CacheSize),
                            this);
      } else {
        throw new InvalidEnumArgumentException("Undefined CacheType is used");
      }

      //キャスターの生成
      _aCaster = this.CreateCaster();
      //SqlPodファクトリの生成
      _aSqlPodFactory = new SqlPodFactory(_params.SqlPodsDir, _dbms);
      //テーブル情報の生成
      _aTableInfoSet = new TableInfoSet(this);
      //レコードとテーブルのマッピング情報の生成
      _aRecordViewTableMapFactory = new RecordViewTableMapFactory(this);

      //ロックマネージャの生成
      //SQLiteの場合は、必ずSqliteLockManagerを使うこと
      if(_dbms == SqlBuilder.DbmsType.Sqlite) {
        _aLockManager = new SqliteLockManager();
      } else if(_params.LockData == DbParameters.LockDataType.Memory) {
        _aLockManager = new MemLockManager(_aRecordViewTableMapFactory, _aCaster);
      } else if(_params.LockData == DbParameters.LockDataType.Db) {
        _aLockManager = new DbLockManager(_aRecordViewTableMapFactory, this);
      } else if(_params.LockData == DbParameters.LockDataType.Sqlite) {
        _aLockManager = new SqliteLockManager();
      } else if(_params.LockData == DbParameters.LockDataType.Null) {
        _aLockManager = new NullLockManager();
      } else {
        throw new InvalidEnumArgumentException("Undefined LockDataType is used");
      }

      //SQL文発行ログの出力メソッドの作成
      if(_params.Logger == DbParameters.LoggerType.Null) {
        _aLogger = new NullLogger();
      } else if(_params.Logger == DbParameters.LoggerType.WindowsEventLog) {
        _aLogger = new WindowsEventLogger();
      } else if(_params.Logger == DbParameters.LoggerType.Console) {
        _aLogger = new ConsoleLogger();
      } else {
        throw new InvalidEnumArgumentException("Undefined LoggerType is used");
      }
    }

    /// <summary>
    /// データベースから切断する
    /// </summary>
    /// <remarks></remarks>
    protected override void DisposeImp(bool disposing) {
      //キャッシュテーブルとこれが保持するResultsオブジェクトを削除する
      if(_aResultsCache != null) {
        _aResultsCache.Dispose();
      }

      if(_connections != null) {
        //コネクションプールの全てのDB接続を閉じる
        foreach(DbConn aDbConn in _connections) {
          //GCによるDbオブジェクト回収時にはCommitAtFinalizingの設定によりCOMMITまたはROLLBACKする
          if(!disposing && !_params.CommitAtFinalizing) {
            aDbConn.Rollback();
          }
          //Dispose()しても被参照カウントが残っていればDB接続は閉じられない
          while(!(aDbConn.IsClosed())) {
            aDbConn.Dispose();
          }
        }
      }
    }

    private DbConn CreateDbConn() {
      try {
        if(_dbms == SqlBuilder.DbmsType.Oracle) {
          return new OracleDbConn(_connStr, _aResultsCache, _params.CommitAtFinalizing, _params.DebugPrint, _aLogger);
        } else if(_dbms == SqlBuilder.DbmsType.Pervasive) {
          return new PsqlDbConn(_connStr, _aResultsCache, _params.CommitAtFinalizing, _params.DebugPrint, _aLogger);
        } else if(_dbms == SqlBuilder.DbmsType.Sqlite) {
          return new SqliteDbConn(_connStr, _aResultsCache, _params.CommitAtFinalizing, _params.DebugPrint, _aLogger);
        } else if(_dbms == SqlBuilder.DbmsType.MsSql) {
          return new MsSqlDbConn(_connStr, _aResultsCache, _params.CommitAtFinalizing, _params.DebugPrint, _aLogger);
        } else {
          throw new InvalidEnumArgumentException("Undefined DbmsType is used");
        }
      } catch(Exception ex) {
        System.Diagnostics.Debug.WriteLine("DBに接続できませんでした");
        throw;
      }
    }

    private ICaster CreateCaster() {
      //CastEditorのDLLをロードする
      ICastEditor castEditor = this.LoadCastEditor(_params.CastEditorFile);

      if(_dbms == SqlBuilder.DbmsType.Oracle) {
        return new Caster(new OracleDataTypeMapper(castEditor), castEditor);
      } else if(_dbms == SqlBuilder.DbmsType.Pervasive) {
        return new Caster(new PsqlDataTypeMapper(castEditor), castEditor);
      } else if(_dbms == SqlBuilder.DbmsType.Sqlite) {
        return new Caster(new SqliteDataTypeMapper(castEditor), castEditor);
      } else if(_dbms == SqlBuilder.DbmsType.MsSql) {
        return new Caster(new MsSqlDataTypeMapper(castEditor), castEditor);
      } else {
        throw new InvalidEnumArgumentException("Undefined DbmsType is used");
      }
    }

    private ICastEditor LoadCastEditor(string castEditorFile) {
      if(string.IsNullOrEmpty(castEditorFile)) {
        return new CastEditor();
      }

      if(!string.IsNullOrEmpty(System.IO.Path.GetDirectoryName(castEditorFile))) {
        throw new ArgumentException("params.CastEditorFileにはディレクトリ名を含めないでください、" + 
                                    "CastEditorはPropertyTypesDirで指定したディレクトリからロードします", "params.CastEditorFile");
      }

      //PropertyTypesDirの絶対パスを取得する
      string rootedPropertyTypesDir = null;
      if(System.IO.Path.IsPathRooted(_params.PropertyTypesDir)) {
        rootedPropertyTypesDir = _params.PropertyTypesDir;
      } else {
        rootedPropertyTypesDir = System.IO.Path.Combine(
                                    new System.IO.FileInfo(
                                      new Uri(this.GetType().Assembly.CodeBase).LocalPath
                                    ).DirectoryName, 
                                    _params.PropertyTypesDir);
      }

      //CastEditorの絶対パスを取得する
      string rootedCastEditorPath = null;
      rootedCastEditorPath = System.IO.Path.Combine(rootedPropertyTypesDir, _params.CastEditorFile);

      //CastEditorのクラス名を取得する
      string CastEditorClassName = System.IO.Path.GetFileNameWithoutExtension(_params.CastEditorFile);

      //CastEditor.Dllからクラスを生成する
      ClassLoader<ICastEditor> classLoader = new ClassLoader<ICastEditor>(rootedCastEditorPath, "SqlAccessor", CastEditorClassName);
      if(classLoader.FileExists()) {
        return classLoader.Load();
      } else {
        return new CastEditor();
      }
    }

    internal IDbConn GetDbConn() {
      if(this.IsDisposed) {
        throw new InvalidOperationException("Dbオブジェクトは既に破棄されています");
      }

      //コネクションプールから利用可能なDB接続を見つける
      foreach(DbConn aDbConn in _connections) {
        if(aDbConn.IsAvailable()) {
          //Double-Checked Lockingパターン
          //同じaDbConnオブジェクトを返さないようにロックする
          lock(_connections) {
            if(aDbConn.IsAvailable()) {
              //見つけたDB接続にProxyをラップして返す
              return new DbConn.DbConnProxy(aDbConn);
            }
          }
        }
      }

      //コネクションプールに利用可能なDB接続が無ければ新たにDB接続を生成して返す
      return this.CreateDbConn();
    }

    /// <summary>
    /// connStrで指定された接続文字列でデータベースに接続できるか判定する
    /// </summary>
    /// <param name="dbms">DBMSの種別</param>
    /// <param name="connStr">接続文字列</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public static bool IsConnectable(SqlBuilder.DbmsType dbms
                                   , string connStr) {
      bool ret = false;
      Db aDb = null;
      DbConn aDbConn = null;

      try {
        aDb = new Db(dbms, connStr);
        aDbConn = aDb.CreateDbConn();
        aDbConn.Dispose();
        aDb.Dispose();
        //接続、及び切断ができたら接続可と判定する
        ret = true;
      } catch(Exception ex) {
        //接続、又は切断時に例外が発生したら接続不可と判定する
        ret = false;
      } finally {
        if(aDbConn != null) {
          aDbConn.Dispose();
        }
        if(aDb != null) {
          aDb.Dispose();
        }
      }

      return ret;
    }

    /// <summary>
    /// トランザクションを開始する
    /// </summary>
    /// <returns>Tranオブジェクト</returns>
    /// <remarks></remarks>
    public Tran CreateTran() {
      if(this.IsDisposed) {
        throw new InvalidOperationException("Dbオブジェクトは既に破棄されています");
      }

      Tran tran = new Tran(this);

      //_tranListにおいて、Tranオブジェクトへの参照が切れている要素を、1件だけ削除する
      //(参照切れ要素が残る可能性あり、暫定実装)
      try {
        _rwLock.AcquireWriterLock(_timeout);

        for(int i = 0; i <= _tranList.Count - 1; i++) {
          if(!_tranList[i].IsAlive) {
            _tranList.RemoveAt(i);
            break;
          }
        }

        //_tranListにTranオブジェクトへの弱い参照を追加する
        _tranList.Add(new WeakReference(tran));
      } finally {
        _rwLock.ReleaseWriterLock();
      }

      return tran;
    }

    //DBのLockDataにアクセスするためのメソッド(暫定設計)
    //このメソッドで生成したTranのAPトランザクションIDは-1になる
    internal Tran CreateTranWithoutLock() {
      if(this.IsDisposed) {
        throw new InvalidOperationException("Dbオブジェクトは既に破棄されています");
      }

      return new Tran(this, -1);
    }

    internal RecordViewTableMapFactory GetRecordViewTableMapFactory() {
      return _aRecordViewTableMapFactory;
    }

    internal TableInfoSet GetTableInfoSet() {
      return _aTableInfoSet;
    }

    internal SqlPodFactory GetSqlPodFactory() {
      return _aSqlPodFactory;
    }

    internal ICaster GetCaster() {
      return _aCaster;
    }

    internal ILockManager GetLockManager() {
      return _aLockManager;
    }

    internal SqlBuilder.IndentType GetSqlIndentType() {
      if((_params.SqlIndent == DbParameters.IndentType.Compact)) {
        return SqlBuilder.IndentType.Compact;
      } else {
        return SqlBuilder.IndentType.Beautiful;
      }
    }

    internal DbParameters.CacheType GetCacheType() {
      return _params.Cache;
    }

    internal bool GetCommitAtFinalizing() {
      return _params.CommitAtFinalizing;
    }

    /// <summary>
    /// データベースからレコードを取得する
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="criteriaRec">抽出条件を格納したレコード</param>
    /// <param name="aPlaceHolders">プレースホルダ</param>
    /// <param name="aLoadMode">読込モード</param>
    /// <param name="aCacheStrategy">キャッシュ動作方式</param>
    /// <returns>取得したレコードのコレクション</returns>
    /// <remarks></remarks>
    /// 
    public IRecordReader<TRecord> Find<TRecord>(TRecord criteriaRec
                                              , Dictionary<string, string> aPlaceHolders
                                              , Tran.LoadMode aLoadMode = Tran.LoadMode.ReadOnly
                                              , Tran.CacheStrategy aCacheStrategy = Tran.CacheStrategy.UseCache)
    where TRecord: class, IRecord, new() {
      //トランザクションの開始
      Tran aTran = this.CreateTran();
      //イテレート終了時にトランザクションを終了するRecordReaderを返す
      return new CommitAtEndRecordReader<TRecord>(aTran.Find(criteriaRec, aPlaceHolders, aLoadMode, aCacheStrategy), aTran);
    }

    /// <summary>
    /// データベースからレコードを取得する
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="aIQuery">Queryオブジェクト</param>
    /// <param name="aPlaceHolders">プレースホルダ</param>
    /// <param name="aLoadMode">読込モード</param>
    /// <param name="aCacheStrategy">キャッシュ動作方式</param>
    /// <returns>取得したレコードのコレクション</returns>
    /// <remarks>aQueryがQuery(Of TRecord)型でないのは、
    /// オーバーロード解決時に、ジェネリックパラメータの制約が無視される為</remarks>
    public IRecordReader<TRecord> Find<TRecord>(Query<TRecord> aIQuery
                                              , Dictionary<string, string> aPlaceHolders
                                              , Tran.LoadMode aLoadMode = Tran.LoadMode.ReadOnly
                                              , Tran.CacheStrategy aCacheStrategy = Tran.CacheStrategy.UseCache) 
    where TRecord: class, IRecord, new() {
      //トランザクションの開始
      Tran aTran = this.CreateTran();
      //イテレート終了時にトランザクションを終了するRecordReaderを返す
      return new CommitAtEndRecordReader<TRecord>(aTran.Find<TRecord>(aIQuery, aPlaceHolders, aLoadMode, aCacheStrategy), aTran);
    }

    /// <summary>
    /// データベースからレコードを取得する
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="criteriaRec">抽出条件を格納したレコード</param>
    /// <param name="aLoadMode">読込モード</param>
    /// <param name="aCacheStrategy">キャッシュ動作方式</param>
    /// <returns>取得したレコードのコレクション</returns>
    /// <remarks></remarks>
    public IRecordReader<TRecord> Find<TRecord>(TRecord criteriaRec
                                              , Tran.LoadMode aLoadMode = Tran.LoadMode.ReadOnly
                                              , Tran.CacheStrategy aCacheStrategy = Tran.CacheStrategy.UseCache) 
    where TRecord: class, IRecord, new() {
      //トランザクションの開始
      Tran aTran = this.CreateTran();
      //イテレート終了時にトランザクションを終了するRecordReaderを返す
      return new CommitAtEndRecordReader<TRecord>(aTran.Find(criteriaRec, null, aLoadMode, aCacheStrategy), aTran);
    }

    /// <summary>
    /// データベースからレコードを取得する
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="aIQuery">Queryオブジェクト</param>
    /// <param name="aLoadMode">読込モード</param>
    /// <param name="aCacheStrategy">キャッシュ動作方式</param>
    /// <returns>取得したレコードのコレクション</returns>
    /// <remarks>aQueryがQuery(Of TRecord)型でないのは、
    /// オーバーロード解決時に、ジェネリックパラメータの制約が無視される為</remarks>
    public IRecordReader<TRecord> Find<TRecord>(Query<TRecord> aIQuery
                                              , Tran.LoadMode aLoadMode = Tran.LoadMode.ReadOnly
                                              , Tran.CacheStrategy aCacheStrategy = Tran.CacheStrategy.UseCache) 
    where TRecord: class, IRecord, new() {
      //トランザクションの開始
      Tran aTran = this.CreateTran();
      //イテレート終了時にトランザクションを終了するRecordReaderを返す
      return new CommitAtEndRecordReader<TRecord>(aTran.Find<TRecord>(aIQuery, null, aLoadMode, aCacheStrategy), aTran);
    }

    /// <summary>
    /// データベースからレコードを1件取得する
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="criteriaRec">抽出条件を格納したレコード</param>
    /// <param name="aPlaceHolders">プレースホルダ</param>
    /// <param name="aLoadMode">読込モード</param>
    /// <param name="aCacheStrategy">キャッシュ動作方式</param>
    /// <returns>取得したレコード</returns>
    /// <remarks>指定された抽出条件で2件以上のレコードが該当した場合は、例外を送出する</remarks>
    public TRecord FindOne<TRecord>(TRecord criteriaRec
                                  , Dictionary<string, string> aPlaceHolders
                                  , Tran.LoadMode aLoadMode = Tran.LoadMode.ReadOnly
                                  , Tran.CacheStrategy aCacheStrategy = Tran.CacheStrategy.UseCache) 
    where TRecord: class, IRecord, new() {
      using(Tran aTran = this.CreateTran()) {
        return aTran.FindOne(criteriaRec, aPlaceHolders, aLoadMode, aCacheStrategy);
      }
    }

    /// <summary>
    /// データベースからレコードを1件取得する
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="aQuery">Queryオブジェクト</param>
    /// <param name="aPlaceHolders">プレースホルダ</param>
    /// <param name="aLoadMode">読込モード</param>
    /// <param name="aCacheStrategy">キャッシュ動作方式</param>
    /// <returns>取得したレコード</returns>
    /// <remarks>指定された抽出条件で2件以上のレコードが該当した場合は、例外を送出する</remarks>
    public TRecord FindOne<TRecord>(Query<TRecord> aQuery
                                  , Dictionary<string, string> aPlaceHolders
                                  , Tran.LoadMode aLoadMode = Tran.LoadMode.ReadOnly
                                  , Tran.CacheStrategy aCacheStrategy = Tran.CacheStrategy.UseCache) 
    where TRecord: class, IRecord, new() {
      using(Tran aTran = this.CreateTran()) {
        return aTran.FindOne<TRecord>(aQuery, aPlaceHolders, aLoadMode, aCacheStrategy);
      }
    }

    /// <summary>
    /// データベースからレコードを1件取得する
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="criteriaRec">抽出条件を格納したレコード</param>
    /// <param name="aLoadMode">読込モード</param>
    /// <param name="aCacheStrategy">キャッシュ動作方式</param>
    /// <returns>取得したレコード</returns>
    /// <remarks>指定された抽出条件で2件以上のレコードが該当した場合は、例外を送出する</remarks>
    public TRecord FindOne<TRecord>(TRecord criteriaRec
                                  , Tran.LoadMode aLoadMode = Tran.LoadMode.ReadOnly
                                  , Tran.CacheStrategy aCacheStrategy = Tran.CacheStrategy.UseCache) 
    where TRecord: class, IRecord, new() {
      using(Tran aTran = this.CreateTran()) {
        return aTran.FindOne(criteriaRec, null, aLoadMode, aCacheStrategy);
      }
    }

    /// <summary>
    /// データベースからレコードを1件取得する
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="aQuery">Queryオブジェクト</param>
    /// <param name="aLoadMode">読込モード</param>
    /// <param name="aCacheStrategy">キャッシュ動作方式</param>
    /// <returns>取得したレコード</returns>
    /// <remarks>指定された抽出条件で2件以上のレコードが該当した場合は、例外を送出する</remarks>
    public TRecord FindOne<TRecord>(Query<TRecord> aQuery
                                  , Tran.LoadMode aLoadMode = Tran.LoadMode.ReadOnly
                                  , Tran.CacheStrategy aCacheStrategy = Tran.CacheStrategy.UseCache) 
    where TRecord: class, IRecord, new() {
      using(Tran aTran = this.CreateTran()) {
        return aTran.FindOne<TRecord>(aQuery, null, aLoadMode, aCacheStrategy);
      }
    }

    /// <summary>
    /// データベースから抽出条件に一致するレコードの件数を返す
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="criteriaRec">抽出条件を格納したレコード</param>
    /// <returns>レコード件数</returns>
    /// <remarks></remarks>
    public int Count<TRecord>(TRecord criteriaRec) 
    where TRecord: class, IRecord, new() {
      using(Tran aTran = this.CreateTran()) {
        return aTran.Count(criteriaRec);
      }
    }

    /// <summary>
    /// データベースから抽出条件に一致するレコードの件数を返す
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="aIQuery">Queryオブジェクト</param>
    /// <returns>レコード件数</returns>
    /// <remarks>aQueryがQuery(Of TRecord)型でないのは、
    /// オーバーロード解決時に、ジェネリックパラメータの制約が無視される為</remarks>
    public int Count<TRecord>(Query<TRecord> aIQuery) 
    where TRecord: class, IRecord, new() {
      using(Tran aTran = this.CreateTran()) {
        return aTran.Count<TRecord>(aIQuery);
      }
    }

    /// <summary>
    /// データベースにレコードを保存する
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="saveRec">保存するレコード</param>
    /// <param name="aPlaceHolders">プレースホルダ</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public int Save<TRecord>(TRecord saveRec
                           , Dictionary<string, string> aPlaceHolders = null) 
    where TRecord: class, IRecord, new() {
      using(Tran aTran = this.CreateTran()) {
        return aTran.Save(saveRec, aPlaceHolders);
      }
    }

    /// <summary>
    /// データベースからレコードを削除する
    /// </summary>
    /// <typeparam name="TRecord"></typeparam>
    /// <param name="criteriaRec">削除条件を格納したレコード</param>
    /// <param name="aPlaceHolders">プレースホルダ</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public int Delete<TRecord>(TRecord criteriaRec
                             , Dictionary<string, string> aPlaceHolders = null) 
    where TRecord: class, IRecord, new() {
      using(Tran aTran = this.CreateTran()) {
        return aTran.Delete(criteriaRec, aPlaceHolders);
      }
    }

    public int Call<TRecord>(string sqlEntryName
                           , TRecord valueRecord
                           , Query<TRecord> aQuery
                           , Dictionary<string, string> aPlaceHolders = null)
    where TRecord: class, IRecord, new() {
      using(Tran aTran = this.CreateTran()) {
        return aTran.Call(sqlEntryName, valueRecord, aQuery, aPlaceHolders);
      }
    }

    public int Call<TRecord>(string sqlEntryName
                           , TRecord valueRecord
                           , Dictionary<string, string> aPlaceHolders = null) 
    where TRecord: class, IRecord, new() {
      using(Tran aTran = this.CreateTran()) {
        return aTran.Call(sqlEntryName, valueRecord, aPlaceHolders);
      }
    }

    public int Call<TRecord>(string sqlEntryName
                           , Query<TRecord> aQuery
                           , Dictionary<string, string> aPlaceHolders = null) 
    where TRecord: class, IRecord, new() {
      using(Tran aTran = this.CreateTran()) {
        return aTran.Call(sqlEntryName, aQuery, aPlaceHolders);
      }
    }

    /// <summary>
    /// 現在実行中のAPトランザクションIDを取得する
    /// </summary>
    /// <returns></returns>
    /// <remarks></remarks>
    public List<long> GetAllTranId() {
      if(this.IsDisposed) {
        throw new InvalidOperationException("Dbオブジェクトは既に破棄されています");
      }

      List<long> ret = new List<long>();

      try {
        _rwLock.AcquireWriterLock(_timeout);

        Tran aTran = null;
        for(int i = 0; i <= _tranList.Count - 1; i++) {
          aTran = (Tran)_tranList[i].Target;
          //Tranオブジェクトへの参照が切れている要素があれば削除する
          if(aTran == null) {
            _tranList.RemoveAt(i);
            i -= 1;
            continue;
          }
          ret.Add(aTran.ApTranId);
        }
      } finally {
        _rwLock.ReleaseWriterLock();
      }

      return ret;
    }

    /// <summary>
    /// 指定されたAPトランザクションIDのTranオブジェクトを取得する
    /// </summary>
    /// <param name="apTranId">APトランザクションID</param>
    /// <returns>Tranオブジェクト</returns>
    /// <remarks>Tranはスレッドセーフでないため問題あり</remarks>
    public Tran GetTran(long apTranId) {
      if(this.IsDisposed) {
        throw new InvalidOperationException("Dbオブジェクトは既に破棄されています");
      }

      try {
        _rwLock.AcquireReaderLock(_timeout);

        foreach(WeakReference weakRef in _tranList) {
          Tran aTran = (Tran)weakRef.Target;
          if(aTran != null && aTran.ApTranId == apTranId) {
            return aTran;
          }
        }
      } finally {
        _rwLock.ReleaseReaderLock();
      }

      return null;
    }

    public double CacheHitRate {
      get { return _aResultsCache.HitRate; }
    }

    public void ClearCache() {
      if(this.IsDisposed) {
        throw new InvalidOperationException("Dbオブジェクトは既に破棄されています");
      }
      _aResultsCache.Clear();
    }

    /// <summary>
    /// 値を指定したプロパティ型に変換する
    /// </summary>
    /// <param name="value">変換する値</param>
    /// <param name="propertyTypeObj">プロパティ型</param>
    /// <returns></returns>
    /// <remarks>
    /// SqlAccesssorDataSourceで利用します
    /// 変換する値には、基本的な型の値しか指定できない
    /// </remarks>
    public object CastToPropertyType(object value, System.Type propertyTypeObj) {
      if(this.IsDisposed) {
        throw new InvalidOperationException("Dbオブジェクトは既に破棄されています");
      }
      return _aCaster.CastToPropertyType(value, propertyTypeObj);
    }

    /// <summary>
    /// 値がNULL表現値か否か判定する
    /// </summary>
    /// <param name="value">判定する値</param>
    /// <returns></returns>
    /// <remarks>SqlAccesssorDataSourceで利用します</remarks>
    public bool IsNullPropertyValue(object value) {
      if(this.IsDisposed) {
        throw new InvalidOperationException("Dbオブジェクトは既に破棄されています");
      }
      return _aCaster.IsNullPropertyValue(value);
    }

    public SqlBuilder.DbmsType Dbms {
      get { return _dbms; }
    }
  }
}

