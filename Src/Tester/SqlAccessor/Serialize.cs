using System;
using System.Collections.Generic;
using MiniSqlParser;
using SqlAccessor;
using NUnit.Framework;

namespace SqlAccessorTester
{
  [TestFixture()]
  class Serializeation
  {
    private readonly SqlBuilder.DbmsType _dbms = SqlBuilder.DbmsType.MsSql;
    private readonly string _connectStr = "Data Source=(localdb)\\v11.0";
    private readonly string _sqlPodsDir = "SqlPods";

    private readonly string tempDir = Environment.GetEnvironmentVariable("TEMP");


    private DbParameters _dbParam;

    public Serializeation() {
      _dbParam = new DbParameters();
      _dbParam.LockData = DbParameters.LockDataType.Memory;
      _dbParam.CommitAtFinalizing = false;
      _dbParam.Cache = DbParameters.CacheType.Null;
      _dbParam.SqlIndent = DbParameters.IndentType.Compact;
      _dbParam.DebugPrint = true;
      _dbParam.SqlPodsDir = _sqlPodsDir;
      if(_dbms == SqlBuilder.DbmsType.Sqlite) {
        _dbParam.LockData = DbParameters.LockDataType.Sqlite;
      }
    }

    private int ReadAll(IReader reader) {
      int i = 0;
      foreach(var rec in reader) {
        // 最適化されないようにダミーの処理を行う
        if(rec is IRecord) {
          ++i;
        }
      }
      return i;
    }

    private void Serialize(Tran aTran){
      var bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
      using(var fs = new System.IO.FileStream(tempDir + "\\test.bin", System.IO.FileMode.Create)){
        bf.Serialize(fs, aTran);
      }
    }

    private Tran Deserialize(){
      var bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
      using(var fs = new System.IO.FileStream(tempDir + "\\test.bin", System.IO.FileMode.Open)) {
        return (Tran)bf.Deserialize(fs);
      }
    }

    [Test]
    public void Serialize_Desirialize1() {
      var person1 = new Person(123,
                                "武田晴信",
                                new DateTime(1521, 12, 1),
                                150M,
                                60.37M,
                                true,
                                "風林火山");

      var aDb1 = new Db(_dbms, _connectStr, _dbParam);
      var aTran1 = aDb1.CreateTran();
      aTran1.Save(person1);

      this.Serialize(aTran1);
      aTran1 = null;
      aTran1 = this.Deserialize();

      var aReader1 = aTran1.Find(person1, Tran.LoadMode.ReadWrite);
      this.ReadAll(aReader1);
      aTran1.Delete(new Person());
      aTran1.Dispose();

    }

    [Test]
    public void Serialize_Desirialize2() {
      var testRec1 = new Person(123,
                                "武田晴信",
                                new DateTime(1521, 12, 1),
                                150M,
                                60.37M,
                                true,
                                "風林火山");

      var aDb1 = new Db(_dbms, _connectStr, _dbParam);
      var aTran1 = aDb1.CreateTran();
      aTran1.Save(testRec1);

      for(int i = 0; i<128 ; ++i){
        this.Serialize(aTran1);
        //Rollback()がないと、aTran1オブジェクトがGCにDispose()されるため
        //INSERT文がコミットされる複数のaTran1オブジェクトがDispose()されると
        //DuplicateKey例外が送出される
        aTran1.Rollback();
        aTran1 = null;
        aTran1 = this.Deserialize();
      }

      var aReader1 = aTran1.Find(testRec1, Tran.LoadMode.ReadWrite);
      this.ReadAll(aReader1);
      aTran1.Delete(new Person());
      aTran1.Dispose();
    }
  }
}
