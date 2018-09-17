using System.Text;
using SqlAccessor;

public class JkCastEditor: CastEditor
{

  private static string TrimWithDefaultValue(string columnValue, ColumnInfo aColumnInfo) {
    if(aColumnInfo == null || string.IsNullOrEmpty(aColumnInfo.DefaultValue)) {
      //aColumnInfoが取得できない、又はDEFAULT値が設定されていない場合は、
      //半角/全角空白を削除する
      return columnValue.TrimEnd(' ', '　');
    } else {
      //DEFAULT値の先頭一文字目を削除文字とする
      char trimChar = aColumnInfo.DefaultValue.ToCharArray(0, 1)[0];
      if(char.IsDigit(trimChar)) {
        //削除文字が数字文字の場合、columnValueの先頭から削除文字を削除する
        return columnValue.TrimStart(trimChar);
      } else {
        //columnValueの末尾から削除文字を削除する
        return columnValue.TrimEnd(trimChar);
      }
    }
  }

  private static string FillWithDefaultValue(object value, ColumnInfo aColumninfo) {
    if(aColumninfo == null || 
      string.IsNullOrEmpty(aColumninfo.DefaultValue) || 
      !aColumninfo.MaxLength.HasValue) {
      //格納先のテーブルカラムの最大文字数が分からない、
      //又はDEFAULT値が設定されていない場合、文字を充填しない
      return value.ToString();
    } else {
      //テーブルカラムの(Shift_JISにおける)最大Byte数
      int columnMaxByte = aColumninfo.MaxLength.Value;
      //DEFAULT値の先頭一文字目を充填文字とする
      char fillChar = aColumninfo.DefaultValue.ToCharArray(0, 1)[0];
      if(char.IsDigit(fillChar)) {
        //充填文字が数字文字の場合、columnValueの先頭に充填文字を充填する
        return value.ToString().PadLeft(columnMaxByte, fillChar);
      } else {
        //columnValueの末尾に充填文字を充填する
        return JkCastEditor.BytePadRight(value.ToString(), columnMaxByte, fillChar);
      }
    }
  }

  /// <summary>
  /// 文字列の末尾に、引数で指定された(Shift_JISにおける)Byte数だけ充填文字を充填する
  /// </summary>
  /// <param name="str">入力文字列</param>
  /// <param name="totalByte">結果として生成される文字列の(Shift_JISにおける)Byte数</param>
  /// <param name="paddingChar">充填文字</param>
  /// <returns></returns>
  /// <remarks></remarks>
  private static string BytePadRight(string str, int totalByte, char paddingChar) {
    Encoding sjisEncoding = Encoding.GetEncoding("Shift_JIS");

    int strByte = sjisEncoding.GetByteCount(str);
    int paddingCharByte = sjisEncoding.GetByteCount(paddingChar.ToString());
    int paddingLength = (totalByte - strByte) / paddingCharByte;

    StringBuilder ret = new StringBuilder(str);
    for(int i = 0; i <= paddingLength - 1; i++) {
      ret.Append(paddingChar);
    }

    return ret.ToString();
  }

  //文字列をDBのCHAR型項目から取得する時は、空白を削除する
  public override object BeforeCast_ViewColumnType(StringViewColumnType viewColumnType
                                                  , ViewColumnInfo aViewColumnInfo
                                                  , object viewColumnValue) {
    //テーブルカラムのデフォルト値からは空白を削除しない
    if(aViewColumnInfo.ViewName == "ColumnInfo" && 
       aViewColumnInfo.ViewColumnName == "DefaultValue") {
      return viewColumnValue.ToString();
    }
    return JkCastEditor.TrimWithDefaultValue(viewColumnValue.ToString(), null);
  }

  //Public Overloads Function BeforeCast(ByVal sqlLiteralType As StringSqlLiteralType, _
  //                                      ByVal propertyType As StringPropertyType, _
  //                                      ByVal aColumnInfo As ColumnInfo, _
  //                                      ByVal propertyValue As Object) As Object
  //  Return JkCastEditor.FillWithDefaultValue(propertyValue, aColumnInfo)
  //End Function

  public object BeforeCast(NumberSqlLiteralType sqlLiteralType
                          , StringPropertyType propertyType
                          , ColumnInfo aColumnInfo
                          , object propertyValue) {
    string numStr = (string)propertyValue;
    decimal dec = default(decimal);
    decimal.TryParse(numStr, out dec);
    return dec.ToString();
  }
}
