
namespace MiniSqlParser
{
  /// <summary>
  /// テーブル名とその別名
  /// </summary>
  [System.Diagnostics.DebuggerDisplay("{Name} AS {AliasName}")]
  public class SqlTable
  {
    private readonly Table _table;
    private int _hashCode = 0;

    internal Table Table {
      get {
        return _table;
      }
    }

    public string Name {
      get {
        return _table.Name;
      }
    }

    /// <summary>
    /// コメント別名も含めて別名を取得する
    /// </summary>
    public string AliasName {
      get {
        return _table.GetAliasOrTableName2();
      }
    }

    /// <summary>
    /// コメント別名を含めずに別名を取得する
    /// </summary>
    public string ExplicitAliasName {
      get {
        return _table.GetAliasOrTableName();
      }
    }

    public override bool Equals(object obj) {
      if(obj == null || obj.GetType() != typeof(SqlTable)) {
        return false;
      }
      var table = (SqlTable)obj;
      return _table.Name              == table._table.Name &&
             _table.AliasName         == table._table.AliasName &&
             _table.ImplicitAliasName == table._table.ImplicitAliasName &&
             _table.ServerName        == table._table.ServerName &&
             _table.DataBaseName      == table._table.DataBaseName &&
             _table.SchemaName        == table._table.SchemaName;
    }

    public override int GetHashCode() {
      if(_hashCode == 0) {
        var seedStr = _table.Name + " " +
                      _table.AliasName + " " +
                      _table.ImplicitAliasName + " " +
                      _table.ServerName + " " +
                      _table.DataBaseName + " " +
                      _table.SchemaName;
        _hashCode = seedStr.GetHashCode();
      }
      return _hashCode;
    }

    public SqlTable(string name, string aliasName) {
      _table = new Table(name, false, aliasName);
    }

    public SqlTable(Table table) {
      _table = table;
    }

    public static bool operator==(SqlTable lSqlTable, SqlTable rSqlTable){
      if((object)lSqlTable == null) {
        return (object)rSqlTable == null;
      }
      return lSqlTable.Equals(rSqlTable);
    }

    public static bool operator !=(SqlTable lSqlTable, SqlTable rSqlTable) {
      if((object)lSqlTable == null) {
        return (object)rSqlTable != null;
      }
      return !lSqlTable.Equals(rSqlTable);
    }
  }

}
