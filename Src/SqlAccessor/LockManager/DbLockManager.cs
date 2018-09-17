using System.Collections.Generic;

namespace SqlAccessor
{
  /// <summary>
  /// ロック情報をデータベースに格納するロックマネージャ
  /// </summary>
  /// <remarks>スレッドセーフ</remarks>
  internal class DbLockManager: LockManager
  {
    //LockDataの格納先データベース
    private readonly Db _aDb;
    //Predicate文字列を格納するテーブル項目の最大文字数

    private readonly int _maxLength = 7168;
    public DbLockManager(RecordViewTableMapFactory aRecordViewTableMapFactory, Db aDb)
      : base(aRecordViewTableMapFactory, aDb.GetCaster()) {
      _aDb = aDb;
    }

    protected override IEnumerable<LockData> FindCandidateSubsumptionLockData(long apTranId
                                                                            , string tableName) {
      //LockDataへのアクセスによるLockDataのアクセスを排除する
      //(LockDataに対するアクセスの場合、APトランザクションIDは-1である(暫定設計))
      if(apTranId == -1) {
        return new NullReader<LockData>();
      }

      Query<LockData> aQuery = new Query<LockData>();
      aQuery.And(val.of("ApTranId") != apTranId & val.of("TableName") == tableName);
      return _aDb.Find<LockData>(aQuery, null, Tran.LoadMode.ReadOnly);
    }

    protected override int CountLockData(long apTranId) {
      //LockDataへのアクセスによるLockDataのアクセスを排除する
      //(LockDataに対するアクセスの場合、APトランザクションIDは-1である(暫定設計))
      if(apTranId == -1) {
        return 0;
      }

      Query<LockData> aQuery = new Query<LockData>();
      aQuery.And(val.of("ApTranId") == apTranId);
      return _aDb.Count<LockData>(aQuery);
    }

    protected override void SaveLockData(LockManager.LockData aLockData) {
      //LockDataへのアクセスによるLockDataのアクセスを排除する
      //(LockDataに対するアクセスの場合、APトランザクションIDは-1である(暫定設計))
      if(aLockData.ApTranId == -1) {
        return;
      }

      //aLockData.PredicateStrがテーブル項目より大きいサイズであれば、途中でカットする
      if(aLockData.PredicateStr.Length > _maxLength) {
        aLockData.PredicateStr = aLockData.Predicate.ToString(_maxLength);
      }

      using(Tran aTran = _aDb.CreateTranWithoutLock()) {
        aTran.Save<LockData>(aLockData);
      }
    }

    protected override void DeleteLockData(long apTranId) {
      //LockDataへのアクセスによるLockDataのアクセスを排除する
      //(LockDataに対するアクセスの場合、APトランザクションIDは-1である(暫定設計))
      if(apTranId == -1) {
        return;
      }

      //削除対象レコードの抽出条件を作成する
      LockData criteria = new LockData();
      criteria.ApTranId = apTranId;

      using(Tran aTran = _aDb.CreateTranWithoutLock()) {
        aTran.Delete<LockData>(criteria);
      }
    }
  }
}
