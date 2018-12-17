using System;
using SqlAccessor;

[Serializable()]
public class PersonUnionAll: IRecord
{
  public int? Id { get; set; }
  public string Name { get; set; }
  public DateTime BirthDay { get; set; }

  public PersonUnionAll() {
  }

  public PersonUnionAll(int id, string name, DateTime birthDay) {
    this.Id = id;
    this.Name = name;
    this.BirthDay = birthDay;
  }

  public override bool Equals(object obj) {
    if(!(obj is PersonUnionAll)) {
      return false;
    }
    PersonUnionAll person = (PersonUnionAll)obj;
    //HasValue=FalseなNull許容型同士を比較するとnullになるので、Trueになるように一致条件を工夫した
    if(this.Id == person.Id && this.Name == person.Name && this.BirthDay == person.BirthDay ) {
      return true;
    } else {
      return false;
    }
  }

  public static bool operator ==(PersonUnionAll lOperand, PersonUnionAll rOperand) {
    return !object.Equals(lOperand, null) && lOperand.Equals(rOperand);
  }

  public static bool operator !=(PersonUnionAll lOperand, PersonUnionAll rOperand) {
    return object.Equals(lOperand, null) || !lOperand.Equals(rOperand);
  }
}