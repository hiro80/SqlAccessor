using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace SqlAccessor
{
  /// <summary>
  /// データベースから抽出したレコードのコレクションを表す
  /// </summary>
  /// <typeparam name="TRecord"></typeparam>
  /// <remarks>
  ///   IRecordReader(Of TRecord)にIListSourceの実装を付与したのが本クラスである。
  /// RecordReaderImpはDisposeを実装継承する必要があるため、IRecordReader(Of TRecord)と
  /// RecordReader(Of TRecord)の2つを定義し、RecordReaderImpはIRecordReader(Of TRecord)を
  /// 継承するようにした。
  ///   SqlAccessorを利用するユーザがIRecordReader(Of TRecord)のデコレータクラスを用意する
  /// 場合は、RecordReader(Of TRecord)を継承してIListSourceの実装を共有するのが望ましい
  /// </remarks>
  public abstract class RecordReader<TRecord>: IRecordReader<TRecord>
  {
    object IEnumerator.Current {
      get { return this.Current; }
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return this.GetEnumerator();
    }

    //GetList()が返すリストの要素もリストであればTrue、それ以外ではFalseを返す
    public bool ContainsListCollection {
      get { return false; }
    }

    public System.Collections.IList GetList() {
      //読み込み位置を最初にもどす
      //(DataTableはリセットできるがDataReaderはできない、
      // 挙動を合わせるためReset()は行わない)
      //this.Reset()

      //全ての要素をリストにコピーする
      List<TRecord> ret = new List<TRecord>();
      ret.AddRange(this);

      //読み取り専用ラッパーで包んで返す
      return ret.AsReadOnly();
    }

    public abstract void Dispose();
    public abstract TRecord Current { get; }
    public abstract bool MoveNext();
    public abstract void Reset();
    public abstract IEnumerator<TRecord> GetEnumerator();
    public abstract bool Writable { get; }
  }
}
