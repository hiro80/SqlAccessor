using System;

namespace MiniSqlParser
{
  public class CountQueryResultInfo: AbstractSingleQueryResultInfo
  {
    public override string TableAliasName { get; internal set; }

    public override string ColumnAliasName { get; internal set; }

    public override KeyType KeyType { get; internal set; }

    public override bool IsNullable {
      get {
        return false;
      }
      internal set{
        if(value) {
          throw new ArgumentException("CountQueryResultInfo.IsNullableはtrueにできません ");
        }
      }
    }

    public override bool IsUnique { get; internal set; }

    public override bool ExplicitDecl { get; internal set; }

    public override bool IsComplemented { get; internal set; }

    public override SqlTable SourceTable { get; internal set; }
    public override string SourceColumnName { get; internal set; }
    public override ResultColumn Node { get; protected set; }
    internal override IResultInfoInternal SourceInfo { get; set; }
    public override ResultInfoList SourceInfoList { get; protected set; }
    public override int SourceInfoListIndex { get; protected set; }

    public override ResultInfoType Type {
      get {
        return ResultInfoType.Count;
      }
    }

    internal override bool IsReferenced { get; set; }
    internal override bool IsOuterJoined { get; set; }

    public override bool IsNullLiteral {
      get {
        return false;
      }
    }

    public override void Accept(IResultInfoVisitor visitor) {
      if(visitor == null) {
        throw new ArgumentNullException("visitor");
      }
      visitor.VisitBefore(this);
      if(this.SourceInfo != null) {
        this.SourceInfo.Accept(visitor);
      }
      visitor.VisitAfter(this);
    }

    public override AbstractQueryResultInfo Clone() {
      var node = this.Node == null ? null : this.Node.Clone();
      var ret = new CountQueryResultInfo(this.TableAliasName
                                          , this.ColumnAliasName
                                          , this.KeyType
                                          , this.ExplicitDecl
                                          , this.IsComplemented
                                          , this.SourceTable
                                          , this.SourceColumnName
                                          , node
                                          , this.SourceInfoList
                                          , this.SourceInfoListIndex);
      ret.IsReferenced = this.IsReferenced;
      ret.IsOuterJoined = this.IsOuterJoined;
      return ret;
    }

    internal CountQueryResultInfo(string tableAliasName
                                , string columnAliasName
                                , KeyType keyType
                                , bool explicitDecl
                                , bool isComplemented
                                , ResultColumn node)
      : this(tableAliasName
            , columnAliasName
            , keyType
            , explicitDecl
            , isComplemented
            , null
            , null
            , node
            , null
            , -1) {
    }

    internal CountQueryResultInfo(string tableAliasName
                                , string columnAliasName
                                , KeyType keyType
                                , bool explicitDecl
                                , bool isComplemented
                                , SqlTable sourceTable
                                , string SourceColumnName
                                , ResultColumn node
                                , ResultInfoList sourceInfoList
                                , int sourceInfoListIndex) {
      this.TableAliasName = tableAliasName;
      this.ColumnAliasName = columnAliasName;
      this.KeyType = keyType;
      this.ExplicitDecl = explicitDecl;
      this.IsComplemented = isComplemented;
      this.SourceTable = sourceTable;
      this.SourceColumnName = SourceColumnName;
      this.Node = node;
      this.SourceInfoList = sourceInfoList;
      this.SourceInfoListIndex = sourceInfoListIndex;
      if(sourceInfoList != null && sourceInfoListIndex >= 0) {
        this.SourceInfo = sourceInfoList[sourceInfoListIndex];
      }
    }
  }

}
