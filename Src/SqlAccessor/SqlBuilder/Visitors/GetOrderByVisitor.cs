using System;
using System.Collections.Generic;
using System.ComponentModel;
using MiniSqlParser;

namespace MiniSqlParser
{
  /// <summary>
  /// ORDER BY句を取得する
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class GetOrderByVisitor: Visitor
  {
    //KeyValueTuple<string, bool> = <OrderBy項目文字列, ASC>
    private readonly List<Tuple<string, bool>> _orderingTerms;

    public GetOrderByVisitor() {
      _orderingTerms = new List<Tuple<string, bool>>();
    }

    public List<Tuple<string, bool>> OrderingTerms {
      get {
        return _orderingTerms;
      }
    }

    public override void VisitAfter(SingleQueryClause query) {
      if(!query.IsSubQuery && query.GetType() == typeof(SingleQuery)) {
        this.GetOrderBy(((SingleQuery)query).OrderBy);
      }
    }

    public override void VisitAfter(BracketedQueryClause query) {
      if(!query.IsSubQuery && query.GetType() == typeof(BracketedQuery)) {
        this.GetOrderBy(((BracketedQuery)query).OrderBy);
      }
    }

    public override void VisitAfter(CompoundQueryClause query) {
      if(!query.IsSubQuery && query.GetType() == typeof(CompoundQuery)) {
        this.GetOrderBy(((CompoundQuery)query).OrderBy);
      }
    }

    private void GetOrderBy(OrderBy orderBy) {
      foreach(var orderingTerm in orderBy) {
        // Order By句の各項目は、文字列にしてから返す
        var stringifier = new CompactStringifier(128);
        orderingTerm.Term.Accept(stringifier);
        var orderingTermStr = stringifier.ToString();
        var asc = orderingTerm.OrderSpec == OrderSpec.Asc;
        _orderingTerms.Add(new Tuple<string, bool>(orderingTermStr, asc));
      }
    }
  }
}
