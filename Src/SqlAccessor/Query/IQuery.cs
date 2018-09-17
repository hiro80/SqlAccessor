using System;
using System.Collections.Generic;

namespace SqlAccessor
{
  /// <summary>
  /// レコードの抽出条件を表す
  /// </summary>
  /// <remarks></remarks>
  public interface IQuery: System.ICloneable
  {
    void And(IExp aExp);
    void And(bool aBoolean);
    void OrderBy(params string[] propertyNames);
    void OrderBy(IEnumerable<string> propertyNames);
    void OrderByDesc(params string[] propertyNames);
    void OrderByDesc(IEnumerable<string> propertyNames);
    void MaxRows(int max);
    void RowRange(int beginRowIndex, int endRowIndex);
    string GetWhereExp();
    List<Tuple<string, bool>> GetOrderByExp();
    int GetMaxRows();
    Tuple<int, int> GetRowRange();
    Query<toRecord> CastTo<toRecord>() where toRecord: class, IRecord, new();
  }
}
namespace SqlAccessor
{

  /// <summary>
  /// Queryオブジェクトにおいて、SqlAccessor外部に公開したくないメソッドを定義する
  /// </summary>
  /// <typeparam name="TRecord"></typeparam>
  /// <remarks></remarks>
  internal interface IQueryExtention<TRecord> where TRecord: class, IRecord, new()
  {
    //Function GetRecord() As TRecord
    void AndedFromQuery(Query<TRecord> aQuery);
    void CastToSqlLiteralType(ICaster caster, IViewInfoGetter viewInfoGetter);
  }
}
