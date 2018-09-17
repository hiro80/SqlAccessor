using System.Collections.Generic;
using System.ComponentModel;
using MiniSqlParser;

namespace MiniSqlParser
{
  /// <summary>
  /// プレースホルダに値を適用する
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class SetPlaceHoldersWrapper<TVisitor>: IVisitor
    where TVisitor : IVisitor
  {
    private readonly TVisitor _visitor;
    private readonly Dictionary<string, Node> _placeHolders;

    public SetPlaceHoldersWrapper(TVisitor visitor, Dictionary<string, string> placeHolders) {
      _visitor = visitor;
      _placeHolders = new Dictionary<string, Node>();

      foreach(var placeHolder in placeHolders) {
        var value = MiniSqlParserAST.CreatePlaceHolderNode(placeHolder.Value);
        _placeHolders.Add(placeHolder.Key, value);
      }
    }

    public SetPlaceHoldersWrapper(TVisitor visitor, Dictionary<string, Node> placeHolders) {
      _visitor = visitor;
      _placeHolders = placeHolders;
    }

    public bool VisitOnFromFirstInQuery {
      get {
        return _visitor.VisitOnFromFirstInQuery;
      } 
    }

    public TVisitor Visitor {
      get {
        return _visitor;
      }
    }

    public override string ToString() {
      return _visitor.ToString();
    }

    // Others
    public void VisitOnSeparator(Node node, int offset, int i) {
      _visitor.VisitOnSeparator(node, offset, i);
    }
    public void VisitOnSeparator(Exprs exprs, int offset, int i) {
      _visitor.VisitOnSeparator(exprs, offset, i);
    }
    public void VisitOnSeparator(CommaJoinSource commaJoinSource, int offset, int i) {
      _visitor.VisitOnSeparator(commaJoinSource, offset, i);
    }
    public void VisitOnSeparator(SubstringFunc expr, int offset, int i) {
      _visitor.VisitOnSeparator(expr, offset, i);
    }
    public void VisitOnSeparator(ExtractFuncExpr expr, int offset) {
      _visitor.VisitOnSeparator(expr, offset);
    }
    public void VisitOnLParen(Node node, int offset) {
      _visitor.VisitOnLParen(node, offset);
    }
    public void VisitOnRParen(Node node, int offset) {
      _visitor.VisitOnRParen(node, offset);
    }
    public void VisitOnWildCard(Node node, int offset)  {
      _visitor.VisitOnWildCard(node, offset);
    }
    public void VisitOnStmtSeparator(Stmt stmt, int offset, int i) {
      _visitor.VisitOnStmtSeparator(stmt, offset, i);
    }

    // Identifiers
    public void Visit(Table table) {
      _visitor.Visit(table);
    }
    public void Visit(Column column) {
      _visitor.Visit(column);
    }
    public void Visit(TableWildcard tableWildcard) {
      _visitor.Visit(tableWildcard);
    }
    public void Visit(UnqualifiedColumnName columnName) {
      _visitor.Visit(columnName);
    }

    // Literals
    public void Visit(StringLiteral literal) {
      _visitor.Visit(literal);
    }
    public void Visit(UNumericLiteral literal) {
      _visitor.Visit(literal);
    }
    public void Visit(NullLiteral literal) {
      _visitor.Visit(literal);
    }
    public void Visit(DateLiteral literal) {
      _visitor.Visit(literal);
    }
    public void Visit(TimeLiteral literal) {
      _visitor.Visit(literal);
    }
    public void Visit(TimeStampLiteral literal) {
      _visitor.Visit(literal);
    }
    public void Visit(IntervalLiteral literal) {
      _visitor.Visit(literal);
    }
    public void Visit(BlobLiteral literal) {
      _visitor.Visit(literal);
    }

    // Expressions
    public void VisitBefore(SignedNumberExpr expr) {
      _visitor.VisitBefore(expr);
    }
    public void VisitAfter(SignedNumberExpr expr) {
      _visitor.VisitAfter(expr);
    }
    public void Visit(PlaceHolderExpr expr) {
      // プレースホルダに当てはまるExpr式に_visitorを適用する
      if(_placeHolders.ContainsKey(expr.LabelName)) {
        _placeHolders[expr.LabelName].Accept(_visitor);
      } else {
        _visitor.Visit(expr);
      }
    }
    public void VisitBefore(BitwiseNotExpr expr) {
      _visitor.VisitBefore(expr);
    }
    public void VisitAfter(BitwiseNotExpr expr) {
      _visitor.VisitAfter(expr);
    }
    public void VisitBefore(BinaryOpExpr expr) {
      _visitor.VisitBefore(expr);
    }
    public void VisitAfter(BinaryOpExpr expr) {
      _visitor.VisitAfter(expr);
    }
    public void VisitOnOperator(BinaryOpExpr expr) {
      _visitor.VisitOnOperator(expr);
    }
    public void VisitBefore(SubstringFunc expr) {
      _visitor.VisitBefore(expr);
    }
    public void VisitAfter(SubstringFunc expr) {
      _visitor.VisitAfter(expr);
    }
    public void VisitBefore(ExtractFuncExpr expr) {
      _visitor.VisitBefore(expr);
    }
    public void VisitAfter(ExtractFuncExpr expr) {
      _visitor.VisitAfter(expr);
    }
    public void VisitBefore(AggregateFuncExpr expr) {
      _visitor.VisitBefore(expr);
    }
    public void VisitAfter(AggregateFuncExpr expr) {
      _visitor.VisitAfter(expr);
    }
    public void VisitBefore(WindowFuncExpr expr) {
      _visitor.VisitBefore(expr);
    }
    public void VisitAfter(WindowFuncExpr expr) {
      _visitor.VisitAfter(expr);
    }
    public void VisitOnOver(WindowFuncExpr expr, int offset) {
      _visitor.VisitOnOver(expr, offset);
    }
    public void VisitBefore(FuncExpr expr) {
      _visitor.VisitBefore(expr);
    }
    public void VisitAfter(FuncExpr expr) {
      _visitor.VisitAfter(expr);
    }
    public void VisitBefore(BracketedExpr expr) {
      _visitor.VisitBefore(expr);
    }
    public void VisitAfter(BracketedExpr expr) {
      _visitor.VisitAfter(expr);
    }
    public void VisitBefore(CastExpr expr) {
      _visitor.VisitBefore(expr);
    }
    public void VisitAfter(CastExpr expr) {
      _visitor.VisitAfter(expr);
    }
    public void VisitBefore(CaseExpr expr) {
      _visitor.VisitBefore(expr);
    }
    public void VisitAfter(CaseExpr expr) {
      _visitor.VisitAfter(expr);
    }
    public void VisitOnWhen(CaseExpr expr, int i) {
      _visitor.VisitOnWhen(expr, i);
    }
    public void VisitOnThen(CaseExpr expr, int i) {
      _visitor.VisitOnThen(expr, i);
    }
    public void VisitOnElse(CaseExpr expr) {
      _visitor.VisitOnElse(expr);
    }
    public void VisitBefore(SubQueryExp expr) {
      _visitor.VisitBefore(expr);
    }
    public void VisitAfter(SubQueryExp expr) {
      _visitor.VisitAfter(expr);
    }

    // Predicates
    public void VisitBefore(BinaryOpPredicate predicate) {
      _visitor.VisitBefore(predicate);
    }
    public void VisitAfter(BinaryOpPredicate predicate) {
      _visitor.VisitAfter(predicate);
    }
    public void Visit(BinaryOpPredicate predicate) {
      _visitor.Visit(predicate);
    }
    public void VisitBefore(NotPredicate notPredicate) {
      _visitor.VisitBefore(notPredicate);
    }
    public void VisitAfter(NotPredicate notPredicate) {
      _visitor.VisitAfter(notPredicate);
    }
    public void VisitBefore(AndPredicate andPredicate) {
      _visitor.VisitBefore(andPredicate);
    }
    public void VisitAfter(AndPredicate andPredicate) {
      _visitor.VisitAfter(andPredicate);
    }
    public void Visit(AndPredicate andPredicate) {
      _visitor.Visit(andPredicate);
    }
    public void VisitBefore(OrPredicate orPredicate) {
      _visitor.VisitBefore(orPredicate);
    }
    public void VisitAfter(OrPredicate orPredicate) {
      _visitor.VisitAfter(orPredicate);
    }
    public void Visit(OrPredicate orPredicate) {
      _visitor.Visit(orPredicate);
    }
    public void Visit(PlaceHolderPredicate predicate) {
      // プレースホルダに当てはまるPredicate式に_visitorを適用する
      if(_placeHolders.ContainsKey(predicate.LabelName)) {
        _placeHolders[predicate.LabelName].Accept(_visitor);
      } else {
        _visitor.Visit(predicate);
      }
    }
    public void VisitBefore(LikePredicate predicate) {
      _visitor.VisitBefore(predicate);
    }
    public void VisitAfter(LikePredicate predicate) {
      _visitor.VisitAfter(predicate);
    }
    public void Visit(LikePredicate predicate, int offset) {
      _visitor.Visit(predicate, offset);
    }
    public void VisitOnEscape(LikePredicate predicate, int offset) {
      _visitor.VisitOnEscape(predicate, offset);
    }
    public void VisitBefore(IsNullPredicate predicate) {
      _visitor.VisitBefore(predicate);
    }
    public void VisitAfter(IsNullPredicate predicate) {
      _visitor.VisitAfter(predicate);
    }
    public void VisitBefore(IsPredicate predicate) {
      _visitor.VisitBefore(predicate);
    }
    public void VisitAfter(IsPredicate predicate) {
      _visitor.VisitAfter(predicate);
    }
    public void Visit(IsPredicate predicate, int offset) {
      _visitor.Visit(predicate, offset);
    }
    public void VisitBefore(BetweenPredicate predicate) {
      _visitor.VisitBefore(predicate);
    }
    public void VisitAfter(BetweenPredicate predicate) {
      _visitor.VisitAfter(predicate);
    }
    public void VisitOnBetween(BetweenPredicate predicate, int offset) {
      _visitor.VisitOnBetween(predicate, offset);
    }
    public void VisitOnAnd(BetweenPredicate predicate, int offset) {
      _visitor.VisitOnAnd(predicate, offset);
    }
    public void VisitBefore(InPredicate predicate) {
      _visitor.VisitBefore(predicate);
    }
    public void VisitAfter(InPredicate predicate) {
      _visitor.VisitAfter(predicate);
    }
    public void Visit(InPredicate predicate, int offset) {
      _visitor.Visit(predicate, offset);
    }
    public void VisitBefore(SubQueryPredicate predicate) {
      _visitor.VisitBefore(predicate);
    }
    public void VisitAfter(SubQueryPredicate predicate) {
      _visitor.VisitAfter(predicate);
    }
    public void Visit(SubQueryPredicate predicate, int offset) {
      _visitor.Visit(predicate, offset);
    }
    public void VisitBefore(ExistsPredicate predicate) {
      _visitor.VisitBefore(predicate);
    }
    public void VisitAfter(ExistsPredicate predicate) {
      _visitor.VisitAfter(predicate);
    }
    public void VisitBefore(CollatePredicate predicate) {
      _visitor.VisitBefore(predicate);
    }
    public void VisitAfter(CollatePredicate predicate) {
      _visitor.VisitAfter(predicate);
    }
    public void VisitBefore(BracketedPredicate predicate) {
      _visitor.VisitBefore(predicate);
    }
    public void VisitAfter(BracketedPredicate predicate) {
      _visitor.VisitAfter(predicate);
    }

    // Clauses
    public void VisitBefore(Assignment assignment) {
      _visitor.VisitBefore(assignment);
    }
    public void VisitAfter(Assignment assignment) {
      _visitor.VisitAfter(assignment);
    }
    public void VisitBefore(Assignments assignments) {
      _visitor.VisitBefore(assignments);
    }
    public void VisitAfter(Assignments assignments) {
      _visitor.VisitAfter(assignments);
    }
    public void Visit(Assignment assignment) {
      _visitor.Visit(assignment);
    }
    public void Visit(Default defoult) {
      _visitor.Visit(defoult);
    }
    public void VisitBefore(WithClause withClause) {
      _visitor.VisitBefore(withClause);
    }
    public void VisitAfter(WithClause withClause) {
      _visitor.VisitAfter(withClause);
    }
    public void VisitBefore(WithDefinition withDefinition) {
      _visitor.VisitBefore(withDefinition);
    }
    public void VisitAfter(WithDefinition withDefinition) {
      _visitor.VisitAfter(withDefinition);
    }
    public void VisitOnAs(WithDefinition withDefinition, int offset) {
      _visitor.VisitOnAs(withDefinition, offset);
    }
    public void VisitBefore(CompoundQueryClause compoundQuery) {
      _visitor.VisitBefore(compoundQuery);
    }
    public void VisitAfter(CompoundQueryClause compoundQuery) {
      _visitor.VisitAfter(compoundQuery);
    }
    public void VisitOnCompoundOp(CompoundQueryClause compoundQuery, int offset) {
      _visitor.VisitOnCompoundOp(compoundQuery, offset);
    }
    public void VisitBefore(BracketedQueryClause bracketedQuery) {
      _visitor.VisitBefore(bracketedQuery);
    }
    public void VisitAfter(BracketedQueryClause bracketedQuery) {
      _visitor.VisitAfter(bracketedQuery);
    }
    public void VisitBefore(SingleQueryClause query) {
      _visitor.VisitBefore(query);
    }
    public void VisitAfter(SingleQueryClause query) {
      _visitor.VisitAfter(query);
    }
    public void VisitOnFrom(SingleQueryClause query, int offset) {
      _visitor.VisitOnFrom(query, offset);
    }
    public void VisitOnWhere(SingleQueryClause query, int offset) {
      _visitor.VisitOnWhere(query, offset);
    }
    public void VisitOnHaving(SingleQueryClause query, int offset) {
      _visitor.VisitOnHaving(query, offset);
    }
    public void VisitBefore(ResultColumns resultColumns) {
      _visitor.VisitBefore(resultColumns);
    }
    public void VisitAfter(ResultColumns resultColumns) {
      _visitor.VisitAfter(resultColumns);
    }
    public void VisitBefore(ResultExpr resultExpr) {
      _visitor.VisitBefore(resultExpr);
    }
    public void VisitAfter(ResultExpr resultExpr) {
      _visitor.VisitAfter(resultExpr);
    }
    public void VisitBefore(ColumnNames columns) {
      _visitor.VisitBefore(columns);
    }
    public void VisitAfter(ColumnNames columns) {
      _visitor.VisitAfter(columns);
    }
    public void VisitBefore(UnqualifiedColumnNames columns) {
      _visitor.VisitBefore(columns);
    }
    public void VisitAfter(UnqualifiedColumnNames columns) {
      _visitor.VisitAfter(columns);
    }
    public void VisitBefore(ValuesList valuesList) {
      _visitor.VisitBefore(valuesList);
    }
    public void VisitAfter(ValuesList valuesList) {
      _visitor.VisitAfter(valuesList);
    }
    public void VisitBefore(Values values) {
      _visitor.VisitBefore(values);
    }
    public void VisitAfter(Values values) {
      _visitor.VisitAfter(values);
    }
    public void VisitBefore(Exprs exprs) {
      _visitor.VisitBefore(exprs);
    }
    public void VisitAfter(Exprs exprs) {
      _visitor.VisitAfter(exprs);
    }
    public void VisitBefore(JoinSource joinSource) {
      _visitor.VisitBefore(joinSource);
    }
    public void VisitAfter(JoinSource joinSource) {
      _visitor.VisitAfter(joinSource);
    }
    public void Visit(JoinSource joinSource) {
      _visitor.Visit(joinSource);
    }
    public void Visit(JoinOperator joinOperator) {
      _visitor.Visit(joinOperator);
    }
    public void VisitBefore(CommaJoinSource commaJoinSource) {
      _visitor.VisitBefore(commaJoinSource);
    }
    public void VisitAfter(CommaJoinSource commaJoinSource) {
      _visitor.VisitAfter(commaJoinSource);
    }
    public void VisitBefore(AliasedQuery aliasedQuery) {
      _visitor.VisitBefore(aliasedQuery);
    }
    public void VisitAfter(AliasedQuery aliasedQuery) {
      _visitor.VisitAfter(aliasedQuery);
    }
    public void VisitBefore(BracketedSource bracketedSource) {
      _visitor.VisitBefore(bracketedSource);
    }
    public void VisitAfter(BracketedSource bracketedSource) {
      _visitor.VisitAfter(bracketedSource);
    }
    public void VisitBefore(GroupBy groupBy) {
      _visitor.VisitBefore(groupBy);
    }
    public void VisitAfter(GroupBy groupBy) {
      _visitor.VisitAfter(groupBy);
    }
    public void VisitBefore(OrderBy orderBy) {
      _visitor.VisitBefore(orderBy);
    }
    public void VisitAfter(OrderBy orderBy) {
      _visitor.VisitAfter(orderBy);
    }
    public void VisitBefore(OrderingTerm orderingTerm) {
      _visitor.VisitBefore(orderingTerm);
    }
    public void VisitAfter(OrderingTerm orderingTerm) {
      _visitor.VisitAfter(orderingTerm);
    }
    public void VisitBefore(PartitionBy partitionBy) {
      _visitor.VisitBefore(partitionBy);
    }
    public void VisitAfter(PartitionBy partitionBy) {
      _visitor.VisitAfter(partitionBy);
    }
    public void VisitBefore(PartitioningTerm partitioningTerm) {
      _visitor.VisitBefore(partitioningTerm);
    }
    public void VisitAfter(PartitioningTerm partitioningTerm) {
      _visitor.VisitAfter(partitioningTerm);
    }
    public void VisitBefore(ILimitClause limitClause) {
      _visitor.VisitBefore(limitClause);
    }
    public void VisitAfter(ILimitClause limitClause) {
      _visitor.VisitAfter(limitClause);
    }
    public void VisitOnOffset(ILimitClause limitClause, int offset) {
      _visitor.VisitOnOffset(limitClause, offset);
    }
    public void VisitBefore(ForUpdateClause forUpdateClause) {
      _visitor.VisitBefore(forUpdateClause);
    }
    public void VisitAfter(ForUpdateClause forUpdateClause) {
      _visitor.VisitAfter(forUpdateClause);
    }
    public void VisitBefore(ForUpdateOfClause forUpdateOfClause) {
      _visitor.VisitBefore(forUpdateOfClause);
    }
    public void VisitAfter(ForUpdateOfClause forUpdateOfClause) {
      _visitor.VisitAfter(forUpdateOfClause);
    }

    // Statements
    public void VisitBefore(SelectStmt selectStmt) {
      _visitor.VisitBefore(selectStmt);
    }
    public void VisitAfter(SelectStmt selectStmt) {
      _visitor.VisitAfter(selectStmt);
    }
    public void VisitBefore(UpdateStmt updateStmt) {
      _visitor.VisitBefore(updateStmt);
    }
    public void VisitAfter(UpdateStmt updateStmt) {
      _visitor.VisitAfter(updateStmt);
    }
    public void VisitOnUpdate(UpdateStmt updateStmt) {
      _visitor.VisitOnUpdate(updateStmt);
    }
    public void VisitOnSet(UpdateStmt updateStmt, int offset) {
      _visitor.VisitOnSet(updateStmt, offset);
    }
    public void VisitOnFrom2(UpdateStmt updateStmt, int offset) {
      _visitor.VisitOnFrom2(updateStmt, offset);
    }
    public void VisitOnWhere(UpdateStmt updateStmt, int offset) {
      _visitor.VisitOnWhere(updateStmt, offset);
    }

    public void VisitBefore(InsertStmt insertStmt) {
      _visitor.VisitBefore(insertStmt);
    }
    public void VisitAfter(InsertStmt insertStmt) {
      _visitor.VisitAfter(insertStmt);
    }
    public void VisitOnInsert(InsertStmt insertStmt) {
      _visitor.VisitOnInsert(insertStmt);
    }
    public void VisitOnValues(InsertValuesStmt insertValuesStmt, int offset) {
      _visitor.VisitOnValues(insertValuesStmt, offset);
    }
    public void VisitBeforeQuery(InsertSelectStmt insertSelectStmt, int offset) {
      _visitor.VisitBeforeQuery(insertSelectStmt, offset);
    }

    public void VisitOnDefaultValues(InsertStmt insertStmt, int offset) {
      _visitor.VisitOnDefaultValues(insertStmt, offset);
    }
    public void VisitBefore(IfStmt ifStmt) {
      _visitor.VisitBefore(ifStmt);
    }
    public void VisitAfter(IfStmt ifStmt) {
      _visitor.VisitAfter(ifStmt);
    }
    public void VisitOnThen(IfStmt ifStmt, int ifThenIndex, int offset) {
      _visitor.VisitOnThen(ifStmt, ifThenIndex, offset);
    }
    public void VisitOnElsIf(IfStmt ifStmt, int ifThenIndex, int offset) {
      _visitor.VisitOnElsIf(ifStmt, ifThenIndex, offset);
    }
    public void VisitOnElse(IfStmt ifStmt, int offset) {
      _visitor.VisitOnElse(ifStmt, offset);
    }
    public void VisitOnEndIf(IfStmt ifStmt, int offset) { 
      _visitor.VisitOnEndIf(ifStmt, offset);
    }

    public void VisitBefore(DeleteStmt deleteStmt) {
      _visitor.VisitBefore(deleteStmt);
    }
    public void VisitAfter(DeleteStmt deleteStmt) {
      _visitor.VisitAfter(deleteStmt);
    }
    public void VisitOnDelete(DeleteStmt deleteStmt) {
      _visitor.VisitOnDelete(deleteStmt);
    }
    public void VisitOnFrom2(DeleteStmt deleteStmt, int offset) {
      _visitor.VisitOnFrom2(deleteStmt, offset);
    }
    public void VisitOnWhere(DeleteStmt deleteStmt, int offset) {
      _visitor.VisitOnWhere(deleteStmt, offset);
    }

    public void VisitBefore(MergeStmt mergeStmt) {
      _visitor.VisitBefore(mergeStmt);
    }
    public void VisitAfter(MergeStmt mergeStmt) {
      _visitor.VisitAfter(mergeStmt);
    }
    public void VisitOnMerge(MergeStmt mergeStmt) {
      _visitor.VisitOnMerge(mergeStmt);
    }
    public void VisitOnUsing(MergeStmt mergeStmt, int offset) {
      _visitor.VisitOnUsing(mergeStmt, offset);
    }
    public void VisitOnOn(MergeStmt mergeStmt, int offset) {
      _visitor.VisitOnOn(mergeStmt, offset);
    }
    public void VisitBefore(MergeUpdateClause updateClause) {
      _visitor.VisitBefore(updateClause);
    }
    public void VisitAfter(MergeUpdateClause updateClause) {
      _visitor.VisitAfter(updateClause);
    }
    public void VisitBefore(MergeInsertClause insertClause) {
      _visitor.VisitBefore(insertClause);
    }
    public void VisitAfter(MergeInsertClause insertClause) {
      _visitor.VisitAfter(insertClause);
    }
    public void VisitOnValues(MergeInsertClause insertClause, int offset) {
      _visitor.VisitOnValues(insertClause, offset);
    }

    public void VisitBefore(CallStmt callStmt) {
      _visitor.VisitBefore(callStmt);
    }
    public void VisitAfter(CallStmt callStmt) {
      _visitor.VisitAfter(callStmt);
    }

    public void VisitBefore(TruncateStmt truncateStmt) {
      _visitor.VisitBefore(truncateStmt);
    }
    public void VisitAfter(TruncateStmt truncateStmt) {
      _visitor.VisitAfter(truncateStmt);
    }

    public void VisitBefore(SqlitePragmaStmt pragmaStmt) {
      _visitor.VisitBefore(pragmaStmt);
    }
    public void VisitAfter(SqlitePragmaStmt pragmaStmt) {
      _visitor.VisitAfter(pragmaStmt);
    }

    public void VisitBefore(NullStmt nullStmt) {
      _visitor.VisitBefore(nullStmt);
    }
    public void VisitAfter(NullStmt nullStmt) {
      _visitor.VisitAfter(nullStmt);
    }

  }
}
