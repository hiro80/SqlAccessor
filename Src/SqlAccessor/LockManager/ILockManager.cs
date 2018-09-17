using System.Collections.Generic;
using MiniSqlParser;

namespace SqlAccessor
{
  /// <summary>
  /// レコードをロックする機能を提供する
  /// </summary>
  /// <remarks></remarks>
  internal interface ILockManager
  {
    bool Lock<TRecord>(long apTranId
                     , SqlBuilder predicate)
    where TRecord: class, IRecord, new();
    bool Lock<TRecord>(long apTranId
                     , TRecord aRecord
                     , IEnumerable<SqlTable> usedTableNames = null) 
    where TRecord: class, IRecord, new();
    int CountLock(long apTranId);
    void UnLock(long apTranId);
  }
}
