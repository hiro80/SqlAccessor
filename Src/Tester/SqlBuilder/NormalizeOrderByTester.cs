using System.Collections.Generic;
using MiniSqlParser;
using NUnit.Framework;

namespace SqlBuilderTester
{
  [TestFixture]
  public class NormalizeOrderByTester
  {
    [Test]
    public void SimpleOrderBy() {
      string result;

      result = this.NormalizeOrderBy(@"select x from T order by x");
      Assert.That(result, Is.EqualTo(@"SELECT x FROM T ORDER BY x"));

      result = this.NormalizeOrderBy(@"select x as A from T order by x");
      Assert.That(result, Is.EqualTo(@"SELECT x AS A FROM T ORDER BY A"));

      result = this.NormalizeOrderBy(@"select * from T order by x, y");
      Assert.That(result, Is.EqualTo(@"SELECT * FROM T ORDER BY x,y"));

      result = this.NormalizeOrderBy(@"select T.* from T order by x, y");
      Assert.That(result, Is.EqualTo(@"SELECT T.* FROM T ORDER BY x,y"));
    }

    [Test]
    public void Join() {
      string result;

      result = this.NormalizeOrderBy(@"select U.x from T left join U on T.x = U.x "
                                    + "order by x");
      Assert.That(result, Is.EqualTo(@"SELECT U.x FROM T LEFT JOIN U ON T.x=U.x "
                                    + "ORDER BY x"));

      result = this.NormalizeOrderBy(@"select x from T left join U on T.x = U.x "
                                    + "order by x");
      Assert.That(result, Is.EqualTo(@"SELECT x FROM T LEFT JOIN U ON T.x=U.x "
                                    + "ORDER BY x"));

      result = this.NormalizeOrderBy(@"select x from T left join U on T.x = U.x "
                                    + "order by y");
      Assert.That(result, Is.EqualTo(@"SELECT x,y FROM T LEFT JOIN U ON T.x=U.x "
                                    + "ORDER BY y"));
    }

    [Test]
    public void Bracketed() {
      string result;

      // メインクエリを括弧で囲むとSQLiteでは文法エラーとなる
      result = this.NormalizeOrderBy(@"((select x as A from T left join U on T.x = U.x))"
                                    + "order by x,y");
      // 実装の都合上、yを補完できない
      Assert.That(result, Is.EqualTo(@"((SELECT x AS A FROM T LEFT JOIN U ON T.x=U.x)) "
                                    + "ORDER BY A,y"));
    }

    [Test]
    public void Union() {
      string result;

      result = this.NormalizeOrderBy(@"select x,  y  from T left join U on T.x = U.x "
                                    + "union all "
                                    + "select x1, x2 from V "
                                    + "order by x");
      Assert.That(result, Is.EqualTo(@"SELECT x,y FROM T LEFT JOIN U ON T.x=U.x "
                                    + "UNION ALL "
                                    + "SELECT x1,x2 FROM V "
                                    + "ORDER BY x"));

      result = this.NormalizeOrderBy(@"select x,  y as B from T left join U on T.x = U.x "
                                    + "union all "
                                    + "select x1, x2 from V "
                                    + "order by y");
      Assert.That(result, Is.EqualTo(@"SELECT x,y AS B FROM T LEFT JOIN U ON T.x=U.x "
                                    + "UNION ALL "
                                    + "SELECT x1,x2 FROM V "
                                    + "ORDER BY B"));

      result = this.NormalizeOrderBy(@"select x,  y  from T left join U on T.x = U.x "
                                    + "union all "
                                    + "select x1, x2 from V "
                                    + "order by z");
      Assert.That(result, Is.EqualTo(@"SELECT x,y FROM T LEFT JOIN U ON T.x=U.x "
                                    + "UNION ALL "
                                    + "SELECT x1,x2 FROM V"));

      result = this.NormalizeOrderBy(@"select x,  y  from T left join U on T.x = U.x "
                                    + "union all "
                                    + "select x1, x2 from V "
                                    + "union all "
                                    + "select 1,  2 "
                                    + "order by y");
      Assert.That(result, Is.EqualTo(@"SELECT x,y FROM T LEFT JOIN U ON T.x=U.x "
                                    + "UNION ALL "
                                    + "SELECT x1,x2 FROM V "
                                    + "UNION ALL "
                                    + "SELECT 1,2 "
                                    + "ORDER BY y"));
    }

    [Test]
    public void SubQueryInResults() {
      string result;

      result = this.NormalizeOrderBy(@"select "
                                    + "  (select x from U u1 "
                                    + "   where u1.x = t1.x) as A "
                                    + "from T "
                                    + "order by A");
      Assert.That(result, Is.EqualTo(@"SELECT "
                                    + "(SELECT x FROM U u1 "
                                    + "WHERE u1.x=t1.x) AS A "
                                    + "FROM T "
                                    + "ORDER BY A"));
    }

    [Test]
    public void SubQueryInOrderBy() {
      string result;

      result = this.NormalizeOrderBy(@"select x as A from T t1 "
                                    + "where exists (select * from U u1 "
                                    + "              where u1.x = t1.x) "
                                    + "order by (select x from U u1 "
                                    + "          where u1.x = t1.x)");
      Assert.That(result, Is.EqualTo(@"SELECT x AS A FROM T t1 "
                                    + "WHERE EXISTS(SELECT * FROM U u1 "
                                    + "WHERE u1.x=t1.x) "
                                    + "ORDER BY (SELECT x FROM U u1 "
                                    + "WHERE u1.x=A)"));

      result = this.NormalizeOrderBy(@"select x as A from T t1 "
                                    + "where exists (select * from U u1 "
                                    + "              where u1.x = t1.x) "
                                    + "order by (select x from U u1 "
                                    + "          where u1.x = x)");
      Assert.That(result, Is.EqualTo(@"SELECT x AS A FROM T t1 "
                                    + "WHERE EXISTS(SELECT * FROM U u1 "
                                    + "WHERE u1.x=t1.x) "
                                    + "ORDER BY (SELECT x FROM U u1 "
                                    + "WHERE u1.x=x)"));

      result = this.NormalizeOrderBy(@"select x1 as A from V v1 "
                                    + "where exists (select * from U u1 "
                                    + "              where u1.x = x1) "
                                    + "order by (select x from U u1 "
                                    + "          where u1.x = x1)");
      Assert.That(result, Is.EqualTo(@"SELECT x1 AS A FROM V v1 "
                                    + "WHERE EXISTS(SELECT * FROM U u1 "
                                    + "WHERE u1.x=x1) "
                                    + "ORDER BY (SELECT x FROM U u1 "
                                    + "WHERE u1.x=A)"));
    }

    [Test]
    public void Distinct() {
      string result;

      result = this.NormalizeOrderBy(@"select distinct x as A from T t0 "
                                    + "order by "
                                    + "  (select z from T "
                                    + "   where T.x = t0.q "
                                    + "   order by z,t0.q)");
      Assert.That(result, Is.EqualTo(@"SELECT DISTINCT x AS A FROM T t0"));

      result = this.NormalizeOrderBy(@"select distinct x as A from T t0 "
                                    + "order by x, A, t0.y, q");
      Assert.That(result, Is.EqualTo(@"SELECT DISTINCT x AS A FROM T t0 "
                                    + "ORDER BY A,A"));
    }

    [Test]
    public void AggregateFunc() {
      string result;

      result = this.NormalizeOrderBy(@"select count(*) as A from T t0 "
                                    + "order by x, y, z");
      Assert.That(result, Is.EqualTo(@"SELECT count(*) AS A FROM T t0"));
    }

    [Test]
    public void GroupBy() {
      string result;

      result = this.NormalizeOrderBy(@"select x as A from T t0 "
                                    + "group by x "
                                    + "order by "
                                    + "  (select z from T "
                                    + "   where T.x = t0.x "
                                    + "   order by z, t0.x)");
      Assert.That(result, Is.EqualTo(@"SELECT x AS A FROM T t0 "
                                    + "GROUP BY x "
                                    + "ORDER BY "
                                    + "(SELECT z FROM T"
                                    + " WHERE T.x=A"
                                    + " ORDER BY z,A)"));

      result = this.NormalizeOrderBy(@"select x as A from T t0 "
                                    + "group by x "
                                    + "order by "
                                    + "  (select z from T "
                                    + "   where T.x = t0.q "
                                    + "   order by z, t0.x)");
      Assert.That(result, Is.EqualTo(@"SELECT x AS A FROM T t0 "
                                    + "GROUP BY x"));

      result = this.NormalizeOrderBy(@"select x as A from T t0 "
                                    + "group by x "
                                    + "order by "
                                    + "  (select z from T "
                                    + "   where T.x = t0.q "
                                    + "   order by z, t0.q)");
      Assert.That(result, Is.EqualTo(@"SELECT x AS A FROM T t0 "
                                    + "GROUP BY x"));
    }

    [Test]
    public void NoTableColumnsParam() {
      string result;

      result = this.NormalizeOrderBy(@"select x from W order by x");
      Assert.That(result, Is.EqualTo(@"SELECT x FROM W ORDER BY x"));

      result = this.NormalizeOrderBy(@"select x as A from W order by x");
      Assert.That(result, Is.EqualTo(@"SELECT x AS A FROM W ORDER BY A"));

      result = this.NormalizeOrderBy(@"select * from W order by x, y");
      Assert.That(result, Is.EqualTo(@"SELECT * FROM W ORDER BY x,y"));

      result = this.NormalizeOrderBy(@"select (select z from W1 where W1.x=W.x) from W order by x, y");
      Assert.That(result, Is.EqualTo(@"SELECT (SELECT z FROM W1 WHERE W1.x=W.x) FROM W ORDER BY x,y"));

      result = this.NormalizeOrderBy(@"select (select z from W1 where W1.x=W.x),y from W " +
                                      "union all " +
                                      "select x,y from W2 " +
                                      "order by x,y");
      Assert.That(result, Is.EqualTo(@"SELECT (SELECT z FROM W1 WHERE W1.x=W.x),y FROM W " +
                                      "UNION ALL " +
                                      "SELECT x,y FROM W2 " +
                                      "ORDER BY x,y"));
    }

    private string NormalizeOrderBy(string sql, DBMSType dbmsType = DBMSType.Unknown, bool ignoreCase = true) {
      var ast = MiniSqlParserAST.CreateStmts(sql, dbmsType);
      var visitor = new NormalizeOrderByVisitor(_tableColumns, ignoreCase);
      ast.Accept(visitor);
      var stringifier = new CompactStringifier(4098, true);
      ast.Accept(stringifier);
      return stringifier.ToString();
    }

    private BestCaseDictionary<IEnumerable<string>> _tableColumns;
    
    [SetUp]
    public void initTest() {
      // テスト用テーブル列
      _tableColumns = new BestCaseDictionary<IEnumerable<string>>();

      var tableT = new string[] { "x", "y", "z" };
      _tableColumns.Add("T", tableT);

      var tableU = new string[] { "x", "y", "z" };
      _tableColumns.Add("U", tableU);

      var tableV = new string[] { "x1", "x2", "x3" };
      _tableColumns.Add("V", tableV);
    }
  }
}
