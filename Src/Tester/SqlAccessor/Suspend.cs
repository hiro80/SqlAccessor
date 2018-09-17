using System;
using System.Collections.Generic;
using MiniSqlParser;
using SqlAccessor;
using NUnit.Framework;

namespace SqlAccessorTester
{
  [TestFixture()]
  class Suspend
  {
    private readonly SqlBuilder.DbmsType _dbms = SqlBuilder.DbmsType.MsSql;
    private readonly string _connectStr = "Data Source=(localdb)\\v11.0";
    private readonly string _sqlPodsDir = "SqlPods";

    private DbParameters _dbParam;
    private Db _db;

    public Suspend() {
      _dbParam = new DbParameters();
      _dbParam.LockData = DbParameters.LockDataType.Memory;
      _dbParam.CommitAtFinalizing = false;
      _dbParam.Cache = DbParameters.CacheType.LRU;
      _dbParam.SqlIndent = DbParameters.IndentType.Compact;
      _dbParam.DebugPrint = true;
      _dbParam.SqlPodsDir = _sqlPodsDir;
      if(_dbms == SqlBuilder.DbmsType.Sqlite) {
        _dbParam.LockData = DbParameters.LockDataType.Sqlite;
      }
    }

    [SetUp()]
    public void initTest(){
      bool b = System.Runtime.GCSettings.IsServerGC;
      _db = new Db(_dbms, _connectStr, _dbParam);
    }

    [TearDown()]
    public void finalTest(){
      if(_db != null) {
        _db.Delete(new Person());
        _db.Delete(new Schedule());
        _db.Dispose();
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

    [Test]
    public void Suspend1() {

      //DB接続生成 → Tran生成 → Tran中断 → コントロールにバインド → Tran破棄
      var aDb1 = new Db(_dbms, _connectStr, _dbParam);
      var aTran1 = aDb1.CreateTran();
      aTran1.Suspend();
      var aReader1 = aTran1.Find(new Person());
      this.ReadAll(aReader1);
      aTran1.Suspend();
      aTran1.Dispose();

      //DB接続生成 → Tran生成 → コントロールにバインド → Tran中断 → Tran破棄
      aDb1 = new Db(_dbms, _connectStr, _dbParam);
      aTran1 = aDb1.CreateTran();
      aReader1 = aTran1.Find(new Person());
      this.ReadAll(aReader1);
      aTran1.Suspend();
      aTran1.Dispose();

      //DB接続 → Tran生成 → Tran中断 → Tran中断 → Tran破棄
      aDb1 = new Db(_dbms, _connectStr, _dbParam);
      aTran1 = aDb1.CreateTran();
      aTran1.Suspend();
      aTran1.Suspend();
      aTran1.Dispose();

      //Tran生成 → Tran中断 → Itr生成 → コントロールにバインド → Tran中断 → ITr破棄 → Tran中断 → Tran破棄 →
      //         → Tran生成 → Tran中断 → Itr生成 → コントロールにバインド → Tran中断 → Itr破棄 → Tran中断 → Tran破棄
      aTran1 = aDb1.CreateTran();
      aTran1.Suspend();
      aReader1 = aTran1.Find(new Person());
      this.ReadAll(aReader1);
      aTran1.Suspend();
      aReader1.Dispose();
      aTran1.Suspend();
      aTran1.Dispose();
      aTran1 = aDb1.CreateTran();
      aTran1.Suspend();
      aReader1 = aTran1.Find(new Person());
      this.ReadAll(aReader1);
      aTran1.Suspend();
      aReader1.Dispose();
      aTran1.Suspend();
      aTran1.Dispose();

      //Tran生成 → Tran中断 → Itr生成 → コントロールにバインド → Tran中断 →            Tran破棄 →
      //         → Tran生成 → Tran中断 → Itr生成 → コントロールにバインド → Tran中断 → Itr破棄 → Tran中断 → Tran破棄
      aTran1 = aDb1.CreateTran();
      aTran1.Suspend();
      aReader1 = aTran1.Find(new Person());
      this.ReadAll(aReader1);
      //aTestItr.dispose();
      aTran1.Suspend();
      aTran1.Dispose();
      aTran1 = aDb1.CreateTran();
      aTran1.Suspend();
      aReader1 = aTran1.Find(new Person());
      this.ReadAll(aReader1);
      aTran1.Suspend();
      aReader1.Dispose();
      aTran1.Suspend();
      aTran1.Dispose();

      //Tran生成 → Tran中断 → Itr生成 → コントロールにバインド → Tran中断 → ITr破棄 → 
      //         → Tran生成 → Tran中断 → Itr生成 → コントロールにバインド → Tran中断 → Itr破棄 → Tran中断 → Tran破棄
      aTran1 = aDb1.CreateTran();
      aTran1.Suspend();
      aReader1 = aTran1.Find(new Person());
      this.ReadAll(aReader1);
      aTran1.Suspend();
      aReader1.Dispose();
      //aTran1.Dispose();
      aTran1 = aDb1.CreateTran();
      aTran1.Suspend();
      aReader1 = aTran1.Find(new Person());
      this.ReadAll(aReader1);
      aTran1.Suspend();
      aReader1.Dispose();
      aTran1.Dispose();

      //Tran生成 → Tran中断 → Itr生成 → コントロールにバインド → Tran中断 → ITr破棄 → Tran破棄 →
      //         → Tran生成 → Tran中断 → Itr生成 → コントロールにバインド → Tran中断            → Tran破棄
      aTran1 = aDb1.CreateTran();
      aTran1.Suspend();
      aReader1 = aTran1.Find(new Person());
      this.ReadAll(aReader1);
      aTran1.Suspend();
      aReader1.Dispose();
      aTran1.Dispose();
      aTran1 = aDb1.CreateTran();
      aTran1.Suspend();
      aReader1 = aTran1.Find(new Person());
      this.ReadAll(aReader1);
      aTran1.Suspend();
      //aTestItr.dispose();
      aTran1.Dispose();

      //Tran生成 → Tran中断 → Itr生成 → コントロールにバインド → Tran中断 → ITr破棄 → Tran破棄 →
      //         → Tran生成 → Tran中断 → Itr生成 → コントロールにバインド → Tran中断 → Itr破棄
      aTran1 = aDb1.CreateTran();
      aTran1.Suspend();
      aReader1 = aTran1.Find(new Person());
      this.ReadAll(aReader1);
      aTran1.Suspend();
      aReader1.Dispose();
      aTran1.Dispose();
      aTran1 = aDb1.CreateTran();
      aTran1.Suspend();
      aReader1 = aTran1.Find(new Person());
      this.ReadAll(aReader1);
      aTran1.Suspend();
      aReader1.Dispose();
      //aTran1.Dispose();

      //Tran生成 → Tran中断 → Itr生成 → Tran中断 → Tran2生成 → Tran2中断 → Itr2生成 → Itr生成
      //Tran2中断 → Itr2破棄 → Tran2中断 → Tran2破棄 →Itr破棄 → Tran中断 → Tran破棄 → DB接続破棄
      aTran1 = aDb1.CreateTran();
      aTran1.Suspend();
      aReader1 = aTran1.Find(new Person());
      aTran1.Suspend();
      var aTran2 = aDb1.CreateTran();
      aTran2.Suspend();
      var aReader2 = aTran2.Find(new Person());
      aReader1 = aTran1.Find(new Person());
      this.ReadAll(aReader1);
      aTran2.Suspend();
      aReader2.Dispose();
      aTran2.Suspend();
      aTran2.Dispose();
      aReader1.Dispose();
      aTran1.Suspend();
      aTran1.Dispose();
      aDb1.Dispose();

    }

    [Test]
    public void Suspend2() {
      var person1 = new Person(11,
                                "今川義元",
                                DateTime.Now,
                                168.54M,
                                69.56M,
                                true,
                                "海道一の弓取り");

      //DB接続生成 → Tran生成 → Tran中断 → コントロールにバインド → Tran破棄
      var aDb1 = new Db(_dbms, _connectStr, _dbParam);
      var aTran1 = aDb1.CreateTran();
      aTran1.Suspend();
      aTran1.Save(person1);
      aTran1.Suspend();
      var aReader1 = aTran1.Find(new Person());
      Assert.That(this.ReadAll(aReader1), Is.EqualTo(1));
      aTran1.Suspend();
      aTran1.Delete(new Person());
      aTran1.Suspend();
      aTran1.Dispose();

    }
  }
}
