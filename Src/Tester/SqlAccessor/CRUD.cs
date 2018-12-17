using System;
using System.Collections.Generic;
using MiniSqlParser;
using NUnit.Framework;
using SqlAccessor;

namespace SqlAccessorTester
{
  [TestFixture()]
  class CRUD
  {
    private readonly string N = "";

    //private SqlBuilder.DbmsType _dbms = SqlBuilder.DbmsType.Sqlite;
    //private string _connectStr = "Data Source=SqliteTable\\Persons.sqlite3";
    //private string _sqlPodsDir = "SqlPodsForSqlite";

    private SqlBuilder.DbmsType _dbms = SqlBuilder.DbmsType.MsSql;
    private string _connectStr = "Data Source=(localdb)\\v11.0";
    private string _sqlPodsDir = "SqlPods";

    private DbParameters _dbParam;
    private Db _db;

    private Dictionary<string, Person> _persons = new Dictionary<string, Person>();

    public CRUD(){
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

    [SetUp()]
    public void initTest(){
      bool b = System.Runtime.GCSettings.IsServerGC;
      _db = new Db(_dbms,_connectStr,_dbParam);
      this.SaveTestRecords();
    }

    private void CreateTestRecords() {
      var p1 = new Person(1, "織田信長", new DateTime(1534, 6, 23), 160.34M, 50M, false, "天下布武");
      _persons.Add("織田信長", p1);
      var p2 = new Person(2, "羽柴秀吉", new DateTime(1537, 3, 17), 150.93M, 75M, false, "太閤殿下");
      _persons.Add("羽柴秀吉", p2);
      var p3 = new Person(3, "徳川家康", new DateTime(1543, 1, 31), 160.93M, 85M, false, "権現様");
      _persons.Add("徳川家康", p3);
    }

    private void SaveTestRecords() {
      Assert.That(_db.Save(_persons["織田信長"]), Is.EqualTo(1));
      Assert.That(_db.Save(_persons["羽柴秀吉"]), Is.EqualTo(1));
      Assert.That(_db.Save(_persons["徳川家康"]), Is.EqualTo(1));
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

    [Test()]
    public void SavePerson() {
      var p4 = new PersonIf(4, "武田晴信", new DateTime(1521, 12, 1), 165.47M, 73M, false, "風林火山");

      using(var t1 = _db.CreateTran()) {
        Assert.That(t1.Save(p4), Is.EqualTo(1));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("INSERT INTO Persons(id,name,birthDay,height,weight,isDaimyou,remarks) " +
                               "VALUES(4," + N + "'武田晴信','1521-12-01',165.47,73,0," + N + "'風林火山')"));
      }
    }

    [Test()]
    public void SavePersonWithPlaceholders() {
      var p4 = new PersonIf(4, "武田晴信", new DateTime(1521, 12, 1), 165.47M, 73M, false, "風林火山");
      var ph = new Dictionary<string,string>(){{"Remarks", N + "'人が石垣人が城'"}};

      using(var t1 = _db.CreateTran()) {
        // プレースホルダで指定した値が最優先される
        Assert.That(t1.Save(p4, ph), Is.EqualTo(1));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("INSERT INTO Persons(id,name,birthDay,height,weight,isDaimyou,remarks) " +
                               "VALUES(4," + N + "'武田晴信','1521-12-01',165.47,73,0," + N + "'人が石垣人が城')"));
      }
    }

    [Test()]
    public void SaveAndFind() {
      // Saveしたレコードをデータ無損失で抽出できるか

      //テストレコード追加
      var person1 = new Person(103,
                               "山本五十六",
                               new DateTime(1884, 4, 4),
                               170M,
                               60M,
                               false,
                               "真珠湾攻撃");
    _db.Save(person1);
    var person2 = _db.FindOne(person1);
    Assert.That(person1, Is.EqualTo(person2));
    }


    [Test()]
    public void FindWithQuery(){

      using(var t1 = _db.CreateTran()) {

        var query = new Query<Person>();
        query.And(val.of("Id") == 1); 
        Assert.That(t1.FindOne(query), Is.EqualTo(_persons["織田信長"]));
        Assert.That(t1.LastExecutedSql, 
                    Is.EqualTo("SELECT * FROM (SELECT id AS Id,name AS Name,birthDay AS BirthDay,height" + 
                                " AS Height,weight AS Weight,isDaimyou AS IsDaimyou,remarks AS Remarks" +
                                Environment.NewLine + "FROM Persons)V0_ WHERE Id=1"));

        query = new Query<Person>();
        query.And(val.of("Id") == 1 &
                  val.of("Name") == "織田信長" &
                  val.of("BirthDay") == new DateTime(1534, 6, 23) &
                  val.of("Height") == 160.34m &
                  val.of("Weight") == 50m &
                  val.of("IsDaimyou") == false &
                  val.of("Remarks") == "天下布武");
        Assert.That(t1.FindOne(query), Is.EqualTo(_persons["織田信長"]));
        Assert.That(t1.LastExecutedSql, 
                    Is.EqualTo("SELECT * FROM (SELECT id AS Id,name AS Name,birthDay AS BirthDay,height" + 
                                " AS Height,weight AS Weight,isDaimyou AS IsDaimyou,remarks AS Remarks" +
                                Environment.NewLine + "FROM Persons)V0_ " +
                                "WHERE Id=1 AND Name=" + N + "'織田信長' AND BirthDay='1534-06-23'" +
                                " AND Height=160.34 AND Weight=50 AND IsDaimyou=0 AND Remarks=" + N + "'天下布武'"));

        query = new Query<Person>();
        query.And(val.of("Id").Between(2, 2) &
                  val.of("Name") != "織田信長" &
                  val.of("BirthDay").In(new DateTime(1537, 3, 17)) &
                  val.of("Height") <= 160.34m &
                  val.of("Weight") >= 50m &
                  val.of("IsDaimyou") == false &
                  val.of("Remarks").Like("%殿下"));
        Assert.That(t1.FindOne(query), Is.EqualTo(_persons["羽柴秀吉"]));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("SELECT * FROM (SELECT id AS Id,name AS Name,birthDay AS BirthDay,height" +
                               " AS Height,weight AS Weight,isDaimyou AS IsDaimyou,remarks AS Remarks" +
                               Environment.NewLine + "FROM Persons)V0_ " +
                               "WHERE Id BETWEEN 2 AND 2 AND Name<>" + N + "'織田信長' AND BirthDay IN('1537-03-17')" +
                               " AND Height<=160.34 AND Weight>=50 AND IsDaimyou=0 AND" +
                               Environment.NewLine + "Remarks LIKE " + N + "'%殿下'"));

        // 
        // !演算子を用いると未定義エラーになるので暫定的にコメントアウトしておく
        //

        //query = new Query<Person>();
        //query.And(val.of("Id").In(1, 2, 3) &
        //          (val.of("Name") == "権現様" | val.of("Name") == "大御所" | val.of("Name") == "徳川家康") &
        //          (new DateTime(1543, 1, 31)) == val.of("BirthDay") &
        //          val.of("Height") < 160.34m &
        //          val.of("Weight") > 50m &
        //          false == val.of("IsDaimyou") &
        //          !(val.of("Remarks").Like("%権現様%")));
        //Assert.That(t1.FindOne(query), Is.EqualTo(_persons["徳川家康"]));
        //Assert.That(t1.LastExecutedSql,
        //            Is.EqualTo(""));

        query = new Query<Person>();
        query.And(val.of("Name") == "権現様" ^ val.of("Name") == "大御所様" ^ val.of("Name") == "徳川家康");
        Assert.That(t1.FindOne(query), Is.EqualTo(_persons["徳川家康"]));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("SELECT * FROM (SELECT id AS Id,name AS Name,birthDay AS BirthDay,height" +
                               " AS Height,weight AS Weight,isDaimyou AS IsDaimyou,remarks AS Remarks" +
                               Environment.NewLine + "FROM Persons)V0_ " +
                               "WHERE ((((Name=" + N + "'権現様' OR Name=" + N + "'大御所様') AND NOT " +
                               "(Name=" + N + "'権現様' AND Name=" + N + "'大御所様')) OR Name=" + N + "'徳川家康') AND " +
                               "NOT (((Name=" + N + "'権現様' OR Name=" +
                                Environment.NewLine + "" + N + "'大御所様') AND " + 
                               "NOT (Name=" + N + "'権現様' AND Name=" + N + "'大御所様')) AND Name=" + N + "'徳川家康'))"));

        //Person p1 = new Person(4, "徳川吉宗", DateTime.Now, 180.94m, 106.5645671m, true, "暴れん坊");
        //Assert.That(_db.Save(p1), Is.EqualTo(1));

        //query = new Query<Person>();
        //query.And(val.of("Id").In(int.MinValue, 4) &
        //          val.of("Name") == "徳川吉宗" &
        //          val.of("BirthDay") != null &
        //          val.of("Height") <= 180.94m &
        //          val.of("Weight") >= 106.5645671m &
        //          true == val.of("IsDaimyou") &
        //          !(val.of("Remarks").Like("%米将軍%")));
        //Assert.That(t1.FindOne(query), Is.EqualTo(p1));
        //Assert.That(t1.LastExecutedSql,
        //            Is.EqualTo(""));


        Person p2 = new Person(5, "徳川家重", DateTime.MinValue, 129.657m, 56m, true, "暗君");
        Assert.That(_db.Save(p2), Is.EqualTo(1));

        query = new Query<Person>();
        query.And(val.of("Id") == 5 &
                  val.of("Name").Like("%") &
                  val.of("BirthDay").Between(null, null) &
                  val.of("Height").Between(129.657m, long.MinValue) &
                  val.of("Weight").Between(int.MinValue, 56m) &
                  null == val.of("IsDaimyou") &
                  val.of("Remarks").Like("暗君%"));
        Assert.That(t1.FindOne(query), Is.EqualTo(p2));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("SELECT * FROM (SELECT id AS Id,name AS Name,birthDay AS BirthDay,height" +
                               " AS Height,weight AS Weight,isDaimyou AS IsDaimyou,remarks AS Remarks" +
                               Environment.NewLine + "FROM Persons)V0_ " +
                               "WHERE Id=5 AND Name LIKE " + N + "'%' AND Height>=129.657 AND " +
                               "Weight<=56 AND Remarks LIKE " + N + "'暗君%'"));


        //Person p3 = new Person(6, "徳川家定", DateTime.Now, 169.54m, 69.68m, true, "");
        //Assert.That(_db.Save(p6), Is.EqualTo(1));

        //query = new Query<Person>();
        //query.And(val.of("Id") == 6 &
        //          val.of("Name") != "" &
        //          val.of("BirthDay") <= null &
        //          val.of("Height") < decimal.MinValue &
        //          val.of("Weight") > "" &
        //          val.of("IsDaimyou") != null &
        //          !(val.of("Remarks") >= ""));
        //Assert.That(t1.FindOne(query), Is.EqualTo(p3));
        //Assert.That(t1.LastExecutedSql,
        //            Is.EqualTo(""));


        Person p4 = new Person(7, "徳川家茂", DateTime.MinValue, 175.45m, 66m, true, "");
        Assert.That(_db.Save(p4), Is.EqualTo(1));

        query = new Query<Person>();
        query.And(val.of("Id") == 7 & val.of("BirthDay").IsNull);
        Assert.That(t1.FindOne(query), Is.EqualTo(p4));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("SELECT * FROM (SELECT id AS Id,name AS Name,birthDay AS BirthDay,height" +
                               " AS Height,weight AS Weight,isDaimyou AS IsDaimyou,remarks AS Remarks" +
                               Environment.NewLine + "FROM Persons)V0_ " +
                               "WHERE Id=7 AND BirthDay IS NULL"));


        Person p5 = new Person(8, "徳川慶喜", DateTime.MinValue, 156.89m, 67m, true, "");
        Assert.That(_db.Save(p5), Is.EqualTo(1));

        query = new Query<Person>();
        query.And(val.of("Id") == 8);
        var ph = new Dictionary<string, string>() { { "Daimyou", "isDaimyou = 0" } };
        Assert.That(t1.FindOne(query, ph), Is.EqualTo(p5));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("SELECT * FROM (SELECT id AS Id,name AS Name,birthDay AS BirthDay,height" +
                               " AS Height,weight AS Weight,isDaimyou AS IsDaimyou,remarks AS Remarks" +
                               Environment.NewLine + "FROM Persons)V0_ " +
                               "WHERE Id=8"));

      }
    }




    [Test()]
    public void FindWithLock() {
      var criteria = new Person();
      criteria.Name = "織田信長";

      using(var t1 = _db.CreateTran()) {
        Assert.That(t1.FindOne(criteria, Tran.LoadMode.ReadWrite)
                  , Is.EqualTo(_persons["織田信長"]));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("SELECT * FROM (SELECT id AS Id,name AS Name,birthDay AS BirthDay,height" +
                               " AS Height,weight AS Weight,isDaimyou AS IsDaimyou,remarks AS Remarks" +
                               Environment.NewLine + "FROM Persons)V0_ WHERE Name=" + N +"'織田信長'"));
      }
    }

    [Test()]
    public void FindWithWildcard() {
      var p1 = new PersonWildcard(1, "織田信長", new DateTime(1534, 6, 23), 160.34M, 50M, false, "天下布武");

      var criteria = new PersonWildcard();
      criteria.Name = "織田信長";

      using(var t1 = _db.CreateTran()) {
        Assert.That(t1.FindOne(criteria, Tran.LoadMode.ReadOnly), Is.EqualTo(p1));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("SELECT * FROM (SELECT * FROM Persons)V0_ WHERE name=" + N + "'織田信長'"));
      }
    }

    [Test()]
    public void FindWithTableWildcard() {
      var p1 = new PersonTableWildcard(1, "織田信長", new DateTime(1534, 6, 23), 160.34M, 50M, false, "天下布武");

      var criteria = new PersonTableWildcard();
      criteria.Name = "織田信長";

      using(var t1 = _db.CreateTran()) {
        Assert.That(t1.FindOne(criteria, Tran.LoadMode.ReadOnly), Is.EqualTo(p1));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("SELECT * FROM (SELECT P.* FROM Persons P)V0_ WHERE name=" + N + "'織田信長'"));
      }
    }

    [Test()]
    public void DeleteByQuery() {
      var query = new Query<Person>();
      query.And(val.of("Name") == "織田信長");

      using(var t1 = _db.CreateTran()) {
        Assert.That(t1.Call("delete", query), Is.EqualTo(1));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("DELETE FROM Persons WHERE name=" + N + "'織田信長'"));
      }
    }

    [Test()]
    public void CallWithRecordAndQuery() {
      var record = new Person(1, "織田信成", new DateTime(1987, 3, 25), 164M, 75M, false, "スケート");
      var query = new Query<Person>();
      query.And(val.of("Name") == "織田信長");

      using(var t1 = _db.CreateTran()) {
        Assert.That(t1.Call("save", record, query), Is.EqualTo(1));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("UPDATE Persons SET name=" + N + "'織田信成',birthDay='1987-03-25',height=164," +
                               "weight=75,isDaimyou=0,remarks=" + N + "'スケート' WHERE name=" + N + "'織田信長'"));
      }
    }

    [Test()]
    public void CallWithPlaceholders() {
      var placeholders = new Dictionary<string, string>();
      placeholders.Add("Id", "1");
      placeholders.Add("Name", "'織田信成'");
      placeholders.Add("BirthDay", "'" + (new DateTime(1987, 3, 25)).ToString("yyyy-MM-dd") + "'");
      placeholders.Add("Height", "164.34");
      placeholders.Add("Weight", "75");
      placeholders.Add("IsDaimyou", "0");
      placeholders.Add("Remarks", N + "'スケート'");

      var query = new Query<Person>();
      query.And(val.of("Name") == "織田信長");

      using(var t1 = _db.CreateTran()) {
        Assert.That(t1.Call("save", query, placeholders), Is.EqualTo(1));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("UPDATE Persons SET name='織田信成',birthDay='1987-03-25',height=164.34," +
                               "weight=75,isDaimyou=0,remarks=" + N + "'スケート' WHERE name=" + N + "'織田信長'"));
      }
    }

    [Test()]
    public void FindWithOuterJoin() {
      var s1 = new Schedule(1
                          , "織田信長"
                          , DateTime.Now
                          , 160.34M
                          , 50M
                          , false
                          , "天下布武"
                          , new DateTime(1534, 6, 23,13,26,0)
                          , "桶狭間");
      using(var t1 = _db.CreateTran()) {
        Assert.That(t1.Save(s1), Is.EqualTo(1));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("INSERT INTO Schedules(id,date,subject)" +
                               " VALUES(1,'1534-06-23'," + N + "'桶狭間')"));

        var criteria = new Schedule();
        criteria.Name = "織田信長";
        for(var i = 0; i < 256; i++) {
          var p2 = t1.FindOne(criteria, Tran.LoadMode.ReadWrite);

        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("SELECT * FROM (SELECT P.id AS Id,P.name AS Name,P.birthDay AS BirthDay" +
                               ",P.height AS Height,P.weight AS Weight,P.isDaimyou AS IsDaimyou,P.remarks" + 
                               Environment.NewLine + "AS Remarks,S.date AS \"Date\",S.subject AS Subject " +
                               "FROM Persons P LEFT JOIN Schedules S ON P.id=S.id)V0_ WHERE Name=" + N + "'織田信長'"));

        }
      }
    }

    [Test()]
    public void SelectInSave() {
      var s1 = new ScheduleSelectInSave(1
                    , "織田信長"
                    , DateTime.Now
                    , 160.34M
                    , 50M
                    , false
                    , "天下布武"
                    , new DateTime(1571, 9, 30, 00, 00, 0)
                    , "叡山焼き討ち");
      using(var t1 = _db.CreateTran()) {
        Assert.That(t1.Save(s1), Is.EqualTo(1));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("INSERT INTO Schedules(id,date,subject)" +
                               " VALUES(1,'1571-09-30'," + N + "'叡山焼き討ち')"));
      }

      var s2 = new ScheduleSelectInSave2(1
                    , "織田信長"
                    , new DateTime(2018,6,5)
                    , 160.34M
                    , 50M
                    , false
                    , "天下布武"
                    , new DateTime(1571, 9, 30, 00, 00, 0)
                    , "叡山焼き討ち");
      using(var t2 = _db.CreateTran()) {
        Assert.That(t2.Save(s2), Is.EqualTo(1));
        Assert.That(t2.LastExecutedSql,
                    Is.EqualTo("UPDATE Persons SET " +
                               "name=" + N + "'織田信長',birthDay='2018-06-05',height=160.34," +
                               "weight=50,isDaimyou=0,remarks=" + N + "'天下布武' " +
                               "WHERE id=1"));
      }
        
      var s3 = new ScheduleSelectInSave3(1
                    , "織田信長"
                    , DateTime.Now
                    , 160.34M
                    , 50M
                    , false
                    , "天下布武"
                    , new DateTime(1571, 9, 30, 00, 00, 0)
                    , "叡山焼き討ち");
      using(var t3 = _db.CreateTran()) {
        Assert.That(t3.Save(s3), Is.EqualTo(1));
        Assert.That(t3.LastExecutedSql,
                    Is.EqualTo("UPDATE Schedules SET subject=" + N + "'叡山焼き討ち' " +
                               "WHERE id=1 AND date='1571-09-30'"));
      }
    }

    [Test()]
    public void SaveUnSelectTable() {
      var p1 = new Person();
      p1.Id = 1;

      using(var t1 = _db.CreateTran()) {
        Assert.That(
          t1.Call("saveSchedule", p1, new Dictionary<string,string>{{"Date", "'1192-08-21'"},{"Subject","'征夷大将軍'"}})
        , Is.EqualTo(0));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("UPDATE Schedules SET id=1,date='1192-08-21',subject='征夷大将軍' WHERE id=1"));
      }
    }

    [Test]
    public void SaveWith2Tables() {
      var s1 = new Schedule2Tables(1
                    , "織田信長"
                    , new DateTime(1534, 6, 23)
                    , 160.34M
                    , 50M
                    , false
                    , "天下布武"
                    , new DateTime(1571, 9, 30, 00, 00, 0)
                    , "叡山焼き討ち");
      using(var t1 = _db.CreateTran()) {
        Assert.That(t1.Save(s1), Is.EqualTo(1));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("INSERT INTO Schedules(id,date,subject)" +
                               " VALUES(1,'1571-09-30'," + N + "'叡山焼き討ち')"));
        Assert.That(t1.Delete(s1), Is.EqualTo(1));
        /* UPDATE/DELETE文のFROM句が記述できるDBMSは限られているため、WHERE句の付加においては
           FROM句がないという前提で処理をしている*/
        Assert.That(t1.LastExecutedSql,
            Is.EqualTo("DELETE FROM Schedules FROM Persons " +
                       "WHERE Schedules.id=1 AND Schedules.date='1571-09-30' AND Schedules.subject=" + N + "'叡山焼き討ち' AND " +
                       "EXISTS(SELECT 1 FROM" + Environment.NewLine +"(SELECT P.id AS Id,P.name AS Name," +
                       "P.birthDay AS BirthDay,P.height AS Height,P.weight AS Weight,P.isDaimyou AS IsDaimyou," +
                       "P.remarks AS Remarks,S_." + Environment.NewLine + "date AS \"Date\",S_.subject AS Subject," +
                       "S_.id AS S__id_ FROM Persons P LEFT JOIN Schedules S_ ON P.id=S_.id)V0_ " +
                       "WHERE Name=" + N + "'織田信長' AND BirthDay=" + Environment.NewLine + "'1534-06-23' AND Height=160.34 AND " +
                       "Weight=50 AND IsDaimyou=0 AND Remarks=" + N + "'天下布武' AND V0_.Date=Schedules.date AND V0_.S__id_=Schedules.id)"));
      }
    }


    [Test()]
    public void FindBankWithOuterJoin() {
      var b1 = new Bank(99, "京都銀行", "ｷｮｳﾄｷﾞﾝｺｳ",
                        99, "本店", "ﾎﾝﾃﾝ");

      Assert.That(_db.Save(b1), Is.EqualTo(1));

      var criteria = new Bank();
      criteria.BankName = "京都銀行";
      criteria.BranchName = "振込専用支店";
      var b2 = _db.FindOne(criteria, Tran.LoadMode.ReadWrite);
    }

    [Test()]
    public void FindWithSelfOuterJoin() {
      var p1 = new PersonPair(1, "織田信長", new DateTime(1534, 6, 23), 2, "羽柴秀吉", new DateTime(2018, 4, 7));

      using(var t1 = _db.CreateTran()) {
        Assert.That(t1.Save(p1), Is.EqualTo(1));
        Assert.That(t1.LastExecutedSql,
            Is.EqualTo("UPDATE Persons SET name=" + N + "'羽柴秀吉',birthDay='2018-04-07' WHERE id=2"));

        var query = new Query<PersonPair>();
        query.And(val.of("Id") == 1);
        Assert.That(t1.FindOne(query), Is.EqualTo(p1));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("SELECT * FROM (SELECT P1.id AS Id,P1.name AS Name,P1.birthDay AS " +
                                "BirthDay,P2.id AS SuccessorId,P2.name AS SuccessorName,P2.birthDay AS" +
                                Environment.NewLine + "SuccessorBirthDay FROM Persons P1 LEFT JOIN " +
                                "Persons P2 ON P1.successor=P2.id)V0_ WHERE Id=1"));
      }
    }

    [Test()]
    public void DeleteWithSelfOuterJoin() {
      var p1 = new PersonPair(1, "織田信長", new DateTime(1534, 6, 23), 2, "羽柴秀吉", new DateTime(2018, 4, 7));

      using(var t1 = _db.CreateTran()) {
        Assert.That(t1.Save(p1), Is.EqualTo(1));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("UPDATE Persons SET name=" + N + "'羽柴秀吉',birthDay='2018-04-07' WHERE id=2"));

        var criteria = new PersonPair();
        criteria.SuccessorName = "羽柴秀吉";
        Assert.That(t1.Delete(criteria), Is.EqualTo(1));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("DELETE FROM Persons WHERE name=" + N + "'羽柴秀吉'"));
      }
    }

    [Test()]
    public void SaveWithSelfOuterJoin() {
      var p1 = new PersonPair(1, "織田信長", new DateTime(1534, 6, 23), 2, "羽柴秀吉", new DateTime(2018, 4, 7));

      using(var t1 = _db.CreateTran()) {
        Assert.That(t1.Save(p1), Is.EqualTo(1));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("UPDATE Persons SET name=" + N + "'羽柴秀吉',birthDay='2018-04-07' WHERE id=2"));

        var record = new PersonPair();
        record.SuccessorName = "羽柴秀吉";
        var query = new Query<PersonPair>();
        query.And(val.of("Id") == 1);
        Assert.That(t1.Call("save", record, query), Is.EqualTo(1));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("UPDATE Persons SET name=" + N + "'羽柴秀吉',birthDay=NULL WHERE " +
                               "EXISTS(SELECT 1 FROM (SELECT P1.id AS Id,P1.name AS Name," +
                               "P1.birthDay AS BirthDay,P2_.id AS" + Environment.NewLine +
                               "SuccessorId,P2_.name AS SuccessorName,P2_.birthDay AS SuccessorBirthDay " +
                               "FROM Persons P1 LEFT JOIN Persons P2_ ON P1.successor=P2_.id)V0_ " +
                               "WHERE"+ Environment.NewLine+"Id=1 AND V0_.SuccessorId=Persons.id)"));

        var criteria = new PersonPair();
        criteria.Id = 1;
        criteria.SuccessorName = "豊臣秀吉";
        Assert.That(t1.Save(criteria), Is.EqualTo(1));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("UPDATE Persons SET name=" + N + "'豊臣秀吉',birthDay=NULL WHERE " +
                               "EXISTS(SELECT 1 FROM (SELECT P1.id AS Id,P1.name AS Name," +
                               "P1.birthDay AS BirthDay,P2_.id AS" + Environment.NewLine +
                               "SuccessorId,P2_.name AS SuccessorName,P2_.birthDay AS SuccessorBirthDay " +
                               "FROM Persons P1 LEFT JOIN Persons P2_ ON P1.successor=P2_.id)V0_ " +
                               "WHERE" + Environment.NewLine + "Id=1 AND V0_.SuccessorId=Persons.id)"));
      }
    }

    [Test()]
    public void OrderByWithTableColumnName() {
      var p1 = new PersonPair(1, "織田信長", new DateTime(1534, 6, 23), 2, "羽柴秀吉", new DateTime(2018,4,7));
      using(var t1 = _db.CreateTran()) {
        Assert.That(t1.Save(p1), Is.EqualTo(1));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("UPDATE Persons SET name=" + N + "'羽柴秀吉',birthDay='2018-04-07' WHERE id=2"));

        var query = new Query<PersonPair>();
        query.And(val.of("BirthDay") == new DateTime(1534, 6, 23));
        query.OrderBy("P2.name");

        Assert.That(t1.FindOne(query), Is.EqualTo(p1));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("SELECT * FROM (SELECT P1.id AS Id,P1.name AS Name,P1.birthDay AS " +
                                "BirthDay,P2.id AS SuccessorId,P2.name AS SuccessorName,P2.birthDay AS" +
                                Environment.NewLine + "SuccessorBirthDay FROM Persons P1 LEFT JOIN " +
                                "Persons P2 ON P1.successor=P2.id)V0_ WHERE BirthDay='1534-06-23' " +
                                "ORDER BY SuccessorName"));
      }
    }

    [Test()]
    public void FindWithUnionAll() {
      var query = new Query<PersonUnionAll>();
      query.And(val.of("Id") == 0);

      using(var t1 = _db.CreateTran()) {
        var p1 = t1.FindOne(query);
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("SELECT * FROM (SELECT id AS Id,name AS Name,birthDay AS " +
                               "BirthDay FROM Persons UNION ALL SELECT 0," + N + "'名無しの権兵衛'," +
                               "'2001-01-01')V0_ WHERE Id=0"));
        
        var comparison = new PersonUnionAll(0, "名無しの権兵衛", new DateTime(2001, 1, 1));
        Assert.That(p1, Is.EqualTo(comparison));
      }
    }

    [Test]
    public void DeleteWithUnionAll() {
      var query = new Query<PersonUnionAll>();
      query.And(val.of("Id") > 0);

      using(var t1 = _db.CreateTran()) {
        Assert.That(t1.Call("delete", query), Is.EqualTo(3));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("DELETE FROM Persons WHERE EXISTS(SELECT 1 FROM (SELECT id AS Id" +
                                ",name AS Name,birthDay AS BirthDay,id AS Persons__Id_ FROM Persons " +
                                "Persons_ UNION" + Environment.NewLine + "ALL SELECT 0," +
                                 N + "'名無しの権兵衛','2001-01-01',NULL AS Persons__Id_)V0_ WHERE " +
                                "Id>0 AND V0_.Persons__Id_=Persons.id)"));
      }
    }

    [Test()]
    public void FindWithUnion() {
      var query = new Query<PersonUnion>();
      query.And(val.of("Id") == 0);

      using(var t1 = _db.CreateTran()) {
        var p1 = t1.FindOne(query);
        var comparison = new PersonUnion(0, "名無しの権兵衛", new DateTime(2001, 1, 1));
        Assert.That(p1, Is.EqualTo(comparison));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("SELECT * FROM (SELECT id AS Id,name AS Name,birthDay AS " +
                               "BirthDay FROM Persons UNION SELECT 0," + N + "'名無しの権兵衛'," +
                               "'2001-01-01')V0_ WHERE Id=0"));
      }
    }

    [Test]
    public void DeleteWithUnion() {
      var query = new Query<PersonUnion>();
      query.And(val.of("Id") > 0);

      using(var t1 = _db.CreateTran()) {
        Assert.That(t1.Call("delete", query), Is.EqualTo(3));
        Assert.That(t1.LastExecutedSql,
                    Is.EqualTo("DELETE FROM Persons WHERE Id>0 AND EXISTS(SELECT 1 FROM (SELECT id AS Id" +
                                ",name AS Name,birthDay AS BirthDay FROM Persons " +
                                "Persons_ UNION SELECT 0," + Environment.NewLine + N + "'名無しの権兵衛'" +
                                ",'2001-01-01')V0_ WHERE Id>0)"));
      }
    }

    [Test]
    public void FindWithGroupBy() {
      var expects = new List<PersonGroupBy>();
      expects.Add(new PersonGroupBy(1, new DateTime(1534, 6, 23)));
      expects.Add(new PersonGroupBy(1, new DateTime(1537, 3, 17)));
      expects.Add(new PersonGroupBy(1, new DateTime(1543, 1, 31)));

      var query = new Query<PersonGroupBy>();
      query.And(val.of("Count") > 0);

      using(var t1 = _db.CreateTran()) {
        int i = 0;
        foreach(var rec in t1.Find(query)) {
          Assert.That(rec, Is.EqualTo(expects[i]));
          ++i;
        }
        Assert.That(t1.LastExecutedSql,
            Is.EqualTo("SELECT * FROM (SELECT count(*) AS \"Count\"" +
                       ",birthDay AS BirthDay FROM Persons GROUP BY" +
                       " birthDay)V0_ WHERE Count>0 ORDER BY BirthDay,\"Count\""));
      }
    }

    [Test]
    public void ChangeBirthDay() {
      var newDateTime = new DateTime(1999, 7, 7);

      using(var t1 = _db.CreateTran()) {
        Assert.That(t1.Call("changeBirthDay", new PersonGroupBy(1, newDateTime)), Is.EqualTo(0));
        Assert.That(t1.LastExecutedSql,
            Is.EqualTo("UPDATE Persons SET birthDay='1999-07-07' " +
                       "WHERE birthDay='1999-07-07'"));
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

    [Test]
    public void Argument_IsNot_Modified1() {
      // 引数の値が変更されていないこと

      var expected0 = new Person();
      var actual0 = new Person();
      var expected1 = new Person(1118,
                                 "平清盛",
                                 new DateTime(1918, 3, 20),
                                 183.49M,
                                 69.3M,
                                 false,
                                 "平家の棟梁");
      var actual1 = new Person(1118,
                                  "平清盛",
                                  new DateTime(1918, 3, 20),
                                  183.49M,
                                  69.3M,
                                  false,
                                  "平家の棟梁");

      var db = new Db(_dbms, _connectStr, _dbParam);
      var aTran1 = db.CreateTran();

      //
      // Find()の呼出し前後で引数の値は不変であること
      //
      var aReader1 = aTran1.Find(actual0, Tran.LoadMode.ReadOnly);
      this.ReadAll(aReader1);
      Assert.IsTrue(expected0 == actual0);

      aReader1 = aTran1.Find(actual1, Tran.LoadMode.ReadOnly);
      this.ReadAll(aReader1);
      Assert.IsTrue(expected1 == actual1);


      //
      // FindOne()の呼出し前後で引数の値は不変であること
      //
      aTran1.FindOne(actual1, Tran.LoadMode.ReadOnly);
      Assert.IsTrue(expected1 == actual1);

      //
      // Count()の呼出し前後で引数の値は不変であること
      //
      int i = aTran1.Count(actual0);
      Assert.IsTrue(expected0 == actual0);

      i = aTran1.Count(actual1);
      Assert.IsTrue(expected1 == actual1);

      //
      // Save()の呼出し前後で引数の値は不変であること
      //
      i = aTran1.Save(actual0);
      Assert.IsTrue(expected0 == actual0);
      i = aTran1.Save(actual1);
      Assert.IsTrue(expected1 == actual1);

      //
      // Delete()の呼出し前後で引数の値は不変であること
      //
      i = aTran1.Delete(actual0);
      Assert.IsTrue(expected0 == actual0);

      i = aTran1.Delete(actual1);
      Assert.IsTrue(expected1 == actual1);

      aTran1.Dispose();
    }

    [Test]
    public void Argument_IsNot_Modified2() {
      
      var person1 = new Person(1192,
                                "源頼朝",
                                new DateTime(1947, 5, 9),
                                160.789M,
                                67.234M,
                                false,
                                "鎌倉幕府初代将軍");
      var person2 = new Person(1192,
                                "源頼朝",
                                new DateTime(1947, 5, 9),
                                160.789M,
                                67.234M,
                                false,
                                "鎌倉幕府初代将軍");

      var aDb1 = new Db(_dbms, _connectStr, _dbParam);
      var aTran1 = aDb1.CreateTran();
      aTran1.Find(person1);
      Assert.AreEqual(person2, person1);

      aTran1.FindOne(person1);
      Assert.AreEqual(person2, person1);

      aTran1.Save(person1);
      Assert.AreEqual(person2, person1);

      aTran1.Delete(person1);
      Assert.AreEqual(person2, person1);

      aTran1.Dispose();

    }

    [Test]
    public void AutoWhereFalse() {
      using(var tran = _db.CreateTran()) {
        var record = new PersonAutoWhereFalse(12, "足利義満", new DateTime(1358,9,25),160,60,false, "金閣寺");

        Assert.That(tran.Delete(record), Is.EqualTo(3));

        Assert.That(tran.Save(record), Is.EqualTo(1));
        Assert.That(tran.LastExecutedSql,
                    Is.EqualTo("INSERT INTO Persons(id,name,birthDay,height,weight,isDaimyou,remarks) " +
                               "VALUES(12," + N + "'足利義満','1358-09-25',160,60,0," + N + "'金閣寺')"));

        Assert.That(tran.Save(record), Is.EqualTo(1));
        Assert.That(tran.LastExecutedSql,
                    Is.EqualTo("UPDATE Persons SET name=" + N + "'足利義満',birthDay='1358-09-25'," +
                               "height=160,weight=60,isDaimyou=0,remarks=" + N + "'金閣寺'"));

        var placeHolders = new Dictionary<string,string>(){{"Id","12"}};
        var p1 = tran.FindOne(record, placeHolders);
        Assert.That(p1, Is.EqualTo(record));
        Assert.That(tran.LastExecutedSql,
                    Is.EqualTo("SELECT * FROM (SELECT id AS Id,name AS Name,birthDay AS BirthDay," +
                               "height AS Height,weight AS Weight,isDaimyou AS IsDaimyou,remarks AS Remarks" +
                               Environment.NewLine + "FROM Persons WHERE Id=12)V0_"));

        Assert.That(tran.Delete(record), Is.EqualTo(1));
        Assert.That(tran.LastExecutedSql,
                    Is.EqualTo("DELETE FROM Persons"));
      }
    }

    [Test]
    public void DefaultPlaceHolders() {
      using(var tran = _db.CreateTran()) {

        // PlaceHolder初期値よりレコード値を優先する
        var record = new PersonDefaultPH(14, "", new DateTime(0001, 1, 1), decimal.MinValue, 85M, false, "遅参");
        Assert.That(tran.Save(record), Is.EqualTo(1));
        Assert.That(tran.LastExecutedSql,
                    Is.EqualTo("INSERT INTO Persons(id,name,birthDay,height,weight,isDaimyou,remarks) " +
                               "VALUES(14,NULL,NULL,NULL,85,0," + N + "'遅参')"));

        // レコード値やPlaceHolder値の指定がなければPlaceHolder初期値が設定される
        Assert.That(tran.Call("save2", new Query<PersonDefaultPH>()), Is.EqualTo(1));
        Assert.That(tran.LastExecutedSql,
                    Is.EqualTo("INSERT INTO Persons(id,name,birthDay,height,weight,isDaimyou,remarks) " +
                               "VALUES(5," + N + "'德川秀忠','1610-04-04',177,90,1," + N + "'二代将軍')"));

      }
    }

    [Test]
    public void NoDefaultPlaceHolders() {
      using(var tran = _db.CreateTran()) {
        /* SqlPodでは@Daimyouプレースホルダの初期値は設定していない
         * 初期化SELECT文の発行時に仮値"0=1"を適用することでViewInfoの
         * 初期化が完了できることを確認する */
        var record = new PersonNoPHcomment(15, "足利義昭", new DateTime(1537,12,5),168,68,false, "逃亡生活");
        Assert.That(tran.Save(record), Is.EqualTo(1));
        Assert.That(tran.LastExecutedSql,
                    Is.EqualTo("INSERT INTO Persons(id,name,birthDay,height,weight,isDaimyou,remarks) " +
                               "VALUES(15," + N + "'足利義昭','1537-12-05',168,68,0," + N + "'逃亡生活')"));
      }
    }

  }
}
