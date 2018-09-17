
namespace MiniSqlParser
{
  public interface IResultInfoVisitor
  {
    void VisitBefore(TableResultInfo resultInfo);
    void VisitAfter(TableResultInfo resultInfo);
    void VisitBefore(QueryResultInfo resultInfo);
    void VisitAfter(QueryResultInfo resultInfo);
    void VisitBefore(CountQueryResultInfo resultInfo);
    void VisitAfter(CountQueryResultInfo resultInfo);
    void VisitBefore(CompoundQueryResultInfo resultInfo);
    void VisitAfter(CompoundQueryResultInfo resultInfo);
    void VisitBefore(ResultInfoList resultInfoList);
    void VisitAfter(ResultInfoList resultInfoList);
  }
}
