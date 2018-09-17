using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using MiniSqlParser;

namespace SqlAccessor
{
  /// <summary>
  /// 抽出したレコードにロックをかける機能を、RecordReaderオブジェクトに付加する
  /// </summary>
  /// <typeparam name="TRecord"></typeparam>
  /// <remarks>Decoratorパターン</remarks>
  internal class ReadWriteRecordReader<TRecord>: RecordReader<TRecord>, IRecordReader<TRecord> where TRecord: class, IRecord, new()
  {
    private readonly IRecordReaderImp<TRecord> _aRecordReaderImp;
    //ロックマネージャ
    private readonly ILockManager _aLockManager;
    //APトランザクションID
    private readonly long _apTranId;
    //データ抽出元テーブルの別名とその先頭主キー
    //(テーブル別名、(テーブル名、先頭主キー項目名))
    private readonly Dictionary<SqlTable, string> _tableAndAliasNames;
    //現在イテレートしているレコードのロックの有無 
    private bool _writable;
    //現在イテレートしているレコード
    private TRecord _currentRecord;

    public ReadWriteRecordReader(IRecordReaderImp<TRecord> aRecordReaderImp
                               , ILockManager aLockManager
                               , long apTranId
                               , Dictionary<SqlTable, string> tableAndAliasNames) {
      _aRecordReaderImp = aRecordReaderImp;
      _aLockManager = aLockManager;
      _apTranId = apTranId;
      _tableAndAliasNames = tableAndAliasNames;
    }

    public override void Dispose() {
      _aRecordReaderImp.Dispose();
    }

    /// <summary>
    /// SELECT文の外部結合により抽出されなかったテーブル行を除外した、抽出元テーブル別名を取得する
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// GetSelectItemInfoVisitorにより、全ての抽出元テーブルの主キーを
    /// メインクエリのSELECT句に並べていることが前提である
    /// </remarks>
    private List<SqlTable> MakeUsedTables() {
      List<SqlTable> ret = new List<SqlTable>();

      foreach(KeyValuePair<SqlTable, string> tableNameAndKey in _tableAndAliasNames) {
        string firstPk = tableNameAndKey.Value;
        if(!(_aRecordReaderImp.GetValueOf(firstPk) is System.DBNull)) {
          ret.Add(tableNameAndKey.Key);
        }
      }

      return ret;
    }

    public override TRecord Current {
      get { return _currentRecord; }
    }

    public override bool MoveNext() {
      bool nextExists = _aRecordReaderImp.MoveNext();
      if(nextExists) {
        // 次のレコードが存在すれば、LockManagerにロックの有無を問い合わせる
        _currentRecord = _aRecordReaderImp.Current;
        _writable = _aLockManager.Lock(_apTranId
                                     , _currentRecord
                                     , this.MakeUsedTables());
      } else {
        _currentRecord = null;
        _writable = false;
      }

      return nextExists;
    }

    public override void Reset() {
      _aRecordReaderImp.Reset();
    }

    public override IEnumerator<TRecord> GetEnumerator() {
      return this;
    }

    public override bool Writable {
      get { return _writable; }
    }

  }
}
