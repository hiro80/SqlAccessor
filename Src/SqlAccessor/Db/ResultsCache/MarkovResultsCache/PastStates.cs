using System;
using System.Collections.Generic;
namespace SqlAccessor
{

  partial class MarkovResultsCache
  {
    /// <summary>
    /// マルコフ連鎖における一連の状態を表す
    /// </summary>
    /// <remarks></remarks>
    public class PastStates: IEquatable<PastStates>
    {

      private readonly Queue<string> _states;
      private readonly string _statesSeq;

      private readonly int _hashCode;
      private static Queue<string> GetNewQueue(string str, int count) {
        Queue<string> ret = new Queue<string>(count);
        for(int i = 0; i <= count - 1; i++) {
          ret.Enqueue(str);
        }
        return ret;
      }

      public PastStates(int stateNumber)
        : this(PastStates.GetNewQueue("", stateNumber)) {
      }

      public PastStates(Queue<string> states) {
        _states = states;

        //一連の状態を1つの文字列に変換する
        foreach(string s in _states) {
          _statesSeq += s;
        }

        //ハッシュコードを算出する
        _hashCode = _statesSeq.GetHashCode();
      }

      public PastStates GetNextPastStates(string nextState) {
        Queue<string> newStates = new Queue<string>(_states);
        newStates.Dequeue();
        newStates.Enqueue(nextState);
        return new PastStates(newStates);
      }

      #region "このクラスをDictionaryのキー要素とするためのメソッド"
      public bool Equals1(PastStates other) {
        return this._statesSeq == other._statesSeq;
      }
      bool System.IEquatable<PastStates>.Equals(PastStates other) {
        return Equals1(other);
      }

      public override int GetHashCode() {
        return _hashCode;
      }
      #endregion

    }
  }
}
