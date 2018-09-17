using System;
using System.Collections.Generic;
using System.Text;
using SqlAccessor;
using NUnit.Framework;

namespace FoundationTester
{
  [TestFixture()]
  public class DictionaryTester
  {
    private TwoKeysDictionary<string, string, string> aTwoKeysDic;
    private TwoKeysDictionary<object, object, object> aTwoKeysDic2;
    private ThreeKeysDictionary<string, string, string, string> aThreeKeysDic;
    private ThreeKeysDictionary<object, object, object, object> aThreeKeysDic2;
    private string str;

    [Test()]
    public void Basic() {
      aTwoKeysDic = new TwoKeysDictionary<string, string, string>();
      Assert.False(aTwoKeysDic.ContainsKey("INVALID_KEY", "1"));
      Assert.False(aTwoKeysDic.ContainsKey("INVALID_KEY"));
      Assert.False(aTwoKeysDic.ContainsKey(""));
      Assert.False(aTwoKeysDic.ContainsKey("\""));
      Assert.Throws<ArgumentNullException>(() => { aTwoKeysDic.ContainsKey("INVALID_KEY", null); });
      Assert.Throws<ArgumentNullException>(() => { aTwoKeysDic.ContainsKey(null, "INVALID_KEY"); });
      Assert.Throws<ArgumentNullException>(() => { aTwoKeysDic.ContainsKey(null); });

      // Items
      Assert.Throws<KeyNotFoundException>(() => { var obj = aTwoKeysDic.Items("INVALID_KEY"); });
      Assert.Throws<ArgumentNullException>(() => { var obj = aTwoKeysDic.Items(null); });
      Assert.Throws<KeyNotFoundException>(() => { var obj = aTwoKeysDic.Items(""); });

      // Item
      Assert.Throws<KeyNotFoundException>(() => { var str = aTwoKeysDic["INVALID_KEY", "INVALID_KEY"]; });
      Assert.Throws<ArgumentNullException>(() => { var str = aTwoKeysDic["INVALID_KEY", null]; });
      Assert.Throws<ArgumentNullException>(() => { var str = aTwoKeysDic[null, "INVALID_KEY"]; });
      Assert.Throws<ArgumentNullException>(() => { var str = aTwoKeysDic[null, null]; });
      Assert.Throws<KeyNotFoundException>(() => { var str = aTwoKeysDic["INVALID_KEY", ""]; });
      Assert.Throws<KeyNotFoundException>(() => { var str = aTwoKeysDic["", "INVALID_KEY"]; });
      Assert.Throws<KeyNotFoundException>(() => { var str = aTwoKeysDic["", ""]; });

      // Item(値の追加後)
      aTwoKeysDic.Add("KEY1", "KEY2", "VALUE");
      Assert.AreEqual("VALUE", aTwoKeysDic["KEY1", "KEY2"]);
      Assert.Throws<KeyNotFoundException>(() => { var str = aTwoKeysDic["KEY2", "KEY1"]; });
      Assert.Throws<KeyNotFoundException>(() => { var str = aTwoKeysDic["KEY1", "VALUE"]; });
      Assert.Throws<KeyNotFoundException>(() => { var str = aTwoKeysDic["VALUE", "KEY2"]; });
      Assert.Throws<KeyNotFoundException>(() => { var str = aTwoKeysDic["KEY2", "VALUE"]; });
      Assert.Throws<KeyNotFoundException>(() => { var str = aTwoKeysDic["VALUE", "KEY1"]; });
      Assert.Throws<KeyNotFoundException>(() => { var str = aTwoKeysDic["KEY1", "INVALID_KEY"]; });
      Assert.Throws<KeyNotFoundException>(() => { var str = aTwoKeysDic["INVALID_KEY", "KEY2"]; });

      // Add
      Assert.Throws<ArgumentException>(() => { aTwoKeysDic.Add("KEY1", "KEY2", "AnotherValue"); });
      aTwoKeysDic.Add("KEY2", "KEY1", "AnotherValue");
      aTwoKeysDic.Add("key1", "key2", "AnotherValue");
      aTwoKeysDic.Add("key1", "KEY2", "AnotherValue");
      aTwoKeysDic.Add("KEY1", "key2", "AnotherValue");
    }

    [Test()]
    public void ObjectData() {
      aTwoKeysDic2 = new TwoKeysDictionary<object, object, object>();
      Assert.False(aTwoKeysDic2.ContainsKey(new object(), "1"));
      Assert.False(aTwoKeysDic2.ContainsKey(1234567890));
      Assert.False(aTwoKeysDic2.ContainsKey(System.DateTime.Now));
      Assert.False(aTwoKeysDic2.ContainsKey("\""));
      aTwoKeysDic2.Add(1, "2", 3D);
      aTwoKeysDic2.Add("1", 2, 3);
    }

    [Test()]
    public void InvalidData() {
      aThreeKeysDic = new ThreeKeysDictionary<string, string, string, string>();

      // ContainsKey
      Assert.False(aThreeKeysDic.ContainsKey("INVALID_KEY", "1", "2"));
      Assert.False(aThreeKeysDic.ContainsKey("INVALID_KEY", "1"));
      Assert.False(aThreeKeysDic.ContainsKey("INVALID_KEY"));
      Assert.False(aThreeKeysDic.ContainsKey("", "", ""));
      Assert.False(aThreeKeysDic.ContainsKey("", ""));
      Assert.False(aThreeKeysDic.ContainsKey(""));
      Assert.False(aThreeKeysDic.ContainsKey("\""));

      Assert.Throws<ArgumentNullException>(() => { aThreeKeysDic.ContainsKey("INVALID_KEY", "1", null); });
      Assert.Throws<ArgumentNullException>(() => { aThreeKeysDic.ContainsKey("INVALID_KEY", null, "2"); });
      Assert.Throws<ArgumentNullException>(() => { aThreeKeysDic.ContainsKey(null, "1", "2"); });
      Assert.Throws<ArgumentNullException>(() => { aThreeKeysDic.ContainsKey(null, null, null); });
      Assert.Throws<ArgumentNullException>(() => { aThreeKeysDic.ContainsKey("INVALID_KEY", null); });
      Assert.Throws<ArgumentNullException>(() => { aThreeKeysDic.ContainsKey(null, "1"); });
      Assert.Throws<ArgumentNullException>(() => { aThreeKeysDic.ContainsKey(null, null); });
      Assert.Throws<ArgumentNullException>(() => { aThreeKeysDic.ContainsKey(null); });

      // Items
      Assert.Throws<KeyNotFoundException>(() => { var obj = aThreeKeysDic.Items("INVALID_KEY", "1"); });
      Assert.Throws<ArgumentNullException>(() => { var obj = aThreeKeysDic.Items("INVALID_KEY", null); });
      Assert.Throws<ArgumentNullException>(() => { var obj = aThreeKeysDic.Items(null, "1"); });
      Assert.Throws<ArgumentNullException>(() => { var obj = aThreeKeysDic.Items(null, null); });
      Assert.Throws<KeyNotFoundException>(() => { var obj = aThreeKeysDic.Items("", ""); });

      // Item
      Assert.Throws<KeyNotFoundException>(() => { var str = aThreeKeysDic["INVALID_KEY", "1", "2"]; });
      Assert.Throws<ArgumentNullException>(() => { var str = aThreeKeysDic["INVALID_KEY", "1", null]; });
      Assert.Throws<ArgumentNullException>(() => { var str = aThreeKeysDic["INVALID_KEY", null, "2"]; });
      Assert.Throws<ArgumentNullException>(() => { var str = aThreeKeysDic[null, "1", "2"]; });
      Assert.Throws<ArgumentNullException>(() => { var str = aThreeKeysDic[null, null, null]; });

      // Add
      Assert.Throws<ArgumentNullException>(() => { aThreeKeysDic.Add(null, "KEY2", "KEY3", "VALUE"); });
      Assert.Throws<ArgumentNullException>(() => { aThreeKeysDic.Add("KEY1", null, "KEY3", "VALUE"); });
      Assert.Throws<ArgumentNullException>(() => { aThreeKeysDic.Add("KEY1", "KEY2", null, "VALUE"); });
      // 値にnullを格納することは可能である
      aThreeKeysDic.Add("日本語", "中國語", "朝鮮語", null);
      Assert.Throws<ArgumentNullException>(() => { aThreeKeysDic.Add(null, null, null, null); });

      TwoKeysDictionary<string, string, string> expected = new TwoKeysDictionary<string, string, string>();

      aThreeKeysDic.Add("KEY1", "KEY2", "KEY3", "VALUE");
      aThreeKeysDic.Add("", "KEY2", "KEY3", "VALUE");
      aThreeKeysDic.Add("KEY1", "", "KEY3", "VALUE");
      aThreeKeysDic.Add("KEY1", "KEY2", "", "VALUE");
      aThreeKeysDic.Add("", "", "", "");

      expected.Add("KEY2", "KEY3", "VALUE");
      expected.Add("", "KEY3", "VALUE");
      expected.Add("KEY2", "", "VALUE");

      //Items(値の追加後)
      //
      //TwoKeys/ThreeKeysDictionaryはDictionaryのようにForEachでも使えるようにしたい
      //
      //Assert.AreEqual(expected, aThreeKeysDic.Items("KEY1"))
      Assert.AreEqual(expected.Items("KEY2"), aThreeKeysDic.Items("KEY1", "KEY2"));
      Assert.AreEqual(expected["KEY2", "KEY3"], aThreeKeysDic["KEY1", "KEY2", "KEY3"]);
    }
  }
}