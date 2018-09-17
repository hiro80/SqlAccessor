using System;
using SqlAccessor;

[Serializable()]
public class PersonWildcard: IRecord
{
  public int? Id { get; set; }
  public string Name { get; set; }
  public DateTime BirthDay { get; set; }
  public decimal Height { get; set; }
  public decimal? Weight { get; set; }
  public bool? IsDaimyou { get; set; }
  public string Remarks { get; set; }

  public PersonWildcard() {
    this.Height = decimal.MinValue;
  }

  public PersonWildcard(int id, string name, DateTime birthDay, decimal height, decimal? weight, bool? isDaimyou, string remarks) {
    this.Id = id;
    this.Name = name;
    this.BirthDay = birthDay;
    this.Height = height;
    this.Weight = weight;
    this.IsDaimyou = isDaimyou;
    this.Remarks = remarks;
  }

  public override bool Equals(object obj) {
    if(!(obj is PersonWildcard)) {
      return false;
    }
    PersonWildcard person = (PersonWildcard)obj;
    //HasValue=FalseなNull許容型同士を比較するとNothingになるので、Trueになるように一致条件を工夫した
    if((this.Id == person.Id || (!this.IsDaimyou.HasValue && !person.IsDaimyou.HasValue)) && this.Name == person.Name && this.BirthDay == person.BirthDay && this.Height == person.Height && (this.Weight == person.Weight || (!this.IsDaimyou.HasValue && !person.IsDaimyou.HasValue)) && (this.IsDaimyou == person.IsDaimyou || (!this.IsDaimyou.HasValue && !person.IsDaimyou.HasValue)) && this.Remarks == person.Remarks) {
      return true;
    } else {
      return false;
    }
  }

  public static bool operator ==(PersonWildcard lOperand, PersonWildcard rOperand) {
    return !object.Equals(lOperand, null) && lOperand.Equals(rOperand);
  }

  public static bool operator !=(PersonWildcard lOperand, PersonWildcard rOperand) {
    return object.Equals(lOperand, null) || !lOperand.Equals(rOperand);
  }
}