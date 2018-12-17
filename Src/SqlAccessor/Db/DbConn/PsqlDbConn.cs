using System;
using System.Data.Common;
//using Pervasive.Data.SqlClient;
using System.Diagnostics;

namespace SqlAccessor
{
  /// <summary>
  /// Pervasive PSQLへの接続を表す
  /// </summary>
  /// <remarks></remarks>
  internal class PsqlDbConn: DbConn
  {
    public PsqlDbConn(string connStr
                    , IResultsCache aResultsCache
                    , bool commitAtFinalizing
                    , bool debugPrint
                    , ILogger logger)
      : base(connStr, aResultsCache, commitAtFinalizing, debugPrint, logger) {
    }

    protected override System.Data.Common.DbConnection CreateDbConnection() {
      // return new PsqlConnection();
      throw new NotImplementedException();
    }

    protected override DbCommand CreateDbCommand(string sql
                                                , DbConnection aDbConnection
                                                , DbTransaction aDbTransaction) {
      //_debugPrint = trueの場合にSQL文発行ログを出力する
      if(_debugPrint) {
        _logger.Print(sql);
      }

      //第2,3引数にはPsqlDbConn.CreateDbConnection()の戻り値、及びその戻り値から生成されるPsqlTransaction型オブジェクトが
      //渡されるので、不正な型のオブジェクトが渡されることはない。
      // return new PsqlCommand(sql, (PsqlConnection)aDbConnection, (PsqlTransaction)aDbTransaction);

      //接続TimeOut値をここで設定する。
      //(接続文字列でも指定できるが最新のパッチを適用したPervasiveでないと機能しない)
      //var aPsqlCommand = new PsqlCommand(sql, (PsqlConnection)aDbConnection, (PsqlTransaction)aDbTransaction);
      //aPsqlCommand.CommandTimeout = 1140;
      //return aPsqlCommand;

      throw new NotImplementedException();
    }

    protected override DataAdapter CreateDataAdapter(DbCommand aDbCommand) {
      //引数にはPsqlDbConn.CreateDbCommandの戻り値が渡されるので、不正な型のオブジェクトが渡されることはない。
      // return new PsqlDataAdapter((PsqlCommand)aDbCommand);
      throw new NotImplementedException();
    }

    protected override string MakeExpEvalSql(string expression) {
      string sql = "";
      sql += "SELECT 1 FROM (SELECT 1) DUAL_" + Environment.NewLine;
      sql += "WHERE " + expression;
      return sql;
    }

    protected override bool IsDuplicateKeyException(System.Exception ex) {
      //if(ex.GetType() == PsqlException && ((PsqlException)ex).Number == -4994) {
      //  return true;
      //}

      return false;
    }
  }
}
