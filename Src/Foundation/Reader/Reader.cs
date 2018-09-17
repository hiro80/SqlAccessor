using System.Collections.Generic;

namespace SqlAccessor
{
  /// <summary>
  /// コレクションとイテレータの両方のインタフェースを併せ持つインタフェース
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <remarks>IReaderのジェネリック版</remarks>
  public interface Reader<T>: IReader, IEnumerator<T>, IEnumerable<T>
  {
  }
}
