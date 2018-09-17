using System;
using System.Collections.Generic;
using MiniSqlParser;
using NUnit.Framework;

namespace SqlBuilderTester
{
  [TestFixture]
  public class SqlExprTester
  {
    [Test]
    public void Empty() {
      SqlExpr e = new SqlExpr();

      Assert.That(e.Clone().ToString(), Is.EqualTo(""));
      Assert.That(e.GetAllPlaceHolders(), Is.EqualTo(new string[] { }));
      Assert.That(e.HasUnplacedHolder("T"), Is.False);
      Assert.That(e.HasUnplacedHolders(), Is.False);
      Assert.That(e.IsEmpty, Is.True);
      Assert.That(e.IsPlaceHolderOnly, Is.False);
      e.Place("PH", "a");
      Assert.That(e.ToString(), Is.EqualTo(""));
    }

    [Test]
    public void Clone() {
      SqlExpr e;
      SqlExpr e1;

      e = new SqlExpr("@PH");
      e1 = e.Clone();
      Assert.That(e.ToString(), Is.EqualTo("@PH"));
      Assert.That(e1.ToString(), Is.EqualTo("@PH"));
      e1.Place("PH", "x");
      Assert.That(e.ToString(), Is.EqualTo("@PH"));
      Assert.That(e1.ToString(), Is.EqualTo("x"));
    }

    [Test]
    public void GetAllPlaceHolders() {
      SqlExpr e;

      e = new SqlExpr("@PH");
      Assert.That(e.GetAllPlaceHolders()
        , Is.EqualTo(new Dictionary<string, string> { { "PH", "" } }));
      e.Place("PH", "'abc'");
      Assert.That(e.GetAllPlaceHolders()
        , Is.EqualTo(new Dictionary<string, string> { { "PH", "'abc'" } }));

      e = new SqlExpr("@PH1 + @PH2");
      e.Place("PH1", "+100");
      Assert.That(e.GetAllPlaceHolders(), Is.EqualTo(new Dictionary<string, string>
        { { "PH1", "+100"}, {"PH2", "" } }));
      e.Place("PH2", "-9");
      Assert.That(e.GetAllPlaceHolders(), Is.EqualTo(new Dictionary<string, string>
        { { "PH1", "+100"}, {"PH2", "-9" } }));

      e = new SqlExpr("@PH || @PH");
      e.Place("PH", "'abc'");
      Assert.That(e.GetAllPlaceHolders(), Is.EqualTo(new Dictionary<string, string>
        { { "PH", "'abc'" } }));
      e.Place("PH", "'abc'");
      Assert.That(e.GetAllPlaceHolders(), Is.EqualTo(new Dictionary<string, string>
        { { "PH", "'abc'" } }));
    }

    [Test]
    public void HasUnplacedHolder() {
      SqlExpr e;

      e = new SqlExpr("@PH");
      Assert.That(e.HasUnplacedHolder("PH"), Is.True);
      e.Place("PH", "'abc'");
      Assert.That(e.HasUnplacedHolder("PH"), Is.False);

      e = new SqlExpr("@PH1 + @PH2");
      e.Place("PH1", "+100");
      Assert.That(e.HasUnplacedHolder("PH1"), Is.False);
      Assert.That(e.HasUnplacedHolder("PH2"), Is.True);
      e.Place("PH2", "-9");
      Assert.That(e.HasUnplacedHolder("PH1"), Is.False);
      Assert.That(e.HasUnplacedHolder("PH2"), Is.False);

      e = new SqlExpr("@PH || @PH");
      e.Place("PH", "'abc'");
      Assert.That(e.HasUnplacedHolder("PH"), Is.False);
      e.Place("PH", "'abc'");
      Assert.That(e.HasUnplacedHolder("PH"), Is.False);
    }

    [Test]
    public void HasUnplacedHolders() {
      SqlExpr e;

      e = new SqlExpr("@PH");
      Assert.That(e.HasUnplacedHolders(), Is.True);
      e.Place("PH", "'abc'");
      Assert.That(e.HasUnplacedHolders(), Is.False);

      e = new SqlExpr("@PH1 + @PH2");
      e.Place("PH1", "+100");
      Assert.That(e.HasUnplacedHolders(), Is.True);
      e.Place("PH2", "-9");
      Assert.That(e.HasUnplacedHolders(), Is.False);

      e = new SqlExpr("@PH || @PH");
      e.Place("PH", "'abc'");
      Assert.That(e.HasUnplacedHolders(), Is.False);
      e.Place("PH", "'abc'");
      Assert.That(e.HasUnplacedHolders(), Is.False);
    }

    [Test]
    public void IsPlaceHolderOnly() {
      SqlExpr e;

      e = new SqlExpr("@PH");
      Assert.That(e.IsPlaceHolderOnly, Is.True);

      e = new SqlExpr("Cast(a as NEWTYPE)");
      Assert.That(e.IsPlaceHolderOnly, Is.False);
    }

    [Test]
    public void Place() {
      SqlExpr e;

      e = new SqlExpr("DEFAULT");
      e.Place("PH", "a");
      Assert.That(e.ToString(), Is.EqualTo("DEFAULT"));

      e = new SqlExpr("@PH");
      Assert.Throws<CannotBuildASTException>(() => { e.Place("PH", "a=1"); });

      e = new SqlExpr("@PH");
      e.Place("PH", "a");
      Assert.That(e.ToString(), Is.EqualTo("a"));

      e = new SqlExpr("cast(@PH as VARCHAR2)");
      e.Place("PH", "x");
      Assert.That(e.ToString(), Is.EqualTo("CAST(x AS VARCHAR2)"));
    }

    [Test]
    public void Place2() {
      SqlExpr e;

      //
      // Expr演算子の結合の優先順位が適用前後で変わらないことを確認する
      //

      e = new SqlExpr("@PH");
      e.Place("PH", "~a");
      Assert.That(e.ToString(), Is.EqualTo("~a"));

      e = new SqlExpr("@PH");
      e.Place("PH", "a+b");
      Assert.That(e.ToString(), Is.EqualTo("a+b"));

      e = new SqlExpr("~@PH");
      e.Place("PH", "~a");
      Assert.That(e.ToString(), Is.EqualTo("~~a"));

      e = new SqlExpr("~@PH");
      e.Place("PH", "a+b");
      Assert.That(e.ToString(), Is.EqualTo("~(a+b)"));

      e = new SqlExpr("@PH || 'abc'");
      e.Place("PH", "~a");
      Assert.That(e.ToString(), Is.EqualTo("~a||'abc'"));

      e = new SqlExpr("@PH || 'abc'");
      e.Place("PH", "a+b");
      Assert.That(e.ToString(), Is.EqualTo("(a+b)||'abc'"));
    }

    [Test]
    public void IsDefault() {
      SqlExpr e;

      e = new SqlExpr("Default");
      Assert.That(e.IsDefault, Is.True);

      e = new SqlExpr("CAST(a As NEWTYPE)");
      Assert.That(e.IsDefault, Is.False);
    }

    [Test]
    public void IsLiteral() {
      SqlExpr e;

      e = new SqlExpr("default");
      Assert.That(e.IsLiteral, Is.False);

      e = new SqlExpr("Cast(1 as NEWTYPE)");
      Assert.That(e.IsLiteral, Is.False);

      e = new SqlExpr("'abc'");
      Assert.That(e.IsLiteral, Is.True);

      e = new SqlExpr("1192");
      Assert.That(e.IsLiteral, Is.True);
    }

    [Test]
    public new void ToString() {
      SqlExpr e;

      e = new SqlExpr("x / 8 ");
      e.ToString();
      Assert.That(e.ToString(), Is.EqualTo("x/8"));

      e = new SqlExpr("Default");
      e.ToString();
      Assert.That(e.ToString(), Is.EqualTo("DEFAULT"));
    }

    [SetUp]
    public void initTest() {
    }
  }
}
