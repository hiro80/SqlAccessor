using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace SqlAccessor
{
  /// <summary>
  /// SqlPodに記述されたテーブルが存在しない時に送出される例外
  /// </summary>
  /// <remarks></remarks>
  [Serializable()]
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class NotExistsTableException: SqlAccessorException
  {
    public NotExistsTableException(string message)
      : base(message) {
    }

    public NotExistsTableException(string message, Exception inner)
      : base(message, inner) {
    }

    protected NotExistsTableException(SerializationInfo info
                                    , StreamingContext context)
      : base(info, context) {
    }
  }
}
