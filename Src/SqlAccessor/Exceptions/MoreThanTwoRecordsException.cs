using System;
using System.Runtime.Serialization;
using System.ComponentModel;
namespace SqlAccessor
{
  /// <summary>
  /// 1件のレコードを取得するメソッドで2件以上のレコードが該当した時に送出される例外
  /// </summary>
  /// <remarks></remarks>
  [Serializable()]
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class MoreThanTwoRecordsException: SqlAccessorException
  {
    public MoreThanTwoRecordsException(string message)
      : base(message) {
    }

    public MoreThanTwoRecordsException(string message, Exception inner)
      : base(message, inner) {
    }

    protected MoreThanTwoRecordsException(SerializationInfo info
                                        , StreamingContext context)
      : base(info, context) {
    }
  }
}
