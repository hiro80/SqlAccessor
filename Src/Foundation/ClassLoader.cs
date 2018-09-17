using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace SqlAccessor
{
  public class ClassLoader<T> where T: class
  {
    private readonly string _fileName;
    private readonly string _nameSpace;
    private readonly string _className;

    private T _loadedClass;

    public ClassLoader(string fileName
                     , string nameSpace
                     , string className) {
      _fileName = fileName;
      _nameSpace = nameSpace;
      _className = className;
    }

    public bool FileExists() {
      //ファイルの存在有無チェック
      return File.Exists(_fileName);
    }

    public T Load() {
      return this.Load(null);
    }

    public T Load(object constructorParam) {
      return this.Load(new object[] { constructorParam });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="constructorParams">コンストラクタに渡す引数</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public T Load(object[] constructorParams) {
      //一度ロードしたクラスは再度ロードしない
      if(_loadedClass != null) {
        return _loadedClass;
      }

      //ファイルの存在有無チェック
      if(!File.Exists(_fileName)) {
        throw new FileNotFoundException(_fileName + "が見つかりませんでした", _fileName);
      }

      T loadClass;
      try {
        //アセンブリファイル(dll)をロードする
        Assembly asm = Assembly.LoadFrom(_fileName);
        //Class名からTypeオブジェクトを取得する
        string typeName = _nameSpace + Type.Delimiter + _className;
        Type aType = asm.GetType(typeName);
        if(constructorParams == null) {
          //Dim aXXX As XXX = New XXX と同等
          loadClass = (T)Activator.CreateInstance(aType);
        } else {
          //Dim aXXX As XXX = New XXX(...) と同等
          loadClass = (T)Activator.CreateInstance(aType, constructorParams);
        }
      } catch(Exception ex) {
        //クラスが生成できなかった場合、例外を送出する
        throw new TypeLoadException(_className + "オブジェクトが生成できませんでした.", ex);
      }

      _loadedClass = loadClass;
      return loadClass;
    }
  }
}
