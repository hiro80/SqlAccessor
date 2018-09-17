using System.ComponentModel;

namespace SqlAccessor
{
  /// <summary>
  /// 
  /// </summary>
  /// <remarks>スレッドセーフ</remarks>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class CastEditor: ICastEditor
  {

    #region "ADO.NETが返すデータ型 → プロパティ型"
    #region "'ADO.NETが返すデータ型毎のCast前編集"
    public virtual object BeforeCast_ViewColumnType(StringViewColumnType viewColumnType
                                                  , ViewColumnInfo aViewColumnInfo
                                                  , object viewColumnValue) {
      return viewColumnValue;
    }
    public virtual object BeforeCast_ViewColumnType(IntegerViewColumnType viewColumnType
                                                  , ViewColumnInfo aViewColumnInfo
                                                  , object viewColumnValue) {
      return viewColumnValue;
    }
    public virtual object BeforeCast_ViewColumnType(LongViewColumnType viewColumnType
                                                  , ViewColumnInfo aViewColumnInfo
                                                  , object viewColumnValue) {
      return viewColumnValue;
    }
    public virtual object BeforeCast_ViewColumnType(DecimalViewColumnType viewColumnType
                                                  , ViewColumnInfo aViewColumnInfo
                                                  , object viewColumnValue) {
      return viewColumnValue;
    }
    public virtual object BeforeCast_ViewColumnType(DoubleViewColumnType viewColumnType
                                                  , ViewColumnInfo aViewColumnInfo
                                                  , object viewColumnValue) {
      return viewColumnValue;
    }
    public virtual object BeforeCast_ViewColumnType(DateTimeViewColumnType viewColumnType
                                                  , ViewColumnInfo aViewColumnInfo
                                                  , object viewColumnValue) {
      return viewColumnValue;
    }
    public virtual object BeforeCast_ViewColumnType(TimeSpanViewColumnType viewColumnType
                                                  , ViewColumnInfo aViewColumnInfo
                                                  , object viewColumnValue) {
      return viewColumnValue;
    }
    public virtual object BeforeCast_ViewColumnType(BooleanViewColumnType viewColumnType
                                                  , ViewColumnInfo aViewColumnInfo
                                                  , object viewColumnValue) {
      return viewColumnValue;
    }
    #endregion

    #region "'ADO.NETが返すデータ型毎のCast後編集"
    public virtual object AfterCast_ViewColumnType(StringViewColumnType viewColumnType
                                                  , ViewColumnInfo aViewColumnInfo
                                                  , object propertyValue) {
      return propertyValue;
    }
    public virtual object AfterCast_ViewColumnType(IntegerViewColumnType viewColumnType
                                                  , ViewColumnInfo aViewColumnInfo
                                                  , object propertyValue) {
      return propertyValue;
    }
    public virtual object AfterCast_ViewColumnType(LongViewColumnType viewColumnType
                                                  , ViewColumnInfo aViewColumnInfo
                                                  , object propertyValue) {
      return propertyValue;
    }
    public virtual object AfterCast_ViewColumnType(DecimalViewColumnType viewColumnType
                                                  , ViewColumnInfo aViewColumnInfo
                                                  , object propertyValue) {
      return propertyValue;
    }
    public virtual object AfterCast_ViewColumnType(DoubleViewColumnType viewColumnType
                                                  , ViewColumnInfo aViewColumnInfo
                                                  , object propertyValue) {
      return propertyValue;
    }
    public virtual object AfterCast_ViewColumnType(DateTimeViewColumnType viewColumnType
                                                  , ViewColumnInfo aViewColumnInfo
                                                  , object propertyValue) {
      return propertyValue;
    }
    public virtual object AfterCast_ViewColumnType(TimeSpanViewColumnType viewColumnType
                                                  , ViewColumnInfo aViewColumnInfo
                                                  , object propertyValue) {
      return propertyValue;
    }
    public virtual object AfterCast_ViewColumnType(BooleanViewColumnType viewColumnType
                                                  , ViewColumnInfo aViewColumnInfo
                                                  , object propertyValue) {
      return propertyValue;
    }
    #endregion

    //プロパティ型毎のCast前編集
    public virtual object BeforeCast_PropertyType(PropertyType propertyType
                                                , ViewColumnInfo aViewColumnInfo
                                                , object viewColumnValue) {
      return viewColumnValue;
    }

    //プロパティ型毎のCast後編集
    public virtual object AfterCast_PropertyType(PropertyType propertyType
                                                , ViewColumnInfo aViewColumnInfo
                                                , object propertyValue) {
      return propertyValue;
    }

    #region "'ADO.NETが返すデータ型とプロパティ型毎のCast前編集"
    public virtual object BeforeCast(StringViewColumnType viewColumnType
                                    , PropertyType propertyType
                                    , ViewColumnInfo aViewColumnInfo
                                    , object viewColumnValue) {
      return viewColumnValue;
    }
    public virtual object BeforeCast(IntegerViewColumnType viewColumnType
                                    , PropertyType propertyType
                                    , ViewColumnInfo aViewColumnInfo
                                    , object viewColumnValue) {
      return viewColumnValue;
    }
    public virtual object BeforeCast(LongViewColumnType viewColumnType
                                    , PropertyType propertyType
                                    , ViewColumnInfo aViewColumnInfo
                                    , object viewColumnValue) {
      return viewColumnValue;
    }
    public virtual object BeforeCast(DecimalViewColumnType viewColumnType
                                    , PropertyType propertyType
                                    , ViewColumnInfo aViewColumnInfo
                                    , object viewColumnValue) {
      return viewColumnValue;
    }
    public virtual object BeforeCast(DoubleViewColumnType viewColumnType
                                    , PropertyType propertyType
                                    , ViewColumnInfo aViewColumnInfo
                                    , object viewColumnValue) {
      return viewColumnValue;
    }
    public virtual object BeforeCast(DateTimeViewColumnType viewColumnType
                                    , PropertyType propertyType
                                    , ViewColumnInfo aViewColumnInfo
                                    , object viewColumnValue) {
      return viewColumnValue;
    }
    public virtual object BeforeCast(TimeSpanViewColumnType viewColumnType
                                    , PropertyType propertyType
                                    , ViewColumnInfo aViewColumnInfo
                                    , object viewColumnValue) {
      return viewColumnValue;
    }
    public virtual object BeforeCast(BooleanViewColumnType viewColumnType
                                    , PropertyType propertyType
                                    , ViewColumnInfo aViewColumnInfo
                                    , object viewColumnValue) {
      return viewColumnValue;
    }
    #endregion

    #region "'ADO.NETが返すデータ型とプロパティ型毎のCast後編集"
    public virtual object AfterCast(StringViewColumnType viewColumnType
                                  , PropertyType propertyType
                                  , ViewColumnInfo aViewColumnInfo
                                  , object propertyValue) {
      return propertyValue;
    }
    public virtual object AfterCast(IntegerViewColumnType viewColumnType
                                  , PropertyType propertyType
                                  , ViewColumnInfo aViewColumnInfo
                                  , object propertyValue) {
      return propertyValue;
    }
    public virtual object AfterCast(LongViewColumnType viewColumnType
                                  , PropertyType propertyType
                                  , ViewColumnInfo aViewColumnInfo
                                  , object propertyValue) {
      return propertyValue;
    }
    public virtual object AfterCast(DecimalViewColumnType viewColumnType
                                  , PropertyType propertyType
                                  , ViewColumnInfo aViewColumnInfo
                                  , object propertyValue) {
      return propertyValue;
    }
    public virtual object AfterCast(DoubleViewColumnType viewColumnType
                                  , PropertyType propertyType
                                  , ViewColumnInfo aViewColumnInfo
                                  , object propertyValue) {
      return propertyValue;
    }
    public virtual object AfterCast(DateTimeViewColumnType viewColumnType
                                  , PropertyType propertyType
                                  , ViewColumnInfo aViewColumnInfo
                                  , object propertyValue) {
      return propertyValue;
    }
    public virtual object AfterCast(TimeSpanViewColumnType viewColumnType
                                  , PropertyType propertyType
                                  , ViewColumnInfo aViewColumnInfo
                                  , object propertyValue) {
      return propertyValue;
    }
    public virtual object AfterCast(BooleanViewColumnType viewColumnType
                                  , PropertyType propertyType
                                  , ViewColumnInfo aViewColumnInfo
                                  , object propertyValue) {
      return propertyValue;
    }
    #endregion

    #endregion

    #region "プロパティ型 → SQLリテラル型"
    #region "'SQLリテラル型毎のCast前編集"
    public virtual object BeforeCast_SqlLiteralType(StringSqlLiteralType sqlLiteralType
                                                  , ColumnInfo aColumnInfo
                                                  , object propertyValue) {
      return propertyValue;
    }
    public virtual object BeforeCast_SqlLiteralType(NumberSqlLiteralType sqlLiteralType
                                                  , ColumnInfo aColumnInfo
                                                  , object propertyValue) {
      return propertyValue;
    }
    public virtual object BeforeCast_SqlLiteralType(DateTimeSqlLiteralType sqlLiteralType
                                                  , ColumnInfo aColumnInfo
                                                  , object propertyValue) {
      return propertyValue;
    }
    public virtual object BeforeCast_SqlLiteralType(OracleDateTimeSqlLiteralType sqlLiteralType
                                                  , ColumnInfo aColumnInfo
                                                  , object propertyValue) {
      return propertyValue;
    }
    public virtual object BeforeCast_SqlLiteralType(DoubleSqlLiteralType sqlLiteralType
                                                  , ColumnInfo aColumnInfo
                                                  , object propertyValue) {
      return propertyValue;
    }
    public virtual object BeforeCast_SqlLiteralType(IntervalSqlLiteralType sqlLiteralType
                                                  , ColumnInfo aColumnInfo
                                                  , object propertyValue) {
      return propertyValue;
    }
    #endregion

    #region "'SQLリテラル型毎のCast後編集"
    public virtual string AfterCast_SqlLiteralType(StringSqlLiteralType sqlLiteralType
                                                  , ColumnInfo aColumnInfo
                                                  , string sqlLiteralValue) {
      return sqlLiteralValue;
    }
    public virtual string AfterCast_SqlLiteralType(NumberSqlLiteralType sqlLiteralType
                                                  , ColumnInfo aColumnInfo
                                                  , string sqlLiteralValue) {
      return sqlLiteralValue;
    }
    public virtual string AfterCast_SqlLiteralType(DateTimeSqlLiteralType sqlLiteralType
                                                  , ColumnInfo aColumnInfo
                                                  , string sqlLiteralValue) {
      return sqlLiteralValue;
    }
    public virtual string AfterCast_SqlLiteralType(OracleDateTimeSqlLiteralType sqlLiteralType
                                                  , ColumnInfo aColumnInfo
                                                  , string sqlLiteralValue) {
      return sqlLiteralValue;
    }
    public virtual string AfterCast_SqlLiteralType(DoubleSqlLiteralType sqlLiteralType
                                                  , ColumnInfo aColumnInfo
                                                  , string sqlLiteralValue) {
      return sqlLiteralValue;
    }
    public virtual string AfterCast_SqlLiteralType(IntervalSqlLiteralType sqlLiteralType
                                                  , ColumnInfo aColumnInfo
                                                  , string sqlLiteralValue) {
      return sqlLiteralValue;
    }
    #endregion

    //プロパティ型毎のCast前編集
    public virtual object BeforeCast_PropertyType(PropertyType propertyType
                                                , ColumnInfo aColumnInfo
                                                , object propertyValue) {
      return propertyValue;
    }

    //プロパティ型毎のCast後編集
    public virtual string AfterCast_PropertyType(PropertyType propertyType
                                                , ColumnInfo aColumnInfo
                                                , string sqlLiteralValue) {
      return sqlLiteralValue;
    }

    #region "'SQLリテラル型とプロパティ型毎のCast前編集"
    public virtual object BeforeCast(StringSqlLiteralType sqlLiteralType
                                    , PropertyType propertyType
                                    , ColumnInfo aColumnInfo
                                    , object propertyValue) {
      return propertyValue;
    }
    public virtual object BeforeCast(NumberSqlLiteralType sqlLiteralType
                                    , PropertyType propertyType
                                    , ColumnInfo aColumnInfo
                                    , object propertyValue) {
      return propertyValue;
    }
    public virtual object BeforeCast(DateTimeSqlLiteralType sqlLiteralType
                                    , PropertyType propertyType
                                    , ColumnInfo aColumnInfo
                                    , object propertyValue) {
      return propertyValue;
    }
    public virtual object BeforeCast(OracleDateTimeSqlLiteralType sqlLiteralType
                                    , PropertyType propertyType
                                    , ColumnInfo aColumnInfo
                                    , object propertyValue) {
      return propertyValue;
    }
    public virtual object BeforeCast(DoubleSqlLiteralType sqlLiteralType
                                    , PropertyType propertyType
                                    , ColumnInfo aColumnInfo
                                    , object propertyValue) {
      return propertyValue;
    }
    public virtual object BeforeCast(IntervalSqlLiteralType sqlLiteralType
                                    , PropertyType propertyType
                                    , ColumnInfo aColumnInfo
                                    , object propertyValue) {
      return propertyValue;
    }
    #endregion

    #region "'SQLリテラル型とプロパティ型毎のCast後編集"
    public virtual string AfterCast(StringSqlLiteralType sqlLiteralType
                                  , PropertyType propertyType
                                  , ColumnInfo aColumnInfo
                                  , string sqlLiteralValue) {
      return sqlLiteralValue;
    }
    public virtual string AfterCast(NumberSqlLiteralType sqlLiteralType
                                  , PropertyType propertyType
                                  , ColumnInfo aColumnInfo
                                  , string sqlLiteralValue) {
      return sqlLiteralValue;
    }
    public virtual string AfterCast(DateTimeSqlLiteralType sqlLiteralType
                                  , PropertyType propertyType
                                  , ColumnInfo aColumnInfo
                                  , string sqlLiteralValue) {
      return sqlLiteralValue;
    }
    public virtual string AfterCast(OracleDateTimeSqlLiteralType sqlLiteralType
                                  , PropertyType propertyType
                                  , ColumnInfo aColumnInfo
                                  , string sqlLiteralValue) {
      return sqlLiteralValue;
    }
    public virtual string AfterCast(DoubleSqlLiteralType sqlLiteralType
                                  , PropertyType propertyType
                                  , ColumnInfo aColumnInfo
                                  , string sqlLiteralValue) {
      return sqlLiteralValue;
    }
    public virtual string AfterCast(IntervalSqlLiteralType sqlLiteralType
                                  , PropertyType propertyType
                                  , ColumnInfo aColumnInfo
                                  , string sqlLiteralValue) {
      return sqlLiteralValue;
    }

    #endregion

    #endregion

  }
}
