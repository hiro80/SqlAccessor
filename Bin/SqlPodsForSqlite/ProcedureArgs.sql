<?xml version="1.0" encoding="UTF-8"?>
<sqlPod>

  <select><![CDATA[
  SELECT '' AS TaisyoYm FROM DUAL
  ]]></select>

  <saveSqls><![CDATA[
    SELECT * FROM KM_KOKYAKU_SFK    ;
    CALL SF030F010_PLSQL(@TaisyoYm) ;
  ]]></saveSqls>

</sqlPod>
