using System.Data.Common;
using Oracle.DataAccess.Client;
using System.Diagnostics;

namespace SqlAccessor
{
  /// <summary>
  /// Oracleへの接続を表す
  /// </summary>
  /// <remarks>Oracleクライアントがない環境では参照エラーになるのでコメントアウト</remarks>
  internal class OracleDbConn: DbConn
  {
    public OracleDbConn(string connStr
                      , IResultsCache aResultsCache
                      , bool commitAtFinalizing
                      , bool debugPrint
                      , ILogger logger)
      : base(connStr, aResultsCache, commitAtFinalizing, debugPrint, logger) {
    }

    protected override System.Data.Common.DbConnection CreateDbConnection() {
      return new OracleConnection();
    }

    protected override DbCommand CreateDbCommand(string sql
                                                , DbConnection aDbConnection
                                                , DbTransaction aDbTransaction) {
      //_debugPrint = Trueの場合にSQL文発行ログを出力する
      if(_debugPrint) {
        _logger.Print(sql);
      }

      //Oracleでは1つの接続に対しては1つのトランザクションしか対応できないので、
      //OracleCommandにOracleTransactionは設定できない
      return new OracleCommand(sql, (OracleConnection)aDbConnection);
    }

    protected override DataAdapter CreateDataAdapter(DbCommand aDbCommand) {
      //引数にはOracleDbConn.CreateDbCommandの戻り値が渡されるので、不正な型のオブジェクトが渡されることはない。
      return new OracleDataAdapter((OracleCommand)aDbCommand);
    }

    protected override string MakeExpEvalSql(string expression) {
      return "SELECT 1 FROM DUAL WHERE " + expression;
    }

    protected override bool IsDuplicateKeyException(System.Exception ex) {
      if(ex is OracleException && ((OracleException)ex).Number == 1) {
        return true;
      }

      return false;
    }

  }
}
