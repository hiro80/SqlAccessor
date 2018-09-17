using System;
using System.Collections.Generic;
using MiniSqlParser;
using NUnit.Framework;

namespace SqlBuilderTester
{
  [TestFixture]
  public class SqlBuilderTester
  {
    [Test]
    public void EmptyStmt() {
      SqlBuilder sql;
      var branches = new List<Tuple<SqlPredicate, SqlBuilders>>();

      sql = new SqlBuilder();
      Assert.That(sql.GetStatementType(), Is.EqualTo(SqlBuilder.StatementType.Null));

      sql = new SqlBuilder();
      Assert.That(sql.IsValidSyntax(), Is.EqualTo(true));

      sql = new SqlBuilder(@"");
      Assert.That(sql.GetAllTableNames(), Is.EqualTo(new string[] { }));

      sql = new SqlBuilder(@" ");
      Assert.That(sql.GetSrcTableNames(), Is.EqualTo(new string[] { }));

      sql = new SqlBuilder();
      Assert.That(sql.GetSrcTableAliasNames(), Is.EqualTo(new string[] { }));

      sql = new SqlBuilder();
      Assert.That(sql.GetTargetTable(), Is.Null);

      sql = new SqlBuilder();
      Assert.That(sql.GetTargetTable(), Is.Null);

      sql = new SqlBuilder();
      Assert.That(sql.GetWhere().ToString(), Is.EqualTo(""));

      sql = new SqlBuilder();
      Assert.That(sql.GetSelectItems(), Is.EqualTo(new string[] { }));

      sql = new SqlBuilder();
      Assert.That(sql.GetSelectItems(_tableColumnDetails), Is.EqualTo(new string[] { }));

      sql = new SqlBuilder();
      branches.AddRange(sql.GetIfBranches());
      Assert.That(branches.Count, Is.EqualTo(0));

      sql = new SqlBuilder();
      Assert.That(sql.GetCNF(_tableColumns).Count, Is.EqualTo(0));

      sql = new SqlBuilder();
      Assert.That(sql.GetAssignments().Count, Is.EqualTo(0));

      sql = new SqlBuilder();
      Assert.That(sql.ToString(), Is.EqualTo(""));

      sql = new SqlBuilder();
      sql.SetMaxRows(20);
      Assert.That(sql.ToString(), Is.EqualTo(""));

      sql = new SqlBuilder();
      sql.SetRowLimit(8, 16);
      Assert.That(sql.ToString(), Is.EqualTo(""));

      sql = new SqlBuilder();
      sql.SetCount();
      Assert.That(sql.ToString(), Is.EqualTo(""));

      sql = new SqlBuilder();
      sql.SetConstant();
      Assert.That(sql.ToString(), Is.EqualTo(""));

      sql = new SqlBuilder();
      sql.WrapInSelectStar(_tableColumns);
      Assert.That(sql.ToString(), Is.EqualTo(""));

      sql = new SqlBuilder();
      sql.RenameTableAliasName("T", "t1");
      Assert.That(sql.ToString(), Is.EqualTo(""));

      sql = new SqlBuilder();
      sql.AddAndPredicate("x = 1");
      Assert.That(sql.ToString(), Is.EqualTo(""));

      sql = new SqlBuilder();
      sql.AddOrderBy(new List<Tuple<string, bool>> { Tuple.Create("a", true) });
      Assert.That(sql.ToString(), Is.EqualTo(""));

      sql = new SqlBuilder();
      sql.ClearWhere();
      Assert.That(sql.ToString(), Is.EqualTo(""));

      sql = new SqlBuilder();
      sql.ClearOrderBy();
      Assert.That(sql.ToString(), Is.EqualTo(""));

      sql = new SqlBuilder();
      sql.RaisePrimaryKey(_tableColumnDetails);
      Assert.That(sql.ToString(), Is.EqualTo(""));
    }

    [Test]
    public void NullStmt() {
      SqlBuilder sql;

      /* SqlBuilderは単体のSQL文を受け付けるため";"は受け付けない */
      sql = new SqlBuilder(";");
      Assert.That(sql.IsValidSyntax(), Is.False);

      sql = new SqlBuilder("/** abc */;");
      Assert.That(sql.IsValidSyntax(), Is.False);
    }

    [Test]
    public void GetStatementType() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select * from A");
      Assert.That(sql.GetStatementType(), Is.EqualTo(SqlBuilder.StatementType.Select));

      sql = new SqlBuilder(@"update T set T.x = 1, y = 'abc'");
      Assert.That(sql.GetStatementType(), Is.EqualTo(SqlBuilder.StatementType.Update));

      sql = new SqlBuilder(@"insert into T(x,y,z) values(1,2,3)");
      Assert.That(sql.GetStatementType(), Is.EqualTo(SqlBuilder.StatementType.InsertValue));

      sql = new SqlBuilder(@"insert into T(x,y,z) select x, y, z from T");
      Assert.That(sql.GetStatementType(), Is.EqualTo(SqlBuilder.StatementType.InsertSelect));

      sql = new SqlBuilder(@"delete from A where A.id = 'abc'");
      Assert.That(sql.GetStatementType(), Is.EqualTo(SqlBuilder.StatementType.Delete));

      sql = new SqlBuilder(@"if @ph=1 then update T set x=1 "
                          + "else insert into T(x,y,z) values(1,2,3) end if");
      Assert.That(sql.GetStatementType(), Is.EqualTo(SqlBuilder.StatementType.If));
    }

    [Test]
    public void IsValidSyntax() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select * from A where exists (select * from B)");
      Assert.That(sql.IsValidSyntax(), Is.EqualTo(true));

      sql = new SqlBuilder(@"select * from A where and exists (select * from B)");
      Assert.That(sql.IsValidSyntax(), Is.EqualTo(false));

      sql = new SqlBuilder(@"insert T(x,y,z) values(1,2,3)");
      Assert.That(sql.IsValidSyntax(), Is.EqualTo(false));
    }

    [Test]
    public void GetAllTableNames() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select 1");
      Assert.That(sql.GetAllTableNames(), Is.EqualTo(new string[] { }));

      sql = new SqlBuilder(@"select * from A where exists (select * from B)");
      Assert.That(sql.GetAllTableNames(), Is.EqualTo(new string[]{"A","B"}));

      sql = new SqlBuilder(@"select * from A left join B on A.id=B.id where A.id='abc'");
      Assert.That(sql.GetAllTableNames(), Is.EqualTo(new string[] { "A", "B" }));

      sql = new SqlBuilder(@"select (select id from B where B.id=A.id) from A where A.id='abc'");
      Assert.That(sql.GetAllTableNames(), Is.EqualTo(new string[] { "B", "A" }));

      sql = new SqlBuilder(@"select x from A union all select x from B");
      Assert.That(sql.GetAllTableNames(), Is.EqualTo(new string[] { "A", "B" }));

      sql = new SqlBuilder(@"select x from A order by (select id from B where B.id=A.id)");
      Assert.That(sql.GetAllTableNames(), Is.EqualTo(new string[] { "A", "B" }));

      sql = new SqlBuilder(@"select x from A a1 order by (select id from B b1 where b1.id=a1.id)");
      Assert.That(sql.GetAllTableNames(), Is.EqualTo(new string[] { "A", "B" }));

      sql = new SqlBuilder(@"update T set x=(select id from A where A.id=T.id)");
      Assert.That(sql.GetAllTableNames(), Is.EqualTo(new string[] { "T", "A" }));

      sql = new SqlBuilder(@"update T set x=(select id from A where A.id=U.id) from U");
      Assert.That(sql.GetAllTableNames(), Is.EqualTo(new string[] { "T", "A", "U" }));

      sql = new SqlBuilder(@"insert into T(x,y,z) select * from A");
      Assert.That(sql.GetAllTableNames(), Is.EqualTo(new string[] { "T", "A" }));

      sql = new SqlBuilder(@"insert into T(x,y,z) values(1,2,(select id from A))");
      Assert.That(sql.GetAllTableNames(), Is.EqualTo(new string[] { "T", "A" }));

      sql = new SqlBuilder(@"delete from T where exists(select * from A where A.id=T.id)");
      Assert.That(sql.GetAllTableNames(), Is.EqualTo(new string[] { "T", "A" }));

      sql = new SqlBuilder(@"if @ph=1 then update T set x=1 "
                          + "else insert into U(x,y,z) values(1,2,3) end if");
      Assert.That(sql.GetAllTableNames(), Is.EqualTo(new string[] { "T", "U" }));

      sql = new SqlBuilder(@"select 1 from ""Date""", SqlBuilder.DbmsType.Oracle);
      Assert.That(sql.GetSrcTableNames(), Is.EqualTo(new string[] { "Date" }));
    }

    [Test]
    public void GetSrcTableNames() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select 1");
      Assert.That(sql.GetSrcTableNames(), Is.EqualTo(new string[] { }));

      sql = new SqlBuilder(@"select * from A a1 where exists (select * from B b1)");
      Assert.That(sql.GetSrcTableNames(), Is.EqualTo(new string[] { "A" }));

      sql = new SqlBuilder(@"select * from A a1 full join B b1 on a1.id=b1.id "
                          + "left join C c1 on b1.id=c1.id");
      Assert.That(sql.GetSrcTableNames(), Is.EqualTo(new string[] { "A", "B", "C" }));

      sql = new SqlBuilder(@"select (select x from T where exists(select x from U)) "
                          + "from A a1 where exists (select * from B b1)");
      Assert.That(sql.GetSrcTableNames(), Is.EqualTo(new string[] { "T", "A"}));

      sql = new SqlBuilder(@"select x from T t1 "
                          + "union all "
                          + "select y from U i1");
      Assert.That(sql.GetSrcTableNames(), Is.EqualTo(new string[] { "T", "U" }));

      sql = new SqlBuilder(@"select x from A a1 order by (select id from B b1 where b1.id=a1.id)");
      Assert.That(sql.GetSrcTableNames(), Is.EqualTo(new string[] { "A" }));

      sql = new SqlBuilder(@"update T t1 set x=(select id from A where A.id=T.id)");
      Assert.That(sql.GetSrcTableNames(), Is.EqualTo(new string[] { "T", "A" }));

      sql = new SqlBuilder(@"update T t1 set x=(select id from A where A.id=U.id) from U");
      Assert.That(sql.GetSrcTableNames(), Is.EqualTo(new string[] { "T", "A", "U" }));

      sql = new SqlBuilder(@"insert into T(x,y,z) select * from A");
      Assert.That(sql.GetSrcTableNames(), Is.EqualTo(new string[] { "T", "A" }));

      sql = new SqlBuilder(@"insert into T(x,y,z) values(1,2,(select id from A))");
      Assert.That(sql.GetSrcTableNames(), Is.EqualTo(new string[] { "T", "A" }));

      sql = new SqlBuilder(@"delete from T t1 where exists(select * from A where A.id=T.id)");
      Assert.That(sql.GetSrcTableNames(), Is.EqualTo(new string[] { "T" }));

      sql = new SqlBuilder(@"if @ph=1 then update T t1 set x=1 "
                          + "else insert into U(x,y,z) values(1,2,3) end if");
      Assert.That(sql.GetSrcTableNames(), Is.EqualTo(new string[] { "T", "U" }));

      sql = new SqlBuilder(@"select 1 from ""Date""", SqlBuilder.DbmsType.Oracle);
      Assert.That(sql.GetSrcTableNames(), Is.EqualTo(new string[] { "Date" }));
    }

    [Test]
    public void GetSrcTableAliasNames() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select 1");
      Assert.That(sql.GetSrcTableAliasNames(), Is.EqualTo(new string[] { }));

      sql = new SqlBuilder(@"select * from A a1 where exists (select * from B b1)");
      Assert.That(sql.GetSrcTableAliasNames(), Is.EqualTo(new string[] { "a1" }));

      sql = new SqlBuilder(@"select * from A a1 full join B b1 on a1.id=b1.id "
                          + "left join C c1 on b1.id=c1.id");
      Assert.That(sql.GetSrcTableAliasNames(), Is.EqualTo(new string[] { "a1", "b1", "c1" }));

      sql = new SqlBuilder(@"select (select x from T where exists(select x from U)) "
                          + "from A a1 where exists (select * from B b1)");
      Assert.That(sql.GetSrcTableAliasNames(), Is.EqualTo(new string[] { "T", "a1" }));

      sql = new SqlBuilder(@"select x from T t1 "
                          + "union all "
                          + "select y from U u1");
      Assert.That(sql.GetSrcTableAliasNames(), Is.EqualTo(new string[] { "t1", "u1" }));

      sql = new SqlBuilder(@"select x from A a1 order by (select id from B b1 where b1.id=a1.id)");
      Assert.That(sql.GetSrcTableAliasNames(), Is.EqualTo(new string[] { "a1" }));

      sql = new SqlBuilder(@"update T t1 set x=(select id from A where A.id=T.id)");
      Assert.That(sql.GetSrcTableAliasNames(), Is.EqualTo(new string[] { "t1", "A" }));

      sql = new SqlBuilder(@"update T t1 set x=(select id from A where A.id=U.id) from U");
      Assert.That(sql.GetSrcTableAliasNames(), Is.EqualTo(new string[] { "t1", "A", "U" }));

      sql = new SqlBuilder(@"insert into T(x,y,z) select * from A");
      Assert.That(sql.GetSrcTableAliasNames(), Is.EqualTo(new string[] { "T", "A" }));

      sql = new SqlBuilder(@"insert into T /** t1 */ (x,y,z) values(1,2,(select id from A))");
      Assert.That(sql.GetSrcTableAliasNames(), Is.EqualTo(new string[] { "t1", "A" }));

      sql = new SqlBuilder(@"delete from T /** t1 */ where exists(select * from A where A.id=T.id)");
      Assert.That(sql.GetSrcTableAliasNames(), Is.EqualTo(new string[] { "t1" }));

      sql = new SqlBuilder(@"if @ph=1 then update T t1 set x=1 "
                          + "else insert into U(x,y,z) values(1,2,3) end if");
      Assert.That(sql.GetSrcTableAliasNames(), Is.EqualTo(new string[] { "t1", "U" }));
    }

    [Test]
    public void GetTargetTableName() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select x as a from T /** t1 */ "
                          + "where T.x = 1 and y =2 ");
      Assert.That(sql.GetTargetTable(), Is.Null);

      sql = new SqlBuilder(@"update T t1 set x=(select id from A where A.id=T.id)");
      Assert.That(sql.GetTargetTable().Name, Is.EqualTo("T"));

      sql = new SqlBuilder(@"update T t1 set x=(select id from A where A.id=U.id) from U u1");
      Assert.That(sql.GetTargetTable().Name, Is.EqualTo("T"));

      sql = new SqlBuilder(@"insert into T(x,y,z) select * from A");
      Assert.That(sql.GetTargetTable().Name, Is.EqualTo("T"));

      sql = new SqlBuilder(@"insert into T /** t1 */ (x,y,z) values(1,2,(select id from A))");
      Assert.That(sql.GetTargetTable().Name, Is.EqualTo("T"));

      sql = new SqlBuilder(@"delete from T t1 /** t2 */ where exists(select * from A where A.id=T.id)");
      Assert.That(sql.GetTargetTable().Name, Is.EqualTo("T"));

      sql = new SqlBuilder(@"delete from T t1 from U u1 where T.id = U.id");
      Assert.That(sql.GetTargetTable().Name, Is.EqualTo("T"));

      sql = new SqlBuilder(@"if @ph=1 then update T t1 set x=1 "
                          + "elsif @ph between 0 and 1 then select * from A "
                          + "else insert into U(x,y,z) values(1,2,3) end if");
      Assert.That(sql.GetTargetTable().Name, Is.EqualTo("T"));
    }

    [Test]
    public void GetTargetTableAliasName() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select x as a from T /** t1 */ "
                          + "where T.x = 1 and y =2 ");
      Assert.That(sql.GetTargetTable(), Is.Null);

      sql = new SqlBuilder(@"update T t1 set x=(select id from A where A.id=T.id)");
      Assert.That(sql.GetTargetTable().AliasName, Is.EqualTo("t1"));

      sql = new SqlBuilder(@"update T t1 set x=(select id from A where A.id=U.id) from U u1");
      Assert.That(sql.GetTargetTable().AliasName, Is.EqualTo("t1"));

      sql = new SqlBuilder(@"insert into T(x,y,z) select * from A");
      Assert.That(sql.GetTargetTable().AliasName, Is.EqualTo("T"));

      sql = new SqlBuilder(@"insert into T /** t1 */ (x,y,z) values(1,2,(select id from A))");
      Assert.That(sql.GetTargetTable().AliasName, Is.EqualTo("t1"));

      sql = new SqlBuilder(@"delete from T t1 /** t2 */ where exists(select * from A where A.id=T.id)");
      Assert.That(sql.GetTargetTable().AliasName, Is.EqualTo("t1"));

      sql = new SqlBuilder(@"delete from T t1 from U u1 where T.id = U.id");
      Assert.That(sql.GetTargetTable().AliasName, Is.EqualTo("t1"));

      sql = new SqlBuilder(@"if @ph=1 then update T t1 set x=1 "
                          + "elsif @ph between 0 and 1 then select * from A "
                          + "else insert into U(x,y,z) values(1,2,3) end if");
      Assert.That(sql.GetTargetTable().AliasName, Is.EqualTo("t1"));
    }

    [Test]
    public void GetTargetTableExplicitAliasName() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select x as a from T /** t1 */ "
                          + "where T.x = 1 and y =2 ");
      Assert.That(sql.GetTargetTable(), Is.Null);

      sql = new SqlBuilder(@"update T t1 set x=(select id from A where A.id=T.id)");
      Assert.That(sql.GetTargetTable().ExplicitAliasName, Is.EqualTo("t1"));

      sql = new SqlBuilder(@"update T t1 set x=(select id from A where A.id=U.id) from U u1");
      Assert.That(sql.GetTargetTable().ExplicitAliasName, Is.EqualTo("t1"));

      sql = new SqlBuilder(@"insert into T(x,y,z) select * from A");
      Assert.That(sql.GetTargetTable().ExplicitAliasName, Is.EqualTo("T"));

      sql = new SqlBuilder(@"insert into T /** t1 */ (x,y,z) values(1,2,(select id from A))");
      Assert.That(sql.GetTargetTable().ExplicitAliasName, Is.EqualTo("T"));

      sql = new SqlBuilder(@"delete from T t1 /** t2 */ where exists(select * from A where A.id=T.id)");
      Assert.That(sql.GetTargetTable().ExplicitAliasName, Is.EqualTo("t1"));

      sql = new SqlBuilder(@"delete from T t1 from U u1 where T.id = U.id");
      Assert.That(sql.GetTargetTable().ExplicitAliasName, Is.EqualTo("t1"));

      sql = new SqlBuilder(@"if @ph=1 then update T t1 set x=1 "
                          + "elsif @ph between 0 and 1 then select * from A "
                          + "else insert into U(x,y,z) values(1,2,3) end if");
      Assert.That(sql.GetTargetTable().ExplicitAliasName, Is.EqualTo("t1"));
    }

    [Test]
    public void GetWhere() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select x as a from T /** t1 */ "
                          + "where T.x = 1 and y =2 ");
      Assert.That(sql.GetWhere().ToString(), Is.EqualTo("T.x=1 AND y=2"));

      sql = new SqlBuilder(@"select 1");
      Assert.That(sql.GetWhere().ToString(), Is.EqualTo(""));

      sql = new SqlBuilder(@"select * from A a1 where exists (select * from B b1)");
      Assert.That(sql.GetWhere().ToString(), Is.EqualTo("EXISTS(SELECT * FROM B b1)"));

      sql = new SqlBuilder(@"select * from A a1 full join B b1 on a1.id=b1.id "
                          + "left join C c1 on b1.id=c1.id "
                          + "where not c1.x=b1.x");
      Assert.That(sql.GetWhere().ToString(), Is.EqualTo("NOT c1.x=b1.x"));

      sql = new SqlBuilder(@"select (select x from T where exists(select x from U)) "
                          + "from A a1 where exists (select * from B b1)");
      Assert.That(sql.GetWhere().ToString(), Is.EqualTo("EXISTS(SELECT * FROM B b1)"));

      sql = new SqlBuilder(@"select x from T t1 where t1.x=2 "
                          + "union all "
                          + "select y from U u1 where u1.y=3");
      Assert.That(sql.GetWhere().ToString(), Is.EqualTo(""));

      sql = new SqlBuilder(@"select x from A a1 order by (select id from B b1 where b1.id=a1.id)");
      Assert.That(sql.GetWhere().ToString(), Is.EqualTo(""));

      sql = new SqlBuilder(@"update T t1 set x=(select id from A where A.id=T.id) "
                          + "where t1.x = 1 or t1.x=2");
      Assert.That(sql.GetWhere().ToString(), Is.EqualTo("t1.x=1 OR t1.x=2"));

      sql = new SqlBuilder(@"update T t1 set x=(select id from A where A.id=T.id) "
                          + "where t1.x = 1 or t1.x=2");
      Assert.That(sql.GetWhere().ToString(), Is.EqualTo("t1.x=1 OR t1.x=2"));
    }

    [Test]
    public void GetSelectItems() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select x, y from T");
      Assert.That(sql.GetSelectItems(), Is.EqualTo(new string[] { "x", "y" }));

      sql = new SqlBuilder(@"select x as a, y as b from T");
      Assert.That(sql.GetSelectItems(), Is.EqualTo(new string[] { "a", "b" }));

      sql = new SqlBuilder(@"(select x as a, y as b from T)");
      Assert.That(sql.GetSelectItems(), Is.EqualTo(new string[] { "a", "b" }));

      sql = new SqlBuilder(@"select 1, 1+2, x||b, (a) from T");
      Assert.That(sql.GetSelectItems(), Is.EqualTo(new string[] { null, null, null, "a" }));

      sql = new SqlBuilder(@"select (select x from T where exists(select x from U)) as a "
                          + "from A a1 where exists (select * from B b1)");
      Assert.That(sql.GetSelectItems(), Is.EqualTo(new string[] { "a" }));

      sql = new SqlBuilder(@"select x, y from T t1 where t1.x=2 "
                          + "union all "
                          + "select a, b from U u1 where u1.y=3");
      Assert.That(sql.GetSelectItems(), Is.EqualTo(new string[] { "x", "y" }));

      sql = new SqlBuilder(@"select x as a, y as b from "
                          + "(select aa, bb, cc from T) t ");
      Assert.That(sql.GetSelectItems(), Is.EqualTo(new string[] { "a", "b" }));

      sql = new SqlBuilder(@"delete from T where a=1");
      Assert.That(sql.GetSelectItems(), Is.EqualTo(new string[] {  }));

      sql = new SqlBuilder(@"select T.x as ""Date"" from T", SqlBuilder.DbmsType.Oracle);
      Assert.That(sql.GetSelectItems(), Is.EqualTo(new string[] { "Date" }));
    }

    [Test]
    public void GetSelectItems2() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select * from T");
      Assert.That(sql.GetSelectItems(_tableColumnDetails), Is.EqualTo(new string[]{"x", "y", "z"}));

      sql = new SqlBuilder(@"select x, y, z from U");
      Assert.That(sql.GetSelectItems(_tableColumnDetails), Is.EqualTo(new string[] { "x", "y", "z" }));

      sql = new SqlBuilder(@"select z from U");
      Assert.That(sql.GetSelectItems(_tableColumnDetails), Is.EqualTo(new string[] { "z" }));

      sql = new SqlBuilder(@"select * from (select x, y, z from U) V_");
      Assert.That(sql.GetSelectItems(_tableColumnDetails), Is.EqualTo(new string[] { "x", "y", "z" }));

      sql = new SqlBuilder(@"select x a, y b, z c from U");
      Assert.That(sql.GetSelectItems(_tableColumnDetails), Is.EqualTo(new string[] { "a", "b", "c" }));

      sql = new SqlBuilder(@"select x a, y b, z c from U "
                          + "union all "
                          + "select * from T "
                          + "union all "
                          + "select * from V");
      Assert.That(sql.GetSelectItems(_tableColumnDetails), Is.EqualTo(new string[] { "a", "b", "c" }));

      sql = new SqlBuilder(@"select 1, 1+2, x||b, (a) from T");
      Assert.That(sql.GetSelectItems(_tableColumnDetails), Is.EqualTo(new string[] {null, null, null, "a" }));

      sql = new SqlBuilder(@"delete from T where a=1");
      Assert.That(sql.GetSelectItems(_tableColumnDetails), Is.EqualTo(new string[] {}));

      sql = new SqlBuilder(@"select T.x as ""Date"" from T", SqlBuilder.DbmsType.Oracle);
      Assert.That(sql.GetSelectItems(_tableColumnDetails), Is.EqualTo(new string[] { "Date" }));
    }

    [Test]
    public void GetIfBranches() {
      SqlBuilder sql;
      var branches = new List<Tuple<SqlPredicate, SqlBuilders>>() ;

      sql = new SqlBuilder(@"if exists (select 1) then "
                          + "  select * from T; "
                          + "  select x from U; "
                          + "elsif @PH = 1 then "
                          + " "
                          + "elsif (x) = 'a' or x is null then "
                          + "  delete from T where T.id=1 "
                          + "else "
                          + "  update T set x = 1; "
                          + "  insert into T(x) values(1); "
                          + "end if");
      branches.AddRange(sql.GetIfBranches());

      Assert.That(branches[0].Item1.ToString(), Is.EqualTo(@"EXISTS(SELECT 1)"));
      Assert.That(branches[0].Item2[0].ToString(), Is.EqualTo(@"SELECT * FROM T;"));
      Assert.That(branches[0].Item2[1].ToString(), Is.EqualTo(@"SELECT x FROM U;"));

      Assert.That(branches[1].Item1.ToString(), Is.EqualTo(@"NOT EXISTS(SELECT 1) AND @PH=1"));
      Assert.That(branches[1].Item2, Is.Empty);

      Assert.That(branches[2].Item1.ToString(), Is.EqualTo(@"NOT EXISTS(SELECT 1) AND NOT @PH=1 AND "
                                                          + "((x)='a' OR x IS NULL)"));
      Assert.That(branches[2].Item2[0].ToString(), Is.EqualTo(@"DELETE FROM T WHERE T.id=1"));

      Assert.That(branches[3].Item1.ToString(), Is.EqualTo(@"NOT EXISTS(SELECT 1) AND NOT @PH=1 AND "
                                                          + "NOT ((x)='a' OR x IS NULL)"));
      Assert.That(branches[3].Item2[0].ToString(), Is.EqualTo(@"UPDATE T SET x=1;"));
      Assert.That(branches[3].Item2[1].ToString(), Is.EqualTo(@"INSERT INTO T(x) VALUES(1);"));

      Assert.That(branches.Count, Is.EqualTo(4));
      Assert.That(branches[0].Item2.Count, Is.EqualTo(2));
      Assert.That(branches[1].Item2.Count, Is.EqualTo(0));
      Assert.That(branches[2].Item2.Count, Is.EqualTo(1));
      Assert.That(branches[3].Item2.Count, Is.EqualTo(2));

      Assert.That(sql.CountIfBranches(), Is.EqualTo(4));
    }

    [Test]
    public void GetCNF() {
      SqlBuilder sql;

      var tableT = new SqlTable("T", null);

      sql = new SqlBuilder(@"Truncate Table T");
      Assert.That(sql.GetCNF(_tableColumns)[tableT], Is.EqualTo(new Dictionary<string, string> { }));

      sql = new SqlBuilder(@"select 1");
      Assert.That(sql.GetCNF(_tableColumns).Count, Is.EqualTo(0));

      sql = new SqlBuilder(@"select * from T where T.x='a' and T.y=1 and z=2");
      Assert.That(sql.GetCNF(_tableColumns)[tableT]
      , Is.EqualTo(new Dictionary<string, string> { { "x", "'a'" }, { "y", "1" }, { "z", "2" } }));

      sql = new SqlBuilder(@"delete from T where x=1");
      Assert.That(sql.GetCNF(_tableColumns)[tableT]
      , Is.EqualTo(new Dictionary<string, string> { { "x", "1" } }));

      sql = new SqlBuilder(@"insert into T(x,y,z) values(1,2,3)");
      Assert.That(sql.GetCNF(_tableColumns)[tableT]
      , Is.EqualTo(new Dictionary<string, string> { { "x", "1" }, { "y", "2" }, { "z", "3" } }));
    }

    [Test]
    public void GetAssignments() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select x,y,z from T");
      Assert.That(sql.GetAssignments().Count, Is.EqualTo(0));

      sql = new SqlBuilder(@"update T set x=1,y=2,z='a' where x=1");
      Assert.That(sql.GetAssignments().Count, Is.EqualTo(3));
      Assert.That(Get1stItem(sql.GetAssignments()["x"]).ToString(), Is.EqualTo("1"));
      Assert.That(Get1stItem(sql.GetAssignments()["y"]).ToString(), Is.EqualTo("2"));
      Assert.That(Get1stItem(sql.GetAssignments()["z"]).ToString(), Is.EqualTo("'a'"));

      sql = new SqlBuilder(@"insert into A(x,y,z) values(1+2, col, (select z from T))");
      Assert.That(sql.GetAssignments().Count, Is.EqualTo(3));
      Assert.That(Get1stItem(sql.GetAssignments()["x"]).ToString(), Is.EqualTo("1+2"));
      Assert.That(Get1stItem(sql.GetAssignments()["y"]).ToString(), Is.EqualTo("col"));
      Assert.That(Get1stItem(sql.GetAssignments()["z"]).ToString(), Is.EqualTo("(SELECT z FROM T)"));

      sql = new SqlBuilder(@"insert into A(x,y,z) select x, func(y), z||'a' from T");
      Assert.That(sql.GetAssignments().Count, Is.EqualTo(3));
      Assert.That(Get1stItem(sql.GetAssignments()["x"]).ToString(), Is.EqualTo("x"));
      Assert.That(Get1stItem(sql.GetAssignments()["y"]).ToString(), Is.EqualTo("func(y)"));
      Assert.That(Get1stItem(sql.GetAssignments()["z"]).ToString(), Is.EqualTo("z||'a'"));

      sql = new SqlBuilder(@"insert into A(x,y,z) values(1,2,3),(4,5,6),(7,8,9)");
      Assert.That(sql.GetAssignments().Count, Is.EqualTo(3));

      var x = sql.GetAssignments()["x"].GetEnumerator();
      x.MoveNext();
      Assert.That(x.Current.ToString(), Is.EqualTo("1"));
      x.MoveNext();
      Assert.That(x.Current.ToString(), Is.EqualTo("4"));
      x.MoveNext();
      Assert.That(x.Current.ToString(), Is.EqualTo("7"));
      Assert.That(x.MoveNext(), Is.False);

      var y = sql.GetAssignments()["y"].GetEnumerator();
      y.MoveNext();
      Assert.That(y.Current.ToString(), Is.EqualTo("2"));
      y.MoveNext();
      Assert.That(y.Current.ToString(), Is.EqualTo("5"));
      y.MoveNext();
      Assert.That(y.Current.ToString(), Is.EqualTo("8"));
      Assert.That(y.MoveNext(), Is.False);

      var z = sql.GetAssignments()["z"].GetEnumerator();
      z.MoveNext();
      Assert.That(z.Current.ToString(), Is.EqualTo("3"));
      z.MoveNext();
      Assert.That(z.Current.ToString(), Is.EqualTo("6"));
      z.MoveNext();
      Assert.That(z.Current.ToString(), Is.EqualTo("9"));
      Assert.That(z.MoveNext(), Is.False);
    }

    [Test]
    public new void ToString() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select x,y,z from T");
      Assert.That(sql.ToString(), Is.EqualTo("SELECT x,y,z FROM T"));
    }

    [Test]
    public void SetMaxRows() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select x,y,z from T");
      sql.SetMaxRows(20);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT x,y,z FROM T LIMIT 0,20"));

      sql = new SqlBuilder(@"select x,y,z from T limit 0, 10");
      sql.SetMaxRows(20);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT x,y,z FROM T LIMIT 0,20"));

      sql = new SqlBuilder(@"select x,y,z from T limit 10 offset 0");
      sql.SetMaxRows(20);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT x,y,z FROM T LIMIT 20 OFFSET 0"));

      sql = new SqlBuilder(@"select x from T "
                          + "union all "
                          + "select y from U");
      sql.SetMaxRows(20);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT x FROM T "
                                           + "UNION ALL "
                                           + "SELECT y FROM U "
                                           + "LIMIT 0,20"));

      sql = new SqlBuilder(@"select x,y,z from T", SqlBuilder.DbmsType.Oracle);
      sql.SetMaxRows(20);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT x,y,z FROM T "
                                           + "OFFSET 0 ROWS FETCH NEXT 20 ROWS ONLY"));

      sql = new SqlBuilder(@"select x,y,z from T offset 0 rows", SqlBuilder.DbmsType.Oracle);
      sql.SetMaxRows(20);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT x,y,z FROM T "
                                           + "OFFSET 0 ROWS FETCH NEXT 20 ROWS ONLY"));

      sql = new SqlBuilder(@"select x,y,z from T offset 0 rows fetch next 10 rows with ties"
                          , SqlBuilder.DbmsType.Oracle);
      sql.SetMaxRows(20);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT x,y,z FROM T "
                                           + "OFFSET 0 ROWS FETCH NEXT 20 ROWS ONLY"));

      sql = new SqlBuilder(@"select x from T "
                          + "union all "
                          + "select y from U", SqlBuilder.DbmsType.Oracle);
      sql.SetMaxRows(20);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT x FROM T "
                                           + "UNION ALL "
                                           + "SELECT y FROM U "
                                           + "OFFSET 0 ROWS FETCH NEXT 20 ROWS ONLY"));


      sql = new SqlBuilder(@"select x,y,z from T", SqlBuilder.DbmsType.Pervasive);
      sql.SetMaxRows(20);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT TOP 20 x,y,z FROM T"));

      sql = new SqlBuilder(@"select x from T "
                          + "union all "
                          + "select y from U"
                          , SqlBuilder.DbmsType.Pervasive);
      sql.SetMaxRows(20);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT TOP 20 x FROM T "
                                           + "UNION ALL "
                                           + "SELECT TOP 20 y FROM U"));

      sql = new SqlBuilder(@"select x,y,z from T", SqlBuilder.DbmsType.MsSql);
      sql.SetMaxRows(20);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT TOP 20 x,y,z FROM T"));

      sql = new SqlBuilder(@"select x from T "
                          + "union all "
                          + "select y from U"
                          , SqlBuilder.DbmsType.MsSql);
      sql.SetMaxRows(20);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT TOP 20 x FROM T "
                                           + "UNION ALL "
                                           + "SELECT TOP 20 y FROM U"));
    }

    [Test]
    public void SetRowLimit() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select x,y,z from T");
      sql.SetRowLimit(8, 16);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT x,y,z FROM T LIMIT 8,16"));

      sql = new SqlBuilder(@"select x,y,z from T limit 0, 10");
      sql.SetRowLimit(8, 16);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT x,y,z FROM T LIMIT 8,16"));

      sql = new SqlBuilder(@"select x,y,z from T limit 10 offset 0");
      sql.SetRowLimit(8, 16);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT x,y,z FROM T LIMIT 16 OFFSET 8"));

      sql = new SqlBuilder(@"select x from T "
                          + "union all "
                          + "select y from U");
      sql.SetRowLimit(8, 16);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT x FROM T "
                                           + "UNION ALL "
                                           + "SELECT y FROM U "
                                           + "LIMIT 8,16"));

      sql = new SqlBuilder(@"select x,y,z from T", SqlBuilder.DbmsType.Oracle);
      sql.SetRowLimit(8, 16);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT x,y,z FROM T "
                                           + "OFFSET 8 ROWS FETCH NEXT 16 ROWS ONLY"));

      sql = new SqlBuilder(@"select x,y,z from T offset 0 rows", SqlBuilder.DbmsType.Oracle);
      sql.SetRowLimit(8, 16);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT x,y,z FROM T "
                                           + "OFFSET 8 ROWS FETCH NEXT 16 ROWS ONLY"));

      sql = new SqlBuilder(@"select x,y,z from T offset 0 rows fetch next 10 rows with ties"
                          , SqlBuilder.DbmsType.Oracle);
      sql.SetRowLimit(8, 16);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT x,y,z FROM T "
                                           + "OFFSET 8 ROWS FETCH NEXT 16 ROWS ONLY"));

      sql = new SqlBuilder(@"select x from T "
                          + "union all "
                          + "select y from U", SqlBuilder.DbmsType.Oracle);
      sql.SetRowLimit(8, 16);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT x FROM T "
                                           + "UNION ALL "
                                           + "SELECT y FROM U "
                                           + "OFFSET 8 ROWS FETCH NEXT 16 ROWS ONLY"));
    }

    [Test]
    public void SetCount() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select * from T");
      sql.SetCount();
      Assert.That(sql.ToString(), Is.EqualTo("SELECT COUNT(*) FROM T"));

      sql = new SqlBuilder(@"select x,y,z from T");
      sql.SetCount();
      Assert.That(sql.ToString(), Is.EqualTo("SELECT COUNT(*) FROM T"));

      sql = new SqlBuilder(@"(select x,y,z from T)");
      sql.SetCount();
      Assert.That(sql.ToString(), Is.EqualTo("(SELECT COUNT(*) FROM T)"));

      sql = new SqlBuilder(@"select x,y,z from T "
                          + "union all "
                          + "select 1,2,3");
      sql.SetCount();
      Assert.That(sql.ToString(), Is.EqualTo("SELECT COUNT(*) FROM "
                                            + "(SELECT x,y,z FROM T "
                                            + "UNION ALL "
                                            + "SELECT 1,2,3"
                                            + ")COUNT_"));
    }

    [Test]
    public void SetConstant() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select * from T");
      sql.SetConstant();
      Assert.That(sql.ToString(), Is.EqualTo("SELECT 1 FROM T"));

      sql = new SqlBuilder(@"select x,y,z from T");
      sql.SetConstant();
      Assert.That(sql.ToString(), Is.EqualTo("SELECT 1 FROM T"));

      sql = new SqlBuilder(@"(select x,y,z from T)");
      sql.SetConstant();
      Assert.That(sql.ToString(), Is.EqualTo("(SELECT 1 FROM T)"));

      sql = new SqlBuilder(@"select x,y,z from T "
                          + "union all "
                          + "select 1,2,3");
      sql.SetConstant();
      Assert.That(sql.ToString(), Is.EqualTo("SELECT 1 FROM T "
                                            + "UNION ALL "
                                            + "SELECT 1"));
    }

    [Test]
    public void WrapInSelectStar() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select * from T");
      sql.WrapInSelectStar(_tableColumns);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM (SELECT * FROM T)V0_"));

      sql = new SqlBuilder(@"select x,y,z from T");
      sql.WrapInSelectStar(_tableColumns);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM (SELECT x,y,z FROM T)V0_"));


      sql = new SqlBuilder(@"select x,y,z from T order by x,y,z");
      sql.WrapInSelectStar(_tableColumns);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM (SELECT x,y,z FROM T)V0_ "
                                           + "ORDER BY x,y,z"));

      sql = new SqlBuilder(@"select x as a,y as b,z as c from T order by x,y,z");
      sql.WrapInSelectStar(_tableColumns);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM (SELECT x AS a,y AS b,z AS c FROM T)V0_ "
                                           + "ORDER BY a,b,c"));

      sql = new SqlBuilder(@"select x as a,y as b,z as c from T order by a,b,c");
      sql.WrapInSelectStar(_tableColumns);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM (SELECT x AS a,y AS b,z AS c FROM T)V0_ "
                                           + "ORDER BY a,b,c"));

      sql = new SqlBuilder(@"select x a, y b, z c from T "
                          + "union all "
                          + "select 1, 2, 3 "
                          + "order by x, y, z");
      sql.WrapInSelectStar(_tableColumns);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM ("
                                            + "SELECT x a,y b,z c FROM T "
                                            + "UNION ALL "
                                            + "SELECT 1,2,3"
                                            + ")V0_ "
                                            + "ORDER BY a,b,c"));

      sql = new SqlBuilder(@"select 1 from T order by x,y,z");
      sql.WrapInSelectStar(_tableColumns);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM (SELECT 1,x,y,z FROM T)V0_ "
                                           + "ORDER BY x,y,z"));

      sql = new SqlBuilder(@"select 1 as x from T order by x,y,z");
      sql.WrapInSelectStar(_tableColumns);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM (SELECT 1 AS x,y,z FROM T)V0_ "
                                           + "ORDER BY x,y,z"));
    }

    [Test]
    public void RenameTableAliasName() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select T.* from T where T.id = 1");
      sql.RenameTableAliasName("T", "t1");
      Assert.That(sql.ToString(), Is.EqualTo("SELECT t1.* FROM T t1 WHERE t1.id=1"));
    }

    /// <summary>
    /// SELECT文にEXISTS句を付加する検証
    /// </summary>
    [Test]
    public void ConvertToExistsExpr1() {
      SqlBuilder mainSql;
      SqlBuilder subSql;
      SqlPredicate p;
      SqlTable t;

      mainSql = new SqlBuilder("select y from T left join U on T.x=U.x");
      subSql = new SqlBuilder("select y from U");
      t = new SqlTable(new Table("U"));
      p = subSql.ConvertToExistsExpr2(mainSql, t, _tableColumnDetails["U"]);
      Assert.That(mainSql.ToString(), Is.EqualTo("SELECT y FROM T LEFT JOIN U ON T.x=U.x"));
      Assert.That(p.ToString(), Is.EqualTo("EXISTS(SELECT 1 FROM U U_ WHERE U_.x=U.x)"));

      mainSql = new SqlBuilder("select * from (select y from T left join U on T.x=U.x) V0_");
      subSql = new SqlBuilder("select y from U");
      t = new SqlTable(new Table("U"));
      p = subSql.ConvertToExistsExpr2(mainSql, t, _tableColumnDetails["U"]);
      Assert.That(mainSql.ToString(), Is.EqualTo("SELECT * FROM (SELECT y,U.x AS U_x_ FROM T LEFT JOIN U ON T.x=U.x)V0_"));
      Assert.That(p.ToString(), Is.EqualTo("EXISTS(SELECT 1 FROM U U_ WHERE U_.x=V0_.U_x_)"));

      mainSql = new SqlBuilder("select * from (select y from U union all select y from T) V0_");
      subSql = new SqlBuilder("select y from U");
      t = new SqlTable(new Table("U"));
      p = subSql.ConvertToExistsExpr2(mainSql, t, _tableColumnDetails["U"]);
      Assert.That(mainSql.ToString(), Is.EqualTo("SELECT * FROM (SELECT y,U.x AS U_x_ FROM U " +
                                                 "UNION ALL SELECT y,NULL AS U_x_ FROM T)V0_"));
      Assert.That(p.ToString(), Is.EqualTo("EXISTS(SELECT 1 FROM U U_ WHERE U_.x=V0_.U_x_)"));

      mainSql = new SqlBuilder("select y from U union all select y from T");
      subSql = new SqlBuilder("select y from U");
      t = new SqlTable(new Table("U"));
      p = subSql.ConvertToExistsExpr2(mainSql, t, _tableColumnDetails["U"]);
      Assert.That(mainSql.ToString(), Is.EqualTo("SELECT * FROM (SELECT y,U.x AS U_x_ FROM U " +
                                                 "UNION ALL SELECT y,NULL AS U_x_ FROM T)V0_"));
      Assert.That(p.ToString(), Is.EqualTo("EXISTS(SELECT 1 FROM U U_ WHERE U_.x=V0_.U_x_)"));

      mainSql = new SqlBuilder("select y from T left join U on T.x=U.x");
      subSql = new SqlBuilder("select y from T left join U on T.x=U.x");
      t = new SqlTable(new Table("U"));
      p = subSql.ConvertToExistsExpr2(mainSql, t, _tableColumnDetails["U"]);
      Assert.That(mainSql.ToString(), Is.EqualTo("SELECT y FROM T LEFT JOIN U ON T.x=U.x"));
      Assert.That(p.ToString(), Is.EqualTo("EXISTS(SELECT 1 FROM T LEFT JOIN U U_ ON T.x=U_.x WHERE U_.x=U.x)"));

      mainSql = new SqlBuilder("select * from (select y from T left join U on T.x=U.x) V0_");
      subSql = new SqlBuilder("select * from (select y from T left join U on T.x=U.x) V0_");
      t = new SqlTable(new Table("U"));
      p = subSql.ConvertToExistsExpr2(mainSql, t, _tableColumnDetails["U"]);
      Assert.That(mainSql.ToString(), Is.EqualTo("SELECT * FROM (SELECT y,U.x AS U_x_ FROM T LEFT JOIN U ON T.x=U.x)V0_"));
      Assert.That(p.ToString(), Is.EqualTo("EXISTS(SELECT 1 FROM (SELECT y,U_.x AS U__x_ FROM T LEFT JOIN U U_ ON T.x=U_.x)V0_ " +
                                           "WHERE V0_.U__x_=V0_.U_x_)"));

      mainSql = new SqlBuilder("select y from U union all select y from T");
      subSql = new SqlBuilder("select y from U union all select y from T");
      t = new SqlTable(new Table("U"));
      p = subSql.ConvertToExistsExpr2(mainSql, t, _tableColumnDetails["U"]);
      Assert.That(mainSql.ToString(), Is.EqualTo("SELECT * FROM (SELECT y,U.x AS U_x_ FROM U " +
                                                 "UNION ALL SELECT y,NULL AS U_x_ FROM T)V0_"));
      Assert.That(p.ToString(), Is.EqualTo("EXISTS(SELECT * FROM (SELECT y,U_.x AS U__x_ FROM U U_ " +
                                           "UNION ALL SELECT y,NULL AS U__x_ FROM T)V0_ WHERE V0_.U__x_=V0_.U_x_)"));
    }

    /// <summary>
    /// UPDATE文にEXISTS句を付加する検証
    /// </summary>
    [Test]
    public void ConvertToExistsExpr2() {
      SqlBuilder mainSql;
      SqlBuilder subSql;
      SqlPredicate p;
      SqlTable t;

      mainSql = new SqlBuilder("update U set U.y=1");

      subSql = new SqlBuilder(@"select * from U");
      t = new SqlTable(new Table("U"));
      p = subSql.ConvertToExistsExpr2(mainSql, t, _tableColumnDetails["U"]);
      Assert.That(p.ToString(), Is.EqualTo("EXISTS(SELECT 1 FROM U U_ WHERE U_.x=U.x)"));

      subSql = new SqlBuilder(@"select * from U A");
      t = new SqlTable(new Table("U", false, "A"));
      p = subSql.ConvertToExistsExpr2(mainSql, t, _tableColumnDetails["U"]);
      Assert.That(p.ToString(), Is.EqualTo("EXISTS(SELECT 1 FROM U A_ WHERE A_.x=A.x)"));

      subSql = new SqlBuilder(@"select * from (select * from U A) V");
      t = new SqlTable(new Table("U", false, "A"));
      p = subSql.ConvertToExistsExpr2(mainSql, t, _tableColumnDetails["U"]);
      Assert.That(p.ToString(), Is.EqualTo("EXISTS(SELECT 1 FROM (SELECT * FROM U A_)V WHERE V.x=A.x)"));

      subSql = new SqlBuilder(@"select count(*) from U A");
      t = new SqlTable(new Table("U", false, "A"));
      p = subSql.ConvertToExistsExpr2(mainSql, t, _tableColumnDetails["U"]);
      Assert.That(p.ToString(), Is.EqualTo("EXISTS(SELECT 1 FROM U A_ WHERE A_.x=A.x)"));

      subSql = new SqlBuilder(@"select * from U A join U B on 1=1");
      t = new SqlTable(new Table("U", false, "B"));
      p = subSql.ConvertToExistsExpr2(mainSql, t, _tableColumnDetails["U"]);
      Assert.That(p.ToString(), Is.EqualTo("EXISTS(SELECT 1 FROM U A JOIN U B_ ON 1=1 WHERE B_.x=B.x)"));

      subSql = new SqlBuilder(@"select x,y,z from T order by x,y,z");
      t = new SqlTable(new Table("T"));
      p = subSql.ConvertToExistsExpr2(mainSql, t, _tableColumnDetails["T"]);
      Assert.That(p.ToString(), Is.EqualTo("EXISTS(SELECT 1 FROM T T_ " +
                                           "WHERE T_.x=T.x AND T_.y=T.y AND T_.z=T.z)"));

      subSql = new SqlBuilder(@"select x from T union all select x from U order by a");
      t = new SqlTable(new Table("U"));
      p = subSql.ConvertToExistsExpr2(mainSql, t, _tableColumnDetails["U"]);
      Assert.That(p.ToString(), Is.EqualTo("EXISTS(SELECT * FROM (SELECT x,NULL AS U__x_ FROM T " +
                                           "UNION ALL " +
                                           "SELECT x,x AS U__x_ FROM U U_)V0_ WHERE V0_.U__x_=U.x)"));
    }

    [Test]
    public void AddAndPredicate() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select * from T");
      sql.AddAndPredicate("");
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T"));

      sql = new SqlBuilder(@"select * from T");
      sql.AddAndPredicate("x between 1 and 2");
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T "
                                           + "WHERE x BETWEEN 1 AND 2"));

      sql = new SqlBuilder(@"select * from T where a=1 or b=2");
      sql.AddAndPredicate("x between 1 and 2 or x is not null");
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T "
                                           + "WHERE (a=1 OR b=2) AND "
                                           + "(x BETWEEN 1 AND 2 OR x IS NOT NULL)"));

      sql = new SqlBuilder(@"select * from T where (a=1 or b=2)");
      sql.AddAndPredicate("(x between 1 and 2 or x is not null)");
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T "
                                           + "WHERE (a=1 OR b=2) AND "
                                           + "(x BETWEEN 1 AND 2 OR x IS NOT NULL)"));

      sql = new SqlBuilder(@"(select * from T)");
      sql.AddAndPredicate("exists (select * from U)");
      Assert.That(sql.ToString(), Is.EqualTo("(SELECT * FROM T WHERE EXISTS(SELECT * FROM U))"));

      sql = new SqlBuilder(@"select x,y,z from T "
                          + "union all "
                          + "select 1,2,3");
      sql.AddAndPredicate("exists (select * from U)");
      Assert.That(sql.ToString(), Is.EqualTo("SELECT x,y,z FROM T "
                                            + "UNION ALL "
                                            + "SELECT 1,2,3"));

      sql = new SqlBuilder(@"update T set x=(select y from T where 1=1)");
      sql.AddAndPredicate("y=2");
      Assert.That(sql.ToString(), Is.EqualTo("UPDATE T SET x=(SELECT y FROM T WHERE 1=1) WHERE y=2"));

      sql = new SqlBuilder(@"delete from T");
      sql.AddAndPredicate("z=3");
      Assert.That(sql.ToString(), Is.EqualTo("DELETE FROM T WHERE z=3"));

    }

    [Test]
    public void AddAndPredicate2() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select * from T");
      var sqlPredicate = new SqlPredicate("x like 'abc%'");
      sql.AddAndPredicate(sqlPredicate);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T "
                                           + "WHERE x LIKE 'abc%'"));

      sql = new SqlBuilder(@"Delete from T");
      sql.AddAndPredicate(new SqlPredicate());
      Assert.That(sql.ToString(), Is.EqualTo("DELETE FROM T"));
    }

    [Test]
    public void AddOrderBy() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select * from T");
      sql.AddOrderBy(new List<Tuple<string,bool>>{Tuple.Create("a", true)});
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T "
                                           + "ORDER BY a DESC"));

      sql = new SqlBuilder(@"select * from T");
      sql.AddOrderBy(new List<Tuple<string, bool>> { Tuple.Create("a", true)
                                                    ,Tuple.Create("b", false)
                                                    ,Tuple.Create("1+2*3", true)});
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T "
                                           + "ORDER BY a DESC,b,1+2*3 DESC"));

      sql = new SqlBuilder(@"select * from T order by x");
      sql.AddOrderBy(new List<Tuple<string, bool>> { Tuple.Create("a", true) });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T "
                                           + "ORDER BY x,a DESC"));

      sql = new SqlBuilder(@"update T set x=(select y from T where 1=1) where 2=2");
      sql.AddOrderBy(new List<Tuple<string, bool>> { Tuple.Create("a", true) });
      Assert.That(sql.ToString(), Is.EqualTo("UPDATE T SET x=(SELECT y FROM T WHERE 1=1) WHERE 2=2"));

      sql = new SqlBuilder(@"delete from T where 1=1");
      sql.AddOrderBy(new List<Tuple<string, bool>> { Tuple.Create("a", true) });
      Assert.That(sql.ToString(), Is.EqualTo("DELETE FROM T WHERE 1=1"));      
    }

    [Test]
    public void ClearWhere() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select * from T order by x");
      sql.ClearWhere();
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T ORDER BY x"));
        
      sql = new SqlBuilder(@"select * from (select * from U where 1=1) V where 1=1");
      sql.ClearWhere();
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM (SELECT * FROM U WHERE 1=1)V"));

      sql = new SqlBuilder(@"select (select y from U where 1=1 and 2=2) from T where 3=3 and 4=4");
      sql.ClearWhere();
      Assert.That(sql.ToString(), Is.EqualTo("SELECT (SELECT y FROM U WHERE 1=1 AND 2=2) FROM T"));

      sql = new SqlBuilder(@"update T set x=(select y from T where 1=1) where 2=2");
      sql.ClearWhere();
      Assert.That(sql.ToString(), Is.EqualTo("UPDATE T SET x=(SELECT y FROM T WHERE 1=1)"));

      sql = new SqlBuilder(@"delete from T where 1=1");
      sql.ClearWhere();
      Assert.That(sql.ToString(), Is.EqualTo("DELETE FROM T"));
    }

    [Test]
    public void ClearOrderBy() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select * from T order by x");
      sql.ClearOrderBy();
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T"));

      sql = new SqlBuilder(@"select * from (select * from U order by y) V order by x");
      sql.ClearOrderBy();
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM (SELECT * FROM U ORDER BY y)V"));

      sql = new SqlBuilder(@"select (select y from U order by y) from T order by x");
      sql.ClearOrderBy();
      Assert.That(sql.ToString(), Is.EqualTo("SELECT (SELECT y FROM U ORDER BY y) FROM T"));
    }

    [Test]
    public void RaisePrimaryKey() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select * from T");
      sql.RaisePrimaryKey(_tableColumnDetails);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T"));

      sql = new SqlBuilder(@"select 1 from T");
      sql.RaisePrimaryKey(_tableColumnDetails);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT 1,T.x AS T_x_,T.y AS T_y_,T.z AS T_z_ FROM T"));

      sql = new SqlBuilder(@"select 1 from T full outer join U on T.x=U.x");
      sql.RaisePrimaryKey(_tableColumnDetails);
      Assert.That(sql.ToString(), Is.EqualTo("SELECT 1,T.x AS T_x_,T.y AS T_y_,T.z AS T_z_"
                                                   + ",U.x AS U_x_ "
                                           + "FROM T FULL OUTER JOIN U ON T.x=U.x"));

      sql = new SqlBuilder(@"select T.x as ""Date"" from T", SqlBuilder.DbmsType.Oracle);
      var keys = sql.RaisePrimaryKey(_tableColumnDetails);
      var t = new SqlTable(new Table("T"));
      Assert.That(keys[t], Is.EqualTo(new string[] { "Date", "T_y_", "T_z_" }));
      Assert.That(sql.ToString(), Is.EqualTo(@"SELECT T.x AS ""Date"",T.y AS T_y_,T.z AS T_z_ FROM T"));

    }

    [Test]
    public void SetPlaceHolder() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select @PH1 from T where @PH1=1 or @PH2 order by @PH1");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH1", "T.x" }, { "PH2", "T.x=2" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT T.x FROM T WHERE T.x=1 OR T.x=2 ORDER BY T.x"));

      sql = new SqlBuilder(@"select * from T where not @PH");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "T.x = 1 or T.y = 2" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T WHERE NOT (T.x=1 OR T.y=2)"));

      sql = new SqlBuilder(@"update T set x=@PH1 where @PH2");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH1", "func(2)" }, { "PH2", "x is null" } });
      Assert.That(sql.ToString(), Is.EqualTo("UPDATE T SET x=func(2) WHERE x IS NULL"));

      sql = new SqlBuilder(@"insert into T(x,y) values(@PH1, @PH2),(@PH1, @PH2)");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH1", "(aa)" }, { "PH2", "'abc'" } });
      Assert.That(sql.ToString(), Is.EqualTo("INSERT INTO T(x,y) VALUES((aa),'abc'),((aa),'abc')"));

      sql = new SqlBuilder(@"update T set x=@PH1");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH1", "default" } });
      Assert.That(sql.ToString(), Is.EqualTo("UPDATE T SET x=DEFAULT"));

      sql = new SqlBuilder(@"insert into T(x) values(@PH1)");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH1", "default" } });
      Assert.That(sql.ToString(), Is.EqualTo("INSERT INTO T(x) VALUES(DEFAULT)"));

      sql = new SqlBuilder(@"insert into T(x) values(@PH1)");
      Assert.Throws<CannotBuildASTException>(
        () => { sql.SetPlaceHolder(new Dictionary<string, string> { { "PH1", "T.x = 1" } }); });

      sql = new SqlBuilder(@"pragma table_info(@SQLite_TableName_)");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "SQLite_TableName_", "TBL" } });
      Assert.That(sql.ToString(), Is.EqualTo("PRAGMA TABLE_INFO(TBL)"));

      sql = new SqlBuilder(@"pragma table_info(@SQLite_TableName_)");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "SQLite_TableName_", "schema.Tbl" } });
      Assert.That(sql.ToString(), Is.EqualTo("PRAGMA TABLE_INFO(schema.Tbl)"));
    }
      
    [Test]
    public void SetPlaceHolder2() {
      SqlBuilder sql;

      //
      // Predicate演算子の結合の優先順位が適用前後で変わらないことを確認する
      //

      sql = new SqlBuilder(@"select * from T where @PH");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "a=1 and b=2" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T WHERE a=1 AND b=2"));

      sql = new SqlBuilder(@"select * from T where @PH");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "a=1 or b=2" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T WHERE a=1 OR b=2"));

      sql = new SqlBuilder(@"select * from T where @PH");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "not a=1" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T WHERE NOT a=1"));

      sql = new SqlBuilder(@"select * from T where @PH");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "a=1 collate jp" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T WHERE a=1 COLLATE jp"));


      sql = new SqlBuilder(@"select * from T where 1=1 AND @PH");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "a=1 and b=2" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T WHERE 1=1 AND a=1 AND b=2"));

      sql = new SqlBuilder(@"select * from T where 1=1 AND @PH");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "a=1 or b=2" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T WHERE 1=1 AND (a=1 OR b=2)"));

      sql = new SqlBuilder(@"select * from T where 1=1 AND @PH");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "not a=1" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T WHERE 1=1 AND NOT a=1"));

      sql = new SqlBuilder(@"select * from T where 1=1 AND @PH");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "a=1 collate jp" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T WHERE 1=1 AND a=1 COLLATE jp"));


      sql = new SqlBuilder(@"select * from T where 1=1 OR @PH");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "a=1 and b=2" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T WHERE 1=1 OR a=1 AND b=2"));

      sql = new SqlBuilder(@"select * from T where 1=1 OR @PH");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "a=1 or b=2" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T WHERE 1=1 OR a=1 OR b=2"));

      sql = new SqlBuilder(@"select * from T where 1=1 OR @PH");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "not a=1" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T WHERE 1=1 OR NOT a=1"));

      sql = new SqlBuilder(@"select * from T where 1=1 OR @PH");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "a=1 collate jp" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T WHERE 1=1 OR a=1 COLLATE jp"));


      sql = new SqlBuilder(@"select * from T where NOT @PH");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "a=1 and b=2" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T WHERE NOT (a=1 AND b=2)"));

      sql = new SqlBuilder(@"select * from T where NOT @PH");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "a=1 or b=2" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T WHERE NOT (a=1 OR b=2)"));

      sql = new SqlBuilder(@"select * from T where NOT @PH");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "not a=1" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T WHERE NOT NOT a=1"));

      sql = new SqlBuilder(@"select * from T where NOT @PH");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "a=1 collate jp" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T WHERE NOT a=1 COLLATE jp"));


      sql = new SqlBuilder(@"select * from T where @PH collate jp");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "a=1 and b=2" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T WHERE (a=1 AND b=2) COLLATE jp"));

      sql = new SqlBuilder(@"select * from T where @PH collate jp");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "a=1 or b=2" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T WHERE (a=1 OR b=2) COLLATE jp"));

      sql = new SqlBuilder(@"select * from T where @PH collate jp");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "not a=1" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T WHERE NOT a=1 COLLATE jp"));

      sql = new SqlBuilder(@"select * from T where @PH collate jp");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "a=1 collate jp" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT * FROM T WHERE a=1 COLLATE jp COLLATE jp"));
    }


    [Test]
    public void SetPlaceHolder3() {
      SqlBuilder sql;

      //
      // Expr演算子の結合の優先順位が適用前後で変わらないことを確認する
      //

      sql = new SqlBuilder(@"select @PH");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "~b" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT ~b"));

      sql = new SqlBuilder(@"select @PH");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "a+b" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT a+b"));


      sql = new SqlBuilder(@"select ~@PH");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "~b" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT ~~b"));

      sql = new SqlBuilder(@"select ~@PH");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "a+b" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT ~(a+b)"));


      sql = new SqlBuilder(@"select @PH / 7");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "~b" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT ~b/7"));

      sql = new SqlBuilder(@"select @PH / 7");
      sql.SetPlaceHolder(new Dictionary<string, string> { { "PH", "a+b" } });
      Assert.That(sql.ToString(), Is.EqualTo("SELECT (a+b)/7"));
    }

    [Test]
    public void ReplaceAllPlaceholdersToNull() {
      SqlBuilder sql;

      // 全てのプレースホルダがNULLまたは0=1に置き換えられること
      sql = new SqlBuilder(@"select @PH1 FROM T WHERE @PH2 GROUP BY @PH3 HAVING @PH4 ORDER BY @PH5");
      sql.SetAllPlaceHolderToNull();
      Assert.That(sql.ToString(), Is.EqualTo("SELECT NULL FROM T WHERE 0=1 GROUP BY NULL HAVING 0=1 ORDER BY NULL"));
    }

    [Test]
    public void HasWildcard() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select * from T where x=1");
      Assert.That(sql.HasWildcard(), Is.True);
      Assert.That(sql.HasTableWildcard(), Is.False);

      sql = new SqlBuilder(@"select x, y, z from (select * from T) t1 where x=1");
      Assert.That(sql.HasWildcard(), Is.False);
      Assert.That(sql.HasTableWildcard(), Is.False);

      sql = new SqlBuilder(@"select * from T where x=1 " +
                            "union all " +
                            "select x,y,z from T");
      Assert.That(sql.HasWildcard(), Is.True);
      Assert.That(sql.HasTableWildcard(), Is.False);

      sql = new SqlBuilder(@"select x,y,z from T where x=1 " +
                            "union all " +
                            "select * from T");
      Assert.That(sql.HasWildcard(), Is.False);
      Assert.That(sql.HasTableWildcard(), Is.False);
    }

    [Test]
    public void HasTableWildcard() {
      SqlBuilder sql;

      sql = new SqlBuilder(@"select T.* from T where x=1");
      Assert.That(sql.HasWildcard(), Is.False);
      Assert.That(sql.HasTableWildcard(), Is.True);

      sql = new SqlBuilder(@"select x, y, z from (select T.* from T) t1 where x=1");
      Assert.That(sql.HasWildcard(), Is.False);
      Assert.That(sql.HasTableWildcard(), Is.False);

      sql = new SqlBuilder(@"select T.*,x,y,z from T where x=1");
      Assert.That(sql.HasWildcard(), Is.False);
      Assert.That(sql.HasTableWildcard(), Is.True);

      sql = new SqlBuilder(@"select x,y,z,T.* from T where x=1");
      Assert.That(sql.HasWildcard(), Is.False);
      Assert.That(sql.HasTableWildcard(), Is.True);

      sql = new SqlBuilder(@"select T.* from T where x=1 " +
                            "union all " +
                            "select x,y,z from T");
      Assert.That(sql.HasWildcard(), Is.False);
      Assert.That(sql.HasTableWildcard(), Is.True);

      sql = new SqlBuilder(@"select x,y,z from T where x=1 " +
                            "union all " +
                            "select T.* from T");
      Assert.That(sql.HasWildcard(), Is.False);
      Assert.That(sql.HasTableWildcard(), Is.False);
    }

    private T Get1stItem<T>(IEnumerable<T> items) {
      foreach(var item in items) {
        return item;
      }
      return default(T);
    }

    private Dictionary<string, IEnumerable<Tuple<string, bool>>> _tableColumnDetails;
    private Dictionary<string, IEnumerable<string>> _tableColumns;

    [SetUp]
    public void initTest() {
      // テスト用テーブル列
      _tableColumns = new Dictionary<string, IEnumerable<string>>();
      _tableColumns.Add("T", new string[] { "x", "y", "z" });
      _tableColumns.Add("U", new string[] { "x", "y", "z" });
      _tableColumns.Add("V", new string[] { "x1", "x2", "x3" });

      _tableColumnDetails = new Dictionary<string, IEnumerable<Tuple<string, bool>>>();
      _tableColumnDetails.Add("T", new Tuple<string, bool>[] { Tuple.Create("x", true),
                                                               Tuple.Create("y", true),
                                                               Tuple.Create("z", true) });
      _tableColumnDetails.Add("U", new Tuple<string, bool>[] { Tuple.Create("x", true),
                                                               Tuple.Create("y", false),
                                                               Tuple.Create("z", false) });
      _tableColumnDetails.Add("V", new Tuple<string, bool>[] { Tuple.Create("x1", true),
                                                               Tuple.Create("x2", true),
                                                               Tuple.Create("x3", true) });
    }
  }
}
