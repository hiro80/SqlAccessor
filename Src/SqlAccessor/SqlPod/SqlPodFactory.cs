using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using MiniSqlParser;

namespace SqlAccessor
{
  internal class SqlPodFactory
  {
    private readonly string _sqlPodDirName;
    private readonly SqlBuilder.DbmsType _dbms;
    private readonly Dictionary<string, SqlPod> _sqlPodHash = new Dictionary<string, SqlPod>();
   
    /// <summary>
    /// 
    /// </summary>
    /// <param name="splPodsPath">SqlPodsディレクトリのファイルパス
    /// (SqlAccessor.dllを基準とした相対パス指定)</param>
    /// <param name="dbms">DBMS種別</param>
    /// <remarks></remarks>
    public SqlPodFactory(string splPodsPath, SqlBuilder.DbmsType dbms) {
      //DBMS種別の設定
      _dbms = dbms;

      //SqlPodsディレクトリは、自アセンブリファイルと同じディレクトリに配置される
      //注意!) C:\\はC:\と同じファイルパスではない。C:\\はネットワーク先のファイルパスを意味するらしい
      _sqlPodDirName = System.IO.Path.Combine(
                          new System.IO.FileInfo(
                              new Uri(this.GetType().Assembly.CodeBase).LocalPath
                          ).DirectoryName
                          , splPodsPath
                       );
    }

    public SqlPod CreateSqlPod(string viewName) {
      //_sqlPodHashへのデータ有無の判断と書込み読込みを不可分処理とする
      //Double-Checked Lockingパターン
      if(!_sqlPodHash.ContainsKey(viewName)) {
        lock(_sqlPodHash) {
          if(!_sqlPodHash.ContainsKey(viewName)) {
            //SqlPodを読み込む
            SqlPod newSqlPod = this.LoadSqlPod(viewName);
            //DBMS種別を設定する
            newSqlPod.Dbms = _dbms;
            //ハッシュに登録する
            _sqlPodHash.Add(viewName, newSqlPod);
          }
        }
      }

      return _sqlPodHash[viewName];
    }

    public SqlPod CreateSqlPod<TRecord>() where TRecord: class, IRecord, new() {
      string viewName = RecordInfo<TRecord>.GetInstance().Name;
      return this.CreateSqlPod(viewName);
    }

    private SqlPod LoadSqlPod(string viewName) {
      SqlPod ret = null;
      //
      //SqlPod.dllの読込み
      //
      //SqlPod.dll及びSqlPodXml.xmlはSqlPodsディレクトリに配置される
      string sqlPodFileName = System.IO.Path.Combine(_sqlPodDirName, viewName + "SqlPod" + ".dll");
      string sqlPodXmlFileName = System.IO.Path.Combine(_sqlPodDirName, viewName + ".sql");
      string sqlPodXmlFileName2 = System.IO.Path.Combine(_sqlPodDirName, viewName + "SqlPodXml" + ".xml");

      //SqlPod.dllからSqlPodオブジェクトを生成する
      ret = this.LoadSqlPodFromDll(sqlPodFileName, viewName);
      if(ret == null) {
        //SqlPod.dllが無い場合は.sqlを読み込む
        ret = this.LoadSqlPodFromXml(sqlPodXmlFileName);
        if(ret == null) {
          //SqlPod.dll及び.sqlが無い場合はSqlPodXml.xmlを読み込む
          ret = this.LoadSqlPodFromXml(sqlPodXmlFileName2);
          if(viewName == "ColumnInfo" && ret == null) {
            //SqlPod.dll及び.sql.xmlがない場合でレコードがColumnInfoの場合
            //ColumnInfoSqlPodを返す
            ret = new ColumnInfoSqlPod();
          }
        }
      }

      //SqlPod.dll及び.sql及びSqlPodXml.xmlが存在しない場合は例外を再送出する
      if(ret == null) {
        throw new FileNotFoundException(
          "レコード\"" + viewName + "\"のSqlPod.dll及びSqlPodXml.xmlが見つかりませんでした", sqlPodXmlFileName);
      }

      return ret;
    }

    private SqlPod LoadSqlPodFromDll(string sqlPodFileName, string viewName) {
      //ファイルの存在有無チェック
      if(!File.Exists(sqlPodFileName)) {
        return null;
      }

      SqlPod aSqlPod = null;
      try {
        //SqlPodクラスのサブクラスを定義したアセンブリファイル(dll)をロードする
        Assembly sqlPodAsm = Assembly.LoadFrom(sqlPodFileName);
        //SqlPodの型名からTypeオブジェクトを取得する
        string sqlPodTypeName = this.GetType().Namespace + Type.Delimiter + viewName + "SqlPod";
        Type sqlPodType = sqlPodAsm.GetType(sqlPodTypeName);
        //SqlPod aSqlPod = new [sqlPodTypeName]; と同等
        aSqlPod = (SqlPod)Activator.CreateInstance(sqlPodType);
      } catch(Exception ex) {
        //SqlPodクラスが生成できなかった場合、例外を送出する
        throw new BadFormatSqlPodException("SqlPodクラスが生成できませんでした.", ex);
      }

      return aSqlPod;
    }

    private SqlPodDefinedByXml LoadSqlPodFromXml(string sqlPodXmlFileName) {
      //ファイルの存在有無チェック
      if(!File.Exists(sqlPodXmlFileName)) {
        return null;
      }

      return new SqlPodDefinedByXml(sqlPodXmlFileName, _dbms);
    }
  }
}
