using System;
using System.Collections.Generic;
namespace SqlAccessor
{

  partial class MarkovResultsCache
  {
    /// <summary>
    /// 予測器
    /// </summary>
    /// <remarks></remarks>
    public partial class MarkovPredictors
    {
      //保持する一連の状態数毎に予測器を用意する

      private readonly List<MarkovPredictor> _markovPredictors = new List<MarkovPredictor>();
      public MarkovPredictors(int stateNumber, int predictDepth) {
        for(int i = 1; i <= stateNumber; i++) {
          _markovPredictors.Add(new MarkovPredictor(i, predictDepth));
        }
      }

      /// <summary>
      /// 現在の状態から次の状態を予測する
      /// </summary>
      /// <param name="state">現在の状態</param>
      /// <param name="appended">現在の状態に紐付く添付オブジェクト</param>
      /// <returns>次の状態(予測確度順)</returns>
      /// <remarks>次の状態は現在状態から予測されるので、現在状態の遷移と予測は不可分な操作とする必要がある</remarks>
      public List<MarkovPredictor.ExpectedState> MemorizeAndPredict(string state, object appended = null) {
        //前回の予測がヒットした場合はヒットカウントをインクリメントする
        _predictCount += 1;
        if(ContainsPrevExpectedStates(state)) {
          _hitCount += 1;
        }

        List<MarkovPredictor.ExpectedState> ret = new List<MarkovPredictor.ExpectedState>();

        foreach(MarkovPredictor predictor in _markovPredictors) {
          List<MarkovPredictor.ExpectedState> predict = predictor.MemorizeAndPredict(state, appended);
          //状態数が多い予想器からの予想を優先する
          if(predict.Count > 0) {
            ret = predict;
          }
        }

        //予測の上位3つまでを一時保存する
        this.StoreExpectedStates(ret);

        return ret;
      }

      #region "HitRate()関連"
      private string[] _prevExpectedStates = new string[3];
      private long _hitCount;

      private long _predictCount;
      //予測した状態の上位3つまでを一時保存する
      private void StoreExpectedStates(List<MarkovPredictor.ExpectedState> expectedStates) {
        for(int i = 0; i <= Math.Min(_prevExpectedStates.Length - 1, expectedStates.Count - 1); i++) {
          _prevExpectedStates[i] = expectedStates[expectedStates.Count - 1 - i].State;
        }
      }

      //一時保存した予測状態に引数の状態が含まれるか調べる
      private bool ContainsPrevExpectedStates(string state) {
        for(int i = 0; i <= _prevExpectedStates.Length - 1; i++) {
          if(_prevExpectedStates[i] == state) {
            return true;
          }
        }
        return false;
      }

      public double HitRate {
        get { return (double)_hitCount / (double)_predictCount; }
      }
      #endregion

    }
  }
}

