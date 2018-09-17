using System;
using System.Collections.Generic;
using System.ComponentModel;
using MiniSqlParser;

namespace MiniSqlParser
{
  /// <summary>
  /// プレースホルダを取得する
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  public class GetPlaceHoldersVisitor: Visitor
  {
    private Dictionary<string, string> _placeHolders;


    public GetPlaceHoldersVisitor() : this(null) { }

    public GetPlaceHoldersVisitor(Dictionary<string, string> placeHolders) {
      if(placeHolders == null) {
        _placeHolders = new Dictionary<string, string>();
      } else {
        _placeHolders = placeHolders;
      }
      _allPlaceHolders = new HashSet<Tuple<string, string>>();
      _placedHolders = new HashSet<Tuple<string, string>>();
      _unPlacedHolders = new HashSet<string>();
    }

    private HashSet<Tuple<string, string>> _allPlaceHolders;
    public HashSet<Tuple<string, string>> GetAllPlaceHolders {
      get {
        return _allPlaceHolders;
      }
    }

    private HashSet<Tuple<string, string>> _placedHolders;
    public HashSet<Tuple<string, string>> GetPlacedHolders {
      get {
        return _placedHolders;
      }
    }

    private HashSet<string> _unPlacedHolders;
    public HashSet<string> GetUnPlacedHolders {
      get{
        return _unPlacedHolders;
      }
    }

    public override void Visit(PlaceHolderExpr expr) {
      this.GetPlaceHolderInfo(expr.LabelName);
    }

    public override void Visit(PlaceHolderPredicate predicate) {
      this.GetPlaceHolderInfo(predicate.LabelName);
    }

    private void GetPlaceHolderInfo(string placeHolderName) {
      if(_placeHolders.ContainsKey(placeHolderName)) {
        // 値が適用済みのプレースホルダを記録する
        _placedHolders.Add(Tuple.Create(placeHolderName, _placeHolders[placeHolderName]));
        // 値の有無に関わらず全てのプレースホルダを記録する
        _allPlaceHolders.Add(Tuple.Create(placeHolderName, _placeHolders[placeHolderName]));
      } else {
        // 値が未適用のプレースホルダを記録する
        _unPlacedHolders.Add(placeHolderName);
        // 値の有無に関わらず全てのプレースホルダを記録する
        _allPlaceHolders.Add(Tuple.Create(placeHolderName, ""));
      }
    }
  }
}
