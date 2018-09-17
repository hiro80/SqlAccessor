using System;

namespace MiniSqlParser
{
  public class CompoundQueryResultInfo: AbstractQueryResultInfo
  {
    public override string TableAliasName { get; internal set; }

    public override string ColumnAliasName { get; internal set; }

    //public override bool IsPrimaryKey {
    //  get {
    //    if(this.LeftResultInfo == null || this.RightResultInfo == null) {
    //      return false;
    //    }
    //    // UNIONの片方のSELECT句が主キーで、もう片方がNULLリテラルの場合のみ
    //    // 主キー判定が可能である
    //    return this.LeftResultInfo.IsPrimaryKey && this.RightResultInfo.IsNullLiteral ||
    //           this.RightResultInfo.IsPrimaryKey && this.LeftResultInfo.IsNullLiteral;
    //  }
    //}
    public override KeyType KeyType { get; internal set; }

    public override bool IsNullable { get; internal set; }

    public override bool IsNullLiteral {
      get {
        return this.LeftResultInfo.IsNullLiteral && this.RightResultInfo.IsNullLiteral;
      }
    }

    public override bool IsUnique { get; internal set; }

    // Compound QueryにはImplicitなSELECT句は含められない
    public override bool ExplicitDecl {
      get {
        return true;
      }
      internal set {
        if(!value) {
          throw new ArgumentException(
            "CompoundQueryResultInfo.ExplicitDecl is not allowed to set false", "ExplicitDecl");
        }
      }
    }

    public override bool IsComplemented { get; internal set; }

    public override SqlTable SourceTable { get; internal set; }
    public override string SourceColumnName { get; internal set; }

    public override ResultInfoType Type {
      get {
        return ResultInfoType.Compound;
      }
    }

    public AbstractQueryResultInfo LeftResultInfo { get; internal set; }

    public AbstractQueryResultInfo RightResultInfo { get; internal set; }

    internal override bool IsReferenced { get; set; }
    internal override bool IsOuterJoined { get; set; }

    //public override bool IsDirectSource(Column destItem, bool ignoreCase = true) {
    //  // UNION ALLの左の被演算クエリ内で同じ列名が使用されている場合、Trueを返す
    //  return this.LeftResultInfo.IsDirectSource(destItem, ignoreCase);
    //}


    public override void Accept(IResultInfoVisitor visitor) {
      if(visitor == null) {
        throw new ArgumentNullException("visitor");
      }
      visitor.VisitBefore(this);
      if(this.LeftResultInfo != null) {
        this.LeftResultInfo.Accept(visitor);
      }
      if(this.RightResultInfo != null) {
        this.RightResultInfo.Accept(visitor);
      }
      visitor.VisitAfter(this);
    }

    public override AbstractQueryResultInfo Clone() {
      var ret = new CompoundQueryResultInfo(this.TableAliasName
                                          , this.ColumnAliasName
                                          , this.IsNullable
                                          //, this.IsUnique
                                          , this.KeyType
                                          , this.IsComplemented
                                          , this.SourceTable
                                          , this.SourceColumnName
                                          , this.LeftResultInfo
                                          , this.RightResultInfo);
      ret.IsReferenced = this.IsReferenced;
      ret.IsOuterJoined = this.IsOuterJoined;
      return ret;
    }

    internal CompoundQueryResultInfo(string tableAliasName
                                    , string columnAliasName
                                    , bool isNullable
                                    //, bool isUnique
                                    , KeyType keyType
                                    , bool isComplemented
                                    , SqlTable sourceTable
                                    , string sourceColumnName
                                    , AbstractQueryResultInfo leftResultInfo
                                    , AbstractQueryResultInfo rightResultInfo) {
      this.TableAliasName = tableAliasName;
      this.ColumnAliasName = columnAliasName;
      this.IsNullable = isNullable;
      //this.IsUnique = isUnique;
      this.KeyType = keyType;
      this.IsComplemented = isComplemented;
      this.SourceTable = sourceTable;
      this.SourceColumnName = sourceColumnName;
      this.LeftResultInfo = leftResultInfo;
      this.RightResultInfo = rightResultInfo;
    }
  }

}
