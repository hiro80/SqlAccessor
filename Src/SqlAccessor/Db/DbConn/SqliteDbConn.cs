using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;

namespace SqlAccessor
{
  /// <summary>
  /// SQLiteへの接続を表す
  /// </summary>
  /// <remarks>SQLite.NETを利用する</remarks>
  internal class SqliteDbConn: DbConn
  {
    public SqliteDbConn(string connStr
                      , IResultsCache aResultsCache
                      , bool commitAtFinalizing
                      , bool debugPrint
                      , ILogger logger)
      : base(connStr, aResultsCache, commitAtFinalizing, debugPrint, logger) {
    }

    protected override System.Data.Common.DbConnection CreateDbConnection() {
      return new SQLiteConnection();
    }

    protected override DbCommand CreateDbCommand(string sql
                                               , DbConnection aDbConnection
                                               , DbTransaction aDbTransaction) {
      //_debugPrint = Trueの場合にSQL文発行ログを出力する
      if(_debugPrint) {
        _logger.Print(sql);
      }

      return new SQLiteCommand(sql
                             , (SQLiteConnection)aDbConnection
                             , (SQLiteTransaction)aDbTransaction);
    }

    protected override DataAdapter CreateDataAdapter(DbCommand aDbCommand) {
      return new SQLiteDataAdapter((SQLiteCommand)aDbCommand);
    }

    protected override string MakeExpEvalSql(string expression) {
      return "SELECT 1 WHERE " + expression;
    }

    protected override bool IsDuplicateKeyException(System.Exception ex) {
      if(ex is SQLiteException) {
        SQLiteException sqliteException = (SQLiteException)ex;
        if(sqliteException.ErrorCode == (int)SQLiteErrorCode.Constraint &&
           sqliteException.Message != null &&
           sqliteException.Message.Contains("UNIQUE constraint")) {
          return true;
        }
      }

      return false;
    }

  }
}
