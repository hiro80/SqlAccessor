using System;
using System.Collections.Generic;

namespace SqlAccessor
{
  /// <summary>
  /// RecordViewTableMapオブジェクトを生成するFactory
  /// </summary>
  /// <remarks>
  /// RecordViewTableMapオブジェクトを生成するには、RecordViewTableMap.GetInstance()に
  /// Dbオブジェクトを引数として渡す必要があり、RecordViewTableMapを生成するクラスが
  /// Dbクラスと関連を持ってしまう。Dbオブジェクトを保持するFactoryクラスを
  /// 経由させることでこの結びつきを解消する。
  /// </remarks>
  internal class RecordViewTableMapFactory
  {
    //(TRecord名、RecordViewTableMap<TRecord>)
    private readonly Dictionary<string, IRecordViewTableMap> _mapHash = new Dictionary<string, IRecordViewTableMap>();
    private readonly Db _aDb;

    public RecordViewTableMapFactory(Db aDb) {
      _aDb = aDb;
    }

    public RecordViewTableMap<TRecord> CreateRecordViewTableMap<TRecord>()
    where TRecord: class, IRecord, new() {
      var recordName = RecordInfo<TRecord>.GetInstance().Name;

      // _mapHashへのデータ有無の判断と書込み読込みを不可分処理とする
      if(!_mapHash.ContainsKey(recordName)) {
        lock(_mapHash) {
          if(!_mapHash.ContainsKey(recordName)) {
            var newMap = new RecordViewTableMap<TRecord>(_aDb);
            _mapHash.Add(recordName, newMap);
          }
        }
      }

      return (RecordViewTableMap<TRecord>)_mapHash[recordName];
    }

    //public RecordViewTableMap<TRecord> CreateRecordViewTableMap<TRecord>()
    //where TRecord: class, IRecord, new() {
    //  //IF文のキャッシュの指定は暫定的にとりあえずUseCache
    //  return RecordViewTableMap<TRecord>.GetInstance(_aDb);
    //}

  }
}
