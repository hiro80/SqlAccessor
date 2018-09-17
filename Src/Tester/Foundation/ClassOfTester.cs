using System;
using SqlAccessor;
using NUnit.Framework;

namespace FoundationTester
{
  public class T1
  {
    public static string f1() {
      return "my name is f1";
    }

    public static string p1 { get; set; }

    public static string p2 {
      get {
        return "my name is p2";
      }
    }

    public static string p3 {
      set {
      }
    }
  }

  public class T2<T1>
  {
    private static string _p1;

    public static string p1 {
      get {
        return _p1;
      }
      set {
        _p1 = value;
      }
    }
  }

  public class T3<T, S>
  {
  }

  public class T4<T>
  {
    public T4(T a) {
    }
  }

  [TestFixture()]
  public class ClassOfTester
  {

    [Test()]
    public void ClassOf() {
      ClassOf c1 = new ClassOf(Type.GetType("FoundationTester.T1"));
      object c1instance = c1.CreateInstance();
      Assert.That(c1instance.GetType() == typeof(T1));
      Assert.That((string)c1.Method("f1") == "my name is f1");
      // Assert.That(c1.Function("notExistsFuncName"), Throws.Exception.TypeOf(Of MissingMethodException))

      // SetProperty()
      c1.SetProperty("p1", "my name is p1");
      // Assert.That(c1.SetProperty("p2", "my name is p2"), Throws.Exception.TypeOf(Of MissingMethodException))
      c1.SetProperty("p3", "my name is p3");

      // GetProperty()
      Assert.That((string)c1.GetProperty("p1") == "my name is p1");
      Assert.That((string)c1.GetProperty("p2") == "my name is p2");
      // Assert.That(c1.GetProperty("p3"), Throws.Exception)

      ClassOf c2 = new ClassOf(Type.GetType("FoundationTester.T2`1"), Type.GetType("System.String"));
      object c2instance = c2.CreateInstance();
      Assert.That(c2instance.GetType() == typeof(T2<string>));

      // SetProperty()
      c2.SetProperty("p1", "my name is p1.");

      // GetProperty()
      Assert.That((string)c2.GetProperty("p1") == "my name is p1.");

      ClassOf c3 = new ClassOf(Type.GetType("FoundationTester.T3`2"), Type.GetType("System.String"), Type.GetType("System.Int32"));
      object c3instance = c3.CreateInstance();
      Assert.That(c3instance.GetType() == typeof(T3<string, int>));

      ClassOf c4 = new ClassOf(Type.GetType("FoundationTester.T4`1"), Type.GetType("System.Int32"));
      object c4instance = c4.CreateInstance(100);
      Assert.That(c4instance.GetType() == typeof(T4<int>));
      // Assert.That(c4.CreateInstance(), Throws.TypeOf(Of MissingMethodException))
    }
  }

}