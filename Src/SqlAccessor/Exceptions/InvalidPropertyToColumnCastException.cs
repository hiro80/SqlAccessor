using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace SqlAccessor
{
  /// <summary>
  /// レコードのプロパティからテーブルのカラムへのデータ変換に失敗した時に送出される例外
  /// </summary>
  /// <remarks></remarks>
  [Serializable()]
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class InvalidPropertyToColumnCastException: SqlAccessorException
  {
    public InvalidPropertyToColumnCastException(string message)
      : base(message) {
    }

    public InvalidPropertyToColumnCastException(string message, Exception inner)
      : base(message, inner) {
    }

    protected InvalidPropertyToColumnCastException(SerializationInfo info
                                                 , StreamingContext context)
      : base(info, context) {
    }
  }
}
