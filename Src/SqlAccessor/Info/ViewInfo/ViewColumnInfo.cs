using System.Threading;
using System.ComponentModel;

namespace SqlAccessor
{
  /// <summary>
  /// Viewカラムのメタ情報を表す
  /// </summary>
  /// <remarks>条件付きスレッドセーフ</remarks>
  [System.Diagnostics.DebuggerDisplay("ViewColumnName:{ViewColumnName}")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class ViewColumnInfo
  {
    //View名
    private readonly string _viewName;
    //Viewカラム名
    private readonly string _viewColumnName;
    //View内でのカラムの位置
    private readonly int _columnPos;
    //ADO.NETから返されるViewカラムの値の型
    private System.Type _hostDataType;
    //ViewカラムのDBMSネイティブデータ型
    private string _dbColumnTypeName;
    //RegistAllColumnsType()を呼びだすための参照
    private readonly ViewInfo _aViewInfo;

    //
    //ColumnType()_GetからColumnType()_Setを呼びだす時に、
    //デッドロックを起こさないようにするための苦肉の策
    //
    //_callFromMeの導入により、排他制御の可否は以下の表になる
    //    |  R    W
    //  --|-----------
    //  R |  o    x
    //    |
    //  W |  x   △(*)
    //
    //    (*)_callFromMe=Trueの場合はo、Falseの場合はx
    //
    //従って、初期化用SELECT文発行中はSetTypeInfo()を複数スレッドで多重に呼び出される可能性があるが、
    //_hostDataTypeと_dbColumnTypeNameに設定される値は常に同じはずなので、問題になることはないだろう
    private bool _callFromMe;

    //ReadWriteLock
    private readonly ReaderWriterLock _rwLock = new ReaderWriterLock();
    //ReadWriteLockのタイムアウト時間

    private readonly int _timeout = Timeout.Infinite;
    internal ViewColumnInfo(ViewInfo aViewInfo
                          , string viewName
                          , string viewColumnName
                          , int columnPos) {
      _aViewInfo = aViewInfo;
      _viewName = viewName;
      _viewColumnName = viewColumnName;
      _columnPos = columnPos;
    }

    public string ViewName {
      get { return _viewName; }
    }

    public string ViewColumnName {
      get { return _viewColumnName; }
    }

    public int ColumnPos {
      get { return _columnPos; }
    }

    public bool HasHostDataType {
      get {
        try {
          //ロックを取得する
          _rwLock.AcquireReaderLock(_timeout);

          return _hostDataType != null;
        } finally {
          //ロックを開放する
          _rwLock.ReleaseReaderLock();
        }
      }
    }

    public bool HasDbColumnType {
      get {
        try {
          //ロックを取得する
          _rwLock.AcquireReaderLock(_timeout);

          return _dbColumnTypeName != null;
        } finally {
          //ロックを開放する
          _rwLock.ReleaseReaderLock();
        }
      }
    }

    private void LockAndGet<T>(T data) {
      try {
        //ロックを取得する
        _rwLock.AcquireReaderLock(_timeout);

        //Double-Checked Lockingパターン
        if(data == null) {
          System.Threading.LockCookie aLockCookie = default(System.Threading.LockCookie);
          try {
            aLockCookie = _rwLock.UpgradeToWriterLock(_timeout);
            if(data == null) {
              _callFromMe = true;
              //初期化用SELECT文を発行し、同じViewInfoに属する他全てのView列と共にメタ情報を取得する
              _aViewInfo.RegistAllColumnsType();
            }
          } finally {
            _callFromMe = false;
            _rwLock.DowngradeFromWriterLock(ref aLockCookie);
          }
        }

        //RegistAllColumnsType()で登録した値は仮引数であるdataには反映されないことに注意
        //Return data
      } finally {
        //ロックを開放する
        _rwLock.ReleaseReaderLock();
      }
    }

    public System.Type HostDataType {
      get {
        this.LockAndGet(_hostDataType);
        return _hostDataType;
      }
    }

    public string DbColumnTypeName {
      get {
        this.LockAndGet(_dbColumnTypeName);
        return _dbColumnTypeName;
      }
    }

    public void SetTypeInfo(System.Type hostDataType, string dbColumnTypeName) {
      //ViewColumnInfo.ColumnType()から呼ばれた場合は、
      //既にWriterロック済みなので、ロックは取得しない
      if(_callFromMe) {
        _hostDataType = hostDataType;
        _dbColumnTypeName = dbColumnTypeName;
        return;
      }

      try {
        //ロックを取得する
        _rwLock.AcquireWriterLock(_timeout);

        _hostDataType = hostDataType;
        _dbColumnTypeName = dbColumnTypeName;
      } finally {
        //ロックを開放する
        _rwLock.ReleaseWriterLock();
      }
    }
  }
}
