using System;
using System.Globalization;
using SqlAccessor;

public class DateTimePropertyType: PropertyType
{
  private readonly System.Type _myType = typeof(System.DateTime);
  // 0001/01/01 00:00:00 を表す
  private readonly System.DateTime _nullDate = System.DateTime.MinValue;

  public DateTimePropertyType(ICastEditor castEditor)
    : base(castEditor) {
  }

  protected override System.Type GetPropertyType() {
    return _myType;
  }

  protected override bool IsNullValue(object propertyValue) {
    return propertyValue == null || (System.DateTime)propertyValue == _nullDate;
  }

  protected override object GetNullValue() {
    return null;
  }

  protected override object CastFromImp(StringViewColumnType viewColumnType, object viewColumnValue) {
    string str = (string)viewColumnValue;
    System.DateTime d = default(System.DateTime);
    if(System.DateTime.TryParseExact(str
                                    , new[] { "yyyyMMdd", "yyyy-MM-dd", "yyyy/MM/dd" }
                                    , null
                                    , DateTimeStyles.AssumeLocal
                                    , out d)) {
      return d;
    } else {
      return _nullDate;
    }
  }

  protected override object CastFromImp(IntegerViewColumnType viewColumnType, object viewColumnValue) {
    int i = (int)viewColumnValue;
    System.DateTime d = default(System.DateTime);
    if(System.DateTime.TryParseExact(i.ToString()
                                    , new[] { "yyyyMMdd" }
                                    , null
                                    , DateTimeStyles.AssumeLocal
                                    , out d)) {
      return d;
    } else {
      return _nullDate;
    }
  }

  protected override object CastFromImp(LongViewColumnType viewColumnType, object viewColumnValue) {
    long l = (long)viewColumnValue;
    System.DateTime d = default(System.DateTime);
    if(System.DateTime.TryParseExact(l.ToString()
                                    , new[] { "yyyyMMdd" }
                                    , null
                                    , DateTimeStyles.AssumeLocal
                                    , out d)) {
      return d;
    } else {
      return _nullDate;
    }
  }

  protected override object CastFromImp(DecimalViewColumnType viewColumnType, object viewColumnValue) {
    decimal dec = (decimal)viewColumnValue;
    System.DateTime d = default(System.DateTime);
    if(System.DateTime.TryParseExact(dec.ToString()
                                    , new[] { "yyyyMMdd" }
                                    , null
                                    , DateTimeStyles.AssumeLocal
                                    , out d)) {
      return d;
    } else {
      return _nullDate;
    }
  }

  protected override object CastFromImp(DoubleViewColumnType viewColumnType, object viewColumnValue) {
    int i = Convert.ToInt32(viewColumnValue);
    System.DateTime d = default(System.DateTime);
    if(System.DateTime.TryParseExact(i.ToString()
                                    , new[] { "yyyyMMdd" }
                                    , null
                                    , DateTimeStyles.AssumeLocal
                                    , out d)) {
      return d;
    } else {
      return _nullDate;
    }
  }

  protected override object CastFromImp(DateTimeViewColumnType viewColumnType, object viewColumnValue) {
    return (System.DateTime)viewColumnValue;
  }

  protected override object CastFromImp(TimeSpanViewColumnType viewColumnType, object viewColumnValue) {
    System.TimeSpan span = (System.TimeSpan)viewColumnValue;
    return (new System.DateTime(1, 1, 1)).AddDays(span.Days);
  }

  protected override object CastFromImp(BooleanViewColumnType viewColumnType, object viewColumnValue) {
    if((bool)viewColumnValue) {
      return new System.DateTime(1, 1, 2);
    } else {
      return new System.DateTime(1, 1, 1);
    }
  }

  protected override string CastToImp(StringSqlLiteralType sqlLiteralType, object propertyValue) {
    System.DateTime d = (System.DateTime)propertyValue;
    return "'" + d.ToString("yyyy-MM-dd") + "'";
  }

  protected override string CastToImp(NumberSqlLiteralType sqlLiteralType, object propertyValue) {
    System.DateTime d = (System.DateTime)propertyValue;
    return d.ToString("yyyyMMdd").TrimStart('0');
  }

  protected override string CastToImp(DateTimeSqlLiteralType sqlLiteralType, object propertyValue) {
    System.DateTime d = (System.DateTime)propertyValue;
    return "'" + d.ToString("yyyy-MM-dd") + "'";
  }

  protected override string CastToImp(OracleDateTimeSqlLiteralType sqlLiteralType, object propertyValue) {
    System.DateTime d = (System.DateTime)propertyValue;
    return "'" + d.ToString("yyyy-MM-dd") + "'";
  }

  protected override string CastToImp(DoubleSqlLiteralType sqlLiteralType, object propertyValue) {
    System.DateTime d = (System.DateTime)propertyValue;
    return d.ToString("yyyyMMdd").TrimStart('0');
  }

  protected override string CastToImp(IntervalSqlLiteralType sqlLiteralType, object propertyValue) {
    System.DateTime d = (System.DateTime)propertyValue;
    int days = (d - new System.DateTime(1, 1, 1)).Days;
    return "INTERVAL '" + days.ToString() + "' DAY";
  }
}
