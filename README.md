# SqlAccessor
データベース(DB)からクラス単位でデータを抽出/追加/更新/削除するライブラリです。いわゆるO/Rマッパーの一つです。DBとクラスの間のデータ変換をSQL文を記述するだけで簡単に定義できることが特徴です。

### 1. レコードの作成

DBとのデータのやり取りの単位となるクラスを作成します。このクラスをレコードと呼んでいます。

    public class Person : IRecord
      public int      Id       { get; set; }
      public string   Name     { get; set; }
      public DateTime BirthDay { get; set; }
    }

### 2. SqlPodの作成

DBに格納されているデータをレコードに格納するために、そのデータをDBからどのように取得するかを定義する必要があります。この定義はSqlPodと呼ぶXMLファイルに1つのSELECT文を記述して定義します。SELECT句のAS別名をプロパティ名と同じ名称にすることで、そのSELECT句の値をプロパティに格納するように定義します。

    <?xml version="1.0" encoding="UTF-8"?>
    <sqlPod>
      <Find><![CDATA[
      SELECT
        PersonId                     AS Id
       ,FirstName || ' ' || LastName AS Name
       ,BirthDay                     AS BirthDay
      FROM
        Persons
      ]]></Find>
    </sqlPod>

### 3. レコードの取得

DBからレコードを抽出するための抽出条件の指定方法には、レコードオブジェクトを指定する方法と、Queryオブジェクトを指定する2つの方法があります。

#### 3.1. レコードオブジェクトを指定する方法

レコードオブジェクトを抽出条件として指定します。レコードオブジェクトの各プロパティとその値を、「プロパティ = 値」という一致条件でAND連結した条件として解釈します。

    var db = new Db([DBMS種別], [接続文字列]);

    // 抽出条件の作成
    var criteria = new Person();
    criteria.Name = "足利 尊氏";

    // DBからPersonレコードを抽出する(型推論を利用)
    var reader = db.Find(criteria);

    // 抽出したPersonレコードを表示する
    foreach(var person In reader) {
      WriteLine("抽出結果 >> " + person.Id.ToString());
      WriteLine("            " + person.Name);
      WriteLine("            " + person.BirthDay.ToString());
      WriteLine("            " + person.Age.ToString());
    }

    db.Dispose();

#### 3.2. Queryオブジェクトを指定する方法

Queryオブジェクトとは、抽出条件を保持するためのオブジェクトです。Queryオブジェクトには一致条件以外の条件も格納することができます。

    var db = new Db([DBMS種別], [接続文字列]);

    // 抽出条件の作成
    var criteria = new Query<Person>;
    criteria.And(val.of("Name").Like("足利%") &&
                 val.of("BirthDay") < Datetime.Now
                );

    // DBからPersonレコードを抽出する
    var reader = db.Find(criteria);

    // 抽出したPersonレコードを表示する
    foreach(var person In reader) {
      WriteLine("抽出結果 >> " + person.Id.ToString());
      WriteLine("          " + person.Name);
      WriteLine("          " + person.BirthDay.ToString());
      WriteLine("          " + person.Age.ToString());
    }

    db.Dispose();

### 4. レコードの格納・削除・件数の取得

SqlPodにそれぞれ必要なSQL文を定義するだけです。詳しくは[チュートリアル](SqlAccessorTutorial.pdf)を御覧ください。

# 必要な環境
* Antlr4.Runtime.Standard 4.7.1
* .NET Framework 2.0 以上

# ライセンス
[MIT](https://github.com/tcnksm/tool/blob/master/LICENCE)
