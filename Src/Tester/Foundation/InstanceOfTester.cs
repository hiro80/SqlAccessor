using System;
using SqlAccessor;
using NUnit.Framework;

namespace FoundationTester
{
  public class I1
  {
    public string f1() {
      return "my name is f1";
    }

    public string p1 { get; set; }
    public string p2 {
      get { return "my name is p2"; }
    }
    public string p3 {
      set { }
    }
  }

  public class I2<T>
  {
    private string _p1;
    public string p1 {
      get { return _p1; }
      set { _p1 = value; }
    }
  }

  [TestFixture()]
  public class InstanceOfTester
  {
    [Test()]
    public void InstanceOf() {
      ClassOf I1class = new ClassOf(typeof(I1));
      InstanceOf I1ins = new InstanceOf(I1class.CreateInstance());

      Assert.That((string)I1ins.Method("f1") == "my name is f1");

      //SetProperty()
      I1ins.SetProperty("p1", "my name is p1");
      //Assert.That(I1ins.SetProperty("p2", "my name is p2"), Throws.Exception.TypeOf(Of MissingMethodException))
      I1ins.SetProperty("p3", "my name is p3");

      //GetProperty()
      Assert.That((string)I1ins.GetProperty("p1") == "my name is p1");
      Assert.That((string)I1ins.GetProperty("p2") == "my name is p2");
      //Assert.That(I1ins.GetProperty("p3"), Throws.Exception)


      ClassOf I2class = new ClassOf(typeof(I2<>), typeof(string));
      InstanceOf I2ins = new InstanceOf(I2class.CreateInstance());

      //SetProperty()
      I2ins.SetProperty("p1", "my name is p1.");

      //GetProperty()
      Assert.That((string)I2ins.GetProperty("p1") == "my name is p1.");
    }
  }
}