using System;
using System.Collections.Generic;
using MiniSqlParser;

namespace SqlAccessor
{
  public partial class Tran
  {
    /// <summary>
    /// SQL文を作成する機能を纏めたクラス
    /// </summary>
    /// <remarks></remarks>
    private class SqlMaker
    {
      private readonly RecordViewTableMapFactory _aRecordViewTableMapFactory;
      private readonly SqlPodFactory _aSqlPodFactory;

      public SqlMaker(RecordViewTableMapFactory aRecordViewTableMapFactory
                    , SqlPodFactory aSqlPodFactory) {
        _aRecordViewTableMapFactory = aRecordViewTableMapFactory;
        _aSqlPodFactory = aSqlPodFactory;
      }

      public SqlBuilder GetSelectSql<TRecord>(Query<TRecord> query
                                            , LoadMode aLoadMode)
      where TRecord: class, IRecord, new() {
        //TRecord型レコードとViewのマッピング情報を取得する
        RecordViewTableMap<TRecord> aRecordViewMap = _aRecordViewTableMapFactory.CreateRecordViewTableMap<TRecord>();
        //TRecordに対応するSqlPodを生成する
        SqlPod aSqlPod = _aSqlPodFactory.CreateSqlPod<TRecord>();
        //aQueryが持つ条件のプロパティ型リテラル値をDBデータ型に型変換する
        query = aRecordViewMap.CastQuery(query);

        //SQL文を作成する
        //引数aLoadModeとaRecordTableMapを追加したのは苦渋の決断
        //(SqlBuilderがテーブルのメタ情報を扱うことを想定していなかったので、後付による拡張が綺麗にならない)
        RecordViewTableMap<TRecord> aRecordViewTableMap = _aRecordViewTableMapFactory.CreateRecordViewTableMap<TRecord>();
        return aSqlPod.GetSelectSql(query, aRecordViewTableMap);
      }

      public SqlBuilder GetCountSql<TRecord>(IQuery aIQuery)
      where TRecord: class, IRecord, new() {
        //aQueryのDownキャスト
        Query<TRecord> aQuery = (Query<TRecord>)aIQuery;
        //TRecord型レコードとViewのマッピング情報を取得する
        RecordViewTableMap<TRecord> aRecordViewMap = _aRecordViewTableMapFactory.CreateRecordViewTableMap<TRecord>();
        //TRecordに対応するSqlPodを生成する
        SqlPod aSqlPod = _aSqlPodFactory.CreateSqlPod<TRecord>();
        //aQueryが持つ条件のプロパティ型リテラル値をDBデータ型に型変換する
        aQuery = aRecordViewMap.CastQuery(aQuery);

        //SQL文を作成する
        RecordViewTableMap<TRecord> aRecordViewTableMap = _aRecordViewTableMapFactory.CreateRecordViewTableMap<TRecord>();
        return aSqlPod.GetCountSql(aQuery, aRecordViewTableMap);
      }
    }

  }
}
