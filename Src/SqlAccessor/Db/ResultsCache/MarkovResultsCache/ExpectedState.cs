using System.Collections.Generic;
namespace SqlAccessor
{
  partial class MarkovResultsCache
  {
    public partial class MarkovPredictor
    {

      /// <summary>
      /// マルコフ連鎖における次に予想する状態
      /// </summary>
      /// <remarks>スレッドセーフ</remarks>
      public class ExpectedState: IComparer<ExpectedState>
      {

        public readonly string State;
        public readonly object Appended;

        public readonly double Provably;
        public ExpectedState(string state, object appended, double probability) {
          this.State = state;
          this.Appended = appended;
          this.Provably = probability;
        }

        public int Compare(ExpectedState x, ExpectedState y) {
          return x.Provably.CompareTo(y.Provably);
        }
      }

    }
  }
}

