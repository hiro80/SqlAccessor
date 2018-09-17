namespace SqlAccessor
{
  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="TRecord"></typeparam>
  /// <remarks></remarks>
  internal interface IRecordReaderImp<TRecord>: IRecordReader<TRecord>
  {
    object GetValueOf(string columnName);
    object GetValueOf(int columnPos);
  }
}
