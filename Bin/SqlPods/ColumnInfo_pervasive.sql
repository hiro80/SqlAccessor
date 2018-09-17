<?xml version="1.0" encoding="UTF-8"?>
<!-- use for Pervasive SQL -->
<sqlPod>
  <find><![CDATA[
  SELECT
       /* Owner名を格納するテーブルX$Userが存在しないため空文字とする */
      '' AS "Owner",
      "X$File"."Xf$Name" AS TableName,
      "X$Field"."Xe$Name" AS ColumnName,
      CASE "X$Field"."Xe$DataType"
        WHEN '0' THEN 'CHAR'
        WHEN '1' THEN 'INTEGER'
        WHEN '2' THEN 'FLOAT'
        WHEN '3' THEN 'DATE'
        WHEN '4' THEN 'TIME'
        WHEN '5' THEN 'DECIMAL'
        WHEN '6' THEN 'MONEY'
        WHEN '7' THEN 'LOGICAL'
        WHEN '8' THEN 'NUMERIC'
        WHEN '9' THEN 'BFLOAT'
        WHEN '10' THEN 'LSTRING'
        WHEN '11' THEN 'ZSTRING'
        WHEN '12' THEN 'NOTE'
        WHEN '13' THEN 'LVAR'
        WHEN '14' THEN 'UNSIGNED BINARY'
        WHEN '15' THEN 'AUTOINCREMENT'
        WHEN '16' THEN 'BIT'
        WHEN '17' THEN 'NUMERICSTS'
        WHEN '18' THEN 'NUMERICSA'
        WHEN '19' THEN 'CURRENCY'
        WHEN '20' THEN 'TIMESTAMP'
        WHEN '21' THEN 'BLOB'
      END AS DataType,
      CASE
        WHEN ("X$Index"."Xi$Flags" / power(2, 14)) >= 1 THEN '1'
        ELSE '0'
      END AS PrimaryKey,
      CASE
        WHEN "X$Field"."Xe$Flags" = '2' THEN '1'
        ELSE '0'
      END AS Nullable,
      (SELECT "X$Attrib"."Xa$Attrs" + 'EOF'
       FROM   "X$Attrib"
       WHERE  "X$Field"."Xe$Id" = "X$Attrib"."Xa$Id"
      ) AS DefaultValue,
      "X$Field"."Xe$Size" AS MaxLength
  FROM
      "X$File"
      INNER JOIN "X$Field" ON ("X$File"."Xf$Id" = "X$Field"."Xe$File"
                                 /* 制約名とインデックス名を除外する */
                                 AND "X$Field"."Xe$DataType" <> 227
                                 AND "X$Field"."Xe$DataType" <> 255)
      LEFT JOIN "X$Index" ON ("X$Field"."Xe$Id" = "X$Index"."Xi$Field"
                                AND PrimaryKey = '1')
  ]]></find>

  <count>
  <![CDATA[
    
  ]]>
  </count>

  <saveSqls />

  <updateSqls>
    <sql></sql>
    <sql></sql>
  </updateSqls>

  <insertSqls>
    <sql></sql>
    <sql></sql>
  </insertSqls>

  <deleteSqls>
    <sql />
    <sql> </sql>
    <sql><![CDATA[ ]]></sql>
    <sql></sql>
    <sql></sql>
  </deleteSqls>
</sqlPod>
