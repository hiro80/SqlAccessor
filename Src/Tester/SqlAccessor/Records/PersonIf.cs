using System;
using SqlAccessor;

[Serializable()]
public class PersonIf: Person
{
  public PersonIf() : base() {
  }

  public PersonIf(int id, string name, DateTime birthDay, decimal height, decimal? weight, bool? isDaimyou, string remarks) 
    : base(id, name, birthDay, height, weight, isDaimyou, remarks){
  }

  public override bool Equals(object obj) {
    if(!(obj is PersonIf)) {
      return false;
    }
    Person person = (Person)obj;
    //HasValue=FalseなNull許容型同士を比較するとnullになるので、Trueになるように一致条件を工夫した
    if((this.Id == person.Id || (!this.IsDaimyou.HasValue && !person.IsDaimyou.HasValue)) && this.Name == person.Name && this.BirthDay == person.BirthDay && this.Height == person.Height && (this.Weight == person.Weight || (!this.IsDaimyou.HasValue && !person.IsDaimyou.HasValue)) && (this.IsDaimyou == person.IsDaimyou || (!this.IsDaimyou.HasValue && !person.IsDaimyou.HasValue)) && this.Remarks == person.Remarks) {
      return true;
    } else {
      return false;
    }
  }

  public static bool operator ==(PersonIf lOperand, PersonIf rOperand) {
    return !object.Equals(lOperand, null) && lOperand.Equals(rOperand);
  }

  public static bool operator !=(PersonIf lOperand, PersonIf rOperand) {
    return object.Equals(lOperand, null) || !lOperand.Equals(rOperand);
  }
}