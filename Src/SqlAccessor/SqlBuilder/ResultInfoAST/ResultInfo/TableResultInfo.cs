using System;
using System.Collections;
using System.Collections.Generic;

namespace MiniSqlParser
{
  [System.Diagnostics.DebuggerDisplay(
    "TableAliasName: {TableAliasName}, ColumnAliasName: {ColumnAliasName}")]
  public class TableResultInfo: IResultInfo, IResultInfoInternal
  {
    public string ServerName { get; private set; }
    public string DataBaseName { get; private set; }
    public string SchemaName { get; private set; }
    public string TableName { get; private set; }

    private string _tableAliasName;
    public string TableAliasName {
      get {
        // テーブル別名が指定されていない場合、テーブル名をテーブル列名と見做す
        if(string.IsNullOrEmpty(_tableAliasName)) {
          return this.TableName;
        } else {
          return _tableAliasName;
        }
      }
      internal set {
        _tableAliasName = value;
      }
    }

    public string ColumnAliasName { get; private set; }

    /// <summary>
    /// Not NullかつUniqueな列はTrue
    /// </summary>
    //public bool IsPrimaryKey {
    //  get {
    //    return !this.IsNullable && this.IsUnique;
    //  }
    //}
    public KeyType KeyType { get; private set; }

    public bool IsNullable { get; private set; }

    public bool IsUnique { get; private set; }

    public bool ExplicitDecl {
      get {
        return true;
      }
    }

    public bool IsComplemented {
      get {
        return false;
      }
    }

    public SqlTable SourceTable { get; internal set; }
    public string SourceColumnName {
      get {
        return this.ColumnAliasName;
      }
    }
    public bool IsDirectSource(Column destItem, bool ignoreCase = true) {
      if(string.Compare(destItem.Name, this.ColumnAliasName, ignoreCase) != 0) {
        return false;

      } else if(string.IsNullOrEmpty(destItem.TableAliasName)) {
        // SELECT句で抽出元テーブル名が省略されている場合、trueを返す
        return true;
      } else if(string.Compare(destItem.TableAliasName, this.TableAliasName, ignoreCase) != 0) {
        // テーブル別名が指定されている場合はテーブル別名を参照する
        return false;

      } else if(string.IsNullOrEmpty(destItem.SchemaName)) {
        // SELECT句でSchemaNameが省略されている場合、trueを返す
        return true;
      } else if(string.IsNullOrEmpty(this.SchemaName)) {
        // SELECT句でSchemaNameが指定され、テーブル名では省略されている場合
        // テーブルのSchemaNameはデフォルトSchemaNameと見做される
        throw new ApplicationException("TableResultInfo.SchemaNameがnullまたは空です");
      } else if(string.Compare(destItem.SchemaName, this.SchemaName, ignoreCase) != 0) {
        return false;

      } else if(string.IsNullOrEmpty(destItem.DataBaseName)) {
        // SELECT句でDataBaseNameが省略されている場合、trueを返す
        return true;
      } else if(string.IsNullOrEmpty(this.DataBaseName)) {
        // SELECT句でDataBaseNameが指定され、テーブル名では省略されている場合
        // テーブルのDataBaseNameはデフォルトDataBaseNameと見做される
        throw new ApplicationException("TableResultInfo.DataBaseNameがnullまたは空です");
      } else if(string.Compare(destItem.DataBaseName, this.DataBaseName, ignoreCase) != 0) {
        return false;

      } else if(string.IsNullOrEmpty(destItem.ServerName)) {
        // SELECT句でServerNameが省略されている場合、trueを返す
        return true;
      } else if(string.IsNullOrEmpty(this.ServerName)) {
        // SELECT句でServerNameが指定され、テーブル名では省略されている場合
        // テーブルのServerNameはデフォルトServerNameと見做される
        throw new ApplicationException("TableResultInfo.ServerNameがnullまたは空です");
      } else if(string.Compare(destItem.ServerName, this.ServerName, ignoreCase) != 0) {
        return false;

      } else {
        // SELECT句とテーブル名がフルスペックの名称指定で、ServerNameからColumnNameまで
        // 一致する場合、tureを返す
        return true;
      }

    }

    public bool IsNullLiteral {
      get {
        // Table列はNULLリテラルではない
        return false;
      }
    }

    public ResultInfoType Type {
      get {
        return ResultInfoType.Table;
      }
    }

    internal bool IsReferenced { get; set; }
    internal bool IsOuterJoined { get; set; }

    public void Accept(IResultInfoVisitor visitor) {
      if(visitor == null) {
        throw new ArgumentNullException("visitor");
      }
      visitor.VisitBefore(this);
      visitor.VisitAfter(this);
    }

    public static bool operator==(TableResultInfo lResultInfo, TableResultInfo rResultInfo){
      return lResultInfo.ColumnAliasName == rResultInfo.ColumnAliasName
          && lResultInfo.TableName == rResultInfo.TableName
          && lResultInfo.SchemaName == rResultInfo.SchemaName
          && lResultInfo.DataBaseName == rResultInfo.DataBaseName
          && lResultInfo.ServerName == rResultInfo.ServerName
          && lResultInfo.IsNullable == rResultInfo.IsNullable
          //&& lResultInfo.IsUnique == rResultInfo.IsUnique
          && lResultInfo.KeyType == rResultInfo.KeyType
          && lResultInfo.TableAliasName == rResultInfo.TableAliasName
          && lResultInfo.IsReferenced == rResultInfo.IsReferenced
          && lResultInfo.SourceTable == rResultInfo.SourceTable
          && lResultInfo.SourceColumnName == rResultInfo.SourceColumnName;
    }

    public static bool operator !=(TableResultInfo lResultInfo, TableResultInfo rResultInfo) {
      return !(lResultInfo == rResultInfo);
    }

    public TableResultInfo Clone() {
      var ret = new TableResultInfo(this.ServerName
                                  , this.DataBaseName
                                  , this.SchemaName
                                  , this.TableName
                                  , this.ColumnAliasName
                                  , this.IsNullable
                                  , this.KeyType == KeyType.Table);
                                  //, this.IsUnique);
      ret.TableAliasName = this.TableAliasName;
      ret.IsReferenced = this.IsReferenced;
      ret.SourceTable = this.SourceTable;
      return ret;
    }

    public TableResultInfo(string serverName
                          , string dataBaseName
                          , string schemaName
                          , string tableName
                          , string columnAliasName
                          , bool isNullable
                          , bool isPrimaryKey) {
                          //, bool isUnique) {
      this.ServerName = serverName;
      this.DataBaseName = dataBaseName;
      this.SchemaName = schemaName;
      this.TableName = tableName;
      this.ColumnAliasName = columnAliasName;
      this.IsNullable = isNullable;
      //this.IsUnique = isUnique;
      if(isPrimaryKey) {
        // Tableの主キー種別にGroupByキーまたはCountキーは指定しない
        this.KeyType = KeyType.Table;
      }
    }

    public TableResultInfo(string tableName
                          , string columnAliasName
                          , bool isNullable
                          , bool isPrimaryKey = false) {
                          //, bool isUnique) {
      this.ServerName = null;
      this.DataBaseName = null;
      this.SchemaName = null;
      this.TableName = tableName;
      this.ColumnAliasName = columnAliasName;
      this.IsNullable = isNullable;
      //this.IsUnique = isUnique;
      if(isPrimaryKey) {
        this.KeyType = KeyType.Table;
      }
    }
  }
}
