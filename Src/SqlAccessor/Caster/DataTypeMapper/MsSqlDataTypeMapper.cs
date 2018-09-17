using System;

namespace SqlAccessor
{
  /// <summary>
  /// Microsoft SQL Serverのネイティブデータ型とSQLリテラル型の対応を定義する
  /// </summary>
  /// <remarks>スレッドセーフ</remarks>
  internal class MsSqlDataTypeMapper: IDataTypeMapper
  {
    private readonly ICastEditor _castEditor;

    public MsSqlDataTypeMapper(ICastEditor castEditor) {
      _castEditor = castEditor;
    }

    public SqlLiteralType GetSqlLiteralTypeOf(ViewColumnInfo aViewColumnInfo) {
      if(aViewColumnInfo == null) {
        throw new ArgumentNullException("aViewColumnInfo", "aViewColumnInfoがNothingです");
      }
      if(aViewColumnInfo.DbColumnTypeName == null) {
        throw new ApplicationException(
          aViewColumnInfo.ViewName + "." + aViewColumnInfo.ViewColumnName + 
          "に対応するSQL Serverのデータ型が分からないため、プロパティ値をSQLリテラル型に変換できませんでした");
      }

      //ADO.NETが返すSQL Serverのデータ型名
      string typeName = aViewColumnInfo.DbColumnTypeName;

      if(typeName == "nvarchar" || 
         typeName == "ntext"    || 
         typeName == "nchar") {
        return new StringSqlLiteralType(_castEditor, true);
      } else if(typeName == "varchar" || 
                typeName == "text"    || 
                typeName == "char") {
        return new StringSqlLiteralType(_castEditor, false);
      } else if(typeName == "int"     || 
                typeName == "numeric" || 
                typeName == "decimal" || 
                typeName == "bit"     || 
                typeName == "money" || 
                typeName == "uniqueidentifier" || 
                typeName == "bigint"   || 
                typeName == "tinyint"  || 
                typeName == "smallint" || 
                typeName == "smallmoney") {
        return new NumberSqlLiteralType(_castEditor);
      } else if(typeName == "date"       || 
                typeName == "datetime2"  || 
                typeName == "timestamp"  || 
                typeName == "rowversion" || 
                typeName == "datetime"   || 
                typeName == "smalldatetime") {
        return new DateTimeSqlLiteralType(_castEditor);
      } else if(typeName == "real" || 
                typeName == "float") {
        return new DoubleSqlLiteralType(_castEditor);
      } else if(typeName == "datetimeoffset" || 
                typeName == "time") {
        return new IntervalSqlLiteralType(_castEditor);
      } else if(typeName == "binary"      || 
                typeName == "FILESTREAM attribute " || 
                typeName == "image"       || 
                typeName == "sql_variant" || 
                typeName == "varbinary"   || 
                typeName == "xml") {
        throw new InvalidPropertyToColumnCastException(
          aViewColumnInfo.ViewName + "." + aViewColumnInfo.ViewColumnName + 
          "に対応するSQL Serverのデータ型 " + typeName + "はSQLリテラル型に変換できません");
      } else {
        throw new ApplicationException(
          aViewColumnInfo.ViewName + "." + aViewColumnInfo.ViewColumnName + 
          "に対応するSQL Serverのデータ型が未定義なため、プロパティ値をSQLリテラル型に変換できませんでした");
      }
    }

  }
}
