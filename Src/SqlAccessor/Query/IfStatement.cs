using System;
using System.Collections.Generic;
using MiniSqlParser;

namespace SqlAccessor
{
  /// <summary>
  /// IF文の判定をする
  /// </summary>
  /// <remarks></remarks>
  internal class IfStatement
  {
    public class SqlAndSeq
    {
      private readonly int _seq;
      private readonly SqlBuilders _sqls;
      public SqlAndSeq(int id, SqlBuilders sql) {
        _seq = id;
        _sqls = sql;
      }
      /// <summary>
      /// IF文内でのSQL文の表記順番(0番から付番)
      /// </summary>
      /// <value></value>
      /// <returns></returns>
      /// <remarks></remarks>
      public int Seq {
        get { return _seq; }
      }
      public SqlBuilders Sqls {
        get { return _sqls; }
      }
    }

    /// <summary>
    /// IF文を判定し、条件がTrueになる入れ子SQL文を返す
    /// </summary>
    /// <param name="aDbConn">Db接続</param>
    /// <param name="ifSql">IF文</param>
    /// <param name="placeHolders">IF文の条件式に適用するPlaceHolder</param>
    /// <param name="aCacheStrategy"></param>
    /// <returns></returns>
    /// <remarks>条件がTrueになる入れ子SQL文がない場合、最大の表記順番+1を返す</remarks>
    public static SqlAndSeq Evaluate(IDbConn aDbConn
                                   , SqlBuilder ifSql
                                   , PlaceHolders placeHolders
                                   , Tran.CacheStrategy aCacheStrategy) {
      //IF文でない場合は渡されたSQLをそのまま返す
      if(ifSql.GetStatementType() != SqlBuilder.StatementType.If) {
        return new SqlAndSeq(0, new SqlBuilders(new SqlBuilder[] { ifSql }));
      }

      int i = 0;
      foreach(Tuple<SqlPredicate, SqlBuilders> branch in ifSql.GetIfBranches()) {
        //IF文の条件式にPlaceHolderを適用する
        branch.Item1.Place(placeHolders.ToDictionary());
        string ifCondition = branch.Item1.ToString();
        if(IfStatement.EvaluateCondition(aDbConn, ifCondition, aCacheStrategy)) {
          return new SqlAndSeq(i, branch.Item2);
        }
        i += 1;
      }

      return new SqlAndSeq(i, new SqlBuilders());
    }

    /// <summary>
    /// IF文内のSQL文の件数を返す
    /// </summary>
    /// <param name="ifSql"></param>
    /// <returns></returns>
    /// <remarks></remarks>
    public static int CountSubStatement(SqlBuilder ifSql) {
      //IF文でもNULL文でもない場合、SQL文は1件
      if(ifSql.GetStatementType() != SqlBuilder.StatementType.If ||
         ifSql.GetStatementType() != SqlBuilder.StatementType.Null) {
        return 1;
      }
      return ifSql.CountIfBranches();
    }

    /// <summary>
    /// 条件式の判定結果を返す
    /// </summary>
    /// <param name="aDbConn"></param>
    /// <param name="ifCondition"></param>
    /// <param name="aCacheStrategy"></param>
    /// <returns></returns>
    /// <remarks></remarks>
    public static bool EvaluateCondition(IDbConn aDbConn
                                       , string ifCondition
                                       , Tran.CacheStrategy aCacheStrategy) {
      if(string.IsNullOrEmpty(ifCondition)) {
        throw new System.ArgumentNullException("ifCondition", "指定された条件式が空です");
      }

      //参照テーブル名を取得するため、ここで仮のSQL文を作成する
      SqlBuilder conditionSql = new SqlBuilder("SELECT 1 WHERE " + ifCondition);
      HashSet<string> usedTableNames = conditionSql.GetAllTableNames();
      //ダミーテーブルTを除外する
      //usedTableNames.Remove(conditionSql.GetSrcTableNames())
      return aDbConn.ExecExp(ifCondition, usedTableNames, aCacheStrategy);
    }
  }
}
