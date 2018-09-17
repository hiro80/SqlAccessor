using System;
using System.Xml;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;

namespace SqlAccessor
{
  /// <summary>
  /// XML文書から開始/終了タグを取得する
  /// </summary>
  /// <remarks></remarks>
  internal class XmlSimpleReader: Disposable, Reader<XmlSimpleReader.XmlElement>
  {
    public class XmlElement
    {
      public string Name { get; set; }
    }

    public class XmlStartElement: XmlElement
    {
      public bool IsEmptyElement { get; set; }
      public Dictionary<string, string> Attributes { get; set; }
      public bool HasAttributes { get; set; }
      public string Contents { get; set; }
      public XmlStartElement(string name
                           , bool isEmptyElement
                           , Dictionary<string, string> attributes
                           , bool hasAttributes
                           , string contents) {
        this.Name = name;
        this.IsEmptyElement = isEmptyElement;
        this.Attributes = attributes;
        this.HasAttributes = hasAttributes;
        this.Contents = contents;
      }
    }

    public class XmlEndElement: XmlElement
    {
      public XmlEndElement(string name) {
        this.Name = name;
      }
    }

    private readonly XmlReader _xmlReader;
    private object _current;
    private object _next;

    public XmlSimpleReader(XmlReader xmlReader) {
      _xmlReader = xmlReader;

      //最初に要素を一つ先読みしておく
      if(this.MoveNextElementOrContents()) {
        _next = this.CurrentElementOrContents();
        if(_next is string || _next is XmlEndElement) {
          throw new BadFormatSqlPodException("Well-formed XMLではありません");
        }
      }
    }

    protected override void DisposeImp(bool disposing) {
      base.DisposeImp(disposing);
      _xmlReader.Close();
    }

    object IEnumerator.Current {
      get { return this.Current; }
    }

    XmlSimpleReader.XmlElement IEnumerator<XmlSimpleReader.XmlElement>.Current {
      get { return this.Current; }
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return this.GetEnumerator();
    }

    IEnumerator<XmlSimpleReader.XmlElement> IEnumerable<XmlSimpleReader.XmlElement>.GetEnumerator() {
      return this.GetEnumerator();
    }

    public IList GetList() {
      throw new NotSupportedException(
        "XmlSimpleReader.GetList()はサポートしていません");
    }

    public bool ContainsListCollection {
      get {
        throw new NotSupportedException(
          "XmlSimpleReader.ContainsListCollectionはサポートしていません");
      }
    }

    public XmlElement Current {
      get { return (XmlElement)_current; }
    }

    public bool MoveNext() {
      _current = _next;

      if(_current == null) {
        return false;
      }

      //空タグは開始タグと終了タグの2要素に分離して返す
      if(_current is XmlStartElement && 
         ((XmlStartElement)_current).IsEmptyElement) {
        _next = new XmlEndElement((((XmlStartElement)_current).Name));
        return true;
      }

      bool nextIsTextNode = false;
      do {
        if(this.MoveNextElementOrContents()) {
          _next = this.CurrentElementOrContents();
        } else {
          _next = null;
        }

        nextIsTextNode = _current is XmlStartElement &&
                         _next != null && 
                         _next is string;

        //開始タグの直後に続くテキスト要素は、その開始タグのContentsとして扱う
        if(nextIsTextNode) {
          ((XmlStartElement)_current).Contents += _next.ToString();
        }
      } while(nextIsTextNode);

      return true;
    }

    private object CurrentElementOrContents() {
      if(_xmlReader.NodeType == XmlNodeType.Element) {
        return this.CreateXmlStartElement();
      } else if(_xmlReader.NodeType == XmlNodeType.EndElement) {
        return new XmlEndElement(_xmlReader.Name);
      } else if(_xmlReader.NodeType == XmlNodeType.Text) {
        return _xmlReader.Value.Trim();
      } else if(_xmlReader.NodeType == XmlNodeType.CDATA) {
        return _xmlReader.Value.Trim();
      } else {
        throw new BadFormatSqlPodException("予期しないXMLノードです");
      }
    }

    private bool MoveNextElementOrContents() {
      bool ret = false;
      do {
        ret = _xmlReader.Read();
      } while(ret && !(_xmlReader.NodeType == XmlNodeType.Element ||
                       _xmlReader.NodeType == XmlNodeType.EndElement ||
                       _xmlReader.NodeType == XmlNodeType.Text ||
                       _xmlReader.NodeType == XmlNodeType.CDATA));
      return ret;
    }

    private XmlStartElement CreateXmlStartElement() {
      string name = _xmlReader.Name;
      bool isEmptyElement = _xmlReader.IsEmptyElement;
      bool hasAttributes = _xmlReader.HasAttributes;
      string contents = "";
      //開始タグの属性値を取得する
      Dictionary<string, string> attributes = this.GetAttributes();

      return new XmlStartElement(name
                               , isEmptyElement
                               , attributes
                               , hasAttributes
                               , contents);
    }

    //開始タグの属性値を取得する
    private Dictionary<string, string> GetAttributes() {
      Dictionary<string, string> ret = new Dictionary<string, string>();
      for(int i = 0; i <= _xmlReader.AttributeCount - 1; i++) {
        _xmlReader.MoveToAttribute(i);
        ret.Add(_xmlReader.Name, _xmlReader.Value);
      }
      return ret;
    }

    public void Reset() {
      throw new NotSupportedException(
        "XmlSimpleReader.Reset()はサポートしていません");
    }

    public IEnumerator<XmlElement> GetEnumerator() {
      throw new NotSupportedException(
        "XmlSimpleReader.GetEnumerator()はサポートしていません");
    }
  }
}
