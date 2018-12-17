using System;
using SqlAccessor;

[Serializable()]
public class PersonAutoWhereFalse: IRecord
{
  public int? Id { get; set; }
  public string Name { get; set; }
  public DateTime BirthDay { get; set; }
  public decimal Height { get; set; }
  public decimal? Weight { get; set; }
  public bool? IsDaimyou { get; set; }
  public string Remarks { get; set; }

  public PersonAutoWhereFalse() {
    this.Height = decimal.MinValue;
  }

  public PersonAutoWhereFalse(int id, string name, DateTime birthDay, decimal height, decimal? weight, bool? isDaimyou, string remarks) {
    this.Id = id;
    this.Name = name;
    this.BirthDay = birthDay;
    this.Height = height;
    this.Weight = weight;
    this.IsDaimyou = isDaimyou;
    this.Remarks = remarks;
  }

  public override bool Equals(object obj) {
    if(!(obj is PersonAutoWhereFalse)) {
      return false;
    }
    PersonAutoWhereFalse PersonAutoWhereFalse = (PersonAutoWhereFalse)obj;
    //HasValue=FalseなNull許容型同士を比較するとnullになるので、Trueになるように一致条件を工夫した
    if((this.Id == PersonAutoWhereFalse.Id || 
      (!this.IsDaimyou.HasValue && !PersonAutoWhereFalse.IsDaimyou.HasValue)) && 
      this.Name == PersonAutoWhereFalse.Name && 
      this.BirthDay == PersonAutoWhereFalse.BirthDay && 
      this.Height == PersonAutoWhereFalse.Height &&
      (this.Weight == PersonAutoWhereFalse.Weight || (!this.Weight.HasValue && !PersonAutoWhereFalse.Weight.HasValue)) && 
      (this.IsDaimyou == PersonAutoWhereFalse.IsDaimyou || (!this.IsDaimyou.HasValue && !PersonAutoWhereFalse.IsDaimyou.HasValue)) && 
      this.Remarks == PersonAutoWhereFalse.Remarks) {
      return true;
    } else {
      return false;
    }
  }

  public static bool operator ==(PersonAutoWhereFalse lOperand, PersonAutoWhereFalse rOperand) {
    return !object.Equals(lOperand, null) && lOperand.Equals(rOperand);
  }

  public static bool operator !=(PersonAutoWhereFalse lOperand, PersonAutoWhereFalse rOperand) {
    return object.Equals(lOperand, null) || !lOperand.Equals(rOperand);
  }
}