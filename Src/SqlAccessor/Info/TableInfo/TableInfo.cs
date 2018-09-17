using System.Collections.Generic;
using System.Collections.ObjectModel;
using MiniSqlParser;

namespace SqlAccessor
{

  /// <summary>
  /// テーブルのメタ情報を表す
  /// </summary>
  /// <remarks>条件付スレッドセーフ
  /// Item()が返すColumnInfoオブジェクトが変更されない限りスレッドセーフである</remarks>
  [System.Diagnostics.DebuggerDisplay("TableName:{Name}")]
  internal class TableInfo
  {
    //ColumnInfoオブジェクトを集約する
    //(カラム名、ColumnInfo)
    private readonly Dictionary<string, ColumnInfo> _columnInfoHash = new Dictionary<string, ColumnInfo>();
    //テーブルの全ての主キー項目
    private readonly List<ColumnInfo> _primaryKeys = new List<ColumnInfo>();

    private readonly Db _aDb;
    private readonly string _tableName;

    public TableInfo(Db aDb, string tableName) {
      _aDb = aDb;
      _tableName = tableName;
      //テーブルの全てのカラムのメタ情報を一括取得する
      if(this.RegistColumnInfo(aDb.Dbms, _tableName) == false) {
        //引数で指定された名称のテーブルが存在しない場合、例外を送出する
        throw new System.ArgumentOutOfRangeException("tableName", "指定された名称のテーブルが存在しません");
      }
    }

    private bool RegistColumnInfo(SqlBuilder.DbmsType dbms, string tableName) {
      //引数で指定された名称のテーブルが存在すれば(カラムのメタ情報が1件以上存在すれば)True
      bool tableExists = false;

      Query<ColumnInfo> aQuery = new Query<ColumnInfo>();
      Dictionary<string, string> aPlaceHolders = new Dictionary<string, string>();
      if(dbms == SqlBuilder.DbmsType.Sqlite) {
        //For SQLite
        aPlaceHolders.Add("SQLite_TableName_", tableName);
      } else {
        aQuery.And(val.of("TableName") == tableName);
      }

      using(Tran aTran = _aDb.CreateTranWithoutLock()) {
        //テーブルの全てのカラムのメタ情報を取得し、_columnInfoHashに格納する
        Reader<ColumnInfo> reader = aTran.Find(aQuery
                                              , aPlaceHolders
                                              , Tran.LoadMode.ReadOnly
                                              , Tran.CacheStrategy.NoCache);
        foreach(ColumnInfo aColumnInfo in reader) {
          //SQLiteのPRAGMA TABLE_INFO()の結果にテーブル名が含まれないので、ここで補う
          if(dbms == SqlBuilder.DbmsType.Sqlite) {
            aColumnInfo.TableName = tableName;
          }

          _columnInfoHash.Add(aColumnInfo.ColumnName, aColumnInfo);
          //主キーカラムの場合は_primaryKeysにも登録する
          if(aColumnInfo.PrimaryKey.HasValue && aColumnInfo.PrimaryKey.Value) {
            _primaryKeys.Add(aColumnInfo);
          }
          tableExists = true;
        }
      }

      //主キーカラムのないテーブルは、全てのカラムが主キーと見做す
      if(tableExists && _primaryKeys.Count == 0) {
        _primaryKeys.AddRange(_columnInfoHash.Values);
      }

      return tableExists;
    }

    public ColumnInfo this[string columnName] {
      get {
        if(!_columnInfoHash.ContainsKey(columnName)) {
          return null;
        }

        return _columnInfoHash[columnName];
      }
    }

    public ReadOnlyCollection<ColumnInfo> GetPrimaryKeys() {
      return new ReadOnlyCollection<ColumnInfo>(_primaryKeys);
    }

    public ReadOnlyCollection<ColumnInfo> GetAllColumns() {
      List<ColumnInfo> ret = new List<ColumnInfo>(_columnInfoHash.Values);
      return new ReadOnlyCollection<ColumnInfo>(ret);
    }

  }
}
