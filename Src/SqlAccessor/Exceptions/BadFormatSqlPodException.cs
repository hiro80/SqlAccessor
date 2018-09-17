using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace SqlAccessor
{
  /// <summary>
  /// SqlPod.dllファイルが正常に読込めなかった時に送出される例外
  /// </summary>
  /// <remarks></remarks>
  [Serializable()]
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class BadFormatSqlPodException: SqlAccessorException
  {
    public BadFormatSqlPodException(string message)
      : base(message) {
    }

    public BadFormatSqlPodException(string message, Exception inner)
      : base(message, inner) {
    }

    protected BadFormatSqlPodException(SerializationInfo info
                                     , StreamingContext context)
      : base(info, context) {
    }
  }
}
