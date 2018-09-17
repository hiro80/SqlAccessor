using System;
using SqlAccessor;

[Serializable()]
public class PersonUnion: IRecord
{
  public int? Id { get; set; }
  public string Name { get; set; }
  public DateTime BirthDay { get; set; }

  public PersonUnion() {
  }

  public PersonUnion(int id, string name, DateTime birthDay) {
    this.Id = id;
    this.Name = name;
    this.BirthDay = birthDay;
  }

  public override bool Equals(object obj) {
    if(!(obj is PersonUnion)) {
      return false;
    }
    PersonUnion person = (PersonUnion)obj;
    //HasValue=FalseなNull許容型同士を比較するとNothingになるので、Trueになるように一致条件を工夫した
    if(this.Id == person.Id && this.Name == person.Name && this.BirthDay == person.BirthDay ) {
      return true;
    } else {
      return false;
    }
  }

  public static bool operator ==(PersonUnion lOperand, PersonUnion rOperand) {
    return !object.Equals(lOperand, null) && lOperand.Equals(rOperand);
  }

  public static bool operator !=(PersonUnion lOperand, PersonUnion rOperand) {
    return object.Equals(lOperand, null) || !lOperand.Equals(rOperand);
  }
}