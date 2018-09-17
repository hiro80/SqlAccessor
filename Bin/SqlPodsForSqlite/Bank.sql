<?xml version="1.0" encoding="UTF-8"?>
<sqlPod>
  <find><![CDATA[
  SELECT
      B.BANK_CD  AS BankId
     ,B.BANK_NM  AS BankName
     ,B.BANK_NMK AS BankKanaName
     ,K.STN_CD   AS BranchId
     ,K.STN_NM   AS BranchName
     ,K.STN_NMK  AS BranchKanaName
  FROM
      CM_BANK  B LEFT JOIN
      CM_KINYU K
      ON B.BANK_CD = K.BANK_CD
  WHERE
        B.DELFLG = 0
    AND K.DELFLG = 0
  ]]></find>

  <save><![CDATA[
    UPDATE CM_BANK B SET
      BANK_NM     = @BankName
     ,BANK_NMK    = @BankKanaName
     ,DELFLG      = 0
     ,UPD_SHOKUIN = 'tester'
     ,UPD_DATE    = @CURRENT_DATE
     ,UPD_TIME    = TRUNC(@CURRENT_TIMESTAMP/10)
    ;

    IF @LAST_AFFECTED_ROWS = 0 THEN
      INSERT INTO CM_BANK(
        BANK_CD
       ,BANK_NM
       ,BANK_NMK
       ,DELFLG
       ,INS_CLIENT
       ,INS_SHOKUIN
       ,INS_DATE
       ,INS_TIME
       ,UPD_SHOKUIN
       ,UPD_DATE
       ,UPD_TIME
      )VALUES(
        @BankId
       ,@BankName
       ,@BankKanaName
       ,0
       ,'::1'
       ,'tester'
       ,@CURRENT_DATE
       ,TRUNC(@CURRENT_TIMESTAMP/10)
       ,'tester'
       ,@CURRENT_DATE
       ,TRUNC(@CURRENT_TIMESTAMP/10)
      )
    END IF
    ;

    UPDATE CM_KINYU K SET
      BANK_NM = @BankName
     ,STN_NM = @BranchName
     ,STN_NMK = @BranchKanaName
     ,DELFLG = 0
     ,UPD_SHOKUIN = 'tester'
     ,UPD_DATE = @CURRENT_DATE
     ,UPD_TIME = TRUNC(@CURRENT_TIMESTAMP/10)
    ;

    IF @LAST_AFFECTED_ROWS = 0 THEN
      INSERT INTO CM_KINYU(
        BANK_CD
       ,STN_CD
       ,BANK_NM
       ,STN_NM
       ,STN_NMK
       ,DELFLG
       ,INS_CLIENT
       ,INS_SHOKUIN
       ,INS_DATE
       ,INS_TIME
       ,UPD_SHOKUIN
       ,UPD_DATE
       ,UPD_TIME
      )VALUES(
        @BankId
       ,@BranchId
       ,@BankName
       ,@BranchName
       ,@BranchKanaName
       ,0
       ,'::1'
       ,'tester'
       ,@CURRENT_DATE
       ,TRUNC(@CURRENT_TIMESTAMP/10)
       ,'tester'
       ,@CURRENT_DATE
       ,TRUNC(@CURRENT_TIMESTAMP/10)
      )
    END IF
  ]]></save>

  <insertSqls>
  </insertSqls>

  <delete><![CDATA[
    DELETE FROM CM_BANK B  ;
    DELETE FROM CM_KINYU K ;
  ]]></delete>
</sqlPod>
