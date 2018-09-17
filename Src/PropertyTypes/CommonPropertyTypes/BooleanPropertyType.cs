using System;
using SqlAccessor;

public class BooleanPropertyType: PropertyType
{
  private readonly System.Type _myType = typeof(System.Boolean);

  public BooleanPropertyType(ICastEditor castEditor)
    : base(castEditor) {
  }

  protected override System.Type GetPropertyType() {
    return _myType;
  }

  protected override bool IsNullValue(object propertyValue) {
    //BooleanにはNULL表現値はない
    return false;
  }

  protected override object GetNullValue() {
    //BooleanにはNULL表現値はない
    throw new NotSupportedException("Boolean型にはNULL表現値はありません");
  }

  protected override object CastFromImp(StringViewColumnType viewColumnType, object viewColumnValue) {
    string viewColumnValueStr = (string)viewColumnValue;

    if(string.IsNullOrEmpty(viewColumnValueStr)) {
      return false;
    }

    viewColumnValueStr = viewColumnValueStr.Trim();

    if(string.IsNullOrEmpty(viewColumnValueStr) ||
        viewColumnValueStr == "0" ||
        viewColumnValueStr.ToUpper() == "FALSE" ||
        viewColumnValueStr.ToUpper() == "F") {
      return false;
    } else {
      return true;
    }
  }

  protected override object CastFromImp(IntegerViewColumnType viewColumnType, object viewColumnValue) {
    int i = Convert.ToInt32(viewColumnValue);
    return !(i == 0);
  }

  protected override object CastFromImp(LongViewColumnType viewColumnType, object viewColumnValue) {
    long l = Convert.ToInt64(viewColumnValue);
    return !(l == 0);
  }

  protected override object CastFromImp(DecimalViewColumnType viewColumnType, object viewColumnValue) {
    decimal d = Convert.ToDecimal(viewColumnValue);
    return !(d == 0);
  }

  protected override object CastFromImp(DoubleViewColumnType viewColumnType, object viewColumnValue) {
    double d = Convert.ToDouble(viewColumnValue);
    return !(d == 0);
  }

  protected override object CastFromImp(DateTimeViewColumnType viewColumnType, object viewColumnValue) {
    System.DateTime d = (System.DateTime)viewColumnValue;
    // 0001/01/01 00:00:00 以外はtrueと見做す
    return !(d == new System.DateTime(1, 1, 1, 0, 0, 0));
  }

  protected override object CastFromImp(TimeSpanViewColumnType viewColumnType, object viewColumnValue) {
    TimeSpan t = (System.TimeSpan)viewColumnValue;
    return !(t == TimeSpan.Zero);
  }

  protected override object CastFromImp(BooleanViewColumnType viewColumnType, object viewColumnValue) {
    return Convert.ToBoolean(viewColumnValue);
  }

  protected override string CastToImp(StringSqlLiteralType sqlLiteralType, object propertyValue) {
    bool b = (bool)propertyValue;
    if(b) {
      return "'T'";
    } else {
      return "'F'";
    }
  }

  protected override string CastToImp(NumberSqlLiteralType sqlLiteralType, object propertyValue) {
    bool b = (bool)propertyValue;
    if(b) {
      return "1";
    } else {
      return "0";
    }
  }

  protected override string CastToImp(DateTimeSqlLiteralType sqlLiteralType, object propertyValue) {
    bool b = (bool)propertyValue;
    if(b) {
      return "0001-01-02";
    } else {
      return "0001-01-01";
    }
  }

  protected override string CastToImp(OracleDateTimeSqlLiteralType sqlLiteralType, object propertyValue) {
    bool b = (bool)propertyValue;
    if(b) {
      return "0001-01-02";
    } else {
      return "0001-01-01";
    }
  }

  protected override string CastToImp(DoubleSqlLiteralType sqlLiteralType, object propertyValue) {
    bool b = (bool)propertyValue;
    if(b) {
      return "1";
    } else {
      return "0";
    }
  }

  protected override string CastToImp(IntervalSqlLiteralType sqlLiteralType, object propertyValue) {
    bool b = (bool)propertyValue;
    if(b) {
      return "INTERVAL '1' DAY";
    } else {
      return "INTERVAL '2' DAY";
    }
  }
}
