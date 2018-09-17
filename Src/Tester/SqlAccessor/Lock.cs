using System;
using System.Collections.Generic;
using MiniSqlParser;
using SqlAccessor;
using NUnit.Framework;

namespace SqlAccessorTester
{
  [TestFixture()]
  class Lock
  {
    private SqlBuilder.DbmsType _dbms = SqlBuilder.DbmsType.MsSql;
    private string _connectStr = "Data Source=(localdb)\\v11.0";
    private string _sqlPodsDir = "SqlPods";

    private DbParameters _dbParam;
    private Db _db;

    public Lock() {
      _dbParam = new DbParameters();
      _dbParam.LockData = DbParameters.LockDataType.Db;
      _dbParam.CommitAtFinalizing = false;
      _dbParam.Cache = DbParameters.CacheType.Null;
      _dbParam.SqlIndent = DbParameters.IndentType.Beautiful;
      _dbParam.DebugPrint = true;
      _dbParam.SqlPodsDir = _sqlPodsDir;
      if(_dbms == SqlBuilder.DbmsType.Sqlite) {
        _dbParam.LockData = DbParameters.LockDataType.Sqlite;
      }
    }

    [SetUp()]
    public void initTest() {
      bool b = System.Runtime.GCSettings.IsServerGC;
      _db = new Db(_dbms, _connectStr, _dbParam);;
    }

    [TearDown()]
    public void finalTest() {
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

    private bool IsLockedRecords<TRecord>(IRecordReader<TRecord> reader){
      while(reader.MoveNext()){
        if(reader.Writable){
          // 1レコードでもロックされていなければFalseとする
          return false;
        }
      }
      return true;
    }

    [Test]
    public void Lock1() {
      //
      // テスト用レコードを格納する
      //
      var person1 = new Person(11, "毛利元就", new DateTime(1547, 4, 16), 170.1M, 67.5M, true, "三本の矢");
      var person2 = new Person(12, "毛利輝元", new DateTime(1553, 1, 22), 180.4M, 87.11M, true, "西軍大将");

      var db = new Db(_dbms, _connectStr, _dbParam);
      db.Save(person1);
      db.Save(person2);

      //
      //Personを全てロックする
      //
      var criteria1 = new Person();
      var aTran1 = db.CreateTran();
      var aReader1 = aTran1.Find(criteria1, Tran.LoadMode.ReadWrite);
      this.ReadAll(aReader1);

      //
      //Personを全て抽出し、その全てのレコードがロックされていること
      //
      var aTran2 = db.CreateTran();
      Assert.IsTrue(this.IsLockedRecords(aTran2.Find(criteria1, Tran.LoadMode.ReadWrite)));
      aTran2.Dispose();

      //
      //主キーを条件指定して抽出し、そのレコードがロックされていること
      //
      var criteria2 = new Person();
      criteria2.Id = 11;
      aTran2 = db.CreateTran();
      Assert.IsTrue(this.IsLockedRecords(aTran2.Find(criteria2, Tran.LoadMode.ReadWrite)));
      aTran2.Dispose();

      //
      //非キーを条件指定して抽出し、そのレコードがロックされていること
      //
      var criteria3 = new Person();
      criteria3.Height = 170.1M;
      aTran2 = db.CreateTran();
      Assert.IsTrue(this.IsLockedRecords(aTran2.Find(criteria3, Tran.LoadMode.ReadWrite)));
      aTran2.Dispose();

      //
      //主キーと非キーを条件指定して抽出し、そのレコードがロックされていること
      //
      var criteria4 = new Person();
      criteria4.Id = 11;
      criteria4.Height = 170.1M;
      aTran2 = db.CreateTran();
      Assert.IsTrue(this.IsLockedRecords(aTran2.Find(criteria4, Tran.LoadMode.ReadWrite)));
      aTran2.Dispose();

      //
      //複数の値を条件指定して抽出し、そのレコードがロックされていること
      //
      var criteria5 = new Person();
      criteria5.Name = "毛利輝元";
      criteria5.BirthDay = new DateTime(1553, 1, 22);
      criteria5.Weight = 87.11M;
      criteria5.IsDaimyou = true;
      aTran2 = db.CreateTran();
      Assert.IsTrue(this.IsLockedRecords(aTran2.Find(criteria5, Tran.LoadMode.ReadWrite)));
      aTran2.Dispose();

      //
      //ロック中のレコードを更新すると、WriteToLockedRecordException例外が送出されること
      //
      Assert.Throws<WriteToLockedRecordException>(()=>{ var criteria6 = new Person();
                                                        criteria6.Id = 11;
                                                        var aTran= db.CreateTran();
                                                        aTran.Save(criteria6);
                                                        aTran.Dispose();});

      //
      //存在しないレコードを更新すると、例外は送出されないこと
      //
      Assert.DoesNotThrow(() =>{ var criteria7 = new Person();
                                criteria7.Id = 13;
                                var aTran = db.CreateTran();
                                aTran.Save(criteria7);
                                aTran.Dispose();});

      //
      //他レコードをロックして、そのレコードを更新
      //
      Assert.Throws<WriteToLockedRecordException>(() => { var rec = new Person();
                                                          Person updRec;

                                                          var aTran = db.CreateTran();
                                                          var reader = aTran.Find(rec, Tran.LoadMode.ReadWrite);
                                                          reader.MoveNext();
                                                          updRec = reader.Current;

                                                          var aTran3 = db.CreateTran();
                                                          try{
                                                            int i = aTran3.Save(updRec);
                                                          }finally{
                                                            aTran.Dispose();
                                                            aTran3.Dispose();
                                                          }});

      //
      //ロックを解除する
      //
      aTran1.Dispose();

    }

    [Test]
    public void Lock2() {
      //
      // テスト用レコードを格納する
      //
      var person1 = new Person(11, "毛利元就", new DateTime(1547, 4, 16), 170.1M, 67.5M, true, "三本の矢");
      var person2 = new Person(12, "毛利輝元", new DateTime(1553, 1, 22), 180.4M, 87.11M, true, "西軍大将");

      var db = new Db(_dbms, _connectStr, _dbParam);
      db.Save(person1);
      db.Save(person2);

      //
      //1つめのレコードのみロックする
      //
      var criteria1 = new Person();
      criteria1.Id = 11;
      var aTran1 = db.CreateTran();
      var aReader1 = aTran1.Find(criteria1, Tran.LoadMode.ReadWrite);
      this.ReadAll(aReader1);

      //
      //1つめのレコードを抽出し、そのレコードがロックされていること
      //
      var criteria2 = new Person();
      criteria2.Id = 11;
      var aTran2 = db.CreateTran();
      Assert.True(this.IsLockedRecords(aTran2.Find(criteria2, Tran.LoadMode.ReadWrite)));
      aTran2.Dispose();

      //
      //2つめのレコードを抽出し、そのレコードがロックされていないこと
      //
      var criteria3 = new Person();
      criteria3.Id = 12;
      aTran2 = db.CreateTran();
      Assert.IsFalse(this.IsLockedRecords(aTran2.Find(criteria3, Tran.LoadMode.ReadWrite)));
      aTran2.Dispose();

      //
      //ロックを解除する
      //
      aTran1.Dispose();

      //
      //ロック解除後に全てのレコードを抽出し、ロックされていないこと
      //
      var criteria4 = new Person();
      aTran2 = db.CreateTran();
      Assert.IsFalse(this.IsLockedRecords(aTran2.Find(criteria4, Tran.LoadMode.ReadWrite)));
      aTran2.Dispose();
    }

    [Test]
    public void IsFalseExpression() {
      var criteria = new Query<Person>();
      criteria.And(val.of("Id") == 1 & val.of("Id") == 2);

      // WHERE句が恒偽式の場合、例外が発生しないこと
      // かつ、恒偽式の対象テーブルへのロックデータが作成されないこと
      using(var tran = _db.CreateTran()) {
        var reader = tran.Call("delete", criteria);
      }
    }

    [Test]
    public void SelectInSave() {
      var record = new ScheduleSelectInSave2(11
                                            , "足利 義政"
                                            , new DateTime(1436, 1, 20)
                                            , 160
                                            , null
                                            , true
                                            , "銀閣寺"
                                            , new DateTime(1490, 1, 27)
                                            , "崩御");
      using(var tran = _db.CreateTran()) {
        var i = tran.Save(record);
      }
    }
  }
}
