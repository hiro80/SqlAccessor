using System.Collections.Generic;
using MiniSqlParser;

namespace SqlAccessor
{
  /// <summary>
  /// ロックしないロックマネージャ
  /// </summary>
  /// <remarks>スレッドセーフ</remarks>
  internal class NullLockManager: ILockManager
  {
    public bool Lock<TRecord>(long apTranId
                            , SqlBuilder predicate) 
    where TRecord: class, IRecord, new() {
      return true;
    }

    public bool Lock<TRecord>(long apTranId
                            , TRecord aRecord
                            , IEnumerable<SqlTable> usedTableNames = null) 
    where TRecord: class, IRecord, new() {
      return true;
    }

    public int CountLock(long apTranId) {
      return 0;
    }

    public void UnLock(long apTranId) {
    }
  }
}
