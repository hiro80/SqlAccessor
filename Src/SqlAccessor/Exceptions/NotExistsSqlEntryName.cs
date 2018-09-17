using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace SqlAccessor
{
  /// <summary>
  /// 指定されたSQLエントリがSqlPodに存在しなかった時に送出される例外
  /// </summary>
  [Serializable()]
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class NotExistsSqlEntryName: SqlAccessorException
  {
    public NotExistsSqlEntryName(string message)
      : base(message) {
    }

    public NotExistsSqlEntryName(string message, Exception inner)
      : base(message, inner) {
    }

    protected NotExistsSqlEntryName(SerializationInfo info
                                  , StreamingContext context)
      : base(info, context) {
    }
  }
}


