using System;
using System.Collections.Generic;
using MiniSqlParser;
using SqlAccessor;
using NUnit.Framework;

namespace SqlAccessorTester
{
  [TestFixture()]
  class Cache
  {
    private SqlBuilder.DbmsType _dbms = SqlBuilder.DbmsType.MsSql;
    private string _connectStr = "Data Source=(localdb)\\v11.0";
    private string _sqlPodsDir = "SqlPods";

    private DbParameters _dbParam;
    private Db _db;

    public Cache() {
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

    [Test]
    public void CachMode(){
      //'テストレコード追加
      var person1 = new Person(103,
                               "山本五十六",
                               new DateTime(1884, 4, 4),
                               170M,
                               60M,
                               false,
                               "真珠湾攻撃");
      _db.Save(person1);

      // キャッシュクリア
      _db.ClearCache();

      // キャッシュなし
      var person2 = _db.FindOne(person1, Tran.LoadMode.ReadOnly, Tran.CacheStrategy.NoCache);
      Assert.That(person2, Is.EqualTo(person1));

      // もしキャッシュがあれば使う(よって使わない)
      person2 = _db.FindOne(person1, Tran.LoadMode.ReadOnly, Tran.CacheStrategy.UseCacheIfExists);
      Assert.That(person2, Is.EqualTo(person1));

      // (キャッシュが無ければ作成して)キャッシュを使う(よって作成する)
      person2 = _db.FindOne(person1, Tran.LoadMode.ReadOnly, Tran.CacheStrategy.UseCache);
      Assert.That(person2, Is.EqualTo(person1));

      // もしキャッシュがあれば使う(よって使う)
      person2 = _db.FindOne(person1, Tran.LoadMode.ReadOnly, Tran.CacheStrategy.UseCacheIfExists);
      Assert.That(person2, Is.EqualTo(person1));

      // キャッシュヒット率
      Assert.That(_db.CacheHitRate, Is.GreaterThan(0.5));
    }

    [Test]
    public void UseCache() {
      //'テストレコード
      var person = new Person(103,
                              "山本五十六",
                              new DateTime(1884, 4, 4),
                              170M,
                              60M,
                              false,
                              "真珠湾攻撃");

      for(int i = 0; i < 32; ++i) {
        _db.Save(person);
        var aReader1 = _db.Find(new Person(), Tran.LoadMode.ReadOnly, Tran.CacheStrategy.UseCache);
        aReader1.Dispose();
        aReader1 = _db.Find(new Person(), Tran.LoadMode.ReadWrite, Tran.CacheStrategy.UseCache);
        aReader1.Dispose();
        var rec1  = _db.FindOne(person, Tran.LoadMode.ReadOnly, Tran.CacheStrategy.UseCache);
        var rec2  = _db.FindOne(person, Tran.LoadMode.ReadWrite, Tran.CacheStrategy.UseCache);
        _db.Delete(person);
      }
    }
  }
}
