using System;
using System.Reflection;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using MiniSqlParser;

namespace SqlAccessor
{
  /// <summary>
  /// データベースから抽出したレコードのコレクションを表す
  /// </summary>
  /// <typeparam name="TRecord"></typeparam>
  /// <remarks></remarks>
  internal class RecordReaderImp<TRecord>: Disposable, IRecordReaderImp<TRecord> 
  where TRecord: class, IRecord, new()
  {
    private IResults _aResults;
    private readonly ViewInfo _aViewInfo;
    private readonly ICaster _aCaster;
    private readonly SqlBuilder.DbmsType _aDbms;
    private readonly Tran _aTran;
    //レコードのメタ情報
    private readonly string _recordName;
    private readonly PropertyInfo[] _properties;

    public RecordReaderImp(IResults aResults
                         , ViewInfo aViewInfo
                         , ICaster aCaster
                         , SqlBuilder.DbmsType aDbms
                         , Tran aTran) {
      _aResults = aResults;
      _aViewInfo = aViewInfo;
      _aCaster = aCaster;
      _aDbms = aDbms;
      _aTran = aTran;
      //レコードのメタ情報の取得
      System.Type recordType = (new TRecord()).GetType();
      _recordName = recordType.Name;
      _properties = recordType.GetProperties();
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

    #region IEnumerator メンバー

    object IEnumerator.Current {
      get { return this.Current; }
    }

    #endregion

    #region IEnumerable メンバー

    IEnumerator IEnumerable.GetEnumerator() {
      return this.GetEnumerator();
    }

    #endregion

    public TRecord Current {
      get {
        if(_aResults.IsDisposed()) {
          throw new InvalidOperationException(
            "トランザクション中断後または終了後に、RecordReaderオブジェクトからレコードを読むことはできません");
        }

        //返値となるレコードを生成する
        TRecord aRecord = new TRecord();

        foreach(PropertyInfo aProperty in _properties) {
          if(!aProperty.CanWrite) {
            //値を格納できないプロパティは除外する
            continue;
          }

          //マッパーオブジェクトを利用してRecordのProperty名から対応するViewのColumn名を取得する
          ViewColumnInfo aViewColumnInfo = _aViewInfo[aProperty.Name];

          if(aViewColumnInfo == null) {
            //Property名に対応するColumn名が存在しない場合
            continue;
          }

          //指定したViewColumnの値を取得する
          object viewColumnValue = null;
          try {
            viewColumnValue = _aResults.GetValueOf(aViewColumnInfo.ColumnPos);
          } catch(Exception ex) {
            _aTran.Rollback();
            throw new DbAccessException("SELECT文の結果の読込みに失敗しました", ex);
          }

          //ViewColumnの型情報を登録する
          //(SQLiteは行ごとに異なるデータ型を格納できるので、行ごとに型情報を登録し直す)
          if(!aViewColumnInfo.HasHostDataType || _aDbms == SqlBuilder.DbmsType.Sqlite) {
            aViewColumnInfo.SetTypeInfo(_aResults.GetDataType(aViewColumnInfo.ColumnPos)
                                      , _aResults.GetDbColumnTypeName(aViewColumnInfo.ColumnPos));
          }

          //ViewColumnの値をレコードのプロパティに格納する
          //
          //'第3引数のColumnInfoはどうやって取ってくるか考え中。
          //'暫定的にNothingにする。
          aProperty.SetValue(aRecord, _aCaster.CastToPropertyType(viewColumnValue, aViewColumnInfo, aProperty.PropertyType), null);
        }

        return aRecord;
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

    public IEnumerator<TRecord> GetEnumerator() {
      return this;
    }

    public bool Writable {
      get { return true; }
    }

    public object GetValueOf(string columnName) {
      try {
        return _aResults.GetValueOf(columnName);
      } catch(Exception ex) {
        _aTran.Rollback();
        throw new DbAccessException("SELECT文の結果の読込みに失敗しました.", ex);
      }
    }

    public object GetValueOf(int columnPos) {
      try {
        return _aResults.GetValueOf(columnPos);
      } catch(Exception ex) {
        _aTran.Rollback();
        throw new DbAccessException("SELECT文の結果の読込みに失敗しました..", ex);
      }
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
