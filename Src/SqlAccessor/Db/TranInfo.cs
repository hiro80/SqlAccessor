namespace SqlAccessor
{
  public class TranInfo
  {
    private long _apTranId;
    private int _state;
    private System.DateTime _startTime;
    private System.DateTime _endTime;
    private Tran _tran;

    internal TranInfo(Tran tran) {
      _tran = tran;
      _apTranId = tran.ApTranId;
      _state = 0;
      _startTime = System.DateTime.Now;
      _endTime = default(System.DateTime);
    }
  }
}
