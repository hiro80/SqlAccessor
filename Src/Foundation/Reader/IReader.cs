using System.Collections;
using System.ComponentModel;

namespace SqlAccessor
{
  /// <summary>
  /// コレクションとイテレータの両方のインタフェースを併せ持つインタフェース
  /// </summary>
  /// <remarks></remarks>
  public interface IReader: IEnumerator, IEnumerable, IListSource, System.IDisposable
  {
    //IDisposableを継承したIEnumerableは、
    //foreach文から抜けるときに自動的にdispose()が実行される
  }
}
