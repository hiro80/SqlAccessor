namespace SqlAccessor
{
  /// <summary>
  /// RecordViewTableMapオブジェクトを生成するFactory
  /// </summary>
  /// <remarks>
  /// RecordViewTableMapオブジェクトを生成するには、RecordViewTableMap.GetInstance()に
  /// Casterオブジェクトを引数として渡す必要があり、RecordViewTableMapを生成するクラスが
  /// Casterクラスと関連を持ってしまう。Casterオブジェクトを保持するFactoryクラスを
  /// 経由させることでこの結びつきを解消し、またTableInfoSetオブジェクトを保持する。
  /// </remarks>
  internal class RecordViewTableMapFactory
  {
    private readonly TableInfoSet _aTableInfoSet;
    private readonly ICaster _aCaster;
    private readonly SqlPodFactory _aSqlPodFactory;

    private readonly Db _aDb;
    public RecordViewTableMapFactory(Db aDb) {
      _aTableInfoSet = aDb.GetTableInfoSet();
      _aCaster = aDb.GetCaster();
      _aSqlPodFactory = aDb.GetSqlPodFactory();
      _aDb = aDb;
    }

    public RecordViewTableMap<TRecord> CreateRecordViewTableMap<TRecord>()
    where TRecord: class, IRecord, new() {
      //IF文のキャッシュの指定は暫定的にとりあえずUseCache
      return RecordViewTableMap<TRecord>.GetInstance(_aTableInfoSet
                                                  , _aCaster
                                                  , _aSqlPodFactory
                                                  , Tran.CacheStrategy.UseCache
                                                  , _aDb);
    }
  }
}
