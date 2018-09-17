using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace SqlAccessor
{
  /// <summary>
  /// レコードを新規追加しようとして一意性制約エラーが発生した時に送出される例外
  /// </summary>
  /// <remarks></remarks>
  [Serializable()]
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class DuplicateKeyException: DbAccessException
  {
    public DuplicateKeyException(string message)
      : base(message) {
    }

    public DuplicateKeyException(string message, string sql)
      : base(message) {
      _failedSql = sql;
    }

    public DuplicateKeyException(string message, Exception inner)
      : base(message, inner) {
    }

    public DuplicateKeyException(string message, string sql, Exception inner)
      : base(message, inner) {
      _failedSql = sql;
    }

    protected DuplicateKeyException(SerializationInfo info
                                  , StreamingContext context)
      : base(info, context) {
      //固有のメンバ変数をデシリアライズする
      _failedSql = info.GetString("sql");
    }
  }
}
