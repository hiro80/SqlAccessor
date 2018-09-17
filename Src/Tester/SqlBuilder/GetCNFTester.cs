using System.Collections.Generic;
using MiniSqlParser;
using NUnit.Framework;

namespace SqlBuilderTester
{
  [TestFixture]
  public class GetCNFTester
  {

    [Test]
    public void Case() {
      string result;

      result = this.GetCNF(@"select * from T
                             where case when T.x = 1 then 1
                                        else 0
                                   end = 0");
      Assert.That(result, Is.EqualTo(@"T :"));

    }

    [Test]
    public void SubQueryInExpr() {
      string result;

      result = this.GetCNF(@"select 
                               func(
                                 (select 1 from T
                                  where T.x = U.x
                                    and T.x = 0)
                               ) as f
                             from U
                             where f=1");
      Assert.That(result, Is.EqualTo(@"T : x = 0 "
                                    + "U :"));

    }

    [Test]
    public void SubQueryInFrom() {
      string result;

      result = this.GetCNF(@"select 1 from
                               (select * from T) v
                             where v.x=5");
      Assert.That(result, Is.EqualTo(@"T : x = 5"));

      result = this.GetCNF(@"select v.q from (
                               select (select T.x from T) as q , U.* from U
                             ) v
                             where v.q = 1 and v.y=2");
      Assert.That(result, Is.EqualTo(@"T : x = 1 "
                                    + "U : y = 2"));

      result = this.GetCNF(@"select v.q from (
                               select (select T.x from T) as q , U.* from U
                             ) v
                             where v.q = 1 and v.y=2");
      Assert.That(result, Is.EqualTo(@"T : x = 1 "
                                    + "U : y = 2"));

      result = this.GetCNF(@"select * from
                              (select x,y,z from T)
                             where x=1");
      Assert.That(result, Is.EqualTo(@"T : x = 1"));
    }


    [Test]
    public void SubQueryInFrom2() {
      string result;

      result = this.GetCNF(@"SELECT * FROM T P WHERE EXISTS
                             (SELECT * FROM (SELECT * FROM T P)V_ WHERE V_.x=1) AND x=3");
      Assert.That(result, Is.EqualTo(@"P : x = 3"));

      result = this.GetCNF(@"SELECT * FROM T P WHERE x=3 
                             AND EXISTS(SELECT * FROM (SELECT *FROM T P_)V_ WHERE V_.x=1)");
      Assert.That(result, Is.EqualTo(@"P : x = 3"));

      result = this.GetCNF(@"SELECT * FROM T P WHERE EXISTS
                             (SELECT * FROM (SELECT * FROM T P_)V1_ JOIN (SELECT * FROM U Q_)V2_ ON 1=1 
                              WHERE V2_.x=2) AND x=3");
      Assert.That(result, Is.EqualTo(@"P : x = 3"));

      result = this.GetCNF(@"SELECT (SELECT * FROM (SELECT * FROM T P_)V1_ JOIN 
                             (SELECT * FROM U Q_)V2_ ON 1=1 WHERE V2_.x=2) FROM T P WHERE x=3");
      Assert.That(result, Is.EqualTo(@"P : x = 3 "
                                    + "Q_ : x = 2 "
                                    + "P_ :"));
    }

    [Test]
    public void Join() {
      string result;

      result = this.GetCNF(@"select T.* from T left join U on T.x = U.x
                             where x='abc'");
      Assert.That(result, Is.EqualTo(@"T : x = 'abc' "
                                    + "U : x = 'abc'"));

      result = this.GetCNF(@"select T.* from
                               T left join U on T.x = U.x and T.y = U.y
                             where x='abc' and y='def'");
      Assert.That(result, Is.EqualTo(@"T : x = 'abc' y = 'def' "
                                    + "U : x = 'abc' y = 'def'"));

      result = this.GetCNF(@"select T.* from T left join A on T.x = A.x
                             where x='abc'");
      Assert.That(result, Is.EqualTo(@"T : x = 'abc' "
                                    + "A :"));

      result = this.GetCNF(@"select T.* from T left join A on x='abc'
                             where T.x = A.x");
      Assert.That(result, Is.EqualTo(@"T : x = 'abc' "
                                    + "A :"));

      result = this.GetCNF(@"select T.* from T /** t1 */ left join A /** a1 */ 
                                          on x='abc'
                             where T.x = A.x");
      Assert.That(result, Is.EqualTo(@"t1 : x = 'abc' "
                                    + "a1 :"));
    }


    [Test]
    public void CommaJoin() {
      string result;

      result = this.GetCNF(@"select * from T, U
                             where T.x = U.x
                               and T.x = 1");
      Assert.That(result, Is.EqualTo(@"T : x = 1 "
                                    + "U : x = 1"));

      result = this.GetCNF(@"select * from T, U
                             where T.x = 1
                               and T.x = U.x");
      Assert.That(result, Is.EqualTo(@"T : x = 1 "
                                    + "U : x = 1"));

      result = this.GetCNF(@"select * from (select x from U) u1, (select x1 from V) v1
                             where x = 1 and x1 = 9");
      Assert.That(result, Is.EqualTo(@"U : x = 1 "
                                    + "V : x1 = 9"));

      result = this.GetCNF(@"select * from (select x from U) , (select x1 from V)
                             where x = 1 and x1 = 9");
      Assert.That(result, Is.EqualTo(@"U : x = 1 "
                                    + "V : x1 = 9"));
    }


    [Test]
    public void SubQueryInResults() {
      string result;

      result = this.GetCNF(@"select 
                               (select 1 from T
                                where T.x = U.x
                                  and T.x = 1)
                             from U");
      Assert.That(result, Is.EqualTo(@"T : x = 1 "
                                    + "U :"));

      result = this.GetCNF(@"select 
                               (select 1 from T
                                where T.x = 1
                                  and T.x = U.x)
                             from U");
      Assert.That(result, Is.EqualTo(@"T : x = 1 "
                                    + "U :"));

      result = this.GetCNF(@"select 
                               (select 1 from T
                                where U.x = 1)
                             from U");
      Assert.That(result, Is.EqualTo(@"U : "
                                    + "T :"));

      result = this.GetCNF(@"select
                              (select U.x from U
                               where U.x = T.x)
                             from T
                             where T.x = 1");
      Assert.That(result, Is.EqualTo(@"T : x = 1 "
                                    + "U : x = 1"));

      result = this.GetCNF(@"select
                               (select
                                 (select U.x from U
                                  where U.x = V.x1)
                                from V
                                where V.x1 = T.x)
                             from T
                             where T.x = 1");
      Assert.That(result, Is.EqualTo(@"T : x = 1 "
                                    + "V : x1 = 1 "
                                    + "U : x = 1"));

      result = this.GetCNF(@"select 
                               (select x from U
                                where U.x = T.x
                                  and U.x = 1)
                              ,(select x from U
                                where U.x = T.x)
                            from T");
      Assert.That(result, Is.EqualTo(@"U : x = 1 "
                                    + "T : "
                                    + "U :"));

      result = this.GetCNF(@"select 
                               (select x from U
                                where U.x = T.x)
                              ,(select x from U
                                where U.x = 1)
                            from T
                            where T.x = 9");
      Assert.That(result, Is.EqualTo(@"T : x = 9 "
                                    + "U : x = 9 "
                                    + "U : x = 1"));
    }

    [Test]
    public void SubQueryUnionInResults() {
      string result;

      result = this.GetCNF(@"select 
                               (select x from U
                                where U.x = T.x
                                union all
                                select x from U
                                where U.x = 1)
                            from T
                            where T.x = 9");
      Assert.That(result, Is.EqualTo(@"T : x = 9 "
                                    + "U : x = 9 "
                                    + "U : x = 1"));
    }

    [Test]
    public void Exists() {
      string result;

      result = this.GetCNF(@"select * from T t0
                             where exists (select * from T t1 
                                           where exists (select * from U
                                                         where U.x = t1.x
                                                           and U.x = 1)
                                             and t1.x = t0.x
                                          )");
      Assert.That(result, Is.EqualTo(@"t0 : x = 1"));

      result = this.GetCNF(@"select x from T
                             where 
                               exists (select y from U join V on U.x = V.x1
                                       where U.x = T.x
                                         and V.x1 = 1)");
      Assert.That(result, Is.EqualTo(@"T : x = 1"));

      result = this.GetCNF(@"select * from T
                             where T.x = 1
                               and exists (select * from U
                                           where U.x = T.x)");
      Assert.That(result, Is.EqualTo(@"T : x = 1"));

      result = this.GetCNF(@"select * from T
                             where exists (select * from U
                                           where U.x = T.x)
                               and T.x = 1");
      Assert.That(result, Is.EqualTo(@"T : x = 1"));

      result = this.GetCNF(@"select * from T
                             where exists (select * from U
                                           where U.x = T.x
                                             and U.x = 1)
                               and exists (select * from U
                                           where U.x = T.x)");
      Assert.That(result, Is.EqualTo(@"T : x = 1"));
    }

    [Test]
    public void ExistsUnion() {
      string result;

      result = this.GetCNF(@"select * from T
                             where
                              exists(
	                             select T.x from T
	                             union all
	                             select U.x from U
                              )");
      Assert.That(result, Is.EqualTo(@"T :"));
    }

    [Test]
    public void Union() {
      string result;

      result = this.GetCNF(@"select * from (
	                           select T.x from T
	                           union all
	                           select U.x from U
                             ) v
                             where x = 2");
      Assert.That(result, Is.EqualTo(@"T : x = 2 "
                                    + "U : x = 2"));

      result = this.GetCNF(@"select * from (
	                            select x, y, z from T group by x
	                            union all
	                            select x, y, z from U
                            ) v
                            where v.z = 9");
      Assert.That(result, Is.EqualTo(@"U : z = 9 "
                                    + "T :"));
    }


    [Test]
    public void GroupBy() {
      string result;

      result = this.GetCNF(@"select x, y, z from T
                             where x=1 and y=2 and z=3
                             group by x, y");
      Assert.That(result, Is.EqualTo(@"T : x = 1 y = 2 z = 3"));

      result = this.GetCNF(@"select * from
                               (select x, y, z from T
                                group by x, y) v
                             where x=1 and y=2 and z=3");
      Assert.That(result, Is.EqualTo(@"T : x = 1 y = 2"));

      result = this.GetCNF(@"select x, y, count(*) from T
                             where x=1 and y=2
                             group by x, y");
      Assert.That(result, Is.EqualTo(@"T : x = 1 y = 2"));

      result = this.GetCNF(@"select * from
                               (select x, y, count(*) from T
                                group by x, y) v
                             where x=1 and y=2");
      Assert.That(result, Is.EqualTo(@"T : x = 1 y = 2"));
    }

    [Test]
    public void AggregateFunc() {
      string result;

      result = this.GetCNF(@"select count(*) from T
                             where x = 1");
      Assert.That(result, Is.EqualTo(@"T : x = 1"));

      result = this.GetCNF(@"select * from
	                           (select count(*),x from T) v
                             where x =1");
      Assert.That(result, Is.EqualTo(@"T :"));
    }


    [Test]
    public void Distinct() {
      string result;

      result = this.GetCNF(@"select distinct x, y, z from T
                             where y = 1 and z = 2");
      Assert.That(result, Is.EqualTo(@"T : y = 1 z = 2"));

      result = this.GetCNF(@"select distinct * from T
                             where y = 1 and z = 2");
      Assert.That(result, Is.EqualTo(@"T : y = 1 z = 2"));
    }

    [Test]
    public void WithoutFrom() {
      string result;

      result = this.GetCNF(@"select 1");
      Assert.That(result, Is.EqualTo(@""));

      result = this.GetCNF(@"select x");
      Assert.That(result, Is.EqualTo(@""));

      result = this.GetCNF(@"select (select * from T where x = 5)");
      Assert.That(result, Is.EqualTo(@"T : x = 5"));
    }

    [Test]
    public void UpdateStmt() {
      string result;

      result = this.GetCNF(@"update T set x=1 where y=2");
      Assert.That(result, Is.EqualTo(@"T : y = 2"));

      result = this.GetCNF(@"update T set
                               x = 1
                              ,y = 'aaa'
                              ,z = (select x from U
                                    where U.x = 0)
                             where T.x = 1");
      Assert.That(result, Is.EqualTo(@"T : x = 1"));

      result = this.GetCNF(@"update T /** t1 */ set z= 1 where x =2 and T.z =3");
      Assert.That(result, Is.EqualTo(@"t1 : x = 2 z = 3"));

    }


    [Test]
    public void InsertStmt() {
      string result;

      result = this.GetCNF(@"insert into T values(1,2,3)");
      Assert.That(result, Is.EqualTo(@"T : x = 1 y = 2 z = 3"));

      result = this.GetCNF(@"insert into T /** t1 */ values(1,2,3)");
      Assert.That(result, Is.EqualTo(@"t1 : x = 1 y = 2 z = 3"));
    }


    [Test]
    public void MergeStmt() {
      string result;


    }


    [Test]
    public void DeleteStmt() {
      string result;

      result = this.GetCNF(@"delete from T where x=1 and y=1");
      Assert.That(result, Is.EqualTo(@"T : x = 1 y = 1"));

      result = this.GetCNF(@"delete from T /** t1 */ where x=1 and T.y=1");
      Assert.That(result, Is.EqualTo(@"t1 : x = 1 y = 1"));
    }

    [Test]
    public void SimpleQuery() {
      string result;

      result = this.GetCNF(@"select * from T");
      Assert.That(result, Is.EqualTo(@"T :"));

      result = this.GetCNF(@"select * from A");
      Assert.That(result, Is.EqualTo(@"A :"));

      result = this.GetCNF(@"select * from T where x=1 and y=2 and z=3");
      Assert.That(result, Is.EqualTo(@"T : x = 1 y = 2 z = 3"));

      result = this.GetCNF(@"select * from T where (x=1 or y=2) and z=3");
      Assert.That(result, Is.EqualTo(@"T : z = 3"));

      result = this.GetCNF(@"select * from T where x=1 and not y=2 and z=3");
      Assert.That(result, Is.EqualTo(@"T : x = 1 z = 3"));

      result = this.GetCNF(@"select * from T where x=1 and x=2");
      Assert.That(result, Is.EqualTo(@""));

      result = this.GetCNF(@"select * from T where x=1 or x=2");
      Assert.That(result, Is.EqualTo(@"T :"));

      result = this.GetCNF(@"select T.* from T /** t1 */
                             where T.x = 1");
      Assert.That(result, Is.EqualTo(@"t1 : x = 1"));
     }

    [Test]
    public void EscapedAliasName() {
      string result;

      result = this.GetCNF(@"select * from T [Table] where Table.x=1 and [Table].y=2", DBMSType.MsSql);
      Assert.That(result, Is.EqualTo(@"Table : x = 1 y = 2"));

      result = this.GetCNF(@"select Table.*, [Table].* from T as [Table] /** t1 */
                             where [Table].x = 1", DBMSType.MsSql);
      Assert.That(result, Is.EqualTo(@"Table : x = 1"));
    }

    //[Test]
    //public void Predicate() {
    //  string result;

    //  result = this.GetCNFOfPredicate(@"x=1 and y='abc'", DBMSType.MsSql);
    //  Assert.That(result, Is.EqualTo(@"Table : x = 1 y = 'abc'"));

    //  result = this.GetCNFOfPredicate(@"x=z and y='abc'", DBMSType.MsSql);
    //  Assert.That(result, Is.EqualTo(@"Table : y = 'abc'"));

    //}

    private string GetCNF(string sql, DBMSType dbmsType = DBMSType.Unknown, bool ignoreCase = true) {
      var ast = MiniSqlParserAST.CreateStmts(sql, dbmsType);
      var visitor = new GetCNFVisitor(_tableColumns, ignoreCase);
      ast.Accept(visitor);
      return visitor.Print();
    }

    private string GetCNFOfPredicate(string predicate, DBMSType dbmsType = DBMSType.Unknown, bool ignoreCase = true) {
      var ast = MiniSqlParserAST.CreatePredicate(predicate, dbmsType);
      var visitor = new GetCNFVisitor(_tableColumns, ignoreCase);
      ast.Accept(visitor);
      return visitor.Print();
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