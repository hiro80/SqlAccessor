using System;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Collections.Generic;
using MiniSqlParser;

namespace SqlAccessor
{
  /// <summary>
  /// XMLで定義されたSqlPodを表す
  /// </summary>
  /// <remarks>スレッドセーフ</remarks>
  internal class SqlPodDefinedByXml: SqlPod
  {
    //ロック用オブジェクト
    private readonly object _lock = new object();

    private readonly string _sqlPodXml;
    private readonly SqlBuilder.DbmsType _dbms;
    private System.DateTime _lastWriteTime;

    private string _findSql;
    private string _countSql;
    //(Sqlエントリ名、SQL文)
    private Dictionary<string, string> _entrySqls = new Dictionary<string, string>();

    public SqlPodDefinedByXml(string sqlPodXml, SqlBuilder.DbmsType dbms) {
      _sqlPodXml = sqlPodXml;
      _dbms = dbms;
      _lastWriteTime = File.GetLastWriteTime(sqlPodXml);
      this.LoadSqlPodXml();
    }

    private List<XmlSimpleReader.XmlElement> _definedSqlEntries = new List<XmlSimpleReader.XmlElement>();
   
    private void LoadSqlPodXml() {
      //リソースからxsdファイルを読み込む
      Assembly myAssembly = Assembly.GetExecutingAssembly();
      Stream xsdStream = myAssembly.GetManifestResourceStream("SqlAccessor.XmlSchema.sqlPod.xsd");
      System.Xml.Schema.XmlSchema sqlPodSchema = System.Xml.Schema.XmlSchema.Read(xsdStream, null);

      //XMLの妥当性を検証するためのXML Schema
      System.Xml.Schema.XmlSchemaSet schemaSet = new System.Xml.Schema.XmlSchemaSet();
      schemaSet.Add(sqlPodSchema);

      //XMLの妥当性は検証する
      XmlReaderSettings readerSettings = new XmlReaderSettings();
      readerSettings.ValidationType = ValidationType.Schema;
      readerSettings.Schemas = schemaSet;

      //コメント及び空白はスキップする
      readerSettings.IgnoreComments = true;
      readerSettings.IgnoreWhitespace = true;

      XmlReader aXmlReader = XmlReader.Create(_sqlPodXml, readerSettings);
      XmlSimpleReader xmlSimpleReader = new XmlSimpleReader(aXmlReader);
      var lookaheadReader = new LookaheadEnumerator<XmlSimpleReader.XmlElement>(xmlSimpleReader);

      using(lookaheadReader) {
        this.ParseSqlPodElement(lookaheadReader);
      }
    }

    private void ParseSqlPodElement(LookaheadEnumerator<XmlSimpleReader.XmlElement> xmlReader) {
      this.ReadElement(xmlReader, "sqlPod");

      this.ParseFindElement(xmlReader);

      if(this.NextElementIs(xmlReader, "count")) {
        this.ParseCountElement(xmlReader);
      }

      while(this.NextElementIsSqlEntry(xmlReader)) {
        this.ParseSqlEntryElement(xmlReader);
      }

      this.ReadEndElement(xmlReader, "sqlPod");
    }

    private void ParseFindElement(LookaheadEnumerator<XmlSimpleReader.XmlElement> xmlReader) {
      XmlSimpleReader.XmlStartElement findElement = this.ReadElement(xmlReader, "find");
      this.HasAutoWhereAttrOnly(findElement);
      this.ReadEndElement(xmlReader, "find");
      //SELECT文を取得する
      _findSql = findElement.Contents;
    }

    private void ParseCountElement(LookaheadEnumerator<XmlSimpleReader.XmlElement> xmlReader) {
      XmlSimpleReader.XmlStartElement countElement = this.ReadElement(xmlReader, "count");
      this.HasAutoWhereAttrOnly(countElement);
      this.ReadEndElement(xmlReader, "count");
      //COUNT文を取得する
      _countSql = countElement.Contents;
    }

    private void ParseSqlEntryElement(LookaheadEnumerator<XmlSimpleReader.XmlElement> xmlReader) {
      XmlSimpleReader.XmlStartElement sqlEntryElement = this.ReadElement(xmlReader);
      if(sqlEntryElement.HasAttributes) {
        throw new BadFormatSqlPodException("sqlエントリタグに属性は指定できません");
      }
      _definedSqlEntries.Add(sqlEntryElement);

      //SQL文を取得する
      _entrySqls[sqlEntryElement.Name] = sqlEntryElement.Contents;

      this.ReadEndElement(xmlReader, sqlEntryElement.Name);
    }

    private bool NextElementIs(LookaheadEnumerator<XmlSimpleReader.XmlElement> xmlReader, string elementName) {
      return xmlReader.Next != null &&
             xmlReader.Next is XmlSimpleReader.XmlStartElement && 
             xmlReader.Next.Name == elementName;
    }

    private bool NextElementIsSqlEntry(LookaheadEnumerator<XmlSimpleReader.XmlElement> xmlReader) {
      if(xmlReader.Next == null || !(xmlReader.Next is XmlSimpleReader.XmlStartElement)) {
        return false;
      }

      foreach(XmlSimpleReader.XmlElement sqlEntryElement in _definedSqlEntries) {
        if(sqlEntryElement.Name == xmlReader.Next.Name) {
          throw new BadFormatSqlPodException("SqlPodXMLののSqlエントリタグの名称が重複しています");
        }
      }

      return true;
    }

    private XmlSimpleReader.XmlStartElement ReadElement(LookaheadEnumerator<XmlSimpleReader.XmlElement> xmlReader
                                                      , string elementName = null) {
      if(!xmlReader.MoveNext()) {
        throw new BadFormatSqlPodException("予期しないEndOfFileです");
      }
      XmlSimpleReader.XmlElement element = xmlReader.Current;
      if(elementName != null && element.Name != elementName) {
        throw new BadFormatSqlPodException("予期しないXMLタグです");
      } else if(!(element is XmlSimpleReader.XmlStartElement)) {
        throw new BadFormatSqlPodException("XMLの開始タグが見つかりません");
      }
      return (XmlSimpleReader.XmlStartElement)element;
    }

    private void ReadEndElement(LookaheadEnumerator<XmlSimpleReader.XmlElement> xmlReader
                              , string elementName = null) {
      if(!xmlReader.MoveNext()) {
        throw new BadFormatSqlPodException("予期しないEndOfFileです");
      }
      XmlSimpleReader.XmlElement element = xmlReader.Current;
      if(elementName != null && element.Name != elementName) {
        throw new BadFormatSqlPodException("予期しないXMLタグです");
      } else if(!(element is XmlSimpleReader.XmlEndElement)) {
        throw new BadFormatSqlPodException("XMLの終了タグが見つかりません");
      }
    }

    private void HasAutoWhereAttrOnly(XmlSimpleReader.XmlStartElement element) {
      foreach(KeyValuePair<string, string> attr in element.Attributes) {
        if(attr.Key != "autoWhere") {
          throw new BadFormatSqlPodException(
            "'" + element.Name + "'タグに指定できる属性は'autoWhere'のみです");
        }
      }
    }

    private List<string> LoadSqlTags(XmlReader xmlReader) {
      //<sql>タグの内容の読込
      List<string> sqls = new List<string>();
      do {
        string sqlStr = xmlReader.ReadElementString().Trim();
        if(!string.IsNullOrEmpty(sqlStr)) {
          sqls.Add(sqlStr);
        }
      } while(xmlReader.IsStartElement("sql"));

      return sqls;
    }

    //SqlPodXmlファイルを再読み込みする
    private void LoadSqlPodXmlIfUpdated() {
      if(File.GetLastWriteTime(_sqlPodXml) > _lastWriteTime) {
        lock(_lock) {
          if(File.GetLastWriteTime(_sqlPodXml) > _lastWriteTime) {
            //保持しているSQL文を破棄する
            _findSql = null;
            _countSql = null;
            _entrySqls = new Dictionary<string, string>();
            _definedSqlEntries = new List<XmlSimpleReader.XmlElement>();

            //SqlPodXmlを再読み込みする
            this.LoadSqlPodXml();

            //最終更新日付を取得する
            _lastWriteTime = File.GetLastWriteTime(_sqlPodXml);
          }
        }
      }
    }

    private SqlBuilders WrapBySqlBuilderList(string sqlsStr) {
      if(string.IsNullOrEmpty(sqlsStr)) {
        return new SqlBuilders();
      }

      return new SqlBuilders(sqlsStr, _dbms);
    }

    protected override SqlBuilder SelectSql() {
      this.LoadSqlPodXmlIfUpdated();
      return new SqlBuilder(_findSql, _dbms);
    }

    protected override SqlBuilder CountSql() {
      this.LoadSqlPodXmlIfUpdated();
      if(string.IsNullOrEmpty(_countSql)) {
        return null;
      }
      return new SqlBuilder(_countSql, _dbms);
    }

    protected override SqlBuilders EntrySqls(string sqlEntryName) {
      this.LoadSqlPodXmlIfUpdated();
      if(!_entrySqls.ContainsKey(sqlEntryName)) {
        throw new NotExistsSqlEntryName(_sqlPodXml + "にSQLエントリ名'" + sqlEntryName + "'は定義されていません");
      }
      return this.WrapBySqlBuilderList(_entrySqls[sqlEntryName]);
    }

    internal override List<string> GetAllEntryNames() {
      this.LoadSqlPodXmlIfUpdated();
      return new List<string>(_entrySqls.Keys);
    }
  }
}
