using System.ComponentModel;

namespace SqlAccessor
{
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class StringSqlLiteralType: SqlLiteralType
  {
    private readonly bool _isNString;
    public StringSqlLiteralType(ICastEditor castEditor, bool isNString = false)
      : base(castEditor) {
      _isNString = isNString;
    }
    internal override string CastFrom(PropertyType propertyType
                                    , ColumnInfo aColumnInfo
                                    , object propertyValue) {
      //プロパティ値を、Cast前編集する
      object value = _castEditor.BeforeCast_SqlLiteralType(this, aColumnInfo, propertyValue);
      //プロパティ型から、Sqlリテラル型にキャストする
      string castedValue = propertyType.CastTo(this, aColumnInfo, value);
      //Nストリングの場合は先頭に"N"を付加する
      if(_isNString) {
        castedValue = "N" + castedValue;
      }
      //SQLリテラル値を、Cast後編集する
      return _castEditor.AfterCast_SqlLiteralType(this, aColumnInfo, castedValue);
    }
    internal override string DefaultValue {
      //SQL文字列リテラルの初期値を返す
      //(OracleではNULLと空文字は同じに扱われることに注意)
      get { return "' '"; }
    }
  }
}
namespace SqlAccessor
{
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class NumberSqlLiteralType: SqlLiteralType
  {
    public NumberSqlLiteralType(ICastEditor castEditor)
      : base(castEditor) {
    }
    internal override string CastFrom(PropertyType propertyType
                                    , ColumnInfo aColumnInfo
                                    , object propertyValue) {
      object value = _castEditor.BeforeCast_SqlLiteralType(this, aColumnInfo, propertyValue);
      string castedValue = propertyType.CastTo(this, aColumnInfo, value);
      return _castEditor.AfterCast_SqlLiteralType(this, aColumnInfo, castedValue);
    }
    internal override string DefaultValue {
      get { return "0"; }
    }
  }
}

namespace SqlAccessor
{
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class DateTimeSqlLiteralType: SqlLiteralType
  {
    public DateTimeSqlLiteralType(ICastEditor castEditor)
      : base(castEditor) {
    }
    internal override string CastFrom(PropertyType propertyType
                                    , ColumnInfo aColumnInfo
                                    , object propertyValue) {
      object value = _castEditor.BeforeCast_SqlLiteralType(this, aColumnInfo, propertyValue);
      string castedValue = propertyType.CastTo(this, aColumnInfo, value);
      return _castEditor.AfterCast_SqlLiteralType(this, aColumnInfo, castedValue);
    }
    internal override string DefaultValue {
      get { return "'0001-01-01 00:00:00'"; }
    }
  }
}

namespace SqlAccessor
{
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class DoubleSqlLiteralType: SqlLiteralType
  {
    public DoubleSqlLiteralType(ICastEditor castEditor)
      : base(castEditor) {
    }
    internal override string CastFrom(PropertyType propertyType
                                    , ColumnInfo aColumnInfo
                                    , object propertyValue) {
      object value = _castEditor.BeforeCast_SqlLiteralType(this, aColumnInfo, propertyValue);
      string castedValue = propertyType.CastTo(this, aColumnInfo, value);
      return _castEditor.AfterCast_SqlLiteralType(this, aColumnInfo, castedValue);
    }
    internal override string DefaultValue {
      get { return "0"; }
    }
  }
}

namespace SqlAccessor
{
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class IntervalSqlLiteralType: SqlLiteralType
  {
    public IntervalSqlLiteralType(ICastEditor castEditor)
      : base(castEditor) {
    }
    internal override string CastFrom(PropertyType propertyType
                                    , ColumnInfo aColumnInfo
                                    , object propertyValue) {
      object value = _castEditor.BeforeCast_SqlLiteralType(this, aColumnInfo, propertyValue);
      string castedValue = propertyType.CastTo(this, aColumnInfo, value);
      return _castEditor.AfterCast_SqlLiteralType(this, aColumnInfo, castedValue);
    }
    internal override string DefaultValue {
      get { return "INTERVAL '0' DAY"; }
    }
  }
}

namespace SqlAccessor
{

  /// <summary>
  /// Oracleの日付時刻リテラル
  /// </summary>
  /// <remarks>
  /// Oracleの日付と時刻のリテラル表記は、TIMESTAMP '1945-08-15 12:00:00'となる
  /// Oracleだけ他DBMSと異なるので、Oracle専用のSqlLiteralTypeクラスを用意する
  /// </remarks>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class OracleDateTimeSqlLiteralType: SqlLiteralType
  {
    public OracleDateTimeSqlLiteralType(ICastEditor castEditor)
      : base(castEditor) {
    }
    internal override string CastFrom(PropertyType propertyType
                                    , ColumnInfo aColumnInfo
                                    , object propertyValue) {
      object value = _castEditor.BeforeCast_SqlLiteralType(this, aColumnInfo, propertyValue);
      string castedValue = propertyType.CastTo(this, aColumnInfo, value);
      return _castEditor.AfterCast_SqlLiteralType(this, aColumnInfo, castedValue);
    }
    internal override string DefaultValue {
      get { return "TIMESTAMP '0001-01-01 00:00:00'"; }
    }
  }
}
