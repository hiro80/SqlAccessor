using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;

namespace SqlAccessor
{

  /// <summary>
  /// Microsoft SQL Serverへの接続を表す
  /// </summary>
  /// <remarks></remarks>
  internal class MsSqlDbConn: DbConn
  {
    public MsSqlDbConn(string connStr
                      , IResultsCache aResultsCache
                      , bool commitAtFinalizing
                      , bool debugPrint
                      , ILogger logger)
      : base(connStr, aResultsCache, commitAtFinalizing, debugPrint, logger) {
    }

    protected override DbConnection CreateDbConnection() {
      return new SqlConnection();
    }

    protected override DbCommand CreateDbCommand(string sql
                                                , DbConnection aDbConnection
                                                , DbTransaction aDbTransaction) {
      //_debugPrint = Trueの場合にSQL文発行ログを出力する
      if(_debugPrint) {
        _logger.Print(sql);
      }

      //第2,3引数にはSqlDbConn.CreateDbConnection()の戻り値、及びその戻り値から生成されるSqlTransaction型オブジェクトが
      //渡されるので、不正な型のオブジェクトが渡されることはない。
      return new SqlCommand(sql, (SqlConnection)aDbConnection, (SqlTransaction)aDbTransaction);
    }

    protected override DataAdapter CreateDataAdapter(DbCommand aDbCommand) {
      //引数にはSqlDbConn.CreateDbCommandの戻り値が渡されるので、不正な型のオブジェクトが渡されることはない。
      return new SqlDataAdapter((SqlCommand)aDbCommand);
    }

    protected override string MakeExpEvalSql(string expression) {
      return "SELECT 1 WHERE " + expression;
    }

    protected override bool IsDuplicateKeyException(System.Exception ex) {
      if(ex is SqlException && 
         ((SqlException)ex).ErrorCode == 5) {
        return true;
      }

      return false;
    }
  }
}
