using System;
using SqlAccessor;

[Serializable()]
public class LuckyNo: IRecord
{
  public DateTime? Date { get; set; }
  public int? Number { get; set; }

  public LuckyNo() {
    this.Date = DateTime.MinValue;
    this.Number = int.MinValue;
  }

  public LuckyNo(DateTime? date, int? luckyNo) {
    this.Date = date;
    this.Number = luckyNo;
  }

  public override bool Equals(object obj) {
    if(!(obj is LuckyNo)) {
      return false;
    }
    LuckyNo luckyNo = (LuckyNo)obj;
    //HasValue=FalseなNull許容型同士を比較するとNothingになるので、Trueになるように一致条件を工夫した
    if((this.Date ==    luckyNo.Date    || (!this.Date.HasValue    && !luckyNo.Date.HasValue)) &&
       (this.Number == luckyNo.Number || (!this.Number.HasValue && !luckyNo.Number.HasValue))) {
      return true;
    } else {
      return false;
    }
  }

  public static bool operator ==(LuckyNo lOperand, LuckyNo rOperand) {
    return !object.Equals(lOperand, null) && lOperand.Equals(rOperand);
  }

  public static bool operator !=(LuckyNo lOperand, LuckyNo rOperand) {
    return object.Equals(lOperand, null) || !lOperand.Equals(rOperand);
  }
}