using System.ComponentModel;

namespace SqlAccessor
{
  [EditorBrowsable(EditorBrowsableState.Never)]
  public abstract class ViewColumnType
  {
    protected readonly ICastEditor _castEditor;
    protected ViewColumnType(ICastEditor castEditor) {
      _castEditor = castEditor;
    }

    internal abstract object CastTo(PropertyType propertyType
                                  , ViewColumnInfo aViewColumnInfo
                                  , object viewColumnValue);
  }
}
