using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace SqlAccessor
{
  /// <summary>
  /// テーブルのカラムからレコードのプロパティへのデータ変換に失敗した時に送出される例外
  /// </summary>
  /// <remarks></remarks>
  [Serializable()]
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class InvalidColumnToPropertyCastException: SqlAccessorException
  {
    public InvalidColumnToPropertyCastException(string message)
      : base(message) {
    }

    public InvalidColumnToPropertyCastException(string message, Exception inner)
      : base(message, inner) {
    }

    protected InvalidColumnToPropertyCastException(SerializationInfo info
                                                 , StreamingContext context)
      : base(info, context) {
    }
  }
}
