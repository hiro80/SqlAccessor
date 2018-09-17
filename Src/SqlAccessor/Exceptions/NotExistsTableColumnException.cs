using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace SqlAccessor
{
  /// <summary>
  /// SqlPodに記述されたテーブルの列が存在しない時に送出される例外
  /// </summary>
  /// <remarks></remarks>
  [Serializable()]
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class NotExistsTableColumnException: SqlAccessorException
  {
    public NotExistsTableColumnException(string message)
      : base(message) {
    }

    public NotExistsTableColumnException(string message, Exception inner)
      : base(message, inner) {
    }

    protected NotExistsTableColumnException(SerializationInfo info
                                          , StreamingContext context)
      : base(info, context) {
    }
  }
}
