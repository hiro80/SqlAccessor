using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace SqlAccessor
{
  /// <summary>
  /// イテレート終了時にトランザクションを終了するRecordReader
  /// </summary>
  /// <typeparam name="TRecord"></typeparam>
  /// <remarks>RecordReaderにトランザクションの終了処理を加えるDecorator</remarks>
  internal class CommitAtEndRecordReader<TRecord>: Disposable, IRecordReader<TRecord> 
  where TRecord: class, IRecord, new()
  {
    private IRecordReader<TRecord> _aRecordReader;
    private Tran _aTran;

    public CommitAtEndRecordReader(IRecordReader<TRecord> aRecordReader, Tran aTran) {
      _aRecordReader = aRecordReader;
      _aTran = aTran;
    }

    protected override void DisposeImp(bool disposing) {
      //RecordReaderオブジェクトの破棄
      try {
        if(_aRecordReader != null) {
          _aRecordReader.Dispose();
          _aRecordReader = null;
        }
      } catch(Exception ex) {
        //GCによる回収時には例外を送出しない
        if(disposing) {
          throw;
        }
      }

      //Tranオブジェクトの破棄
      try {
        if(_aTran != null) {
          _aTran.Dispose();
          _aTran = null;
        }
      } catch(Exception ex) {
        //GCによる回収時には例外を送出しない
        if(disposing) {
          throw;
        }
      }
    }

    object IEnumerator.Current {
      get { return this.Current; }
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return this.GetEnumerator();
    }

    public TRecord Current {
      get { return _aRecordReader.Current; }
    }

    public bool MoveNext() {
      try {
        bool nextExists = _aRecordReader.MoveNext();
        //最後までイテレートされた時点でトランザクションを終了する
        if(!nextExists) {
          this.Dispose();
        }
        return nextExists;
      } catch {
        return false;
      }
    }

    public void Reset() {
      _aRecordReader.Reset();
    }

    public IEnumerator<TRecord> GetEnumerator() {
      return this;
    }

    public bool Writable {
      get { return _aRecordReader.Writable; }
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

  }
}
