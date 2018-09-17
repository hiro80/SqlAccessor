<?xml version="1.0" encoding="UTF-8"?>
<sqlPod>
  <find><![CDATA[
  /** @Daimyou = "isDaimyou IN (0,1)" */
  SELECT
      id        As Id
     ,name      As Name
     ,birthDay  As BirthDay
     ,height    As Height
     ,weight    As Weight
     ,isDaimyou As IsDaimyou
     ,remarks   As Remarks
  FROM
      Persons
/*  WHERE
      @Daimyou */
  ]]></find>

  <save><![CDATA[
  IF @Id IS NOT NULL THEN
      UPDATE Persons SET
          name      = @Name
         ,birthDay  = @BirthDay
         ,height    = @Height
         ,weight    = @Weight
         ,isDaimyou = @IsDaimyou
         ,remarks   = @Remarks
      ;

      IF @LAST_AFFECTED_ROWS = 0 THEN
          INSERT INTO Persons(
              id
             ,name
             ,birthDay
             ,height
             ,weight
             ,isDaimyou
             ,remarks
          )VALUES(
              @Id
             ,@Name
             ,@BirthDay
             ,@Height
             ,@Weight
             ,@IsDaimyou
             ,@Remarks
          )
      END IF
  END IF
  ]]></save>

  <saveSchedule><![CDATA[
    /* findで参照されないテーブルを更新する */
    UPDATE Schedules SET
        id      = @Id
       ,date    = @Date
       ,subject = @Subject
  ]]></saveSchedule>

  <delete><![CDATA[
    DELETE FROM Persons
  ]]></delete>
</sqlPod>
