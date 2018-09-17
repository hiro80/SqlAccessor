namespace SqlAccessor
{
  /// <summary>
  /// Dbクラスのコンストラクタで指定する設定情報
  /// </summary>
  /// <remarks></remarks>
  public class DbParameters: System.ICloneable
  {
    public enum LockDataType
    {
      Null,
      Memory,
      Db,
      Sqlite
    }

    public enum CacheType
    {
      Null,
      LRU,
      MarkovLRU
    }

    public enum IndentType
    {
      Compact,
      Beautiful
    }

    public enum LoggerType
    {
      Null,
      Console,
      WindowsEventLog
    }

    //
    // 設定値の初期値
    //
    private LockDataType _lockData = LockDataType.Memory;
    private int _connectionPool = -1;
    private bool _commitAtFinalizing = false;
    private CacheType _cache = CacheType.Null;
    private int _cacheSize = 256;
    private IndentType _sqlIndent = IndentType.Compact;
    private bool _debugPrint = false;
    private LoggerType _logger = LoggerType.Console;
    private string _sqlPodsDir = "SqlPods";
    private string _propertyTypesDir = "PropertyTypes";
    private string _castEditorFile;

    /// <summary>
    /// ロック情報の格納場所
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
    public LockDataType LockData {
      get { return _lockData; }
      set { _lockData = value; }
    }

    /// <summary>
    /// DBコネクションのプール数
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
    public int ConnectionPool {
      get { return _connectionPool; }
      set { _connectionPool = value; }
    }

    /// <summary>
    /// Dispose()の実行漏れのTranオブジェクトが
    /// ガベージコレクタによりFinalize処理される時、
    /// COMMITするかROLLBACKするか指定する
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
    public bool CommitAtFinalizing {
      get { return _commitAtFinalizing; }
      set { _commitAtFinalizing = value; }
    }

    /// <summary>
    /// SQL結果のキャッシュ方式
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
    public CacheType Cache {
      get { return _cache; }
      set { _cache = value; }
    }

    /// <summary>
    /// SQL結果のキャッシュサイズ
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
    public int CacheSize {
      get { return _cacheSize; }
      set { _cacheSize = value; }
    }

    /// <summary>
    /// SQL文のインデント方式
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
    public IndentType SqlIndent {
      get { return _sqlIndent; }
      set { _sqlIndent = value; }
    }

    /// <summary>
    /// デバッグ情報の出力有無
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
    public bool DebugPrint {
      get { return _debugPrint; }
      set { _debugPrint = value; }
    }

    /// <summary>
    /// 発行直前のSQL文ログの出力方法
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
    public LoggerType Logger {
      get { return _logger; }
      set { _logger = value; }
    }

    /// <summary>
    /// SqlPodディレクトリの配置場所
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
    public string SqlPodsDir {
      get { return _sqlPodsDir; }
      set { _sqlPodsDir = value; }
    }

    /// <summary>
    /// PropertyTypesディレクトリの配置場所
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
    public string PropertyTypesDir {
      get { return _propertyTypesDir; }
      set { _propertyTypesDir = value; }
    }

    /// <summary>
    /// CastEditorの配置場所
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
    public string CastEditorFile {
      get { return _castEditorFile; }
      set { _castEditorFile = value; }
    }

    public object Clone() {
      DbParameters ret = new DbParameters();
      ret._lockData = _lockData;
      ret._connectionPool = _connectionPool;
      ret._commitAtFinalizing = _commitAtFinalizing;
      ret._cache = _cache;
      ret._cacheSize = _cacheSize;
      ret._sqlIndent = _sqlIndent;
      ret._debugPrint = _debugPrint;
      ret._logger = _logger;
      ret._sqlPodsDir = _sqlPodsDir;
      ret._propertyTypesDir = _propertyTypesDir;
      ret._castEditorFile = _castEditorFile;
      return ret;
    }
  }
}
