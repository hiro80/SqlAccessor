namespace SqlAccessor
{
  /// <summary>
  /// DBMSのネイティブデータ型とSQLリテラル型の対応を定義する
  /// </summary>
  /// <remarks></remarks>
  internal interface IDataTypeMapper
  {
    /// <summary>
    /// ADO.NETから取得したDBMSのネイティブデータ型から、対応するSQLリテラル型を取得する
    /// </summary>
    /// <param name="aViewColumnInfo">View列のメタ情報</param>
    /// <returns>対応するSQLリテラル型</returns>
    /// <remarks></remarks>
    SqlLiteralType GetSqlLiteralTypeOf(ViewColumnInfo aViewColumnInfo);
  }
}
