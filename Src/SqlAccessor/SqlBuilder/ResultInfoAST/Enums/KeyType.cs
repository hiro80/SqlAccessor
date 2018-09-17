namespace MiniSqlParser
{
  public enum KeyType
  {
    /// <summary>
    /// キーではない
    /// </summary>
    None,
    /// <summary>
    /// テーブルの主キー列、又はその列を参照する列 (Not Nullである)
    /// </summary>
    Table,
    /// <summary>
    /// Inline View/Tableから集約関数、GROUPBY、またはDISTINCTによって
    /// 行を一意に特定できる列、又はその列を参照する列 (Not Nullで無い)
    /// </summary>
    Group,
    /// <summary>
    /// Inline View/TableのGroupキーだけでは行の有無が判定できない場合
    /// COUNT()によりその有無を判定できる列、又はその列を参照する列 (Not Nullである)
    /// </summary>
    Count
  }
}
