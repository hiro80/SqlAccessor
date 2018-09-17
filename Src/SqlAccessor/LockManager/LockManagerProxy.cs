using System.Collections.Generic;
using MiniSqlParser;

namespace SqlAccessor
{
  /// <summary>
  /// ロック情報をDBに1件も格納していない場合、
  /// UnLock()で削除処理する必要が無いのでしたくない(余分なDELETE文の発行を控えたい)
  /// この機能をProxyパターンによるLockManagerへのアクセスコントロールで実現する
  /// </summary>
  /// <remarks></remarks>
  internal class LockManagerProxy: ILockManager
  {
    //Proxyパターンで代理されるオブジェクト
    private readonly ILockManager _lockManager;
    //ロック情報を1件以上格納している時、Trueを格納する
    private bool _lockDataInserted;

    public LockManagerProxy(ILockManager lockManager) {
      _lockManager = lockManager;
    }

    public bool Lock<TRecord>(long apTranId
                            , SqlBuilder predicate) 
    where TRecord: class, IRecord, new() {
      if(!_lockManager.Lock<TRecord>(apTranId, predicate)) {
        return false;
      }

      _lockDataInserted = true;
      return true;
    }

    public bool Lock<TRecord>(long apTranId
                            , TRecord aRecord
                            , IEnumerable<SqlTable> usedTableNames = null)
    where TRecord: class, IRecord, new() {
      if(!_lockManager.Lock(apTranId, aRecord, usedTableNames)) {
        return false;
      }

      _lockDataInserted = true;
      return true;
    }

    public int CountLock(long apTranId) {
      return _lockManager.CountLock(apTranId);
    }

    public void UnLock(long apTranId) {
      if(_lockDataInserted) {
        _lockManager.UnLock(apTranId);
      }
    }
  }
}
