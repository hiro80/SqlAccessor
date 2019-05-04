using System;
using System.Collections.Generic;
using MiniSqlParser;
using NUnit.Framework;
using SqlAccessor;

namespace SqlAccessorTester
{
  class FinalizingTester
  {

    //private SqlBuilder.DbmsType _dbms = SqlBuilder.DbmsType.Sqlite;
    //private string _connectStr = "Data Source=SqliteTable\\Persons.sqlite3";
    //private string _sqlPodsDir = "SqlPodsForSqlite";

    private SqlBuilder.DbmsType _dbms = SqlBuilder.DbmsType.MsSql;
    private string _connectStr = "Data Source=(localdb)\\v11.0";
    private string _sqlPodsDir = "SqlPods";

    private DbParameters _dbParam;
    private Db _db;

    public FinalizingTester() {
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

    [SetUp()]
    public void initTest() {
      bool b = System.Runtime.GCSettings.IsServerGC;
      _db = new Db(_dbms, _connectStr, _dbParam);
    }

    [TearDown()]
    public void finalTest(){
      if(_db != null) {
        _db.Delete(new Person());
        _db.Dispose();
      }
    }

    [Test]
    public void CommitAtFinalizing() {
      var param = new DbParameters();
      // GCによる終了処理ではCOMMITが行われるよう設定する
      param.CommitAtFinalizing = true;
      param.SqlPodsDir = _sqlPodsDir;
      param.DebugPrint = true;

      var db = new Db(_dbms, _connectStr, param);
      var tran = db.CreateTran();

      tran.Save(new Person(5, "長尾景虎", new DateTime(1530, 2, 18), 155.01M, 70.5M, true, "毘沙門天"));

      // Dispose()未実行のためGCにより終了処理が行われる
      tran = null;
      db = null;

      // GCの強制実行
      System.GC.Collect();

      // Save()のCOMMITが行われたのでレコードは格納されている
      var person = new Person();
      person.Id = 5;
      db = new Db(_dbms, _connectStr, param);
      Assert.That(db.Count(person), Is.EqualTo(1));

      db.Dispose();
    }

    [Test]
    public void RollbackAtFinalizing() {
      var param = new DbParameters();
      // GCによる終了処理ではROLLBACKが行われるよう設定する
      param.CommitAtFinalizing = false;
      param.SqlPodsDir = _sqlPodsDir;
      param.DebugPrint = true;

      var db = new Db(_dbms, _connectStr, param);
      var tran = db.CreateTran();

      tran.Save(new Person(5, "長尾景虎", new DateTime(1530, 2, 18), 155.01M, 70.5M, true, "毘沙門天"));

      // Dispose()未実行のためGCにより終了処理が行われる
      tran = null;
      db = null;

      // GCの強制実行
      System.GC.Collect();

      // Save()のRollbackが行われたのでレコードは格納されていない
      var person = new Person();
      person.Id = 5;
      db = new Db(_dbms, _connectStr, param);
      Assert.That(db.Count(person), Is.EqualTo(0));
    }

  }
}
