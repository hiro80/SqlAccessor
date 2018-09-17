using System.Collections.Generic;
using MiniSqlParser;
using NUnit.Framework;

namespace SqlBuilderTester
{
  [TestFixture]
  public class GetIfConditionsTester
  {

    [Test]
    public void SimpleQuery() {
      Result result;

      result = GetResult("select * from T");
      Assert.That(result.Conditions.Count, Is.EqualTo(0));
      Assert.That(result.StmtsList.Count, Is.EqualTo(0));
    }

    [Test]
    public void IfStmt() {
      Result result;

      result = GetResult("IF 1=0 OR 'aa'='bb' THEN SELECT * FROM A "
                       + "ELSIF 2=0           THEN SELECT * FROM B "
                       + "                    ELSE SELECT * FROM C END IF");
      Assert.That(result.Conditions[0], Is.EqualTo(@"1=0 OR 'aa'='bb'"));
      Assert.That(result.Conditions[1], Is.EqualTo(@"NOT (1=0 OR 'aa'='bb') AND 2=0"));
      Assert.That(result.Conditions[2], Is.EqualTo(@"NOT (1=0 OR 'aa'='bb') AND NOT 2=0"));
      Assert.That(result.StmtsList[0], Is.EqualTo(@"SELECT * FROM A"));
      Assert.That(result.StmtsList[1], Is.EqualTo(@"SELECT * FROM B"));
      Assert.That(result.StmtsList[2], Is.EqualTo(@"SELECT * FROM C"));

      result = GetResult("IF 1=0 AND 'aa'='bb' THEN SELECT * FROM A "
                        + "ELSIF 2=0           THEN SELECT * FROM B "
                        + "                    ELSE SELECT * FROM C END IF");
      Assert.That(result.Conditions[0], Is.EqualTo(@"1=0 AND 'aa'='bb'"));
      Assert.That(result.Conditions[1], Is.EqualTo(@"NOT (1=0 AND 'aa'='bb') AND 2=0"));
      Assert.That(result.Conditions[2], Is.EqualTo(@"NOT (1=0 AND 'aa'='bb') AND NOT 2=0"));
      Assert.That(result.StmtsList[0], Is.EqualTo(@"SELECT * FROM A"));
      Assert.That(result.StmtsList[1], Is.EqualTo(@"SELECT * FROM B"));
      Assert.That(result.StmtsList[2], Is.EqualTo(@"SELECT * FROM C"));

      result = GetResult("IF 2=0                 THEN SELECT * FROM A "
                       + "ELSIF 1=0 OR 'aa'='bb' THEN SELECT * FROM B "
                       + "                       ELSE SELECT * FROM C END IF");
      Assert.That(result.Conditions[0], Is.EqualTo(@"2=0"));
      Assert.That(result.Conditions[1], Is.EqualTo(@"NOT 2=0 AND (1=0 OR 'aa'='bb')"));
      Assert.That(result.Conditions[2], Is.EqualTo(@"NOT 2=0 AND NOT (1=0 OR 'aa'='bb')"));
      Assert.That(result.StmtsList[0], Is.EqualTo(@"SELECT * FROM A"));
      Assert.That(result.StmtsList[1], Is.EqualTo(@"SELECT * FROM B"));
      Assert.That(result.StmtsList[2], Is.EqualTo(@"SELECT * FROM C"));

      result = GetResult("IF 1=0 THEN "
                       + "    IF EXISTS (SELECT * FROM A) THEN SELECT * FROM B END IF "
                       + "ELSIF 2=0 THEN SELECT * FROM C "
                       + "          ELSE SELECT * FROM D END IF");
      Assert.That(result.Conditions[0], Is.EqualTo(@"1=0"));
      Assert.That(result.Conditions[1], Is.EqualTo(@"NOT 1=0 AND 2=0"));
      Assert.That(result.Conditions[2], Is.EqualTo(@"NOT 1=0 AND NOT 2=0"));
      Assert.That(result.StmtsList[0], Is.EqualTo(@"IF EXISTS(SELECT * FROM A) THEN SELECT * FROM B END IF "));
      Assert.That(result.StmtsList[1], Is.EqualTo(@"SELECT * FROM C"));
      Assert.That(result.StmtsList[2], Is.EqualTo(@"SELECT * FROM D"));

      result = GetResult("IF 1 = 0 THEN "
                       + "    IF EXISTS (SELECT * FROM A) THEN SELECT * FROM B "
                       + "    ELSIF 2 = 0 THEN SELECT * FROM C "
                       + "    ELSE SELECT * FROM D END IF "
                       + "END IF");
      Assert.That(result.Conditions[0], Is.EqualTo(@"1=0"));
      Assert.That(result.StmtsList[0], Is.EqualTo(@"IF EXISTS(SELECT * FROM A) THEN SELECT * FROM B "
                                                 + "ELSIF 2=0 THEN SELECT * FROM C "
                                                 + "ELSE SELECT * FROM D END IF "));

      result = GetResult(result.StmtsList[0]);
      Assert.That(result.Conditions[0], Is.EqualTo(@"EXISTS(SELECT * FROM A)"));
      Assert.That(result.Conditions[1], Is.EqualTo(@"NOT EXISTS(SELECT * FROM A) AND 2=0"));
      Assert.That(result.Conditions[2], Is.EqualTo(@"NOT EXISTS(SELECT * FROM A) AND NOT 2=0"));
      Assert.That(result.StmtsList[0], Is.EqualTo(@"SELECT * FROM B"));
      Assert.That(result.StmtsList[1], Is.EqualTo(@"SELECT * FROM C"));
      Assert.That(result.StmtsList[2], Is.EqualTo(@"SELECT * FROM D"));
    }


    private Result GetResult(string sql) {
      var ast = MiniSqlParserAST.CreateStmts(sql);
      var visitor = new GetIfConditionsVisitor();
      ast.Accept(visitor);

      var conditions = new List<string>();
      var stmtList = new List<string>();
      for(int i = 0; i < visitor.Count; ++i) {
        // 条件式の取得
        var stringifier = new CompactStringifier(144);
        visitor.Conditions[i].Accept(stringifier);
        conditions.Add(stringifier.ToString());
        // 文の取得(ただし1番目の文のみ)
        stringifier = new CompactStringifier(144);
        visitor.StmtsList[i][0].Accept(stringifier);
        stmtList.Add(stringifier.ToString());
      }
      return new Result(conditions, stmtList);
    }

    private class Result
    {
      public List<string> Conditions;
      public List<string> StmtsList;
      public Result(List<string> conditions
                  , List<string> stmtsList) {
        this.Conditions = conditions;
        this.StmtsList = stmtsList;
      }
    }

  }
}