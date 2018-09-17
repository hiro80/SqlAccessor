using System.Diagnostics;
namespace SqlAccessor
{

  public class ConsoleLogger: ILogger
  {

    public void Print(string sql) {
      Trace.WriteLine(sql);
    }
  }
}
