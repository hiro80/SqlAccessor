using System.Collections;
using System.Collections.Generic;
namespace SqlAccessor
{

  partial class MarkovResultsCache
  {
    public partial class MarkovPredictor
    {

      /// <summary>
      /// マルコフ連鎖における次に予想する状態のコレクション
      /// </summary>
      /// <remarks></remarks>
      private class ExpectedStates
      {
        //文字列値とその出現回数値
        private readonly Dictionary<string, int> _stateToFrequency = new Dictionary<string, int>();
        //文字列値とその附属オブジェクト
        private readonly Dictionary<string, object> _stateToAppended = new Dictionary<string, object>();
        //出現頻度値のランキングリスト
        private readonly List<LinkedList<string>> _frequencyRank = new List<LinkedList<string>>();
        //全ての出現回数

        private int _totalFreqency;
        //添付オブジェクトを取得する
        private object GetAppended(string str) {
          if(!_stateToAppended.ContainsKey(str)) {
            return null;
          }
          return _stateToAppended[str];
        }

        //ランキングリストから指定した出現回数の文字列リストを取得する
        private LinkedList<string> FrequencyRank(int rank) {
          while(_frequencyRank.Count <= rank) {
            _frequencyRank.Add(new LinkedList<string>());
          }
          return _frequencyRank[rank];
        }

        //文字列値を登録する
        public void AddExpectedState(string expectedState, object appended = null) {
          if(!_stateToFrequency.ContainsKey(expectedState)) {
            //初回登録の場合
            _stateToFrequency.Add(expectedState, 0);
            FrequencyRank(0).AddLast(expectedState);
            if(appended != null) {
              _stateToAppended.Add(expectedState, appended);
            }
          } else {
            //2回目以降の登録の場合
            int currentFreq = _stateToFrequency[expectedState];
            //出現回数の増加により、ランキングが1つ上昇する
            FrequencyRank(currentFreq).Remove(expectedState);
            FrequencyRank(currentFreq + 1).AddLast(expectedState);
            _stateToFrequency[expectedState] += 1;
          }

          _totalFreqency += 1;
        }

        /// <summary>
        /// 予測状態の確率順リストを出力する
        /// </summary>
        /// <param name="priorProbability">事前確率</param>
        /// <param name="top">出力数</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public ExpectedState[] GetExpectedStateRank(double priorProbability, int top) {
          int retCount = System.Math.Min(top - 1, _stateToFrequency.Count - 1);
          ExpectedState[] ret = new ExpectedState[retCount + 1];

          for(int frequency = 1; frequency <= _frequencyRank.Count; frequency++) {
            foreach(string state in _frequencyRank[frequency - 1]) {
              if(retCount < 0) {
                return ret;
              }
              ret[retCount] = new ExpectedState(state
                                              , this.GetAppended(state)
                                              , (double)frequency / (double)_totalFreqency * priorProbability);
              retCount -= 1;
            }
          }

          return ret;
        }

        public ExpectedState[] GetExpectedStateRank(double priorProbability) {
          return this.GetExpectedStateRank(priorProbability, _stateToFrequency.Count);
        }
      }

    }
  }
}
