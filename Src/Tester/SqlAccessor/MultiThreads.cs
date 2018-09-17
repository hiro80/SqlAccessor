using System;
using System.Diagnostics;
using System.Reflection;
using MiniSqlParser;
using SqlAccessor;
using NUnit.Framework;

namespace SqlAccessorTester
{
  [TestFixture()]
  public class MultiThreadTester
  {

    //マルチスレッド(SELECT)
    [Test()]
    public void MultiSelect() {
      myThread myt1 = new myThread("Thread1");
      myThread myt2 = new myThread("Thread2");
      System.Threading.Thread t1 = null;
      System.Threading.Thread t2 = null;

      t1 = new System.Threading.Thread(myt1.SelectProcedure1_1);
      t2 = new System.Threading.Thread(myt2.SelectProcedure1_2);
      t1.Start();
      t2.Start();

      t1 = new System.Threading.Thread(myt1.SelectProcedure2);
      t2 = new System.Threading.Thread(myt2.SelectProcedure2);
      t1.Start();
      t2.Start();

      t1 = new System.Threading.Thread(myt1.SelectProcedure3_1);
      t2 = new System.Threading.Thread(myt2.SelectProcedure3_2);
      t1.Start();
      t2.Start();

      t1 = new System.Threading.Thread(myt1.SelectProcedure4_1);
      t2 = new System.Threading.Thread(myt2.SelectProcedure4_2);
      t1.Start();
      t2.Start();

      t1 = new System.Threading.Thread(myt1.SelectProcedure5_1);
      t2 = new System.Threading.Thread(myt2.SelectProcedure5_2);
      t1.Start();
      t2.Start();

      t1 = new System.Threading.Thread(myt1.SelectProcedure6_1);
      t2 = new System.Threading.Thread(myt2.SelectProcedure6_2);
      t1.Start();
      t2.Start();

      t1 = new System.Threading.Thread(myt1.SelectProcedure7_1);
      t2 = new System.Threading.Thread(myt2.SelectProcedure7_2);
      t1.Start();
      t2.Start();

      t1 = new System.Threading.Thread(myt1.SelectProcedure8_1);
      t2 = new System.Threading.Thread(myt2.SelectProcedure8_2);
      t1.Start();
      t2.Start();

      t1 = new System.Threading.Thread(myt1.SelectProcedure9_1);
      t2 = new System.Threading.Thread(myt2.SelectProcedure9_2);
      t1.Start();
      t2.Start();

      t1 = new System.Threading.Thread(myt1.SelectProcedure10_1);
      t2 = new System.Threading.Thread(myt2.SelectProcedure10_2);
      t1.Start();
      t2.Start();

      t1 = new System.Threading.Thread(myt1.SelectProcedure11_1);
      t2 = new System.Threading.Thread(myt2.SelectProcedure11_2);
      t1.Start();
      t2.Start();

      t1 = new System.Threading.Thread(myt1.SelectProcedure12_1);
      t2 = new System.Threading.Thread(myt2.SelectProcedure12_2);
      t1.Start();
      t2.Start();

      t1 = new System.Threading.Thread(myt1.SelectProcedure13_1);
      t2 = new System.Threading.Thread(myt2.SelectProcedure13_2);
      t1.Start();
      t2.Start();

      t1 = new System.Threading.Thread(myt1.SelectProcedure14_1);
      t2 = new System.Threading.Thread(myt2.SelectProcedure14_2);
      t1.Start();
      t2.Start();

      t1 = new System.Threading.Thread(myt1.SelectProcedure15_1);
      t2 = new System.Threading.Thread(myt2.SelectProcedure15_2);
      t1.Start();
      t2.Start();

      t1 = new System.Threading.Thread(myt1.SelectProcedure16_1);
      t2 = new System.Threading.Thread(myt2.SelectProcedure16_2);
      t1.Start();
      t2.Start();

      t1 = new System.Threading.Thread(myt1.SelectProcedure17_1);
      t2 = new System.Threading.Thread(myt2.SelectProcedure17_2);
      t1.Start();
      t2.Start();

    }

    [Test()]
    public void MultiDelInsUpd() {
      myThread myt1 = new myThread("Thread1");
      myThread myt2 = new myThread("Thread2");
      System.Threading.Thread t1 = null;
      System.Threading.Thread t2 = null;

      t1 = new System.Threading.Thread(myt1.DeleteInsertUpdateProcedure1_1);
      t2 = new System.Threading.Thread(myt2.DeleteInsertUpdateProcedure1_2);
      t1.Start();
      t2.Start();

      t1 = new System.Threading.Thread(myt1.DeleteInsertUpdateProcedure2_1);
      t2 = new System.Threading.Thread(myt2.DeleteInsertUpdateProcedure2_2);
      t1.Start();
      t2.Start();

      t1 = new System.Threading.Thread(myt1.DeleteInsertUpdateProcedure3_1);
      t2 = new System.Threading.Thread(myt2.DeleteInsertUpdateProcedure3_2);
      t1.Start();
      t2.Start();

      t1 = new System.Threading.Thread(myt1.DeleteInsertUpdateProcedure4_1);
      t2 = new System.Threading.Thread(myt2.DeleteInsertUpdateProcedure4_2);
      t1.Start();
      t2.Start();

      //t1 = New System.Threading.Thread(AddressOf myt1.DeleteInsertUpdateProcedure5_1)
      //t2 = New System.Threading.Thread(AddressOf myt2.DeleteInsertUpdateProcedure5_2)
      //t1.Start()
      //t2.Start()

      //t1 = New System.Threading.Thread(AddressOf myt1.DeleteInsertUpdateProcedure6_1)
      //t2 = New System.Threading.Thread(AddressOf myt2.DeleteInsertUpdateProcedure6_2)
      //t1.Start()
      //t2.Start()

      //t1 = New System.Threading.Thread(AddressOf myt1.DeleteInsertUpdateProcedure7_1)
      //t2 = New System.Threading.Thread(AddressOf myt2.DeleteInsertUpdateProcedure7_2)
      //t1.Start()
      //t2.Start()

      //t1 = New System.Threading.Thread(AddressOf myt1.DeleteInsertUpdateProcedure8_1)
      //t2 = New System.Threading.Thread(AddressOf myt2.DeleteInsertUpdateProcedure8_2)
      //t1.Start()
      //t2.Start()

      //t1 = New System.Threading.Thread(AddressOf myt1.DeleteInsertUpdateProcedure9_1)
      //t2 = New System.Threading.Thread(AddressOf myt2.DeleteInsertUpdateProcedure9_2)
      //t1.Start()
      //t2.Start()

      //t1 = New System.Threading.Thread(AddressOf myt1.DeleteInsertUpdateProcedure10_1)
      //t2 = New System.Threading.Thread(AddressOf myt2.DeleteInsertUpdateProcedure10_2)
      //t1.Start()
      //t2.Start()

      //t1 = New System.Threading.Thread(AddressOf myt1.DeleteInsertUpdateProcedure11_1)
      //t2 = New System.Threading.Thread(AddressOf myt2.DeleteInsertUpdateProcedure11_2)
      //t1.Start()
      //t2.Start()

      //t1 = New System.Threading.Thread(AddressOf myt1.DeleteInsertUpdateProcedure12_1)
      //t2 = New System.Threading.Thread(AddressOf myt2.DeleteInsertUpdateProcedure12_2)
      //t1.Start()
      //t2.Start()

      //t1 = New System.Threading.Thread(AddressOf myt1.DeleteInsertUpdateProcedure13_1)
      //t2 = New System.Threading.Thread(AddressOf myt2.DeleteInsertUpdateProcedure13_2)
      //t1.Start()
      //t2.Start()

      //t1 = New System.Threading.Thread(AddressOf myt1.DeleteInsertUpdateProcedure14_1)
      //t2 = New System.Threading.Thread(AddressOf myt2.DeleteInsertUpdateProcedure14_2)
      //t1.Start()
      //t2.Start()

    }

  }

  public class myThread
  {
    //private SqlBuilder.DbmsType _dbms = SqlBuilder.DbmsType.Sqlite;
    //private string _connectStr = "Data Source=SqliteTable\\Persons.sqlite3";
    //private string _sqlPodsDir = "SqlPodsForSqlite";

    private SqlBuilder.DbmsType dbms = SqlBuilder.DbmsType.MsSql;
    private string connectStr = "Data Source=(localdb)\\v11.0";
    private string sqlPodsDir = "SqlPods";

    //Private _aDb As Db
    //Private aTran1 As Tran
    //Private aTran1_2 As Tran
    //Private aTran2 As Tran
    //Private aTran2_2 As Tran
    //Private aReader1 As IReader
    //Private aReader1_2 As IReader
    //Private aReader2 As IReader
    //Private aReader2_2 As IReader
    private ColumnInfo aColumnInfo = new ColumnInfo();
    //Private _aTestRec1 As New TestRec
    //Private _aTestRec2 As New TestRec
    private Person _person1 = new Person();
    private Person _person2 = new Person();

    private DbParameters _params = new DbParameters();

    public myThread() {
      //aColumnInfoに抽出条件を格納する
      //(カタログテーブルの全件抽出は時間がかかるため)
      aColumnInfo.TableName = "Persons";

      //デバッグ出力あり
      _params.DebugPrint = true;
      _params.SqlPodsDir = sqlPodsDir;
    }

    private void ReadAll(IReader reader) {
      int i = 0;
      foreach(IRecord rec in reader) {
        //最適化されないようにダミーの処理を行う
        if(rec is IRecord) {
          i += 1;
        }
      }
      i = 0;
    }


    private string _name;
    public myThread(string name)
      : this() {
      this._name = name;
    }


    public void SelectProcedure1_1() {
      //DB接続生成  → Tran生成  → 全要素をイテレート  → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);
      IReader aReader1 = default(IReader);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader1 = aTran1.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      this.ReadAll(aReader1);
      Trace.WriteLine(this._name + " 全要素をイテレート " + MethodBase.GetCurrentMethod().Name);

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void SelectProcedure1_2() {
      //DB接続生成  → Tran生成  → 全要素をイテレート  → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);
      IReader aReader2 = default(IReader);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader2 = aTran2.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      this.ReadAll(aReader2);
      Trace.WriteLine(this._name + " 全要素をイテレート " + MethodBase.GetCurrentMethod().Name);

      aTran2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void SelectProcedure2() {
      //(DB接続生成  → DB接続破棄 ) x 256
      Db _aDb = default(Db);


      for(int i = 0; i <= 255; i++) {
        _aDb = new Db(dbms, connectStr);
        Trace.WriteLine(this._name + " DB接続生成 " + "SelectProcedure2");

        _aDb.Dispose();
        Trace.WriteLine(this._name + " DB接続破棄 " + "SelectProcedure2");

      }

    }


    public void SelectProcedure3_1() {
      //(DB接続生成  → Tran生成  → Itr生成 → ITr破棄 → Tran破棄  → DB接続破棄 ) x256
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);
      IReader aReader1 = default(IReader);


      for(int i = 0; i <= 255; i++) {
        _aDb = new Db(dbms, connectStr);
        Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

        aTran1 = _aDb.CreateTran();
        Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

        aReader1 = aTran1.Find(aColumnInfo);
        Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

        aReader1.Dispose();
        Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

        aTran1.Dispose();
        Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

        _aDb.Dispose();
        Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

      }

    }


    public void SelectProcedure3_2() {
      //(DB接続生成  → Tran生成  → Itr生成 → ITr破棄 → Tran破棄  → DB接続破棄 ) x256
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);
      IReader aReader2 = default(IReader);


      for(int i = 0; i <= 255; i++) {
        _aDb = new Db(dbms, connectStr);
        Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

        aTran2 = _aDb.CreateTran();
        Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

        aReader2 = aTran2.Find(aColumnInfo);
        Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

        aReader2.Dispose();
        Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

        aTran2.Dispose();
        Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

        _aDb.Dispose();
        Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

      }

    }


    public void SelectProcedure4_1() {
      //(DB接続生成  → Tran生成  →                    → Tran破棄  → DB接続破棄 ) x256
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);


      for(int i = 0; i <= 255; i++) {
        _aDb = new Db(dbms, connectStr);
        Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

        aTran1 = _aDb.CreateTran();
        Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

        aTran1.Dispose();
        Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

        _aDb.Dispose();
        Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

      }

    }


    public void SelectProcedure4_2() {
      //(DB接続生成  → Tran生成  →                    → Tran破棄  → DB接続破棄 ) x256
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);


      for(int i = 0; i <= 255; i++) {
        _aDb = new Db(dbms, connectStr);
        Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

        aTran2 = _aDb.CreateTran();
        Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

        aTran2.Dispose();
        Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

        _aDb.Dispose();
        Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

      }

    }


    public void SelectProcedure5_1() {
      //(DB接続生成  → Tran生成  → Itr生成 →            Tran破棄  → DB接続破棄 ) x256
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);
      IReader aReader1 = default(IReader);


      for(int i = 0; i <= 255; i++) {
        _aDb = new Db(dbms, connectStr);
        Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

        aTran1 = _aDb.CreateTran();
        Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

        aReader1 = aTran1.Find(aColumnInfo);
        Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

        aTran1.Dispose();
        Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

        _aDb.Dispose();
        Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

      }

    }


    public void SelectProcedure5_2() {
      //(DB接続生成  → Tran生成  → Itr生成 →            Tran破棄  → DB接続破棄 ) x256
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);
      IReader aReader2 = default(IReader);


      for(int i = 0; i <= 255; i++) {
        _aDb = new Db(dbms, connectStr);
        Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

        aTran2 = _aDb.CreateTran();
        Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

        aReader2 = aTran2.Find(aColumnInfo);
        Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

        aTran2.Dispose();
        Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

        _aDb.Dispose();
        Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

      }

    }


    public void SelectProcedure6_1() {
      // DB接続生成  →(Tran生成  → Itr生成 → ITr破棄 → Tran破棄 )→ DB接続破棄   x256
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);
      IReader aReader1 = default(IReader);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);


      for(int i = 0; i <= 255; i++) {
        aTran1 = _aDb.CreateTran();
        Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

        aReader1 = aTran1.Find(aColumnInfo);
        Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

        aReader1.Dispose();
        Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

        aTran1.Dispose();
        Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      }

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void SelectProcedure6_2() {
      // DB接続生成  →(Tran生成  → Itr生成 → ITr破棄 → Tran破棄 )→ DB接続破棄   x256
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);
      IReader aReader2 = default(IReader);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);


      for(int i = 0; i <= 255; i++) {
        aTran2 = _aDb.CreateTran();
        Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

        aReader2 = aTran2.Find(aColumnInfo);
        Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

        aReader2.Dispose();
        Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

        aTran2.Dispose();
        Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      }

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void SelectProcedure7_1() {
      // DB接続生成  →(Tran生成  →                    → Tran破棄 )→ DB接続破棄   x256
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);


      for(int i = 0; i <= 255; i++) {
        aTran1 = _aDb.CreateTran();
        Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

        aTran1.Dispose();
        Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      }

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void SelectProcedure7_2() {
      // DB接続生成  →(Tran生成  →                    → Tran破棄 )→ DB接続破棄   x256
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);


      for(int i = 0; i <= 255; i++) {
        aTran2 = _aDb.CreateTran();
        Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

        aTran2.Dispose();
        Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      }

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void SelectProcedure8_1() {
      // DB接続生成  →(Tran生成  → Itr生成 →            Tran破棄 )→ DB接続破棄   x256
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);
      IReader aReader1 = default(IReader);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);


      for(int i = 0; i <= 255; i++) {
        aTran1 = _aDb.CreateTran();
        Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

        aReader1 = aTran1.Find(aColumnInfo);
        Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

        aTran1.Dispose();
        Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      }

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void SelectProcedure8_2() {
      // DB接続生成  →(Tran生成  → Itr生成 →            Tran破棄 )→ DB接続破棄   x256
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);
      IReader aReader2 = default(IReader);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);


      for(int i = 0; i <= 255; i++) {
        aTran2 = _aDb.CreateTran();
        Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

        aReader2 = aTran2.Find(aColumnInfo);
        Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

        aTran2.Dispose();
        Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      }

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void SelectProcedure9_1() {
      // DB接続生成  → Tran生成  →(Itr生成 → ITr破棄)x256 → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);
      IReader aReader1 = default(IReader);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);


      for(int i = 0; i <= 255; i++) {
        aReader1 = aTran1.Find(aColumnInfo);
        Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

        aReader1.Dispose();
        Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

      }

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void SelectProcedure9_2() {
      // DB接続生成  → Tran生成  →(Itr生成 → ITr破棄)x256 → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);
      IReader aReader2 = default(IReader);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);


      for(int i = 0; i <= 255; i++) {
        aReader2 = aTran2.Find(aColumnInfo);
        Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

        aReader2.Dispose();
        Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

      }

      aTran2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void SelectProcedure10_1() {
      // DB接続生成  → Tran生成  →(Itr生成 →        )x256 → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);
      IReader aReader1 = default(IReader);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);


      for(int i = 0; i <= 255; i++) {
        aReader1 = aTran1.Find(aColumnInfo);
        Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      }

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void SelectProcedure10_2() {
      // DB接続生成  → Tran生成  →(Itr生成 →        )x256 → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);
      IReader aReader2 = default(IReader);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);


      for(int i = 0; i <= 255; i++) {
        aReader2 = aTran2.Find(aColumnInfo);
        Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      }

      aTran2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void SelectProcedure11_1() {
      //DB接続生成  → Tran生成  → Itr生成 → 全要素をイテレート  → ITr破棄 → Tran破棄  →
      //           → Tran生成  → Itr生成 → 全要素をイテレート  → Itr破棄 → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);
      IReader aReader1 = default(IReader);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader1 = aTran1.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      this.ReadAll(aReader1);
      Trace.WriteLine(this._name + " 全要素をイテレート " + MethodBase.GetCurrentMethod().Name);

      aReader1.Dispose();
      Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader1 = aTran1.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      this.ReadAll(aReader1);
      Trace.WriteLine(this._name + " 全要素をイテレート " + MethodBase.GetCurrentMethod().Name);

      aReader1.Dispose();
      Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void SelectProcedure11_2() {
      //DB接続生成  → Tran生成  → Itr生成 → 全要素をイテレート  → ITr破棄 → Tran破棄  →
      //           → Tran生成  → Itr生成 → 全要素をイテレート  → Itr破棄 → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);
      IReader aReader1 = default(IReader);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader1 = aTran1.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      this.ReadAll(aReader1);
      Trace.WriteLine(this._name + " 全要素をイテレート " + MethodBase.GetCurrentMethod().Name);

      aReader1.Dispose();
      Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader1 = aTran1.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      this.ReadAll(aReader1);
      Trace.WriteLine(this._name + " 全要素をイテレート " + MethodBase.GetCurrentMethod().Name);

      aReader1.Dispose();
      Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void SelectProcedure12_1() {
      //DB接続生成  → Tran生成  → Itr生成 → 全要素をイテレート  →            Tran破棄  →
      //           → Tran生成  → Itr生成 → 全要素をイテレート  → Itr破棄 → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);
      IReader aReader1 = default(IReader);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader1 = aTran1.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      this.ReadAll(aReader1);
      Trace.WriteLine(this._name + " 全要素をイテレート " + MethodBase.GetCurrentMethod().Name);

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader1 = aTran1.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      this.ReadAll(aReader1);
      Trace.WriteLine(this._name + " 全要素をイテレート " + MethodBase.GetCurrentMethod().Name);

      aReader1.Dispose();
      Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void SelectProcedure12_2() {
      //DB接続生成  → Tran生成  → Itr生成 → 全要素をイテレート  →            Tran破棄  →
      //           → Tran生成  → Itr生成 → 全要素をイテレート  → Itr破棄 → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);
      IReader aReader2 = default(IReader);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader2 = aTran2.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      this.ReadAll(aReader2);
      Trace.WriteLine(this._name + " 全要素をイテレート " + MethodBase.GetCurrentMethod().Name);

      aTran2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader2 = aTran2.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      this.ReadAll(aReader2);
      Trace.WriteLine(this._name + " 全要素をイテレート " + MethodBase.GetCurrentMethod().Name);

      aReader2.Dispose();
      Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void SelectProcedure13_1() {
      //DB接続生成  → Tran生成  → Itr生成 → 全要素をイテレート  → ITr破棄 → 
      //           → Tran生成  → Itr生成 → 全要素をイテレート  → Itr破棄 → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);
      IReader aReader1 = default(IReader);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader1 = aTran1.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      this.ReadAll(aReader1);
      Trace.WriteLine(this._name + " 全要素をイテレート " + MethodBase.GetCurrentMethod().Name);

      aReader1.Dispose();
      Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader1 = aTran1.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      this.ReadAll(aReader1);
      Trace.WriteLine(this._name + " 全要素をイテレート " + MethodBase.GetCurrentMethod().Name);

      aReader1.Dispose();
      Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void SelectProcedure13_2() {
      //DB接続生成  → Tran生成  → Itr生成 → 全要素をイテレート  → ITr破棄 → 
      //           → Tran生成  → Itr生成 → 全要素をイテレート  → Itr破棄 → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);
      IReader aReader2 = default(IReader);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader2 = aTran2.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      this.ReadAll(aReader2);
      Trace.WriteLine(this._name + " 全要素をイテレート " + MethodBase.GetCurrentMethod().Name);

      aReader2.Dispose();
      Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader2 = aTran2.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      this.ReadAll(aReader2);
      Trace.WriteLine(this._name + " 全要素をイテレート " + MethodBase.GetCurrentMethod().Name);

      aReader2.Dispose();
      Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void SelectProcedure14_1() {
      //DB接続生成  → Tran生成  → Itr生成 → 全要素をイテレート  → ITr破棄 → Tran破棄  →
      //           → Tran生成  → Itr生成 → 全要素をイテレート             → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);
      IReader aReader1 = default(IReader);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader1 = aTran1.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      this.ReadAll(aReader1);
      Trace.WriteLine(this._name + " 全要素をイテレート " + MethodBase.GetCurrentMethod().Name);

      aReader1.Dispose();
      Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader1 = aTran1.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      this.ReadAll(aReader1);
      Trace.WriteLine(this._name + " 全要素をイテレート " + MethodBase.GetCurrentMethod().Name);

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void SelectProcedure14_2() {
      //DB接続生成  → Tran生成  → Itr生成 → 全要素をイテレート  → ITr破棄 → Tran破棄  →
      //           → Tran生成  → Itr生成 → 全要素をイテレート             → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);
      IReader aReader2 = default(IReader);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader2 = aTran2.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      this.ReadAll(aReader2);
      Trace.WriteLine(this._name + " 全要素をイテレート " + MethodBase.GetCurrentMethod().Name);

      aReader2.Dispose();
      Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader2 = aTran2.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      this.ReadAll(aReader2);
      Trace.WriteLine(this._name + " 全要素をイテレート " + MethodBase.GetCurrentMethod().Name);

      aTran2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void SelectProcedure15_1() {
      //DB接続生成  → Tran生成  → Itr生成 → 全要素をイテレート  → ITr破棄 → Tran破棄  →
      //           → Tran生成  → Itr生成 → 全要素をイテレート  → Itr破棄             → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);
      IReader aReader1 = default(IReader);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader1 = aTran1.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      this.ReadAll(aReader1);
      Trace.WriteLine(this._name + " 全要素をイテレート " + MethodBase.GetCurrentMethod().Name);

      aReader1.Dispose();
      Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader1 = aTran1.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      this.ReadAll(aReader1);
      Trace.WriteLine(this._name + " 全要素をイテレート " + MethodBase.GetCurrentMethod().Name);

      aReader1.Dispose();
      Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void SelectProcedure15_2() {
      //DB接続生成  → Tran生成  → Itr生成 → 全要素をイテレート  → ITr破棄 → Tran破棄  →
      //           → Tran生成  → Itr生成 → 全要素をイテレート  → Itr破棄             → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);
      IReader aReader2 = default(IReader);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader2 = aTran2.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      this.ReadAll(aReader2);
      Trace.WriteLine(this._name + " 全要素をイテレート " + MethodBase.GetCurrentMethod().Name);

      aReader2.Dispose();
      Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader2 = aTran2.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      this.ReadAll(aReader2);
      Trace.WriteLine(this._name + " 全要素をイテレート " + MethodBase.GetCurrentMethod().Name);

      aReader2.Dispose();
      Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void SelectProcedure16_1() {
      //DB接続生成  → Tran生成  → Itr生成 → Itr2生成 → Itr2破棄 → Itr破棄 → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);
      IReader aReader1_2 = default(IReader);
      IReader aReader1 = default(IReader);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader1 = aTran1.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      aReader1_2 = aTran1.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      this.ReadAll(aReader1_2);
      Trace.WriteLine(this._name + " 全要素をイテレート " + MethodBase.GetCurrentMethod().Name);

      aReader1_2.Dispose();
      Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

      aReader1.Dispose();
      Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void SelectProcedure16_2() {
      //DB接続生成  → Tran生成  → Itr生成 → Itr2生成 → Itr2破棄 → Itr破棄 → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);
      IReader aReader2 = default(IReader);
      IReader aReader2_2 = default(IReader);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader2 = aTran2.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      aReader2_2 = aTran2.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      this.ReadAll(aReader2_2);
      Trace.WriteLine(this._name + " 全要素をイテレート " + MethodBase.GetCurrentMethod().Name);

      aReader2_2.Dispose();
      Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

      aReader2.Dispose();
      Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void SelectProcedure17_1() {
      //DB接続生成  → Tran生成  → Itr生成 → Tran2生成 → Itr2生成 → Itr2破棄 → Tran2破棄 →Itr破棄 → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);
      Tran aTran1_2 = default(Tran);
      IReader aReader1 = default(IReader);
      IReader aReader1_2 = default(IReader);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader1 = aTran1.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      aTran1_2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader1_2 = aTran1_2.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      this.ReadAll(aReader1_2);
      Trace.WriteLine(this._name + " 全要素をイテレート " + MethodBase.GetCurrentMethod().Name);

      aReader1_2.Dispose();
      Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran1_2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      aReader1.Dispose();
      Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void SelectProcedure17_2() {
      //DB接続生成  → Tran生成  → Itr生成 → Tran2生成 → Itr2生成 → Itr2破棄 → Tran2破棄 →Itr破棄 → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);
      Tran aTran2_2 = default(Tran);
      IReader aReader2 = default(IReader);
      IReader aReader2_2 = default(IReader);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader2 = aTran2.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      aTran2_2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      aReader2_2 = aTran2_2.Find(aColumnInfo);
      Trace.WriteLine(this._name + " 全要素を取得 " + MethodBase.GetCurrentMethod().Name);

      this.ReadAll(aReader2_2);
      Trace.WriteLine(this._name + " 全要素をイテレート " + MethodBase.GetCurrentMethod().Name);

      aReader2_2.Dispose();
      Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran2_2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      aReader2.Dispose();
      Trace.WriteLine(this._name + " ITr破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void DeleteInsertUpdateProcedure1_1() {
      //DB接続生成  → Tran生成  → TestRecのDelete　→　TestRecのInsert  → TestRecのUpdate → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);

      _aDb = new Db(dbms, connectStr, _params);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      this.DeleteRows1(aTran1);
      Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows1(aTran1);
      Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows3(aTran1);
      Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void DeleteInsertUpdateProcedure1_2() {
      //DB接続生成  → Tran生成  → TestRecのDelete　→　TestRecのInsert → TestRecのUpdate → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);

      _aDb = new Db(dbms, connectStr, _params);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      this.DeleteRows2(aTran2);
      Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows2(aTran2);
      Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows4(aTran2);
      Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

      aTran2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }



    public void DeleteInsertUpdateProcedure2_1() {
      //(DB接続生成  → Tran生成  → TestRecのDelete　→　TestRecのInsert → TestRecのUpdate → Tran破棄  → DB接続破棄 ) x256
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);


      for(int i = 0; i <= 255; i++) {
        _aDb = new Db(dbms, connectStr);
        Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

        aTran1 = _aDb.CreateTran();
        Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

        this.DeleteRows1(aTran1);
        Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

        this.SaveRows1(aTran1);
        Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

        this.SaveRows3(aTran1);
        Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

        aTran1.Dispose();
        Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

        _aDb.Dispose();
        Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

      }

    }


    public void DeleteInsertUpdateProcedure2_2() {
      //(DB接続生成  → Tran生成  → TestRecのDelete　→　TestRecのInsert → TestRecのUpdate → Tran破棄  → DB接続破棄 ) x256
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);


      for(int i = 0; i <= 255; i++) {
        _aDb = new Db(dbms, connectStr);
        Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

        aTran2 = _aDb.CreateTran();
        Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

        this.DeleteRows2(aTran2);
        Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

        this.SaveRows2(aTran2);
        Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

        this.SaveRows4(aTran2);
        Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

        aTran2.Dispose();
        Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

        _aDb.Dispose();
        Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

      }

    }


    public void DeleteInsertUpdateProcedure3_1() {
      //DB接続生成  → Tran生成  → TestRecのDelete →            Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      this.DeleteRows1(aTran1);
      Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void DeleteInsertUpdateProcedure3_2() {
      //DB接続生成  → Tran生成  → TestRecのDelete →            Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      this.DeleteRows2(aTran2);
      Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

      aTran2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void DeleteInsertUpdateProcedure4_1() {
      //(DB接続生成  → Tran生成  → TestRecのDelete →            Tran破棄  → DB接続破棄 ) x256
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);


      for(int i = 0; i <= 255; i++) {
        _aDb = new Db(dbms, connectStr, _params);
        Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

        aTran1 = _aDb.CreateTran();
        Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

        this.DeleteRows1(aTran1);
        Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

        aTran1.Dispose();
        Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

        _aDb.Dispose();
        Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

      }

    }


    public void DeleteInsertUpdateProcedure4_2() {
      //(DB接続生成  → Tran生成  → TestRecのDelete →            Tran破棄  → DB接続破棄 ) x256
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);


      for(int i = 0; i <= 255; i++) {
        _aDb = new Db(dbms, connectStr, _params);
        Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

        aTran2 = _aDb.CreateTran();
        Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

        this.DeleteRows2(aTran2);
        Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

        aTran2.Dispose();
        Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

        _aDb.Dispose();
        Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

      }

    }


    public void DeleteInsertUpdateProcedure5_1() {
      //DB接続生成  → Tran生成  → TestRecのInsert →            Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows1(aTran1);
      Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void DeleteInsertUpdateProcedure5_2() {
      //DB接続生成  → Tran生成  → TestRecのInsert →            Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows2(aTran2);
      Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

      aTran2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void DeleteInsertUpdateProcedure6_1() {
      // DB接続生成  →(Tran生成  → TestRecのDelete → TestRecのInsert → TestRecのUpdate → Tran破棄 ) x256 → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);


      for(int i = 0; i <= 255; i++) {
        aTran1 = _aDb.CreateTran();
        Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

        this.DeleteRows1(aTran1);
        Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

        this.SaveRows1(aTran1);
        Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

        this.SaveRows3(aTran1);
        Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

        aTran1.Dispose();
        Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      }

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void DeleteInsertUpdateProcedure6_2() {
      // DB接続生成  →(Tran生成  → TestRecのDelete → TestRecのInsert → TestRecのUpdate → Tran破棄 ) x256 → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);


      for(int i = 0; i <= 255; i++) {
        aTran2 = _aDb.CreateTran();
        Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

        this.DeleteRows2(aTran2);
        Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

        this.SaveRows2(aTran2);
        Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

        this.SaveRows4(aTran2);
        Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

        aTran2.Dispose();
        Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      }

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void DeleteInsertUpdateProcedure7_1() {
      // DB接続生成  →(Tran生成  → TestRecのInsert → TestRecのUpdate →           Tran破棄 ) x256 → DB接続破棄   
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);


      for(int i = 0; i <= 255; i++) {
        aTran1 = _aDb.CreateTran();
        Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

        this.SaveRows1(aTran1);
        Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

        this.SaveRows3(aTran1);
        Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

        aTran1.Dispose();
        Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      }

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void DeleteInsertUpdateProcedure7_2() {
      // DB接続生成  →(Tran生成  → TestRecのInsert → TestRecのUpdate →           Tran破棄 ) x256 → DB接続破棄   
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);


      for(int i = 0; i <= 255; i++) {
        aTran2 = _aDb.CreateTran();
        Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

        this.SaveRows2(aTran2);
        Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

        this.SaveRows4(aTran2);
        Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

        aTran2.Dispose();
        Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      }

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void DeleteInsertUpdateProcedure8_1() {
      // DB接続生成  → Tran生成  →(TestRecのDelete → TestRecのInsert → TestRecのUpdate)x256 → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);


      for(int i = 0; i <= 255; i++) {
        this.DeleteRows1(aTran1);
        Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

        this.SaveRows1(aTran1);
        Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

        this.SaveRows3(aTran1);
        Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

      }

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void DeleteInsertUpdateProcedure8_2() {
      // DB接続生成  → Tran生成  →(TestRecのDelete → TestRecのInsert → TestRecのUpdate)x256 → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);


      for(int i = 0; i <= 255; i++) {
        this.DeleteRows2(aTran2);
        Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

        this.SaveRows2(aTran2);
        Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

        this.SaveRows4(aTran2);
        Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

      }

      aTran2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void DeleteInsertUpdateProcedure9_1() {
      // DB接続生成  → Tran生成  →(TestRecのDelete →        )x256 → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);


      for(int i = 0; i <= 255; i++) {
        this.DeleteRows1(aTran1);
        Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

      }

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void DeleteInsertUpdateProcedure9_2() {
      // DB接続生成  → Tran生成  →(TestRecのDelete →        )x256 → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);


      for(int i = 0; i <= 255; i++) {
        this.DeleteRows2(aTran2);
        Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

      }

      aTran2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void DeleteInsertUpdateProcedure10_1() {
      //DB接続生成  → Tran生成  → TestRecのDelete → TestRecのInsert → TestRecのUpdate → Tran破棄  →
      //           → Tran生成  → TestRecのDelete → TestRecのInsert → TestRecのUpdate → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      this.DeleteRows1(aTran1);
      Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows1(aTran1);
      Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows3(aTran1);
      Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      this.DeleteRows1(aTran1);
      Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows1(aTran1);
      Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows3(aTran1);
      Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void DeleteInsertUpdateProcedure10_2() {
      //DB接続生成  → Tran生成  → TestRecのDelete → TestRecのInsert → TestRecのUpdate → Tran破棄  →
      //           → Tran生成  → TestRecのDelete → TestRecのInsert → TestRecのUpdate → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      this.DeleteRows2(aTran2);
      Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows2(aTran2);
      Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows4(aTran2);
      Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

      aTran2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      this.DeleteRows2(aTran2);
      Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows2(aTran2);
      Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows4(aTran2);
      Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

      aTran2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void DeleteInsertUpdateProcedure11_1() {
      //DB接続生成  → Tran生成  → TestRecのDelete → TestRecのInsert → TestRecのUpdate →
      //           → Tran生成  → TestRecのDelete → TestRecのInsert → TestRecのUpdate → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      this.DeleteRows1(aTran1);
      Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows1(aTran1);
      Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows3(aTran1);
      Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      this.DeleteRows1(aTran1);
      Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows1(aTran1);
      Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows3(aTran1);
      Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void DeleteInsertUpdateProcedure11_2() {
      //DB接続生成  → Tran生成  → TestRecのDelete → TestRecのInsert → TestRecのUpdate →
      //           → Tran生成  → TestRecのDelete → TestRecのInsert → TestRecのUpdate → Tran破棄  → DB接続破棄
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      this.DeleteRows2(aTran2);
      Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows2(aTran2);
      Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows4(aTran2);
      Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      this.DeleteRows2(aTran2);
      Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows2(aTran2);
      Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows4(aTran2);
      Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

      aTran2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void DeleteInsertUpdateProcedure12_1() {
      //DB接続生成  → Tran生成  → TestRecのDelete → TestRecのInsert → TestRecのUpdate → Tran破棄  →
      //           → Tran生成  → TestRecのDelete → TestRecのInsert → TestRecのUpdate           → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      this.DeleteRows1(aTran1);
      Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows1(aTran1);
      Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows3(aTran1);
      Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      this.DeleteRows1(aTran1);
      Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows1(aTran1);
      Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows3(aTran1);
      Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void DeleteInsertUpdateProcedure12_2() {
      //DB接続生成  → Tran生成  → TestRecのDelete → TestRecのInsert → TestRecのUpdate → Tran破棄  →
      //           → Tran生成  → TestRecのDelete → TestRecのInsert → TestRecのUpdate           → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      this.DeleteRows2(aTran2);
      Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows2(aTran2);
      Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows4(aTran2);
      Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

      aTran2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      this.DeleteRows2(aTran2);
      Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows2(aTran2);
      Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows4(aTran2);
      Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

      aTran2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void DeleteInsertUpdateProcedure13_1() {
      //DB接続生成  → Tran生成  → TestRecのDelete → TestRecのInsert → TestRecのUpdate → Tran破棄  →
      //           → Tran生成  → TestRecのDelete → TestRecのInsert → TestRecのUpdate →            → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      this.DeleteRows1(aTran1);
      Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows1(aTran1);
      Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows3(aTran1);
      Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      this.DeleteRows1(aTran1);
      Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows1(aTran1);
      Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows3(aTran1);
      Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void DeleteInsertUpdateProcedure13_2() {
      //DB接続生成  → Tran生成  → TestRecのDelete → TestRecのInsert → TestRecのUpdate → Tran破棄  →
      //           → Tran生成  → TestRecのDelete → TestRecのInsert → TestRecのUpdate →            → DB接続破棄
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      this.DeleteRows2(aTran2);
      Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows2(aTran2);
      Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows4(aTran2);
      Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

      aTran2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      this.DeleteRows2(aTran2);
      Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows2(aTran2);
      Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows4(aTran2);
      Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void DeleteInsertUpdateProcedure14_1() {
      //DB接続生成  → Tran生成  → TestRecのDelete → TestRecのInsert → TestRecのUpdate → Tran2生成 → 
      //TestRecのDelete → TestRecのInsert→ TestRecのUpdate → Tran2破棄 → Tran破棄  → DB接続破棄 
      Db _aDb = default(Db);
      Tran aTran1 = default(Tran);
      Tran aTran1_2 = default(Tran);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran1 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      this.DeleteRows1(aTran1);
      Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows1(aTran1);
      Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows3(aTran1);
      Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

      aTran1_2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      this.DeleteRows1(aTran1_2);
      Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows1(aTran1_2);
      Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows3(aTran1_2);
      Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

      aTran1_2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran1.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }


    public void DeleteInsertUpdateProcedure14_2() {
      //DB接続生成  → Tran生成  → TestRecのDelete → TestRecのInsert → TestRecのUpdate → Tran2生成 → 
      //TestRecのDelete → TestRecのInsert→ TestRecのUpdate → Tran2破棄 → Tran破棄  → DB接続破棄  
      Db _aDb = default(Db);
      Tran aTran2 = default(Tran);
      Tran aTran2_2 = default(Tran);

      _aDb = new Db(dbms, connectStr);
      Trace.WriteLine(this._name + " DB接続生成 " + MethodBase.GetCurrentMethod().Name);

      aTran2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      this.DeleteRows2(aTran2);
      Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows2(aTran2);
      Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows4(aTran2);
      Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

      aTran2_2 = _aDb.CreateTran();
      Trace.WriteLine(this._name + " Tran生成 " + MethodBase.GetCurrentMethod().Name);

      this.DeleteRows2(aTran2_2);
      Trace.WriteLine(this._name + " TestRecのDelete " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows2(aTran2_2);
      Trace.WriteLine(this._name + " TestRecのInsert " + MethodBase.GetCurrentMethod().Name);

      this.SaveRows4(aTran2_2);
      Trace.WriteLine(this._name + " TestRecのUpdate " + MethodBase.GetCurrentMethod().Name);

      aTran2_2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      aTran2.Dispose();
      Trace.WriteLine(this._name + " Tran破棄 " + MethodBase.GetCurrentMethod().Name);

      _aDb.Dispose();
      Trace.WriteLine(this._name + " DB接続破棄 " + MethodBase.GetCurrentMethod().Name);

    }

    private void DeleteRows1(Tran aTran1) {
      //主キー重複を避けるため、削除
      aTran1.Delete(_person1);
    }

    private void DeleteRows2(Tran aTran2) {
      //主キー重複を避けるため、削除
      aTran2.Delete(_person2);
    }

    private void SaveRows1(Tran aTran1) {
      _person1.Id = 10;
      _person1.Name = "足利義尚";
      _person1.BirthDay = new DateTime(1530, 4, 1);
      _person1.Height = 110.45m;
      _person1.Weight = 40.76m;
      _person1.IsDaimyou = false;
      _person1.Remarks = "東軍大将";
      aTran1.Save(_person1);
    }

    private void SaveRows2(Tran aTran2) {
      _person2.Id = 10;
      _person2.Name = "足利義視";
      _person2.BirthDay = new DateTime(1510, 12, 16);
      _person2.Height = 167.3846m;
      _person2.Weight = 68.309m;
      _person2.IsDaimyou = false;
      _person2.Remarks = "西軍大将";
      aTran2.Save(_person2);
    }

    private void SaveRows3(Tran aTran1) {
      _person1.Id = 10;
      _person1.Name = "足利義尚";
      _person1.BirthDay = new DateTime(1530, 4, 1);
      _person1.Height = 150.21m;
      _person1.Weight = 43.87m;
      _person1.IsDaimyou = false;
      _person1.Remarks = "即位後暫らく後に死亡";
      aTran1.Save(_person1);
    }

    private void SaveRows4(Tran aTran2) {
      _person2.Id = 10;
      _person2.Name = "足利義視";
      _person2.BirthDay = new DateTime(1510, 12, 16);
      _person2.Height = 157.8673m;
      _person2.Weight = 64.354m;
      _person2.IsDaimyou = false;
      _person2.Remarks = "東軍に寝返り";
      aTran2.Save(_person2);
    }

  }

}
