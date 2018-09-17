<?xml version="1.0" encoding="UTF-8"?>
<!-- use for SQLite -->
<sqlPod>
  <find><![CDATA[
  /* テーブル名の指定がなければ、SQLITE_MASTERのメタ情報を取得する*/
  /** @SQLite_TableName_="SQLITE_MASTER" */
  PRAGMA TABLE_INFO(@SQLite_TableName_)
  ]]></find>

  <count>
  <![CDATA[
    
  ]]>
  </count>

  <save />

  <delete>
    <sql />
    <sql> </sql>
    <sql><![CDATA[ ]]></sql>
    <sql><!-- this is a comment --></sql>
    <sql></sql>
  </delete>
</sqlPod>
