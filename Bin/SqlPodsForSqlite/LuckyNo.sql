<?xml version="1.0" encoding="UTF-8"?>
<sqlPod>
  <find><![CDATA[
  SELECT
      date   As Date
     ,number As Number
  FROM
      NoPKeyTable
  ]]></find>

  <save><![CDATA[
    UPDATE NoPKeyTable SET
        date   = @Date
       ,number = @Number
    ;
    IF @LAST_AFFECTED_ROWS = 0 THEN
        INSERT INTO NoPKeyTable(
            date
           ,number
        )VALUES(
            @Date
           ,@Number
        )
    END IF
  ]]></save>

  <delete><![CDATA[
    DELETE FROM NoPKeyTable
  ]]></delete>
</sqlPod>
