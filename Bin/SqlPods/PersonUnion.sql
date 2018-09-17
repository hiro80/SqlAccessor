<?xml version="1.0" encoding="UTF-8"?>
<sqlPod>
  <find><![CDATA[
  /** @Daimyou = "isDaimyou IN (0,1)" */
  SELECT
      id        As Id
     ,name      As Name
     ,birthDay  As BirthDay
  FROM
      Persons
  UNION
  SELECT
      0
     ,N'名無しの権兵衛'
     ,'2001-01-01'
  ]]></find>

  <save><![CDATA[
    UPDATE Persons SET
        name      = @Name
       ,birthDay  = @BirthDay
    ;

    IF @LAST_AFFECTED_ROWS = 0 THEN
        INSERT INTO Persons(
            id
           ,name
           ,birthDay
        )VALUES(
            @Id
           ,@Name
           ,@BirthDay
        )
    END IF
  ]]></save>

  <insertSqls>
  </insertSqls>

  <delete><![CDATA[
    DELETE FROM Persons WHERE Id > 0
  ]]></delete>
</sqlPod>
