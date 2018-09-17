using System;
using System.ComponentModel;

namespace SqlAccessor
{
  /// <summary>
  /// ADO.NETから返されるViewカラムの値の型をViewColumnInfoに登録するReader
  /// </summary>
  /// <remarks></remarks>
  internal class RegistColumnTypeReader: Disposable, IReader
  {
    private IResults _aResults;
    private readonly ViewInfo _aViewInfo;
    private readonly Tran _aTran;

    public RegistColumnTypeReader(IResults aResults, ViewInfo aViewInfo, Tran aTran) {
      _aResults = aResults;
      _aViewInfo = aViewInfo;
      _aTran = aTran;
    }

    protected override void DisposeImp(bool disposing) {
      try {
        if(_aResults != null) {
          _aResults.Dispose();
          _aResults = null;
        }
      } catch(Exception ex) {
        //GCによる回収時には例外を送出しない
        if(disposing) {
          _aTran.Rollback();
          throw new DbAccessException("Resultsオブジェクトの破棄に失敗しました", ex);
        }
      }
    }

    public object Current {
      get {
        if(_aResults.IsDisposed()) {
          throw new InvalidOperationException(
            "トランザクション中断後または終了後に、RecordReaderオブジェクトからレコードを読むことはできません");
        }

        foreach(ViewColumnInfo aViewColumnInfo in _aViewInfo.Items()) {
          //ViewColumnの型情報を登録する
          if(!aViewColumnInfo.HasHostDataType) {
            aViewColumnInfo.SetTypeInfo(_aResults.GetDataType(aViewColumnInfo.ColumnPos)
                                      , _aResults.GetDbColumnTypeName(aViewColumnInfo.ColumnPos));
          }
        }

        return null;
      }
    }

    public bool MoveNext() {
      if(_aResults == null || _aResults.IsDisposed()) {
        throw new InvalidOperationException(
          "トランザクション中断後または終了後に、RecordReaderオブジェクトからレコードを読むことはできません");
      }

      bool nextExists = false;
      try {
        nextExists = _aResults.MoveNext();
      } catch(Exception ex) {
        _aTran.Rollback();
        throw new DbAccessException("SELECT文の結果の読込みに失敗しました.", ex);
      }

      //最後までイテレートされた時点で終了処理を行う
      if(!nextExists) {
        this.Dispose();
      }
      return nextExists;
    }

    public void Reset() {
    }

    public System.Collections.IEnumerator GetEnumerator() {
      return this;
    }

    public bool ContainsListCollection {
      get { return false; }
    }

    public System.Collections.IList GetList() {
      throw new NotSupportedException("RegistColumnTypeReaderはGetList()を実装していません");
    }
  }
}
