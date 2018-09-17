using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;

namespace SqlAccessor
{
  /// <summary>
  /// ロック情報をリモートオブジェクトに格納するロックマネージャ
  /// </summary>
  /// <remarks></remarks>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RemoteLockData: System.MarshalByRefObject
  {
    private readonly System.Collections.ArrayList _lockDataSet = new System.Collections.ArrayList();
    //ReadWriteLock

    private readonly ReaderWriterLock _rwLock = new ReaderWriterLock();
    public int Count {
      get { return this._lockDataSet.Count; }
    }
    public object Item(int index) {
      return this._lockDataSet[index];
    }
    public void Add(object lockData) {
      this._lockDataSet.Add(lockData);
    }
    public void RemoveAt(int index) {
      this._lockDataSet.RemoveAt(index);
    }
    public void AcquireReaderLock(int _timeout) {
      this._rwLock.AcquireReaderLock(_timeout);
    }
    public void AcquireWriterLock(int _timeout) {
      this._rwLock.AcquireWriterLock(_timeout);
    }
    public void ReleaseReaderLock() {
      this._rwLock.ReleaseReaderLock();
    }
    public void ReleaseWriterLock() {
      this._rwLock.ReleaseWriterLock();
    }
  }
}
namespace SqlAccessor
{
  internal class RemoteLockManager: LockManager
  {

    //ロックデータ

    private RemoteLockData _memLockData = new RemoteLockData();
    //ReadWriteLockのタイムアウト時間

    private readonly int _timeout = Timeout.Infinite;
    public RemoteLockManager(RecordViewTableMapFactory aRecordViewTableMapFactory
                           , ICaster aCaster)
      : base(aRecordViewTableMapFactory, aCaster) {
    }

    protected override IEnumerable<LockData> FindCandidateSubsumptionLockData(long apTranId
                                                                            , string tableName) {
      List<LockData> ret = new List<LockData>();
      try {
        //Readerロックを取得する
        _memLockData.AcquireReaderLock(_timeout);

        LockData aLockData = null;
        for(int i = 0; i <= _memLockData.Count - 1; i++) {
          aLockData = (LockData)_memLockData.Item(i);
          if(aLockData.ApTranId != apTranId && aLockData.TableName == tableName) {
            ret.Add(aLockData);
          }
        }
      } finally {
        //Readerロックを開放する
        _memLockData.ReleaseReaderLock();
      }

      return ret;
    }

    protected override void SaveLockData(LockManager.LockData aLockData) {
      try {
        //Writerロックを取得する
        _memLockData.AcquireWriterLock(_timeout);

        _memLockData.Add(aLockData);
      } finally {
        //Writerロックを開放する
        _memLockData.ReleaseWriterLock();
      }
    }

    protected override int CountLockData(long apTranId) {
      int ret = 0;
      try {
        //Readerロックを取得する
        _memLockData.AcquireReaderLock(_timeout);

        LockData aLockData = null;
        for(int i = 0; i <= _memLockData.Count - 1; i++) {
          aLockData = (LockData)_memLockData.Item(i);
          if(aLockData.ApTranId == apTranId) {
            ret += 1;
          }
        }
      } finally {
        //Readerロックを開放する
        _memLockData.ReleaseReaderLock();
      }

      return ret;
    }

    protected override void DeleteLockData(long apTranId) {
      int i = 0;
      try {
        //Writerロックを取得する
        _memLockData.AcquireWriterLock(_timeout);

        while((i < _memLockData.Count)) {
          if(((LockData)_memLockData.Item(i)).ApTranId == apTranId) {
            _memLockData.RemoveAt(i);
          } else {
            i += 1;
          }
        }
      } finally {
        //Writerロックを開放する
        _memLockData.ReleaseWriterLock();
      }
    }
  }
}
