using MiniSqlParser;
using NUnit.Framework;

namespace SqlBuilderTester
{
  [TestFixture]
  public class AddWherePredicateTester
  {

    [Test]
    public void SimpleQuery() {
      string result;

      result = this.AddPredicate(@"select x, y, z from T", "x = 1");
      Assert.That(result, Is.EqualTo(@"SELECT x,y,z FROM T WHERE x=1"));

      result = this.AddPredicate(@"select x, y, z from T where x = 'abc'", "y = 'def'");
      Assert.That(result, Is.EqualTo(@"SELECT x,y,z FROM T WHERE x='abc' AND y='def'"));

      result = this.AddPredicate(@"select x, y, z from T "
                                + "where x = 'abc' and y = 'def'", "z = 'ghq'");
      Assert.That(result, Is.EqualTo(@"SELECT x,y,z FROM T "
                                    + "WHERE x='abc' AND y='def' AND z='ghq'"));

      result = this.AddPredicate(@"select x, y, z from T "
                                + "where x = 'abc' or y = 'def'", "z = 'ghq'");
      Assert.That(result, Is.EqualTo(@"SELECT x,y,z FROM T "
                                    + "WHERE (x='abc' OR y='def') AND z='ghq'"));

      result = this.AddPredicate(@"select x, y, z from T "
                                + "where (x = 'abc' or y = 'def') and z = 'ghq'", "z='ghq'");
      Assert.That(result, Is.EqualTo(@"SELECT x,y,z FROM T "
                                    + "WHERE (x='abc' OR y='def') AND z='ghq' AND z='ghq'"));

      result = this.AddPredicate(@"select x, y, z from T "
                                + "where z = 'ghq' and (x = 'abc' or y = 'def')", "z='ghq'");
      Assert.That(result, Is.EqualTo(@"SELECT x,y,z FROM T "
                                    + "WHERE z='ghq' AND (x='abc' OR y='def') AND z='ghq'"));

      result = this.AddPredicate(@"select x, y, z from T where x = 'abc'", "y = 'def' and z=10");
      Assert.That(result, Is.EqualTo(@"SELECT x,y,z FROM T WHERE x='abc' AND y='def' AND z=10"));

      result = this.AddPredicate(@"select x, y, z from T where x = 'abc'", "y = 'def' or z=10");
      Assert.That(result, Is.EqualTo(@"SELECT x,y,z FROM T WHERE x='abc' AND (y='def' OR z=10)"));

      result = this.AddPredicate(@"select x, y, z from T where not x = 'abc'", "y = 'def' or z=10");
      Assert.That(result, Is.EqualTo(@"SELECT x,y,z FROM T WHERE NOT x='abc' AND (y='def' OR z=10)"));

      result = this.AddPredicate(@"select x, y, z from T where not x = 'abc' and y=1", "y = 'def' or z=10");
      Assert.That(result, Is.EqualTo(@"SELECT x,y,z FROM T WHERE NOT x='abc' AND y=1 AND (y='def' OR z=10)"));
    }

    [Test]
    public void UnionAll() {
      string result;

      result = this.AddPredicate(@"select x, y, z from T " +
                                  "union all " +
                                  "select 1, 2, 3 from U"
                                , "x = 1");
      Assert.That(result, Is.EqualTo(@"SELECT x,y,z FROM T UNION ALL SELECT 1,2,3 FROM U"));

    }

    private string AddPredicate(string sql, string predicateStr) {
      var ast = MiniSqlParserAST.CreateStmts(sql);

      var visitor = new AddWherePredicateVisitor(predicateStr);

      ast.Accept(visitor);
      var stringifier = new CompactStringifier(4098, true);
      ast.Accept(stringifier);
      return stringifier.ToString();
    }

  }

}