using System;
using System.Globalization;
using SqlAccessor;

public class Int32PropertyType: PropertyType
{
  private readonly System.Type _myType = typeof(System.Int32);

  public Int32PropertyType(ICastEditor castEditor)
    : base(castEditor) {
  }

  protected override System.Type GetPropertyType() {
    return _myType;
  }

  protected override bool IsNullValue(object propertyValue) {
    return Convert.ToInt32(propertyValue) == int.MinValue;
  }

  protected override object GetNullValue() {
    return int.MinValue;
  }

  protected override object CastFromImp(StringViewColumnType viewColumnType, object viewColumnValue) {
    int ret = 0;
    if(int.TryParse(viewColumnValue.ToString(), out ret)) {
      return ret;
    } else {
      return int.MinValue;
    }
  }

  protected override object CastFromImp(IntegerViewColumnType viewColumnType, object viewColumnValue) {
    return viewColumnValue;
  }

  protected override object CastFromImp(LongViewColumnType viewColumnType, object viewColumnValue) {
    return Convert.ToInt32(viewColumnValue);
  }

  protected override object CastFromImp(DecimalViewColumnType viewColumnType, object viewColumnValue) {
    return decimal.ToInt32(Convert.ToDecimal(viewColumnValue));
  }

  protected override object CastFromImp(DoubleViewColumnType viewColumnType, object viewColumnValue) {
    return Convert.ToInt32(viewColumnValue);
  }

  protected override object CastFromImp(DateTimeViewColumnType viewColumnType, object viewColumnValue) {
    int ret = 0;
    System.DateTime d = (System.DateTime)viewColumnValue;
    int.TryParse(d.ToString("yyyyMMdd"), out ret);
    return ret;
  }

  protected override object CastFromImp(TimeSpanViewColumnType viewColumnType, object viewColumnValue) {
    return ((System.TimeSpan)viewColumnValue).Days;
  }

  protected override object CastFromImp(BooleanViewColumnType viewColumnType, object viewColumnValue) {
    if((bool)viewColumnValue) {
      return 1;
    } else {
      return 0;
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
                                , "yyyyMMdd"
                                , DateTimeFormatInfo.InvariantInfo
                                , DateTimeStyles.None, out d);
    return "'" + d.ToString("yyyy-MM-dd") + "'";
  }

  protected override string CastToImp(OracleDateTimeSqlLiteralType sqlLiteralType, object propertyValue) {
    System.DateTime d = default(System.DateTime);
    System.DateTime.TryParseExact(propertyValue.ToString()
                                , "yyyyMMdd"
                                , DateTimeFormatInfo.InvariantInfo
                                , DateTimeStyles.None, out d);
    return "'" + d.ToString("yyyy-MM-dd") + "'";
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
