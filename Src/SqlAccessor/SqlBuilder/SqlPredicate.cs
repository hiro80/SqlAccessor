using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MiniSqlParser;

namespace MiniSqlParser
{
  /// <summary>
  /// SQLの論理式を表す
  /// </summary>
  public class SqlPredicate
  {
    // 空のPredicateオブジェクトを表す
    private class NullPredicate: Predicate
    {
      public override Predicate Clone() {
        return this;
      }
      public bool IsDefault {
        get {
          return false;
        }
      }
      protected override void AcceptImp(IVisitor visitor) { }
    }

    private Predicate _predicate;
    private Dictionary<string, INode> _placedHolders;

    public SqlPredicate() : this(null, null) { }

    public SqlPredicate(string predicate) {
      _predicate = MiniSqlParserAST.CreatePredicate(predicate);
    }

    internal SqlPredicate(Predicate predicate) : this(predicate, null) { }

    private SqlPredicate(Predicate predicate, Dictionary<string, INode> placedHolders) {
      if(predicate == null) {
        _predicate = new NullPredicate();
      } else {
        _predicate = predicate;
      }
      _placedHolders = placedHolders;
    }

    public SqlPredicate Clone() {
      if(_placedHolders == null) {
        return new SqlPredicate(_predicate.Clone());
      } else {
        return new SqlPredicate(_predicate.Clone(), new Dictionary<string, INode>(_placedHolders));
      }
    }

    internal Predicate Predicate {
      get {
        if(_predicate.GetType() == typeof(NullPredicate)) {
          // NullPredicateはSqlPredicate以外で参照できないようにする
          return null;
        } else {
          return _predicate;
        }
      }
    }

    /// <summary>
    /// Column列とSqlExprとの一致条件を作成する
    /// </summary>
    /// <param name="columnName"></param>
    /// <param name="sqlExpr"></param>
    /// <returns></returns>
    public static SqlPredicate CreateEqualExpr(string columnName, SqlExpr sqlExpr) {
      return SqlPredicate.CreateEqualExpr(new Column(columnName), sqlExpr);
    }

    /// <summary>
    /// Column列とSqlExprとの一致条件を作成する
    /// </summary>
    /// <param name="tableAliasName"></param>
    /// <param name="columnName"></param>
    /// <param name="sqlExpr"></param>
    /// <returns></returns>
    public static SqlPredicate CreateEqualExpr(string tableAliasName, string columnName, SqlExpr sqlExpr) {
      return SqlPredicate.CreateEqualExpr(new Column(tableAliasName, columnName), sqlExpr);
    }

    private static SqlPredicate CreateEqualExpr(Column column, SqlExpr sqlExpr) {
      var value = sqlExpr.Value;
      if(value.IsDefault) {
        throw new ApplicationException("一致条件の被演算子にDEFAULTキーワードは指定できません");
      }
      var equalPredicate = new BinaryOpPredicate(column, PredicateOperator.Equal, (Expr)value);
      return new SqlPredicate(equalPredicate);
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
        var placeHolderName = ((PlaceHolderPredicate)_predicate).LabelName;
        if(placeHolders.ContainsKey(placeHolderName)) {
          try {
            _predicate = MiniSqlParserAST.CreatePredicate(placeHolders[placeHolderName]);
            // 適用したプレースホルダを記録する
            if(!_placedHolders.ContainsKey(placeHolderName)) {
              _placedHolders.Add(placeHolderName, _predicate);
            }
            return;
          } catch(SqlSyntaxErrorsException ex) {;
            throw new CannotBuildASTException("Type of placeholder value is mismatched", ex);
          }
        }
      }

      // プレースホルダに値を適用する
      var visitor = new ReplacePlaceHolders(placeHolders);
      _predicate.Accept(visitor);

      // 適用したプレースホルダを記録する
      foreach(var placedHolder in visitor.PlacedHolders) {
        if(!_placedHolders.ContainsKey(placedHolder.Key)){
          _placedHolders.Add(placedHolder.Key, placedHolder.Value);
        }
      }
    }

    public bool IsEmpty {
      get { return _predicate == null || _predicate.GetType() == typeof(NullPredicate); }
    }

    public Dictionary<string, string> GetAllPlaceHolders() {
      var ret = new Dictionary<string, string>();
      var getPlaceHoldersVisitor = new GetPlaceHoldersVisitor();
      _predicate.Accept(getPlaceHoldersVisitor);
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
      _predicate.Accept(getPlaceHoldersVisitor);
      return getPlaceHoldersVisitor.GetUnPlacedHolders.Contains(placeHolder);
    }

    public bool HasUnplacedHolders() {
      var getPlaceHoldersVisitor = new GetPlaceHoldersVisitor();
      _predicate.Accept(getPlaceHoldersVisitor);
      return getPlaceHoldersVisitor.GetUnPlacedHolders.Count > 0;
    }

    public bool IsPlaceHolderOnly {
      get {
        return _predicate.GetType() == typeof(PlaceHolderPredicate);
      }
    }

    public override string ToString() {
      var stringifier = new CompactStringifier(144, false);
      _predicate.Accept(stringifier);
      return stringifier.ToString();
    }

    /// <summary>
    /// 論理式条件を追加する
    /// </summary>
    /// <param name="sqlExp"></param>
    /// <remarks>
    /// AddAndExpVisitorは走査対象にPredicate単体を指定された場合、
    /// そのPredicateを置き換えることはできないので利用できなかった.
    /// </remarks>
    public SqlPredicate And(SqlPredicate predicate) {
      //predicateがNullの場合、処理を終了する
      if(predicate == null || predicate.IsEmpty) {
        return this;
      } else if(_predicate.GetType() == typeof(NullPredicate)) {
        return new SqlPredicate(predicate._predicate);
      }

      Predicate leftPredicate;
      if(_predicate.GetType() == typeof(OrPredicate)) {
        leftPredicate = new BracketedPredicate(_predicate);
      } else {
        leftPredicate = _predicate;
      }

      Predicate rightPredicate;
      if(predicate._predicate.GetType() == typeof(OrPredicate)) {
        rightPredicate = new BracketedPredicate(predicate._predicate);
      } else {
        rightPredicate = predicate._predicate;
      }

      // PlacedHoldersをマージする
      Dictionary<string, INode> newPlacedHolders = null;
      if(_placedHolders == null) {
        newPlacedHolders = predicate._placedHolders;
      } else if(predicate._placedHolders == null) {
        newPlacedHolders = _placedHolders;
      } else {
        foreach(var placedHolder in predicate._placedHolders) {
          if(!_placedHolders.ContainsKey(placedHolder.Key)) {
            _placedHolders.Add(placedHolder.Key, placedHolder.Value);
          }
        }
      }

      return new SqlPredicate(new AndPredicate(leftPredicate, rightPredicate), newPlacedHolders);
    }


    // 2つのSqlPredicateの結合処理において、SetPlaceHolder()で設定されたプレースホルダ名が
    // 重複した場合、正しく処理する必要がある。(現状は暫定実装)
    //private Dictionary<string, string> MergePlaceHolders(Dictionary<string, string> ph1
    //                                                   , Dictionary<string, string> ph2) {
    //  if(ph1 == null) {
    //    return ph2;
    //  } else if(ph2 == null) {
    //    return ph1;
    //  }

    //  var many = ph1;
    //  var few = ph2;

    //  if(many.Count < few.Count) {
    //    var p = few;
    //    few = many;
    //    many = p;
    //  }

    //  foreach(var p in few) {
    //    if(many.ContainsKey(p.Key)) {
    //      ph1[p.Key] = p.Value;
    //    } else {
    //      ph1.Add(p.Key, p.Value);
    //    }
    //  }

    //  return ph1;
    //}

    //internal static bool IsEmptyPredicate(Predicate predicate) {
    //  return predicate == null || predicate.GetType() == typeof(NullPredicate);
    //}

    //public Dictionary<string, string> GetCNF(DBMSType dbmsType, bool mySqlAnsiQuotes) {
    //  var ret = new Dictionary<string,string>();
    //  var getCNFVisitor = new GetCNFVisitor(dbmsType, mySqlAnsiQuotes);
    //  _predicate.Accept(getCNFVisitor);
    //  foreach(var equality in getCNFVisitor.CNF) {
    //    ret.Add(equality.Key, equality.Value.);
    //  }
    //  return ret;
    //}
  }
}
