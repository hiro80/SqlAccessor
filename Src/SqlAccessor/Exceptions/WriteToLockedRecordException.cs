using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace SqlAccessor
{
  /// <summary>
  /// ロックされたレコードを更新しようとした時に送出される例外
  /// </summary>
  /// <remarks></remarks>
  [Serializable()]
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class WriteToLockedRecordException: SqlAccessorException
  {
    //例外を送出したTranオブジェクトのAPトランザクションID
    private long _apTranId;

    public WriteToLockedRecordException(string message)
      : base(message) {
    }

    public WriteToLockedRecordException(string message, long apTranId)
      : base(message) {
      _apTranId = apTranId;
    }

    public WriteToLockedRecordException(string message, Exception inner)
      : base(message, inner) {
    }

    public WriteToLockedRecordException(string message, long apTranId, Exception inner)
      : base(message, inner) {
      _apTranId = apTranId;
    }

    protected WriteToLockedRecordException(SerializationInfo info
                                         , StreamingContext context)
      : base(info, context) {
    }

    public long ApTranId {
      get { return _apTranId; }
    }
  }
}
