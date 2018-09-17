using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace SqlAccessor
{
  /// <summary>
  /// データベースで一意性制約エラー以外のエラーが発生した時に送出される例外
  /// </summary>
  /// <remarks></remarks>
  [Serializable()]
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class DbAccessException: SqlAccessorException, ISerializable
  {
    //実行に失敗したSQL文
    protected string _failedSql;

    public DbAccessException(string message)
      : base(message) {
    }

    public DbAccessException(string message, string sql)
      : base(message) {
      _failedSql = sql;
    }

    public DbAccessException(string message, Exception inner)
      : base(message, inner) {
    }

    public DbAccessException(string message, string sql, Exception inner)
      : base(message, inner) {
      _failedSql = sql;
    }

    protected DbAccessException(SerializationInfo info
                              , StreamingContext context)
      : base(info, context) {
      //固有のメンバ変数をデシリアライズする
      _failedSql = info.GetString("sql");
    }

    //<SecurityPermission(SecurityAction.Demand, serializationformatter = True)> _
    public override void GetObjectData(SerializationInfo info
                                     , StreamingContext context) {
      base.GetObjectData(info, context);
      //固有のメンバ変数をシリアライズする
      info.AddValue("sql", _failedSql);
    }

    public string FailedSql {
      get { return _failedSql; }
    }
  }
}
