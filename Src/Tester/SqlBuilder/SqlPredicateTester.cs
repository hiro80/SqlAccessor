using System;
using System.Collections.Generic;
using MiniSqlParser;
using NUnit.Framework;

namespace SqlBuilderTester
{
  [TestFixture]
  public class SqlPredicateTester
  {
    [Test]
    public void Empty() {
      SqlPredicate p = new SqlPredicate();

      Assert.That(p.And(p).ToString(), Is.EqualTo(""));
      Assert.That(p.Clone().ToString(), Is.EqualTo(""));
      Assert.That(p.GetAllPlaceHolders(), Is.EqualTo(new string[] { }));
      Assert.That(p.HasUnplacedHolder("T"), Is.False);
      Assert.That(p.HasUnplacedHolders(), Is.False);
      Assert.That(p.IsEmpty, Is.True);
      Assert.That(p.IsPlaceHolderOnly, Is.False);
      p.Place("PH", "a=1");
      Assert.That(p.ToString(), Is.EqualTo(""));
    }

    [Test]
    public void Clone() {
      SqlPredicate p;
      SqlPredicate p1;

      p = new SqlPredicate("@PH");
      p1 = p.Clone();
      Assert.That(p.ToString(), Is.EqualTo("@PH"));
      Assert.That(p1.ToString(), Is.EqualTo("@PH"));
      p1.Place("PH", "x=1");
      Assert.That(p.ToString(), Is.EqualTo("@PH"));
      Assert.That(p1.ToString(), Is.EqualTo("x=1"));
    }

    [Test]
    public void GetAllPlaceHolders() {
      SqlPredicate p;

      p = new SqlPredicate("@PH");
      Assert.That(p.GetAllPlaceHolders()
        , Is.EqualTo(new Dictionary<string, string> { { "PH", "" } }));
      p.Place("PH", "'abc' > 0");
      Assert.That(p.GetAllPlaceHolders()
        , Is.EqualTo(new Dictionary<string, string> { { "PH", "'abc'>0" } }));

      p = new SqlPredicate("@PH1 And @PH2");
      p.Place("PH1", "x=+100");
      Assert.That(p.GetAllPlaceHolders(), Is.EqualTo(new Dictionary<string, string>
        { { "PH1", "x=+100"}, {"PH2", "" } }));
      p.Place("PH2", "y = -9");
      Assert.That(p.GetAllPlaceHolders(), Is.EqualTo(new Dictionary<string, string>
        { { "PH1", "x=+100"}, {"PH2", "y=-9" } }));

      p = new SqlPredicate("@PH AND @PH");
      p.Place("PH", "x = 'abc'");
      Assert.That(p.GetAllPlaceHolders(), Is.EqualTo(new Dictionary<string, string>
        { { "PH", "x='abc'" } }));
      p.Place("PH", "x = 'abc'");
      Assert.That(p.GetAllPlaceHolders(), Is.EqualTo(new Dictionary<string, string>
        { { "PH", "x='abc'" } }));
    }

    [Test]
    public void HasUnplacedHolder() {
      SqlPredicate p;

      p = new SqlPredicate("@PH");
      Assert.That(p.HasUnplacedHolder("PH"), Is.True);
      p.Place("PH", "'abc' <= 'abcd'");
      Assert.That(p.HasUnplacedHolder("PH"), Is.False);

      p = new SqlPredicate("@PH1 And @PH2");
      p.Place("PH1", "x = +100");
      Assert.That(p.HasUnplacedHolder("PH1"), Is.False);
      Assert.That(p.HasUnplacedHolder("PH2"), Is.True);
      p.Place("PH2", "y = -9");
      Assert.That(p.HasUnplacedHolder("PH1"), Is.False);
      Assert.That(p.HasUnplacedHolder("PH2"), Is.False);

      p = new SqlPredicate("@PH AND @PH");
      p.Place("PH", "'abc' <= 'abcd'");
      Assert.That(p.HasUnplacedHolder("PH"), Is.False);
      p.Place("PH", "'abc' <= 'abcd'");
      Assert.That(p.HasUnplacedHolder("PH"), Is.False);
    }

    [Test]
    public void HasUnplacedHolders() {
      SqlPredicate p;

      p = new SqlPredicate("@PH");
      Assert.That(p.HasUnplacedHolders(), Is.True);
      p.Place("PH", "'abc' <= 'abcd'");
      Assert.That(p.HasUnplacedHolders(), Is.False);

      p = new SqlPredicate("@PH1 And @PH2");
      p.Place("PH1", "x = +100");
      Assert.That(p.HasUnplacedHolders(), Is.True);
      p.Place("PH2", "y = -9");
      Assert.That(p.HasUnplacedHolders(), Is.False);

      p = new SqlPredicate("@PH AND @PH");
      p.Place("PH", "'abc' <= 'abcd'");
      Assert.That(p.HasUnplacedHolders(), Is.False);
      p.Place("PH", "'abc' <= 'abcd'");
      Assert.That(p.HasUnplacedHolders(), Is.False);
    }

    [Test]
    public void IsPlaceHolderOnly() {
      SqlPredicate p;

      p = new SqlPredicate("@PH");
      Assert.That(p.IsPlaceHolderOnly, Is.True);

      p = new SqlPredicate("a is not null");
      Assert.That(p.IsPlaceHolderOnly, Is.False);
    }

    [Test]
    public void Place() {
      SqlPredicate p;

      p = new SqlPredicate("not c > 1");
      p.Place("PH", "a");
      Assert.That(p.ToString(), Is.EqualTo("NOT c>1"));

      p = new SqlPredicate("@PH");
      Assert.Throws<CannotBuildASTException>(() => { p.Place("PH", "1"); });

      p = new SqlPredicate("@PH");
      p.Place("PH", "a=1");
      Assert.That(p.ToString(), Is.EqualTo("a=1"));

      p = new SqlPredicate("Not @PH");
      p.Place("PH", "x=2 or y=3");
      Assert.That(p.ToString(), Is.EqualTo("NOT (x=2 OR y=3)"));
    }

    [Test]
    public void Place2() {
      SqlPredicate p;

      //
      // Predicate演算子の結合の優先順位が適用前後で変わらないことを確認する
      //

      p = new SqlPredicate("@PH");
      p.Place("PH", "a=1 and b=2");
      Assert.That(p.ToString(), Is.EqualTo("a=1 AND b=2"));

      p = new SqlPredicate("@PH");
      p.Place("PH", "a=1 or b=2");
      Assert.That(p.ToString(), Is.EqualTo("a=1 OR b=2"));

      p = new SqlPredicate("@PH");
      p.Place("PH", "not a=1");
      Assert.That(p.ToString(), Is.EqualTo("NOT a=1"));

      p = new SqlPredicate("@PH");
      p.Place("PH", "a=1 collate jp");
      Assert.That(p.ToString(), Is.EqualTo("a=1 COLLATE jp"));


      p = new SqlPredicate("@PH AND 1 = 1");
      p.Place("PH", "a=1 and b=2");
      Assert.That(p.ToString(), Is.EqualTo("a=1 AND b=2 AND 1=1"));

      p = new SqlPredicate("@PH AND 1 = 1");
      p.Place("PH", "a=1 or b=2");
      Assert.That(p.ToString(), Is.EqualTo("(a=1 OR b=2) AND 1=1"));

      p = new SqlPredicate("@PH AND 1 = 1");
      p.Place("PH", "not a=1");
      Assert.That(p.ToString(), Is.EqualTo("NOT a=1 AND 1=1"));

      p = new SqlPredicate("@PH AND 1 = 1");
      p.Place("PH", "a=1 collate jp");
      Assert.That(p.ToString(), Is.EqualTo("a=1 COLLATE jp AND 1=1"));


      p = new SqlPredicate("@PH OR 1 = 1");
      p.Place("PH", "a=1 and b=2");
      Assert.That(p.ToString(), Is.EqualTo("a=1 AND b=2 OR 1=1"));

      p = new SqlPredicate("@PH OR 1 = 1");
      p.Place("PH", "a=1 or b=2");
      Assert.That(p.ToString(), Is.EqualTo("a=1 OR b=2 OR 1=1"));

      p = new SqlPredicate("@PH OR 1 = 1");
      p.Place("PH", "not a=1");
      Assert.That(p.ToString(), Is.EqualTo("NOT a=1 OR 1=1"));

      p = new SqlPredicate("@PH OR 1 = 1");
      p.Place("PH", "a=1 collate jp");
      Assert.That(p.ToString(), Is.EqualTo("a=1 COLLATE jp OR 1=1"));


      p = new SqlPredicate("Not @PH");
      p.Place("PH", "a=1 and b=2");
      Assert.That(p.ToString(), Is.EqualTo("NOT (a=1 AND b=2)"));

      p = new SqlPredicate("Not @PH");
      p.Place("PH", "a=1 or b=2");
      Assert.That(p.ToString(), Is.EqualTo("NOT (a=1 OR b=2)"));

      p = new SqlPredicate("Not @PH");
      p.Place("PH", "not a=1");
      Assert.That(p.ToString(), Is.EqualTo("NOT NOT a=1"));

      p = new SqlPredicate("Not @PH");
      p.Place("PH", "a=1 collate jp");
      Assert.That(p.ToString(), Is.EqualTo("NOT a=1 COLLATE jp"));


      p = new SqlPredicate("@PH collate jp");
      p.Place("PH", "a=1 and b=2");
      Assert.That(p.ToString(), Is.EqualTo("(a=1 AND b=2) COLLATE jp"));

      p = new SqlPredicate("@PH collate jp");
      p.Place("PH", "a=1 or b=2");
      Assert.That(p.ToString(), Is.EqualTo("(a=1 OR b=2) COLLATE jp"));

      p = new SqlPredicate("@PH collate jp");
      p.Place("PH", "not a=1");
      Assert.That(p.ToString(), Is.EqualTo("NOT a=1 COLLATE jp"));

      p = new SqlPredicate("@PH collate jp");
      p.Place("PH", "a=1 collate jp");
      Assert.That(p.ToString(), Is.EqualTo("a=1 COLLATE jp COLLATE jp"));
    }

    [Test]
    public new void ToString() {
      SqlPredicate p;

      p = new SqlPredicate("x is null");
      p.ToString();
      Assert.That(p.ToString(), Is.EqualTo("x IS NULL"));
    }

    [Test]
    public void And() {
      SqlPredicate p;
      SqlPredicate p1;

      p = new SqlPredicate("@PH and 1 = 1");
      p1 = p.And(new SqlPredicate("x is null and 2=2"));
      Assert.That(p1.ToString(), Is.EqualTo("@PH AND 1=1 AND x IS NULL AND 2=2"));

      p1 = p.And(new SqlPredicate("x is null or 2=2"));
      Assert.That(p1.ToString(), Is.EqualTo("@PH AND 1=1 AND (x IS NULL OR 2=2)"));

      p = new SqlPredicate("@PH or 1 = 1");
      p1 = p.And(new SqlPredicate("x is null and 2 = 2"));
      Assert.That(p1.ToString(), Is.EqualTo("(@PH OR 1=1) AND x IS NULL AND 2=2"));

      p = new SqlPredicate("x is null");
      p1 = p.And(p);
      Assert.That(p1.ToString(), Is.EqualTo("x IS NULL AND x IS NULL"));

      p = new SqlPredicate("x = 11");
      p1 = p.And(new SqlPredicate());
      Assert.That(p1.ToString(), Is.EqualTo("x=11"));
    }

    private Dictionary<string, IEnumerable<string>> _tableColumns;

    [SetUp]
    public void initTest() {
      // テスト用テーブル列
      _tableColumns = new Dictionary<string, IEnumerable<string>>();
      _tableColumns.Add("T", new string[] { "x", "y", "z" });
      _tableColumns.Add("U", new string[] { "x", "y", "z" });
      _tableColumns.Add("V", new string[] { "x1", "x2", "x3" });
    }
  }
}
