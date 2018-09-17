using System;
using System.Collections.Generic;
using MiniSqlParser;
using NUnit.Framework;

namespace SqlBuilderTester
{
  [TestFixture]
  public class SqlBuildersTester
  {
    [Test]
    public void NullStmt() {
      SqlBuilders sqls;

      sqls = new SqlBuilders(";");
      Assert.That(sqls[0].ToString(), Is.EqualTo(";"));

      sqls = new SqlBuilders("/** abc */;");
      Assert.That(sqls[0].ToString(SqlBuilder.IndentType.Beautiful)
     , Is.EqualTo("/** abc */" + Environment.NewLine + ";" + Environment.NewLine));
    }

    [Test]
    public void SelectStmt() {
      SqlBuilders sqls;

      sqls = new SqlBuilders("select * from T ; select * from U");
      Assert.That(sqls[0].ToString(), Is.EqualTo("SELECT * FROM T;"));
      Assert.That(sqls[1].ToString(), Is.EqualTo("SELECT * FROM U"));

      sqls = new SqlBuilders("/* 1 */; /* 2 */select * from T ; /* 3 */select * from U");
      Assert.That(sqls[0].ToString(SqlBuilder.IndentType.Beautiful)
        , Is.EqualTo("/* 1 */" + Environment.NewLine + ";" + Environment.NewLine));
      Assert.That(sqls[1].ToString(SqlBuilder.IndentType.Beautiful)
        , Is.EqualTo("/* 2 */" + Environment.NewLine + "SELECT * FROM T;" + Environment.NewLine));
      Assert.That(sqls[2].ToString(SqlBuilder.IndentType.Beautiful)
        , Is.EqualTo("/* 3 */" + Environment.NewLine + "SELECT * FROM U"));

    }
  }
}
