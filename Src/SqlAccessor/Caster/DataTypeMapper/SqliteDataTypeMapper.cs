using System;

namespace SqlAccessor
{
  /// <summary>
  /// SQLiteのネイティブデータ型とSQLリテラル型の対応を定義する
  /// </summary>
  /// <remarks>スレッドセーフ</remarks>
  public class SqliteDataTypeMapper: IDataTypeMapper
  {
    private readonly ICastEditor _castEditor;

    public SqliteDataTypeMapper(ICastEditor castEditor) {
      _castEditor = castEditor;
    }

    public SqlLiteralType GetSqlLiteralTypeOf(ViewColumnInfo aViewColumnInfo) {
      if(aViewColumnInfo == null) {
        throw new ArgumentNullException("aViewColumnInfo", "aViewColumnInfoがNothingです");
      }
      if(aViewColumnInfo.DbColumnTypeName == null) {
        throw new ApplicationException(
          aViewColumnInfo.ViewName + "." + aViewColumnInfo.ViewColumnName + 
          "に対応するSQLiteのデータ型が分からないため、プロパティ値をSQLリテラル型に変換できませんでした");
      }

      //ADO.NETが返すPervasiveのデータ型名
      string typeName = aViewColumnInfo.DbColumnTypeName.ToUpper();

      //
      // http://system.data.sqlite.org/index.html/doc/trunk/www/downloads.wiki
      //
      if(typeName.Contains("INT")) {
        //1. If the declared type contains the string "INT" then it is assigned INTEGER affinity.
        return new NumberSqlLiteralType(_castEditor);
      } else if(typeName.Contains("TEXT") ||
                typeName.Contains("CHAR") ||
                typeName.Contains("CLOB")) {
        //2. If the declared type of the column contains any of the strings "CHAR", 
        //   "CLOB", or "TEXT" then that column has TEXT affinity.
        return new StringSqlLiteralType(_castEditor);
      } else if(typeName.Contains("BLOB")) {
        //3. If the declared type for a column contains the string "BLOB" or 
        //   if no type is specified then the column has affinity NONE.
        throw new ApplicationException(aViewColumnInfo.ViewName + "." + aViewColumnInfo.ViewColumnName + 
          "に対応するSQLiteのデータ型が未定義なため、プロパティ値をSQLリテラル型に変換できませんでした");
      } else if(typeName == "") {
        // expressions do no necessarily have an affinity.
        var sqlLiteralType = this.GetSqlLiteralTypeOf(aViewColumnInfo.HostDataType);
        if(sqlLiteralType == null) {
          throw new ApplicationException(aViewColumnInfo.ViewName + "." + aViewColumnInfo.ViewColumnName +
            "に対応するSQLiteのデータ型が未定義なため、プロパティ値をSQLリテラル型に変換できませんでした.");
        }
        return sqlLiteralType;
      } else if(typeName.Contains("REAL") ||
                typeName.Contains("FLOA") ||
                typeName.Contains("DOUB")) {
        //4. If the declared type for a column contains any of the strings "REAL", 
        //   "FLOA", or "DOUB" then the column has REAL affinity.
        return new DoubleSqlLiteralType(_castEditor);
      //} else if(typeName == "STRING" ||
      //          typeName == "OBJECT") {
      //  //SQLite.NETはnone型テーブル列を"Object"や"String"という複数の型名で返すようだ
      //  return new StringSqlLiteralType(_castEditor);
      } else if(typeName == "NONE"){
        //SQLite.NETはnone型テーブル列を"none"という複数の型名で返すようだ
        return new StringSqlLiteralType(_castEditor);
      } else {
        //5. Otherwise, the affinity is NUMERIC.
        return new NumberSqlLiteralType(_castEditor);
      }
    }

    private SqlLiteralType GetSqlLiteralTypeOf(Type hostDataType) {
      if(hostDataType == null) {
        throw new InvalidPropertyToColumnCastException(
          "ADO.NETが返したデータ型が分からないため、プロパティ値をSQLリテラル型に変換できませんでした");
      }

      //ADO.NETが返すデータ型の完全修飾名
      string typeName = hostDataType.FullName;

      if(typeName == "System.String" ||
         typeName == "System.Char[]") {
        return new StringSqlLiteralType(_castEditor);
      } else if(typeName == "System.Byte" ||
                typeName == "System.Int16" ||
                typeName == "System.Int32" ||
                typeName == "System.Int64" ||
                typeName == "System.Decimal") {
        return new NumberSqlLiteralType(_castEditor);
      } else if(typeName == "System.DateTime") {
        return new OracleDateTimeSqlLiteralType(_castEditor);
      } else if(typeName == "System.Single" ||
                typeName == "System.Double") {
        return new DoubleSqlLiteralType(_castEditor);
      } else if(typeName == "System.TimeSpan") {
        // SQLiteではSystem.TimeSpan型が返ることはないだろう
        return new IntervalSqlLiteralType(_castEditor);
      } else {
        return null;
      }
    }

  }
}
