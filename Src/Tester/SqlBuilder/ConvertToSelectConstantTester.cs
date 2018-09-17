using MiniSqlParser;
using NUnit.Framework;

namespace SqlBuilderTester
{
  [TestFixture]
  public class ConvertToSelectConstantTester
  {

    [Test]
    public void SimpleQuery() {
      string result;

      result = this.Convert(@"select x, y, z from T");
      Assert.That(result, Is.EqualTo(@"SELECT 1 FROM T"));

      result = this.Convert(@"select x, y, z from T where x = 'abc'");
      Assert.That(result, Is.EqualTo(@"SELECT 1 FROM T WHERE x='abc'"));

      result = this.Convert(@"select x, y, z from T "
                                + "where x = 'abc' and y = 'def'");
      Assert.That(result, Is.EqualTo(@"SELECT 1 FROM T "
                                    + "WHERE x='abc' AND y='def'"));

      result = this.Convert(@"select x, y, z from T "
                                + "where x = 'abc' or y = 'def'");
      Assert.That(result, Is.EqualTo(@"SELECT 1 FROM T "
                                    + "WHERE x='abc' OR y='def'"));

      result = this.Convert(@"select x, y, z from T "
                                + "where (x = 'abc' or y = 'def') and z = 'ghq'");
      Assert.That(result, Is.EqualTo(@"SELECT 1 FROM T "
                                    + "WHERE (x='abc' OR y='def') AND z='ghq'"));

      result = this.Convert(@"select x, y, z from T "
                                + "where z = 'ghq' and (x = 'abc' or y = 'def')");
      Assert.That(result, Is.EqualTo(@"SELECT 1 FROM T "
                                    + "WHERE z='ghq' AND (x='abc' OR y='def')"));

      result = this.Convert(@"select x, y, z from T where x = 'abc'");
      Assert.That(result, Is.EqualTo(@"SELECT 1 FROM T WHERE x='abc'"));

      result = this.Convert(@"select x, y, z from T where x = 'abc'");
      Assert.That(result, Is.EqualTo(@"SELECT 1 FROM T WHERE x='abc'"));
    }

    [Test]
    public void Union() {
      string result;

      result = this.Convert(@"select x, y, z from T where x = 'abc' "
                           + "union "
                           + "select a, b, c from U");
      Assert.That(result, Is.EqualTo(@"SELECT 1 FROM T WHERE x='abc' "
                                    + "UNION "
                                    + "SELECT 1 FROM U"));

      result = this.Convert(@"select x, y, z from T where x = 'abc' "
                           + "union "
                           + "select a, b, c from U "
                           + "union all "
                           + "(select a, b from U "
                           + " union all "
                           + " select x, y from T )");
      Assert.That(result, Is.EqualTo(@"SELECT 1 FROM T WHERE x='abc' "
                                    + "UNION "
                                    + "SELECT 1 FROM U "
                                    + "UNION ALL "
                                    + "(SELECT 1 FROM U"
                                    + " UNION ALL"
                                    + " SELECT 1 FROM T)"));
    }

    [Test]
    public void Bracket() {
      string result;

      result = this.Convert(@"(((select x, y, z from T where x = 'abc')))");
      Assert.That(result, Is.EqualTo(@"(((SELECT 1 FROM T WHERE x='abc')))"));
    }

    private string Convert(string sql) {
      var ast = MiniSqlParserAST.CreateStmts(sql);

      var visitor = new ConvertToSelectConstant();

      ast.Accept(visitor);

      var stringifier = new CompactStringifier(4098, true);
      ast.Accept(stringifier);

      return stringifier.ToString();
    }
  }

}