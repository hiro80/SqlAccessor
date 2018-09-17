<?xml version="1.0" encoding="UTF-8"?>
<sqlPod>
  <find><![CDATA[
  /** プレースホルダ初期値は後に記述されたほうを優先する */
  /** @Daimyou = "isDaimyou IN (0,0)" */
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
  WHERE
      @Daimyou
  ]]></find>

  <count>
  <![CDATA[
    
  ]]>
  </count>

  <save><![CDATA[
    /** @Id = "3" */
    /** @Id = "4" */
    UPDATE Persons SET
        name      = @Name
       ,birthDay  = @BirthDay
       ,height    = @Height
       ,weight    = @Weight
       ,isDaimyou = @IsDaimyou
       ,remarks   = @Remarks
    WHERE
        id = @Id
    ;
    /** @Name = "N'德川秀忠'" */
    ;

    /** @BirthDay = "'1600-01-01'" */
    IF @LAST_AFFECTED_ROWS = 0 THEN
        /** @Height = "177" */
        /** @Weight = "90"  */
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
  ]]></save>

  <save2><![CDATA[
    /** @Id       = "5"           */
    /** @Name     = "'德川秀忠'" */
    /** @BirthDay = "'1600-01-01'"*/
    /** @Height   = "188"         */
    /** @Weight   = "78"          */
    /** @IsDaimyou= "1"           */
    /** @Remarks  = "'二代将軍'" */
    UPDATE Persons SET
        name      = @Name
       ,birthDay  = @BirthDay
       ,height    = @Height
       ,weight    = @Weight
       ,isDaimyou = @IsDaimyou
       ,remarks   = @Remarks
    WHERE
        id = @Id
    ;
    /* 以下ののコメントは直前のUPDATE文に属する、     */
    /* 従ってSQL文の先頭に位置しないので              */
    /* プレースホルダの初期値コメントとして認識しない */
    /** @Name = "'德川家光'" */
    ;

    /** @BirthDay = "'1610-04-04'" */
    IF @LAST_AFFECTED_ROWS = 0 THEN
        /** @Height = "177" */
        /** @Weight = "90"  */
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
  ]]></save2>

  <delete><![CDATA[
    DELETE FROM Persons
  ]]></delete>
</sqlPod>
