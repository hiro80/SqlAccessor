using System;
using System.Reflection;

namespace SqlAccessor
{
  /// <summary>
  /// インスタンスのメソッド、プロパティをリフレクションにより呼び出す
  /// </summary>
  /// <remarks>メソッド、プロパティを、その名称の文字列を指定して呼び出すことができる</remarks>
  public class InstanceOf
  {
    private readonly Type _type;
    private readonly object _instance;

    public InstanceOf(object instance) {
      if(instance == null) {
        throw new ArgumentNullException("instance"
                                      , "instance引数がnullです." + 
                                        "引数には呼び出したいメソッド又はプロパティをメンバに持つインスタンスを指定する必要があります.");
      }
      _type = instance.GetType();
      _instance = instance;
    }

    /// <summary>
    /// Staticメソッドを呼出す
    /// </summary>
    /// <param name="funcName">メソッド名</param>
    /// <param name="args">引数</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public object Method(string funcName, params object[] args) {
      try {
        //BindingFlags.Publicを指定するとMissingMethodExceptionが送出される、原因不明
        return _type.InvokeMember(funcName
                                , BindingFlags.InvokeMethod | BindingFlags.ExactBinding
                                , null
                                , _instance
                                , args);
      } catch {
        throw;
      }
    }

    /// <summary>
    /// プロパティのGetを呼出す
    /// </summary>
    /// <param name="propertyName">プロパティ名</param>
    /// <param name="indexes">インデクサ</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public object GetProperty(string propertyName, params object[] indexes) {
      try {
        //BindingFlags.Publicを指定するとMissingMethodExceptionが送出される、原因不明
        return _type.InvokeMember(propertyName
                                , BindingFlags.GetProperty | BindingFlags.ExactBinding
                                , null
                                , _instance
                                , indexes);
      } catch {
        throw;
      }
    }

    /// <summary>
    /// プロパティのSetを呼出す
    /// </summary>
    /// <param name="propertyName">プロパティ名</param>
    /// <param name="value">格納する値</param>
    /// <param name="indexes">インデクサ</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public object SetProperty(string propertyName, object value, params object[] indexes)
		{
			//valueとindexesを一つの配列に格納する
			object[] valueAndIndexes = new object[] {};
			if (indexes != null && indexes.Length > 0) {
				Array.Resize(ref valueAndIndexes, indexes.Length + 1);
				for (int i = 0; i <= indexes.Length - 1; i++) {
					valueAndIndexes.SetValue(indexes[i], i);
				}
				valueAndIndexes[indexes.Length] = value;
			} else {
				Array.Resize(ref valueAndIndexes, 1);
				valueAndIndexes[0] = value;
			}

			try {
				//BindingFlags.Publicを指定するとMissingMethodExceptionが送出される、原因不明
				return _type.InvokeMember(propertyName
                                        , BindingFlags.SetProperty | BindingFlags.ExactBinding
                                        , null
                                        , _instance
                                        , valueAndIndexes);
			} catch {
				throw;
			}
		}

  }
}

