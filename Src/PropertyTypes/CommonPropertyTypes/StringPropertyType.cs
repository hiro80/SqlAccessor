using System;
using System.Globalization;
using SqlAccessor;

public class StringPropertyType: PropertyType
{
  private readonly System.Type _myType = typeof(System.String);

  public StringPropertyType(ICastEditor castEditor)
    : base(castEditor) {
  }

  protected override System.Type GetPropertyType() {
    return _myType;
  }

  protected override bool IsNullValue(object propertyValue) {
    return string.IsNullOrEmpty((string)propertyValue);
  }

  protected override object GetNullValue() {
    return "";
  }

  protected override object CastFromImp(StringViewColumnType viewColumnType, object viewColumnValue) {
    return viewColumnValue;
  }

  protected override object CastFromImp(IntegerViewColumnType viewColumnType, object viewColumnValue) {
    return viewColumnValue.ToString();
  }

  protected override object CastFromImp(LongViewColumnType viewColumnType, object viewColumnValue) {
    return viewColumnValue.ToString();
  }

  protected override object CastFromImp(DecimalViewColumnType viewColumnType, object viewColumnValue) {
    return viewColumnValue.ToString();
  }

  protected override object CastFromImp(DoubleViewColumnType viewColumnType, object viewColumnValue) {
    return viewColumnValue.ToString();
  }

  protected override object CastFromImp(DateTimeViewColumnType viewColumnType, object viewColumnValue) {
    return viewColumnValue.ToString();
  }

  protected override object CastFromImp(TimeSpanViewColumnType viewColumnType, object viewColumnValue) {
    return viewColumnValue.ToString();
  }

  protected override object CastFromImp(BooleanViewColumnType viewColumnType, object viewColumnValue) {
    return viewColumnValue.ToString();
  }

  protected override string CastToImp(StringSqlLiteralType sqlLiteralType, object propertyValue) {
    // 文字列値内の"'"をエスケープする
    return "'" + propertyValue.ToString().Replace("'", "''") + "'";
  }

  protected override string CastToImp(NumberSqlLiteralType sqlLiteralType, object propertyValue) {
    long ret = 0;
    //数値に変換できない場合は0を返す
    long.TryParse(propertyValue.ToString(), out ret);
    return ret.ToString();
  }

  protected override string CastToImp(DateTimeSqlLiteralType sqlLiteralType, object propertyValue) {
    System.DateTime d = default(System.DateTime);
    System.DateTime.TryParseExact(propertyValue.ToString()
                                , new []{"yyyyMMdd","yyyy/MM/dd"}
                                , DateTimeFormatInfo.InvariantInfo
                                , DateTimeStyles.None
                                , out d);
    return "'" + d.ToString("yyyy-MM-dd") + "'";
  }

  protected override string CastToImp(OracleDateTimeSqlLiteralType sqlLiteralType, object propertyValue) {
    System.DateTime d = default(System.DateTime);
    System.DateTime.TryParseExact(propertyValue.ToString()
                                , new []{"yyyyMMdd","yyyy/MM/dd"}
                                , DateTimeFormatInfo.InvariantInfo
                                , DateTimeStyles.None
                                , out d);
    return "'" + d.ToString("yyyy-MM-dd") + "'";
  }

  protected override string CastToImp(DoubleSqlLiteralType sqlLiteralType, object propertyValue) {
    double d = 0;
    double.TryParse(propertyValue.ToString(), out d);
    return d.ToString();
  }

  protected override string CastToImp(IntervalSqlLiteralType sqlLiteralType, object propertyValue) {
    throw new InvalidColumnToPropertyCastException("未定義な型変換です");
  }

}
