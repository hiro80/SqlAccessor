using System;
using System.Collections.Generic;
namespace SqlAccessor
{

  partial class MarkovResultsCache
  {
    /// <summary>
    /// 予測器
    /// </summary>
    /// <remarks>スレッドセーフ</remarks>
    public partial class MarkovPredictor
    {
      //_currentStateで保持する状態数
      private readonly int _stateNumber;
      //予測範囲とする遷移回数
      private readonly int _predictDepth;
      //次の遷移先の探索数
      private readonly int _predictWidth = 8;
      //過去の状態遷移の実績
      private readonly Dictionary<PastStates, ExpectedStates> _pastToExpected = new Dictionary<PastStates, ExpectedStates>();
      //ある時点から現在までの一連の状態
      private PastStates _currentState;

      private object _lock = new object();
      /// <summary>
      /// コンストラクタ
      /// </summary>
      /// <param name="stateNumber">保持する状態数</param>
      /// <param name="predictDepth">予測範囲とする遷移回数</param>
      /// <remarks></remarks>
      public MarkovPredictor(int stateNumber, int predictDepth) {
        _stateNumber = stateNumber;
        _predictDepth = predictDepth;
        _currentState = new PastStates(_stateNumber);
      }

      /// <summary>
      /// 現在の状態から次の状態を予測する
      /// </summary>
      /// <param name="state">現在の状態</param>
      /// <param name="appended">現在の状態に紐付く添付オブジェクト</param>
      /// <returns>次の状態(予測確度順)</returns>
      /// <remarks>次の状態は現在状態から予測されるので、現在状態の遷移と予測は不可分な操作とする必要がある</remarks>
      public List<ExpectedState> MemorizeAndPredict(string state, object appended = null) {
        lock(_lock) {
          this.Memorize(state, appended);
          return this.GetExpectedStateRank();
        }
      }

      /// <summary>
      /// 次の状態を記録する
      /// </summary>
      /// <param name="state">次の状態</param>
      /// <param name="appended">次の状態に紐付く添付オブジェクト</param>
      /// <remarks></remarks>
      private void Memorize(string state, object appended = null) {
        if(!_pastToExpected.ContainsKey(_currentState)) {
          _pastToExpected.Add(_currentState, new ExpectedStates());
        }
        _pastToExpected[_currentState].AddExpectedState(state, appended);

        _currentState = _currentState.GetNextPastStates(state);
      }



      private BestRank _bestRank;
      /// <summary>
      /// 次に入力されると予想する状態を予想順で取得する
      /// </summary>
      /// <returns></returns>
      /// <remarks></remarks>
      private List<ExpectedState> GetExpectedStateRank() {
        _bestRank = new BestRank();
        _bestRank.AddRange(this.GetExpectedStateRank(_currentState, 1, _predictDepth));
        return _bestRank.ToList();
      }

      private ExpectedState[] GetExpectedStateRank(PastStates currentState, double provably, int depth) {
        if(depth <= 0) {
          return new ExpectedState[] { };
        }

        if(!_pastToExpected.ContainsKey(currentState)) {
          return new ExpectedState[] { };
        }

        ExpectedState[] nextExpectedStates = _pastToExpected[currentState].GetExpectedStateRank(provably, _predictWidth);
        foreach(ExpectedState nextExpectedState in nextExpectedStates) {
          _bestRank.AddRange(this.GetExpectedStateRank(currentState.GetNextPastStates(nextExpectedState.State), nextExpectedState.Provably, depth - 1));
        }

        return nextExpectedStates;
      }

    }
  }
}
