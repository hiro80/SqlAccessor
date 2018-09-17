using System.ComponentModel;

namespace SqlAccessor
{
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class StringViewColumnType: ViewColumnType
  {
    public StringViewColumnType(ICastEditor castEditor)
      : base(castEditor) {
    }
    internal override object CastTo(PropertyType propertyType
                                  , ViewColumnInfo aViewColumnInfo
                                  , object viewColumnValue) {
      //ADO.NETが返した値を、Cast前編集する
      object value = _castEditor.BeforeCast_ViewColumnType(this, aViewColumnInfo, viewColumnValue);
      //ADO.NETが返したデータ型から、プロパティ型にキャストする
      value = propertyType.CastFrom(this, aViewColumnInfo, value);
      //プロパティ値をCast後編集する
      return _castEditor.AfterCast_ViewColumnType(this, aViewColumnInfo, value);
    }
  }
}
namespace SqlAccessor
{

  [EditorBrowsable(EditorBrowsableState.Never)]
  public class IntegerViewColumnType: ViewColumnType
  {
    public IntegerViewColumnType(ICastEditor castEditor)
      : base(castEditor) {
    }
    internal override object CastTo(PropertyType propertyType
                                  , ViewColumnInfo aViewColumnInfo
                                  , object viewColumnValue) {
      object value = _castEditor.BeforeCast_ViewColumnType(this, aViewColumnInfo, viewColumnValue);
      value = propertyType.CastFrom(this, aViewColumnInfo, value);
      return _castEditor.AfterCast_ViewColumnType(this, aViewColumnInfo, value);
    }
  }
}
namespace SqlAccessor
{

  [EditorBrowsable(EditorBrowsableState.Never)]
  public class LongViewColumnType: ViewColumnType
  {
    public LongViewColumnType(ICastEditor castEditor)
      : base(castEditor) {
    }
    internal override object CastTo(PropertyType propertyType
                                  , ViewColumnInfo aViewColumnInfo
                                  , object viewColumnValue) {
      object value = _castEditor.BeforeCast_ViewColumnType(this, aViewColumnInfo, viewColumnValue);
      value = propertyType.CastFrom(this, aViewColumnInfo, value);
      return _castEditor.AfterCast_ViewColumnType(this, aViewColumnInfo, value);
    }
  }
}
namespace SqlAccessor
{

  [EditorBrowsable(EditorBrowsableState.Never)]
  public class DecimalViewColumnType: ViewColumnType
  {
    public DecimalViewColumnType(ICastEditor castEditor)
      : base(castEditor) {
    }
    internal override object CastTo(PropertyType propertyType
                                  , ViewColumnInfo aViewColumnInfo
                                  , object viewColumnValue) {
      object value = _castEditor.BeforeCast_ViewColumnType(this, aViewColumnInfo, viewColumnValue);
      value = propertyType.CastFrom(this, aViewColumnInfo, value);
      return _castEditor.AfterCast_ViewColumnType(this, aViewColumnInfo, value);
    }
  }
}
namespace SqlAccessor
{

  [EditorBrowsable(EditorBrowsableState.Never)]
  public class DoubleViewColumnType: ViewColumnType
  {
    public DoubleViewColumnType(ICastEditor castEditor)
      : base(castEditor) {
    }
    internal override object CastTo(PropertyType propertyType
                                  , ViewColumnInfo aViewColumnInfo
                                  , object viewColumnValue) {
      object value = _castEditor.BeforeCast_ViewColumnType(this, aViewColumnInfo, viewColumnValue);
      value = propertyType.CastFrom(this, aViewColumnInfo, value);
      return _castEditor.AfterCast_ViewColumnType(this, aViewColumnInfo, value);
    }
  }
}
namespace SqlAccessor
{

  [EditorBrowsable(EditorBrowsableState.Never)]
  public class DateTimeViewColumnType: ViewColumnType
  {
    public DateTimeViewColumnType(ICastEditor castEditor)
      : base(castEditor) {
    }
    internal override object CastTo(PropertyType propertyType
                                  , ViewColumnInfo aViewColumnInfo
                                  , object viewColumnValue) {
      object value = _castEditor.BeforeCast_ViewColumnType(this, aViewColumnInfo, viewColumnValue);
      value = propertyType.CastFrom(this, aViewColumnInfo, value);
      return _castEditor.AfterCast_ViewColumnType(this, aViewColumnInfo, value);
    }
  }
}
namespace SqlAccessor
{

  [EditorBrowsable(EditorBrowsableState.Never)]
  public class TimeSpanViewColumnType: ViewColumnType
  {
    public TimeSpanViewColumnType(ICastEditor castEditor)
      : base(castEditor) {
    }
    internal override object CastTo(PropertyType propertyType
                                  , ViewColumnInfo aViewColumnInfo
                                  , object viewColumnValue) {
      object value = _castEditor.BeforeCast_ViewColumnType(this, aViewColumnInfo, viewColumnValue);
      value = propertyType.CastFrom(this, aViewColumnInfo, value);
      return _castEditor.AfterCast_ViewColumnType(this, aViewColumnInfo, value);
    }
  }
}
namespace SqlAccessor
{

  [EditorBrowsable(EditorBrowsableState.Never)]
  public class BooleanViewColumnType: ViewColumnType
  {
    public BooleanViewColumnType(ICastEditor castEditor)
      : base(castEditor) {
    }
    internal override object CastTo(PropertyType propertyType
                                  , ViewColumnInfo aViewColumnInfo
                                  , object viewColumnValue) {
      object value = _castEditor.BeforeCast_ViewColumnType(this, aViewColumnInfo, viewColumnValue);
      value = propertyType.CastFrom(this, aViewColumnInfo, value);
      return _castEditor.AfterCast_ViewColumnType(this, aViewColumnInfo, value);
    }
  }
}
