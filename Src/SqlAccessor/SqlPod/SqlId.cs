namespace SqlAccessor
{
  /// <summary>
  /// SqlPod内で一意のIDを表す
  /// </summary>
  /// <remarks></remarks>
  [System.Diagnostics.DebuggerDisplay("EntryName:{_entryName}  Seq:{_seq}")]
  internal class SqlId
  {
    private readonly string _entryName;
    private readonly int _seq;

    public SqlId(string entryName, int seq) {
      _entryName = entryName;
      _seq = seq;
    }

    public string EntryName {
      get { return _entryName; }
    }

    public int Seq {
      get { return _seq; }
    }

    public override string ToString() {
      return _entryName + "_" + _seq.ToString();
    }

    public static bool operator ==(SqlId lOperand, SqlId rOperand) {
      if((object)lOperand != null && (object)rOperand != null &&
         lOperand._entryName == rOperand._entryName &&
         lOperand._seq == rOperand._seq) {
        return true;
      } else {
        return (object)lOperand == null && (object)rOperand == null;
      }
    }

    public static bool operator !=(SqlId lOperand, SqlId rOperand) {
      return !(lOperand == rOperand);
    }

    public static bool operator <(SqlId lOperand, SqlId rOperand) {
      return lOperand._entryName == rOperand._entryName &&
             lOperand._seq < rOperand._seq;
    }

    public static bool operator >(SqlId lOperand, SqlId rOperand) {
      return lOperand._entryName == rOperand._entryName &&
             lOperand._seq > rOperand._seq;
    }

    public static SqlId operator +(SqlId operand, int i) {
      return new SqlId(operand._entryName, operand._seq + i);
    }
  }
}
