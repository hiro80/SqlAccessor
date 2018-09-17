using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace SqlAccessor
{
  /// <summary>
  /// 指定されたViewの列が存在しなかった時に送出される例外
  /// </summary>
  [Serializable()]
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class NotExistsViewColumnException: SqlAccessorException
  {
    public NotExistsViewColumnException(string message)
      : base(message) {
    }

    public NotExistsViewColumnException(string message, Exception inner)
      : base(message, inner) {
    }

    protected NotExistsViewColumnException(SerializationInfo info
                                         , StreamingContext context)
      : base(info, context) {
    }
  }
}


