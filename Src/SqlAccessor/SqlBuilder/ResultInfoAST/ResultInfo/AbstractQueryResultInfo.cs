
namespace MiniSqlParser
{
  [System.Diagnostics.DebuggerDisplay(
    "TableAliasName: {TableAliasName}, ColumnAliasName: {ColumnAliasName}")]
  public abstract class AbstractQueryResultInfo: IResultInfo, IResultInfoInternal
  {
    public abstract string TableAliasName { get; internal set; }
    public abstract string ColumnAliasName { get; internal set; }
    //public abstract bool IsPrimaryKey { get; }
    public abstract KeyType KeyType { get; internal set; }
    public abstract bool IsNullable { get; internal set; }
    public abstract bool IsNullLiteral { get; }
    public abstract bool IsUnique { get; internal set; }
    public abstract bool ExplicitDecl { get; internal set; }
    public abstract bool IsComplemented { get; internal set; }
    public abstract SqlTable SourceTable { get; internal set; }
    public abstract string SourceColumnName { get; internal set; }
    public abstract ResultInfoType Type { get; }

    // CompoundQueryResultInfoのLeftSouorce/RightSourceに格納されたQueryResultInfoは、
    // そのTableAliasNameに値は設定されないので、そのIsDirectSource()は正しい結果を返さない
    // そのためCompoundQueryResultInfo.IsDirectSource()の実装をLeftSouorce/RightSourceに
    // 格納されたQueryResultInfoに委譲するような実装をすると正しい結果を返さない
    public bool IsDirectSource(Column destItem, bool ignoreCase = true) {
      if(string.Compare(destItem.Name, this.ColumnAliasName, ignoreCase) != 0) {
        return false;
      } else if(string.IsNullOrEmpty(destItem.TableAliasName)) {
        // SELECT句でテーブル名が省略されている場合、trueを返す
        return true;
      } else if(string.IsNullOrEmpty(this.TableAliasName)) {
        // SELECT句でテーブル名が指定され、抽出元Queryの別名が省略されている場合、falseを返す
        return false;
      } else if(string.Compare(destItem.TableAliasName, this.TableAliasName, ignoreCase) != 0) {
        // テーブル別名が指定されている場合はテーブル別名を参照する
        return false;
      } else {
        // SELECT句とQueryがテーブル別名を指定され、全て一致する場合、tureを返す
        return true;
      }
    }

    public abstract void Accept(IResultInfoVisitor visitor);
    public abstract AbstractQueryResultInfo Clone();
    internal abstract bool IsReferenced { get; set; }
    internal abstract bool IsOuterJoined { get; set; }
  }

}
