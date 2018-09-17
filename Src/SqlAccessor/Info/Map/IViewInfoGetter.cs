namespace SqlAccessor
{
  /// <summary>
  /// ViewInfoとViewColumnInfonの取得機能を表す
  /// </summary>
  /// <remarks>
  /// IExp.CastToSqlLiteralType()の引数の型に用いるために用意したインタフェース
  /// </remarks>
  internal interface IViewInfoGetter
  {
    ViewInfo GetViewInfo();
    ViewColumnInfo GetViewColumnInfo(string propertyName);
  }
}
