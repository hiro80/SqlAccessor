using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace SqlAccessor
{
  /// <summary>
  /// データベースに接続できなかった時に送出される例外
  /// </summary>
  /// <remarks></remarks>
  [Serializable()]
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class CannotConnectToDbException: SqlAccessorException
  {
    public CannotConnectToDbException(string message)
      : base(message) {
    }

    public CannotConnectToDbException(string message, Exception inner)
      : base(message, inner) {
    }

    protected CannotConnectToDbException(SerializationInfo info
                                       , StreamingContext context)
      : base(info, context) {
    }
  }
}
