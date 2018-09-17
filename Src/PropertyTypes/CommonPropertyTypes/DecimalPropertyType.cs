using System;
using System.Globalization;
using SqlAccessor;

public class DecimalPropertyType: PropertyType
{
  private readonly System.Type _myType = typeof(System.Decimal);

  public DecimalPropertyType(ICastEditor castEditor)
    : base(castEditor) {
  }

  protected override System.Type GetPropertyType() {
    return _myType;
  }

  protected override bool IsNullValue(object propertyValue) {
    return Convert.ToDecimal(propertyValue) == decimal.MinValue;
  }

  protected override object GetNullValue() {
    return decimal.MinValue;
  }

  protected override object CastFromImp(StringViewColumnType viewColumnType, object viewColumnValue) {
    decimal ret = default(decimal);
    if(decimal.TryParse(viewColumnValue.ToString(), out ret)) {
      return ret;
    } else {
      return decimal.MinValue;
    }
  }

  protected override object CastFromImp(IntegerViewColumnType viewColumnType, object viewColumnValue) {
    return Convert.ToDecimal(viewColumnValue);
  }

  protected override object CastFromImp(LongViewColumnType viewColumnType, object viewColumnValue) {
    return Convert.ToDecimal(viewColumnValue);
  }

  protected override object CastFromImp(DecimalViewColumnType viewColumnType, object viewColumnValue) {
    return viewColumnValue;
  }

  protected override object CastFromImp(DoubleViewColumnType viewColumnType, object viewColumnValue) {
    return Convert.ToDecimal(viewColumnValue);
  }

  protected override object CastFromImp(DateTimeViewColumnType viewColumnType, object viewColumnValue) {
    decimal ret = default(decimal);
    System.DateTime d = (System.DateTime)viewColumnValue;
    decimal.TryParse(d.ToString("yyyyMMdd"), out ret);
    return ret;
  }

  protected override object CastFromImp(TimeSpanViewColumnType viewColumnType, object viewColumnValue) {
    return Convert.ToDecimal(((System.TimeSpan)viewColumnValue).Days);
  }

  protected override object CastFromImp(BooleanViewColumnType viewColumnType, object viewColumnValue) {
    if((bool)viewColumnValue) {
      return 1m;
    } else {
      return 0m;
    }
  }

  protected override string CastToImp(StringSqlLiteralType sqlLiteralType, object propertyValue) {
    return "'" + propertyValue.ToString() + "'";
  }

  protected override string CastToImp(NumberSqlLiteralType sqlLiteralType, object propertyValue) {
    return propertyValue.ToString();
  }

  protected override string CastToImp(DateTimeSqlLiteralType sqlLiteralType, object propertyValue) {
    decimal dec = (decimal)propertyValue;
    System.DateTime d = default(System.DateTime);
    System.DateTime.TryParseExact(decimal.ToInt32(dec).ToString()
                                , "yyyyMMdd"
                                , DateTimeFormatInfo.InvariantInfo
                                , DateTimeStyles.None
                                , out d);
    return "'" + d.ToString("yyyy-MM-dd") + "'";
  }

  protected override string CastToImp(OracleDateTimeSqlLiteralType sqlLiteralType, object propertyValue) {
    decimal dec = (decimal)propertyValue;
    System.DateTime d = default(System.DateTime);
    System.DateTime.TryParseExact(decimal.ToInt32(dec).ToString()
                                , "yyyyMMdd"
                                , DateTimeFormatInfo.InvariantInfo
                                , DateTimeStyles.None
                                , out d);
    return "'" + d.ToString("yyyy-MM-dd") + "'";
  }

  protected override string CastToImp(DoubleSqlLiteralType sqlLiteralType, object propertyValue) {
    decimal dec = (decimal)propertyValue;
    return Convert.ToDouble(dec).ToString();
  }

  protected override string CastToImp(IntervalSqlLiteralType sqlLiteralType, object propertyValue) {
    decimal dec = (decimal)propertyValue;
    return "INTERVAL '" + decimal.ToInt32(dec).ToString() + "' DAY";
  }
}
