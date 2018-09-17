using System;
using System.Collections.Generic;
using SqlAccessor;
using NUnit.Framework;

[TestFixture()]
public class QueryTester
{
  private IQuery aQuery;

  private readonly string tempDir = Environment.GetEnvironmentVariable("TEMP");

  //2項演算子
  [Test()]
  public void BinaryOp() {
    //boolean
    aQuery = new Query<ColumnInfo>();
    aQuery.And(1 + 1 == 2);
    Assert.AreEqual("1 = 1", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(false);
    Assert.AreEqual("1 <> 1", aQuery.GetWhereExp());

    //=
    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM") == 1);
    Assert.AreEqual("NUM = 1", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(1 == val.of("NUM"));
    Assert.AreEqual("1 = NUM", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("CHAR") == "str");
    Assert.AreEqual("CHAR = str", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And("str" == val.of("CHAR"));
    Assert.AreEqual("str = CHAR", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM") == val.of("CHAR"));
    Assert.AreEqual("NUM = CHAR", aQuery.GetWhereExp());

    //<>
    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM") != 1);
    Assert.AreEqual("NUM <> 1", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("CHAR") != "str");
    Assert.AreEqual("CHAR <> str", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM") != val.of("CHAR"));
    Assert.AreEqual("NUM <> CHAR", aQuery.GetWhereExp());

    //<
    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM") < 1);
    Assert.AreEqual("NUM < 1", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("CHAR") < "str");
    Assert.AreEqual("CHAR < str", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM") < val.of("CHAR"));
    Assert.AreEqual("NUM < CHAR", aQuery.GetWhereExp());

    //<=
    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM") <= 1);
    Assert.AreEqual("NUM <= 1", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("CHAR") <= "str");
    Assert.AreEqual("CHAR <= str", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM") <= val.of("CHAR"));
    Assert.AreEqual("NUM <= CHAR", aQuery.GetWhereExp());

    //>
    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM") > 1);
    Assert.AreEqual("NUM > 1", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("CHAR") > "str");
    Assert.AreEqual("CHAR > str", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM") > val.of("CHAR"));
    Assert.AreEqual("NUM > CHAR", aQuery.GetWhereExp());

    //>=
    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM") >= 1);
    Assert.AreEqual("NUM >= 1", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("CHAR") >= "str");
    Assert.AreEqual("CHAR >= str", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM") >= val.of("CHAR"));
    Assert.AreEqual("NUM >= CHAR", aQuery.GetWhereExp());

    //LIKE
    //aQuery = New Query(Of ColumnInfo)
    //aQuery.And(val.of("NUM") Like 1)
    //Assert.AreEqual("NUM LIKE 1", aQuery.GetWhereExp())

    //aQuery = new Query<ColumnInfo>();
    //aQuery.And(val.of("CHAR") Like "str");
    //Assert.AreEqual("CHAR LIKE str", aQuery.GetWhereExp());

    //aQuery = New Query(Of ColumnInfo)
    //aQuery.And(val.of("NUM") Like val.of("CHAR"))
    //Assert.AreEqual("NUM LIKE CHAR", aQuery.GetWhereExp())
  }

  //3項以上の演算子
  [Test()]
  public void TernaryOp() {
    //BETWEEN
    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM").Between(1, 9));
    Assert.AreEqual("NUM BETWEEN 1 AND 9", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("CHAR").Between("abc", "xyz"));
    Assert.AreEqual("CHAR BETWEEN abc AND xyz", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM").Between(val.of("CHAR1"), val.of("CHAR2")));
    Assert.AreEqual("NUM BETWEEN CHAR1 AND CHAR2", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM").Between(val.of("CHAR1"), "abc"));
    Assert.AreEqual("NUM BETWEEN CHAR1 AND abc", aQuery.GetWhereExp());

    //IN
    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM").In(1, 2, 3));
    Assert.AreEqual("NUM IN (1, 2, 3)", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("CHAR").In("abc", "def", "ghi"));
    Assert.AreEqual("CHAR IN (abc, def, ghi)", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM").In(val.of("CHAR1"), val.of("CHAR2"), val.of("CHAR3")));
    Assert.AreEqual("NUM IN (CHAR1, CHAR2, CHAR3)", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM").In(val.of("CHAR1"), "abc", "def"));
    Assert.AreEqual("NUM IN (CHAR1, abc, def)", aQuery.GetWhereExp());

    var nums = new List<int>(new int[] { 0, 1, 2 });
    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM").In(nums));
    Assert.AreEqual("NUM IN (0, 1, 2)", aQuery.GetWhereExp());

    var strings = new List<string>(new string[] { "a", "b", "c" });
    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM").In(strings));
    Assert.AreEqual("NUM IN (a, b, c)", aQuery.GetWhereExp());

    var vals = new List<val>(new val[] { val.of("CHAR1"), val.of("CHAR2"), val.of("CHAR3") });
    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM").In(vals));
    Assert.AreEqual("NUM IN (CHAR1, CHAR2, CHAR3)", aQuery.GetWhereExp());

    var numEnumerable = (IEnumerable<int>)nums;
    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM").In(numEnumerable));
    Assert.AreEqual("NUM IN (0, 1, 2)", aQuery.GetWhereExp());

    var strEnumerable = (IEnumerable<string>)strings;
    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM").In(strEnumerable));
    Assert.AreEqual("NUM IN (a, b, c)", aQuery.GetWhereExp());

    var valEnumerable = (IEnumerable<val>)vals;
    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM").In(vals));
    Assert.AreEqual("NUM IN (CHAR1, CHAR2, CHAR3)", aQuery.GetWhereExp());
  }

  //論理演算子
  [Test()]
  public void LogicalOp() {
    //AND
    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM").Between(1, 9) & val.of("CHAR").Between("abc", "xyz"));
    Assert.AreEqual("NUM BETWEEN 1 AND 9 AND CHAR BETWEEN abc AND xyz", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM").In(1, 2, 3) & val.of("CHAR").In("abc", "def", "ghi"));
    Assert.AreEqual("NUM IN (1, 2, 3) AND CHAR IN (abc, def, ghi)", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM") != val.of("and") & val.of("CHAR") != val.of("or"));
    Assert.AreEqual("NUM <> \"and\" AND CHAR <> \"or\"", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM") == 1 & val.of("CHAR") == "a" & val.of("CHAR") == "a" & val.of("CHAR") == "a" & val.of("CHAR") == "a" & val.of("CHAR") == "a");
    Assert.AreEqual("NUM = 1 AND CHAR = a AND CHAR = a AND CHAR = a AND CHAR = a AND CHAR = a", aQuery.GetWhereExp());
  }

  //論理演算において被演算子にNULLを指定する
  [Test()]
  public void NullOperand() {
    val nullVal = null;

    //AND
    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM") == 10 & null);
    Assert.AreEqual("NUM = 10", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(null & val.of("NUM") == 10);
    Assert.AreEqual("NUM = 10", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    //aQuery.And(nullVal And nullVal)
    Assert.AreEqual("", aQuery.GetWhereExp());

    //OR
    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM") == 10 | null);
    Assert.AreEqual("NUM = 10", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(null | val.of("NUM") == 10);
    Assert.AreEqual("NUM = 10", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    //aQuery.And(nullVal Or nullVal)
    Assert.AreEqual("", aQuery.GetWhereExp());

    //XOR
    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("NUM") == 10 ^ null);
    Assert.AreEqual("NUM = 10", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(null ^ val.of("NUM") == 10);
    Assert.AreEqual("NUM = 10", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    //aQuery.And(nullVal Xor nullVal)
    Assert.AreEqual("", aQuery.GetWhereExp());
  }

  //論理演算の括弧対応
  [Test()]
  public void BracketsOnLogicalOp() {
    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("CHAR") == "AAA" | val.of("CHAR") == "BBB" | val.of("CHAR") == "CCC");
    Assert.AreEqual("(CHAR = AAA OR CHAR = BBB OR CHAR = CCC)", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("CHAR") == "AAA" & val.of("CHAR") == "BBB" | val.of("CHAR") == "CCC");
    Assert.AreEqual("(CHAR = AAA AND CHAR = BBB OR CHAR = CCC)", aQuery.GetWhereExp());

    aQuery = new Query<ColumnInfo>();
    aQuery.And(val.of("CHAR") == "AAA" | val.of("CHAR") == "BBB" & val.of("CHAR") == "CCC");
    Assert.AreEqual("(CHAR = AAA OR CHAR = BBB AND CHAR = CCC)", aQuery.GetWhereExp());
  }

  //シリアライズ
  [Test()]
  public void Seriaraizable()
	{
		aQuery = new Query<Person>();
		aQuery.MaxRows(7);
		aQuery.RowRange(8, 9);
		aQuery.OrderBy("id","name","birthDay");
		aQuery.And(val.of("name") == "源頼朝" & 
          val.of("birthDay") != DateTime.Now & 
          !(val.of("id").In(1, 2, 3) | val.of("id").Between(1, 3)) ^ 
          val.of("name").Like("源%") &
          (val.of("birthDay") <  DateTime.Now |
           val.of("birthDay") <= DateTime.Now |
           val.of("birthDay") >  DateTime.Now |
           val.of("birthDay") <= DateTime.Now));

		this.Serialize(aQuery);
		aQuery = null;
		aQuery = (Query<Person>)this.Deserialize();
	}

  private void Serialize(object obj) {
    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
    using(System.IO.FileStream fs = new System.IO.FileStream(tempDir + "\\test.bin", System.IO.FileMode.Create)) {
      bf.Serialize(fs, obj);
    }
  }

  private object Deserialize() {
    object ret = null;
    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
    using(System.IO.FileStream fs = new System.IO.FileStream(tempDir + "\\test.bin", System.IO.FileMode.Open)) {
      ret = bf.Deserialize(fs);
    }
    return ret;
  }
}
