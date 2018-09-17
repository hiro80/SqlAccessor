using System;
using System.Collections;
using System.Collections.Generic;

namespace MiniSqlParser
{
  [System.Diagnostics.DebuggerDisplay("Count: {Count}")]
  public class ResultInfoLists: IEnumerable<ResultInfoList>
  {
    public List<ResultInfoList> Items { get; private set; }

    internal void Add(ResultInfoList item) {
      this.Items.Add(item);
    }

    internal void AddRange(IEnumerable<ResultInfoList> items) {
      this.Items.AddRange(items);
    }

    internal void Insert(int index, ResultInfoList item) {
      if(index < 0) {
        index = reverseIndex(index);
      }
      this.Items.Insert(index, item);
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

    IEnumerator<ResultInfoList> IEnumerable<ResultInfoList>.GetEnumerator() {
      return this.Items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return this.Items.GetEnumerator();
    }

    internal IEnumerable<IResultInfoInternal> GetAllResultInfo() {
      foreach(var resultInfoList in this.Items){
        foreach(var resultInfo in resultInfoList) {
          yield return (IResultInfoInternal)resultInfo;
        }
      }
    }

    public int Count {
      get {
        return this.Items.Count;
      }
    }

    public ResultInfoList this[int i] {
      get {
        return this.Items[i];
      }
      internal set {
        this.Items[i] = value;
      }
    }

    public ResultInfoList ToList() {
      var ret = new ResultInfoList();
      foreach(var resultInfoList in this) {
        ret.AddRange(resultInfoList.Items);
      }
      return ret;
    }

    public void Accept(IResultInfoVisitor visitor) {
      if(visitor == null) {
        throw new ArgumentNullException("visitor");
      }
      foreach(var resultInfo in this) {
        resultInfo.Accept(visitor);
      }
    }

    public ResultInfoLists() {
      this.Items = new List<ResultInfoList>();
    }

    public ResultInfoLists(params ResultInfoList[] items) : this() {
      this.Items.AddRange(items);
    }
  }

}
