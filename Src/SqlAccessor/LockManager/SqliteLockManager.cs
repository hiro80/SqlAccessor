using System.Collections.Generic;
using System.Threading;
using MiniSqlParser;

namespace SqlAccessor
{
  /// <summary>
  /// DB全体をロックするロックマネージャ、SQLite用
  /// </summary>
  /// <remarks>スレッドセーフ</remarks>
  public class SqliteLockManager: ILockManager
  {
    //ReadWriteLock
    private readonly ReaderWriterLock _rwLock = new ReaderWriterLock();
    //ReadWriteLockのタイムアウト時間
    private readonly int _timeout = Timeout.Infinite;
    //ApTranIdに値が格納されていないことを示す値
    private readonly long _nullApTranId = long.MinValue;
    //DB全体をロックしているApTranId
    private long _apTranId;

    public SqliteLockManager() {
      _apTranId = _nullApTranId;
    }

    public bool Lock<TRecord>(long apTranId
                            , SqlBuilder predicate)
    where TRecord: class, IRecord, new() {
      try {
        //Readerロックを取得する
        _rwLock.AcquireReaderLock(_timeout);

        return _apTranId == _nullApTranId || _apTranId == apTranId;
      } finally {
        //Readerロックを開放する
        _rwLock.ReleaseReaderLock();
      }
    }

    public bool Lock<TRecord>(long apTranId
                            , TRecord aRecord
                            , IEnumerable<SqlTable> usedTableNames = null)
    where TRecord: class, IRecord, new() {
      try {
        //Readerロックを取得する
        _rwLock.AcquireReaderLock(_timeout);

        return _apTranId == _nullApTranId || _apTranId == apTranId;
      } finally {
        //Readerロックを開放する
        _rwLock.ReleaseReaderLock();
      }
    }

    public int CountLock(long apTranId) {
      try {
        //Readerロックを取得する
        _rwLock.AcquireReaderLock(_timeout);

        if(_apTranId == apTranId) {
          return 1;
        } else {
          return 0;
        }
      } finally {
        //Readerロックを開放する
        _rwLock.ReleaseReaderLock();
      }
    }

    public void UnLock(long apTranId) {
      try {
        //Writerロックを取得する
        _rwLock.AcquireWriterLock(_timeout);

        if(_apTranId == apTranId) {
          _apTranId = _nullApTranId;
        }
      } finally {
        //Writerロックを開放する
        _rwLock.ReleaseWriterLock();
      }
    }
  }
}
