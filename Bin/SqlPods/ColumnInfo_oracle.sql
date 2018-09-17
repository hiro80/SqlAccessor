<?xml version="1.0" encoding="UTF-8"?>
<!-- use for ORACLE -->
<sqlPod>
  <find><![CDATA[
  SELECT
     ''              AS Owner
    ,UC.TABLE_NAME   AS TableName
    ,UC.COLUMN_NAME  AS ColumnName
    ,UC.DATA_TYPE    AS DataType
    ,COALESCE(
       ( SELECT '1' FROM USER_IND_COLUMNS UIC
         WHERE UIC.TABLE_NAME  = UC.TABLE_NAME
           AND UIC.COLUMN_NAME = UC.COLUMN_NAME
           AND EXISTS (SELECT * FROM USER_CONSTRAINTS UCS
                       WHERE UCS.INDEX_NAME = UIC.INDEX_NAME
                         AND UCS.CONSTRAINT_TYPE = 'P')
       )
      ,'0'
     )               AS PrimaryKey
    ,CASE UC.NULLABLE
       WHEN 'Y' THEN '1'
       ELSE '0'
     END             AS Nullable
    /* DATA_DEFAULT列はLONG型のため取得しない */
    ,''              AS DefaultValue
    /* 列のバイト単位での長さ */
    ,UC.DATA_LENGTH  AS MaxLength
  FROM
     USER_TAB_COLUMNS UC
  /* カタログテーブル自身のメタ情報 */
  UNION ALL
  SELECT
     AC.OWNER        AS Owner
    ,AC.TABLE_NAME   AS TableName
    ,AC.COLUMN_NAME  AS ColumnName
    ,AC.DATA_TYPE    AS DataType
    ,'0'             AS PrimaryKey
    ,CASE AC.NULLABLE
       WHEN 'Y' THEN '1'
       ELSE '0'
     END             AS Nullable
    ,''              AS DefaultValue
    ,AC.DATA_LENGTH  AS MaxLength 
  FROM
     ALL_TAB_COLUMNS AC
  WHERE AC.TABLE_NAME IN ('USER_IND_COLUMNS','USER_TAB_COLUMNS','ALL_TAB_COLUMNS')
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
