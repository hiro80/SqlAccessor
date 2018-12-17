using System;
using System.Reflection;

namespace SqlAccessor
{
  /// <summary>
  /// レコードのメタ情報を表す
  /// </summary>
  /// <typeparam name="TRecord"></typeparam>
  /// <remarks>スレッドセーフ、Singletonパターン</remarks>
  [System.Diagnostics.DebuggerDisplay("RecordName:{Name}")]
  internal class RecordInfo<TRecord> where TRecord: class, IRecord, new()
  {
    private static readonly RecordInfo<TRecord> _recordInfo = new RecordInfo<TRecord>();
    //レコードのメタ情報
    private readonly Type _recordType;
    private readonly string _recordName;
    private readonly PropertyInfo[] _properties;

    private RecordInfo() {
      _recordType = (new TRecord()).GetType();
      _recordName = _recordType.Name;
      _properties = _recordType.GetProperties();
    }

    public static RecordInfo<TRecord> GetInstance() {
      return _recordInfo;
    }

    public string Name {
      get { return _recordName; }
    }

    public PropertyInfo[] Properties {
      get { return _properties; }
    }

    public PropertyInfo GetPropertyInfo(string propertyName) {
      //指定したプロパティが無ければnullが返る
      return _recordType.GetProperty(propertyName);
    }
  }
}
