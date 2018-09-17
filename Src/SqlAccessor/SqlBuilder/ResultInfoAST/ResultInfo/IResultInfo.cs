
namespace MiniSqlParser
{
  /// <summary>
  /// DLL外部へ公開するResultInfoインターフェース
  /// </summary>
  public interface IResultInfo
  {
    string TableAliasName { get; }
    string ColumnAliasName { get; }
    /// <summary>
    /// Not NullかつUniqueな列はTrue
    /// </summary>
    KeyType KeyType { get; }
    /// <summary>
    /// Nullを返す可能性がある列はTrue
    /// </summary>
    bool IsNullable { get; }
    /// <summary>
    /// NullリテラルはTrue
    /// </summary>
    bool IsNullLiteral { get; }
    /// <summary>
    /// Unique列またはUniqueを構成する列はTrue
    /// </summary>
    bool IsUnique { get; }
    /// <summary>
    /// SELECT句で明示されている列はTrue
    /// </summary>
    bool ExplicitDecl { get; }
    /// <summary>
    /// 主キー補完機能により追加された列はTrue
    /// </summary>
    bool IsComplemented { get; }

    ResultInfoType Type { get; }

    SqlTable SourceTable { get; }

    string SourceColumnName { get; }

    void Accept(IResultInfoVisitor visitor);
  }

  /// <summary>
  /// DLL内部で用いるResultInfoインターフェース
  /// </summary>
  internal interface IResultInfoInternal: IResultInfo
  {
    bool IsDirectSource(Column destItem, bool ignoreCase = true);
  }
}