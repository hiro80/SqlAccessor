<?xml version="1.0" encoding="UTF-8"?>
<!-- use for ORACLE -->
<sqlPod>
  <find><![CDATA[
  SELECT
    T.TABLE_CATALOG
   ,T.TABLE_SCHEMA             AS Owner
   ,T.TABLE_NAME               AS TableName
   ,C.COLUMN_NAME              AS ColumnName
   ,C.DATA_TYPE                AS DataType
   ,K.ORDINAL_POSITION         AS PrimaryKey
   ,CASE C.IS_NULLABLE
      WHEN 'NO'  THEN 0
      WHEN 'YES' THEN 1
    END                        AS Nullable
   ,C.COLUMN_DEFAULT           AS DefaultValue
   ,C.CHARACTER_MAXIMUM_LENGTH AS MaxLength
   ,NULL                       AS MinLength
  FROM
    INFORMATION_SCHEMA.TABLES T
    JOIN INFORMATION_SCHEMA.COLUMNS C
    ON  T.TABLE_CATALOG = C.TABLE_CATALOG
    AND T.TABLE_SCHEMA  = C.TABLE_SCHEMA
    AND T.TABLE_NAME    = C.TABLE_NAME
    LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE K
    ON  C.TABLE_CATALOG = K.TABLE_CATALOG
    AND C.TABLE_SCHEMA  = K.TABLE_SCHEMA
    AND C.TABLE_NAME    = K.TABLE_NAME
    AND C.COLUMN_NAME   = K.COLUMN_NAME
  WHERE
    TABLE_TYPE = 'BASE TABLE'
  /* カタログテーブル自身のメタ情報 */
  UNION ALL
  SELECT
    NULL
   ,S.NAME
   ,V.NAME
   ,C.NAME
   ,''
   ,CASE C.NAME
      WHEN 'TABLE_CATALOG' THEN 1
      WHEN 'TABLE_SCHEMA'  THEN 2
      WHEN 'TABLE_NAME'    THEN 3
      WHEN 'COLUMN_NAME'   THEN 4
      ELSE NULL
    END
   ,C.IS_NULLABLE
   ,NULL
   ,C.MAX_LENGTH
   ,NULL
  FROM
    SYS.SCHEMAS S
    JOIN SYS.ALL_VIEWS V
    ON S.SCHEMA_ID = V.SCHEMA_ID
    JOIN SYS.ALL_COLUMNS C
    ON  V.OBJECT_ID = C.OBJECT_ID
  WHERE
        S.NAME = 'sys'                AND V.NAME = 'schemas'
    OR  S.NAME = 'sys'                AND V.NAME = 'all_views'
    OR  S.NAME = 'sys'                AND V.NAME = 'all_columns'
    OR  S.NAME = 'INFORMATION_SCHEMA' AND V.NAME = 'TABLES'
    OR  S.NAME = 'INFORMATION_SCHEMA' AND V.NAME = 'COLUMNS'
    OR  S.NAME = 'INFORMATION_SCHEMA' AND V.NAME = 'KEY_COLUMN_USAGE'
  ]]></find>
</sqlPod>
