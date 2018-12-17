using System;
using System.Reflection;
using System.Collections.Generic;

namespace SqlAccessor
{
  /// <summary>
  /// レコードの抽出条件を表す
  /// </summary>
  /// <typeparam name="TRecord"></typeparam>
  /// <remarks></remarks>
  [Serializable()]
  public class Query<TRecord>: IQuery, IQueryExtention<TRecord>
  where TRecord: class, IRecord, new()
  {
    private List<IExp> _criteria = new List<IExp>();
    //(ソートキー、ASC)
    private List<Tuple<string, bool>> _orderExp = new List<Tuple<string, bool>>();
    private int _maxRows = -1;
    //抽出する行の順序番号(開始Index、終了Index)
    private Tuple<int, int> _rowRange;

    /// <summary>
    /// 自オブジェクトのコピーを返す
    /// </summary>
    /// <returns>コピーしたQueryオブジェクト</returns>
    /// <remarks>深いコピー</remarks>
    public object Clone() {
      Query<TRecord> ret = new Query<TRecord>();

      foreach(IExp criteria in _criteria) {
        ret._criteria.Add((IExp)criteria.Clone());
      }
      ret._orderExp = new List<Tuple<string, bool>>(_orderExp);
      ret._maxRows = _maxRows;
      ret._rowRange = _rowRange;

      return ret;
    }

    /// <summary>
    /// 条件を追加する
    /// </summary>
    /// <param name="aExp">条件</param>
    /// <remarks></remarks>
    public void And(IExp aExp) {
      _criteria.Add(aExp);
    }

    /// <summary>
    /// 条件を追加する
    /// </summary>
    /// <param name="aBoolean">条件</param>
    /// <remarks></remarks>
    public void And(bool aBoolean) {
      IExp aExp = null;
      if(aBoolean) {
        aExp = new EqualExp(new Literal(1), new Literal(1));
      } else {
        aExp = new NotEqualExp(new Literal(1), new Literal(1));
      }
      _criteria.Add(aExp);
    }

    /// <summary>
    /// データベースから抽出する時の抽出順序を昇順で追加する
    /// </summary>
    /// <param name="propertyNames">ソートキー</param>
    /// <remarks></remarks>
    public void OrderBy(params string[] propertyNames) {
      //propertyNamesがnullの場合は何も追加しない
      if(propertyNames == null) {
        return;
      }

      foreach(string propertyName in propertyNames) {
        if(!string.IsNullOrEmpty(propertyName)) {
          _orderExp.Add(new Tuple<string, bool>(propertyName, false));
        }
      }
    }

    /// <summary>
    /// データベースから抽出する時の抽出順序を昇順で追加する
    /// </summary>
    /// <param name="propertyNames">ソートキー</param>
    /// <remarks></remarks>
    public void OrderBy(IEnumerable<string> propertyNames) {
      //propertyNamesがnullの場合は何も追加しない
      if(propertyNames == null) {
        return;
      }

      foreach(string propertyName in propertyNames) {
        this.OrderBy(propertyName);
      }
    }

    /// <summary>
    /// データベースから抽出する時の抽出順序を降順で追加する
    /// </summary>
    /// <param name="propertyNames">ソートキー</param>
    /// <remarks></remarks>
    public void OrderByDesc(params string[] propertyNames) {
      //propertyNamesがnullの場合は何も追加しない
      if(propertyNames == null) {
        return;
      }

      foreach(string propertyName in propertyNames) {
        if(!string.IsNullOrEmpty(propertyName)) {
          _orderExp.Add(new Tuple<string, bool>(propertyName, true));
        }
      }
    }

    /// <summary>
    /// データベースから抽出する時の抽出順序を降順で追加する
    /// </summary>
    /// <param name="propertyNames">ソートキー</param>
    /// <remarks></remarks>
    public void OrderByDesc(IEnumerable<string> propertyNames) {
      //propertyNamesがnullの場合は何も追加しない
      if(propertyNames == null) {
        return;
      }

      foreach(string propertyName in propertyNames) {
        this.OrderByDesc(propertyName);
      }
    }

    /// <summary>
    /// データベースから抽出する時の最大抽出件数を指定する
    /// </summary>
    /// <param name="max">最大抽出件数</param>
    /// <remarks></remarks>
    public void MaxRows(int max) {
      _maxRows = max;
    }

    /// <summary>
    /// データベースから抽出する時の抽出範囲を、行の順序番号で指定する
    /// </summary>
    /// <param name="beginRowIndex"></param>
    /// <param name="endRowIndex"></param>
    /// <remarks></remarks>
    public void RowRange(int beginRowIndex, int endRowIndex) {
      _rowRange = new Tuple<int, int>(beginRowIndex, endRowIndex);
    }

    //抽出条件をQueryで指定する
    void IQueryExtention<TRecord>.AndedFromQuery(Query<TRecord> aQuery) {
      _criteria.AddRange(aQuery._criteria);
    }

    //最上位階層のAND句(WHERE句直下のAND句)における、プロパティに対する一致条件の式を取得する
    internal List<Tuple<string, string>> GetMatchingConditions() {
      List<Tuple<string, string>> ret = new List<Tuple<string, string>>();
      foreach(IExp aExp in this._criteria) {
        ret.AddRange(aExp.ToPropertyValuePairs());
      }
      return ret;
    }

    internal void RemoveEqualExp(Tuple<string, string> match) {
      EqualExp matchEqExp = new EqualExp(val.of(match.Item1), new Literal(match.Item2));

      List<IExp> newCriteria = new List<IExp>();
      foreach(IExp aExp in this._criteria) {
        IExp replacedExp = aExp.RemoveEqualExp(matchEqExp);
        if(replacedExp != null) {
          newCriteria.Add(replacedExp);
        }
      }

      _criteria = newCriteria;
    }

    //保持する抽出条件から、NULL表現値を被演算子に持つ抽出条件を削除し、
    //プロパティ型からSQLリテラル型に型変換する
    void IQueryExtention<TRecord>.CastToSqlLiteralType(ICaster caster, IViewInfoGetter viewInfoGetter) {
      //ループの操作対象をループ内で削除するので、逆順にループさせないといけない
      int i = _criteria.Count - 1;
      while(i >= 0) {
        //Queryオブジェクトに格納された値オブジェクトがNULL表現値の場合、その条件式を削除する
        _criteria[i] = _criteria[i].RemoveExpIfNull(caster);
        if(_criteria[i] == null) {
          //配列の要素を削除する
          _criteria.RemoveAt(i);
          i -= 1;
          continue;
        }

        //Queryオブジェクトに格納された値オブジェクトを型変換する
        _criteria[i].CastToSqlLiteralType(caster, viewInfoGetter);
        i -= 1;
      }
    }

    public string GetWhereExp() {
      string retStr = "";
      foreach(IExp aExp in this._criteria) {
        if(!string.IsNullOrEmpty(retStr)) {
          retStr += Environment.NewLine + "  AND ";
        }
        retStr += aExp.ToString();
      }
      return retStr;
    }

    public List<Tuple<string, bool>> GetOrderByExp() {
      return _orderExp;
    }

    public int GetMaxRows() {
      return _maxRows;
    }

    public Tuple<int, int> GetRowRange() {
      return _rowRange;
    }

    public bool HasCriteria() {
      return _criteria.Count > 0;
    }

    public Query<toRecord> CastTo<toRecord>() where toRecord: class, IRecord, new() {
      //キャスト先Queryオブジェクト
      Query<toRecord> ret = new Query<toRecord>();

      //無条件にキャスト先Queryオブジェクトにコピーできる値をコピーする
      ret._maxRows = _maxRows;
      if(_rowRange != null) {
        ret._rowRange = new Tuple<int, int>(_rowRange.Item1, _rowRange.Item2);
      }

      //キャスト先レコードのプロパティ情報の取得
      PropertyInfo[] toProperties = (new toRecord()).GetType().GetProperties();

      //キャスト先レコードに存在しないプロパティを含んでいる抽出条件は除外する
      foreach(IExp exp in _criteria) {
        IExp toExp = exp.RemoveExp(toProperties);
        if(toExp != null) {
          ret._criteria.Add(toExp);
        }
      }

      //キャスト先レコードに存在しないソートキー(プロパティ)は除外する
      foreach(Tuple<string, bool> kv in _orderExp) {
        string propertyName = kv.Item1;
        bool asc = kv.Item2;
        foreach(PropertyInfo aPropertyInfo in toProperties) {
          if(aPropertyInfo.Name == propertyName) {
            ret._orderExp.Add(new Tuple<string, bool>(propertyName, asc));
          }
        }
      }

      return ret;
    }
  }
}
