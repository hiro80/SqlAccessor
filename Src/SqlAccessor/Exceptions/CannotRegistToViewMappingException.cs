using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace SqlAccessor
{
  /// <summary>
  /// レコードとView(SELECT文の結果)の紐付けに失敗した時に送出される例外
  /// </summary>
  /// <remarks></remarks>
  [Serializable()]
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class CannotRegistToViewMappingException: SqlAccessorException
  {
    public CannotRegistToViewMappingException(string message)
      : base(message) {
    }

    public CannotRegistToViewMappingException(string message, Exception inner)
      : base(message, inner) {
    }

    protected CannotRegistToViewMappingException(SerializationInfo info
                                               , StreamingContext context)
      : base(info, context) {
    }
  }
}
