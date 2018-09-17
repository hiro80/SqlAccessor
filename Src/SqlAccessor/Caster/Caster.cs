using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace SqlAccessor
{
  /// <summary>
  /// 
  /// </summary>
  /// <remarks>
  /// スレッドセーフ
  /// Visitorパターンによる多重ディスパッチにより、View列型とプロパティ型、またはプロパティ型とSQLリテラル型の
  /// 型種別の組合せごとに編集処理を定義できる。また、プロパティ型は既存コードを変更せずに追加できるようになる
  ///</remarks>
  internal class Caster: ICaster
  {
    //(プロパティの型、キャストコマンド)
    private readonly Dictionary<Type, PropertyType> _propertyTypeHash = new Dictionary<Type, PropertyType>();
    //SELECT句のデータ型に対応したSQLリテラル型を返すオブジェクト
    private readonly IDataTypeMapper _dataTypeMapper;
    //型変換前の編集処理を行うオブジェクト

    private readonly ICastEditor _castEditor;
    public Caster(IDataTypeMapper dataTypeMapper
                , ICastEditor castEditor) {
      _dataTypeMapper = dataTypeMapper;
      _castEditor = castEditor;

      //PropertyTypesディレクトリから全てのPropertyType.dllをロードする
      this.LoadAllPropertyTypes();
    }

    /// <summary>
    /// PropertyTypesディレクトリにある全てのDLLファイルからPropertyTypeクラスを生成する
    /// </summary>
    /// <remarks></remarks>
    private void LoadAllPropertyTypes() {
      //PropertyTypesディレクトリは、自アセンブリファイルと同じディレクトリに配置される
      string propertyTypeDirPath = System.IO.Path.Combine(
                                          new System.IO.FileInfo(
                                              new Uri(this.GetType().Assembly.CodeBase).LocalPath
                                              ).DirectoryName
                                            , "PropertyTypes");

      if(!Directory.Exists(propertyTypeDirPath)) {
        throw new CannotLoadPropertyTypeException("ディレクトリ " + propertyTypeDirPath + "が存在しません");
      }

      string propertyTypeFilePath = "";
      try {
        foreach(string filePath in Directory.GetFiles(propertyTypeDirPath, "*.dll")) {
          propertyTypeFilePath = filePath;
          //ファイル名を指定してアセンブリを読み込む
          Assembly proppertyTypeAsm = Assembly.LoadFrom(propertyTypeFilePath);

          foreach(Type proppertyTypeType in proppertyTypeAsm.GetTypes()) {
            //生成にGenericパラメータが必要なクラスは除外する
            if(proppertyTypeType.IsGenericType == false &&
               proppertyTypeType.IsSubclassOf(typeof(PropertyType))) {
              //Dim aXXX As XXX = New XXX(...) と同等
              PropertyType aPropertyType = (PropertyType)Activator.CreateInstance(proppertyTypeType, new object[] { _castEditor });
              if(_propertyTypeHash.ContainsKey(aPropertyType.GetPropertyType())) {
                throw new CannotLoadPropertyTypeException(
                    "ディレクトリ" + propertyTypeDirPath + "に同じ型情報を返すPropertyTypeクラスが複数存在します");
              }
              _propertyTypeHash.Add(aPropertyType.GetPropertyType(), aPropertyType);
            }
          }
        }
      } catch(Exception ex) {
        string propertyTypeFileName = System.IO.Path.GetFileName(propertyTypeFilePath);
        throw new CannotLoadPropertyTypeException(
            "ディレクトリ " + propertyTypeDirPath + "から" + propertyTypeFileName + "クラスをロードする時にエラーが発生しました", ex);
      }
    }

    public bool IsNullPropertyValue(object propertyValue) {
      //propertyValueがNothing、またはNull許容型にNothingが格納された値の場合、NULLとする
      if(propertyValue == null) {
        return true;
      }
      //propertyValueのデータ型から呼び出すCastCmdの種別を決定する
      return this.GetPropertyType(propertyValue.GetType()).IsNullValue(propertyValue);
    }

    public object CastToPropertyType(object value, System.Type propertyTypeObj) {
      if(value == null) {
        throw new InvalidColumnToPropertyCastException("変換前の値がNullです");
      }

      //ダミーのViewColumnInfoを用いる
      ViewColumnInfo aViewColumnInfo = new ViewColumnInfo(null, "", "", 0);
      aViewColumnInfo.SetTypeInfo(value.GetType(), "");

      return this.CastToPropertyType(value, aViewColumnInfo, propertyTypeObj);
    }

    public object CastToPropertyType(object viewColumnValue
                                   , ViewColumnInfo aViewColumnInfo
                                   , System.Type propertyTypeObj
                                   , ColumnInfo aColumnInfo = null) {
      //columnValueがNothingまたはDbNullの場合、NULL表現値を返す
      if(viewColumnValue == null || viewColumnValue is DBNull) {
        if(this.IsNullableType(propertyTypeObj)) {
          //変換先プロパティ型がNull許容型ならば、NothingがNull表現値である
          return null;
        } else {
          //Null許容型でないならば、データ型固有のNull表現値を返す
          return this.GetPropertyType(propertyTypeObj).GetNullValue();
        }
      }

      //変換先プロパティがNull許容型ならば、Null許容型が含むジェネリックパラメータ型に
      //対応するPropertyTypeを取得する
      PropertyType aPropertyType = null;
      if(this.IsNullableType(propertyTypeObj)) {
        aPropertyType = this.GetPropertyType(this.GetGenericParameterType(propertyTypeObj));
      } else {
        aPropertyType = this.GetPropertyType(propertyTypeObj);
      }

      //ViewColumnTypeを生成する
      ViewColumnType viewColumnType = this.GetViewColumnType(aViewColumnInfo);

      //ADO.NETが返したデータ型から、プロパティ型にキャスト処理を行う
      return viewColumnType.CastTo(aPropertyType, aViewColumnInfo, viewColumnValue);
    }

    public string CastToSqlLiteralType(object propertyValue
                                     , ViewColumnInfo aViewColumnInfo
                                     , ColumnInfo aColumnInfo = null) {
      //プロパティの取得元SELECT句のデータ型から、対応するSQLリテラル型を決める
      SqlLiteralType sqlLiteralType = _dataTypeMapper.GetSqlLiteralTypeOf(aViewColumnInfo);

      //propertyTypeを生成する
      PropertyType aPropertyType = null;

      //propertyValueがNothingまたはNULL表現値の場合、"NULL", "DEFAULT", SQLリテラル初期値のいずれかを返す
      if(propertyValue == null) {
        //Nothing、またはNull許容型にNothingが格納された値の場合
        return this.NullOrDefaultValue(sqlLiteralType, aColumnInfo);
      } else {
        aPropertyType = this.GetPropertyType(propertyValue.GetType());
        if(aPropertyType.IsNullValue(propertyValue)) {
          return this.NullOrDefaultValue(sqlLiteralType, aColumnInfo);
        }
      }

      //プロパティ型から、SQLリテラル型にキャスト処理を行う
      return sqlLiteralType.CastFrom(aPropertyType, aColumnInfo, propertyValue);
    }

    /// <summary>
    /// 1対1で対応するテーブルカラムが存在する場合、NULL、DEFAULT、SQLリテラル初期値の何れを返すかを決定する
    /// </summary>
    /// <param name="aSqlLiteralType">SQLリテラル型</param>
    /// <param name="aColumnInfo">1対1で対応するテーブルカラム</param>
    /// <returns></returns>
    /// <remarks></remarks>
    private string NullOrDefaultValue(SqlLiteralType aSqlLiteralType
                                    , ColumnInfo aColumnInfo) {
      if(aColumnInfo == null) {
        // ''変換先の列のメタ情報がわからない場合、SQLリテラルの初期値を格納する
        //'Return aSqlLiteralType.DefaultValue
        //変換先の列のメタ情報がわからない場合、NULLを格納する
        return "NULL";
      } else if(!aColumnInfo.Nullable.HasValue || !aColumnInfo.Nullable.Value) {
        //変換先の列がNOT NULLの場合
        if(string.IsNullOrEmpty(aColumnInfo.DefaultValue)) {
          // ''NOT NULLかつDEFAULT値が設定されていない場合、SQLリテラルの初期値を格納する
          //'Return aSqlLiteralType.DefaultValue
          //NOT NULLかつDEFAULT値が設定されていない場合、
          //プロパティ値の設定誤りになるのでNULLを格納しSQLの実行エラーにする
          return "NULL";
        }
        return "DEFAULT";
      } else {
        //変換先の列がNULL可能の場合
        return "NULL";
      }
    }

    private PropertyType GetPropertyType(Type propertyTypeObj) {
      if(_propertyTypeHash.ContainsKey(propertyTypeObj)) {
        return _propertyTypeHash[propertyTypeObj];
      } else {
        throw new CannotLoadPropertyTypeException(
          propertyTypeObj.FullName + "型のプロパティの型変換を定義したPropertyType.dllが見つかりません");
      }
    }

    /// <summary>
    /// 指定したTypeオブジェクトがNull許容型か否か判定する
    /// </summary>
    /// <param name="type">Typeオブジェクト、GetType演算子で取得したものを指定すること</param>
    /// <returns></returns>
    /// <remarks></remarks>
    private bool IsNullableType(Type type) {
      return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
    }

    /// <summary>
    /// 指定したTypeオブジェクトがジェネリックパラメータを持つ場合は、
    /// そのジェネリックパラメータのTypeオブジェクトを返す
    /// </summary>
    /// <param name="type">Typeオブジェクト</param>
    /// <returns></returns>
    /// <remarks>複数のジェネリックパラメータを持つ場合は、1つ目を返す</remarks>
    private Type GetGenericParameterType(Type type) {
      if(type.GetGenericArguments() == null ||
         type.GetGenericArguments().Length == 0) {
        return null;
      }

      return type.GetGenericArguments()[0];
    }

    /// <summary>
    /// View列のメタ情報からViewColumnTypeオブジェクトを取得する
    /// </summary>
    /// <param name="aViewColumnInfo"></param>
    /// <returns></returns>
    /// <remarks></remarks>
    private ViewColumnType GetViewColumnType(ViewColumnInfo aViewColumnInfo) {
      if(aViewColumnInfo == null || aViewColumnInfo.HostDataType == null) {
        throw new InvalidColumnToPropertyCastException(
          "ADO.NETが返したデータ型が分からないため、プロパティ値をSQLリテラル型に変換できませんでした.");
      }

      //ADO.NETが返すデータ型の完全修飾名
      string typeName = aViewColumnInfo.HostDataType.FullName;

      //StringとIntegerは使用頻度が高いと思われるので、先にチェックする
      if(typeName == "System.String") {
        return new StringViewColumnType(_castEditor);
      } else if(typeName == "System.Int32") {
        return new IntegerViewColumnType(_castEditor);
      } else if(typeName == "System.Byte"   ||
                typeName == "System.SByte"  ||
                typeName == "System.Int16"  ||
                typeName == "System.UInt16" ||
                typeName == "System.UInt32" ||
                typeName == "System.Int64") {
        //Byte, SByte, Short, UShort, Integer, UInteger, Longで共用する
        return new LongViewColumnType(_castEditor);
      } else if(typeName == "System.Char" ||
                typeName == "System.Char[]") {
        //Char, Stringで共用する
        return new StringViewColumnType(_castEditor);
      } else if(typeName == "System.UInt64" ||
                typeName == "System.Decimal") {
        //ULong, Decimalで共用する
        return new DecimalViewColumnType(_castEditor);
      } else if(typeName == "System.Single" ||
                typeName == "System.Double") {
        //Single, Doubleで共用する
        return new DoubleViewColumnType(_castEditor);
      } else if(typeName == "System.DateTime") {
        //DateTimeで使用する
        return new DateTimeViewColumnType(_castEditor);
      } else if(typeName == "System.TimeSpan") {
        //TimeSpanで使用する
        return new TimeSpanViewColumnType(_castEditor);
      } else if(typeName == "System.Boolean") {
        //Booleanで使用する
        return new BooleanViewColumnType(_castEditor);
      } else {
        throw new InvalidColumnToPropertyCastException("ADO.NETから予期しないデータ型を受け取りました");
      }
    }

  }
}
