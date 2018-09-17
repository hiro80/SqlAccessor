using System;
using System.Reflection;

namespace SqlAccessor
{
  /// <summary>
  /// クラスオブジェクトのコンストラクタ、メソッド、プロパティをリフレクションにより呼び出す
  /// </summary>
  /// <remarks>メソッド、プロパティを、その名称の文字列を指定して呼び出すことができる</remarks>
  public class ClassOf
  {
    private readonly Type _type;

    public ClassOf(Type classType, params Type[] genericParamTypes) {
      if(classType == null) {
        throw new ArgumentNullException("classType"
                                      , "classType引数がnullです. " + 
                                        "Type.GetType()には名前空間を含めたクラス名を指定する必要があります. " + 
                                        "クラスがジェネリック型の場合は[クラス名]`[ジェネリックパラメータの数]で指定する必要があります");
      }

      foreach(Type genericParamType in genericParamTypes) {
        if(genericParamType == null) {
          throw new ArgumentNullException("genericParamTypes"
                                        , "genericParamTypes引数の配列要素にnullが含まれています." + 
                                          "Type.GetType()には名前空間を含めたクラス名を指定する必要があります. " + 
                                          "クラスがジェネリック型の場合は[クラス名]`[ジェネリックパラメータの数]で指定する必要があります");
        }
      }

      try {
        if(classType.IsGenericType) {
          _type = this.CreateGenericType(classType, genericParamTypes);
        } else {
          _type = classType;
        }
      } catch(Exception ex) {
        throw new ApplicationException(classType.Name + "型の作成に失敗しました", ex);
      }
    }

    /// <summary>
    /// クラスのインスタンスを生成する
    /// </summary>
    /// <param name="args">コンストラクタ引数</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public object CreateInstance(params object[] args) {
      try {
        return _type.InvokeMember(null
                                , BindingFlags.CreateInstance
                                , null
                                , null
                                , args);
      } catch {
        throw;
      }
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
        return _type.InvokeMember(funcName
                                , BindingFlags.InvokeMethod |
                                  BindingFlags.ExactBinding | 
                                  BindingFlags.Public | 
                                  BindingFlags.Static
                                , null
                                , null
                                , args);
      } catch {
        throw;
      }
    }

    /// <summary>
    /// StaticプロパティのGetを呼出す
    /// </summary>
    /// <param name="propertyName">プロパティ名</param>
    /// <param name="indexes">インデクサ</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public object GetProperty(string propertyName, params object[] indexes) {
      try {
        return _type.InvokeMember(propertyName
                                , BindingFlags.GetProperty |
                                  BindingFlags.ExactBinding |
                                  BindingFlags.Public |
                                  BindingFlags.Static
                                , null
                                , null
                                , indexes);
      } catch {
        throw;
      }
    }

    /// <summary>
    /// StaticプロパティのSetを呼出す
    /// </summary>
    /// <param name="propertyName">プロパティ名</param>
    /// <param name="value">格納する値</param>
    /// <param name="indexes">インデクサ</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public object SetProperty(string propertyName, object value, params object[] indexes){
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
		return _type.InvokeMember(propertyName
                                , BindingFlags.SetProperty |
                                  BindingFlags.ExactBinding |
                                  BindingFlags.Public |
                                  BindingFlags.Static
                                , null
                                , null
                                , valueAndIndexes);
	  } catch {
		throw;
	  }
	}

    private Type CreateGenericType(Type genericType, Type[] genericArgMetadata) {
      if(!genericType.IsGenericType || !genericType.Equals(genericType.GetGenericTypeDefinition())) {
        throw new ArgumentException(
          "指定されたgenericTypeは、ジェネリック型を構築する元になるジェネリック型定義(GenericTypeDefinition)ではありません。");
      }
      int genericTypeArgumentCount = genericType.GetGenericArguments().Length;
      if(genericTypeArgumentCount != genericArgMetadata.Length) {
        throw new ArgumentOutOfRangeException("生成するジェネリック型の引数の数と、Type[]メタデータの数が異なります。");
      }
      Type gtd = genericType.GetGenericTypeDefinition();
      Type dgtype = gtd.MakeGenericType(genericArgMetadata);
      return dgtype;
    }

  }
}
