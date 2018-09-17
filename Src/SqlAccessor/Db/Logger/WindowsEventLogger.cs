using System.Diagnostics;
namespace SqlAccessor
{
  public class WindowsEventLogger: ILogger
  {

    private readonly string _source = "SqlAccessor";
    private readonly string _log = "Application";

    private readonly string _event = "SQL Executing";

    public void Print(string sql) {
      //'管理者権限がないとソースの有無をチェックできない
      //If Not EventLog.SourceExists(_source) Then
      //  'イベントソースが存在しない場合、SqlAccessorソースを作成する
      //  ' (管理者権限がないとソースを新規作成できない)
      //  EventLog.CreateEventSource(_source, _log)
      //End If

      //イベントログを出力する
      EventLog.WriteEntry(".NET Runtime", sql, EventLogEntryType.Information);
    }
  }
}
