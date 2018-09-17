using System;

namespace SqlAccessor
{
  /// <summary>
  /// テーブルカラムのメタ情報を表す
  /// </summary>
  /// <remarks></remarks>
  [Serializable()]
  [System.Diagnostics.DebuggerDisplay("Table:{TableName} Column:{ColumnName} Type:{DataType}")]
  public class ColumnInfo: IRecord
  {
    //オーナー名
    private string _owner;
    //テーブル名
    private string _tableName;
    //列名
    private string _columnName;
    //型
    private string _dataType;
    //主キーか否か
    private Nullable<bool> _primaryKey;
    //NotNullか否か
    private Nullable<bool> _nullable;
    //初期値
    private string _defaultValue;
    //最大桁数
    private Nullable<int> _maxLength;
    //最小桁数
    private Nullable<int> _minLength;

    public string Owner {
      get { return _owner; }
      set { _owner = value; }
    }

    public string TableName {
      get { return _tableName; }
      set { _tableName = value; }
    }

    public string ColumnName {
      get { return _columnName; }
      set { _columnName = value; }
    }

    public string DataType {
      get { return _dataType; }
      set { _dataType = value; }
    }

    public Nullable<bool> PrimaryKey {
      get { return _primaryKey; }
      set { _primaryKey = value; }
    }

    public Nullable<bool> Nullable {
      get { return _nullable; }
      set { _nullable = value; }
    }

    public string DefaultValue {
      get { return _defaultValue; }
      set { _defaultValue = value; }
    }

    public Nullable<int> MaxLength {
      get { return _maxLength; }
      set { _maxLength = value; }
    }

    public Nullable<int> MinLength {
      get { return _minLength; }
      set { _minLength = value; }
    }

    #region "For SQLite"
    public string name {
      get { return _columnName; }
      set { _columnName = value; }
    }

    public string type {
      get { return _dataType; }
      set { _dataType = value; }
    }

    public Nullable<bool> notnull {
      get { return _nullable; }
      set { _nullable = !value; }
    }

    public string dflt_value {
      get { return _defaultValue; }
      set { _defaultValue = value; }
    }

    public Nullable<bool> pk {
      get { return _primaryKey; }
      set { _primaryKey = value; }
    }
    #endregion

    public void Clear() {
      _owner = null;
      _tableName = null;
      _columnName = null;
      _dataType = null;
      _primaryKey = null;
      _nullable = null;
      _defaultValue = null;
      _maxLength = null;
      _minLength = null;
    }
  }
}
