using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace SqlAccessor
{
  /// <summary>
  /// PropertyType.dllファイルが正常に読込めなかった時に送出される例外
  /// </summary>
  [Serializable()]
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class CannotLoadPropertyTypeException: SqlAccessorException
  {
    public CannotLoadPropertyTypeException(string message)
      : base(message) {
    }

    public CannotLoadPropertyTypeException(string message, Exception inner)
      : base(message, inner) {
    }

    protected CannotLoadPropertyTypeException(SerializationInfo info
                                            , StreamingContext context)
      : base(info, context) {
    }
  }
}

