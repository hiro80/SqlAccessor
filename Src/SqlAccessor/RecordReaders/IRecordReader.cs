namespace SqlAccessor
{
  /// <summary>
  /// データベースから抽出したレコードのコレクションを表す
  /// </summary>
  /// <typeparam name="TRecord"></typeparam>
  /// <remarks></remarks>
  public interface IRecordReader<TRecord>: Reader<TRecord>
  {
    bool Writable { get; }
  }
}
