using System;
using System.Collections.Generic;
using System.ComponentModel;
using MiniSqlParser;

namespace MiniSqlParser
{
  /// <summary>
  /// ORDER BY句にソート項目を追加する
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class AddOrderByVisitor: Visitor
  {
    private readonly OrderBy _orderBy;
    private readonly DBMSType _dmbsType;

    public AddOrderByVisitor(IEnumerable<Tuple<string, bool>> orderByItems
                           , DBMSType dbmsType = DBMSType.Unknown
                           , bool forSqlAccessor = false) {
      // KeyValuePair(Of String, Boolean) = (OrderBy項目文字列, ASC)
      _orderBy = new OrderBy();
      foreach(var orderByItem in orderByItems){
        var expr = MiniSqlParserAST.CreateExpr(orderByItem.Item1, dbmsType, forSqlAccessor);
        _orderBy.Add(new OrderingTerm(expr,null
                                    , orderByItem.Item2 ? OrderSpec.Desc : OrderSpec.None
                                    , NullOrder.None));
      }
      _dmbsType = dbmsType;
    }

    public override void VisitAfter(SingleQueryClause query) {
      if(!query.IsSubQuery && query.GetType() == typeof(SingleQuery)) {
        ((SingleQuery)query).OrderBy.AddRange(_orderBy);
      }
    }

    public override void VisitAfter(BracketedQueryClause query) {
      if(!query.IsSubQuery && query.GetType() == typeof(BracketedQuery)) {
        ((BracketedQuery)query).OrderBy.AddRange(_orderBy);
      }
    }

    public override void VisitAfter(CompoundQueryClause query) {
      if(!query.IsSubQuery && query.GetType() == typeof(CompoundQuery)) {
        ((CompoundQuery)query).OrderBy.AddRange(_orderBy);
      }
    }
  }
}
