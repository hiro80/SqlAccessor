using System.Collections.Generic;
using MiniSqlParser;
using NUnit.Framework;

namespace SqlBuilderTester
{
  [TestFixture]
  public class ResultInfoASTTester
  {

    [Test]
    public void SimpleQuery() {
      Result result;

      result = this.GetResultInfoList(@"select x, y, z from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT x,y,z FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull tableKey [T.x notNull tableKey]]"
                                         + "[.y notNull [T.y notNull]][.z [T.z]]"));

      result = this.GetResultInfoList(@"select x, y, z from T t1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT x,y,z FROM T t1"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull tableKey [t1.x notNull tableKey]]"
                                         + "[.y notNull [t1.y notNull]][.z [t1.z]]"));

      result = this.GetResultInfoList(@"select t1.x, t1.y, t1.z from T t1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT t1.x,t1.y,t1.z FROM T t1"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull tableKey [t1.x notNull tableKey]]"
                                         + "[.y notNull [t1.y notNull]][.z [t1.z]]"));


      result = this.GetResultInfoList(@"select x from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT x FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull tableKey [T.x notNull tableKey]]"
                                         + "(.y notNull [T.y notNull])(.z [T.z])"));

      result = this.GetResultInfoList(@"select x from T t1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT x FROM T t1"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull tableKey [t1.x notNull tableKey]]"
                                         + "(.y notNull [t1.y notNull])(.z [t1.z])"));

      result = this.GetResultInfoList(@"select t1.x from T t1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT t1.x FROM T t1"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull tableKey [t1.x notNull tableKey]]"
                                         + "(.y notNull [t1.y notNull])(.z [t1.z])"));


      result = this.GetResultInfoList(@"select z from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT z,T.x AS T_x_ FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[.z [T.z]]"
                                         + "[.T_x_ notNull tableKey complemented [T.x notNull tableKey]]"
                                         + "(.y notNull [T.y notNull])"));

      result = this.GetResultInfoList(@"select z from T t1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT z,t1.x AS t1_x_ FROM T t1"));
      Assert.That(result.Info, Is.EqualTo(@"[.z [t1.z]]"
                                         + "[.t1_x_ notNull tableKey complemented [t1.x notNull tableKey]]"
                                         + "(.y notNull [t1.y notNull])"));

      result = this.GetResultInfoList(@"select t1.z from T t1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT t1.z,t1.x AS t1_x_ FROM T t1"));
      Assert.That(result.Info, Is.EqualTo(@"[.z [t1.z]]"
                                         + "[.t1_x_ notNull tableKey complemented [t1.x notNull tableKey]]"
                                         + "(.y notNull [t1.y notNull])"));
    }

    [Test]
    public void WithoutFrom() {
      Result result;

      result = this.GetResultInfoList(@"select 1, null");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 1,NULL"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull ][. ]"));

      result = this.GetResultInfoList(@"select (select 'a'), (select null)");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT (SELECT 'a'),(SELECT NULL)"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull [. notNull ]][. [. ]]"));

      result = this.GetResultInfoList(@"select sum(x)");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT sum(x)"));
      Assert.That(result.Info, Is.EqualTo(@"[. ]"));

      result = this.GetResultInfoList(@"select count(x)");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT count(x)"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull ]"));

      result = this.GetResultInfoList(@"select distinct 'abc', 1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT DISTINCT 'abc',1"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull groupKey ][. notNull groupKey ]"));

      Assert.Throws<CannotBuildASTException>(() => this.GetResultInfoList(@"select (select *)"));
    }

    [Test]
    public void Wildcard() {
      Result result;

      result = this.GetResultInfoList(@"select * from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT * FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull tableKey [T.x notNull tableKey]]"
                                         + "[.y notNull [T.y notNull]][.z [T.z]]"));

      result = this.GetResultInfoList(@"select * from T t1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT * FROM T t1"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull tableKey [t1.x notNull tableKey]]"
                                         + "[.y notNull [t1.y notNull]][.z [t1.z]]"));


      result = this.GetResultInfoList(@"select T.* from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT T.* FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull tableKey [T.x notNull tableKey]]"
                                         + "[.y notNull [T.y notNull]][.z [T.z]]"));

      result = this.GetResultInfoList(@"select t1.* from T t1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT t1.* FROM T t1"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull tableKey [t1.x notNull tableKey]]"
                                         + "[.y notNull [t1.y notNull]][.z [t1.z]]"));


      result = this.GetResultInfoList(@"select T.*, T.* from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT T.*,T.* FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull tableKey [T.x notNull tableKey]]"
                                         + "[.y notNull [T.y notNull]][.z [T.z]]"
                                         + "[.x notNull tableKey [T.x notNull tableKey]]"
                                         + "[.y notNull [T.y notNull]][.z [T.z]]"));

      result = this.GetResultInfoList(@"select t1.*,t1.* from T t1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT t1.*,t1.* FROM T t1"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull tableKey [t1.x notNull tableKey]]"
                                         + "[.y notNull [t1.y notNull]][.z [t1.z]]"
                                         + "[.x notNull tableKey [t1.x notNull tableKey]]"
                                         + "[.y notNull [t1.y notNull]][.z [t1.z]]"));
    }

    [Test]
    public void Literal() {
      Result result;

      result = this.GetResultInfoList(@"select 1 as one, 2, 'abc', null from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 1 AS one,2,'abc',NULL,T.x AS T_x_ FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[.one notNull ][. notNull ][. notNull ][. ]"
                                         + "[.T_x_ notNull tableKey complemented [T.x notNull tableKey]]"
                                         + "(.y notNull [T.y notNull])(.z [T.z])"));

      result = this.GetResultInfoList(@"select (2 + 3) * 4, 'aa' || 'bb' from V");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT (2+3)*4,'aa'||'bb',V.x1 AS V_x1_"
                                         + ",V.x2 AS V_x2_,V.x3 AS V_x3_ FROM V"));
      Assert.That(result.Info, Is.EqualTo(@"[. ][. ][.V_x1_ notNull tableKey complemented "
                                         + "[V.x1 notNull tableKey]][.V_x2_ notNull tableKey complemented "
                                         + "[V.x2 notNull tableKey]][.V_x3_ notNull tableKey complemented "
                                         + "[V.x3 notNull tableKey]]"));

      // 空文字等のNULL表現値はNULLではないとする
      // そのため、SqlAccessorのテーブル行の有無判定は、Casterによる値の変換前に行う必要があるだろう
      var nullInt = int.MinValue;
      var nullLong = long.MinValue;
      result = this.GetResultInfoList(@"select ''," + nullInt + "," + nullLong + ",x from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT ''," + nullInt + "," + nullLong + ",x FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull ][. notNull ][. notNull ]"
                                         + "[.x notNull tableKey [T.x notNull tableKey]]"
                                         + "(.y notNull [T.y notNull])(.z [T.z])"));
    }


    [Test]
    public void Distinct() {
      Result result;

      result = this.GetResultInfoList(@"select distinct x, y, z from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT DISTINCT x,y,z FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull groupKey [T.x notNull tableKey]]"
                                         + "[.y notNull groupKey [T.y notNull]][.z groupKey [T.z]]"));

      result = this.GetResultInfoList(@"select distinct y, z from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT DISTINCT y,z FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[.y notNull groupKey [T.y notNull]]"
                                         + "[.z groupKey [T.z]]"));

      result = this.GetResultInfoList(@"select distinct y from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT DISTINCT y FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[.y notNull groupKey [T.y notNull]]"));

      result = this.GetResultInfoList(@"select distinct sum(y) from T group by y");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT DISTINCT sum(y),1 AS RESULT_EXISTS_ FROM T GROUP BY y"));
      Assert.That(result.Info, Is.EqualTo(@"[. groupKey ][.RESULT_EXISTS_ notNull countKey complemented ]"));
    }

    [Test]
    public void AggregateFunc() {
      Result result;

      result = this.GetResultInfoList(@"select sum(x), y, z from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT sum(x),y,z,COUNT(T.x) AS RESULT_COUNT_ FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[. ][.y [T.y notNull]]"
                                         + "[.z [T.z]][.RESULT_COUNT_ notNull countKey complemented "
                                         + "[T.x notNull tableKey]]"));

      result = this.GetResultInfoList(@"select sum(y), sum(z) from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT sum(y),sum(z),COUNT(T.x) AS RESULT_COUNT_ FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[. ][. ]"
                                         + "[.RESULT_COUNT_ notNull countKey complemented [T.x notNull tableKey]]"));

      result = this.GetResultInfoList(@"select sum(y) from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT sum(y),COUNT(T.x) AS RESULT_COUNT_ FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[. ][.RESULT_COUNT_ notNull countKey complemented "
                                         + "[T.x notNull tableKey]]"));

      result = this.GetResultInfoList(@"select count(x), y, z from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT count(x),y,z FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull countKey [T.x notNull tableKey]]"
                                         + "[.y [T.y notNull]][.z [T.z]]"));

      result = this.GetResultInfoList(@"select count(y), count(z) from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT count(y),count(z) FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull countKey [T.y notNull]]"
                                         + "[. notNull [T.z]]"));

      result = this.GetResultInfoList(@"select count(y) from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT count(y) FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull countKey [T.y notNull]]"));

      result = this.GetResultInfoList(@"select count(1), count(T.x + 1) from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT count(1),count(T.x+1)"
                                         + ",COUNT(T.x) AS RESULT_COUNT_ FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull ][. notNull ]"
                                         + "[.RESULT_COUNT_ notNull countKey complemented [T.x notNull tableKey]]"));

      result = this.GetResultInfoList(@"select case when count(*) > 0 then 1 else 2 end from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT CASE WHEN count(*)>0 THEN 1 ELSE 2 END"
                                        + ",COUNT(T.x) AS RESULT_COUNT_ FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[. ][.RESULT_COUNT_ notNull countKey complemented [T.x notNull tableKey]]"));

      result = this.GetResultInfoList(@"select (select count(*) from T) from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT (SELECT count(*) FROM T),T.x AS T_x_ FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull [. notNull ]]"
                                         + "[.T_x_ notNull tableKey complemented [T.x notNull tableKey]]"
                                         + "(.y notNull [T.y notNull])"
                                         + "(.z [T.z])"));

      result = this.GetResultInfoList(@"select 1 from "
                                     + " (select count(v0.x) from "
                                     + "   (select U.x from T left join U on 1=1) v0 ) v2");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 1,v2.v2_col0_ AS v2_v2_col0__"
                                        + ",v2.RESULT_COUNT_ AS RESULT_COUNT_"
                                        + " FROM (SELECT count(v0.x) AS v2_col0_"
                                        + ",COUNT(v0.T_x_) AS RESULT_COUNT_"
                                        + " FROM (SELECT U.x,T.x AS T_x_"
                                        + " FROM T LEFT JOIN U ON 1=1)v0)v2"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull ]"
                                        + "[.v2_v2_col0__ notNull countKey complemented "
                                        + "[v2.v2_col0_ notNull countKey [v0.x tableKey [U.x notNull tableKey]]]]"
                                        + "[.RESULT_COUNT_ notNull countKey complemented "
                                        + "[v2.RESULT_COUNT_ notNull countKey complemented "
                                        + "[v0.T_x_ notNull tableKey complemented [T.x notNull tableKey]]]]"));

      result = this.GetResultInfoList(@"select case case"
                                      + " case when exists (select count(x) from T where id=0 and id2=0)"
                                      + " then case (select count(x) from T where a=1 and b=2) "
                                      + " when 1 then 1 else null end when a=(case when 1=1 then 1"
                                      + " else null end) then 2 end when 1 then 0 when 2 then 1"
                                      + " end when 3 then 2 else 3 end from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT CASE CASE CASE WHEN EXISTS(SELECT count(x) FROM T "
                                         + "WHERE id=0 AND id2=0) THEN CASE (SELECT count(x) FROM T "
                                         + "WHERE a=1 AND b=2) WHEN 1 THEN 1 ELSE NULL END "
                                         + "WHEN a=(CASE WHEN 1=1 THEN 1 ELSE NULL END) THEN 2 " 
                                         + "END WHEN 1 THEN 0 WHEN 2 THEN 1 END WHEN 3 THEN 2 "
                                         + "ELSE 3 END,T.x AS T_x_ FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[. ][.T_x_ notNull tableKey complemented "
                                         + "[T.x notNull tableKey]]"
                                         + "(.y notNull [T.y notNull])(.z [T.z])"));
    }

    [Test]
    public void GroupBy() {
      Result result;

      result = this.GetResultInfoList(@"select * from T group by x, y, z");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT * FROM T GROUP BY x,y,z"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull groupKey [T.x notNull tableKey]]"
                                         + "[.y notNull groupKey [T.y notNull]]"
                                         + "[.z groupKey [T.z]]"));

      result = this.GetResultInfoList(@"select T.* from T group by x, y, z");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT T.* FROM T GROUP BY x,y,z"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull groupKey [T.x notNull tableKey]]"
                                         + "[.y notNull groupKey [T.y notNull]]"
                                         + "[.z groupKey [T.z]]"));

      result = this.GetResultInfoList(@"select x, y, z from T group by x, y, z");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT x,y,z FROM T GROUP BY x,y,z"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull groupKey [T.x notNull tableKey]]"
                                         + "[.y notNull groupKey [T.y notNull]]"
                                         + "[.z groupKey [T.z]]"));

      result = this.GetResultInfoList(@"select y, z from T group by z, y");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT y,z FROM T GROUP BY z,y"));
      Assert.That(result.Info, Is.EqualTo(@"[.y notNull groupKey [T.y notNull]]"
                                         + "[.z groupKey [T.z]]"));

      result = this.GetResultInfoList(@"select y from T group by 1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT y FROM T GROUP BY 1"));
      Assert.That(result.Info, Is.EqualTo(@"[.y notNull groupKey [T.y notNull]]"));

      result = this.GetResultInfoList(@"select 1 from V group by x1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 1,V.x1 AS V_x1_ FROM V GROUP BY x1"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull ][.V_x1_ notNull groupKey complemented "
                                         + "[V.x1 notNull tableKey]]"));

      result = this.GetResultInfoList(@"select 1 from V group by x2");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 1,V.x2 AS V_x2_ FROM V GROUP BY x2"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull ][.V_x2_ notNull groupKey complemented "
                                         + "[V.x2 notNull tableKey]]"));

      result = this.GetResultInfoList(@"select 1 from V group by x1, x2");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 1,V.x1 AS V_x1_,V.x2 AS V_x2_ FROM V GROUP BY x1,x2"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull ]"
                                         + "[.V_x1_ notNull groupKey complemented [V.x1 notNull tableKey]]"
                                         + "[.V_x2_ notNull groupKey complemented [V.x2 notNull tableKey]]"));

      result = this.GetResultInfoList(@"select x1 from V group by x3, x2");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT x1,V.x2 AS V_x2_,V.x3 AS V_x3_ FROM V GROUP BY x3,x2"));
      Assert.That(result.Info, Is.EqualTo(@"[.x1 notNull [V.x1 notNull tableKey]]"
                                         + "[.V_x2_ notNull groupKey complemented [V.x2 notNull tableKey]]"
                                         + "[.V_x3_ notNull groupKey complemented [V.x3 notNull tableKey]]"));

      result = this.GetResultInfoList(@"select 1 from (select 1 from T group by x) v");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 1,v.T_x_ AS T_x_ FROM "
                                        + "(SELECT 1,T.x AS T_x_ FROM T GROUP BY x)v"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull ][.T_x_ notNull groupKey complemented "
                                        + "[v.T_x_ notNull groupKey complemented [T.x notNull tableKey]]]"
                                        + "(. notNull [v. notNull ])"));

      result = this.GetResultInfoList(@"select 1 from (select 1 from T group by z) v");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 1,v.T_z_ AS T_z_,v.RESULT_COUNT_ AS RESULT_COUNT_ "
                                        + "FROM (SELECT 1,T.z AS T_z_,COUNT(T.x) AS RESULT_COUNT_ "
                                        + "FROM T GROUP BY z)v"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull ]"
                                        + "[.T_z_ groupKey complemented [v.T_z_ groupKey complemented [T.z]]]"
                                        + "[.RESULT_COUNT_ notNull countKey complemented "
                                        + "[v.RESULT_COUNT_ notNull countKey complemented [T.x notNull tableKey]]]"
                                        + "(. notNull [v. notNull ])"));

      result = this.GetResultInfoList(@"select 1 from (select 1 from T group by T.x) v");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 1,v.T_x_ AS T_x_ FROM "
                                        + "(SELECT 1,T.x AS T_x_ FROM T GROUP BY T.x)v"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull ]"
                                        + "[.T_x_ notNull groupKey complemented [v.T_x_ notNull groupKey complemented "
                                        + "[T.x notNull tableKey]]](. notNull [v. notNull ])"));

      result = this.GetResultInfoList(@"select 1 from (select 1 from T group by T.z) v");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 1,v.T_z_ AS T_z_,v.RESULT_COUNT_ AS RESULT_COUNT_ "
                                        + "FROM (SELECT 1,T.z AS T_z_,COUNT(T.x) AS RESULT_COUNT_ "
                                        + "FROM T GROUP BY T.z)v"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull ]"
                                        + "[.T_z_ groupKey complemented [v.T_z_ groupKey complemented [T.z]]]"
                                        + "[.RESULT_COUNT_ notNull countKey complemented "
                                        + "[v.RESULT_COUNT_ notNull countKey complemented [T.x notNull tableKey]]]"
                                        + "(. notNull [v. notNull ])"));

      result = this.GetResultInfoList(@"select 1 from (select T.x from T group by x) v");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 1,v.x AS v_x_ FROM "
                                        + "(SELECT T.x FROM T GROUP BY x)v"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull ][.v_x_ notNull groupKey complemented "
                                        + "[v.x notNull groupKey [T.x notNull tableKey]]]"));

      result = this.GetResultInfoList(@"select 1 from (select T.z from T group by z) v");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 1,v.z AS v_z_,v.RESULT_COUNT_ AS RESULT_COUNT_ "
                                        + "FROM (SELECT T.z,COUNT(T.x) AS RESULT_COUNT_ FROM T GROUP BY z)v"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull ][.v_z_ groupKey complemented [v.z groupKey [T.z]]]"
                                         + "[.RESULT_COUNT_ notNull countKey complemented "
                                         + "[v.RESULT_COUNT_ notNull countKey complemented [T.x notNull tableKey]]]"));

      result = this.GetResultInfoList(@"select 1 from "
                                      + "(select 1,T.z as T_z_,count(T.x) as RESULT_COUNT_ from T"
                                      + " group by z) v");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 1,v.T_z_ AS v_T_z__,v.RESULT_COUNT_ AS v_RESULT_COUNT__"
                                         + " FROM (SELECT 1,T.z AS T_z_,count(T.x) AS RESULT_COUNT_ FROM T"
                                         + " GROUP BY z)v"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull ][.v_T_z__ groupKey complemented [v.T_z_ groupKey [T.z]]]"
                                         + "[.v_RESULT_COUNT__ notNull countKey complemented "
                                         + "[v.RESULT_COUNT_ notNull countKey [T.x notNull tableKey]]]"
                                         + "(. notNull [v. notNull ])"));
    }

    [Test]
    public void GroupByAggregateFunc() {
      Result result;

      result = this.GetResultInfoList(@"select sum(y) from T group by y");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT sum(y),T.y AS T_y_ FROM T GROUP BY y"));
      Assert.That(result.Info, Is.EqualTo(@"[. ][.T_y_ notNull groupKey complemented [T.y notNull]]"));

      result = this.GetResultInfoList(@"select distinct y from T group by y");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT DISTINCT y FROM T GROUP BY y"));
      Assert.That(result.Info, Is.EqualTo(@"[.y notNull groupKey [T.y notNull]]"));

      result = this.GetResultInfoList(@"select distinct sum(y) from T group by y");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT DISTINCT sum(y),1 AS RESULT_EXISTS_ FROM T GROUP BY y"));
      Assert.That(result.Info, Is.EqualTo(@"[. groupKey ][.RESULT_EXISTS_ notNull countKey complemented ]"));


      result = this.GetResultInfoList(@"select sum(z) from T group by z");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT sum(z),T.z AS T_z_,COUNT(T.x) AS RESULT_COUNT_ FROM T GROUP BY z"));
      Assert.That(result.Info, Is.EqualTo(@"[. ][.T_z_ groupKey complemented [T.z]]"
                                         + "[.RESULT_COUNT_ notNull countKey complemented [T.x notNull tableKey]]"));

      result = this.GetResultInfoList(@"select distinct z from T group by z");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT DISTINCT z,1 AS RESULT_EXISTS_ FROM T GROUP BY z"));
      Assert.That(result.Info, Is.EqualTo(@"[.z groupKey [T.z]]"
                                         + "[.RESULT_EXISTS_ notNull countKey complemented ]"));

      result = this.GetResultInfoList(@"select distinct sum(z) from T group by z");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT DISTINCT sum(z),1 AS RESULT_EXISTS_ FROM T GROUP BY z"));
      Assert.That(result.Info, Is.EqualTo(@"[. groupKey ][.RESULT_EXISTS_ notNull countKey complemented ]"));

      result = this.GetResultInfoList(@"select count(y) from T group by y");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT count(y),T.y AS T_y_ FROM T GROUP BY y"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull countKey [T.y notNull]]"
                                         + "[.T_y_ notNull groupKey complemented [T.y notNull]]"));

      result = this.GetResultInfoList(@"select distinct count(y) from T group by y");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT DISTINCT count(y) FROM T GROUP BY y"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull groupKey [T.y notNull]]"));


      result = this.GetResultInfoList(@"select count(z) from T group by z");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT count(z),T.z AS T_z_,COUNT(T.x) AS RESULT_COUNT_ FROM T GROUP BY z"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull [T.z]]"
                                         + "[.T_z_ groupKey complemented [T.z]]"
                                         + "[.RESULT_COUNT_ notNull countKey complemented [T.x notNull tableKey]]"));

      result = this.GetResultInfoList(@"select distinct count(z) from T group by z");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT DISTINCT count(z) FROM T GROUP BY z"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull groupKey [T.z]]"));

      result = this.GetResultInfoList(@"select count(x) from T group by count(x)");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT count(x) FROM T GROUP BY count(x)"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull groupKey [T.x notNull tableKey]]"));

      result = this.GetResultInfoList(@"select count(*) over (partition by y order by x ) from t");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT count(*)OVER( PARTITION BY y ORDER BY x)"
                                        + ",T.x AS T_x_ FROM t"));
      Assert.That(result.Info, Is.EqualTo(@"[. ][.T_x_ notNull tableKey complemented [T.x notNull tableKey]]"
                                          + "(.y notNull [T.y notNull])"
                                          + "(.z [T.z])"));
    }

    [Test]
    public void SubQuery1() {
      Result result;

      result = this.GetResultInfoList(@"select (select x from T t0) as s1 from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT (SELECT x FROM T t0) AS s1,T.x AS T_x_ FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[.s1 notNull tableKey [.x notNull tableKey [t0.x notNull tableKey]]]"
                                         + "[.T_x_ notNull tableKey complemented [T.x notNull tableKey]]"
                                         + "(.y notNull [T.y notNull])(.z [T.z])"));

      result = this.GetResultInfoList(@"SELECT (SELECT (SELECT (SELECT x FROM T t0) s3 FROM S t0) s2 FROM U t0)"
                                  + "AS s1 FROM T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT (SELECT (SELECT (SELECT x FROM T t0) s3 FROM S t0) "
                                        + "s2 FROM U t0) AS s1,T.x AS T_x_ FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[.s1 notNull tableKey [.s2 notNull tableKey [.s3 notNull tableKey "
                                         + "[.x notNull tableKey [t0.x notNull tableKey]]]]]"
                                         + "[.T_x_ notNull tableKey complemented [T.x notNull tableKey]]"
                                         + "(.y notNull [T.y notNull])(.z [T.z])"));

      result = this.GetResultInfoList(@"select (select x from T t0) as s1 , (select z from T t0) as s2 from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT (SELECT x FROM T t0) AS s1,(SELECT z FROM T t0) AS s2"
                                         + ",T.x AS T_x_ FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[.s1 notNull tableKey [.x notNull tableKey [t0.x notNull tableKey]]]"
                                         + "[.s2 [.z [t0.z]]][.T_x_ notNull tableKey "
                                         + "complemented [T.x notNull tableKey]](.y notNull [T.y notNull])"
                                         + "(.z [T.z])"));

      result = this.GetResultInfoList(@"select (select y from"
                                  + "(select t1.y from T t1 join T t2 on t1.x = t2.x) v"
                                  + ") from U u1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT (SELECT y FROM (SELECT t1.y FROM T t1 JOIN T t2"
                                  + " ON t1.x=t2.x)v),u1.x AS u1_x_ FROM U u1"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull [.y notNull [v.y notNull [t1.y notNull]]]]"
                                  + "[.u1_x_ notNull tableKey complemented [u1.x notNull tableKey]]"
                                  + "(.y notNull [u1.y notNull])(.z [u1.z])"));

      result = this.GetResultInfoList(@"select (select y from T union all select y from U ) from dual");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT (SELECT y FROM T UNION ALL SELECT y FROM U) FROM dual"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull [.y notNull [.y notNull [T.y notNull]] "
                                         + "[.y notNull [U.y notNull]]]]"));

      result = this.GetResultInfoList(@"select ((select y from T )union all( select y from U )) from dual");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT ((SELECT y FROM T) UNION ALL (SELECT y FROM U)) FROM dual"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull [.y notNull [.y notNull [T.y notNull]] "
                                         + "[.y notNull [U.y notNull]]]]"));
    }

    [Test]
    public void SubQuery2() {
      Result result;

      result = this.GetResultInfoList(@"select case when exists (select y from T) then 1 else 0 end from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT CASE WHEN EXISTS(SELECT y FROM T) THEN 1 ELSE 0 END"
                                         + ",T.x AS T_x_ FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[. ][.T_x_ notNull tableKey complemented [T.x notNull tableKey]]"
                                         + "(.y notNull [T.y notNull])(.z [T.z])"));

      result = this.GetResultInfoList(@"select y from T where y = (select y from T)");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT y,T.x AS T_x_ FROM T WHERE y=(SELECT y FROM T)"));
      Assert.That(result.Info, Is.EqualTo(@"[.y notNull [T.y notNull]][.T_x_ notNull tableKey complemented "
                                         + "[T.x notNull tableKey]](.z [T.z])"));
    }

    [Test]
    public void AmbiguousColumn() {
      Assert.Throws<InvalidASTStructureError>(()=>{
        this.GetResultInfoList(@"select x from T left join U on T.id = U.id");
      });
    }

    [Test]
    public void From() {
      Result result;

      result = this.GetResultInfoList(@"select * from (select x from T t0) v");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT * FROM (SELECT x FROM T t0)v"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull tableKey [v.x notNull tableKey [t0.x notNull tableKey]]]"));

      result = this.GetResultInfoList(@"select null from (select y from T)v");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT NULL,v.T_x_ AS T_x_ FROM "
                                         + "(SELECT y,T.x AS T_x_ FROM T)v"));
      Assert.That(result.Info, Is.EqualTo(@"[. ][.T_x_ notNull tableKey complemented "
                                         + "[v.T_x_ notNull tableKey complemented [T.x notNull tableKey]]]"
                                         + "(.y notNull [v.y notNull [T.y notNull]])"));

      result = this.GetResultInfoList(@"select z from (select z from T)");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT z,T_x_ AS T_x_ FROM (SELECT z,T.x AS T_x_ FROM T)"));
      Assert.That(result.Info, Is.EqualTo(@"[.z [.z [T.z]]][.T_x_ notNull tableKey complemented "
                                         + "[.T_x_ notNull tableKey complemented [T.x notNull tableKey]]]"));

      result = this.GetResultInfoList(@"select a0 from (select b0 as a0 from (select z as b0 from T)v0 )v1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT a0,v1.T_x_ AS T_x_ FROM (SELECT b0 AS a0,"
                                         + "v0.T_x_ AS T_x_ FROM (SELECT z AS b0,T.x AS T_x_ FROM T)v0)v1"));
      Assert.That(result.Info, Is.EqualTo(@"[.a0 [v1.a0 [v0.b0 [T.z]]]]"
                                         + "[.T_x_ notNull tableKey complemented "
                                         + "[v1.T_x_ notNull tableKey complemented "
                                         + "[v0.T_x_ notNull tableKey complemented [T.x notNull tableKey]]]]"));
    }


    [Test]
    public void Join() {
      Result result;

      result = this.GetResultInfoList(@"select null from T cross join U");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT NULL,T.x AS T_x_,U.x AS U_x_"
                                        + " FROM T CROSS JOIN U"));
      Assert.That(result.Info, Is.EqualTo(@"[. ][.T_x_ notNull tableKey complemented [T.x notNull tableKey]]"
                                         + "[.U_x_ notNull tableKey complemented [U.x notNull tableKey]]"
                                         + "(.y notNull [T.y notNull])(.z [T.z])"
                                         + "(.y notNull [U.y notNull])(.z [U.z])"));

      result = this.GetResultInfoList(@"select null from T join U on T.id = U.id");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT NULL,T.x AS T_x_,U.x AS U_x_"
                                        + " FROM T JOIN U ON T.id=U.id"));
      Assert.That(result.Info, Is.EqualTo(@"[. ][.T_x_ notNull tableKey complemented [T.x notNull tableKey]]"
                                         + "[.U_x_ notNull tableKey complemented [U.x notNull tableKey]]"
                                         + "(.y notNull [T.y notNull])(.z [T.z])"
                                         + "(.y notNull [U.y notNull])(.z [U.z])"));

      result = this.GetResultInfoList(@"select null from T left join U on T.id = U.id");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT NULL,T.x AS T_x_,U.x AS U_x_"
                                        + " FROM T LEFT JOIN U ON T.id=U.id"));
      Assert.That(result.Info, Is.EqualTo(@"[. ][.T_x_ notNull tableKey complemented [T.x notNull tableKey]]"
                                         + "[.U_x_ tableKey complemented [U.x notNull tableKey]]"
                                         + "(.y notNull [T.y notNull])(.z [T.z])"
                                         + "(.y [U.y notNull])(.z [U.z])"));

      result = this.GetResultInfoList(@"select null from T right join U on T.id = U.id");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT NULL,T.x AS T_x_,U.x AS U_x_"
                                        + " FROM T RIGHT JOIN U ON T.id=U.id"));
      Assert.That(result.Info, Is.EqualTo(@"[. ][.T_x_ tableKey complemented [T.x notNull tableKey]]"
                                         + "[.U_x_ notNull tableKey complemented [U.x notNull tableKey]]"
                                         + "(.y [T.y notNull])(.z [T.z])"
                                         + "(.y notNull [U.y notNull])(.z [U.z])"));

      result = this.GetResultInfoList(@"select null from T full join U on T.id = U.id");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT NULL,T.x AS T_x_,U.x AS U_x_"
                                        + " FROM T FULL JOIN U ON T.id=U.id"));
      Assert.That(result.Info, Is.EqualTo(@"[. ][.T_x_ tableKey complemented [T.x notNull tableKey]]"
                                         + "[.U_x_ tableKey complemented [U.x notNull tableKey]]"
                                         + "(.y [T.y notNull])(.z [T.z])"
                                         + "(.y [U.y notNull])(.z [U.z])"));

      result = this.GetResultInfoList(@"select null from T, U where T.id = U.id");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT NULL,T.x AS T_x_,U.x AS U_x_ FROM T,U WHERE T.id=U.id"));
      Assert.That(result.Info, Is.EqualTo(@"[. ][.T_x_ notNull tableKey complemented [T.x notNull tableKey]]"
                                         + "[.U_x_ notNull tableKey complemented [U.x notNull tableKey]]"
                                         + "(.y notNull [T.y notNull])(.z [T.z])"
                                         + "(.y notNull [U.y notNull])(.z [U.z])"));

      result = this.GetResultInfoList(@"select null from T, U, V where T.id = U.id");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT NULL,T.x AS T_x_,U.x AS U_x_,V.x1 AS V_x1_"
                                         + ",V.x2 AS V_x2_,V.x3 AS V_x3_ FROM T,U,V WHERE T.id=U.id"));
      Assert.That(result.Info, Is.EqualTo(@"[. ][.T_x_ notNull tableKey complemented [T.x notNull tableKey]]"
                                         + "[.U_x_ notNull tableKey complemented [U.x notNull tableKey]]"
                                         + "[.V_x1_ notNull tableKey complemented [V.x1 notNull tableKey]]"
                                         + "[.V_x2_ notNull tableKey complemented [V.x2 notNull tableKey]]"
                                         + "[.V_x3_ notNull tableKey complemented [V.x3 notNull tableKey]]"
                                         + "(.y notNull [T.y notNull])(.z [T.z])"
                                         + "(.y notNull [U.y notNull])(.z [U.z])"));

      result = this.GetResultInfoList(@"select null from T left join U on T.id = U.id"
                                  + " left join V on U.id = V.id");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT NULL,T.x AS T_x_,U.x AS U_x_,V.x1 AS V_x1_"
                                        + ",V.x2 AS V_x2_,V.x3 AS V_x3_ FROM"
                                        + " T LEFT JOIN U ON T.id=U.id LEFT JOIN V ON U.id=V.id"));
      Assert.That(result.Info, Is.EqualTo(@"[. ][.T_x_ notNull tableKey complemented [T.x notNull tableKey]]"
                                         + "[.U_x_ tableKey complemented [U.x notNull tableKey]]"
                                         + "[.V_x1_ tableKey complemented [V.x1 notNull tableKey]]"
                                         + "[.V_x2_ tableKey complemented [V.x2 notNull tableKey]]"
                                         + "[.V_x3_ tableKey complemented [V.x3 notNull tableKey]]"
                                         + "(.y notNull [T.y notNull])(.z [T.z])"
                                         + "(.y [U.y notNull])(.z [U.z])"));

      result = this.GetResultInfoList(@"select T.x from T left join U on T.id = U.id");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT T.x,U.x AS U_x_ FROM T LEFT JOIN U ON T.id=U.id"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull tableKey [T.x notNull tableKey]]"
                                         + "[.U_x_ tableKey complemented [U.x notNull tableKey]]"
                                         + "(.y notNull [T.y notNull])(.z [T.z])(.y [U.y notNull])"
                                         + "(.z [U.z])"));

      result = this.GetResultInfoList(@"select T.y from T left join U on T.id = U.id");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT T.y,T.x AS T_x_,U.x AS U_x_"
                                         + " FROM T LEFT JOIN U ON T.id=U.id"));
      Assert.That(result.Info, Is.EqualTo(@"[.y notNull [T.y notNull]][.T_x_ notNull tableKey complemented "
                                         + "[T.x notNull tableKey]]"
                                         + "[.U_x_ tableKey complemented [U.x notNull tableKey]]"
                                         + "(.z [T.z])(.y [U.y notNull])(.z [U.z])"));

      result = this.GetResultInfoList(@"select T.z from T left join U on T.id = U.id");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT T.z,T.x AS T_x_,U.x AS U_x_"
                                         + " FROM T LEFT JOIN U ON T.id=U.id"));
      Assert.That(result.Info, Is.EqualTo(@"[.z [T.z]][.T_x_ notNull tableKey complemented "
                                         + "[T.x notNull tableKey]]"
                                         + "[.U_x_ tableKey complemented [U.x notNull tableKey]]"
                                         + "(.y notNull [T.y notNull])(.y [U.y notNull])(.z [U.z])"));

      result = this.GetResultInfoList(@"select T.x, U.x from T left join U on T.id = U.id");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT T.x,U.x FROM T LEFT JOIN U ON T.id=U.id"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull tableKey [T.x notNull tableKey]][.x tableKey "
                                         + "[U.x notNull tableKey]](.y notNull [T.y notNull])"
                                         + "(.z [T.z])(.y [U.y notNull])"
                                         + "(.z [U.z])"));

      result = this.GetResultInfoList(@"select T.x,T.y,T.z,U.x,U.y,U.z from T left join U on T.id = U.id");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT T.x,T.y,T.z,U.x,U.y,U.z"
                                         + " FROM T LEFT JOIN U ON T.id=U.id"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull tableKey [T.x notNull tableKey]][.y notNull "
                                         + "[T.y notNull]][.z [T.z]][.x tableKey "
                                         + "[U.x notNull tableKey]][.y [U.y notNull]]"
                                         + "[.z [U.z]]"));
    }

    [Test]
    public void JoinGroupBy() {
      Result result;

      result = this.GetResultInfoList(@"select T.x from T join U on T.x = U.x group by T.x");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT T.x,COUNT(U.x) AS RESULT_COUNT_ "
                                        + "FROM T JOIN U ON T.x=U.x GROUP BY T.x"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull groupKey [T.x notNull tableKey]]"
                                         + "[.RESULT_COUNT_ notNull countKey complemented [U.x notNull tableKey]]"));

      result = this.GetResultInfoList(@"select T.z from T join U on T.z = U.z group by T.z");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT T.z,COUNT(T.x) AS RESULT_COUNT_,COUNT(U.x) AS RESULT_COUNT__ "
                                        + "FROM T JOIN U ON T.z=U.z GROUP BY T.z"));
      Assert.That(result.Info, Is.EqualTo(@"[.z groupKey [T.z]]"
                                         + "[.RESULT_COUNT_ notNull countKey complemented [T.x notNull tableKey]]"
                                         + "[.RESULT_COUNT__ notNull countKey complemented [U.x notNull tableKey]]"));

      result = this.GetResultInfoList(@"select z from (select T.z from T join U on 1=1) v group by z");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT z,COUNT(v.T_x_) AS RESULT_COUNT_,COUNT(v.U_x_) AS RESULT_COUNT__ "
                                        + "FROM (SELECT T.z,T.x AS T_x_,U.x AS U_x_ FROM T JOIN U ON 1=1)v GROUP BY z"));
      Assert.That(result.Info, Is.EqualTo(@"[.z groupKey [v.z [T.z]]][.RESULT_COUNT_ notNull countKey complemented "
                                         + "[v.T_x_ notNull tableKey complemented [T.x notNull tableKey]]]"
                                         + "[.RESULT_COUNT__ notNull countKey complemented "
                                         + "[v.U_x_ notNull tableKey complemented [U.x notNull tableKey]]]"));

      result = this.GetResultInfoList(@"select 1 from
                                        (select count(*) from T join U on 1=1 group by z) v");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 1,v.T_z_ AS T_z_,v.U_z_ AS U_z_"
                                         + ",v.RESULT_COUNT_ AS RESULT_COUNT_"
                                         + ",v.RESULT_COUNT__ AS RESULT_COUNT__ "
                                         + "FROM (SELECT count(*),T.z AS T_z_,U.z AS U_z_"
                                         + ",COUNT(T.x) AS RESULT_COUNT_,COUNT(U.x) AS RESULT_COUNT__"
                                         + " FROM T JOIN U ON 1=1 GROUP BY z)v"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull ]"
                                         + "[.T_z_ groupKey complemented [v.T_z_ groupKey complemented [T.z]]]"
                                         + "[.U_z_ groupKey complemented [v.U_z_ groupKey complemented [U.z]]]"
                                         + "[.RESULT_COUNT_ notNull countKey complemented "
                                         + "[v.RESULT_COUNT_ notNull countKey complemented [T.x notNull tableKey]]]"
                                         + "[.RESULT_COUNT__ notNull countKey complemented "
                                         + "[v.RESULT_COUNT__ notNull countKey complemented [U.x notNull tableKey]]]"
                                         + "(. notNull [v. notNull ])"));

      result = this.GetResultInfoList(@"select 1 from (select count(U.x) from T left join U on 1=1 group by z)");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 1,col0_ AS col0__,T_z_ AS T_z_,U_z_ AS U_z_"
                                         + ",RESULT_COUNT_ AS RESULT_COUNT_ FROM "
                                         + "(SELECT count(U.x) AS col0_,T.z AS T_z_,U.z AS U_z_"
                                         + ",COUNT(T.x) AS RESULT_COUNT_ FROM "
                                         + "T LEFT JOIN U ON 1=1 GROUP BY z)"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull ][.col0__ notNull countKey complemented "
                                         + "[.col0_ notNull countKey [U.x notNull tableKey]]]"
                                         + "[.T_z_ groupKey complemented [.T_z_ groupKey complemented [T.z]]]"
                                         + "[.U_z_ groupKey complemented [.U_z_ groupKey complemented [U.z]]]"
                                         + "[.RESULT_COUNT_ notNull countKey complemented "
                                         + "[.RESULT_COUNT_ notNull countKey complemented "
                                         + "[T.x notNull tableKey]]]"));

      result = this.GetResultInfoList(@"SELECT 1"
                                         + ",COUNT(v1.t1_x_) AS RESULT_COUNT_,COUNT(v1.t2_x_) AS RESULT_COUNT__"
                                         + ",COUNT(v2.u1_x_) AS RESULT_COUNT___,COUNT(v2.u2_x_) AS RESULT_COUNT____"
                                         + " FROM"
                                         + " (SELECT 1,t1.x AS t1_x_,t2.x AS t2_x_"
                                         + " FROM T t1 LEFT JOIN T t2 ON t1.x=t2.x) v1"
                                         + " LEFT JOIN (SELECT 1,u1.x AS u1_x_,u2.x AS u2_x_"
                                         + " FROM U u1 LEFT JOIN U u2 ON u1.x=u2.x) v2"
                                         + " ON v1.x=v2.x GROUP BY v1.z");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 1"
                                         + ",COUNT(v1.t1_x_) AS RESULT_COUNT_,COUNT(v1.t2_x_) AS RESULT_COUNT__"
                                         + ",COUNT(v2.u1_x_) AS RESULT_COUNT___,COUNT(v2.u2_x_) AS RESULT_COUNT____"
                                         + " FROM"
                                         + " (SELECT 1,t1.x AS t1_x_,t2.x AS t2_x_"
                                         + " FROM T t1 LEFT JOIN T t2 ON t1.x=t2.x)v1"
                                         + " LEFT JOIN (SELECT 1,u1.x AS u1_x_,u2.x AS u2_x_"
                                         + " FROM U u1 LEFT JOIN U u2 ON u1.x=u2.x)v2"
                                         + " ON v1.x=v2.x GROUP BY v1.z"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull ]"
                                         + "[.RESULT_COUNT_ notNull countKey [v1.t1_x_ notNull tableKey [t1.x notNull tableKey]]]"
                                         + "[.RESULT_COUNT__ notNull countKey [v1.t2_x_ tableKey [t2.x notNull tableKey]]]"
                                         + "[.RESULT_COUNT___ notNull countKey [v2.u1_x_ notNull tableKey [u1.x notNull tableKey]]]"
                                         + "[.RESULT_COUNT____ notNull countKey [v2.u2_x_ tableKey [u2.x notNull tableKey]]]"));

      result = this.GetResultInfoList("select 1 from "
                                    + "  (select v1.x1 from V v1 left join V v2 on v1.x1=v2.x1) view "
                                    + "group by view.x1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 1,view.x1 AS view_x1_,COUNT(view.v2_x1_) AS RESULT_COUNT_ "
                                        + "FROM (SELECT v1.x1,v1.x2 AS v1_x2_,v1.x3 AS v1_x3_"
                                        + ",v2.x1 AS v2_x1_,v2.x2 AS v2_x2_,v2.x3 AS v2_x3_ "
                                        + "FROM V v1 LEFT JOIN V v2 ON v1.x1=v2.x1)view "
                                        + "GROUP BY view.x1"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull ]"
                                          + "[.view_x1_ notNull groupKey complemented "
                                          + "[view.x1 notNull tableKey [v1.x1 notNull tableKey]]]"
                                          + "[.RESULT_COUNT_ notNull countKey complemented "
                                          + "[view.v2_x1_ tableKey complemented [v2.x1 notNull tableKey]]]"));
    }


    [Test]
    public void UnionAll() {
      Result result;

      result = this.GetResultInfoList(@"select 1 from T union all select x from U");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 1,T.x AS T_x_,NULL AS U_x_ FROM T UNION ALL "
                                         + "SELECT x,NULL AS T_x_,x AS U_x_ FROM U"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull [. notNull ] [.x notNull tableKey [U.x notNull tableKey]]]"
                                         + "[.T_x_ tableKey complemented "
                                         + "[.T_x_ notNull tableKey complemented [T.x notNull tableKey]] "
                                         + "[.T_x_ complemented ]]"
                                         + "[.U_x_ tableKey complemented [.U_x_ complemented ] "
                                         + "[.U_x_ notNull tableKey complemented [U.x notNull tableKey]]]"));

      result = this.GetResultInfoList(@"select T.x from T union all select U.x from U");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT T.x,T.x AS T_x_,NULL AS U_x_ FROM T "
                                         + "UNION ALL "
                                         + "SELECT U.x,NULL AS T_x_,U.x AS U_x_ FROM U"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull [.x notNull tableKey [T.x notNull tableKey]] [.x notNull tableKey "
                                         + "[U.x notNull tableKey]]][.T_x_ tableKey complemented [.T_x_ notNull tableKey complemented "
                                         + "[T.x notNull tableKey]] [.T_x_ complemented ]][.U_x_ tableKey complemented "
                                         + "[.U_x_ complemented ] [.U_x_ notNull tableKey complemented [U.x notNull tableKey]]]"));

      result = this.GetResultInfoList(@"select x from T union all select x from U "
                                  + "union all select 1 from V");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT x,x AS T_x_,NULL AS U_x_,NULL AS V_x1_,NULL AS V_x2_"
                                         + ",NULL AS V_x3_ FROM T UNION ALL SELECT x,NULL AS T_x_"
                                         + ",x AS U_x_,NULL AS V_x1_,NULL AS V_x2_,NULL AS V_x3_ FROM U"
                                         + " UNION ALL SELECT 1,NULL AS T_x_,NULL AS U_x_,V.x1 AS V_x1_"
                                         + ",V.x2 AS V_x2_,V.x3 AS V_x3_ FROM V"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull [.x notNull [.x notNull tableKey [T.x notNull tableKey]] "
                                         + "[.x notNull tableKey [U.x notNull tableKey]]] [. notNull ]]"
                                         + "[.T_x_ tableKey complemented [.T_x_ tableKey complemented [.T_x_ notNull tableKey complemented "
                                         + "[T.x notNull tableKey]] [.T_x_ complemented ]] [.T_x_ complemented ]]"
                                         + "[.U_x_ tableKey complemented [.U_x_ tableKey complemented "
                                         + "[.U_x_ complemented ] [.U_x_ notNull tableKey complemented [U.x notNull tableKey]]] "
                                         + "[.U_x_ complemented ]][.V_x1_ tableKey complemented [.V_x1_ complemented "
                                         + "[.V_x1_ complemented ] [.V_x1_ complemented ]] "
                                         + "[.V_x1_ notNull tableKey complemented [V.x1 notNull tableKey]]]"
                                         + "[.V_x2_ tableKey complemented [.V_x2_ complemented [.V_x2_ complemented ] "
                                         + "[.V_x2_ complemented ]] [.V_x2_ notNull tableKey complemented "
                                         + "[V.x2 notNull tableKey]]][.V_x3_ tableKey complemented [.V_x3_ complemented "
                                         + "[.V_x3_ complemented ] [.V_x3_ complemented ]] "
                                         + "[.V_x3_ notNull tableKey complemented [V.x3 notNull tableKey]]]"));

      result = this.GetResultInfoList(@"select * from (select 1 from T) onet "
                                  + "left join (select 2 from U) two on 1=1 "
                                  + "union all select * from (select 1 from T) onet "
                                  + "left join (select 2 from U) two on 1=1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT onet.onet_col0_,two.two_col1_,onet.T_x_ AS T_x_"
                                  + ",NULL AS T_x__,two.U_x_ AS U_x_,NULL AS U_x__ FROM "
                                  + "(SELECT 1 AS onet_col0_,T.x AS T_x_ FROM T)onet"
                                  + " LEFT JOIN (SELECT 2 AS two_col1_,U.x AS U_x_"
                                  + " FROM U)two ON 1=1 UNION ALL SELECT onet.onet_col0_"
                                  + ",two.two_col1_,NULL AS T_x_,onet.T_x_ AS T_x__"
                                  + ",NULL AS U_x_,two.U_x_ AS U_x__ FROM (SELECT 1 AS onet_col0_"
                                  + ",T.x AS T_x_ FROM T)onet LEFT JOIN (SELECT 2 AS two_col1_"
                                  + ",U.x AS U_x_ FROM U)two ON 1=1"));
      Assert.That(result.Info, Is.EqualTo(@"[.onet_col0_ notNull [.onet_col0_ notNull [onet.onet_col0_ notNull ]] "
                                  + "[.onet_col0_ notNull [onet.onet_col0_ notNull ]]][.two_col1_ [.two_col1_ "
                                  + "[two.two_col1_ notNull ]] [.two_col1_ [two.two_col1_ notNull ]]]"
                                  + "[.T_x_ tableKey complemented [.T_x_ notNull tableKey complemented "
                                  + "[onet.T_x_ notNull tableKey complemented [T.x notNull tableKey]]] [.T_x_ complemented ]]"
                                  + "[.T_x__ tableKey complemented [.T_x__ complemented ] [.T_x__ notNull tableKey complemented "
                                  + "[onet.T_x_ notNull tableKey complemented [T.x notNull tableKey]]]]"
                                  + "[.U_x_ tableKey complemented [.U_x_ tableKey complemented [two.U_x_ notNull tableKey complemented "
                                  + "[U.x notNull tableKey]]] [.U_x_ complemented ]][.U_x__ tableKey complemented "
                                  + "[.U_x__ complemented ] [.U_x__ tableKey complemented [two.U_x_ notNull tableKey complemented "
                                  + "[U.x notNull tableKey]]]]"));

      result = this.GetResultInfoList(@"select * from"
                                  + " (select * from"
                                  + "   (select 1 from T) onet "
                                  + " ) o"
                                  + " left join"
                                  + " (select * from"
                                  + "   (select 2 from U) two"
                                  + " ) t"
                                  + " on 1=1 "
                                  + "union all "
                                  + "select * from"
                                  + " (select * from"
                                  + "   (select 1 from T) onet "
                                  + " ) o"
                                  + " left join"
                                  + " (select * from"
                                  + "   (select 2 from U) two"
                                  + " ) t"
                                  + " on 1=1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT o.onet_col0_,t.two_col1_,o.T_x_ AS T_x_,NULL AS T_x__,t.U_x_ AS U_x_,NULL AS U_x__"
                                  + " FROM (SELECT * FROM (SELECT 1 AS onet_col0_,T.x AS T_x_ FROM T)onet)o"
                                  + " LEFT JOIN (SELECT * FROM (SELECT 2 AS two_col1_,U.x AS U_x_ FROM U)two)t"
                                  + " ON 1=1 "
                                  + "UNION ALL "
                                  + "SELECT o.onet_col0_,t.two_col1_,NULL AS T_x_,o.T_x_ AS T_x__,NULL AS U_x_,t.U_x_ AS U_x__"
                                  + " FROM (SELECT * FROM (SELECT 1 AS onet_col0_,T.x AS T_x_ FROM T)onet)o"
                                  + " LEFT JOIN (SELECT * FROM (SELECT 2 AS two_col1_,U.x AS U_x_ FROM U)two)t"
                                  + " ON 1=1"));
      Assert.That(result.Info, Is.EqualTo(@"[.onet_col0_ notNull [.onet_col0_ notNull [o.onet_col0_ notNull [onet.onet_col0_ notNull ]]] "
                                  + "[.onet_col0_ notNull [o.onet_col0_ notNull [onet.onet_col0_ notNull ]]]]"
                                  + "[.two_col1_ [.two_col1_ [t.two_col1_ notNull [two.two_col1_ notNull ]]] "
                                  + "[.two_col1_ [t.two_col1_ notNull [two.two_col1_ notNull ]]]]"
                                  + "[.T_x_ tableKey complemented [.T_x_ notNull tableKey complemented "
                                  + "[o.T_x_ notNull tableKey complemented [onet.T_x_ notNull tableKey complemented "
                                  + "[T.x notNull tableKey]]]] [.T_x_ complemented ]]"
                                  + "[.T_x__ tableKey complemented [.T_x__ complemented ] "
                                  + "[.T_x__ notNull tableKey complemented [o.T_x_ notNull tableKey complemented "
                                  + "[onet.T_x_ notNull tableKey complemented [T.x notNull tableKey]]]]]"
                                  + "[.U_x_ tableKey complemented [.U_x_ tableKey complemented "
                                  + "[t.U_x_ notNull tableKey complemented [two.U_x_ notNull tableKey complemented "
                                  + "[U.x notNull tableKey]]]] [.U_x_ complemented ]]"
                                  + "[.U_x__ tableKey complemented [.U_x__ complemented ] "
                                  + "[.U_x__ tableKey complemented [t.U_x_ notNull tableKey complemented "
                                  + "[two.U_x_ notNull tableKey complemented [U.x notNull tableKey]]]]]"));

      result = this.GetResultInfoList(@"select * from"
                                  + " (select * from"
                                  + "   (select 1 as aaa from T) onet "
                                  + " ) o"
                                  + " left join"
                                  + " (select * from"
                                  + "   (select 2 from U) two"
                                  + " ) t"
                                  + " on 1=1 "
                                  + "union all "
                                  + "select * from"
                                  + " (select * from"
                                  + "   (select 1 from T) onet "
                                  + " ) o"
                                  + " left join"
                                  + " (select * from"
                                  + "   (select 2 as bbb from U) two"
                                  + " ) t"
                                  + " on 1=1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT o.aaa,t.two_col1_,o.T_x_ AS T_x_,NULL AS T_x__,t.U_x_ AS U_x_,NULL AS U_x__"
                                  + " FROM (SELECT * FROM (SELECT 1 AS aaa,T.x AS T_x_ FROM T)onet)o"
                                  + " LEFT JOIN (SELECT * FROM (SELECT 2 AS two_col1_,U.x AS U_x_ FROM U)two)t"
                                  + " ON 1=1 "
                                  + "UNION ALL "
                                  + "SELECT o.onet_col0_,t.bbb,NULL AS T_x_,o.T_x_ AS T_x__,NULL AS U_x_,t.U_x_ AS U_x__"
                                  + " FROM (SELECT * FROM (SELECT 1 AS onet_col0_,T.x AS T_x_ FROM T)onet)o"
                                  + " LEFT JOIN (SELECT * FROM (SELECT 2 AS bbb,U.x AS U_x_ FROM U)two)t"
                                  + " ON 1=1"));
      Assert.That(result.Info, Is.EqualTo(@"[.aaa notNull [.aaa notNull [o.aaa notNull [onet.aaa notNull ]]] "
                                         + "[.onet_col0_ notNull [o.onet_col0_ notNull [onet.onet_col0_ notNull ]]]]"
                                         + "[.two_col1_ [.two_col1_ [t.two_col1_ notNull [two.two_col1_ notNull ]]] "
                                         + "[.bbb [t.bbb notNull [two.bbb notNull ]]]]"
                                         + "[.T_x_ tableKey complemented [.T_x_ notNull tableKey complemented "
                                         + "[o.T_x_ notNull tableKey complemented [onet.T_x_ notNull tableKey complemented "
                                         + "[T.x notNull tableKey]]]] "
                                         + "[.T_x_ complemented ]][.T_x__ tableKey complemented [.T_x__ complemented ] "
                                         + "[.T_x__ notNull tableKey complemented [o.T_x_ notNull tableKey complemented "
                                         + "[onet.T_x_ notNull tableKey complemented [T.x notNull tableKey]]]]]"
                                         + "[.U_x_ tableKey complemented [.U_x_ tableKey complemented "
                                         + "[t.U_x_ notNull tableKey complemented [two.U_x_ notNull tableKey complemented "
                                         + "[U.x notNull tableKey]]]] "
                                         + "[.U_x_ complemented ]][.U_x__ tableKey complemented [.U_x__ complemented ] "
                                         + "[.U_x__ tableKey complemented [t.U_x_ notNull tableKey complemented "
                                         + "[two.U_x_ notNull tableKey complemented [U.x notNull tableKey]]]]]"));

      result = this.GetResultInfoList("select V.* from (select y, z from T) V " +
                                   "union all " +
                                   "select 1, 2 from U");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT V.y,V.z,V.T_x_ AS T_x_,NULL AS U_x_ "
                                        + "FROM (SELECT y,z,T.x AS T_x_ FROM T)V "
                                        + "UNION ALL "
                                        + "SELECT 1,2,NULL AS T_x_,U.x AS U_x_ FROM U"));
      Assert.That(result.Info, Is.EqualTo(@"[.y notNull [.y notNull [V.y notNull [T.y notNull]]] [. notNull ]]"
                                         + "[.z [.z [V.z [T.z]]] [. notNull ]]"
                                         + "[.T_x_ tableKey complemented [.T_x_ notNull tableKey complemented "
                                         + "[V.T_x_ notNull tableKey complemented [T.x notNull tableKey]]] "
                                         + "[.T_x_ complemented ]]"
                                         + "[.U_x_ tableKey complemented [.U_x_ complemented ] "
                                         + "[.U_x_ notNull tableKey complemented [U.x notNull tableKey]]]"));

      result = this.GetResultInfoList("select x from T union all select x from U " +
                                   "union all select 1 from V");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT x,x AS T_x_,NULL AS U_x_,NULL AS V_x1_,NULL AS V_x2_,NULL AS V_x3_ "
                                        + "FROM T "
                                        + "UNION ALL "
                                        + "SELECT x,NULL AS T_x_,x AS U_x_,NULL AS V_x1_,NULL AS V_x2_,NULL AS V_x3_ "
                                        + "FROM U "
                                        + "UNION ALL "
                                        + "SELECT 1,NULL AS T_x_,NULL AS U_x_,V.x1 AS V_x1_,V.x2 AS V_x2_,V.x3 AS V_x3_ "
                                        + "FROM V"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull [.x notNull [.x notNull tableKey [T.x notNull tableKey]] "
                                         + "[.x notNull tableKey [U.x notNull tableKey]]] [. notNull ]]"
                                         + "[.T_x_ tableKey complemented [.T_x_ tableKey complemented [.T_x_ notNull tableKey complemented "
                                         + "[T.x notNull tableKey]] [.T_x_ complemented ]] [.T_x_ complemented ]]"
                                         + "[.U_x_ tableKey complemented [.U_x_ tableKey complemented [.U_x_ complemented ] "
                                         + "[.U_x_ notNull tableKey complemented [U.x notNull tableKey]]] [.U_x_ complemented ]]"
                                         + "[.V_x1_ tableKey complemented [.V_x1_ complemented [.V_x1_ complemented ] "
                                         + "[.V_x1_ complemented ]] [.V_x1_ notNull tableKey complemented "
                                         + "[V.x1 notNull tableKey]]][.V_x2_ tableKey complemented [.V_x2_ complemented "
                                         + "[.V_x2_ complemented ] [.V_x2_ complemented ]] "
                                         + "[.V_x2_ notNull tableKey complemented [V.x2 notNull tableKey]]]"
                                         + "[.V_x3_ tableKey complemented [.V_x3_ complemented [.V_x3_ complemented ] "
                                         + "[.V_x3_ complemented ]] [.V_x3_ notNull tableKey complemented "
                                         + "[V.x3 notNull tableKey]]]"));

      result = this.GetResultInfoList("(select x from T ) "
                                 + " union all (( select x from U)"
                                 + " union all  ( select x from V))");
      Assert.That(result.Sql, Is.EqualTo(@"(SELECT x,NULL AS V_x1_,NULL AS V_x2_,NULL AS V_x3_,NULL AS U_x_,x AS T_x_ "
                                        + "FROM T) "
                                        + "UNION ALL "
                                        + "((SELECT x,NULL AS V_x1_,NULL AS V_x2_,NULL AS V_x3_,x AS U_x_,NULL AS T_x_ "
                                        + "FROM U) "
                                        + "UNION ALL "
                                        + "(SELECT x,V.x1 AS V_x1_,V.x2 AS V_x2_,V.x3 AS V_x3_,NULL AS U_x_,NULL AS T_x_ "
                                        + "FROM V))"));
      Assert.That(result.Info, Is.EqualTo(@"[.x [.x notNull tableKey [T.x notNull tableKey]] [.x [.x notNull tableKey [U.x notNull tableKey]] [.x ]]]"
                                        + "[.V_x1_ tableKey complemented [.V_x1_ complemented ] [.V_x1_ tableKey complemented "
                                        + "[.V_x1_ complemented ] [.V_x1_ notNull tableKey complemented [V.x1 notNull tableKey]]]]"
                                        + "[.V_x2_ tableKey complemented [.V_x2_ complemented ] [.V_x2_ tableKey complemented "
                                        + "[.V_x2_ complemented ] [.V_x2_ notNull tableKey complemented [V.x2 notNull tableKey]]]]"
                                        + "[.V_x3_ tableKey complemented [.V_x3_ complemented ] [.V_x3_ tableKey complemented "
                                        + "[.V_x3_ complemented ] [.V_x3_ notNull tableKey complemented [V.x3 notNull tableKey]]]]"
                                        + "[.U_x_ tableKey complemented [.U_x_ complemented ] [.U_x_ tableKey complemented "
                                        + "[.U_x_ notNull tableKey complemented [U.x notNull tableKey]] [.U_x_ complemented ]]]"
                                        + "[.T_x_ tableKey complemented [.T_x_ notNull tableKey complemented [T.x notNull tableKey]] "
                                        + "[.T_x_ complemented [.T_x_ complemented ] [.T_x_ complemented ]]]"));

      result = this.GetResultInfoList("select * from T "
                                 + " union all select * from U"
                                 + " union all select * from V");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT T.x,T.y,T.z,T.x AS T_x_,NULL AS U_x_,NULL AS V_x1_,NULL AS V_x2_,NULL AS V_x3_"
                                        + " FROM T"
                                        + " UNION ALL "
                                        + "SELECT U.x,U.y,U.z,NULL AS T_x_,U.x AS U_x_,NULL AS V_x1_,NULL AS V_x2_,NULL AS V_x3_"
                                        + " FROM U"
                                        + " UNION ALL "
                                        + "SELECT V.x1,V.x2,V.x3,NULL AS T_x_,NULL AS U_x_,V.x1 AS V_x1_,V.x2 AS V_x2_,V.x3 AS V_x3_"
                                        + " FROM V"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull [.x notNull [.x notNull tableKey [T.x notNull tableKey]] [.x notNull tableKey "
                                        + "[U.x notNull tableKey]]] [.x1 notNull tableKey [V.x1 notNull tableKey]]]"
                                        + "[.y notNull [.y notNull [.y notNull [T.y notNull]] [.y notNull [U.y notNull]]] "
                                        + "[.x2 notNull tableKey [V.x2 notNull tableKey]]]"
                                        + "[.z [.z [.z [T.z]] [.z [U.z]]] "
                                        + "[.x3 notNull tableKey [V.x3 notNull tableKey]]]"
                                        + "[.T_x_ tableKey complemented [.T_x_ tableKey complemented [.T_x_ notNull tableKey complemented "
                                        + "[T.x notNull tableKey]] [.T_x_ complemented ]] [.T_x_ complemented ]]"
                                        + "[.U_x_ tableKey complemented [.U_x_ tableKey complemented [.U_x_ complemented ] "
                                        + "[.U_x_ notNull tableKey complemented [U.x notNull tableKey]]] [.U_x_ complemented ]]"
                                        + "[.V_x1_ tableKey complemented [.V_x1_ complemented [.V_x1_ complemented ] "
                                        + "[.V_x1_ complemented ]] [.V_x1_ notNull tableKey complemented "
                                        + "[V.x1 notNull tableKey]]][.V_x2_ tableKey complemented [.V_x2_ complemented "
                                        + "[.V_x2_ complemented ] [.V_x2_ complemented ]] "
                                        + "[.V_x2_ notNull tableKey complemented [V.x2 notNull tableKey]]][.V_x3_ tableKey complemented "
                                        + "[.V_x3_ complemented [.V_x3_ complemented ] [.V_x3_ complemented ]] "
                                        + "[.V_x3_ notNull tableKey complemented [V.x3 notNull tableKey]]]"));

      // GetSelectItemInfoVisitorで既に主キー補完処理されたSQL文に対しては、補完処理をしない
      result = this.GetResultInfoList("SELECT 1, 2, NULL AS T_x_ "
                                  + "UNION ALL "
                                  + "SELECT x, NULL, x AS T_x_ "
                                  + "FROM T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 1,2,NULL AS T_x_ "
                                        + "UNION ALL "
                                        + "SELECT x,NULL,x AS T_x_ "
                                        + "FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull [. notNull ] [.x notNull tableKey [T.x notNull tableKey]]]"
                                        + "[. [. notNull ] [. ]]"
                                        + "[.T_x_ tableKey [.T_x_ ] [.T_x_ notNull tableKey [T.x notNull tableKey]]]"));


      result = this.GetResultInfoList("SELECT 1, NULL AS T_x_ "
                                  + "UNION ALL  "
                                  + "SELECT x, x AS T_x_ "
                                  + "FROM T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 1,NULL AS T_x_ "
                                        + "UNION ALL "
                                        + "SELECT x,x AS T_x_ "
                                        + "FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull [. notNull ] [.x notNull tableKey [T.x notNull tableKey]]]"
                                        + "[.T_x_ tableKey [.T_x_ ] [.T_x_ notNull tableKey [T.x notNull tableKey]]]"));


      result = this.GetResultInfoList("SELECT 1, x AS T_x_ "
                                  + "UNION ALL  "
                                  + "SELECT x, NULL AS T_x_ "
                                  + "FROM T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 1,x AS T_x_,NULL AS T_x__ "
                                        + "UNION ALL "
                                        + "SELECT x,NULL AS T_x_,x AS T_x__ "
                                        + "FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull [. notNull ] [.x notNull tableKey " 
                                          + "[T.x notNull tableKey]]][.T_x_ [.T_x_ ] [.T_x_ ]]"
                                          + "[.T_x__ tableKey complemented [.T_x__ complemented ] "
                                          + "[.T_x__ notNull tableKey complemented [T.x notNull tableKey]]]"));

      result = this.GetResultInfoList("SELECT 1 "
                                  + "UNION ALL  "
                                  + "SELECT x FROM T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 1,NULL AS T_x_ "
                                        + "UNION ALL "
                                        + "SELECT x,x AS T_x_ "
                                        + "FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull [. notNull ] [.x notNull tableKey [T.x notNull tableKey]]]"
                                          + "[.T_x_ tableKey complemented [.T_x_ complemented ] "
                                          + "[.T_x_ notNull tableKey complemented [T.x notNull tableKey]]]"));

      result = this.GetResultInfoList("SELECT x "
                                  + "UNION ALL  "
                                  + "SELECT 1 FROM T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT x,NULL AS T_x_ "
                                          + "UNION ALL "
                                          + "SELECT 1,T.x AS T_x_ "
                                          + "FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[.x [.x ] [. notNull ]]"
                                          + "[.T_x_ tableKey complemented [.T_x_ complemented ] "
                                          + "[.T_x_ notNull tableKey complemented "
                                          + "[T.x notNull tableKey]]]"));

      result = this.GetResultInfoList("SELECT 1, NULL AS T_x_ "
                                  + "UNION ALL  "
                                  + "SELECT x, x AS T_x_ "
                                  + "FROM T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 1,NULL AS T_x_ "
                                        + "UNION ALL "
                                        + "SELECT x,x AS T_x_ "
                                        + "FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull [. notNull ] [.x notNull tableKey [T.x notNull tableKey]]]"
                                         + "[.T_x_ tableKey [.T_x_ ] [.T_x_ notNull tableKey [T.x notNull tableKey]]]"));

      result = this.GetResultInfoList("select x, null, null from T "
                                  + "union all "
                                  + "select null, x, null from U "
                                  + "union all "
                                  + "select null, null, x from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT x,NULL,NULL FROM T "
                                        + "UNION ALL "
                                        + "SELECT NULL,x,NULL FROM U "
                                        + "UNION ALL "
                                        + "SELECT NULL,NULL,x FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[.x tableKey [.x tableKey [.x notNull tableKey [T.x notNull tableKey]] [. ]] [. ]]"
                                         + "[. tableKey [. tableKey [. ] [.x notNull tableKey [U.x notNull tableKey]]] [. ]]"
                                         + "[. tableKey [. [. ] [. ]] [.x notNull tableKey [T.x notNull tableKey]]]"));

      result = this.GetResultInfoList("select x, null, null from T "
                                  + "union all "
                                  + "select * from ( "
                                  + "    select null, x, null from U "
                                  + "    union all "
                                  + "    select null, null, x from T ) ");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT x,NULL,NULL FROM T UNION ALL SELECT * FROM "
                                        + "(SELECT NULL,x,NULL FROM U "
                                        + "UNION ALL "
                                        + "SELECT NULL,NULL,x FROM T)"));
      Assert.That(result.Info, Is.EqualTo(@"[.x tableKey [.x notNull tableKey [T.x notNull tableKey]] "
                                          + "[. [. [. ] [. ]]]]"
                                          + "[. tableKey [. ] [.x tableKey [.x tableKey [.x notNull tableKey "
                                          + "[U.x notNull tableKey]] [. ]]]]"
                                          + "[. tableKey [. ] [. tableKey [. tableKey [. ] [.x notNull tableKey "
                                          + "[T.x notNull tableKey]]]]]"));
    }

    [Test]
    public void UnionAllGroupBy() {
      Result result;

      result = this.GetResultInfoList("select * from (select y from (select * from T)v0 group by y) v1 " +
                                     "union all " +
                                     "select * from (select z from T union all select z from T) v3");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT v1.y,NULL AS T_x_,NULL AS T_x__,v1.y AS v1_y_ "
                                        + "FROM (SELECT y FROM (SELECT * FROM T)v0 GROUP BY y)v1 "
                                        + "UNION ALL "
                                        + "SELECT v3.z,v3.T_x_ AS T_x_,v3.T_x__ AS T_x__,NULL AS v1_y_ FROM "
                                        + "(SELECT z,T.x AS T_x_,NULL AS T_x__ FROM T "
                                        + "UNION ALL "
                                        + "SELECT z,NULL AS T_x_,T.x AS T_x__ FROM T)v3"));
      Assert.That(result.Info, Is.EqualTo(@"[.y [.y notNull groupKey [v1.y notNull groupKey [v0.y notNull [T.y notNull]]]] "
                                        + "[.z [v3.z [.z [T.z]] [.z [T.z]]]]]"
                                        + "[.T_x_ tableKey complemented [.T_x_ complemented ] [.T_x_ tableKey complemented "
                                        + "[v3.T_x_ tableKey complemented [.T_x_ notNull tableKey complemented "
                                        + "[T.x notNull tableKey]] [.T_x_ complemented ]]]]"
                                        + "[.T_x__ tableKey complemented [.T_x__ complemented ] "
                                        + "[.T_x__ tableKey complemented [v3.T_x__ tableKey complemented "
                                        + "[.T_x__ complemented ] [.T_x__ notNull tableKey complemented "
                                        + "[T.x notNull tableKey]]]]]"
                                        + "[.v1_y_ groupKey complemented [.v1_y_ notNull groupKey complemented "
                                        + "[v1.y notNull groupKey [v0.y notNull [T.y notNull]]]] [.v1_y_ complemented ]]"));

      result = this.GetResultInfoList("select y from (select * from T)v0 group by y " +
                                     "union all " +
                                     "select z from T union all select z from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT y,NULL AS T_x_,y AS v0_y_,NULL AS T_x__ "
                                        + "FROM (SELECT * FROM T)v0 GROUP BY y "
                                        + "UNION ALL "
                                        + "SELECT z,T.x AS T_x_,NULL AS v0_y_,NULL AS T_x__ "
                                        + "FROM T "
                                        + "UNION ALL "
                                        + "SELECT z,NULL AS T_x_,NULL AS v0_y_,T.x AS T_x__ "
                                        + "FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[.y [.y [.y notNull groupKey [v0.y notNull [T.y notNull]]] "
                                        + "[.z [T.z]]] [.z [T.z]]]"
                                        + "[.T_x_ tableKey complemented [.T_x_ tableKey complemented [.T_x_ complemented ] "
                                        + "[.T_x_ notNull tableKey complemented [T.x notNull tableKey]]] [.T_x_ complemented ]]"
                                        + "[.v0_y_ groupKey complemented [.v0_y_ groupKey complemented "
                                        + "[.v0_y_ notNull groupKey complemented [v0.y notNull [T.y notNull]]] "
                                        + "[.v0_y_ complemented ]] [.v0_y_ complemented ]]"
                                        + "[.T_x__ tableKey complemented [.T_x__ complemented [.T_x__ complemented ] "
                                        + "[.T_x__ complemented ]] [.T_x__ notNull tableKey complemented [T.x notNull tableKey]]]"));

      result = this.GetResultInfoList("SELECT x,y,z FROM T GROUP BY z "
                                    + "UNION ALL "
                                    + "SELECT U.x,U.y,U.z FROM U");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT x,y,z,z AS T_z_,NULL AS U_x_ FROM T GROUP BY z "
                                        + "UNION ALL "
                                        + "SELECT U.x,U.y,U.z,NULL AS T_z_,U.x AS U_x_ FROM U"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull [.x notNull [T.x notNull tableKey]] [.x notNull tableKey "
                                          + "[U.x notNull tableKey]]]"
                                          + "[.y notNull [.y notNull [T.y notNull]] [.y notNull [U.y notNull]]]"
                                          + "[.z [.z groupKey [T.z]] [.z [U.z]]]"
                                          + "[.T_z_ groupKey complemented [.T_z_ groupKey complemented [T.z]] "
                                          + "[.T_z_ complemented ]]"
                                          + "[.U_x_ tableKey complemented [.U_x_ complemented ] "
                                          + "[.U_x_ notNull tableKey complemented [U.x notNull tableKey]]]"));

      result = this.GetResultInfoList("SELECT z FROM T GROUP BY z "
                                    + "UNION ALL "
                                    + "SELECT U.z FROM U");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT z,COUNT(T.x) AS RESULT_COUNT_,NULL AS U_x_,z AS T_z_ FROM T GROUP BY z "
                                        + "UNION ALL "
                                        + "SELECT U.z,NULL AS RESULT_COUNT_,U.x AS U_x_,NULL AS T_z_ FROM U"));
      Assert.That(result.Info, Is.EqualTo(@"[.z [.z groupKey [T.z]] [.z [U.z]]]"
                                          + "[.RESULT_COUNT_ countKey complemented [.RESULT_COUNT_ notNull countKey complemented [T.x notNull tableKey]] "
                                          + "[.RESULT_COUNT_ complemented ]]"
                                          + "[.U_x_ tableKey complemented [.U_x_ complemented ] "
                                          + "[.U_x_ notNull tableKey complemented [U.x notNull tableKey]]]"
                                          + "[.T_z_ groupKey complemented [.T_z_ groupKey complemented "
                                          + "[T.z]] [.T_z_ complemented ]]"
                                          ));

      result = this.GetResultInfoList(@"select z,COUNT(v.T_x_) AS RESULT_COUNT_,COUNT(v.U_x_) AS RESULT_COUNT__ from"
                                     + " (select z,T.x AS T_x_,NULL AS U_x_ from T "
                                     + "  union all "
                                     + "  select z,NULL AS T_x_,U.x AS U_x_ from U) v "
                                     + "group by z");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT z,COUNT(v.T_x_) AS RESULT_COUNT_,COUNT(v.U_x_) AS RESULT_COUNT__ FROM "
                                     + "(SELECT z,T.x AS T_x_,NULL AS U_x_ FROM T"
                                     + " UNION ALL"
                                     + " SELECT z,NULL AS T_x_,U.x AS U_x_ FROM U)v "
                                     + "GROUP BY z"));
      Assert.That(result.Info, Is.EqualTo(@"[.z groupKey [v.z [.z [T.z]] [.z [U.z]]]]"
                                         + "[.RESULT_COUNT_ notNull countKey "
                                         + "[v.T_x_ tableKey [.T_x_ notNull tableKey "
                                         + "[T.x notNull tableKey]] [.T_x_ ]]]"
                                         + "[.RESULT_COUNT__ notNull countKey [v.U_x_ tableKey "
                                         + "[.U_x_ ] [.U_x_ notNull tableKey "
                                         + "[U.x notNull tableKey]]]]"));

      result = this.GetResultInfoList(@"select z from ( select * from T "
                                  + "union all select * from U) v");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT z,v.T_x_ AS T_x_,v.U_x_ AS U_x_ FROM "
                                        + "(SELECT T.x,T.y,T.z,T.x AS T_x_,NULL AS U_x_ FROM T "
                                        + "UNION ALL "
                                        + "SELECT U.x,U.y,U.z,NULL AS T_x_,U.x AS U_x_ FROM U)v"));
      Assert.That(result.Info, Is.EqualTo(@"[.z [v.z [.z [T.z]] [.z [U.z]]]]"
                                          + "[.T_x_ tableKey complemented [v.T_x_ tableKey complemented "
                                          + "[.T_x_ notNull tableKey complemented [T.x notNull tableKey]] "
                                          + "[.T_x_ complemented ]]]"
                                          + "[.U_x_ tableKey complemented [v.U_x_ tableKey complemented "
                                          + "[.U_x_ complemented ] [.U_x_ notNull tableKey complemented "
                                          + "[U.x notNull tableKey]]]]"
                                          + "(.x notNull [v.x notNull [.x notNull tableKey [T.x notNull tableKey]] "
                                          + "[.x notNull tableKey [U.x notNull tableKey]]])"
                                          + "(.y notNull [v.y notNull [.y notNull [T.y notNull]] [.y notNull "
                                          + "[U.y notNull]]])"));

      result = this.GetResultInfoList(@"select z from (select * from T "
                                     + "union all select * from U) v group by z");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT z,COUNT(v.T_x_) AS RESULT_COUNT_,COUNT(v.U_x_) AS RESULT_COUNT__ FROM "
                                        + "(SELECT T.x,T.y,T.z,T.x AS T_x_,NULL AS U_x_ FROM T"
                                        + " UNION ALL"
                                        + " SELECT U.x,U.y,U.z,NULL AS T_x_,U.x AS U_x_ FROM U)v"
                                        + " GROUP BY z"));
      Assert.That(result.Info, Is.EqualTo(@"[.z groupKey [v.z [.z [T.z]] [.z [U.z]]]]"
                                         + "[.RESULT_COUNT_ notNull countKey complemented "
                                         + "[v.T_x_ tableKey complemented [.T_x_ notNull tableKey complemented "
                                         + "[T.x notNull tableKey]] [.T_x_ complemented ]]]"
                                         + "[.RESULT_COUNT__ notNull countKey complemented [v.U_x_ tableKey complemented "
                                         + "[.U_x_ complemented ] [.U_x_ notNull tableKey complemented "
                                         + "[U.x notNull tableKey]]]]"));

      result = this.GetResultInfoList("select 1 from "
                                    + "  (select x1 from V v1 union all select null from V v2 ) view "
                                    + "group by view.x1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 1,view.x1 AS view_x1_,COUNT(view.v2_x1_) AS RESULT_COUNT_"
                                        + " FROM (SELECT x1,v1.x2 AS v1_x2_,v1.x3 AS v1_x3_"
                                        + ",NULL AS v2_x1_,NULL AS v2_x2_,NULL AS v2_x3_ FROM V v1"
                                        + " UNION ALL "
                                        + "SELECT NULL,NULL AS v1_x2_,NULL AS v1_x3_,v2.x1 AS v2_x1_"
                                        + ",v2.x2 AS v2_x2_,v2.x3 AS v2_x3_ FROM V v2)view "
                                        + "GROUP BY view.x1"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull ]"
                                    + "[.view_x1_ groupKey complemented [view.x1 tableKey "
                                    + "[.x1 notNull tableKey [v1.x1 notNull tableKey]] [. ]]]"
                                    + "[.RESULT_COUNT_ notNull countKey complemented "
                                    + "[view.v2_x1_ tableKey complemented [.v2_x1_ complemented ] "
                                    + "[.v2_x1_ notNull tableKey complemented [v2.x1 notNull tableKey]]]]"));
    }

    [Test]
    public void UnionAllJoinGroupBy() {
      Result result;

      result = this.GetResultInfoList(@"select 1 from"
                                    + "  (select * from T"
                                    + "   union all"
                                    + "   select * from V ) v0"
                                    + "  left join"
                                    + "  (select * from T"
                                    + "   union all"
                                    + "   select * from V ) v1"
                                    + " group by v0.x");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 1,v0.x AS v0_x_"
                                        + ",COUNT(v0.T_x_) AS RESULT_COUNT_,COUNT(v0.V_x1_) AS RESULT_COUNT__"
                                        + ",COUNT(v1.T_x_) AS RESULT_COUNT___,COUNT(v1.V_x1_) AS RESULT_COUNT____"
                                        + " FROM (SELECT T.x,T.y,T.z,T.x AS T_x_,NULL AS V_x1_,NULL AS V_x2_,NULL AS V_x3_"
                                        + " FROM T"
                                        + " UNION ALL"
                                        + " SELECT V.x1,V.x2,V.x3,NULL AS T_x_,V.x1 AS V_x1_,V.x2 AS V_x2_,V.x3 AS V_x3_"
                                        + " FROM V)v0"
                                        + " LEFT JOIN (SELECT"
                                        + " T.x,T.y,T.z,T.x AS T_x_,NULL AS V_x1_,NULL AS V_x2_,NULL AS V_x3_"
                                        + " FROM T"
                                        + " UNION ALL"
                                        + " SELECT V.x1,V.x2,V.x3,NULL AS T_x_,V.x1 AS V_x1_,V.x2 AS V_x2_,V.x3 AS V_x3_"
                                        + " FROM V)v1 "
                                        + "GROUP BY v0.x"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull ][.v0_x_ notNull groupKey complemented [v0.x notNull "
                                        + "[.x notNull tableKey [T.x notNull tableKey]] "
                                        + "[.x1 notNull tableKey [V.x1 notNull tableKey]]]]"
                                        + "[.RESULT_COUNT_ notNull countKey complemented [v0.T_x_ tableKey complemented "
                                        + "[.T_x_ notNull tableKey complemented [T.x notNull tableKey]] "
                                        + "[.T_x_ complemented ]]]"
                                        + "[.RESULT_COUNT__ notNull countKey complemented [v0.V_x1_ tableKey complemented "
                                        + "[.V_x1_ complemented ] [.V_x1_ notNull tableKey complemented "
                                        + "[V.x1 notNull tableKey]]]]"
                                        + "[.RESULT_COUNT___ notNull countKey complemented [v1.T_x_ tableKey complemented "
                                        + "[.T_x_ notNull tableKey complemented [T.x notNull tableKey]] "
                                        + "[.T_x_ complemented ]]]"
                                        + "[.RESULT_COUNT____ notNull countKey complemented [v1.V_x1_ tableKey complemented "
                                        + "[.V_x1_ complemented ] [.V_x1_ notNull tableKey complemented "
                                        + "[V.x1 notNull tableKey]]]]"));
    }

    [Test]
    public void UnionAllAggregateFunc() {
      Result result;

      result = this.GetResultInfoList(@"select max(z) from T "
                                    + "union all "
                                    + "select sum(z) from U");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT max(z),COUNT(T.x) AS RESULT_COUNT_,NULL AS RESULT_COUNT__ "
                                        + "FROM T "
                                        + "UNION ALL "
                                        + "SELECT sum(z),NULL AS RESULT_COUNT_,COUNT(U.x) AS RESULT_COUNT__ "
                                        + "FROM U"));
      Assert.That(result.Info, Is.EqualTo(@"[. [. ] [. ]]"
                                         + "[.RESULT_COUNT_ countKey complemented [.RESULT_COUNT_ notNull countKey complemented [T.x notNull tableKey]] "
                                         + "[.RESULT_COUNT_ complemented ]][.RESULT_COUNT__ countKey complemented "
                                         + "[.RESULT_COUNT__ complemented ] [.RESULT_COUNT__ notNull countKey complemented [U.x notNull tableKey]]]"));

      result = this.GetResultInfoList(@"select count(x) from T "
                                    + "union all "
                                    + "select x from U");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT count(x),count(x) AS T_RESULT_COUNT_,NULL AS U_x_ FROM T"
                                        + " UNION ALL SELECT x,NULL AS T_RESULT_COUNT_,x AS U_x_ FROM U"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull [. notNull countKey [T.x notNull tableKey]] [.x notNull tableKey [U.x notNull tableKey]]]"
                                          + "[.T_RESULT_COUNT_ countKey complemented [.T_RESULT_COUNT_ notNull countKey complemented [T.x notNull tableKey]] "
                                          + "[.T_RESULT_COUNT_ complemented ]][.U_x_ tableKey complemented [.U_x_ complemented ] "
                                          + "[.U_x_ notNull tableKey complemented [U.x notNull tableKey]]]"));

      result = this.GetResultInfoList(@"select count(z) from T "
                                    + "union all "
                                    + "select z from U");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT count(z),COUNT(T.x) AS RESULT_COUNT_,NULL AS U_x_ FROM T"
                                        + " UNION ALL "
                                        + "SELECT z,NULL AS RESULT_COUNT_,U.x AS U_x_ FROM U"));
      Assert.That(result.Info, Is.EqualTo(@"[. [. notNull [T.z]] [.z [U.z]]]"
                                        + "[.RESULT_COUNT_ countKey complemented "
                                        + "[.RESULT_COUNT_ notNull countKey complemented [T.x notNull tableKey]] "
                                        + "[.RESULT_COUNT_ complemented ]]"
                                        + "[.U_x_ tableKey complemented [.U_x_ complemented ] "
                                        + "[.U_x_ notNull tableKey complemented [U.x notNull tableKey]]]"));

      result = this.GetResultInfoList(@"select z from U "
                                    + "union all "
                                    + "select count(z) from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT z,U.x AS U_x_,NULL AS RESULT_COUNT_ FROM U"
                                        + " UNION ALL "
                                        + "SELECT count(z),NULL AS U_x_,COUNT(T.x) AS RESULT_COUNT_ FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[.z [.z [U.z]] [. notNull [T.z]]]"
                                        + "[.U_x_ tableKey complemented [.U_x_ notNull tableKey complemented "
                                        + "[U.x notNull tableKey]] [.U_x_ complemented ]]"
                                        + "[.RESULT_COUNT_ countKey complemented [.RESULT_COUNT_ complemented ] "
                                        + "[.RESULT_COUNT_ notNull countKey complemented [T.x notNull tableKey]]]"));

      result = this.GetResultInfoList(@"select max(x) from T "
                                    + "union all "
                                    + "select sum(x) from U");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT max(x),COUNT(T.x) AS RESULT_COUNT_,NULL AS RESULT_COUNT__ FROM T"
                                        + " UNION ALL "
                                        + "SELECT sum(x),NULL AS RESULT_COUNT_,COUNT(U.x) AS RESULT_COUNT__ FROM U"));
      Assert.That(result.Info, Is.EqualTo(@"[. [. ] [. ]]"
                                        + "[.RESULT_COUNT_ countKey complemented "
                                        + "[.RESULT_COUNT_ notNull countKey complemented [T.x notNull tableKey]] "
                                        + "[.RESULT_COUNT_ complemented ]][.RESULT_COUNT__ countKey complemented "
                                        + "[.RESULT_COUNT__ complemented ] "
                                        + "[.RESULT_COUNT__ notNull countKey complemented [U.x notNull tableKey]]]"));

    }

    [Test]
    public void UnionAllDistinct() {
      Result result;

      result = this.GetResultInfoList(@"select distinct x from T "
                                    + "union all "
                                    + "select x from U");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT DISTINCT x,x AS T_x_,NULL AS U_x_ FROM T "
                                        + "UNION ALL "
                                        + "SELECT x,NULL AS T_x_,x AS U_x_ FROM U"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull [.x notNull groupKey [T.x notNull tableKey]] [.x notNull tableKey "
                                          + "[U.x notNull tableKey]]]"
                                          + "[.T_x_ groupKey complemented [.T_x_ notNull groupKey complemented [T.x notNull tableKey]] "
                                          + "[.T_x_ complemented ]]"
                                          + "[.U_x_ tableKey complemented [.U_x_ complemented ] [.U_x_ notNull tableKey complemented "
                                          + "[U.x notNull tableKey]]]"));

      result = this.GetResultInfoList(@"select x from T "
                                    + "union all "
                                    + "select distinct x from U");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT x,x AS T_x_,NULL AS U_x_ FROM T "
                                        + "UNION ALL "
                                        + "SELECT DISTINCT x,NULL AS T_x_,x AS U_x_ FROM U"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull [.x notNull tableKey [T.x notNull tableKey]] [.x notNull groupKey "
                                          + "[U.x notNull tableKey]]]"
                                          + "[.T_x_ tableKey complemented [.T_x_ notNull tableKey complemented [T.x notNull tableKey]] "
                                          + "[.T_x_ complemented ]]"
                                          + "[.U_x_ groupKey complemented [.U_x_ complemented ] [.U_x_ notNull groupKey complemented "
                                          + "[U.x notNull tableKey]]]"));

      result = this.GetResultInfoList(@"select distinct x from T "
                                     + "union all "
                                     + "select distinct x from U");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT DISTINCT x,x AS T_x_,NULL AS U_x_ FROM T "
                                        + "UNION ALL "
                                        + "SELECT DISTINCT x,NULL AS T_x_,x AS U_x_ FROM U"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull [.x notNull groupKey [T.x notNull tableKey]] [.x notNull groupKey "
                                          + "[U.x notNull tableKey]]]"
                                          + "[.T_x_ groupKey complemented [.T_x_ notNull groupKey complemented [T.x notNull tableKey]] "
                                          + "[.T_x_ complemented ]]"
                                          + "[.U_x_ groupKey complemented [.U_x_ complemented ] [.U_x_ notNull groupKey complemented "
                                          + "[U.x notNull tableKey]]]"));

      result = this.GetResultInfoList(@"select distinct y from T "
                                      + "union all "
                                      + "select y from U");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT DISTINCT y,NULL AS U_x_,y AS T_y_ FROM T "
                                        + "UNION ALL "
                                        + "SELECT y,U.x AS U_x_,NULL AS T_y_ FROM U"));
      Assert.That(result.Info, Is.EqualTo(@"[.y notNull [.y notNull groupKey [T.y notNull]] [.y notNull "
                                          + "[U.y notNull]]]"
                                          + "[.U_x_ tableKey complemented [.U_x_ complemented ] "
                                          + "[.U_x_ notNull tableKey complemented [U.x notNull tableKey]]]"
                                          + "[.T_y_ groupKey complemented [.T_y_ notNull groupKey complemented [T.y notNull]] "
                                          + "[.T_y_ complemented ]]"));

      result = this.GetResultInfoList(@"select distinct y from T "
                                     + "union all "
                                     + "select distinct y from U");  
      Assert.That(result.Sql, Is.EqualTo(@"SELECT DISTINCT y,y AS T_y_,NULL AS U_y_ FROM T "
                                        + "UNION ALL "
                                        + "SELECT DISTINCT y,NULL AS T_y_,y AS U_y_ FROM U"));
      Assert.That(result.Info, Is.EqualTo(@"[.y notNull [.y notNull groupKey [T.y notNull]] [.y notNull groupKey "
                                          + "[U.y notNull]]]"
                                          + "[.T_y_ groupKey complemented [.T_y_ notNull groupKey complemented [T.y notNull]] "
                                          + "[.T_y_ complemented ]]"
                                          + "[.U_y_ groupKey complemented [.U_y_ complemented ] "
                                          + "[.U_y_ notNull groupKey complemented [U.y notNull]]]"));

      result = this.GetResultInfoList(@"select distinct z from T "
                                    + "union all "
                                    + "select z from U");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT DISTINCT z,1 AS RESULT_EXISTS_,NULL AS U_x_,z AS T_z_ FROM T "
                                        + "UNION ALL "
                                        + "SELECT z,NULL AS RESULT_EXISTS_,U.x AS U_x_,NULL AS T_z_ FROM U"));
      Assert.That(result.Info, Is.EqualTo(@"[.z [.z groupKey [T.z]] [.z [U.z]]]"
                                          + "[.RESULT_EXISTS_ countKey complemented [.RESULT_EXISTS_ notNull countKey complemented ] "
                                          + "[.RESULT_EXISTS_ complemented ]]"
                                          + "[.U_x_ tableKey complemented [.U_x_ complemented ] "
                                          + "[.U_x_ notNull tableKey complemented [U.x notNull tableKey]]]"
                                          + "[.T_z_ groupKey complemented [.T_z_ groupKey complemented [T.z]] "
                                          + "[.T_z_ complemented ]]"
                                          ));

      result = this.GetResultInfoList(@"select distinct z from T "
                                     + "union all "
                                     + "select distinct z from U");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT DISTINCT z,1 AS RESULT_EXISTS_,NULL AS RESULT_EXISTS__,z AS T_z_,NULL AS U_z_ FROM T "
                                          + "UNION ALL "
                                          + "SELECT DISTINCT z,NULL AS RESULT_EXISTS_,1 AS RESULT_EXISTS__,NULL AS T_z_,z AS U_z_ FROM U"));
      Assert.That(result.Info, Is.EqualTo(@"[.z [.z groupKey [T.z]] [.z groupKey [U.z]]]"
                                          + "[.RESULT_EXISTS_ countKey complemented [.RESULT_EXISTS_ notNull countKey complemented ] "
                                          + "[.RESULT_EXISTS_ complemented ]]"
                                          + "[.RESULT_EXISTS__ countKey complemented [.RESULT_EXISTS__ complemented ] "
                                          + "[.RESULT_EXISTS__ notNull countKey complemented ]]"
                                          + "[.T_z_ groupKey complemented [.T_z_ groupKey complemented [T.z]] "
                                          + "[.T_z_ complemented ]]"
                                          + "[.U_z_ groupKey complemented [.U_z_ complemented ] "
                                          + "[.U_z_ groupKey complemented [U.z]]]"));

      result = this.GetResultInfoList(@"select distinct * from"
                                      + " (select y from T"
                                      + "  union all"
                                      + "  select y from U) v");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT DISTINCT * FROM "
                                        + "(SELECT y FROM T UNION ALL SELECT y FROM U)v"));
      Assert.That(result.Info, Is.EqualTo(@"[.y notNull groupKey [v.y notNull [.y notNull [T.y notNull]] "
                                          + "[.y notNull [U.y notNull]]]]"));

      result = this.GetResultInfoList(@"select distinct z from"
                                      + " (select z from T"
                                      + "  union all"
                                      + "  select z from U) v");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT DISTINCT z,1 AS RESULT_EXISTS_ FROM "
                                        + "(SELECT z FROM T UNION ALL SELECT z FROM U)v"));
      Assert.That(result.Info, Is.EqualTo(@"[.z groupKey [v.z [.z [T.z]] [.z [U.z]]]]"
                                         + "[.RESULT_EXISTS_ notNull countKey complemented ]"));
    }

    [Test]
    public void UnionAllWithoutFrom() {
      Result result;

      result = this.GetResultInfoList(@"select T.x union all select U.x");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT T.x UNION ALL SELECT U.x"));
      Assert.That(result.Info, Is.EqualTo(@"[.x [.x ] [.x ]]"));

      result = this.GetResultInfoList(@"select T.x union all select U.x from U");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT T.x,NULL AS U_x_ UNION ALL "
                                         + "SELECT U.x,U.x AS U_x_ FROM U"));
      Assert.That(result.Info, Is.EqualTo(@"[.x [.x ] [.x notNull tableKey [U.x notNull tableKey]]]"
                                         + "[.U_x_ tableKey complemented [.U_x_ complemented ] "
                                         + "[.U_x_ notNull tableKey complemented [U.x notNull tableKey]]]"));

      result = this.GetResultInfoList(@"select 1 from T union all select 2");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 1,T.x AS T_x_ FROM T UNION ALL SELECT 2,NULL AS T_x_"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull [. notNull ] [. notNull ]]"
                                        + "[.T_x_ tableKey complemented [.T_x_ notNull tableKey complemented "
                                        + "[T.x notNull tableKey]] [.T_x_ complemented ]]"));

    }

    [Test]
    public void NameCollision() {
      Result result;

      result = this.GetResultInfoList(@"select null as T_x_ from (select z from T) v");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT NULL AS T_x_,v.T_x_ AS T_x__ FROM "
                                         + "(SELECT z,T.x AS T_x_ FROM T)v"));
      Assert.That(result.Info, Is.EqualTo(@"[.T_x_ ]"
                                         + "[.T_x__ notNull tableKey complemented [v.T_x_ notNull tableKey complemented "
                                         + "[T.x notNull tableKey]]](.z [v.z [T.z]])"));


      result = this.GetResultInfoList(@"select null as T_x_,null as T_x__ from (select z from T) v");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT NULL AS T_x_,NULL AS T_x__,v.T_x_ AS T_x___"
                                         + " FROM (SELECT z,T.x AS T_x_ FROM T)v"));
      Assert.That(result.Info, Is.EqualTo(@"[.T_x_ ][.T_x__ ]"
                                         + "[.T_x___ notNull tableKey complemented [v.T_x_ notNull tableKey complemented "
                                         + "[T.x notNull tableKey]]](.z [v.z [T.z]])"));

      result = this.GetResultInfoList(@"select v.y,v.z,v.y,v.z from (select y,z from T) v"
                                  + " union all select 1,2,1,2 from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT v.y,v.z,v.y,v.z,v.T_x_ AS T_x_,NULL AS T_x__ "
                                        + "FROM (SELECT y,z,T.x AS T_x_ FROM T)v "
                                        + "UNION ALL "
                                        + "SELECT 1,2,1,2,NULL AS T_x_,T.x AS T_x__ FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[.y notNull [.y notNull [v.y notNull [T.y notNull]]] [. notNull ]]"
                                          + "[.z [.z [v.z [T.z]]] [. notNull ]]"
                                          + "[.y notNull [.y notNull [v.y notNull [T.y notNull]]] [. notNull ]]"
                                          + "[.z [.z [v.z [T.z]]] [. notNull ]]"
                                          + "[.T_x_ tableKey complemented [.T_x_ notNull tableKey complemented "
                                          + "[v.T_x_ notNull tableKey complemented [T.x notNull tableKey]]] "
                                          + "[.T_x_ complemented ]][.T_x__ tableKey complemented "
                                          + "[.T_x__ complemented ] [.T_x__ notNull tableKey complemented "
                                          + "[T.x notNull tableKey]]]"));

      result = this.GetResultInfoList(@"select * from (select 1 from T) onet  left join "
                                  + " (select 2 from U) two on 1 = 1 union all "
                                  + "select * from (select 1 from T) onet  left join "
                                  + " (select 2 from U) two  on  1 = 1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT onet.onet_col0_,two.two_col1_,onet.T_x_ AS T_x_,"
                                        + "NULL AS T_x__,two.U_x_ AS U_x_,NULL AS U_x__ "
                                        + "FROM (SELECT 1 AS onet_col0_,T.x AS T_x_ FROM T)onet "
                                        + "LEFT JOIN "
                                        + "(SELECT 2 AS two_col1_,U.x AS U_x_ FROM U)two "
                                        + "ON 1=1 "
                                        + "UNION ALL "
                                        + "SELECT onet.onet_col0_,two.two_col1_,NULL AS T_x_,"
                                        + "onet.T_x_ AS T_x__,NULL AS U_x_,two.U_x_ AS U_x__ "
                                        + "FROM (SELECT 1 AS onet_col0_,T.x AS T_x_ FROM T)onet "
                                        + "LEFT JOIN "
                                        + "(SELECT 2 AS two_col1_,U.x AS U_x_ FROM U)two "
                                        + "ON 1=1"));
      Assert.That(result.Info, Is.EqualTo(@"[.onet_col0_ notNull [.onet_col0_ notNull [onet.onet_col0_ notNull ]] "
                                        + "[.onet_col0_ notNull [onet.onet_col0_ notNull ]]][.two_col1_ [.two_col1_ "
                                        + "[two.two_col1_ notNull ]] [.two_col1_ [two.two_col1_ notNull ]]]"
                                        + "[.T_x_ tableKey complemented [.T_x_ notNull tableKey complemented "
                                        + "[onet.T_x_ notNull tableKey complemented [T.x notNull tableKey]]] [.T_x_ complemented ]]"
                                        + "[.T_x__ tableKey complemented [.T_x__ complemented ] [.T_x__ notNull tableKey complemented "
                                        + "[onet.T_x_ notNull tableKey complemented [T.x notNull tableKey]]]][.U_x_ tableKey complemented "
                                        + "[.U_x_ tableKey complemented [two.U_x_ notNull tableKey complemented [U.x notNull tableKey]]] "
                                        + "[.U_x_ complemented ]][.U_x__ tableKey complemented [.U_x__ complemented ] "
                                        + "[.U_x__ tableKey complemented [two.U_x_ notNull tableKey complemented [U.x notNull tableKey]]]]"));

      result = this.GetResultInfoList(@"select * from (select x from T) t1 left join U on t1.x = U.x");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT * FROM (SELECT x FROM T)t1 LEFT JOIN U ON t1.x=U.x"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull tableKey [t1.x notNull tableKey [T.x notNull tableKey]]]"
                                         + "[.x tableKey [U.x notNull tableKey]][.y [U.y notNull]][.z "
                                         + "[U.z]]"));

      // 大文字小文字の違いを無視する場合
      result = this.GetResultInfoList(@"select 0 as t_x_ from T");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 0 AS t_x_,T.x AS T_x__ FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[.t_x_ notNull ][.T_x__ notNull tableKey complemented [T.x notNull tableKey]]"
                                         + "(.y notNull [T.y notNull])(.z [T.z])"));

      // 大文字小文字の違いを無視しない場合
      result = this.GetResultInfoList(@"select 0 as t_x_ from T", false);
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 0 AS t_x_,T.x AS T_x_ FROM T"));
      Assert.That(result.Info, Is.EqualTo(@"[.t_x_ notNull ][.T_x_ notNull tableKey complemented [T.x notNull tableKey]]"
                                         + "(.y notNull [T.y notNull])(.z [T.z])"));

      // SELECT句内の列名が重複している場合
      result = this.GetResultInfoList("select V.*, V.* from (select y, z from T) V " +
                                       "union all " +
                                       "select 1, 2, 1, 2 from U");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT V.y,V.z,V.y,V.z,V.T_x_ AS T_x_,V.T_x_ AS T_x__,NULL AS U_x_ " +
                                          "FROM (SELECT y,z,T.x AS T_x_ FROM T)V " +
                                          "UNION ALL " +
                                          "SELECT 1,2,1,2,NULL AS T_x_,NULL AS T_x__,U.x AS U_x_ FROM U"));
      Assert.That(result.Info, Is.EqualTo(@"[.y notNull [.y notNull [V.y notNull [T.y notNull]]] [. notNull ]]"
                                        + "[.z [.z [V.z [T.z]]] [. notNull ]]"
                                        + "[.y notNull [.y notNull [V.y notNull [T.y notNull]]] [. notNull ]]"
                                        + "[.z [.z [V.z [T.z]]] [. notNull ]]"
                                        + "[.T_x_ tableKey complemented [.T_x_ notNull tableKey complemented "
                                        + "[V.T_x_ notNull tableKey complemented [T.x notNull tableKey]]] "
                                        + "[.T_x_ complemented ]][.T_x__ tableKey complemented "
                                        + "[.T_x__ notNull tableKey complemented [V.T_x_ notNull tableKey complemented "
                                        + "[T.x notNull tableKey]]] [.T_x__ complemented ]][.U_x_ tableKey complemented "
                                        + "[.U_x_ complemented ] [.U_x_ notNull tableKey complemented [U.x notNull tableKey]]]"));

      // 既存のAS別名と補完SELECT句のAS別名は重複しないこと
      result = this.GetResultInfoList("select 0,0,0 from T " +
                                   "union all " +
                                   "select x as x__, x as x, x as x_ from U");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 0,0,0"
                                        + ",T.x AS T_x_,NULL AS U_x___,NULL AS U_x_,NULL AS U_x__ "
                                        + "FROM T "
                                        + "UNION ALL "
                                        + "SELECT x AS x__,x AS x,x AS x_"
                                        + ",NULL AS T_x_,x AS U_x___,x AS U_x_,x AS U_x__ "
                                        + "FROM U"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull [. notNull ] [.x__ notNull tableKey [U.x notNull tableKey]]]"
                                        + "[. notNull [. notNull ] [.x notNull tableKey [U.x notNull tableKey]]]"
                                        + "[. notNull [. notNull ] [.x_ notNull tableKey [U.x notNull tableKey]]]"
                                        + "[.T_x_ tableKey complemented [.T_x_ notNull tableKey complemented "
                                        + "[T.x notNull tableKey]] [.T_x_ complemented ]]"
                                        + "[.U_x___ tableKey complemented [.U_x___ complemented ] "
                                        + "[.U_x___ notNull tableKey complemented "
                                        + "[U.x notNull tableKey]]][.U_x_ tableKey complemented [.U_x_ complemented ] "
                                        + "[.U_x_ notNull tableKey complemented [U.x notNull tableKey]]]"
                                        + "[.U_x__ tableKey complemented [.U_x__ complemented ] "
                                        + "[.U_x__ notNull tableKey complemented [U.x notNull tableKey]]]"));


      result = this.GetResultInfoList("select 0,0,0 from T "
                                  + "union all "
                                  + "select x as U_x___, x as U_x_, x as U_x__ from U");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 0,0,0"
                                        + ",T.x AS T_x_,NULL AS U_U_x____,NULL AS U_U_x__,NULL AS U_U_x___ "
                                        + "FROM T "
                                        + "UNION ALL "
                                        + "SELECT x AS U_x___,x AS U_x_,x AS U_x__"
                                        + ",NULL AS T_x_,x AS U_U_x____,x AS U_U_x__,x AS U_U_x___ "
                                        + "FROM U"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull [. notNull ] [.U_x___ notNull tableKey [U.x notNull tableKey]]]"
                                        + "[. notNull [. notNull ] [.U_x_ notNull tableKey [U.x notNull tableKey]]]"
                                        + "[. notNull [. notNull ] [.U_x__ notNull tableKey [U.x notNull tableKey]]]"
                                        + "[.T_x_ tableKey complemented [.T_x_ notNull tableKey complemented "
                                        + "[T.x notNull tableKey]] [.T_x_ complemented ]]"
                                        + "[.U_U_x____ tableKey complemented [.U_U_x____ complemented ] "
                                        + "[.U_U_x____ notNull tableKey complemented [U.x notNull tableKey]]]"
                                        + "[.U_U_x__ tableKey complemented [.U_U_x__ complemented ] "
                                        + "[.U_U_x__ notNull tableKey complemented [U.x notNull tableKey]]]"
                                        + "[.U_U_x___ tableKey complemented [.U_U_x___ complemented ] "
                                        + "[.U_U_x___ notNull tableKey complemented [U.x notNull tableKey]]]"));

      result = this.GetResultInfoList("select 0,0,0 from T "
                                  + "union all "
                                  + "select x as U_U_x___, x as U_U_x_, x as U_U_x__ from U");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT 0,0,0"
                                        + ",T.x AS T_x_,NULL AS U_U_U_x____,NULL AS U_U_U_x__,NULL AS U_U_U_x___ "
                                        + "FROM T "
                                        + "UNION ALL "
                                        + "SELECT x AS U_U_x___,x AS U_U_x_,x AS U_U_x__"
                                        + ",NULL AS T_x_,x AS U_U_U_x____,x AS U_U_U_x__,x AS U_U_U_x___ "
                                        + "FROM U"));
      Assert.That(result.Info, Is.EqualTo(@"[. notNull [. notNull ] [.U_U_x___ notNull tableKey [U.x notNull tableKey]]]"
                                        + "[. notNull [. notNull ] [.U_U_x_ notNull tableKey [U.x notNull tableKey]]]"
                                        + "[. notNull [. notNull ] [.U_U_x__ notNull tableKey [U.x notNull tableKey]]]"
                                        + "[.T_x_ tableKey complemented [.T_x_ notNull tableKey complemented "
                                        + "[T.x notNull tableKey]] [.T_x_ complemented ]]"
                                        + "[.U_U_U_x____ tableKey complemented [.U_U_U_x____ complemented ] "
                                        + "[.U_U_U_x____ notNull tableKey complemented "
                                        + "[U.x notNull tableKey]]][.U_U_U_x__ tableKey complemented "
                                        + "[.U_U_U_x__ complemented ] [.U_U_U_x__ notNull tableKey complemented "
                                        + "[U.x notNull tableKey]]][.U_U_U_x___ tableKey complemented "
                                        + "[.U_U_U_x___ complemented ] [.U_U_U_x___ notNull tableKey complemented "
                                        + "[U.x notNull tableKey]]]"));
    }

    [Test]
    public void Bracket() {
      Result result;

      result = this.GetResultInfoList(@"(select ((((x)))) ,y from ((select * from T)))");
      Assert.That(result.Sql, Is.EqualTo(@"(SELECT ((((x)))),y FROM ((SELECT * FROM T)))"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull tableKey [.x notNull tableKey [T.x notNull tableKey]]]"
                                         + "[.y notNull [.y notNull [T.y notNull]]]"
                                         + "(.z [.z [T.z]])"));

      result = this.GetResultInfoList(@"select (t1.x) ,(null), ('abc'), (1) from"
                                  + "  ((select * from T)) t1 left join U on 1=1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT (t1.x),(NULL),('abc'),(1),U.x AS U_x_"
                                        + " FROM ((SELECT * FROM T))t1 LEFT JOIN U ON 1=1"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull tableKey [t1.x notNull tableKey [T.x notNull tableKey]]][. ]"
                                         + "[. notNull ][. notNull ]"
                                         + "[.U_x_ tableKey complemented [U.x notNull tableKey]]"
                                         + "(.y notNull [t1.y notNull [T.y notNull]])(.z [t1.z [T.z]])"
                                         + "(.y [U.y notNull])(.z [U.z])"));

      result = this.GetResultInfoList(@"select (v1.x) ,(null), ('abc'), (1) from"
                                   + "  ((select * from T) t1) v1 left join (U) on 1=1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT (v1.x),(NULL),('abc'),(1),U.x AS U_x_"
                                        + " FROM ((SELECT * FROM T)t1)v1  LEFT JOIN (U) ON 1=1"));
      Assert.That(result.Info, Is.EqualTo(@"[.x notNull tableKey [v1.x notNull tableKey [T.x notNull tableKey]]][. ]"
                                         + "[. notNull ][. notNull ]"
                                         + "[.U_x_ tableKey complemented [U.x notNull tableKey]]"
                                         + "(.y notNull [v1.y notNull [T.y notNull]])(.z [v1.z [T.z]])"
                                         + "(.y [U.y notNull])(.z [U.z])"));
    }

    [Test]
    public void CaseSensitivity() {
      Result result;

      // Table名の一致はBestCase
      // (テーブルTBLとTblで大文字小文字違いを複数登録しているので、ignoreCaseで一致しない)
      result = this.GetResultInfoList(@"select * from Tbl full join TBL on 1=1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT * FROM Tbl FULL JOIN TBL ON 1=1"));
      Assert.That(result.Info, Is.EqualTo(@"[.COL tableKey [Tbl.COL notNull tableKey]]"
                                         + "[.col tableKey [Tbl.col notNull tableKey]]"
                                         + "[.COLUMN tableKey [TBL.COLUMN notNull tableKey]]"
                                         + "[.column tableKey [TBL.column notNull tableKey]]"));

      result = this.GetResultInfoList(@"select * from Tbl full join tbl on 1=1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT * FROM Tbl FULL JOIN tbl ON 1=1"));
      Assert.That(result.Info, Is.EqualTo(@"[.COL tableKey [Tbl.COL notNull tableKey]]"
                                         + "[.col tableKey [Tbl.col notNull tableKey]]"));

      result = this.GetResultInfoList(@"select * from TBL");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT * FROM TBL"));
      Assert.That(result.Info, Is.EqualTo(@"[.COLUMN notNull tableKey [TBL.COLUMN notNull tableKey]]"
                                         + "[.column notNull tableKey [TBL.column notNull tableKey]]"));

      result = this.GetResultInfoList(@"select * from Tbl");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT * FROM Tbl"));
      Assert.That(result.Info, Is.EqualTo(@"[.COL notNull tableKey [Tbl.COL notNull tableKey]]"
                                         + "[.col notNull tableKey [Tbl.col notNull tableKey]]"));

      result = this.GetResultInfoList(@"select * from tbl");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT * FROM tbl"));
      Assert.That(result.Info, Is.EqualTo(@""));

      // Table名の一致はBestCase
      // (テーブルTは大文字小文字違いで複数登録していないので、ignoreCaseで一致する)
      result = this.GetResultInfoList(@"select * from T full join t on 1=1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT * FROM T FULL JOIN t ON 1=1"));
      Assert.That(result.Info, Is.EqualTo(@"[.x tableKey [T.x notNull tableKey]][.y [T.y notNull]]"
                                         + "[.z [T.z]][.x tableKey [T.x notNull tableKey]]"
                                         + "[.y [T.y notNull]][.z [T.z]]"));

      result = this.GetResultInfoList(@"select * from t full join t on 1=1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT * FROM t FULL JOIN t ON 1=1"));
      Assert.That(result.Info, Is.EqualTo(@"[.x tableKey [T.x notNull tableKey]][.y [T.y notNull]]"
                                         + "[.z [T.z]][.x tableKey [T.x notNull tableKey]]"
                                         + "[.y [T.y notNull]][.z [T.z]]"));

      // Table Wildcardの一致 (ignoreCase)
      Assert.Throws<InvalidASTStructureError>(() => {
        this.GetResultInfoList(@"select Tbl.*, TBL.* from Tbl full join TBL on 1=1");
      });
      
      Assert.Throws<InvalidASTStructureError>(() => {
        this.GetResultInfoList(@"select tbl.* from Tbl full join TBL on 1=1");
      });

      result = this.GetResultInfoList(@"select tbl.* from TBL");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT tbl.* FROM TBL"));

      result = this.GetResultInfoList(@"select tbl.* from Tbl");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT tbl.* FROM Tbl"));

      Assert.Throws<InvalidASTStructureError>(() => {
        this.GetResultInfoList(@"select t.* from T full join t on 1=1");
      });

      // Table Aliasの一致 (ignoreCase)
      result = this.GetResultInfoList(@"select t1.* from TBL T1");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT t1.* FROM TBL T1"));
      Assert.That(result.Info, Is.EqualTo(@"[.COLUMN notNull tableKey [T1.COLUMN notNull tableKey]]"
                                         + "[.column notNull tableKey [T1.column notNull tableKey]]"));
      
      Assert.Throws<InvalidASTStructureError>(() => {
        this.GetResultInfoList(@"select t.* from TBL T cross join T");
      });

      Assert.Throws<InvalidASTStructureError>(() => {
        this.GetResultInfoList(@"select t.* from TBL t cross join T");
      });

      // Columnの一致 (ignoreCase)
      Assert.Throws<InvalidASTStructureError>(() => { this.GetResultInfoList(@"select COLUMN from TBL"); });

      Assert.Throws<InvalidASTStructureError>(() => { this.GetResultInfoList(@"select Column from TBL"); });

      Assert.Throws<InvalidASTStructureError>(() => { this.GetResultInfoList(@"select cOlUmN from TBL"); });

      Assert.Throws<InvalidASTStructureError>(() => { this.GetResultInfoList(@"select t.cOlUmN from TBL t"); });

      // Column Aliasの一致 (ignoreCase)
      result = this.GetResultInfoList(@"select V.ABC from (select x as ABC from T) V");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT V.ABC FROM (SELECT x AS ABC FROM T)V"));
      Assert.That(result.Info, Is.EqualTo(@"[.ABC notNull tableKey [V.ABC notNull tableKey "
                                         + "[T.x notNull tableKey]]]"));

      result = this.GetResultInfoList(@"select V.abc from (select X as ABC from T) V");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT V.abc FROM (SELECT X AS ABC FROM T)V"));
      Assert.That(result.Info, Is.EqualTo(@"[.abc notNull tableKey [V.ABC notNull tableKey "
                                         + "[T.x notNull tableKey]]]"));

      result = this.GetResultInfoList(@"select v.ABC from (select X as abc from T) V");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT v.ABC FROM (SELECT X AS abc FROM T)V"));
      Assert.That(result.Info, Is.EqualTo(@"[.ABC notNull tableKey [V.abc notNull tableKey "
                                         + "[T.x notNull tableKey]]]"));


      // Table Wildcardの一致 (ignoreCase=false)
      result = this.GetResultInfoList(@"select Tbl.*, TBL.* from Tbl full join TBL on 1=1", false);
      Assert.That(result.Sql, Is.EqualTo(@"SELECT Tbl.*,TBL.* FROM Tbl FULL JOIN TBL ON 1=1"));
      Assert.That(result.Info, Is.EqualTo(@"[.COL tableKey [Tbl.COL notNull tableKey]]"
                                         + "[.col tableKey [Tbl.col notNull tableKey]]"
                                         + "[.COLUMN tableKey [TBL.COLUMN notNull tableKey]]"
                                         + "[.column tableKey [TBL.column notNull tableKey]]"));

      result = this.GetResultInfoList(@"select tbl.* from Tbl full join TBL on 1=1", false);
      Assert.That(result.Sql, Is.EqualTo(@"SELECT tbl.*" + 
                                          ",Tbl.COL AS Tbl_COL_" +
                                          ",Tbl.col AS Tbl_col_" +
                                          ",TBL.COLUMN AS TBL_COLUMN_" +
                                          ",TBL.column AS TBL_column_ " + 
                                          "FROM Tbl FULL JOIN TBL ON 1=1"));
      Assert.That(result.Info, Is.EqualTo(@"[.Tbl_COL_ tableKey complemented [Tbl.COL notNull tableKey]]"
                                         + "[.Tbl_col_ tableKey complemented [Tbl.col notNull tableKey]]"
                                         + "[.TBL_COLUMN_ tableKey complemented [TBL.COLUMN notNull tableKey]]"
                                         + "[.TBL_column_ tableKey complemented [TBL.column notNull tableKey]]"));

      result = this.GetResultInfoList(@"select tbl.* from TBL", false);
      Assert.That(result.Sql, Is.EqualTo(@"SELECT tbl.*" +
                                         ",TBL.COLUMN AS TBL_COLUMN_" +
                                         ",TBL.column AS TBL_column_ " +
                                         "FROM TBL"));
      Assert.That(result.Info, Is.EqualTo(@"[.TBL_COLUMN_ notNull tableKey complemented [TBL.COLUMN notNull tableKey]]"
                                         + "[.TBL_column_ notNull tableKey complemented [TBL.column notNull tableKey]]"));

      result = this.GetResultInfoList(@"select tbl.* from Tbl", false);
      Assert.That(result.Sql, Is.EqualTo(@"SELECT tbl.*" +
                                          ",Tbl.COL AS Tbl_COL_" +
                                          ",Tbl.col AS Tbl_col_ " +
                                          "FROM Tbl"));
      Assert.That(result.Info, Is.EqualTo(@"[.Tbl_COL_ notNull tableKey complemented [Tbl.COL notNull tableKey]]"
                                         + "[.Tbl_col_ notNull tableKey complemented [Tbl.col notNull tableKey]]"));

      result = this.GetResultInfoList(@"select t.* from T full join t on 1=1", false);
      // (TableWildcardが複数の抽出元テーブルに一致してしまうが、
      //  現在のGetSelectItemInfoVisitorではこれをエラーとすることができない)

      // Table Aliasの一致 (ignoreCase=false)
      result = this.GetResultInfoList(@"select t1.* from TBL T1", false);
      Assert.That(result.Sql, Is.EqualTo(@"SELECT t1.*,T1.COLUMN AS T1_COLUMN_,T1.column AS T1_column_ FROM TBL T1"));
      Assert.That(result.Info, Is.EqualTo(@"[.T1_COLUMN_ notNull tableKey complemented [T1.COLUMN notNull tableKey]]"
                                         + "[.T1_column_ notNull tableKey complemented [T1.column notNull tableKey]]"));

      result = this.GetResultInfoList(@"select t.* from TBL T cross join T", false);
      Assert.That(result.Sql, Is.EqualTo(@"SELECT t.*"
                                         + ",T.COLUMN AS T_COLUMN_"
                                         + ",T.column AS T_column_"
                                         + ",T.x AS T_x_ "
                                         + "FROM TBL T CROSS JOIN T"));
      Assert.That(result.Info, Is.EqualTo(@"[.T_COLUMN_ notNull tableKey complemented [T.COLUMN notNull tableKey]]"
                                         + "[.T_column_ notNull tableKey complemented [T.column notNull tableKey]]"
                                         + "[.T_x_ notNull tableKey complemented [T.x notNull tableKey]]"
                                         + "(.y notNull [T.y notNull])(.z [T.z])"));

      result = this.GetResultInfoList(@"select t.* from TBL t cross join T", false);
      Assert.That(result.Sql, Is.EqualTo(@"SELECT t.*"
                                         + ",T.x AS T_x_ "
                                         + "FROM TBL t CROSS JOIN T"));
      Assert.That(result.Info, Is.EqualTo(@"[.COLUMN notNull tableKey [t.COLUMN notNull tableKey]]"
                                         + "[.column notNull tableKey [t.column notNull tableKey]]"
                                         + "[.T_x_ notNull tableKey complemented [T.x notNull tableKey]]"
                                         + "(.y notNull [T.y notNull])(.z [T.z])"));

      // Columnの一致 (ignoreCase=false)
      result = this.GetResultInfoList(@"select COLUMN from TBL", false);
      Assert.That(result.Sql, Is.EqualTo(@"SELECT COLUMN,TBL.column AS TBL_column_ FROM TBL"));
      Assert.That(result.Info, Is.EqualTo(@"[.COLUMN notNull tableKey [TBL.COLUMN notNull tableKey]]"
                                         + "[.TBL_column_ notNull tableKey complemented [TBL.column notNull tableKey]]"));

      result = this.GetResultInfoList(@"select Column from TBL", false);
      Assert.That(result.Sql, Is.EqualTo(@"SELECT Column,TBL.COLUMN AS TBL_COLUMN_,TBL.column AS TBL_column_ FROM TBL"));
      Assert.That(result.Info, Is.EqualTo(@"[.Column ]"
                                         + "[.TBL_COLUMN_ notNull tableKey complemented [TBL.COLUMN notNull tableKey]]"
                                         + "[.TBL_column_ notNull tableKey complemented [TBL.column notNull tableKey]]"));

      result = this.GetResultInfoList(@"select cOlUmN from TBL", false);
      Assert.That(result.Sql, Is.EqualTo(@"SELECT cOlUmN,TBL.COLUMN AS TBL_COLUMN_,TBL.column AS TBL_column_ FROM TBL"));
      Assert.That(result.Info, Is.EqualTo(@"[.cOlUmN ]"
                                         + "[.TBL_COLUMN_ notNull tableKey complemented [TBL.COLUMN notNull tableKey]]"
                                         + "[.TBL_column_ notNull tableKey complemented [TBL.column notNull tableKey]]"));

      result = this.GetResultInfoList(@"select t.cOlUmN from TBL t", false);
      Assert.That(result.Sql, Is.EqualTo(@"SELECT t.cOlUmN,t.COLUMN AS t_COLUMN_,t.column AS t_column_ FROM TBL t"));
      Assert.That(result.Info, Is.EqualTo(@"[.cOlUmN ]"
                                         + "[.t_COLUMN_ notNull tableKey complemented [t.COLUMN notNull tableKey]]"
                                         + "[.t_column_ notNull tableKey complemented [t.column notNull tableKey]]"));

      // Column Aliasの一致 (ignoreCase=false)
      result = this.GetResultInfoList(@"select V.ABC from (select x as ABC from T) V", false);
      Assert.That(result.Sql, Is.EqualTo(@"SELECT V.ABC FROM (SELECT x AS ABC FROM T)V"));
      Assert.That(result.Info, Is.EqualTo(@"[.ABC notNull tableKey [V.ABC notNull tableKey "
                                         + "[T.x notNull tableKey]]]"));

      result = this.GetResultInfoList(@"select V.abc from (select X as ABC from T) V", false);
      Assert.That(result.Sql, Is.EqualTo(@"SELECT V.abc,V.T_x_ AS T_x_ FROM (SELECT X AS ABC,T.x AS T_x_ FROM T)V"));
      Assert.That(result.Info, Is.EqualTo(@"[.abc ][.T_x_ notNull tableKey complemented "
                                         + "[V.T_x_ notNull tableKey complemented [T.x notNull tableKey]]]"
                                         + "(.ABC [V.ABC ])"));

      result = this.GetResultInfoList(@"select v.ABC from (select X as abc from T) V", false);
      Assert.That(result.Sql, Is.EqualTo(@"SELECT v.ABC,V.T_x_ AS T_x_ FROM (SELECT X AS abc,T.x AS T_x_ FROM T)V"));
      Assert.That(result.Info, Is.EqualTo(@"[.ABC ][.T_x_ notNull tableKey complemented "
                                         + "[V.T_x_ notNull tableKey complemented [T.x notNull tableKey]]]"
                                         + "(.abc [V.abc ])"));

      // Ambiguous column
      Assert.Throws<InvalidASTStructureError>
        (() => { this.GetResultInfoList(@"select column, COLUMN, Column, cOlUmN from "
                                       + "Tbl cross join TBL"); });

      result = this.GetResultInfoList(@"select column, COLUMN, Column, cOlUmN from "
                                     + "Tbl cross join TBL", false);
      Assert.That(result.Sql, Is.EqualTo(@"SELECT column,COLUMN,Column,cOlUmN,"
                                        + "Tbl.COL AS Tbl_COL_,Tbl.col AS Tbl_col_ "
                                        + "FROM Tbl CROSS JOIN TBL"));
      Assert.That(result.Info, Is.EqualTo(@"[.column notNull tableKey [TBL.column notNull tableKey]]"
                                         + "[.COLUMN notNull tableKey [TBL.COLUMN notNull tableKey]]"
                                         + "[.Column ]"
                                         + "[.cOlUmN ]"
                                         + "[.Tbl_COL_ notNull tableKey complemented [Tbl.COL notNull tableKey]]"
                                         + "[.Tbl_col_ notNull tableKey complemented [Tbl.col notNull tableKey]]"));

      result = this.GetResultInfoList(@"select * from "
                                  + "tbl cross join TBL");
      Assert.That(result.Sql, Is.EqualTo(@"SELECT * FROM tbl CROSS JOIN TBL"));
      Assert.That(result.Info, Is.EqualTo(@"[.COLUMN notNull tableKey [TBL.COLUMN notNull tableKey]]"
                                         + "[.column notNull tableKey [TBL.column notNull tableKey]]"));

    }

    [Test]
    public void DualTable() {

    }

      //debugString = this.GetSourceItems("select * from (select x from T t0) v");
      //debugString = this.GetSourceItems("select (select x from T t0) from V v");
      ////debugString = this.GetSourceItems("select x, y from T union select 1, 2");
      //debugString = this.GetSourceItems("select a from T");
      //debugString = this.GetSourceItems("select T.* from T");
      //debugString = this.GetSourceItems("select count(z) from T");
      //debugString = this.GetSourceItems("select sum(x) from T");
      //debugString = this.GetSourceItems("select * from (select x,y,z,1 from T) t1");
      //debugString = this.GetSourceItems("select z from T group by z");
      //debugString = this.GetSourceItems("select null from (select z from T) v");
      //debugString = this.GetSourceItems("select x from T group by x");
      //debugString = this.GetSourceItems("select null from (select z from T group by z) v");
      //debugString = this.GetSourceItems("select sum(x) from T");
      //debugString = this.GetSourceItems("select distinct z from T");
      //debugString = this.GetSourceItems("select t1.* from T t1 join T  t2 on t1.id = t2.id");
      //debugString = this.GetSourceItems("select * from T t1 union all select * from T t2");
      ////debugString = this.GetSourceItems("select * from (select * from T t1 union select * from T t2) v");
      ////debugString = this.GetSourceItems("select x, y, z from (select * from T t1 union select * from T t2) v");
      //debugString = this.GetSourceItems("select * from (select z from T)");
      ////debugString = this.GetSourceItems("select * from (select y, z, x from T) V " +
      ////                                  "union all " +
      ////                                  "select 1, 2, 3 from U");
      //debugString = this.GetSourceItems("select * from (select y, z from T) V " +
      //                                  "union all " +
      //                                  "select 1, 2 from U");
      ////debugString = this.GetSourceItems("select * from (select y, z from T) V " +
      ////                                  "union all " +
      ////                                  "select 1, 2 from U");
      ////debugString = this.GetSourceItems("select W.* from (select y, z from T) V " +
      ////                                  "union all " +
      ////                                  "select 1, 2 from U");
      ////debugString = this.GetSourceItems("select y, z from (select y, z from T) V " +
      ////                                  "union all " +
      ////                                  "select 1, 2 from U");
      ////debugString = this.GetSourceItems("select x, y, z from (select x, y, z from T) V " +
      ////                                  "union all " +
      ////                                  "select 0, 1, 2 from U");

    private class Result
    {
      public string Sql;
      public string Info;
      public Result(string sql, string info) {
        this.Sql = sql;
        this.Info = info;
      }
    }

    private Dictionary<string, IEnumerable<TableResultInfo>> _tableColumns;

    [SetUp]
    public void initTest() {
      // テスト用テーブル列
      _tableColumns = new Dictionary<string, IEnumerable<TableResultInfo>>();

      var tableT = new List<TableResultInfo>();
      tableT.Add(new TableResultInfo("T", "x", false, true));
      tableT.Add(new TableResultInfo("T", "y", false));
      tableT.Add(new TableResultInfo("T", "z", true));
      _tableColumns.Add("T", tableT);

      var tableU = new List<TableResultInfo>();
      tableU.Add(new TableResultInfo("U", "x", false, true));
      tableU.Add(new TableResultInfo("U", "y", false));
      tableU.Add(new TableResultInfo("U", "z", true));
      _tableColumns.Add("U", tableU);

      var tableV = new List<TableResultInfo>();
      tableV.Add(new TableResultInfo("V", "x1", false, true));
      tableV.Add(new TableResultInfo("V", "x2", false, true));
      tableV.Add(new TableResultInfo("V", "x3", false, true));
      _tableColumns.Add("V", tableV);

      var tableTBL = new List<TableResultInfo>();
      tableTBL.Add(new TableResultInfo("TBL", "COLUMN", false, true));
      tableTBL.Add(new TableResultInfo("TBL", "column", false, true));
      _tableColumns.Add("TBL", tableTBL);

      var tableTbl = new List<TableResultInfo>();
      tableTbl.Add(new TableResultInfo("Tbl", "COL", false, true));
      tableTbl.Add(new TableResultInfo("Tbl", "col", false, true));
      _tableColumns.Add("Tbl", tableTbl);
    }

    [TearDown]
    public void finalTest() {
    }

    private Result GetResultInfoList(string sql, bool ignoreCase = true) {
      var ast = MiniSqlParserAST.CreateStmts(sql);

      //
      // var visitor = new GetResultInfoListVisitor(_tableColumns
      //                                          , GetResultInfoListVisitor.PrimaryKeyCompletion.AllQuery
      //                                          , ignoreCase);
      // ast.Accept(visitor);
      //
      var resultInfoAST = new ResultInfoAST((SelectStmt)ast[0]
                                          , _tableColumns
                                          , ResultInfoAST.PrimaryKeyCompletion.AllQuery
                                          , ignoreCase);

      var stringifier = new CompactStringifier(4098, true);
      ast.Accept(stringifier);

      return new Result(stringifier.ToString(), resultInfoAST.Print());
    }
  }
}
