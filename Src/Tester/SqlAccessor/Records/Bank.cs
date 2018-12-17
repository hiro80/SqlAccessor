using System;
using SqlAccessor;

[Serializable()]
public class Bank: IRecord
{
  public int?   BankId        { get; set; }
  public string BankName      { get; set; }
  public string BankKanaName  { get; set; }
  public int?   BranchId      { get; set; }
  public string BranchName    { get; set; }
  public string BranchKanaName{ get; set; }

  public Bank() { }

  public Bank(int?   bankId
            , string bankName
            , string bankKanaName
            , int?   branchId
            , string branchName
            , string branchKanaName) {
    this.BankId = bankId;
    this.BankName = bankName;
    this.BankKanaName = bankKanaName;
    this.BranchId = branchId;
    this.BranchName = branchName;
    this.BranchKanaName = branchKanaName;
  }

  public override bool Equals(object obj) {
    if(!(obj is Bank)) {
      return false;
    }
    Bank bank = (Bank)obj;
    //HasValue=FalseなNull許容型同士を比較するとnullになるので、Trueになるように一致条件を工夫した
    if(this.BankId == bank.BankId && this.BranchId == bank.BranchId) {
      return true;
    }else{
      return false;
    }
  }

  public static bool operator ==(Bank lOperand, Bank rOperand) {
    return lOperand != null && lOperand.Equals(rOperand);
  }

  public static bool operator !=(Bank lOperand, Bank rOperand) {
    return lOperand == null || !(lOperand == rOperand);
  }
}