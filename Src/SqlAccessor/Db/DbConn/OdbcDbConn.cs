using System.Data.Common;
using System.Data.Odbc;
using System.Diagnostics;

namespace SqlAccessor
{
  /// <summary>
  /// ODBC接続を表す
  /// </summary>
  /// <remarks></remarks>
  internal class OdbcDbConn: DbConn
  {
    public OdbcDbConn(string connStr
                    , IResultsCache aResultsCache
                    , bool commitAtFinalizing
                    , bool debugPrint
                    , ILogger logger)
      : base(connStr, aResultsCache, commitAtFinalizing, debugPrint, logger) {
    }

    protected override System.Data.Common.DbConnection CreateDbConnection() {
      return new OdbcConnection();
    }

    protected override DbCommand CreateDbCommand(string sql
                                                , DbConnection aDbConnection
                                                , DbTransaction aDbTransaction) {
      //_debugPrint = Trueの場合にSQL文発行ログを出力する
      if(_debugPrint) {
        _logger.Print(sql);
      }

      //第2,3引数にはPsqlDbConn.CreateDbConnection()の戻り値、及びその戻り値から生成されるPsqlTransaction型オブジェクトが
      //渡されるので、不正な型のオブジェクトが渡されることはない。
      return new OdbcCommand(sql, (OdbcConnection)aDbConnection, (OdbcTransaction)aDbTransaction);
    }

    protected override DataAdapter CreateDataAdapter(DbCommand aDbCommand) {
      //引数にはPsqlDbConn.CreateDbCommandの戻り値が渡されるので、不正な型のオブジェクトが渡されることはない。
      return new OdbcDataAdapter((OdbcCommand)aDbCommand);
    }

    protected override string MakeExpEvalSql(string expression) {
      throw new System.NotImplementedException();
    }

    protected override bool IsDuplicateKeyException(System.Exception ex) {
      //(未実装)
      throw new System.NotImplementedException();
    }
  }
}
