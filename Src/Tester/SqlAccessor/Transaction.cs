using System;
using System.Collections.Generic;
using MiniSqlParser;
using SqlAccessor;
using NUnit.Framework;

namespace SqlAccessorTester
{
  [TestFixture()]
  class Transaction
  {
    private SqlBuilder.DbmsType _dbms = SqlBuilder.DbmsType.MsSql;
    private string _connectStr = "Data Source=(localdb)\\v11.0";
    private string _sqlPodsDir = "SqlPods";

    private readonly string N = "";
    private DbParameters _dbParam;

    public Transaction() {
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
      if(_dbms == SqlBuilder.DbmsType.MsSql) {
        N = "N";
      }
      this.CreateTestRecords();
    }

    private Dictionary<string, Person> _persons = new Dictionary<string, Person>();

    private void CreateTestRecords() {
      var p1 = new Person(1, "織田信長", new DateTime(1534, 6, 23), 160.34M, 50M, false, "天下布武");
      _persons.Add("織田信長", p1);
      var p2 = new Person(2, "羽柴秀吉", new DateTime(1537, 3, 17), 150.93M, 75M, false, "太閤殿下");
      _persons.Add("羽柴秀吉", p2);
      var p3 = new Person(3, "徳川家康", new DateTime(1543, 1, 31), 160.93M, 85M, false, "権現様");
      _persons.Add("徳川家康", p3);
    }

    private void SaveTestRecords() {
      using(var db = new Db(_dbms, _connectStr)) {
        Assert.That(db.Save(_persons["織田信長"]), Is.EqualTo(1));
        Assert.That(db.Save(_persons["羽柴秀吉"]), Is.EqualTo(1));
        Assert.That(db.Save(_persons["徳川家康"]), Is.EqualTo(1));
      }
    }

    [SetUp()]
    public void initTest() {
      bool b = System.Runtime.GCSettings.IsServerGC;
      this.SaveTestRecords();
    }

    [TearDown()]
    public void finalTest() {
      using(var db = new Db(_dbms, _connectStr)) {
        db.Delete(new Person());
        db.Delete(new Schedule());
      }
    }

    [Test()]
    public void OperationOrder1() {
      // DB接続生成 → Tran生成 → コントロールにバインド → Tran破棄
      var db = new Db(_dbms, _connectStr, _dbParam);
      var tran = db.CreateTran();
      var reader = tran.Find(new Person());
      this.ReadAll(reader);
      tran.Dispose();

      // Tran生成 → Itr生成 → コントロールにバインド → ITr破棄 → Tran破棄 →
      //          → Tran生成 → Itr生成 → コントロールにバインド → Itr破棄 → Tran破棄
      tran = db.CreateTran();
      reader = tran.Find(new Person());
      this.ReadAll(reader);
      reader.Dispose();
      tran.Dispose();
      tran = db.CreateTran();
      reader = tran.Find(new Person());
      this.ReadAll(reader);
      reader.Dispose();
      tran.Dispose();

      // Tran生成 → Itr生成 → コントロールにバインド →            Tran破棄 →
      //          → Tran生成 → Itr生成 → コントロールにバインド → Itr破棄 → Tran破棄
      tran = db.CreateTran();
      reader = tran.Find(new Person());
      this.ReadAll(reader);
      tran.Dispose();
      tran = db.CreateTran();
      reader = tran.Find(new Person());
      this.ReadAll(reader);
      reader.Dispose();
      tran.Dispose();

      // Tran生成 → Itr生成 → コントロールにバインド → ITr破棄 → 
      //          → Tran生成 → Itr生成 → コントロールにバインド → Itr破棄 → Tran破棄
      tran = db.CreateTran();
      reader = tran.Find(new Person());
      this.ReadAll(reader);
      reader.Dispose();
      tran = db.CreateTran();
      reader = tran.Find(new Person());
      this.ReadAll(reader);
      reader.Dispose();
      tran.Dispose();

      // Tran生成 → Itr生成 → コントロールにバインド → ITr破棄 → Tran破棄 →
      //          → Tran生成 → Itr生成 → コントロールにバインド            → Tran破棄
      tran = db.CreateTran();
      reader = tran.Find(new Person());
      this.ReadAll(reader);
      reader.Dispose();
      tran.Dispose();
      tran = db.CreateTran();
      reader = tran.Find(new Person());
      this.ReadAll(reader);
      tran.Dispose();

      // Tran生成 → Itr生成 → コントロールにバインド → ITr破棄 → Tran破棄 →
      //          → Tran生成 → Itr生成 → コントロールにバインド → Itr破棄
      tran = db.CreateTran();
      reader = tran.Find(new Person());
      this.ReadAll(reader);
      reader.Dispose();
      tran.Dispose();
      tran = db.CreateTran();
      reader = tran.Find(new Person());
      this.ReadAll(reader);
      reader.Dispose();

      // Tran生成 → Itr生成 → Itr2生成 → [1つのトランザクションで同時にReaderを生成できない]
      var tran1 = db.CreateTran();
      reader = tran1.Find(new Person());
      var reader2 = tran1.Find(new Person());
      Assert.Throws<InvalidOperationException>(() => { this.ReadAll(reader); });

      // Tran生成 → Itr生成 → Tran2生成 → Itr2生成 → Itr2破棄 → Tran2破棄 →Itr破棄 → Tran破棄 → DB接続破棄
      tran1 = db.CreateTran();
      reader = tran1.Find(new Person());
      var tran2 = db.CreateTran();
      reader2 = tran2.Find(new Person());
      this.ReadAll(reader);
      this.ReadAll(reader2);
      reader2.Dispose();
      tran2.Dispose();
      reader.Dispose();
      tran1.Dispose();
      db.Dispose();
    }

    [Test()]
    public void OperationOrder2() {
      int repeatTime = 10;

      //DB接続生成 → Tran生成 → 全要素をイテレート → Tran破棄 → DB接続破棄
      var aDb1 = new Db(_dbms, _connectStr, _dbParam);
      var aTran1 = aDb1.CreateTran();
      var aReader1 = aTran1.Find(new Person());
      this.ReadAll(aReader1);
      aTran1.Dispose();
      aDb1.Dispose();

      //(DB接続生成 → DB接続破棄) x repeatTime
      for(int i=0; i<repeatTime; ++i){
        aDb1 = new Db(_dbms, _connectStr, _dbParam);
        aDb1.Dispose();
      }

      //(DB接続生成 → Tran生成 → Itr生成 → ITr破棄 → Tran破棄 → DB接続破棄) x repeatTime
      for(int i=0; i<repeatTime; ++i){
        aDb1 = new Db(_dbms, _connectStr, _dbParam);
        aTran1 = aDb1.CreateTran();
        aReader1 = aTran1.Find(new Person());
        aReader1.Dispose();
        aTran1.Dispose();
        aDb1.Dispose();
      }

      //(DB接続生成 → Tran生成 →                    → Tran破棄 → DB接続破棄) x repeatTime
      for(int i=0; i<repeatTime; ++i){
        aDb1 = new Db(_dbms, _connectStr, _dbParam);
        aTran1 = aDb1.CreateTran();
        //aTestItr = aTran1.Find(new Person());
        //aTestItr.dispose();
        aTran1.Dispose();
        aDb1.Dispose();
      }

      //(DB接続生成 → Tran生成 → Itr生成 → ITr破棄 →             DB接続破棄) x repeatTime
      for(int i = 0; i < repeatTime; ++i) {
        aDb1 = new Db(_dbms, _connectStr);
        aTran1 = aDb1.CreateTran();
        aReader1 = aTran1.Find(new Person());
        aReader1.Dispose();
        //aTran1.Dispose();
        aDb1.Dispose();
      }

      //(DB接続生成 → Tran生成 → Itr生成 →            Tran破棄 → DB接続破棄) x repeatTime
      for(int i=0; i<repeatTime; ++i){
        aDb1 = new Db(_dbms, _connectStr, _dbParam);
        aTran1 = aDb1.CreateTran();
        aReader1 = aTran1.Find(new Person());
        //aTestItr.dispose();
        aTran1.Dispose();
        aDb1.Dispose();
      }

      //(DB接続生成 → Tran生成 → Itr生成 →                        DB接続破棄) x repeatTime
      for(int i = 0; i < repeatTime; ++i) {
        aDb1 = new Db(_dbms, _connectStr);
        aTran1 = aDb1.CreateTran();
        aReader1 = aTran1.Find(new Person());
        //aTestItr.dispose();
        //aTran1.Dispose();
        aDb1.Dispose();
      }

      // DB接続生成 →(Tran生成 → Itr生成 → ITr破棄 → Tran破棄)→ DB接続破棄  x repeatTime
      aDb1 = new Db(_dbms, _connectStr, _dbParam);
      for(int i=0; i<repeatTime; ++i){
        aTran1 = aDb1.CreateTran();
        aReader1 = aTran1.Find(new Person());
        aReader1.Dispose();
        aTran1.Dispose();
      }
      aDb1.Dispose();

      // DB接続生成 →(Tran生成 →                    → Tran破棄)→ DB接続破棄  x repeatTime
      aDb1 = new Db(_dbms, _connectStr, _dbParam);
      for(int i=0; i<repeatTime; ++i){
        aTran1 = aDb1.CreateTran();
        //aTestItr = aTran1.Find(new Person());
        //aTestItr.dispose();
        aTran1.Dispose();
      }
      aDb1.Dispose();

      // DB接続生成 →(Tran生成 → Itr生成 → ITr破棄            )→ DB接続破棄  x repeatTime
      aDb1 = new Db(_dbms, _connectStr);
      for(int i = 0; i < repeatTime; ++i) {
        aTran1 = aDb1.CreateTran();
        aReader1 = aTran1.Find(new Person());
        aReader1.Dispose();
        //aTran1.Dispose();
      }
      aDb1.Dispose();

      // DB接続生成 →(Tran生成 → Itr生成 →            Tran破棄)→ DB接続破棄  x repeatTime
      aDb1 = new Db(_dbms, _connectStr, _dbParam);
      for(int i=0; i<repeatTime; ++i){
        aTran1 = aDb1.CreateTran();
        aReader1 = aTran1.Find(new Person());
        //aTestItr.dispose();
        aTran1.Dispose();
      }
      aDb1.Dispose();

      // DB接続生成 →(Tran生成 → Itr生成 →                    )→ DB接続破棄  x repeatTime
      aDb1 = new Db(_dbms, _connectStr);
      for(int i = 0; i < repeatTime; ++i) {
        aTran1 = aDb1.CreateTran();
        aReader1 = aTran1.Find(new Person());
        //aTestItr.dispose();
        //aTran1.Dispose();
      }
      aDb1.Dispose();

      // DB接続生成 → Tran生成 →(Itr生成 → ITr破棄)x repeatTime → Tran破棄 → DB接続破棄
      aDb1 = new Db(_dbms, _connectStr, _dbParam);
      aTran1 = aDb1.CreateTran();
      for(int i=0; i<repeatTime; ++i){
        aReader1 = aTran1.Find(new Person());
        aReader1.Dispose();
      }
      aTran1.Dispose();
      aDb1.Dispose();

      // DB接続生成 → Tran生成 →(Itr生成 →        )x repeatTime → Tran破棄 → DB接続破棄
      aDb1 = new Db(_dbms, _connectStr, _dbParam);
      aTran1 = aDb1.CreateTran();
      for(int i=0; i<repeatTime; ++i){
        aReader1 = aTran1.Find(new Person());
        //aTestItr.dispose();
      }
      aTran1.Dispose();
      aDb1.Dispose();


      //DB接続生成 → Tran生成 → Itr生成 → 全要素をイテレート → ITr破棄 → Tran破棄 →
      //           → Tran生成 → Itr生成 → 全要素をイテレート → Itr破棄 → Tran破棄 → DB接続破棄
      aDb1 = new Db(_dbms, _connectStr, _dbParam);
      aTran1 = aDb1.CreateTran();
      aReader1 = aTran1.Find(new Person());
      this.ReadAll(aReader1);

      aReader1.Dispose();
      aTran1.Dispose();
      aTran1 = aDb1.CreateTran();
      aReader1 = aTran1.Find(new Person());
      this.ReadAll(aReader1);

      aReader1.Dispose();
      aTran1.Dispose();
      aDb1.Dispose();


      //DB接続生成 → Tran生成 → Itr生成 → 全要素をイテレート →            Tran破棄 →
      //           → Tran生成 → Itr生成 → 全要素をイテレート → Itr破棄 → Tran破棄 → DB接続破棄
      aDb1 = new Db(_dbms, _connectStr, _dbParam);
      aTran1 = aDb1.CreateTran();
      aReader1 = aTran1.Find(new Person());
      this.ReadAll(aReader1);

      //aTestItr.dispose();
      aTran1.Dispose();
      aTran1 = aDb1.CreateTran();
      aReader1 = aTran1.Find(new Person());
      this.ReadAll(aReader1);

      aReader1.Dispose();
      aTran1.Dispose();
      aDb1.Dispose();


      //DB接続生成 → Tran生成 → Itr生成 → 全要素をイテレート → ITr破棄 → 
      //           → Tran生成 → Itr生成 → 全要素をイテレート → Itr破棄 → Tran破棄 → DB接続破棄
      aDb1 = new Db(_dbms, _connectStr, _dbParam);
      aTran1 = aDb1.CreateTran();
      aReader1 = aTran1.Find(new Person());
      this.ReadAll(aReader1);

      aReader1.Dispose();
      //aTran1.Dispose();
      aTran1 = aDb1.CreateTran();
      aReader1 = aTran1.Find(new Person());
      this.ReadAll(aReader1);

      aReader1.Dispose();
      aTran1.Dispose();
      aDb1.Dispose();


      //DB接続生成 → Tran生成 → Itr生成 → 全要素をイテレート → ITr破棄 → Tran破棄 →
      //                       → Itr生成 → 全要素をイテレート → Itr破棄 → Tran破棄 → DB接続破棄
      aDb1 = new Db(_dbms, _connectStr);
      aTran1 = aDb1.CreateTran();
      aReader1 = aTran1.Find(new Person());
      this.ReadAll(aReader1);

      aReader1.Dispose();
      aTran1.Dispose();
      Assert.Throws<InvalidOperationException>(() => { aReader1 = aTran1.Find(new Person()); });
      Assert.Throws<InvalidOperationException>(() => { this.ReadAll(aReader1); });

      aReader1.Dispose();
      aTran1.Dispose();
      aDb1.Dispose();

      //DB接続生成 → Tran生成 → Itr生成 → 全要素をイテレート → ITr破棄 → Tran破棄 →
      //           → Tran生成            → 全要素をイテレート → Itr破棄 → Tran破棄 → DB接続破棄
      aDb1 = new Db(_dbms, _connectStr);
      aTran1 = aDb1.CreateTran();
      aReader1 = aTran1.Find(new Person());
      this.ReadAll(aReader1);

      aReader1.Dispose();
      aTran1.Dispose();
      aTran1 = aDb1.CreateTran();
      //aTestItr = aTran1.Find(new Person());
      Assert.Throws<InvalidOperationException>(() => { this.ReadAll(aReader1); });

      aReader1.Dispose();
      aTran1.Dispose();
      aDb1.Dispose();


      //DB接続生成 → Tran生成 → Itr生成 → 全要素をイテレート → ITr破棄 → Tran破棄 →
      //           → Tran生成 → Itr生成 → 全要素をイテレート            → Tran破棄 → DB接続破棄
      aDb1 = new Db(_dbms, _connectStr, _dbParam);
      aTran1 = aDb1.CreateTran();
      aReader1 = aTran1.Find(new Person());
      this.ReadAll(aReader1);

      aReader1.Dispose();
      aTran1.Dispose();
      aTran1 = aDb1.CreateTran();
      aReader1 = aTran1.Find(new Person());
      this.ReadAll(aReader1);

      //aTestItr.dispose();
      aTran1.Dispose();
      aDb1.Dispose();


      //DB接続生成 → Tran生成 → Itr生成 → 全要素をイテレート → ITr破棄 → Tran破棄 →
      //           → Tran生成 → Itr生成 → 全要素をイテレート → Itr破棄             → DB接続破棄
      aDb1 = new Db(_dbms, _connectStr, _dbParam);
      aTran1 = aDb1.CreateTran();
      aReader1 = aTran1.Find(new Person());
      this.ReadAll(aReader1);

      aReader1.Dispose();
      aTran1.Dispose();
      aTran1 = aDb1.CreateTran();
      aReader1 = aTran1.Find(new Person());
      this.ReadAll(aReader1);

      aReader1.Dispose();
      //aTran1.Dispose();
      aDb1.Dispose();


      //DB接続生成 → Tran生成 → Itr生成 → Itr2生成 → Itr2破棄 → Itr破棄 → Tran破棄 → DB接続破棄

      aDb1 = new Db(_dbms, _connectStr, _dbParam);
      aTran1 = aDb1.CreateTran();
      aReader1 = aTran1.Find(new Person());
      var aReader2 = aTran1.Find(new Person());

      this.ReadAll(aReader2);

      aReader1.Dispose();
      aReader2.Dispose();
      aReader1.Dispose();
      aTran1.Dispose();
      aDb1.Dispose();


      //DB接続生成 → Tran生成 → Itr生成 → Tran2生成 → Itr2生成 → Itr2破棄 → Tran2破棄 →Itr破棄 → Tran破棄 → DB接続破棄
      aDb1 = new Db(_dbms, _connectStr, _dbParam);
      aTran1 = aDb1.CreateTran();
      aReader1 = aTran1.Find(new Person());
      var aTran2 = aDb1.CreateTran();
      aReader2 = aTran2.Find(new Person());

      this.ReadAll(aReader2);

      aReader2.Dispose();
      aTran2.Dispose();
      aReader1.Dispose();
      aTran1.Dispose();
      aDb1.Dispose();
    }

    [Test()]
    public void DuplicateDispose() {
      // DB接続 → DB破棄 → DB破棄
      var aDb1 = new Db(_dbms, _connectStr, _dbParam);
      aDb1.Dispose();
      aDb1.Dispose();

      // DB接続 → Tran生成 → Tran破棄 → Tran破棄
      aDb1 = new Db(_dbms, _connectStr, _dbParam);
      var aTran1 = aDb1.CreateTran();
      aTran1.Dispose();
      aTran1.Dispose();

      // Tran生成 → Itr生成 → Itr破棄 → Itr破棄
      aTran1 = aDb1.CreateTran();
      var aReader1 = aTran1.Find(new Person());
      aReader1.Dispose();
      aReader1.Dispose();
    }

    [Test()]
    public void DuplicateInsert1() {
      // INSERTを発行し、そのままDBトランザクションを維持している期間に
      // 他ユーザが同じデータをINSERTした場合
      var person1 = new Person(4,
                              "武田晴信",
                              new DateTime(1521, 12, 1),
                              150M,
                              60.37M,
                              true,
                              "風林火山");

      using(var aDb1 = new Db(_dbms, _connectStr, _dbParam)) {
        // Tranから重複格納を試みる
        using(var aTran1 = aDb1.CreateTran()) {
          using(var aTran2 = aDb1.CreateTran()) {
            // 1回目の格納
            aTran1.Call("insert", person1);
            // 2回目の格納
            Assert.Throws<WriteToLockedRecordException>(() => { aTran2.Call("insert", person1); });
          }
        }
      }
    }


    public void DuplicateInsert2(){
      // キー重複エラーが送出されることを確認する
      var person1 = new Person(4,
                              "武田晴信",
                              new DateTime(1521, 12, 1),
                              150M,
                              60.37M,
                              true,
                              "風林火山");
      using(var aDb1 = new Db(_dbms, _connectStr, _dbParam)) {
         aDb1.Call("insert", person1);
        Assert.Throws<DuplicateKeyException>(() => { aDb1.Call("insert", person1); });
      }
    }
    
    [Test()]
    public void DisposedObject() {
      // DB接続
      var aDb1 = new Db(_dbms, _connectStr, _dbParam);

      // DB破棄 → Tran生成 → 例外を送出
      aDb1.Dispose();
      Assert.Throws<InvalidOperationException>(()=>{aDb1.CreateTran();});

      // Tran破棄 → Itr生成 → 例外を送出
      var aDb2 = new Db(_dbms, _connectStr, _dbParam);
      var aTran2 = aDb2.CreateTran();
      aTran2.Dispose();
      Assert.Throws<InvalidOperationException>(() => { aTran2.Find(new Person());});
    }

    [Test()]
    public void Rollback() {
      //       
      // テスト用レコードを格納する
      // 
      var person1 = new Person(1118,
                                "平清盛",
                                new DateTime(1918, 3, 20),
                                183.49M,
                                69.3M,
                                false,
                                "平家の棟梁");
      var person2 = new Person(1192,
                                "源頼朝",
                                new DateTime(1947, 5, 9),
                                160.789M,
                                67.234m,
                                false,
                                "鎌倉府初代将軍");
      // DB接続
      var aDb1 = new Db(_dbms, _connectStr, _dbParam);
      // レコード削除処理を行う
      aDb1.Delete(new Person());

      //
      // Rollbackができることを確認する
      //
      var aTran1 = aDb1.CreateTran();
      aTran1.Save(person1);
      aTran1.Save(person2);
      aTran1.Rollback();
      aTran1.Dispose();
      Assert.Null(aDb1.FindOne(new Person()));

      //
      // Save → Suspend → Rollback
      //
      aTran1 = aDb1.CreateTran();
      aTran1.Save(person1);
      aTran1.Save(person2);
      aTran1.Suspend();
      // サスペンドにより一旦ROLLBACKする
      Assert.Null(aDb1.FindOne(new Person()));

      //
      // ロックを解除する
      //
      aTran1.Dispose();
    }

    [Test()]
    public void Load() {
      var b = System.Runtime.GCSettings.IsServerGC;

      var random = new System.Random();

      var person1 = new Person(random.Next(999),
                                "伊達政宗",
                                new DateTime(1567, 9, 5),
                                173.4M,
                                78.39M,
                                true,
                                "独眼竜");

      using(var aDb1 = new Db(_dbms, _connectStr, _dbParam)) {
        for(int i =0; i<99; ++i){
          person1.Id = random.Next(999);
          aDb1.Save(person1);
          var reader = aDb1.Find(new Query<Person>());
          GC.Collect();
          GC.WaitForPendingFinalizers();
          reader.Dispose();
          aDb1.FindOne(person1);
          aDb1.Delete(person1);
          Console.WriteLine(i.ToString() + " time");
        }
      } // using
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



  }

}