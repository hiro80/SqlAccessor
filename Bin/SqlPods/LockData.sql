<?xml version="1.0" encoding="UTF-8"?>
<sqlPod>
  <find><![CDATA[
  SELECT
       lockId     AS LockId
      ,apTranId   AS ApTranId
      ,recordName AS RecordName
      ,tableOwner AS TableOwner
      ,tableName  AS TableName
      ,predicate  AS PredicateStr
  FROM
      LockData
  ]]></find>

  <save><![CDATA[
  INSERT INTO LockData(
      lockId
     ,apTranId
     ,recordName
     ,tableOwner
     ,tableName
     ,predicate
  )VALUES(
      @LockId
     ,@ApTranId
     ,@RecordName
     ,@TableOwner
     ,@TableName
     ,@PredicateStr
  )
  ]]></save>

  <delete><![CDATA[
    DELETE FROM LockData
  ]]></delete>
</sqlPod>
