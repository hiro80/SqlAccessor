using System;

namespace MiniSqlParser
{
  public abstract class AbstractSingleQueryResultInfo : AbstractQueryResultInfo
  {
    public abstract ResultColumn Node { get; protected set; }
    internal abstract IResultInfoInternal SourceInfo { get; set; }
    public abstract ResultInfoList SourceInfoList { get; protected set; }
    public abstract int SourceInfoListIndex { get; protected set; }
  }
}
