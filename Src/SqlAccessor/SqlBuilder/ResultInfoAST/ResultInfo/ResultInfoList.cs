using System;
using System.Collections;
using System.Collections.Generic;

namespace MiniSqlParser
{
  [System.Diagnostics.DebuggerDisplay("Count: {Count}")]
  public class ResultInfoList: IEnumerable<IResultInfo>
  {
    internal List<IResultInfoInternal> Items { get; private set; }

    internal void Add(IResultInfoInternal item) {
      this.Items.Add(item);
    }

    internal void AddRange(IEnumerable<IResultInfoInternal> items) {
      this.Items.AddRange(items);
    }

    internal void Insert(int index, IResultInfoInternal item) {
      if(index < 0) {
        index = reverseIndex(index);
      }
      this.Items.Insert(index, item);
    }

    internal void RemoveAt(int index) {
      if(index < 0) {
        index = reverseIndex(index);
      }
      this.Items.RemoveAt(index);
    }

    internal void Clear() {
      this.Items.Clear();
    }

    // 負数の指定位置は右からの位置指定(-1が右端)である
    private int reverseIndex(int rIndex) {
      var ret = this.Items.Count + rIndex + 1;
      if(ret < 0) {
        throw new ArgumentOutOfRangeException("rIndex",
            "Reverse index is out of range in Node Collections");
      }
      return ret;
    }

    internal int FindResultInfo(Column destItem
                              , bool explicitOnly = false
                              , bool ignoreCase = true) {
      var ret = -1;
      IResultInfoInternal findItem = null;
      
      for(var i=0; i<this.Items.Count; ++i){
        var resultInfo = this.Items[i];
        if(explicitOnly && !resultInfo.ExplicitDecl) {
          continue;
        }
        if(resultInfo.IsDirectSource(destItem, ignoreCase)) {
          // 抽出元SELECT項目の候補が2つ以上ある場合は例外を返す
          if(findItem == null) {
            findItem = resultInfo;
            ret = i;
          } else {
            throw new InvalidASTStructureError(
              "Ambiguous column name: " + resultInfo.ColumnAliasName);
          }
        }
      }
      return ret;
    }

    internal int FindNotNullResultInfo(bool explicitOnly = false) {
      for(var i = 0; i < this.Items.Count; ++i) {
        var resultInfo = this.Items[i];
        if(explicitOnly && !resultInfo.ExplicitDecl) {
          continue;
        }
        if(!resultInfo.IsNullable) {
          return i;
        }
      }
      return -1;
    }

    public bool ContainsAggregateFunc() {
      foreach(var item in Items) {
        if(item.Type == ResultInfoType.Query){
          if(((QueryResultInfo)item).IsAggregative()) {
            return true;
          }
        } else if(item.Type == ResultInfoType.Count){
          return true;
        } 
      }
      return false;
    }

    public bool ContainsNotNull() {
      foreach(var item in Items) {
        if(!item.IsNullable) {
          return true;
        }
      }
      return false;
    }

    IEnumerator<IResultInfo> IEnumerable<IResultInfo>.GetEnumerator() {
      return this.Items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      //return this.Items.GetEnumerator();
      foreach(var resultInfoInternal in this.Items) {
        yield return (IResultInfo)resultInfoInternal;
      }
    }

    public int Count {
      get {
        return this.Items.Count;
      }
    }

    internal IResultInfoInternal this[int i] {
      get {
        return this.Items[i];
      }
      set {
        this.Items[i] = value;
      }
    }

    public void Accept(IResultInfoVisitor visitor) {
      if(visitor == null) {
        throw new ArgumentNullException("visitor");
      }
      foreach(var resultInfo in this) {
        resultInfo.Accept(visitor);
      }
    }

    public ResultInfoList() {
      this.Items = new List<IResultInfoInternal>();
    }

    internal IFromSource FromSource { get; set; }
  }

}
