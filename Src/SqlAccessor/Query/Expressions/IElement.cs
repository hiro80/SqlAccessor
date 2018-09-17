namespace SqlAccessor
{
  internal interface IElement: System.ICloneable
  {
    string ToString(int orNestLevel = 0);
  }
}
