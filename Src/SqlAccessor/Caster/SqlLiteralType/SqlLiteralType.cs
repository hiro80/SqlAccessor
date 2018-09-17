using System.ComponentModel;

namespace SqlAccessor
{
  [EditorBrowsable(EditorBrowsableState.Never)]
  public abstract class SqlLiteralType
  {
    protected readonly ICastEditor _castEditor;
    protected SqlLiteralType(ICastEditor castEditor) {
      _castEditor = castEditor;
    }

    internal abstract string CastFrom(PropertyType propertyType
                                    , ColumnInfo aColumnInfo
                                    , object propertyValue);

    /// <summary>
    /// SQLリテラル型の初期値
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
    internal abstract string DefaultValue { get; }
  }
}
