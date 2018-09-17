using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace SqlAccessor
{
  /// <summary>
  /// ロックをかけずにレコードを抽出する機能を、RecordReaderオブジェクトに付加する
  /// </summary>
  /// <typeparam name="TRecord"></typeparam>
  /// <remarks></remarks>
  internal class ReadOnlyRecordReader<TRecord>: RecordReader<TRecord>, IRecordReader<TRecord>
  where TRecord: class, IRecord, new()
  {
    private readonly IRecordReaderImp<TRecord> _aRecordReaderImp;

    public ReadOnlyRecordReader(IRecordReaderImp<TRecord> aRecordReaderImp) {
      _aRecordReaderImp = aRecordReaderImp;
    }

    public override void Dispose() {
      _aRecordReaderImp.Dispose();
    }

    public override TRecord Current {
      get { return _aRecordReaderImp.Current; }
    }

    public override bool MoveNext() {
      return _aRecordReaderImp.MoveNext();
    }

    public override void Reset() {
      _aRecordReaderImp.Reset();
    }

    public override IEnumerator<TRecord> GetEnumerator() {
      return this;
    }

    public override bool Writable {
      //ReadOnlyモードで抽出したレコードは全てReadOnlyである
      get { return false; }
    }

  }
}
