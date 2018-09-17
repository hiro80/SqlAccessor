using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace SqlAccessor
{
  /// <summary>
  /// SqlAccessorが送出する例外のスーパークラス
  /// </summary>
  /// <remarks></remarks>
  [Serializable()]
  [EditorBrowsable(EditorBrowsableState.Never)]
  public abstract class SqlAccessorException: ApplicationException
  {
    public SqlAccessorException(string message)
      : base(message) {
    }

    public SqlAccessorException(string message, Exception inner)
      : base(message, inner) {
    }

    protected SqlAccessorException(SerializationInfo info
                                , StreamingContext context)
      : base(info, context) {
    }

    /// <summary>
    /// SqlAccessorのバージョン
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
    public string Version {
      get {
        //自分自身のAssemblyを取得
        System.Reflection.Assembly asm = 
          System.Reflection.Assembly.GetExecutingAssembly();
        //バージョンの取得
        return asm.GetName().Version.ToString();
      }
    }
  }
}
