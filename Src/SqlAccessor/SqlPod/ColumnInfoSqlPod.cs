using System;
using System.ComponentModel;
using MiniSqlParser;

namespace SqlAccessor
{
  /// <summary>
  /// ColumnInfoレコード用のSqlPod
  /// 指定されたDBMS種別に対応するテーブル列取得SELECT文を返す
  /// </summary>
  /// <remarks>スレッドセーフ</remarks>
  internal class ColumnInfoSqlPod: SqlPod
  {
    protected override SqlBuilder SelectSql() {
      if(Dbms == SqlBuilder.DbmsType.Sqlite) {
        return new SqlBuilder(this.GetSqliteSelectStmt(), this.Dbms);
      } else if(Dbms == SqlBuilder.DbmsType.MsSql) {
        return new SqlBuilder(this.GetMsSqlSelectStmt(), this.Dbms);
      } else if(Dbms == SqlBuilder.DbmsType.Oracle) {
        return new SqlBuilder(this.GetOracleSelectStmt(), this.Dbms);
      } else if(Dbms == SqlBuilder.DbmsType.Pervasive) {
        return new SqlBuilder(this.GetPervasiveSelectStmt(), this.Dbms);
      } else {
        throw new InvalidEnumArgumentException("Undefined DbmsType is used");
      }
    }

    private string GetSqliteSelectStmt() {
      return "/* テーブル名の指定がなければ、SQLITE_MASTERのメタ情報を取得する*/" +
             "/** @SQLite_TableName_=\"SQLITE_MASTER\" */" +
             "PRAGMA TABLE_INFO(@SQLite_TableName_)";
    }
    private string GetMsSqlSelectStmt() {
      return "SELECT" +
             "  T.TABLE_CATALOG" +
             " ,T.TABLE_SCHEMA             AS Owner" +
             " ,T.TABLE_NAME               AS TableName" +
             " ,C.COLUMN_NAME              AS ColumnName" +
             " ,C.DATA_TYPE                AS DataType" +
             " ,K.ORDINAL_POSITION         AS PrimaryKey" +
             " ,CASE C.IS_NULLABLE" +
             "    WHEN 'NO'  THEN 0" +
             "    WHEN 'YES' THEN 1" +
             "  END                        AS Nullable" +
             " ,C.COLUMN_DEFAULT           AS DefaultValue" +
             " ,C.CHARACTER_MAXIMUM_LENGTH AS MaxLength" +
             " ,NULL                       AS MinLength " +
             "FROM" +
             "  INFORMATION_SCHEMA.TABLES T" +
             "  JOIN INFORMATION_SCHEMA.COLUMNS C" +
             "  ON  T.TABLE_CATALOG = C.TABLE_CATALOG" +
             "  AND T.TABLE_SCHEMA  = C.TABLE_SCHEMA" +
             "  AND T.TABLE_NAME    = C.TABLE_NAME" +
             "  LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE K" +
             "  ON  C.TABLE_CATALOG = K.TABLE_CATALOG" +
             "  AND C.TABLE_SCHEMA  = K.TABLE_SCHEMA" +
             "  AND C.TABLE_NAME    = K.TABLE_NAME" +
             "  AND C.COLUMN_NAME   = K.COLUMN_NAME " +
             "WHERE" +
             "  TABLE_TYPE = 'BASE TABLE'" +
             "/* カタログテーブル自身のメタ情報 */" +
             "UNION ALL " +
             "SELECT" +
             "  NULL" +
             " ,S.NAME" +
             " ,V.NAME" +
             " ,C.NAME" +
             " ,''" +
             " ,CASE C.NAME" +
             "    WHEN 'TABLE_CATALOG' THEN 1" +
             "    WHEN 'TABLE_SCHEMA'  THEN 2" +
             "    WHEN 'TABLE_NAME'    THEN 3" +
             "    WHEN 'COLUMN_NAME'   THEN 4" +
             "    ELSE NULL" +
             "  END" +
             " ,C.IS_NULLABLE" +
             " ,NULL" +
             " ,C.MAX_LENGTH" +
             " ,NULL " +
             "FROM" +
             "  SYS.SCHEMAS S" +
             "  JOIN SYS.ALL_VIEWS V" +
             "  ON S.SCHEMA_ID = V.SCHEMA_ID" +
             "  JOIN SYS.ALL_COLUMNS C" +
             "  ON  V.OBJECT_ID = C.OBJECT_ID " +
             "WHERE" +
             "      S.NAME = 'sys'                AND V.NAME = 'schemas'" +
             "  OR  S.NAME = 'sys'                AND V.NAME = 'all_views'" +
             "  OR  S.NAME = 'sys'                AND V.NAME = 'all_columns'" +
             "  OR  S.NAME = 'INFORMATION_SCHEMA' AND V.NAME = 'TABLES'" +
             "  OR  S.NAME = 'INFORMATION_SCHEMA' AND V.NAME = 'COLUMNS'" +
             "  OR  S.NAME = 'INFORMATION_SCHEMA' AND V.NAME = 'KEY_COLUMN_USAGE'";
    }

    private string GetOracleSelectStmt() {
      return "SELECT" +
             "   ''              AS Owner" +
             "  ,UC.TABLE_NAME   AS TableName" +
             "  ,UC.COLUMN_NAME  AS ColumnName" +
             "  ,UC.DATA_TYPE    AS DataType" +
             "  ,COALESCE(" +
             "     ( SELECT '1' FROM USER_IND_COLUMNS UIC" +
             "       WHERE UIC.TABLE_NAME  = UC.TABLE_NAME" +
             "         AND UIC.COLUMN_NAME = UC.COLUMN_NAME" +
             "         AND EXISTS (SELECT * FROM USER_CONSTRAINTS UCS" +
             "                     WHERE UCS.INDEX_NAME = UIC.INDEX_NAME" +
             "                       AND UCS.CONSTRAINT_TYPE = 'P')" +
             "     )" +
             "    ,'0'" +
             "   )               AS PrimaryKey" +
             "  ,CASE UC.NULLABLE" +
             "     WHEN 'Y' THEN '1'" +
             "     ELSE '0'" +
             "   END             AS Nullable" +
             "  /* DATA_DEFAULT列はLONG型のため取得しない */" +
             "  ,''              AS DefaultValue" +
             "  /* 列のバイト単位での長さ */" +
             "  ,UC.DATA_LENGTH  AS MaxLength " +
             "FROM" +
             "   USER_TAB_COLUMNS UC" +
             "/* カタログテーブル自身のメタ情報 */" +
             "UNION ALL " +
             "SELECT" +
             "   AC.OWNER        AS Owner" +
             "  ,AC.TABLE_NAME   AS TableName" +
             "  ,AC.COLUMN_NAME  AS ColumnName" +
             "  ,AC.DATA_TYPE    AS DataType" +
             "  ,'0'             AS PrimaryKey" +
             "  ,CASE AC.NULLABLE" +
             "     WHEN 'Y' THEN '1'" +
             "     ELSE '0'" +
             "   END             AS Nullable" +
             "  ,''              AS DefaultValue" +
             "  ,AC.DATA_LENGTH  AS MaxLength " +
             "FROM" +
             "   ALL_TAB_COLUMNS AC " +
             "WHERE AC.TABLE_NAME IN ('USER_IND_COLUMNS','USER_TAB_COLUMNS','ALL_TAB_COLUMNS')";
    }

    private string GetPervasiveSelectStmt() {
      return "SELECT" +
             "   /* Owner名を格納するテーブルX$Userが存在しないため空文字とする */" +
             "  '' AS \"Owner\"," +
             "  \"X$File\".\"Xf$Name\" AS TableName," +
             "  \"X$Field\".\"Xe$Name\" AS ColumnName," +
             "  CASE \"X$Field\".\"Xe$DataType\"" +
             "    WHEN '0' THEN 'CHAR'" +
             "    WHEN '1' THEN 'INTEGER'" +
             "    WHEN '2' THEN 'FLOAT'" +
             "    WHEN '3' THEN 'DATE'" +
             "    WHEN '4' THEN 'TIME'" +
             "    WHEN '5' THEN 'DECIMAL'" +
             "    WHEN '6' THEN 'MONEY'" +
             "    WHEN '7' THEN 'LOGICAL'" +
             "    WHEN '8' THEN 'NUMERIC'" +
             "    WHEN '9' THEN 'BFLOAT'" +
             "    WHEN '10' THEN 'LSTRING'" +
             "    WHEN '11' THEN 'ZSTRING'" +
             "    WHEN '12' THEN 'NOTE'" +
             "    WHEN '13' THEN 'LVAR'" +
             "    WHEN '14' THEN 'UNSIGNED BINARY'" +
             "    WHEN '15' THEN 'AUTOINCREMENT'" +
             "    WHEN '16' THEN 'BIT'" +
             "    WHEN '17' THEN 'NUMERICSTS'" +
             "    WHEN '18' THEN 'NUMERICSA'" +
             "    WHEN '19' THEN 'CURRENCY'" +
             "    WHEN '20' THEN 'TIMESTAMP'" +
             "    WHEN '21' THEN 'BLOB'" +
             "  END AS DataType," +
             "  CASE" +
             "    WHEN (\"X$Index\".\"Xi$Flags\" / power(2, 14)) >= 1 THEN '1'" +
             "    ELSE '0'" +
             "  END AS PrimaryKey," +
             "  CASE" +
             "    WHEN \"X$Field\".\"Xe$Flags\" = '2' THEN '1'" +
             "    ELSE '0'" +
             "  END AS Nullable," +
             "  (SELECT \"X$Attrib\".\"Xa$Attrs\" + 'EOF'" +
             "   FROM   \"X$Attrib\"" +
             "   WHERE  \"X$Field\".\"Xe$Id\" = \"X$Attrib\".\"Xa$Id\"" +
             "  ) AS DefaultValue," +
             "  \"X$Field\".\"Xe$Size\" AS MaxLength" +
             "FROM" +
             "  \"X$File\"" +
             "  INNER JOIN \"X$Field\" ON (\"X$File\".\"Xf$Id\" = \"X$Field\".\"Xe$File\"" +
             "                             /* 制約名とインデックス名を除外する */" +
             "                             AND \"X$Field\".\"Xe$DataType\" <> 227" +
             "                             AND \"X$Field\".\"Xe$DataType\" <> 255)" +
             "  LEFT JOIN \"X$Index\" ON (\"X$Field\".\"Xe$Id\" = \"X$Index\".\"Xi$Field\"" +
             "                            AND PrimaryKey = '1')";
    }
  }
}
