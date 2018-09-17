using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace SqlAccessor
{
  /// <summary>
  /// 規定どおりに定義されていないRecordクラスが使用された時に送出される例外
  /// </summary>
  /// <remarks></remarks>
  [Serializable()]
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class BadFormatRecordException: SqlAccessorException
  {
    public BadFormatRecordException(string message)
      : base(message) {
    }

    public BadFormatRecordException(string message, Exception inner)
      : base(message, inner) {
    }

    protected BadFormatRecordException(SerializationInfo info
                                     , StreamingContext context)
      : base(info, context) {
    }
  }
}
