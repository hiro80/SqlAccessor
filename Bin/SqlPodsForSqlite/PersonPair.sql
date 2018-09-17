<?xml version="1.0" encoding="UTF-8"?>
<sqlPod>
  <find><![CDATA[
  /** @Daimyou = "isDaimyou IN (0,1)" */
  SELECT
      P1.id        As Id
     ,P1.name      As Name
     ,P1.birthDay  As BirthDay
     ,P2.id        As SuccessorId
     ,P2.name      As SuccessorName
     ,P2.birthDay  As SuccessorBirthDay
  FROM
      Persons P1
      LEFT JOIN
      Persons P2
      ON P1.successor = P2.id
  ]]></find>

  <count>
  <![CDATA[
    
  ]]>
  </count>

  <save><![CDATA[
    UPDATE Persons /** P1 */ SET
        name      = @Name
       ,birthDay  = @BirthDay
       ,successor = @SuccessorId
    ;

    IF @LAST_AFFECTED_ROWS = 0 AND @Id IS NOT NULL THEN
        INSERT INTO Persons /** P1 */ (
            id
           ,name
           ,birthDay
           ,successor
        )VALUES(
            @Id
           ,@Name
           ,@BirthDay
           ,@SuccessorId
        )
    END IF
    ;

    UPDATE Persons /** P2 */ SET
        name      = @SuccessorName
       ,birthDay  = @SuccessorBirthDay
    ;

    IF @LAST_AFFECTED_ROWS = 0 AND @SuccessorId IS NOT NULL THEN
        INSERT INTO Persons /** P2 */ (
            id
           ,name
           ,birthDay
        )VALUES(
            @SuccessorId
           ,@SuccessorName
           ,@SuccessorBirthDay
        )
    END IF
  ]]></save>

  <insertSqls>
  </insertSqls>

  <delete><![CDATA[
    DELETE FROM Persons /** P1 */ ;
    DELETE FROM Persons /** P2 */ ;
  ]]></delete>
</sqlPod>
