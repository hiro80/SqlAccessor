using System;
using System.Collections.Generic;
using System.Text;
using SqlAccessor;
using NUnit.Framework;

namespace FoundationTester
{
  [TestFixture()]
  public class StringExtensionTester
  {

    [Test()]
    public void Split() {
      StringExtension str;
      string[] ret;
      str = new StringExtension("This is an \"example.\".Cool!");
      ret = str.Split(".", "\"");
      Assert.AreEqual(new string[] {
                    "This is an \"example.\"",
                    "Cool!"}, ret);
      str = new StringExtension("");
      ret = str.Split(",", "\"");
      Assert.AreEqual(new string[0], ret);
      str = new StringExtension("AAA,BBB,CCC");
      ret = str.Split(",", "\"");
      Assert.AreEqual(new string[] {
                    "AAA",
                    "BBB",
                    "CCC"}, ret);
      str = new StringExtension("\'AAA , BBB , CCC\'");
      ret = str.Split(",", "\'");
      Assert.AreEqual(new string[] {
                    "\'AAA , BBB , CCC\'"}, ret);
      str = new StringExtension("AAA,BBB,CCC,");
      ret = str.Split(",", "\"");
      Assert.AreEqual(new string[] {
                    "AAA",
                    "BBB",
                    "CCC"}, ret);
      str = new StringExtension("AAA,BBB,CCC,,,");
      ret = str.Split(",", "\"");
      Assert.AreEqual(new string[] {
                    "AAA",
                    "BBB",
                    "CCC"}, ret);
      str = new StringExtension("\'AAA , BBB , CCC");
      ret = str.Split(",", "\'");
      Assert.AreEqual(new string[] {
                    "\'AAA , BBB , CCC"}, ret);
      str = new StringExtension("\'AAA , BBB , CCC");
      ret = str.Split(",", "\'");
      Assert.AreEqual(new string[] {
                    "\'AAA , BBB , CCC"}, ret);
      str = new StringExtension("STRYMD = 4220320 AND DANTAI = \'00\' AND SYOZOK = \'001001001000010001\'");
      ret = str.Split(" AND ", "\'");
      Assert.AreEqual(new string[] {
                    "STRYMD = 4220320",
                    "DANTAI = \'00\'",
                    "SYOZOK = \'001001001000010001\'"}, ret);
      str = new StringExtension("STRYMD = 4220320 AND DANTAI = \'00\' AND SYOZOK = \'001001001000010001\' ");
      ret = str.Split(" AND ", "\'");
      Assert.AreEqual(new string[] {
                    "STRYMD = 4220320",
                    "DANTAI = \'00\'",
                    "SYOZOK = \'001001001000010001\' "}, ret);
      str = new StringExtension("STRYMD = 4220320 AND DANTAI = qqq00qqq AND SYOZOK = qqq001001001000010001qqq ");
      ret = str.Split(" AND ", "qqq");
      Assert.AreEqual(new string[] {
                    "STRYMD = 4220320",
                    "DANTAI = qqq00qqq",
                    "SYOZOK = qqq001001001000010001qqq "}, ret);
    }

    [Test()]
    public void IsInComment() {
      StringExtension str;
      str = new StringExtension("0123qqq7890123qqq7890123qqq7890");
      Assert.IsFalse(str.IsInComment(0, "qqq"));
      str = new StringExtension("0123qqq7890123qqq7890123qqq7890");
      Assert.IsFalse(str.IsInComment(3, "qqq"));
      str = new StringExtension("0123qqq7890123qqq7890123qqq7890");
      Assert.IsTrue(str.IsInComment(4, "qqq"));
      str = new StringExtension("0123qqq7890123qqq7890123qqq7890");
      Assert.IsTrue(str.IsInComment(6, "qqq"));
      str = new StringExtension("0123qqq7890123qqq7890123qqq7890");
      Assert.IsTrue(str.IsInComment(10, "qqq"));
      str = new StringExtension("0123qqq7890123qqq7890123qqq7890");
      Assert.IsTrue(str.IsInComment(14, "qqq"));
      str = new StringExtension("0123qqq7890123qqq7890123qqq7890");
      Assert.IsTrue(str.IsInComment(16, "qqq"));
      str = new StringExtension("0123qqq7890123qqq7890123qqq7890");
      Assert.IsFalse(str.IsInComment(20, "qqq"));
      str = new StringExtension("0123qqq7890123qqq7890123qqq7890");
      Assert.IsTrue(str.IsInComment(27, "qqq"));
      str = new StringExtension("0123qqq7890123qqq7890123qqq7890");
      Assert.IsTrue(str.IsInComment(30, "qqq"));
    }

    [Test()]
    public void IsInComment2() {
      StringExtension str;
      str = new StringExtension("0123qqq7890123ppp7890123qqq7890");
      Assert.IsFalse(str.IsInComment(0, "qqq", "ppp"));
      str = new StringExtension("0123qqq7890123ppp7890123qqq7890");
      Assert.IsFalse(str.IsInComment(3, "qqq", "ppp"));
      str = new StringExtension("0123qqq7890123ppp7890123qqq7890");
      Assert.IsTrue(str.IsInComment(4, "qqq", "ppp"));
      str = new StringExtension("0123qqq7890123ppp7890123qqq7890");
      Assert.IsTrue(str.IsInComment(6, "qqq", "ppp"));
      str = new StringExtension("0123qqq7890123ppp7890123qqq7890");
      Assert.IsTrue(str.IsInComment(10, "qqq", "ppp"));
      str = new StringExtension("0123qqq7890123ppp7890123qqq7890");
      Assert.IsTrue(str.IsInComment(14, "qqq", "ppp"));
      str = new StringExtension("0123qqq7890123ppp7890123qqq7890");
      Assert.IsTrue(str.IsInComment(16, "qqq", "ppp"));
      str = new StringExtension("0123qqq7890123ppp7890123qqq7890");
      Assert.IsFalse(str.IsInComment(20, "qqq", "ppp"));
      str = new StringExtension("0123qqq7890123ppp7890123qqq7890");
      Assert.IsTrue(str.IsInComment(27, "qqq", "ppp"));
      str = new StringExtension("0123qqq7890123ppp7890123qqq7890");
      Assert.IsTrue(str.IsInComment(30, "qqq", "ppp"));
      str = new StringExtension("0123qqq7890123qqq7ppp123ppp7890");
      Assert.IsTrue(str.IsInComment(7, "qqq", "ppp"));
      str = new StringExtension("0123qqq7890123qqq7ppp123ppp7890");
      Assert.IsTrue(str.IsInComment(17, "qqq", "ppp"));
      str = new StringExtension("0123qqq7890123qqq7ppp123ppp7890");
      Assert.IsFalse(str.IsInComment(30, "qqq", "ppp"));
      str = new StringExtension("0123ppp7890123ppp7qqq123qqq7890");
      Assert.IsFalse(str.IsInComment(17, "qqq", "ppp"));
      str = new StringExtension("0123456/*90123456789012*/567890");
      Assert.IsTrue(str.IsInComment(15, "/*", "*/"));
      str = new StringExtension("0123456/*901*/456789012*/567890");
      Assert.IsFalse(str.IsInComment(15, "/*", "*/"));
    }
  }
}