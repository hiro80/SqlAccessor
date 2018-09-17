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
      //'    Return New PsqlConnection()
      throw new NotImplementedException();
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
      //Return New PsqlCommand(sql, DirectCast(aDbConnection, PsqlConnection), DirectCast(aDbTransaction, PsqlTransaction))

      //接続TimeOut値をここで設定する。
      //(接続文字列でも指定できるが最新のパッチを適用したPervasiveでないと機能しない)
      //'    Dim aPsqlCommand As PsqlCommand _
      //'      = New PsqlCommand(sql, DirectCast(aDbConnection, PsqlConnection), DirectCast(aDbTransaction, PsqlTransaction))
      //'    aPsqlCommand.CommandTimeout = 1140

      //'    Return aPsqlCommand
      throw new NotImplementedException();
    }

    protected override DataAdapter CreateDataAdapter(DbCommand aDbCommand) {
      //引数にはPsqlDbConn.CreateDbCommandの戻り値が渡されるので、不正な型のオブジェクトが渡されることはない。
      //'    Return New PsqlDataAdapter(DirectCast(aDbCommand, PsqlCommand))
      throw new NotImplementedException();
    }

    protected override string MakeExpEvalSql(string expression) {
      string sql = "";
      sql += "SELECT 1 FROM (SELECT 1) DUAL_" + Environment.NewLine;
      sql += "WHERE " + expression;
      return sql;
    }

    protected override bool IsDuplicateKeyException(System.Exception ex) {
      //'    If TypeOf ex Is PsqlException _
      //'       AndAlso DirectCast(ex, PsqlException).Number = -4994 Then
      //'    Return True
      //'    End If

      return false;
    }
  }
}
