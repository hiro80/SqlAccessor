using System;
using System.Net.Mail;
using SqlAccessor;

public class MailAddressPropertyType: PropertyType
{
  private readonly System.Type _myType = typeof(MailAddress);
  public MailAddressPropertyType(ICastEditor castEditor)
    : base(castEditor) {
  }

  protected override System.Type GetPropertyType() {
    return _myType;
  }

  protected override bool IsNullValue(object propertyValue) {
    return propertyValue == null;
  }

  protected override object GetNullValue() {
    return null;
  }

  protected override object CastFromImp(StringViewColumnType viewColumnType, object viewColumnValue) {
    try {
      return new MailAddress(viewColumnValue.ToString());
    } catch(ArgumentException ex) {
      return this.GetNullValue();
    } catch(FormatException ex) {
      return this.GetNullValue();
    }
  }

  protected override object CastFromImp(IntegerViewColumnType viewColumnType, object viewColumnValue) {
    return this.GetNullValue();
  }

  protected override object CastFromImp(LongViewColumnType viewColumnType, object viewColumnValue) {
    return this.GetNullValue();
  }

  protected override object CastFromImp(DecimalViewColumnType viewColumnType, object viewColumnValue) {
    return this.GetNullValue();
  }

  protected override object CastFromImp(DoubleViewColumnType viewColumnType, object viewColumnValue) {
    return this.GetNullValue();
  }

  protected override object CastFromImp(DateTimeViewColumnType viewColumnType, object viewColumnValue) {
    return this.GetNullValue();
  }

  protected override object CastFromImp(TimeSpanViewColumnType viewColumnType, object viewColumnValue) {
    return this.GetNullValue();
  }

  protected override object CastFromImp(BooleanViewColumnType viewColumnType, object viewColumnValue) {
    return this.GetNullValue();
  }

  protected override string CastToImp(StringSqlLiteralType sqlLiteralType, object propertyValue) {
    MailAddress m = (MailAddress)propertyValue;
    return "'" + m.Address + "'";
  }

  protected override string CastToImp(NumberSqlLiteralType sqlLiteralType, object propertyValue) {
    return "0";
  }

  protected override string CastToImp(DateTimeSqlLiteralType sqlLiteralType, object propertyValue) {
    return "'0001-01-01'";
  }

  protected override string CastToImp(OracleDateTimeSqlLiteralType sqlLiteralType, object propertyValue) {
    return "'0001-01-01'";
  }

  protected override string CastToImp(DoubleSqlLiteralType sqlLiteralType, object propertyValue) {
    return "0";
  }

  protected override string CastToImp(IntervalSqlLiteralType sqlLiteralType, object propertyValue) {
    return "INTERVAL '0' DAY";
  }
}
