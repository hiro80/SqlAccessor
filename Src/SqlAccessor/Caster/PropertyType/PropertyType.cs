using System.Reflection;
using System.ComponentModel;

namespace SqlAccessor
{
  [EditorBrowsable(EditorBrowsableState.Never)]
  public abstract class PropertyType
  {
    private readonly ICastEditor _castEditor;

    private readonly System.Type _castEditorType;

    protected PropertyType(ICastEditor castEditor) {
      _castEditor = castEditor;
      object e = castEditor;
      _castEditorType = e.GetType();
    }

    #region "CastEditorの呼出"
    private object BeforeCast_PropertyType(ViewColumnInfo aViewColumnInfo
                                          , object viewColumnValue) {
      MethodInfo methodInfo = _castEditorType.GetMethod("BeforeCast_PropertyType"
                                                        , new System.Type[] {this.GetType()
				                                                           , aViewColumnInfo.GetType()
				                                                           , typeof(object)});
      return methodInfo.Invoke(_castEditor, 
                               new object[] {this,
				                             aViewColumnInfo,
				                             viewColumnValue});
    }

    private object AfterCast_PropertyType(ViewColumnInfo aViewColumnInfo
                                          , object propertyValue) {
      MethodInfo methodInfo = _castEditorType.GetMethod("AfterCast_PropertyType",
                                                        new System.Type[] {this.GetType(),
				                                                           aViewColumnInfo.GetType(),
				                                                           typeof(object)});
      return methodInfo.Invoke(_castEditor,
                               new object[] {this,
				                             aViewColumnInfo,
				                             propertyValue});
    }

    private object BeforeCast(ViewColumnType viewColumnType
                            , ViewColumnInfo aViewColumnInfo
                            , object viewColumnValue) {
      try {
        MethodInfo methodInfo = _castEditorType.GetMethod("BeforeCast",
                                                          new System.Type[] {viewColumnType.GetType(),
					                                                         this.GetType(),
					                                                         aViewColumnInfo.GetType(),
					                                                         typeof(object)});
        return methodInfo.Invoke(_castEditor,
                                 new object[] {viewColumnType,
					                           this,
					                           aViewColumnInfo,
					                           viewColumnValue});
      } catch(System.Exception ex) {
        MethodInfo[] m = _castEditorType.GetMethods();
        throw;
      }
    }

    private object AfterCast(ViewColumnType viewColumnType
                            , ViewColumnInfo aViewColumnInfo
                            , object propertyValue) {
      MethodInfo methodInfo = _castEditorType.GetMethod("AfterCast"
                                                       , new System.Type[] {viewColumnType.GetType(),
				                                                            this.GetType(),
				                                                            aViewColumnInfo.GetType(),
				                                                            typeof(object)});
      return methodInfo.Invoke(_castEditor
                             , new object[] {viewColumnType,
				                             this,
				                             aViewColumnInfo,
				                             propertyValue});
    }
    #endregion

    #region "CastEditorの呼出"
    private object BeforeCast_PropertyType(ColumnInfo aColumnInfo
                                          , object propertyValue) {
      MethodInfo methodInfo = _castEditorType.GetMethod("BeforeCast_PropertyType"
                                                      , new System.Type[] {this.GetType(),
				                                                           typeof(ColumnInfo),
				                                                           typeof(object)});
      return methodInfo.Invoke(_castEditor,
                               new object[] {this,
				                             aColumnInfo,
				                             propertyValue});
    }

    private string AfterCast_PropertyType(ColumnInfo aColumnInfo
                                        , string sqlLiteralValue) {
      MethodInfo methodInfo = _castEditorType.GetMethod("AfterCast_PropertyType"
                                                      , new System.Type[] {this.GetType(),
				                                                           typeof(ColumnInfo),
				                                                           typeof(string)});
      return methodInfo.Invoke(_castEditor,
                               new object[] {this,
				                             aColumnInfo,
				                             sqlLiteralValue}).ToString();
    }

    private object BeforeCast(SqlLiteralType sqlLiteralType
                            , ColumnInfo aColumnInfo
                            , object propertyValue) {
      MethodInfo methodInfo = _castEditorType.GetMethod("BeforeCast"
                                                      , new System.Type[] {sqlLiteralType.GetType(),
				                                                           this.GetType(),
				                                                           typeof(ColumnInfo),
				                                                           typeof(object)});
      return methodInfo.Invoke(_castEditor,
                               new object[] {sqlLiteralType,
				                             this,
				                             aColumnInfo,
				                             propertyValue});
    }

    private string AfterCast(SqlLiteralType sqlLiteralType
                            , ColumnInfo aColumnInfo
                            , string sqlLiteralValue) {
      MethodInfo methodInfo = _castEditorType.GetMethod("AfterCast"
                                                      , new System.Type[] {sqlLiteralType.GetType(),
				                                                           this.GetType(),
				                                                           typeof(ColumnInfo),
				                                                           typeof(string)});
      return methodInfo.Invoke(_castEditor,
                               new object[] {sqlLiteralType,
				                             this,
				                             aColumnInfo,
				                             sqlLiteralValue}).ToString();
    }
    #endregion

    internal object CastFrom(StringViewColumnType viewColumnType
                           , ViewColumnInfo aViewColumnInfo
                           , object viewColumnValue) {
      //ADO.NETが返した値を、Cast前編集する
      object value = this.BeforeCast_PropertyType(aViewColumnInfo, viewColumnValue);
      //ADO.NETが返した値を、Cast前編集する
      value = this.BeforeCast(viewColumnType, aViewColumnInfo, value);
      //ADO.NETが返したデータ型から、プロパティ型にキャストする
      value = this.CastFromImp(viewColumnType, value);
      //プロパティ値を、Cast後編集する
      value = this.AfterCast(viewColumnType, aViewColumnInfo, value);
      //プロパティ値を、Cast後編集する
      return this.AfterCast_PropertyType(aViewColumnInfo, value);
    }

    internal object CastFrom(IntegerViewColumnType viewColumnType
                            , ViewColumnInfo aViewColumnInfo
                            , object viewColumnValue) {
      object value = this.BeforeCast_PropertyType(aViewColumnInfo, viewColumnValue);
      value = this.BeforeCast(viewColumnType, aViewColumnInfo, value);
      value = this.CastFromImp(viewColumnType, value);
      value = this.AfterCast(viewColumnType, aViewColumnInfo, value);
      return this.AfterCast_PropertyType(aViewColumnInfo, value);
    }

    internal object CastFrom(LongViewColumnType viewColumnType
                            , ViewColumnInfo aViewColumnInfo
                            , object viewColumnValue) {
      object value = this.BeforeCast_PropertyType(aViewColumnInfo, viewColumnValue);
      value = this.BeforeCast(viewColumnType, aViewColumnInfo, value);
      value = this.CastFromImp(viewColumnType, value);
      value = this.AfterCast(viewColumnType, aViewColumnInfo, value);
      return this.AfterCast_PropertyType(aViewColumnInfo, value);
    }

    internal object CastFrom(DecimalViewColumnType viewColumnType
                            , ViewColumnInfo aViewColumnInfo
                            , object viewColumnValue) {
      object value = this.BeforeCast_PropertyType(aViewColumnInfo, viewColumnValue);
      value = this.BeforeCast(viewColumnType, aViewColumnInfo, value);
      value = this.CastFromImp(viewColumnType, value);
      value = this.AfterCast(viewColumnType, aViewColumnInfo, value);
      return this.AfterCast_PropertyType(aViewColumnInfo, value);
    }

    internal object CastFrom(DoubleViewColumnType viewColumnType
                            , ViewColumnInfo aViewColumnInfo
                            , object viewColumnValue) {
      object value = this.BeforeCast_PropertyType(aViewColumnInfo, viewColumnValue);
      value = this.BeforeCast(viewColumnType, aViewColumnInfo, value);
      value = this.CastFromImp(viewColumnType, value);
      value = this.AfterCast(viewColumnType, aViewColumnInfo, value);
      return this.AfterCast_PropertyType(aViewColumnInfo, value);
    }

    internal object CastFrom(DateTimeViewColumnType viewColumnType
                            , ViewColumnInfo aViewColumnInfo
                            , object viewColumnValue) {
      object value = this.BeforeCast_PropertyType(aViewColumnInfo, viewColumnValue);
      value = this.BeforeCast(viewColumnType, aViewColumnInfo, value);
      value = this.CastFromImp(viewColumnType, value);
      value = this.AfterCast(viewColumnType, aViewColumnInfo, value);
      return this.AfterCast_PropertyType(aViewColumnInfo, value);
    }

    internal object CastFrom(TimeSpanViewColumnType viewColumnType
                            , ViewColumnInfo aViewColumnInfo
                            , object viewColumnValue) {
      object value = this.BeforeCast_PropertyType(aViewColumnInfo, viewColumnValue);
      value = this.BeforeCast(viewColumnType, aViewColumnInfo, value);
      value = this.CastFromImp(viewColumnType, value);
      value = this.AfterCast(viewColumnType, aViewColumnInfo, value);
      return this.AfterCast_PropertyType(aViewColumnInfo, value);
    }

    internal object CastFrom(BooleanViewColumnType viewColumnType
                            , ViewColumnInfo aViewColumnInfo
                            , object viewColumnValue) {
      object value = this.BeforeCast_PropertyType(aViewColumnInfo, viewColumnValue);
      value = this.BeforeCast(viewColumnType, aViewColumnInfo, value);
      value = this.CastFromImp(viewColumnType, value);
      value = this.AfterCast(viewColumnType, aViewColumnInfo, value);
      return this.AfterCast_PropertyType(aViewColumnInfo, value);
    }

    internal string CastTo(StringSqlLiteralType sqlLiteralType
                          , ColumnInfo aColumnInfo
                          , object propertyValue) {
      //プロパティ値を、Cast前編集する
      object value = this.BeforeCast_PropertyType(aColumnInfo, propertyValue);
      //プロパティ値を、Cast前編集する
      value = this.BeforeCast(sqlLiteralType, aColumnInfo, value);
      //プロパティ型から、SQLリテラル型にキャストする
      string castedValue = this.CastToImp(sqlLiteralType, value);
      //プロパティ値を、Cast後編集する
      castedValue = this.AfterCast(sqlLiteralType, aColumnInfo, castedValue);
      //プロパティ値を、Cast後編集する
      return this.AfterCast_PropertyType(aColumnInfo, castedValue);
    }

    internal string CastTo(NumberSqlLiteralType sqlLiteralType
                          , ColumnInfo aColumnInfo
                          , object propertyValue) {
      object value = this.BeforeCast_PropertyType(aColumnInfo, propertyValue);
      value = this.BeforeCast(sqlLiteralType, aColumnInfo, value);
      string castedValue = this.CastToImp(sqlLiteralType, value);
      castedValue = this.AfterCast(sqlLiteralType, aColumnInfo, castedValue);
      return this.AfterCast_PropertyType(aColumnInfo, castedValue);
    }

    internal string CastTo(DateTimeSqlLiteralType sqlLiteralType
                          , ColumnInfo aColumnInfo
                          , object propertyValue) {
      object value = this.BeforeCast_PropertyType(aColumnInfo, propertyValue);
      value = this.BeforeCast(sqlLiteralType, aColumnInfo, value);
      string castedValue = this.CastToImp(sqlLiteralType, value);
      castedValue = this.AfterCast(sqlLiteralType, aColumnInfo, castedValue);
      return this.AfterCast_PropertyType(aColumnInfo, castedValue);
    }

    internal string CastTo(OracleDateTimeSqlLiteralType sqlLiteralType
                          , ColumnInfo aColumnInfo
                          , object propertyValue) {
      object value = this.BeforeCast_PropertyType(aColumnInfo, propertyValue);
      value = this.BeforeCast(sqlLiteralType, aColumnInfo, value);
      string castedValue = this.CastToImp(sqlLiteralType, value);
      castedValue = this.AfterCast(sqlLiteralType, aColumnInfo, castedValue);
      return this.AfterCast_PropertyType(aColumnInfo, castedValue);
    }

    internal string CastTo(DoubleSqlLiteralType sqlLiteralType
                          , ColumnInfo aColumnInfo
                          , object propertyValue) {
      object value = this.BeforeCast_PropertyType(aColumnInfo, propertyValue);
      value = this.BeforeCast(sqlLiteralType, aColumnInfo, value);
      string castedValue = this.CastToImp(sqlLiteralType, value);
      castedValue = this.AfterCast(sqlLiteralType, aColumnInfo, castedValue);
      return this.AfterCast_PropertyType(aColumnInfo, castedValue);
    }

    internal string CastTo(IntervalSqlLiteralType sqlLiteralType
                          , ColumnInfo aColumnInfo
                          , object propertyValue) {
      object value = this.BeforeCast_PropertyType(aColumnInfo, propertyValue);
      value = this.BeforeCast(sqlLiteralType, aColumnInfo, value);
      string castedValue = this.CastToImp(sqlLiteralType, value);
      castedValue = this.AfterCast(sqlLiteralType, aColumnInfo, castedValue);
      return this.AfterCast_PropertyType(aColumnInfo, castedValue);
    }

    protected internal abstract System.Type GetPropertyType();
    protected internal abstract bool IsNullValue(object propertyValue);
    protected internal abstract object GetNullValue();

    protected abstract object CastFromImp(StringViewColumnType viewColumnType
                                        , object viewColumnValue);
    protected abstract object CastFromImp(IntegerViewColumnType viewColumnType
                                        , object viewColumnValue);
    protected abstract object CastFromImp(LongViewColumnType viewColumnType
                                        , object viewColumnValue);
    protected abstract object CastFromImp(DecimalViewColumnType viewColumnType
                                        , object viewColumnValue);
    protected abstract object CastFromImp(DoubleViewColumnType viewColumnType
                                        , object viewColumnValue);
    protected abstract object CastFromImp(DateTimeViewColumnType viewColumnType
                                        , object viewColumnValue);
    protected abstract object CastFromImp(TimeSpanViewColumnType viewColumnType
                                        , object viewColumnValue);
    protected abstract object CastFromImp(BooleanViewColumnType viewColumnType
                                        , object viewColumnValue);

    protected abstract string CastToImp(StringSqlLiteralType sqlLiteralType
                                      , object propertyValue);
    protected abstract string CastToImp(NumberSqlLiteralType sqlLiteralType
                                      , object propertyValue);
    protected abstract string CastToImp(DateTimeSqlLiteralType sqlLiteralType
                                      , object propertyValue);
    protected abstract string CastToImp(OracleDateTimeSqlLiteralType sqlLiteralType
                                      , object propertyValue);
    protected abstract string CastToImp(DoubleSqlLiteralType sqlLiteralType
                                      , object propertyValue);
    protected abstract string CastToImp(IntervalSqlLiteralType sqlLiteralType
                                      , object propertyValue);

  }
}
