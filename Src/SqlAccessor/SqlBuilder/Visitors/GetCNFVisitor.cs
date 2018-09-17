using System;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using MiniSqlParser;
//using SqlAccessor;

// サーバ名、スキーマ名等の修飾子を考慮してテーブル別名の一致を判定する必要がある

// Merge Stmt

namespace MiniSqlParser
{
  /// <summary>
  /// 抽出条件の乗法標準形(Conjunctive Normal Form)を取得する
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class GetCNFVisitor: GetResultInfoListVisitor
  {
    private class CNFSet
    {
      // 一致条件による制約がないテーブル
      private readonly HashSet<Table> _noEqualityTables;
      // EXISTS句内で用いるテーブル
      private readonly HashSet<Table> _tablesUsedInExists;
      // <テーブル名, 列名, リテラル値>
      //private readonly TwoKeysDictionary<Table, string, Literal> _halfEqualitiesToLiteral;
      //private readonly Dictionary<TableAndColumn, Literal> _halfEqualitiesToLiteral;
      private readonly Dictionary<Table, Dictionary<string, Literal>> _halfEqualitiesToLiteral;
      // <テーブル名, 列名> => <テーブル名, 列名>
      private readonly HashSet<Tuple<TableAndColumn, TableAndColumn>> _halfEqualities;

      public CNFSet() {
        _noEqualityTables = new HashSet<Table>();
        _tablesUsedInExists = new HashSet<Table>();
        //_halfEqualitiesToLiteral = new TwoKeysDictionary<Table, string, Literal>();
        _halfEqualitiesToLiteral = new Dictionary<Table, Dictionary<string, Literal>>();
        _halfEqualities = new HashSet<Tuple<TableAndColumn, TableAndColumn>>();
      }

      public Dictionary<Table, Dictionary<string, Literal>> GetEqualities(){
        //var ret = _halfEqualitiesToLiteral.Items();
        var ret = _halfEqualitiesToLiteral;
        // 制約がないテーブルはCNFは無い
        foreach(var noEqualityTable in _noEqualityTables) {
          ret.Add(noEqualityTable, new Dictionary<string, Literal>());
        }
        // EXISTS句内で用いるテーブルのCNFは取得しない
        foreach(var tableUsedInExists in _tablesUsedInExists) {
          ret.Remove(tableUsedInExists);
        }
        return ret;
      }

      // テーブル全件を対象とする
      public void Add(Table table) {
        _noEqualityTables.Add(table);
      }

      // EXISTS内で用いるテーブルの登録
      internal void AddExistsTable(Table table) {
        _tablesUsedInExists.Add(table);
      }

      // これらサブクエリ内で親クエリの抽出元テーブルへの一致条件がある場合
      // ・SELECT句サブクエリの場合
      //   サブクエリへの抽出条件になるが、親クエリへの抽出条件にはならない
      // ・EXISTS句サブクエリの場合
      //   サブクエリへの抽出条件にならず、親クエリへの抽出条件になる
      // の二通りの解釈が必要になる。そのため以下の仕様とする。

      //・結合条件のための一致条件では双方向の演算とする
      //  select * from T join U
      //  where T.x = U.x
      //  ----------------------
      //  T.x <- U.x
      //  U.x <- T.x
      //
      //・SELECT句サブクエリの抽出元テーブル方向への一致条件のみとする
      //  select
      //    (select x from U
      //     where U.x = T.x)
      //  from T
      //  -------------------
      //  U.x <- T.x
      //
      //・EXIST句の親クエリの抽出元テーブル方向への一致条件のみとする
      //  select * from T
      //  where exists (select * from U
      //                where U.x = T.x)
      //  ------------------------------
      //  T.x <- U.x
      //
      //・サブクエリの抽出元テーブルが被演算子になっていない一致条件は無視する
      //  (サブクエリに親テーブルのみの一致条件が記述されることはあまりない上に
      //   実装はやや手間取るためこの仕様とする)
      //  select * from T
      //  where exists (select * from U
      //                where exists (select * from V
      //                              where T.x = 1    <= 無視する
      //                                and U.x = T.x) <= 無視する
      //               )

      // tableColumn <- literal
      public void Add(TableAndColumn tableColumn, Literal literal) {

        //if(_halfEqualitiesToLiteral.ContainsKey(tableColumn.Table, tableColumn.ColumnName)) {
        //  if(_halfEqualitiesToLiteral[tableColumn.Table, tableColumn.ColumnName] == literal) {

        var tableKeyExists = _halfEqualitiesToLiteral.ContainsKey(tableColumn.Table);
        if(tableKeyExists && 
           _halfEqualitiesToLiteral[tableColumn.Table].ContainsKey(tableColumn.ColumnName)) {
          if(_halfEqualitiesToLiteral[tableColumn.Table][tableColumn.ColumnName] == literal) { 
            // 同じ一致条件は重複して登録しない
          } else {
            // 同じDB列に異なる値への一致条件が存在する場合、対象テーブルからの抽出件数は0件である
            // 従って対象テーブルのエントリ自体を削除する
            _halfEqualitiesToLiteral.Remove(tableColumn.Table);
          }
        } else {
          // 半等式の推移律を適用して、(列B <- 列A , 列A <- 1) ⇒ (列B <- 1 , 列A <- 1) に変換する

          // (列A <- 1)を登録する
          _noEqualityTables.Remove(tableColumn.Table);
          //_halfEqualitiesToLiteral.Add(tableColumn.Table, tableColumn.ColumnName, literal);
          if(tableKeyExists) {
            _halfEqualitiesToLiteral[tableColumn.Table].Add(tableColumn.ColumnName, literal);
          } else {
            _halfEqualitiesToLiteral.Add(
                tableColumn.Table
              , new Dictionary<string, Literal>() { { tableColumn.ColumnName, literal } });
          }

          foreach(var halfEquality in _halfEqualities) {
            if(halfEquality.Item2 == tableColumn) {
              // (列B <- 1)を登録する
              this.Add(halfEquality.Item1, literal);
            }
          } // foreach
        } // if
      }

      // lTableColumn <- rTableColumn
      public void Add(TableAndColumn lTableColumn, TableAndColumn rTableColumn) {
        // (列B <- 列A)を登録する
        var halfEquality = Tuple.Create(lTableColumn, rTableColumn);
        if(!_halfEqualities.Contains(halfEquality)) {
          _halfEqualities.Add(halfEquality);
        }
        // (列A <- 1)があれば(列B <- 1)を登録する
        //if(_halfEqualitiesToLiteral.ContainsKey(rTableColumn.Table, rTableColumn.ColumnName)) {
        //  var literal = _halfEqualitiesToLiteral[rTableColumn.Table, rTableColumn.ColumnName];
        //  this.Add(lTableColumn, literal);
        //}
        if(_halfEqualitiesToLiteral.ContainsKey(rTableColumn.Table) &&
           _halfEqualitiesToLiteral[rTableColumn.Table].ContainsKey(rTableColumn.ColumnName)) {
          var literal = _halfEqualitiesToLiteral[rTableColumn.Table][rTableColumn.ColumnName];
          this.Add(lTableColumn, literal);
        }
      }

    }

    // <テーブル名, 列名, リテラル値>
    private readonly CNFSet _cnfSet;

    // ORとNOTの入子階層
    private int _orNestLevel = 0;
    private int _notNestLevel = 0;

    public GetCNFVisitor(BestCaseDictionary<IEnumerable<string>> tableColumns
                        , bool ignoreCase = true)
    : base(tableColumns, ignoreCase)
    {
      //_cnfSet = new TwoKeysDictionary<Table, string, string>();
      _cnfSet = new CNFSet();
    }

    public Dictionary<Table, Dictionary<string, Literal>> CNF {
      get {
        if(_cnfSet == null) {
          var ret = new CNFSet();
          return ret.GetEqualities();
        }
        return _cnfSet.GetEqualities();
      }
    }

    /// <summary>
    /// デバッグ用出力
    /// </summary>
    /// <param name="indent"></param>
    /// <returns></returns>
    public string Print(bool indent = false) {
      StringBuilder ret = new StringBuilder();
      var equalities = _cnfSet.GetEqualities();
      var isFirstLoop = true;
      foreach(var equality in equalities) {
        var table = equality.Key;
        if(isFirstLoop) {
          isFirstLoop = false;
        } else if(indent) {
          ret.AppendLine();
        } else {
          ret.Append(" ");
        }
        ret.Append(this.GetTableAliasName(table) + " :");
        foreach(var equaltyToLiteral in equality.Value) {
          ret.Append(" " + equaltyToLiteral.Key + " = " +
                     equaltyToLiteral.Value);
        }
      }
      return ret.ToString();
    }

    /// <summary>
    /// デバッグ用出力
    /// </summary>
    /// <param name="indent"></param>
    /// <returns></returns>
    public string PrintResultInfoList() {
      var ret = "";

      if(_stack.Count == 0) {
        return ret;
      }

      var resultInfoList = _stack.Pop();

      foreach(var resultInfo in resultInfoList) {
        ret += resultInfo.ColumnAliasName +" : ";
        foreach(var source in resultInfo.Sources) {
          ret += source.Table.Name + "." + source.ColumnName + " ";
        }
        ret += Environment.NewLine;
      }

      return ret;
    }

    public override void Visit(Table table) {
      if(this.IsNotInMainResultsSource()) {
        return;
      }

      if(_tableColumns.ContainsKey(table.Name)) {
        var tableResultInfoList = new ResultInfoList();
        foreach(string tableColumnName in _tableColumns[table.Name]) {
          tableResultInfoList.Add(
            new ResultInfo(table.GetAliasOrTableName()
                         , tableColumnName
                         , new TableAndColumn(table, tableColumnName))
          );
        }
        _cnfSet.Add(table);
        _stack.Push(tableResultInfoList);
      } else {
        _cnfSet.Add(table);
        _stack.Push(new ResultInfoList());
      }

      // EXISTS句内で用いるテーブルへの一致条件はCNFに含めない
      if(_subQueryStack.Contains(SubQueryType.Exists)) {
        _cnfSet.AddExistsTable(table);
      }
    }


    #region CNF accumulate process

    public override void VisitAfter(BinaryOpPredicate predicate) {
      if(this.IsNotInMainResultsSource()) {
        return;
      }

      // OR又はNOTの被演算子ではない、かつ"="演算子の場合に一致条件として抽出する
      if((predicate.Operator != PredicateOperator.Equal && predicate.Operator != PredicateOperator.Equal2)
         || _orNestLevel > 0 || _notNestLevel > 0) {
        return;
      }

      //
      // 一致条件の抽出
      //

      if(predicate.Left.GetType() == typeof(Column) && predicate.Right.GetType() == typeof(Column)) {
        Column lColumn = (Column)predicate.Left;
        Column rColumn = (Column)predicate.Right;

        var bothColumnsReferCurrentSources = false;
        if(this.GetSourcesOf(lColumn, _stack.Peek()) == null) {
          if(this.GetSourcesOf(rColumn, _stack.Peek()) == null) {
            // サブクエリの抽出元テーブルが被演算子になっていない一致条件は無視する
            return;
          } else {
            // サブクエリの抽出元テーブルがあればlColumnにこれを格納する
            var tmp = lColumn;
            lColumn = rColumn;
            lColumn = tmp;
          }
        } else {
          if(this.GetSourcesOf(rColumn, _stack.Peek()) == null) {
          } else {
            // 両方のColumnが現在のサブクエリの抽出元テーブルを参照する
            bothColumnsReferCurrentSources = true;
          }
        }

        foreach(var lSource in this.GetSourcesOf(lColumn)) {
          foreach(var rSource in this.GetSourcesOf(rColumn)) {
            // JOIINの場合は双方向
            if(bothColumnsReferCurrentSources || this.CurrentSubQueryIsNot(SubQueryType.Exists)) {
              _cnfSet.Add(lSource, rSource);
            }
            if(bothColumnsReferCurrentSources || this.CurrentSubQueryIsNot(SubQueryType.InResults)) {
              _cnfSet.Add(rSource, lSource);
            }
          }
        }

      } else {
        Column column;
        Literal literal;

        if(predicate.Left.GetType() == typeof(Column) && predicate.Right is Literal) {
          column = (Column)predicate.Left;
          literal = (Literal)predicate.Right;
        } else if(predicate.Left is Literal && predicate.Right.GetType() == typeof(Column)) {
          column = (Column)predicate.Right;
          literal = (Literal)predicate.Left;
        } else {
          return;
        } // if

        // サブクエリの抽出元テーブルが被演算子になっていない一致条件は無視する
        if(this.GetSourcesOf(column, _stack.Peek()) == null) {
          return;
        }

        foreach(var source in this.GetSourcesOf(column)) {
          _cnfSet.Add(source, literal);
        }

      } // if
    }

    private List<TableAndColumn> GetSourcesOf(Column column) {
      // 現在の参照スコープ内を探す
      var ret = this.GetSourcesOf(column, _stack.Peek());
      if(ret != null) {
        return ret;
      }

      // 現在の参照スコープより外側のスコープを探す
      var scopeQueue = new Queue<ResultInfoList>();

      foreach(var resultInfoList in _stack) {
        if(resultInfoList.IsLeftOperandOfJoinOrCompoundOp) {
          scopeQueue.Clear();
        } else {
          scopeQueue.Enqueue(resultInfoList);
        }
      }

      foreach(var resultInfoList in scopeQueue) {
        ret = this.GetSourcesOf(column, resultInfoList);
        if(ret != null) {
          return ret;
        }
      }
      
      return new List<TableAndColumn>();
    }

    private List<TableAndColumn> GetSourcesOf(Column column, ResultInfoList resultInfoList) {
      foreach(var resultInfo in resultInfoList) {
        if(resultInfo.IsDirectSource(column, _ignoreCase)) {
          return resultInfo.Sources;
        }
      }
      return null;
    }

    public override void VisitBefore(OrPredicate orPredicate) {
      if(this.IsNotInMainResultsSource()) {
        return;
      }
      ++_orNestLevel;
    }

    public override void VisitAfter(OrPredicate orPredicate) {
      if(this.IsNotInMainResultsSource()) {
        return;
      }
      --_orNestLevel;
    }

    public override void VisitBefore(NotPredicate notPredicate) {
      if(this.IsNotInMainResultsSource()) {
        return;
      }
      ++_notNestLevel;
    }

    public override void VisitAfter(NotPredicate notPredicate) {
      if(this.IsNotInMainResultsSource()) {
        return;
      }
      --_notNestLevel;
    }

    #endregion


    public override void VisitOnValues(InsertValuesStmt insertValuesStmt, int offset) {
      var table = insertValuesStmt.Table;

      if(insertValuesStmt.HasTableColumns) {
        for(int i = 0; i < insertValuesStmt.ValuesList.Count; ++i) {
          foreach(var assignment in insertValuesStmt.GetAssignments(i)) {
            var columnName = assignment.Column.Name;
            if(assignment.Value.IsDefault) {

            } else if(assignment.Value is Literal) {
              var tableColumn = new TableAndColumn(table, columnName);
              _cnfSet.Add(tableColumn, (Literal)assignment.Value);
            }
          }
          table = table.Clone();
        }

      } else if(_tableColumns.ContainsKey(table.Name)){
        // テーブル列名がCREATE文の定義順に格納されていることが前提である
        foreach(var values in insertValuesStmt.ValuesList) {
          int columnIndex = 0;
          foreach(var columnName in _tableColumns[table.Name]) {
            var value = values[columnIndex];
            if(value.IsDefault) {

            } else if(value is Literal) {
              var tableColumn = new TableAndColumn(table, columnName);
              _cnfSet.Add(tableColumn, (Literal)value);
            }
            ++columnIndex;
          }
          table = table.Clone();
        }

      }
    }

    private string GetTableAliasName(Table table) {
      // テーブル別名、テーブル別名コメント、テーブル名の順に優先して名称を取得する
      if(!Identifier.IsNullOrEmpty(table.AliasName)) {
        return table.AliasName;
      } else if(!string.IsNullOrEmpty(table.ImplicitAliasName)) {
        return table.ImplicitAliasName;
      } else {
        return table.Name;
      }
    }

    private bool CurrentSubQueryIsNot(SubQueryType subQueryType) {
      return _subQueryStack.Count == 0 || _subQueryStack.Peek() != subQueryType;
    }

  }
}
