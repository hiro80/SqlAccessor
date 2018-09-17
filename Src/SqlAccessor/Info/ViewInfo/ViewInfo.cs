using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using MiniSqlParser;

namespace SqlAccessor
{
  /// <summary>
  /// Viewのメタ情報を表す
  /// </summary>
  /// <remarks>スレッドセーフ</remarks>
  [System.Diagnostics.DebuggerDisplay("TableName:{_viewName}")]
  internal class ViewInfo
  {
    //Viewオブジェクトを集約する
    //(Viewカラム名(大文字)、ViewColumnInfo)
    private readonly Dictionary<string, ViewColumnInfo> _viewColumnInfoDic = new Dictionary<string, ViewColumnInfo>();
    private readonly TableInfoSet _tableInfoSet;
    private readonly Db _aDb;
    private readonly string _viewName;
    private readonly SqlPod _aSqlPod;

    public ViewInfo(string viewName
                  , SqlPod aSqlPod
                  , TableInfoSet tableInfoSet
                  , Db aDb) {
      _aDb = aDb;
      _viewName = viewName;
      _tableInfoSet = tableInfoSet;
      _aSqlPod = aSqlPod;

      //ViewColumnInfoを取得する
      this.GetAllViewColumnInfo();
    }

    private void GetAllViewColumnInfo() {
      //SqlPodから初期化用SELECT文を取得する
      SqlBuilder sql = _aSqlPod.GetInitSelectSql();

      //SELECT文から全てのSELECT項目を取得する
      List<string> selectItems = null;
      if(sql.HasWildcard() || sql.HasTableWildcard()) {
        //全ての抽出元テーブルの全ての列名をallSrcTableColumnsに格納する
        var allSrcTableColumns = new Dictionary<string, IEnumerable<Tuple<string, bool>>>();
        foreach(string srcTableName in sql.GetSrcTableNames()) {
          List<Tuple<string, bool>> listOfTableColumn = new List<Tuple<string, bool>>();
          foreach(ColumnInfo columnInfo in _tableInfoSet[srcTableName].GetAllColumns()) {
            listOfTableColumn.Add(Tuple.Create(columnInfo.ColumnName
                                             , columnInfo.PrimaryKey.HasValue && columnInfo.PrimaryKey.Value));
          }
          allSrcTableColumns.Add(srcTableName, listOfTableColumn);
        }
        //メインクエリのSELECT句にワイルドカードが存在する場合
        selectItems = sql.GetSelectItems(allSrcTableColumns);
      } else {
        //存在しない場合
        //(こちらの方が処理は速い)
        selectItems = sql.GetSelectItems();
      }

      int i = 0;
      foreach(string selectItem in selectItems) {
        //名称の一致は大文字小文字を区別すべきである!
        if(_viewColumnInfoDic.ContainsKey(selectItem.ToUpper())) {
          //同一名称のSELECT句が存在する場合、先に定義されたSELECT句のみを採用する
          i += 1;
          continue;
        }
        var aViewColumnInfo = new ViewColumnInfo(this, _viewName, selectItem, i);
        _viewColumnInfoDic.Add(selectItem.ToUpper(), aViewColumnInfo);
        i += 1;
      }

      if(sql.HasWildcard()) {
        return;
      }

    }

    public IEnumerable<ViewColumnInfo> Items() {
      return _viewColumnInfoDic.Values;
    }

    public ViewColumnInfo this[string viewColumnName] {
      get {
        if(!_viewColumnInfoDic.ContainsKey(viewColumnName.ToUpper())) {
          return null;
        }

        return _viewColumnInfoDic[viewColumnName.ToUpper()];
      }
    }

    public void RegistAllColumnsType() {
      Tran aTran = null;
      try {
        aTran = _aDb.CreateTranWithoutLock();
        aTran.RegistToViewMapping(this, _aSqlPod);
      } catch(Exception ex) {
        throw new CannotRegistToViewMappingException(
          "SELECT結果からレコードへのマッピングを取得できませんでした.", ex);
      } finally {
        if(aTran != null) {
          aTran.Dispose();
        }
      }
    }

    public string Name {
      get { return _viewName; }
    }
  }
}
