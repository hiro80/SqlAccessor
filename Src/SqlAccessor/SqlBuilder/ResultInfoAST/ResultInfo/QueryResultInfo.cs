using System;

namespace MiniSqlParser
{
  public class QueryResultInfo: AbstractSingleQueryResultInfo
  {
    /// <summary>
    /// Query等の別名
    /// </summary>
    public override string TableAliasName { get; internal set; }

    /// <summary>
    /// QueryのSELECTがColumn型の場合はその名称を格納する
    /// </summary>
    public string ColumnName { get; private set; }

    private string _columnAliasName;
    /// <summary>
    /// QueryのSELECT句での別名
    /// </summary>
    public override string ColumnAliasName {
      get {
        // AS別名が指定されていない場合、列名をAS列名と見做す
        if(string.IsNullOrEmpty(_columnAliasName)) {
          return this.ColumnName;
        } else {
          return _columnAliasName;
        }
      }
      internal set {
        _columnAliasName = value;
      }
    }

    /// <summary>
    /// 単独または他項目との組合せで、Tableの結果を一意に特定できる場合: True
    /// </summary>
    //public override bool IsPrimaryKey {
    //  get {
    //    //return this.SourceInfo != null ?
    //    //           this.SourceInfo.IsPrimaryKey
    //    //        : !this.IsNullable && this.IsUnique;
    //    return !this.IsNullable && this.IsUnique;
    //  }
    //}
    public override KeyType KeyType { get; internal set; }

    public override bool IsNullable { get; internal set; }
    public override bool IsUnique { get; internal set; }
    public override bool ExplicitDecl { get; internal set; }
    public override bool IsComplemented { get; internal set; }

    public override SqlTable SourceTable { get; internal set; }
    public override string SourceColumnName { get; internal set; }
    public override ResultColumn Node { get; protected set; }
    internal override IResultInfoInternal SourceInfo { get; set; }
    public override ResultInfoList SourceInfoList { get; protected set; }
    public override int SourceInfoListIndex { get; protected set; }

    public override bool IsNullLiteral {
      get {
        if(this.Node == null || this.Node.IsTableWildcard) {
          return this.SourceInfo != null && this.SourceInfo.IsNullLiteral;
        }
        // 指定したSELECT句が括弧で包まれている場合は、括弧を剥く
        var exprValue = ((ResultExpr)this.Node).Value;
        while(exprValue.GetType() == typeof(BracketedExpr)) {
          exprValue = ((BracketedExpr)exprValue).Operand;
        }
        return exprValue.GetType() == typeof(NullLiteral);
      }
    }

    public override ResultInfoType Type {
      get {
        return ResultInfoType.Query;
      }
    }

    internal override bool IsReferenced { get; set; }
    internal override bool IsOuterJoined { get; set; }

    private Expr GetResultExprValue() {
      if(this.Node == null || this.Node.IsTableWildcard) {
        return null;
      }
      var exprValue = ((ResultExpr)this.Node).Value;
      // 指定したSELECT句が括弧で包まれている場合は、括弧を剥く
      while(exprValue.GetType() == typeof(BracketedExpr)) {
        exprValue = ((BracketedExpr)exprValue).Operand;
      }
      return exprValue;
    }

    // 集約関数の場合、Trueを返す
    internal bool IsAggregative() {
      if(this.Node == null || this.Node.IsTableWildcard) {
        return false;
      }
      var exprValue = ((ResultExpr)this.Node).Value;
      var visitor = new FindAggregateExprVisitor();
      exprValue.Accept(visitor);
      return visitor.ContainsAggregativeExpr;
    }

    // NULLを返さない集約関数の場合、Trueを返す
    internal bool IsNotNullAggregateFunc() {
      var exprValue = this.GetResultExprValue();
      if(exprValue == null || exprValue.GetType() != typeof(AggregateFuncExpr)) {
        return false;
      }

      var aggregateFuncName = ((AggregateFuncExpr)exprValue).Name.ToUpper();
      return aggregateFuncName == "COUNT" ||
             aggregateFuncName == "TOTAL" ||
             aggregateFuncName == "COUNT_BIG";
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
      var ret = new QueryResultInfo(this.TableAliasName
                                  , this.ColumnName
                                  , this.ColumnAliasName
                                  , this.IsNullable
                                  //, this.IsUnique
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

    internal QueryResultInfo(string tableAliasName
                            , string columnName
                            , string columnAliasName
                            , bool isNullable
                            , KeyType keyType
                            , bool explicitDecl
                            , bool isComplemented
                            , ResultColumn node)
      : this(tableAliasName
            , columnName
            , columnAliasName
            , isNullable
            , keyType
            , explicitDecl
            , isComplemented
            , null
            , null
            , node
            , null
            , -1) {
    }

    internal QueryResultInfo(string tableAliasName
                            , string columnName
                            , string columnAliasName
                            , bool isNullable
                            //, bool isUnique
                            , KeyType keyType
                            , bool explicitDecl
                            , bool isComplemented
                            , SqlTable sourceTable
                            , string sourceColumnName
                            , ResultColumn node
                            , ResultInfoList sourceInfoList
                            , int sourceInfoListIndex) {
      this.TableAliasName = tableAliasName;
      this.ColumnName = columnName;
      this.ColumnAliasName = columnAliasName;
      this.IsNullable = isNullable;
      //this.IsUnique = isUnique;
      this.KeyType = keyType;
      this.ExplicitDecl = explicitDecl;
      this.IsComplemented = isComplemented;
      this.SourceTable = sourceTable;
      this.SourceColumnName = sourceColumnName;
      this.Node = node;
      this.SourceInfoList = sourceInfoList;
      this.SourceInfoListIndex = sourceInfoListIndex;
      if(sourceInfoList != null && sourceInfoListIndex >= 0) {
        this.SourceInfo = sourceInfoList[sourceInfoListIndex];
      }
    }

  }

}
