using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MiniSqlParser;

namespace MiniSqlParser
{
  /// <summary>
  /// SQLの式を表す
  /// </summary>
  public class SqlExpr
  {
    // 空のValueオブジェクトを表す
    private class NullValue: Node, IValue
    {
      public IValue Clone() {
        return this;
      }
      public bool IsDefault {
        get {
          return false;
        }
      }
      protected override void AcceptImp(IVisitor visitor) { }
    }

    private IValue _value;
    private Dictionary<string, INode> _placedHolders;

    public SqlExpr() : this(null, null) { }

    public SqlExpr(string value) {
      if(value.Trim().ToUpper() == "DEFAULT") {
        _value = new Default();
      } else {
        _value = MiniSqlParserAST.CreateExpr(value);
      }
    }

    internal SqlExpr(IValue value) : this(value, null) { }

    private SqlExpr(IValue value, Dictionary<string, INode> placedHolders) {
      if(value == null) {
        _value = new NullValue();
      } else {
        _value = value;
      }
      _placedHolders = placedHolders;
    }

    public SqlExpr Clone() {
      if(_placedHolders == null) {
        return new SqlExpr(_value.Clone());
      } else {
        return new SqlExpr(_value.Clone(), new Dictionary<string, INode>(_placedHolders));
      }
    }

    internal IValue Value {
      get {
        if(_value.GetType() == typeof(NullValue)) {
          // NullValueはSqlExpr以外で参照できないようにする
          return null;
        } else {
          return _value;
        }
      }
    }

    /// <summary>
    /// プレースホルダに値を適用する
    /// </summary>
    /// <param name="placeHolderName">プレースホルダ名(@を含まない)</param>
    /// <param name="value">プレースホルダに適用する値</param>
    /// <remarks></remarks>
    public void Place(string placeHolderName, string value) {
      if(placeHolderName == null) {
        throw new ArgumentNullException("placeHolderName", "placeHolderNameがNullです");
      }
      var placeHoldersDic = new Dictionary<string, string>();
      placeHoldersDic.Add(placeHolderName, value);
      this.Place(placeHoldersDic);
    }

    /// <summary>
    /// プレースホルダに値を適用する
    /// </summary>
    /// <param name="placeHolders"></param>
    /// <remarks></remarks>
    public void Place(Dictionary<string, string> placeHolders) {
      if(_placedHolders == null) {
        _placedHolders = new Dictionary<string, INode>();
      }

      // プレースホルダノード単体の場合はReplacePlaceHoldersで置き換えられない
      if(this.IsPlaceHolderOnly) {
        var placeHolderName = ((PlaceHolderExpr)_value).LabelName;
        if(placeHolders.ContainsKey(placeHolderName)) {
          try {
            _value = MiniSqlParserAST.CreateExpr(placeHolders[placeHolderName]);
            // 適用したプレースホルダを記録する
            if(!_placedHolders.ContainsKey(placeHolderName)) {
              _placedHolders.Add(placeHolderName, _value);
            }
            return;
          } catch(SqlSyntaxErrorsException ex) {;
            throw new CannotBuildASTException("Type of placeholder value is mismatched", ex);
          }
        }
      }

      // プレースホルダに値を適用する
      var visitor = new ReplacePlaceHolders(placeHolders);
      _value.Accept(visitor);

      // 適用したプレースホルダを記録する
      foreach(var placedHolder in visitor.PlacedHolders) {
        if(!_placedHolders.ContainsKey(placedHolder.Key)) {
          _placedHolders.Add(placedHolder.Key, placedHolder.Value);
        }
      }
    }

    public bool IsEmpty {
      get { return _value == null || _value.GetType() == typeof(NullValue); }
    }

    public Dictionary<string, string> GetAllPlaceHolders() {
      var ret = new Dictionary<string, string>();
      var getPlaceHoldersVisitor = new GetPlaceHoldersVisitor();
      _value.Accept(getPlaceHoldersVisitor);
      // 未適用プレースホルダと適用済みプレースホルダをマージする
      foreach(var unplacedHolder in getPlaceHoldersVisitor.GetUnPlacedHolders) {
        ret.Add(unplacedHolder, "");
      }
      if(_placedHolders == null) {
        return ret;
      }
      foreach(var placedHolder in _placedHolders) {
        if(!ret.ContainsKey(placedHolder.Key)) {
          var stringifier = new CompactStringifier(144);
          placedHolder.Value.Accept(stringifier);
          ret.Add(placedHolder.Key, stringifier.ToString());
        }
      }
      return ret;
    }

    public bool HasUnplacedHolder(string placeHolder) {
      var getPlaceHoldersVisitor = new GetPlaceHoldersVisitor();
      _value.Accept(getPlaceHoldersVisitor);
      return getPlaceHoldersVisitor.GetUnPlacedHolders.Contains(placeHolder);
    }

    public bool HasUnplacedHolders() {
      var getPlaceHoldersVisitor = new GetPlaceHoldersVisitor();
      _value.Accept(getPlaceHoldersVisitor);
      return getPlaceHoldersVisitor.GetUnPlacedHolders.Count > 0;
    }

    public bool IsPlaceHolderOnly {
      get {
        return _value.GetType() == typeof(PlaceHolderExpr);
      }
    }

    public bool IsDefault {
      get {
        return _value.IsDefault;
      }
    }

    public bool IsLiteral {
      get {
        return _value is Literal;
      }
    }

    public override string ToString() {
      var stringifier = new CompactStringifier(144, false);
      _value.Accept(stringifier);
      return stringifier.ToString();
    }
  }
}
