using System.Collections.Generic;
namespace SqlAccessor
{
  partial class MarkovResultsCache
  {
    public partial class MarkovPredictor
    {

      /// <summary>
      /// 予測状態のランキングリスト
      /// </summary>
      /// <remarks></remarks>
      private class BestRank
      {

        private readonly List<ExpectedState> _rank = new List<ExpectedState>();
        public BestRank() {
          _rank.Add(new ExpectedState("DUMMY", new object(), 0));
        }

        /// <summary>
        /// ランキングリストに予測状態とその出現確率を格納する
        /// </summary>
        /// <param name="expectedState"></param>
        /// <remarks></remarks>
        public void Add(ExpectedState expectedState) {
          //既に同じ状態が存在する場合は、出現確率を合計する
          for(int i = 0; i <= _rank.Count - 1; i++) {
            if(expectedState.State == _rank[i].State) {
              expectedState = new ExpectedState(expectedState.State, expectedState.Appended, expectedState.Provably + _rank[i].Provably);
              _rank.RemoveAt(i);
              break;
            }
          }

          for(int i = _rank.Count - 1; i >= 0; i += -1) {
            if(expectedState.Provably > _rank[i].Provably) {
              _rank.Insert(i + 1, expectedState);
              return;
            }
          }
        }

        public void AddRange(ExpectedState[] expectedStates) {
          foreach(ExpectedState p in expectedStates) {
            this.Add(p);
          }
        }

        /// <summary>
        /// 予測状態のランキングリストを取得する
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<ExpectedState> ToList() {
          List<ExpectedState> ret = new List<ExpectedState>(_rank);
          ret.RemoveAt(0);
          ret.Reverse();
          return ret;
        }
      }

    }
  }
}
