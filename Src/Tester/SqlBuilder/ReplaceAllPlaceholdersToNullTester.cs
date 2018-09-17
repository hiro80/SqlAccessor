using System.Collections.Generic;
using MiniSqlParser;
using NUnit.Framework;

namespace SqlBuilderTester
{
  [TestFixture]
  public class ReplaceAllPlaceholdersToNullTester
  {
    [Test]
    public void SelectStmt() {
      string result;

      result = this.ReplaceToNull(@"select * from T where id=@id and not @id");
      Assert.That(result, Is.EqualTo(@"SELECT * FROM T WHERE id=NULL AND NOT 0=1"));
    }

    [Test]
    public void UpdateStmt() {
      string result;

      result = this.ReplaceToNull(@"update T set T.x = @ph1, y = @ph2");
      Assert.That(result, Is.EqualTo(@"UPDATE T SET T.x=NULL,y=NULL"));
    }

    public string ReplaceToNull(string sql, DBMSType dbmsType = DBMSType.Unknown) {
      var ast = MiniSqlParserAST.CreateStmts(sql, dbmsType);

      var visitor = new ReplaceAllPlaceholdersToNull();
      ast.Accept(visitor);

      var stringifier = new CompactStringifier(4098, true);
      ast.Accept(stringifier);
      return stringifier.ToString();
    }
  }
}
