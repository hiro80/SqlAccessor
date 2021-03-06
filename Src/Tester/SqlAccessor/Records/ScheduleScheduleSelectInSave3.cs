﻿using System;
using SqlAccessor;

[Serializable()]
public class ScheduleSelectInSave3: IRecord
{
  public int? Id { get; set; }
  public string Name { get; set; }
  public DateTime BirthDay { get; set; }
  public decimal Height { get; set; }
  public decimal? Weight { get; set; }
  public bool? IsDaimyou { get; set; }
  public string Remarks { get; set; }
  public DateTime Date { get; set; }
  public string Subject { get; set; }

  public ScheduleSelectInSave3() {
    this.Height = decimal.MinValue;
  }

  public ScheduleSelectInSave3(int id
                            , string name
                            , DateTime birthDay
                            , decimal height
                            , decimal? weight
                            , bool? isDaimyou
                            , string remarks
                            , DateTime date
                            , string subject) {
    this.Id = id;
    this.Name = name;
    this.BirthDay = birthDay;
    this.Height = height;
    this.Weight = weight;
    this.IsDaimyou = isDaimyou;
    this.Remarks = remarks;
    this.Date = date;
    this.Subject = subject;
  }

  public override bool Equals(object obj) {
    if(!(obj is ScheduleSelectInSave3)) {
      return false;
    }
    ScheduleSelectInSave3 schedule = (ScheduleSelectInSave3)obj;
    //HasValue=FalseなNull許容型同士を比較するとnullになるので、Trueになるように一致条件を工夫した
    if(this.Id == schedule.Id){
      return true;
    }else{
      return false;
    }
  }

  public static bool operator ==(ScheduleSelectInSave3 lOperand, ScheduleSelectInSave3 rOperand) {
    return !object.Equals(lOperand, null) && lOperand.Equals(rOperand);
  }

  public static bool operator !=(ScheduleSelectInSave3 lOperand, ScheduleSelectInSave3 rOperand) {
    return object.Equals(lOperand, null) || !lOperand.Equals(rOperand);
  }
}