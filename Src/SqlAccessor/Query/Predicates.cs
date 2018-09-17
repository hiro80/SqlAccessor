namespace SqlAccessor
{
  /// <summary>
  /// テーブル名をハッシュキーとしてAND条件(文字列)を格納したハッシュ
  /// </summary>
  /// <remarks></remarks>
  internal class Predicates: System.Collections.Generic.Dictionary<string, string>
  {
    public new string this[string key] {
      get {
        if(key == null) {
          return null;
        }

        //テーブル名は大小文字を区別しない
        string tableName = key.ToUpper();
        if(!this.ContainsKey(tableName)) {
          return null;
        }
        return base[tableName];
      }

      set {
        //テーブル名は大小文字を区別しない
        string tableName = key.ToUpper();
        base[tableName] = value;
      }
    }
  }
}
