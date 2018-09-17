using System;
using System.Globalization;
using SqlAccessor;

public class Int16PropertyType: PropertyType
{
  private readonly System.Type _myType = typeof(System.Int16);

  public Int16PropertyType(ICastEditor castEditor)
    : base(castEditor) {
  }

  protected override System.Type GetPropertyType() {
    return _myType;
  }

  protected override bool IsNullValue(object propertyValue) {
    return Convert.ToInt16(propertyValue) == short.MinValue;
  }

  protected override object GetNullValue() {
    return short.MinValue;
  }

  protected override object CastFromImp(StringViewColumnType viewColumnType, object viewColumnValue) {
    short ret = 0;
    if(short.TryParse(viewColumnValue.ToString(), out ret)) {
      return ret;
    } else {
      return short.MinValue;
    }
  }

  protected override object CastFromImp(IntegerViewColumnType viewColumnType, object viewColumnValue) {
    return Convert.ToInt16(viewColumnValue);
  }

  protected override object CastFromImp(LongViewColumnType viewColumnType, object viewColumnValue) {
    return Convert.ToInt16(viewColumnValue);
  }

  protected override object CastFromImp(DecimalViewColumnType viewColumnType, object viewColumnValue) {
    return decimal.ToInt16(Convert.ToDecimal(viewColumnValue));
  }

  protected override object CastFromImp(DoubleViewColumnType viewColumnType, object viewColumnValue) {
    return Convert.ToInt16(viewColumnValue);
  }

  protected override object CastFromImp(DateTimeViewColumnType viewColumnType, object viewColumnValue) {
    short ret = 0;
    System.DateTime d = (System.DateTime)viewColumnValue;
    short.TryParse(d.ToString("yyyy"), out ret);
    return ret;
  }

  protected override object CastFromImp(TimeSpanViewColumnType viewColumnType, object viewColumnValue) {
    return Convert.ToInt16(((System.TimeSpan)viewColumnValue).Days);
  }

  protected override object CastFromImp(BooleanViewColumnType viewColumnType, object viewColumnValue) {
    if((bool)viewColumnValue) {
      return (short)1;
    } else {
      return (short)0;
    }
  }

  protected override string CastToImp(StringSqlLiteralType sqlLiteralType, object propertyValue) {
    return "'" + propertyValue.ToString() + "'";
  }

  protected override string CastToImp(NumberSqlLiteralType sqlLiteralType, object propertyValue) {
    return propertyValue.ToString();
  }

  protected override string CastToImp(DateTimeSqlLiteralType sqlLiteralType, object propertyValue) {
    System.DateTime d = default(System.DateTime);
    System.DateTime.TryParseExact(propertyValue.ToString()
                                , "yyyy"
                                , DateTimeFormatInfo.InvariantInfo
                                , DateTimeStyles.None, out d);
    return "'" + d.ToString("yyyy-01-01") + "'";
  }

  protected override string CastToImp(OracleDateTimeSqlLiteralType sqlLiteralType, object propertyValue) {
    System.DateTime d = default(System.DateTime);
    System.DateTime.TryParseExact(propertyValue.ToString()
                                , "yyyy"
                                , DateTimeFormatInfo.InvariantInfo
                                , DateTimeStyles.None, out d);
    return "'" + d.ToString("yyyy-01-01") + "'";
  }

  protected override string CastToImp(DoubleSqlLiteralType sqlLiteralType, object propertyValue) {
    double d = 0;
    double.TryParse(propertyValue.ToString(), out d);
    return d.ToString();
  }

  protected override string CastToImp(IntervalSqlLiteralType sqlLiteralType, object propertyValue) {
    return "INTERVAL '" + propertyValue.ToString() + "' DAY";
  }
}
