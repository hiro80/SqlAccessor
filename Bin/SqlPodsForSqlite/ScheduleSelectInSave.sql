<?xml version="1.0" encoding="UTF-8"?>
<sqlPod>
  <find><![CDATA[
  /** @Daimyou = "isDaimyou IN (0,1)" */
  SELECT
      P.id        As Id
     ,P.name      As Name
     ,P.birthDay  As BirthDay
     ,P.height    As Height
     ,P.weight    As Weight
     ,P.isDaimyou As IsDaimyou
     ,P.remarks   As Remarks
     ,S.date      As "Date"
     ,S.subject   As Subject
  FROM
      Persons P
      LEFT JOIN
      Schedules S
      ON P.id = S.id
/*  WHERE
      @Daimyou */
  ]]></find>

  <save><![CDATA[
    /* 更新時にはPersonsの対象行をロックする */
    SELECT
        P.id        As Id
       ,P.name      As Name
       ,P.birthDay  As BirthDay
       ,P.height    As Height
       ,P.weight    As Weight
       ,P.isDaimyou As IsDaimyou
       ,P.remarks   As Remarks
       ,S.date      As "Date"
       ,S.subject   As Subject
    FROM
        Persons P
        JOIN
        Schedules S
        ON P.id = S.id
    ;

    UPDATE Schedules /** S */ SET
        subject  = @Subject
    ;

    IF @LAST_AFFECTED_ROWS = 0 THEN
        INSERT INTO Schedules /** S */ (
            id
           ,date
           ,subject
        )VALUES(
            @Id
           ,@Date
           ,@Subject
        )
    END IF
  ]]></save>

  <delete><![CDATA[
    DELETE FROM Persons   /** P */ ;
    DELETE FROM Schedules /** S */ ;
  ]]></delete>
</sqlPod>
