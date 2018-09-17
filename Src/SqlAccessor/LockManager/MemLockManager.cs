using System.Threading;
using System.Collections.Generic;

namespace SqlAccessor
{
  /// <summary>
  /// ロック情報をメモリに格納するロックマネージャ
  /// </summary>
  /// <remarks></remarks>
  internal class MemLockManager: LockManager
  {
    //ロックデータ
    private readonly LinkedList<LockData> _lockDataSet = new LinkedList<LockData>();
    //ReadWriteLock
    private readonly ReaderWriterLock _rwLock = new ReaderWriterLock();
    //ReadWriteLockのタイムアウト時間

    private readonly int _timeout = Timeout.Infinite;
    public MemLockManager(RecordViewTableMapFactory aRecordViewTableMapFactory
                        , ICaster aCaster)
      : base(aRecordViewTableMapFactory, aCaster) {
    }

    protected override IEnumerable<LockData> FindCandidateSubsumptionLockData(long apTranId
                                                                            , string tableName) {
      List<LockData> ret = new List<LockData>();
      try {
        //Readerロックを取得する
        _rwLock.AcquireReaderLock(_timeout);

        foreach(LockData aLockData in _lockDataSet) {
          if(aLockData.ApTranId != apTranId && aLockData.TableName == tableName) {
            ret.Add(aLockData);
          }
        }
      } finally {
        //Readerロックを開放する
        _rwLock.ReleaseReaderLock();
      }

      return ret;
    }

    protected override int CountLockData(long apTranId) {
      int ret = 0;

      try {
        //Readerロックを取得する
        _rwLock.AcquireReaderLock(_timeout);

        foreach(LockData aLockData in _lockDataSet) {
          if(aLockData.ApTranId == apTranId) {
            ret += 1;
          }
        }
      } finally {
        //Readerロックを開放する
        _rwLock.ReleaseReaderLock();
      }

      return ret;
    }

    protected override void SaveLockData(LockManager.LockData aLockData) {
      try {
        //Writerロックを取得する
        _rwLock.AcquireWriterLock(_timeout);

        _lockDataSet.AddLast(aLockData);
      } finally {
        //Writerロックを開放する
        _rwLock.ReleaseWriterLock();
      }
    }

    protected override void DeleteLockData(long apTranId) {
      int i = 0;
      try {
        //Writerロックを取得する
        _rwLock.AcquireWriterLock(_timeout);

        LinkedListNode<LockData> node = _lockDataSet.First;
        while(node != null) {
          LinkedListNode<LockData> nextNode = node.Next;
          if(node.Value.ApTranId == apTranId) {
            _lockDataSet.Remove(node.Value);
          }
          node = nextNode;
        }

      } finally {
        //Writerロックを開放する
        _rwLock.ReleaseWriterLock();
      }
    }
  }
}
