using System.Collections.Generic;
using System.ComponentModel;
using MiniSqlParser;

namespace MiniSqlParser
{
  /// <summary>
  /// SQL文の全ての抽出元テーブル又は更新テーブルを返す
  /// テーブル名が重複する場合はひとつに纏めて返す
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class GetSourceTablesVisitor: SetPlaceHoldersVisitor
  {
    private HashSet<Table> _tables;
    // UPDATE文やDELETE文の対象テーブル
    private Table _targetTable;
    private bool _updateTableStmt;
    private int _predicateLevel;
    private int _clauseLevel;

    public GetSourceTablesVisitor(Dictionary<string, Node> placeHolders = null)
    : base(placeHolders)
    {
      _tables = new HashSet<Table>();
    }

    public HashSet<Table> Tables {
      get {
        return _tables;
      }
    }

    public Table TargetTable {
      get {
        if(_updateTableStmt) {
          return _targetTable;
        } else {
          return null;
        }
      }
    }

    public override void Visit(Table table) {
      // GroupBy等のClause内及びPredicate内にあるクエリは
      // メインクエリの抽出結果の値にならないので除外する
      if(_predicateLevel == 0 && _clauseLevel == 0) {
        _tables.Add(table);
        if(_targetTable == null) {
          _targetTable = table;
        }
      }
    }

    public override void VisitBefore(UpdateStmt updateStmt) {
      _updateTableStmt = true;
    }

    public override void VisitBefore(InsertStmt insertStmt) {
      _updateTableStmt = true;
    }

    public override void VisitBefore(DeleteStmt deleteStmt) {
      _updateTableStmt = true;
    }

    public override void VisitBefore(TruncateStmt truncateStmt) {
      _updateTableStmt = true;
    }

    public override void VisitBefore(MergeStmt mergeStmt) {
      _updateTableStmt = true;
    }


    public override void VisitBefore(GroupBy groupBy) {
      ++_clauseLevel;
    }
    public override void VisitAfter(GroupBy groupBy) {
      --_clauseLevel;
    }
    public override void VisitBefore(OrderBy orderBy) {
      ++_clauseLevel;
    }
    public override void VisitAfter(OrderBy orderBy) {
      --_clauseLevel;
    }
    public override void VisitBefore(PartitionBy partitionBy) {
      ++_clauseLevel;
    }
    public override void VisitAfter(PartitionBy partitionBy) {
      --_clauseLevel;
    }
    public override void VisitBefore(ILimitClause limitClause) {
      ++_clauseLevel;
    }
    public override void VisitAfter(ILimitClause limitClause) {
      --_clauseLevel;
    }


    public override void VisitBefore(BinaryOpPredicate predicate) {
      ++_predicateLevel;
    }
    public override void VisitAfter(BinaryOpPredicate predicate) {
      --_predicateLevel;
    }
    public override void VisitBefore(NotPredicate notPredicate) {
      ++_predicateLevel;
    }
    public override void VisitAfter(NotPredicate notPredicate) {
      --_predicateLevel;
    }
    public override void VisitBefore(AndPredicate andPredicate) {
      ++_predicateLevel;
    }
    public override void VisitAfter(AndPredicate andPredicate) { 
      --_predicateLevel;
    }
    public override void VisitBefore(OrPredicate orPredicate) {
      ++_predicateLevel;
    }
    public override void VisitAfter(OrPredicate orPredicate) {
      --_predicateLevel;
    }
    public override void VisitBefore(LikePredicate predicate) {
      ++_predicateLevel;
    }
    public override void VisitAfter(LikePredicate predicate) {
      --_predicateLevel;
    }
    public override void VisitBefore(IsNullPredicate predicate) {
      ++_predicateLevel;
    }
    public override void VisitAfter(IsNullPredicate predicate) {
      --_predicateLevel;
    }
    public override void VisitBefore(IsPredicate predicate) {
      ++_predicateLevel;
    }
    public override void VisitAfter(IsPredicate predicate) {
      --_predicateLevel;
    }
    public override void VisitBefore(BetweenPredicate predicate) {
      ++_predicateLevel;
    }
    public override void VisitAfter(BetweenPredicate predicate) {
      --_predicateLevel;
    }
    public override void VisitBefore(InPredicate predicate) {
      ++_predicateLevel;
    }
    public override void VisitAfter(InPredicate predicate) {
      --_predicateLevel;
    }
    public override void VisitBefore(SubQueryPredicate predicate) {
      ++_predicateLevel;
    }
    public override void VisitAfter(SubQueryPredicate predicate) {
      --_predicateLevel;
    }
    public override void VisitBefore(ExistsPredicate predicate) {
      ++_predicateLevel;
    }
    public override void VisitAfter(ExistsPredicate predicate) {
      --_predicateLevel;
    }
    public override void VisitBefore(CollatePredicate predicate) {
      ++_predicateLevel;
    }
    public override void VisitAfter(CollatePredicate predicate) {
      --_predicateLevel;
    }
    public override void VisitBefore(BracketedPredicate predicate) {
      ++_predicateLevel;
    }
    public override void VisitAfter(BracketedPredicate predicate) {
      --_predicateLevel;
    }
  }
}
