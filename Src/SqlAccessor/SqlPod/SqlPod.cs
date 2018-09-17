using System;
using System.Collections.Generic;
using MiniSqlParser;

namespace SqlAccessor
{
  /// <summary>
  /// SqlPodを表す
  /// </summary>
  /// <remarks>全てのSqlPodのスーパークラス</remarks>
  public abstract class SqlPod
  {
    private SqlBuilder.DbmsType _dbms;

    /// <summary>
    /// DBMS種別を設定する
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks>スレッドセーフでないことに注意</remarks>
    internal SqlBuilder.DbmsType Dbms {
      get { return _dbms; }
      set { _dbms = value; }
    }

    /// <summary>
    /// レコードのプロパティ→Viewのカラム名、のマッピングを
    /// ToViewMapperに登録するための初期化用SELECT文を取得する
    /// </summary>
    /// <returns></returns>
    /// <remarks></remarks>
    internal SqlBuilder GetInitSelectSql() {
      //OrderBy句を削除する
      //(Pervasiveでは、1つのSELECT文でCOUNT(*)とOrderBy句は使えない)
      SqlBuilder selectSql = this.SelectSql();
      //件数が多い場合にOrderBy句が指定されていれば遅くなるため、OrderBy句を削除する
      selectSql.ClearOrderBy();

      //最大抽出件数を1にする
      selectSql.SetMaxRows(1);

      //プレースホルダに仮の値を適用しSELECT文を実行可能にする
      selectSql.SetAllPlaceHolderToNull();

      return selectSql;
    }

    /// <summary>
    /// テーブルカラムとプロパティのマッピング情報を
    /// RecordViewTableMapに登録するための更新系SQL文を取得する
    /// </summary>
    /// <returns></returns>
    /// <remarks></remarks>
    internal SqlBuilders GetEntrySqlsForMapping(string entryName) {
      return this.EntrySqls(entryName);
    }

    /// <summary>
    /// SELECT文を取得する
    /// </summary>
    /// <param name="aQuery">最大抽出件数,AND条件,OrderBy句,プレースホルダ値をQueryオブジェクトで指定する</param>
    /// <returns></returns>
    /// <remarks>PervasiveはWhere句などにAS別名が使用できるので、
    /// SelectSql()で生成したSQLを抱合するSELECT文は必要ない。(Oracleでは必要)</remarks>
    internal SqlBuilder GetSelectSql<TRecord>(Query<TRecord> aQuery
                                            , RecordViewTableMap<TRecord> aRecordViewTableMap) 
    where TRecord: class, IRecord, new() {
      SqlBuilder sql = this.SelectSql();

      // OrderBy句の追加
      // (WrapInSelectStar()でOrderBy句の変換処理を行うのでWrapInSelectStar()の前に処理する)
      sql.AddOrderBy(aQuery.GetOrderByExp());

      // AS別名とテーブル列名が重複する場合にSQL実行エラーになることがあるため
      // 全てのSELECT文についてSELECT * で囲む
      sql.WrapInSelectStar(aRecordViewTableMap.GetAllColumns(sql));

      //最大抽出件数の設定
      //OracleのRowNumによる指定は、SELECT * FROMに指定する
      if(aQuery.GetMaxRows() >= 0) {
        sql.SetMaxRows(aQuery.GetMaxRows());
      }

      if(sql.GetAutoWhere()) {
        //AND条件の追加
        sql.AddAndPredicate(aQuery.GetWhereExp());
      }

      //Query.RowRange()で抽出条件が指定されている場合
      if(aQuery.GetRowRange() != null) {
        int begin = aQuery.GetRowRange().Item1;
        int end = aQuery.GetRowRange().Item2;
        sql.SetRowLimit(begin, end);
      }

      return sql;
    }

    internal SqlBuilder GetCountSql<TRecord>(Query<TRecord> aQuery
                                           , RecordViewTableMap<TRecord> aRecordViewTableMap = null)
    where TRecord: class, IRecord, new() {
      //サブクラスでSqlPod.CountSql()にSQLが定義されていなければ、
      //SELECT文にSELECT COUNT(*)を付加する
      if(this.CountSql() == null) {
        SqlBuilder selectSql = this.GetSelectSql(aQuery, aRecordViewTableMap);
        selectSql.ClearOrderBy();
        //Return "SELECT COUNT(*) FROM (" + selectSql + ") V1_"
        selectSql.SetCount();
        return selectSql;
      }

      SqlBuilder sql = this.CountSql();

      //Query.RowRange()で抽出条件が指定されている場合
      if(aQuery.GetRowRange() != null & aRecordViewTableMap != null) {
        int begin = aQuery.GetRowRange().Item1;
        int end = aQuery.GetRowRange().Item2;
        sql.SetRowLimit(begin, end);
      }

      //Pervasiveを除くDBMSではWHERE句などにAS別名は使用できない
      sql.WrapInSelectStar(aRecordViewTableMap.GetAllColumns(sql));

      //最大抽出件数の設定
      //OracleのRowNumによる指定は、SELECT * FROMに指定する
      if(aQuery.GetMaxRows() >= 0) {
        sql.SetMaxRows(aQuery.GetMaxRows());
      }

      //AND条件の追加
      if(sql.GetAutoWhere()) {
        sql.AddAndPredicate(aQuery.GetWhereExp());
      }

      return sql;
    }

    /// <summary>
    /// 指定したSQLエントリ名に対応するSQL文を取得する
    /// </summary>
    /// <param name="sqlEntryName">SQLエントリ名</param>
    /// <returns></returns>
    /// <remarks></remarks>
    internal SqlBuilders GetEntrySqls(string sqlEntryName) {
      return new SqlBuilders(this.EntrySqls(sqlEntryName));
    }

    /// <summary>
    /// このSqlPodに定義されたSQLエントリ名をすべて取得する
    /// </summary>
    /// <returns></returns>
    /// <remarks></remarks>
    internal virtual List<string> GetAllEntryNames() {
      return new List<string>();
    }

    protected abstract SqlBuilder SelectSql();

    protected virtual SqlBuilder CountSql() {
      return null;
    }

    protected virtual SqlBuilders EntrySqls(string sqlEntryName) {
      return new SqlBuilders();
    }
  }
}
