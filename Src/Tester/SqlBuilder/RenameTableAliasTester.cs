using System.Collections.Generic;
using MiniSqlParser;
using NUnit.Framework;

namespace SqlBuilderTester
{
  [TestFixture]
  public class RenameTableAliasTester
  {

    [Test]
    public void UpdateStmt() {
      string result;

      result = this.Rename(@"update T set T.x = 1, y = 'abc'");
      Assert.That(result, Is.EqualTo(@"UPDATE T Table SET Table.x=1,y='abc'"));

      result = this.Rename(@"update T set T.x = 1, y = (select y from T)");
      Assert.That(result, Is.EqualTo(@"UPDATE T Table SET Table.x=1,y=(SELECT y FROM T)"));

      result = this.Rename(@"update T /** t1 */ set T.x = 1, y = 'abc'");
      Assert.That(result, Is.EqualTo(@"UPDATE T/** t1 */ Table SET Table.x=1,y='abc'"));

      result = this.Rename(@"update T set T.x = 1, U.y = 'abc' from U");
      Assert.That(result, Is.EqualTo(@"UPDATE T Table SET Table.x=1,U.y='abc' FROM U"));

      result = this.Rename(@"update T set T.x = 1, U.y = 'abc' from U "
                          + "where T.x = U.x and T.y = 1");
      Assert.That(result, Is.EqualTo(@"UPDATE T Table SET Table.x=1,U.y='abc' FROM U "
                          + "WHERE Table.x=U.x AND Table.y=1"));
    }

    [Test]
    public void InsertStmt() {
      string result;

      result = this.Rename(@"insert into T values(1,2,3)");
      Assert.That(result, Is.EqualTo(@"INSERT INTO T VALUES(1,2,3)"));

      result = this.Rename(@"insert into T /** t1 */ values(1,2,3)");
      Assert.That(result, Is.EqualTo(@"INSERT INTO T/** t1 */ VALUES(1,2,3)"));

      result = this.Rename(@"insert into T values(T.x,T.y,T.z)");
      Assert.That(result, Is.EqualTo(@"INSERT INTO T VALUES(T.x,T.y,T.z)"));

      result = this.Rename(@"insert into T select U.x, U.y from U");
      Assert.That(result, Is.EqualTo(@"INSERT INTO T SELECT U.x,U.y FROM U"));
    }

    [Test]
    public void DeleteStmt() {
      string result;

      result = this.Rename(@"delete from T where x=1 and T.y=1");
      Assert.That(result, Is.EqualTo(@"DELETE FROM T Table WHERE x=1 AND Table.y=1"));

      result = this.Rename(@"delete from T /** t1 */ where x=1 and T.y=1");
      Assert.That(result, Is.EqualTo(@"DELETE FROM T/** t1 */ Table WHERE x=1 AND Table.y=1"));

      result = this.Rename(@"delete from T from U where T.x=1 and U.y=1");
      Assert.That(result, Is.EqualTo(@"DELETE FROM T Table FROM U WHERE Table.x=1 AND U.y=1"));
    }

    [Test]
    public void TruncateStmt() {
      string result;

      result = this.Rename(@"truncate Table T");
      Assert.That(result, Is.EqualTo(@"TRUNCATE TABLE T"));
    }

    [Test]
    public void SimpleQuery() {
      string result;

      result = this.Rename(@"select * from T");
      Assert.That(result, Is.EqualTo(@"SELECT * FROM T Table"));

      result = this.Rename(@"select * from T T");
      Assert.That(result, Is.EqualTo(@"SELECT * FROM T Table"));

      result = this.Rename(@"select * from A");
      Assert.That(result, Is.EqualTo(@"SELECT * FROM A"));

      result = this.Rename(@"select T.col from T where T.col = 'abc'");
      Assert.That(result, Is.EqualTo(@"SELECT Table.col FROM T Table WHERE Table.col='abc'"));
    }

    [Test]
    public void Join() {
      string result;

      result = this.Rename(@"select T.* from T left join U on T.x = U.x
                             where x='abc'");
      Assert.That(result, Is.EqualTo(@"SELECT Table.* FROM T Table LEFT JOIN U ON Table.x=U.x "
                                    + "WHERE x='abc'"));

      result = this.Rename(@"select T.* from T /** t1 */ left join A /** a1 */ 
                                          on x='abc'
                             where T.x = A.x");
      Assert.That(result, Is.EqualTo(@"SELECT Table.* FROM T/** t1 */ Table LEFT JOIN A/** a1 */ "
                                    + "ON x='abc' WHERE Table.x=A.x"));
    }

    [Test]
    public void SubQueryInResults() {
      string result;

      result = this.Rename(@"select 
                               (select 1 from T
                                where T.x = U.x
                                  and T.x = 1)
                             from U");
      Assert.That(result, Is.EqualTo(@"SELECT (SELECT 1 FROM T Table WHERE Table.x=U.x"
                                    + " AND Table.x=1) FROM U"));


    }

    [Test]
    public void Exists() {
      string result;

      result = this.Rename(@"select * from T
                             where exists (select * from T t1
                                           where exists (select * from U
                                                         where U.x = t1.x
                                                           and U.x = 1)
                                             and t1.x = T.x
                                          )");
      Assert.That(result, Is.EqualTo(@"SELECT * FROM T Table WHERE EXISTS(SELECT * FROM T t1 "
                                    + "WHERE EXISTS(SELECT * FROM U WHERE U.x=t1.x "
                                    + "AND U.x=1) AND t1.x=Table.x)"));

    }

    [Test]
    public void Union() {
      string result;

      result = this.Rename(@"select * from (
	                           select T.x from T
	                           union all
	                           select U.x from U
                             ) v
                             where x = 2");
      Assert.That(result, Is.EqualTo(@"SELECT * FROM (SELECT Table.x FROM T Table"
	                                + " UNION ALL SELECT U.x FROM U)v WHERE x=2"));

    }

    [Test]
    public void GroupBy() {
      string result;

      result = this.Rename(@"select x, y, z from T
                             where T.x=1 and y=2 and z=3
                             group by T.x, y");
      Assert.That(result, Is.EqualTo(@"SELECT x,y,z FROM T Table "
                                    + "WHERE Table.x=1 AND y=2 AND z=3 "
                                    + "GROUP BY Table.x,y"));

    }

    [Test]
    public void EscapedAliasName() {
      string result;

      result = this.Rename(@"select * from Table [T] where T.x=1 and [T].y=2", DBMSType.MsSql);
      Assert.That(result, Is.EqualTo(@"SELECT * FROM Table Table WHERE Table.x=1 AND Table.y=2"));

      result = this.Rename(@"select Table.*, [T].* from Table as [T] /** t1 */
                             where [T].x = 1", DBMSType.MsSql);
      Assert.That(result, Is.EqualTo(@"SELECT Table.*,Table.* FROM Table AS Table/** t1 */ "
                                    + "WHERE Table.x=1"));
    }

    [Test]
    public void CollisionName() {
      string result;

      result = this.Rename(@"select * from T join U ""Table"" on T.x = ""Table"".x", DBMSType.Oracle);
      Assert.That(result, Is.EqualTo(@"SELECT * FROM T Table_ JOIN U ""Table"" ON Table_.x=""Table"".x"));
    }
    

    public string Rename(string sql, DBMSType dbmsType = DBMSType.Unknown, bool ignoreCase = true) {
      var ast = MiniSqlParserAST.CreateStmts(sql, dbmsType);

      var visitor = new RenameTableAliasVisitor("T", "Table");
      ast.Accept(visitor);

      var stringifier = new CompactStringifier(4098, true);
      ast.Accept(stringifier);
      return stringifier.ToString();
    }
  }
}
