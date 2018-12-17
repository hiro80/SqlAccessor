using System;
using SqlAccessor;

[Serializable()]
public class PersonPair: IRecord
{
  public int?          Id { get; set; }
  public string        Name { get; set; }
  public DateTime      BirthDay { get; set; }
  public int?          SuccessorId { get; set; }
  public string        SuccessorName { get; set; }
  public DateTime      SuccessorBirthDay { get; set; }

  public PersonPair() { }

  public PersonPair(int id, string name, DateTime birthDay,
                    int successorId, string successorName, DateTime successorBirthDay) {
    this.Id                = id;
    this.Name              = name;
    this.BirthDay          = birthDay;
    this.SuccessorId       = successorId;
    this.SuccessorName     = successorName;
    this.SuccessorBirthDay = successorBirthDay;
  }

  public override bool Equals(object obj) {
    if(!(obj is PersonPair)) {
      return false;
    }
    PersonPair personPair = (PersonPair)obj;
    //HasValue=FalseなNull許容型同士を比較するとnullになるので、Trueになるように一致条件を工夫した
    if(this.Id == personPair.Id){
      return true;
    } else {
      return false;
    }
  }

  public static bool operator ==(PersonPair lOperand, PersonPair rOperand) {
    return !object.Equals(lOperand, null) && lOperand.Equals(rOperand);
  }

  public static bool operator !=(PersonPair lOperand, PersonPair rOperand) {
    return object.Equals(lOperand, null) || !lOperand.Equals(rOperand);
  }
}