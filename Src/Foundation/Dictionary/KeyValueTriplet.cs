namespace SqlAccessor
{
  public class KeyValueTriplet<TKey1, TKey2, TValue>
  {
    private TKey1 _key1;
    private TKey2 _key2;
    private TValue _value;

    public KeyValueTriplet(TKey1 key1, TKey2 key2, TValue value) {
      _key1 = key1;
      _key2 = key2;
      _value = value;
    }

    public TKey1 Key1 {
      get { return _key1; }
    }

    public TKey2 Key2 {
      get { return _key2; }
    }

    public TValue Value {
      get { return _value; }
    }
  }
}
