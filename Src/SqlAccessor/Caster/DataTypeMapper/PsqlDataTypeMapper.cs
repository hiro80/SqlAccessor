using System;

namespace SqlAccessor
{
  /// <summary>
  /// Pervasiveのネイティブデータ型とSQLリテラル型の対応を定義する
  /// </summary>
  /// <remarks>スレッドセーフ</remarks>
  internal class PsqlDataTypeMapper: IDataTypeMapper
  {
    private readonly ICastEditor _castEditor;

    public PsqlDataTypeMapper(ICastEditor castEditor) {
      _castEditor = castEditor;
    }

    public SqlLiteralType GetSqlLiteralTypeOf(ViewColumnInfo aViewColumnInfo) {
      if(aViewColumnInfo == null) {
        throw new ArgumentNullException("aViewColumnInfo", "aViewColumnInfoがNothingです");
      }
      if(aViewColumnInfo.DbColumnTypeName == null) {
        throw new ApplicationException(
          aViewColumnInfo.ViewName + "." + aViewColumnInfo.ViewColumnName + 
          "に対応するPervasiveのデータ型が分からないため、プロパティ値をSQLリテラル型に変換できませんでした");
      }

      //ADO.NETが返すPervasiveのデータ型名
      string typeName = aViewColumnInfo.DbColumnTypeName;

      if(typeName == "CHAR" ||
         typeName == "VARCHAR" || 
         typeName == "LONGVARCHAR" || 
         typeName == "UNIQUE_IDENTIFIER") {
        return new StringSqlLiteralType(_castEditor);
      } else if(typeName == "BIGINT" || 
                typeName == "CURRENCY" || 
                typeName == "DECIMAL" || 
                typeName == "IDENTITY" || 
                typeName == "INTEGER" || 
                typeName == "MONEY" || 
                typeName == "NUMERIC" || 
                typeName == "NUMERICSA" || 
                typeName == "NUMERICSTS" || 
                typeName == "SMALLIDENTITY" || 
                typeName == "SMALLINT" || 
                typeName == "TINYINT" || 
                typeName == "UBIGINT" || 
                typeName == "UINTEGER" || 
                typeName == "USMALLINT" ||
                typeName == "UTINYINT") {
        return new NumberSqlLiteralType(_castEditor);
      } else if(typeName == "DATE" || 
                typeName == "DATE/TIME" || 
                typeName == "TIMESTAMP") {
        return new DateTimeSqlLiteralType(_castEditor);
      } else if(typeName == "BFLOAT4" || 
                typeName == "BFLOAT8" || 
                typeName == "DOUBLE" || 
                typeName == "FLOAT" || 
                typeName == "REAL") {
        return new DoubleSqlLiteralType(_castEditor);
      } else if(typeName == "NUMBER") {
        //基本的には、ネイティブデータ型とPsqlDbTypeとは1対1で対応しています。
        //ただし、Pervasiveデータ型NUMBERは、DecimalまたはDoubleのどちらでも取れるので、この限りではありません。
        return new DoubleSqlLiteralType(_castEditor);
      } else if(typeName == "TIME") {
        return new IntervalSqlLiteralType(_castEditor);
      } else if(typeName == "BINARY" || 
                typeName == "BIT" || 
                typeName == "LONGVARBINARY") {
        throw new InvalidPropertyToColumnCastException(
          aViewColumnInfo.ViewName + "." + aViewColumnInfo.ViewColumnName + 
          "に対応するPervasiveのデータ型 " + typeName + "はSQLリテラル型に変換できません");
      } else {
        throw new ApplicationException(
          aViewColumnInfo.ViewName + "." + aViewColumnInfo.ViewColumnName + 
          "に対応するPervasiveのデータ型が未定義なため、プロパティ値をSQLリテラル型に変換できませんでした");
      }

    }

  }
}
