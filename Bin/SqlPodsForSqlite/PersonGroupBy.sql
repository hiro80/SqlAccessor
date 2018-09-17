<?xml version="1.0" encoding="UTF-8"?>
<sqlPod>
  <find><![CDATA[
  /** @Daimyou = "isDaimyou IN (0,1)" */
  SELECT
      count(*) As "Count"
     ,birthDay As BirthDay
  FROM
      Persons
  GROUP BY
      birthDay
  ORDER BY
      birthDay, "Count"
  ]]></find>

  <changeBirthDay><![CDATA[
    /* 指定した誕生日をすべて変更する */
    UPDATE Persons SET
        birthDay  = @BirthDay
  ]]></changeBirthDay>

  <delete><![CDATA[
    DELETE FROM Persons
  ]]></delete>
</sqlPod>
