using System;

namespace MiniSqlParser
{
  abstract public class ResultInfoVisitor: IResultInfoVisitor
  {
    virtual public void VisitBefore(TableResultInfo resultInfo) { }

    virtual public void VisitAfter(TableResultInfo resultInfo) { }

    virtual public void VisitBefore(QueryResultInfo resultInfo) { }

    virtual public void VisitAfter(QueryResultInfo resultInfo) { }

    virtual public void VisitBefore(CountQueryResultInfo resultInfo) { }

    virtual public void VisitAfter(CountQueryResultInfo resultInfo) { }

    virtual public void VisitBefore(CompoundQueryResultInfo resultInfo) { }

    virtual public void VisitAfter(CompoundQueryResultInfo resultInfo) { }

    virtual public void VisitBefore(ResultInfoList resultInfoList) { }

    virtual public void VisitAfter(ResultInfoList resultInfoList) { }
  }
}
