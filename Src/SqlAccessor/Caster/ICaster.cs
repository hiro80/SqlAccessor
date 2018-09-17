namespace SqlAccessor
{
  internal interface ICaster
  {
    /// <summary>
    /// 指定したプロパティ値がNULL表現値であればTrueを返す
    /// </summary>
    /// <param name="propertyValue">プロパティ値</param>
    /// <returns></returns>
    /// <remarks></remarks>
    bool IsNullPropertyValue(object propertyValue);

    /// <summary>
    /// 値をプロパティ型に変換する
    /// </summary>
    /// <param name="value">値</param>
    /// <param name="propertyTypeObj">変換先プロパティの型情報</param>
    /// <returns>変換結果</returns>
    /// <remarks>View列に紐付かない値を指定したプロパティ型に変換する(DbAccessDataSource用)</remarks>
    object CastToPropertyType(object value, System.Type propertyTypeObj);

    /// <summary>
    /// ADO.NETから取得した値をプロパティ型に変換する
    /// </summary>
    /// <param name="viewColumnValue">ADO.NETから取得したSELECT句の値</param>
    /// <param name="aViewColumnInfo">columnValueの型情報</param>
    /// <param name="propertyTypeObj">変換先プロパティの型情報</param>
    /// <param name="aColumnInfo">1対1で対応するテーブルカラムが存在する場合に指定する</param>
    /// <returns>変換結果</returns>
    /// <remarks></remarks>
    object CastToPropertyType(object viewColumnValue
                            , ViewColumnInfo aViewColumnInfo
                            , System.Type propertyTypeObj
                            , ColumnInfo aColumnInfo = null);
    /// <summary>
    /// レコードのプロパティ値をSQLリテラル型に変換する
    /// </summary>
    /// <param name="propertyValue">プロパティ値</param>
    /// <param name="aViewColumnInfo">プロパティ値の取得元SELECT句の型情報</param>
    /// <param name="aColumnInfo">1対1で対応するテーブルカラムが存在する場合に指定する</param>
    /// <returns>変換結果</returns>
    /// <remarks>
    /// プロパティ値からリフレクション機能によって型情報を取得できるが
    /// Nullable型か否かという情報は欠落するため、
    /// プロパティ値の型情報も引数で指定する必要がある
    /// </remarks>
    string CastToSqlLiteralType(object propertyValue
                              , ViewColumnInfo aViewColumnInfo
                              , ColumnInfo aColumnInfo = null);
  }
}
