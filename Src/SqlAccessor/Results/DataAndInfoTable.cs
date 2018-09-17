using System.Data;
namespace SqlAccessor
{
  internal class DataAndInfoTable: DataTable
  {
    //DBMSのネイティブデータ型名
    private string[] _DbColumnTypeNames;
    //DBMSのネイティブデータ型に対応するホスト言語でのデータ型
    private System.Type[] _hostDataType;

    public DataAndInfoTable(IDataReader reader)
      : base() {
      //DataTable.Load()時にConstraintExceptionが送出されるのを防ぐ
      this.LoadFromReader(reader);
    }

    private void LoadFromReader(IDataReader reader) {
      //Viewの全ての列のデータ型情報を取得する
      _DbColumnTypeNames = new string[reader.FieldCount];
      _hostDataType = new System.Type[reader.FieldCount];
      for(int i = 0; i <= reader.FieldCount - 1; i++) {
        _DbColumnTypeNames[i] = reader.GetDataTypeName(i);
        _hostDataType[i] = reader.GetFieldType(i);
      }

      //Load()時にConstraintsExceptionの送出を抑止する
      this.DisableConstraints();
      //ViewのデータをDataTableオブジェクトにロードする
      this.Load(reader);
    }

    private void DisableConstraints() {
      //SQLiteにおいて、外部結合のSELECT文の結果について、
      //結合対象のテーブル行がない場合に送出されるが、その原因は不明
      DataSet dataSet = new DataSet();
      dataSet.Tables.Add(this);
      dataSet.EnforceConstraints = false;
    }

    public string GetDbColumnTypeName(int i) {
      return _DbColumnTypeNames[i];
    }

    public System.Type GetDataType(int i) {
      //Return Me.GetRowType
      return _hostDataType[i];
    }
  }
}
