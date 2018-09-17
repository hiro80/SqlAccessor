using System;
using SqlAccessor;

[Serializable()]

public class PersonGroupBy: IRecord
{
  public long Count { get; set; }
  public DateTime BirthDay { get; set; }

  public PersonGroupBy() {
    this.Count = long.MinValue;
  }
  public PersonGroupBy(DateTime birthDay) {
    this.Count = long.MinValue;
    this.BirthDay = birthDay;
  }

  public PersonGroupBy(long count, DateTime birthDay) {
    this.Count = count;
    this.BirthDay = birthDay;
  }
  public override bool Equals(object obj) {
    if(!(obj is PersonGroupBy)) {
      return false;
    }
    PersonGroupBy person = (PersonGroupBy)obj;
    //HasValue=FalseなNull許容型同士を比較するとNothingになるので、Trueになるように一致条件を工夫した
    if(this.Count == person.Count && this.BirthDay == person.BirthDay) {
      return true;
    } else {
      return false;
    }
  }

  public static bool operator ==(PersonGroupBy lOperand, PersonGroupBy rOperand) {
    return !object.Equals(lOperand, null) && lOperand.Equals(rOperand);
  }

  public static bool operator !=(PersonGroupBy lOperand, PersonGroupBy rOperand) {
    return object.Equals(lOperand, null) || !lOperand.Equals(rOperand);
  }
}

