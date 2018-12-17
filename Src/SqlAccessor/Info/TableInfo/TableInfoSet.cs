using System.Collections.Generic;

namespace SqlAccessor
{
  /// <summary>
  /// 全てのテーブルのメタ情報を表す
  /// </summary>
  /// <remarks>スレッドセーフ</remarks>
  internal class TableInfoSet
  {
    //TableInfoオブジェクトを集約する
    //(テーブル名、TableInfo)
    private readonly Dictionary<string, TableInfo> _tableInfoHash = new Dictionary<string, TableInfo>();
    private readonly Db _aDb;

    public TableInfoSet(Db aDb) {
      _aDb = aDb;
    }

    public TableInfo this[string tableName] {
      get {
        //_tableInfoHashへのデータ有無の判断と書込み読込みを不可分処理とする
        lock(_tableInfoHash) {
          if(!_tableInfoHash.ContainsKey(tableName)) {
            //テーブルカラムのメタ情報は、そのカラムが属すテーブルの全てのカラムを一括取得する
            TableInfo newTableInfo = null;
            try {
              newTableInfo = new TableInfo(_aDb, tableName);
            } catch(System.ArgumentOutOfRangeException ex) {
              //引数で指定された名称のテーブルが存在しない場合、nullを登録する
              newTableInfo = null;
            }
            _tableInfoHash.Add(tableName, newTableInfo);
          }

          return _tableInfoHash[tableName];
        }
      }
    }

  }
}
