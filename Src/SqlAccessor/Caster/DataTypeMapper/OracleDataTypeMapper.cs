using System;

namespace SqlAccessor
{
  /// <summary>
  /// Oracleのネイティブデータ型とSQLリテラル型の対応を定義する
  /// </summary>
  /// <remarks>スレッドセーフ</remarks>
  internal class OracleDataTypeMapper: IDataTypeMapper
  {
    private readonly ICastEditor _castEditor;

    public OracleDataTypeMapper(ICastEditor castEditor) {
      _castEditor = castEditor;
    }

    public SqlLiteralType GetSqlLiteralTypeOf(ViewColumnInfo aViewColumnInfo) {
      if(aViewColumnInfo == null ||
         aViewColumnInfo.HostDataType == null) {
        throw new InvalidPropertyToColumnCastException(
          "ADO.NETが返したデータ型が分からないため、プロパティ値をSQLリテラル型に変換できませんでした");
      }

      //ADO.NETが返すデータ型の完全修飾名
      string typeName = aViewColumnInfo.HostDataType.FullName;

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
        //Int64で返るデータ型は、Oracleネイティブデータ型では、NUMBER型とINTERVAL YEAR TO MONTH型の2つの可能性があるが、
        //Int64はNumber型に対応付けている。そのため、INTERVAL YEAR TO MONTH型に対するSQLリテラル型はNumber型になる。
        return new IntervalSqlLiteralType(_castEditor);
      } else {
        throw new InvalidPropertyToColumnCastException(
          "ADO.NETが未定義なデータ型を返したため、プロパティ値をSQLリテラル型に変換できませんでした");
      }
    }
  }
}
