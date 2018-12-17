using System.ComponentModel;

namespace SqlAccessor
{

  /// <summary>
  /// Cast処理前後での、Cast対象値のデータ編集を定義する
  /// </summary>
  /// <remarks>
  /// DBMSの種別に依存せずに定義できる
  /// 新たなプロパティ型に対しても定義できる
  /// </remarks>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public interface ICastEditor
  {
    #region "ADO.NETが返すデータ型 → プロパティ型"
    #region "ADO.NETが返すデータ型毎のCast前編集"
    object BeforeCast_ViewColumnType(StringViewColumnType viewColumnType
                                    , ViewColumnInfo aViewColumnInfo
                                    , object viewColumnValue);
    object BeforeCast_ViewColumnType(IntegerViewColumnType viewColumnType
                                    , ViewColumnInfo aViewColumnInfo
                                    , object viewColumnValue);
    object BeforeCast_ViewColumnType(LongViewColumnType viewColumnType
                                    , ViewColumnInfo aViewColumnInfo
                                    , object viewColumnValue);
    object BeforeCast_ViewColumnType(DecimalViewColumnType viewColumnType
                                    , ViewColumnInfo aViewColumnInfo
                                    , object viewColumnValue);
    object BeforeCast_ViewColumnType(DoubleViewColumnType viewColumnType
                                    , ViewColumnInfo aViewColumnInfo
                                    , object viewColumnValue);
    object BeforeCast_ViewColumnType(DateTimeViewColumnType viewColumnType
                                    , ViewColumnInfo aViewColumnInfo
                                    , object viewColumnValue);
    object BeforeCast_ViewColumnType(TimeSpanViewColumnType viewColumnType
                                    , ViewColumnInfo aViewColumnInfo
                                    , object viewColumnValue);
    object BeforeCast_ViewColumnType(BooleanViewColumnType viewColumnType
                                    , ViewColumnInfo aViewColumnInfo
                                    , object viewColumnValue);
    #endregion

    #region "ADO.NETが返すデータ型毎のCast後編集"
    object AfterCast_ViewColumnType(StringViewColumnType viewColumnType
                                  , ViewColumnInfo aViewColumnInfo
                                  , object propertyValue);
    object AfterCast_ViewColumnType(IntegerViewColumnType viewColumnType
                                  , ViewColumnInfo aViewColumnInfo
                                  , object propertyValue);
    object AfterCast_ViewColumnType(LongViewColumnType viewColumnType
                                  , ViewColumnInfo aViewColumnInfo
                                  , object propertyValue);
    object AfterCast_ViewColumnType(DecimalViewColumnType viewColumnType
                                  , ViewColumnInfo aViewColumnInfo
                                  , object propertyValue);
    object AfterCast_ViewColumnType(DoubleViewColumnType viewColumnType
                                  , ViewColumnInfo aViewColumnInfo
                                  , object propertyValue);
    object AfterCast_ViewColumnType(DateTimeViewColumnType viewColumnType
                                  , ViewColumnInfo aViewColumnInfo
                                  , object propertyValue);
    object AfterCast_ViewColumnType(TimeSpanViewColumnType viewColumnType
                                  , ViewColumnInfo aViewColumnInfo
                                  , object propertyValue);
    object AfterCast_ViewColumnType(BooleanViewColumnType viewColumnType
                                  , ViewColumnInfo aViewColumnInfo
                                  , object propertyValue);
    #endregion

    //プロパティ型毎のCast前編集
    object BeforeCast_PropertyType(PropertyType propertyType
                                  , ViewColumnInfo aViewColumnInfo
                                  , object viewColumnValue);
    //プロパティ型毎のCast後編集
    object AfterCast_PropertyType(PropertyType propertyType
                                , ViewColumnInfo aViewColumnInfo
                                , object propertyValue);

    #region "ADO.NETが返すデータ型とプロパティ型毎のCast前編集"
    object BeforeCast(StringViewColumnType viewColumnType
                    , PropertyType propertyType
                    , ViewColumnInfo aViewColumnInfo
                    , object viewColumnValue);
    object BeforeCast(IntegerViewColumnType viewColumnType
                    , PropertyType propertyType
                    , ViewColumnInfo aViewColumnInfo
                    , object viewColumnValue);
    object BeforeCast(LongViewColumnType viewColumnType
                    , PropertyType propertyType
                    , ViewColumnInfo aViewColumnInfo
                    , object viewColumnValue);
    object BeforeCast(DecimalViewColumnType viewColumnType
                    , PropertyType propertyType
                    , ViewColumnInfo aViewColumnInfo
                    , object viewColumnValue);
    object BeforeCast(DoubleViewColumnType viewColumnType
                    , PropertyType propertyType
                    , ViewColumnInfo aViewColumnInfo
                    , object viewColumnValue);
    object BeforeCast(DateTimeViewColumnType viewColumnType
                    , PropertyType propertyType
                    , ViewColumnInfo aViewColumnInfo
                    , object viewColumnValue);
    object BeforeCast(TimeSpanViewColumnType viewColumnType
                    , PropertyType propertyType
                    , ViewColumnInfo aViewColumnInfo
                    , object viewColumnValue);
    object BeforeCast(BooleanViewColumnType viewColumnType
                    , PropertyType propertyType
                    , ViewColumnInfo aViewColumnInfo
                    , object viewColumnValue);
    #endregion

    #region "ADO.NETが返すデータ型とプロパティ型毎のCast後編集"
    object AfterCast(StringViewColumnType viewColumnType
                    , PropertyType propertyType
                    , ViewColumnInfo aViewColumnInfo
                    , object propertyValue);
    object AfterCast(IntegerViewColumnType viewColumnType
                    , PropertyType propertyType
                    , ViewColumnInfo aViewColumnInfo
                    , object propertyValue);
    object AfterCast(LongViewColumnType viewColumnType
                    , PropertyType propertyType
                    , ViewColumnInfo aViewColumnInfo
                    , object propertyValue);
    object AfterCast(DecimalViewColumnType viewColumnType
                    , PropertyType propertyType
                    , ViewColumnInfo aViewColumnInfo
                    , object propertyValue);
    object AfterCast(DoubleViewColumnType viewColumnType
                    , PropertyType propertyType
                    , ViewColumnInfo aViewColumnInfo
                    , object propertyValue);
    object AfterCast(DateTimeViewColumnType viewColumnType
                    , PropertyType propertyType
                    , ViewColumnInfo aViewColumnInfo
                    , object propertyValue);
    object AfterCast(TimeSpanViewColumnType viewColumnType
                    , PropertyType propertyType
                    , ViewColumnInfo aViewColumnInfo
                    , object propertyValue);
    object AfterCast(BooleanViewColumnType viewColumnType
                    , PropertyType propertyType
                    , ViewColumnInfo aViewColumnInfo
                    , object propertyValue);
    #endregion

    #endregion

    #region "プロパティ型 → SQLリテラル型"
    #region "SQLリテラル型毎のCast前編集"
    object BeforeCast_SqlLiteralType(StringSqlLiteralType sqlLiteralType
                                    , ColumnInfo aColumnInfo
                                    , object propertyValue);
    object BeforeCast_SqlLiteralType(NumberSqlLiteralType sqlLiteralType
                                    , ColumnInfo aColumnInfo
                                    , object propertyValue);
    object BeforeCast_SqlLiteralType(DateTimeSqlLiteralType sqlLiteralType
                                    , ColumnInfo aColumnInfo
                                    , object propertyValue);
    object BeforeCast_SqlLiteralType(OracleDateTimeSqlLiteralType sqlLiteralType
                                    , ColumnInfo aColumnInfo
                                    , object propertyValue);
    object BeforeCast_SqlLiteralType(DoubleSqlLiteralType sqlLiteralType
                                    , ColumnInfo aColumnInfo
                                    , object propertyValue);
    object BeforeCast_SqlLiteralType(IntervalSqlLiteralType sqlLiteralType
                                    , ColumnInfo aColumnInfo
                                    , object propertyValue);
    #endregion

    #region "SQLリテラル型毎のCast後編集"
    string AfterCast_SqlLiteralType(StringSqlLiteralType sqlLiteralType
                                  , ColumnInfo aColumnInfo
                                  , string sqlLiteralValue);
    string AfterCast_SqlLiteralType(NumberSqlLiteralType sqlLiteralType
                                  , ColumnInfo aColumnInfo
                                  , string sqlLiteralValue);
    string AfterCast_SqlLiteralType(DateTimeSqlLiteralType sqlLiteralType
                                  , ColumnInfo aColumnInfo
                                  , string sqlLiteralValue);
    string AfterCast_SqlLiteralType(OracleDateTimeSqlLiteralType sqlLiteralType
                                  , ColumnInfo aColumnInfo
                                  , string sqlLiteralValue);
    string AfterCast_SqlLiteralType(DoubleSqlLiteralType sqlLiteralType
                                  , ColumnInfo aColumnInfo
                                  , string sqlLiteralValue);
    string AfterCast_SqlLiteralType(IntervalSqlLiteralType sqlLiteralType
                                  , ColumnInfo aColumnInfo
                                  , string sqlLiteralValue);
    #endregion

    //プロパティ型毎のCast前編集
    object BeforeCast_PropertyType(PropertyType propertyType
                                  , ColumnInfo aColumnInfo
                                  , object propertyValue);
    //プロパティ型毎のCast後編集
    string AfterCast_PropertyType(PropertyType propertyType
                                , ColumnInfo aColumnInfo
                                , string sqlLiteralValue);

    #region "SQLリテラル型とプロパティ型毎のCast前編集"
    object BeforeCast(StringSqlLiteralType sqlLiteralType
                    , PropertyType propertyType
                    , ColumnInfo aColumnInfo
                    , object propertyValue);
    object BeforeCast(NumberSqlLiteralType sqlLiteralType
                    , PropertyType propertyType
                    , ColumnInfo aColumnInfo
                    , object propertyValue);
    object BeforeCast(DateTimeSqlLiteralType sqlLiteralType
                    , PropertyType propertyType
                    , ColumnInfo aColumnInfo
                    , object propertyValue);
    object BeforeCast(OracleDateTimeSqlLiteralType sqlLiteralType
                    , PropertyType propertyType
                    , ColumnInfo aColumnInfo
                    , object propertyValue);
    object BeforeCast(DoubleSqlLiteralType sqlLiteralType
                    , PropertyType propertyType
                    , ColumnInfo aColumnInfo
                    , object propertyValue);
    object BeforeCast(IntervalSqlLiteralType sqlLiteralType
                    , PropertyType propertyType
                    , ColumnInfo aColumnInfo
                    , object propertyValue);
    #endregion

    #region "SQLリテラル型とプロパティ型毎のCast後編集"
    string AfterCast(StringSqlLiteralType sqlLiteralType
                    , PropertyType propertyType
                    , ColumnInfo aColumnInfo
                    , string sqlLiteralValue);
    string AfterCast(NumberSqlLiteralType sqlLiteralType
                    , PropertyType propertyType
                    , ColumnInfo aColumnInfo
                    , string sqlLiteralValue);
    string AfterCast(DateTimeSqlLiteralType sqlLiteralType
                    , PropertyType propertyType
                    , ColumnInfo aColumnInfo
                    , string sqlLiteralValue);
    string AfterCast(OracleDateTimeSqlLiteralType sqlLiteralType
                    , PropertyType propertyType
                    , ColumnInfo aColumnInfo
                    , string sqlLiteralValue);
    string AfterCast(DoubleSqlLiteralType sqlLiteralType
                    , PropertyType propertyType
                    , ColumnInfo aColumnInfo
                    , string sqlLiteralValue);
    string AfterCast(IntervalSqlLiteralType sqlLiteralType
                    , PropertyType propertyType
                    , ColumnInfo aColumnInfo
                    , string sqlLiteralValue);
    #endregion
    #endregion

  }
}
