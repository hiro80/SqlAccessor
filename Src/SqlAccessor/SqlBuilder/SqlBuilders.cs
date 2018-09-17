using System.Collections.Generic;
using MiniSqlParser;

namespace MiniSqlParser
{
  public class SqlBuilders : IEnumerable<SqlBuilder>
  {
    private readonly List<SqlBuilder> _sqls = new List<SqlBuilder>();

    public SqlBuilders() {
    }

    public SqlBuilders(IEnumerable<SqlBuilder> sqls) {
      _sqls.AddRange(sqls);
    }

    public SqlBuilders(string sqlsStr
                     , SqlBuilder.DbmsType dbmsType = SqlBuilder.DbmsType.Unknown
                     , bool forSqlAccessor = true)
      : this(MiniSqlParserAST.CreateStmts(sqlsStr
                                     , SqlBuilder.ConvertDbmsType(dbmsType)
                                     , forSqlAccessor)) {
    }

    internal SqlBuilders(Stmts stmts) {
      foreach(var stmt in stmts) {
        _sqls.Add(new SqlBuilder(stmt));
      }
    }

    public IEnumerator<SqlBuilder> GetEnumerator() {
      return _sqls.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
      return _sqls.GetEnumerator();
    }

    public int Count {
      get {
        return _sqls.Count;
      }
    }

    public SqlBuilder this[int i] {
      get {
        return _sqls[i];
      }
      internal set {
        _sqls[i] = value;
      }
    }
  }
}
