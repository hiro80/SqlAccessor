using System.Data.Common;
using System.Data.Odbc;
using System.Diagnostics;

namespace SqlAccessor
{
  /// <summary>
  /// SQLiteへのODBC接続を表す
  /// </summary>
  /// <remarks>
  /// SQLiteODBC Ver0.96以上でないとPRAGMA文から結果を取得できない
  /// そして、日本語版はVer0.83止まりである
  /// </remarks>
  internal class SqliteOdbcDbConn: DbConn
  {
    public SqliteOdbcDbConn(string connStr
                          , IResultsCache aResultsCache
                          , bool commitAtFinalizing
                          , bool debugPrint
                          , ILogger logger)
      : base(connStr, aResultsCache, commitAtFinalizing, debugPrint, logger) {
    }

    protected override DbConnection CreateDbConnection() {
      return new OdbcConnection();
    }

    protected override DbCommand CreateDbCommand(string sql
                                               , DbConnection aDbConnection
                                               , DbTransaction aDbTransaction) {
      //_debugPrint = Trueの場合にSQL文発行ログを出力する
      if(_debugPrint) {
        _logger.Print(sql);
      }

      //第2,3引数にはSqliteOdbcConn.CreateDbConnection()の戻り値、及びその戻り値から生成されるOdbcTransaction型オブジェクトが
      //渡されるので、不正な型のオブジェクトが渡されることはない。
      return new OdbcCommand(sql, (OdbcConnection)aDbConnection, (OdbcTransaction)aDbTransaction);
    }

    protected override DataAdapter CreateDataAdapter(DbCommand aDbCommand) {
      //引数にはSqliteOdbcConn.CreateDbCommandの戻り値が渡されるので、不正な型のオブジェクトが渡されることはない。
      return new OdbcDataAdapter((OdbcCommand)aDbCommand);
    }

    protected override string MakeExpEvalSql(string expression) {
      return "SELECT 1 WHERE " + expression;
    }

    protected override bool IsDuplicateKeyException(System.Exception ex) {
      if(ex is OdbcException) {
        OdbcException sqliteOdbcException = (OdbcException)ex;
        //if(sqliteOdbcException.ErrorCode = SQLiteErrorCode.Constraint &&
        //   sqliteException.Message != null &&
        //   sqliteException.Message.Contains("must be unique")) {
        //  return true;
        //}
      }

      return false;
    }
  }
}
