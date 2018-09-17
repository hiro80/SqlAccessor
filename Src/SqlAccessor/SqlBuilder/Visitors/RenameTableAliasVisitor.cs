using System.Collections.Generic;
using System.ComponentModel;
using MiniSqlParser;

namespace MiniSqlParser
{
  /// <summary>
  /// メインクエリの抽出元となるテーブルの別名を変更する
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class RenameTableAliasVisitor: SetPlaceHoldersVisitor
  {
    // サーバ名、スキーマ名等の修飾子を考慮してテーブル別名の一致を判定する必要がある


    private enum FromOrResult{
      FromQuery,
      ResultQuery
    }

    private class Scope
    {
      public Scope(FromOrResult fromOrResult, bool tableAliasNameReplaced) {
        this.FromOrResult = fromOrResult;
        this.TableAliasNameReplaced = tableAliasNameReplaced;
      }
      public FromOrResult FromOrResult { get; set; }
      public bool TableAliasNameReplaced { get; set; }
    }

    // SELECT文内におけるテーブル名のスコープを表すStack
    private Stack<Scope> _scopeStack;

    // Tableオブジェクトのテーブル別名を変更済みの場合True
    // (テーブル別名の変更は1つに限定する)
    private bool _tableAliasNameIsReplaced;

    // Visitorの操作ノードでのEXISTS、IN、ANY、SOME、ALLのPredicateの入子階層
    private int _existNestLevel;
    private int _inNestLevel;
    private int _anySomeAllNestLevel;
    // Visitorの操作ノードでのSubQueryExprの入子階層
    private int _queryExprNestLevel;

    private readonly Identifier _oldTableAliasName;
    private Identifier _newTableAliasName;

    public RenameTableAliasVisitor(Identifier oldTableAliasName
      , Identifier newTableAliasName
      , Dictionary<string, string> placeHolders = null) : base(placeHolders)
    {
      _oldTableAliasName = oldTableAliasName;
      _newTableAliasName = newTableAliasName;
      _scopeStack = new Stack<Scope>();
    }

    public Identifier NewTableAliasName {
      get {
        return _newTableAliasName;
      }
    }

    public override bool VisitOnFromFirstInQuery {
      get {
        // Queryを走査する時はFROM句を最初に走査し、その後にWHERE句、SELECT句の順に走査する
        return true;
      }
    }

    public override void VisitOnFrom(SingleQueryClause query, int offset) {
      if(this.IsNotInMainResultsSource()) {
        return;
      }
      if(this.FindTableAliasName(query.From, _oldTableAliasName)) {
        // 変更後のテーブル別名が既にあるテーブル別名と重複する場合は、末尾に"_"を付加する
        while(this.FindTableAliasName(query.From, _newTableAliasName)) {
          _newTableAliasName += "_";
        }
        _scopeStack.Push(new Scope(FromOrResult.FromQuery, true));
      } else {
        _scopeStack.Push(new Scope(FromOrResult.FromQuery, false));
      }
    }

    public override void VisitOnUpdate(UpdateStmt updateStmt) {
      if(this.FindTableAliasName(updateStmt.Table, _oldTableAliasName) ||
         this.FindTableAliasName(updateStmt.Table2, _oldTableAliasName)) {
        // 変更後のテーブル別名が既にあるテーブル別名と重複する場合は、末尾に"_"を付加する
        while(this.FindTableAliasName(updateStmt.Table, _newTableAliasName) ||
              this.FindTableAliasName(updateStmt.Table2, _oldTableAliasName)) {
          _newTableAliasName += "_";
        }
        _scopeStack.Push(new Scope(FromOrResult.FromQuery, true));
      } else {
        _scopeStack.Push(new Scope(FromOrResult.FromQuery, false));
      }
    }

    public override void VisitOnInsert(InsertStmt insertStmt) {
      // INSERT文のテーブルは別名の指定ができない
      _scopeStack.Push(new Scope(FromOrResult.FromQuery, false));
    }

    public override void VisitOnDelete(DeleteStmt deleteStmt) {
      if(this.FindTableAliasName(deleteStmt.Table,  _oldTableAliasName) ||
         this.FindTableAliasName(deleteStmt.Table2, _oldTableAliasName)) {
        // 変更後のテーブル別名が既にあるテーブル別名と重複する場合は、末尾に"_"を付加する
        while(this.FindTableAliasName(deleteStmt.Table,  _newTableAliasName) ||
              this.FindTableAliasName(deleteStmt.Table2, _oldTableAliasName)) {
          _newTableAliasName += "_";
        }
        _scopeStack.Push(new Scope(FromOrResult.FromQuery, true));
      } else {
        _scopeStack.Push(new Scope(FromOrResult.FromQuery, false));
      }
    }

    public override void VisitOnMerge(MergeStmt mergeStmt) {
      if(this.FindTableAliasName(mergeStmt.Table, _oldTableAliasName) ||
         this.FindTableAliasName(mergeStmt.UsingTable, _oldTableAliasName)) {
        // 変更後のテーブル別名が既にあるテーブル別名と重複する場合は、末尾に"_"を付加する
        while(this.FindTableAliasName(mergeStmt.Table, _newTableAliasName) ||
              this.FindTableAliasName(mergeStmt.UsingTable, _oldTableAliasName)) {
          _newTableAliasName += "_";
        }
        _scopeStack.Push(new Scope(FromOrResult.FromQuery, true));
      } else {
        _scopeStack.Push(new Scope(FromOrResult.FromQuery, false));
      }
    }

    public override void VisitBefore(TruncateStmt truncateStmt) {
      // TRUNCATE文のテーブルは別名の指定ができない
        _scopeStack.Push(new Scope(FromOrResult.FromQuery, false));
    }

    public override void Visit(Table table) {
      var scope = _scopeStack.Peek();
      if(!_tableAliasNameIsReplaced &&
         scope.TableAliasNameReplaced &&
         table.GetAliasOrTableName() == _oldTableAliasName) {
        table.AliasName = _newTableAliasName;
        _tableAliasNameIsReplaced = true;
      }
    }

    public override void Visit(Column column) {
      if(this.IsInReplacedTableScope() &&
         column.TableAliasName == _oldTableAliasName) {
        column.TableAliasName = _newTableAliasName;
      }
    }

    public override void Visit(TableWildcard tableWildcard) {
      if(this.IsInReplacedTableScope() &&
         tableWildcard.TableAliasName == _oldTableAliasName) {
           tableWildcard.TableAliasName = _newTableAliasName;
      }
    }

    public override void VisitAfter(SingleQueryClause query) {
      if(this.IsNotInMainResultsSource()) {
        return;
      }
      if(query.HasFrom) {
        _scopeStack.Pop();
      }
    }


    public override void VisitBefore(ExistsPredicate predicate) {
      ++_existNestLevel;
    }
    public override void VisitAfter(ExistsPredicate predicate) {
      --_existNestLevel;
    }
    public override void VisitBefore(InPredicate predicate) {
      if(predicate.HasSubQuery) {
        ++_inNestLevel;
      }
    }
    public override void VisitAfter(InPredicate predicate) {
      if(predicate.HasSubQuery) {
        --_inNestLevel;
      }
    }
    public override void VisitBefore(SubQueryPredicate predicate) {
      ++_anySomeAllNestLevel;
    }
    public override void VisitAfter(SubQueryPredicate predicate) {
      --_anySomeAllNestLevel;
    }
    public override void VisitBefore(SubQueryExp expr) {
      if(expr.IsUsedInResultColumn()) {
        // SELECT句サブクエリの場合
        if(this.IsNotInMainResultsSource()) {
          return;
        }
        _scopeStack.Push(new Scope(FromOrResult.ResultQuery, false));

      } else {
        ++_queryExprNestLevel;
      }
    }
    public override void VisitAfter(SubQueryExp expr) {
      if(expr.IsUsedInResultColumn()) {
        // SELECT句サブクエリの場合
        if(this.IsNotInMainResultsSource()) {
          return;
        }
        _scopeStack.Pop();

      } else {
        --_queryExprNestLevel;
      }
    }

    // メインクエリの抽出元にならないサブクエリの場合はTrueを返す
    private bool IsNotInMainResultsSource() {
      return _existNestLevel      > 0 || 
             _inNestLevel         > 0 ||
             _anySomeAllNestLevel > 0 ||
             _queryExprNestLevel  > 0;
    }

    // Visitorの操作ノードでのテーブル名/別名の参照スコープが、
    // 変更対象のテーブルであればTrueを返す
    private bool IsInReplacedTableScope() {
      // FROM句で宣言したテーブル名/別名は、そのクエリ内及びSELECT句サブクエリの全範囲内で
      // 参照可能である。_scopeStackにおいて、テーブル名/別名の変更対象のテーブルの直上が
      // SELECT句サブクエリのスタックの場合は、そのスタック以上は変更対象テーブルの参照可能
      // 範囲である。
      Scope upperScope = null;
      foreach(var scope in _scopeStack) {
        if(scope.TableAliasNameReplaced && 
           (upperScope == null || upperScope.FromOrResult == FromOrResult.ResultQuery)) {
          return true;
        }
        upperScope = scope;
      }
      return false;
    }

    // 引数で指定したFROM句に変更対象のテーブル名/別名があればTrueを返す
    private bool FindTableAliasName(IFromSource fromSource, Identifier tableAliasName) {
      if(fromSource == null) {
        return false;
      }else if(fromSource.Type == FromSourceType.Table) {
        var table = ((Table)fromSource);
        return table.GetAliasOrTableName() == tableAliasName;
      } else if(fromSource.Type == FromSourceType.AliasedQuery) {
        return false;
      } else if(fromSource.Type == FromSourceType.Join) {
        var join = ((JoinSource)fromSource);
        return this.FindTableAliasName(join.Left, tableAliasName) ||
               this.FindTableAliasName(join.Right, tableAliasName);
      } else if(fromSource.Type == FromSourceType.Bracketed) {
        return this.FindTableAliasName(((BracketedSource)fromSource).Operand, tableAliasName);
      } else if(fromSource.Type == FromSourceType.CommaJoin) {
        var commaJoin = ((CommaJoinSource)fromSource);
        return this.FindTableAliasName(commaJoin.Left, tableAliasName) ||
               this.FindTableAliasName(commaJoin.Right, tableAliasName);
      } else {
        throw new InvalidEnumArgumentException("Undefined FromSourceType is used");
      }
    }

  }
}
