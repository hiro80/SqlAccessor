using System.Collections.Generic;
using System.Text;
using MiniSqlParser;

namespace Tester
{
  class Tester
  {
    public static string Compact(string inputText
                                , DBMSType dbmsType
                                , Dictionary<string,string> placeHolders = null) {
      var ast = MiniSqlParserAST.CreateStmts(inputText, dbmsType);
      var placeHolderNodes = SetPlaceHoldersVisitor.ConvertPlaceHolders(placeHolders);
      var stringifier = new CompactStringifier(64, true, placeHolderNodes);
      ast.Accept(stringifier);  
      return stringifier.ToString();
    }

    public static string Beautiful(string inputText
                                  , DBMSType dbmsType
                                  , Dictionary<string, string> placeHolders = null) {
      var ast = MiniSqlParserAST.CreateStmts(inputText, dbmsType);
      var placeHolderNodes = SetPlaceHoldersVisitor.ConvertPlaceHolders(placeHolders);
      var stringifier = new BeautifulStringifier(99999
                                                , 2
                                                , BeautifulStringifier.KeywordCase.Upper
                                                , BeautifulStringifier.JoinIndentType.A
                                                , true
                                                , placeHolderNodes);
      ast.Accept(stringifier);
      return stringifier.ToString();
    }

    public static string RenameTableAliasName(string inputText
                                            , DBMSType dbmsType
                                            , Dictionary<string, string> placeHolders = null) {
      var ast = MiniSqlParserAST.CreateStmts(inputText, dbmsType);

      var visitor = new RenameTableAliasVisitor("T", "Table", placeHolders);
      ast.Accept(visitor);

      var stringifier = new BeautifulStringifier(144, visitor.PlaceHolders);
      ast.Accept(stringifier);
      return stringifier.ToString();
    }

    public static string GetResultInfoList(string inputText
                                          , DBMSType dbmsType
                                          , Dictionary<string, string> placeHolders = null) {
      var ast = MiniSqlParserAST.CreateStmts(inputText, dbmsType);

      var placeHolderNodes = SetPlaceHoldersVisitor.ConvertPlaceHolders(placeHolders);

      // テスト用テーブル列
      var tableColumns = new Dictionary<string, IEnumerable<TableResultInfo>>();
      var tableT = new List<TableResultInfo>();
      tableT.Add(new TableResultInfo("T", "x", false, true));
      tableT.Add(new TableResultInfo("T", "y", false));
      tableT.Add(new TableResultInfo("T", "z", true));
      tableColumns.Add("T", tableT);
      var tableU = new List<TableResultInfo>();
      tableU.Add(new TableResultInfo("U", "x", false, true));
      tableU.Add(new TableResultInfo("U", "y", false));
      tableU.Add(new TableResultInfo("U", "z", true));
      tableColumns.Add("U", tableU);
      var tableV = new List<TableResultInfo>();
      tableV.Add(new TableResultInfo("V", "x1", false, true));
      tableV.Add(new TableResultInfo("V", "x2", false, true));
      tableV.Add(new TableResultInfo("V", "x3", false, true));
      tableColumns.Add("V", tableV);
      var tableTBL = new List<TableResultInfo>();
      tableTBL.Add(new TableResultInfo("TBL", "COLUMN", false, true));
      tableTBL.Add(new TableResultInfo("TBL", "column", false, true));
      tableColumns.Add("TBL", tableTBL);
      var tableTbl = new List<TableResultInfo>();
      tableTbl.Add(new TableResultInfo("Tbl", "COL", false, true));
      tableTbl.Add(new TableResultInfo("Tbl", "col", false, true));
      tableColumns.Add("Tbl", tableTbl);

      var resultInfoAST = new ResultInfoAST((SelectStmt)ast[0]
                                          , tableColumns
                                          , ResultInfoAST.PrimaryKeyCompletion.AllQuery);

      var stringifier = new BeautifulStringifier(144, placeHolderNodes);
      ast.Accept(stringifier);

      return stringifier.ToString() + System.Environment.NewLine +
             resultInfoAST.Print(true);
    }

    public static string AddWherePredicate(string inputText
                                          , DBMSType dbmsType
                                          , Dictionary<string, string> placeHolders = null) {
      var ast = MiniSqlParserAST.CreateStmts(inputText, dbmsType);

      var placeHolderNodes = SetPlaceHoldersVisitor.ConvertPlaceHolders(placeHolders);

      var visitor = new AddWherePredicateVisitor("x = 'a'");
      ast.Accept(visitor);

      var stringifier = new BeautifulStringifier(144, placeHolderNodes);
      ast.Accept(stringifier);
      return stringifier.ToString();
    }

    public static string GetCNF(string inputText
                              , DBMSType dbmsType
                              , Dictionary<string, string> placeHolders = null) {
      var ast = MiniSqlParserAST.CreateStmts(inputText, dbmsType);

      // テスト用テーブル列
      var tableColumns = new BestCaseDictionary<IEnumerable<string>>();
      var tableT = new string[] { "x", "y", "z" };
      tableColumns.Add("T", tableT);
      var tableU = new string[] { "x", "y", "z" };
      tableColumns.Add("U", tableU);
      var tableV = new string[] { "x1", "x2", "x3" };
      tableColumns.Add("V", tableV);

      var replacer = new ReplacePlaceHolders(placeHolders);
      ast.Accept(replacer);

      var visitor = new GetCNFVisitor(tableColumns, true);
      ast.Accept(visitor);
      return visitor.Print(true);
    }

    public static string ConvertToSelectConstant(string inputText
                                                , DBMSType dbmsType
                                                , Dictionary<string, string> placeHolders = null) {
      var ast = MiniSqlParserAST.CreateStmts(inputText, dbmsType);

      var placeHolderNodes = SetPlaceHoldersVisitor.ConvertPlaceHolders(placeHolders);

      var visitor = new ConvertToSelectConstant();
      ast.Accept(visitor);

      var stringifier = new BeautifulStringifier(144, placeHolderNodes);
      ast.Accept(stringifier);
      return stringifier.ToString();
    }

    public static string GetSourceTables(string inputText
                                       , DBMSType dbmsType
                                       , Dictionary<string, string> placeHolders = null) {
      var ast = MiniSqlParserAST.CreateStmts(inputText, dbmsType);

      var placeHolderNodes = SetPlaceHoldersVisitor.ConvertPlaceHolders(placeHolders);

      var visitor = new GetSourceTablesVisitor(placeHolderNodes);
      ast.Accept(visitor);

      var sourceTables = visitor.Tables;
      var ret = new StringBuilder();
      foreach(var table in sourceTables) {
        ret.AppendLine(table.GetAliasOrTableName());
      }
      return ret.ToString();
    }

    public static string RenameColumnInOrderBy(string inputText
                                             , DBMSType dbmsType
                                             , Dictionary<string, string> placeHolders = null) {
      var ast = MiniSqlParserAST.CreateStmts(inputText, dbmsType);

      // テスト用テーブル列
      var tableColumns = new BestCaseDictionary<IEnumerable<string>>();
      var tableT = new string[] { "x", "y", "z" };
      tableColumns.Add("T", tableT);
      var tableU = new string[] { "x", "y", "z" };
      tableColumns.Add("U", tableU);
      var tableV = new string[] { "x1", "x2", "x3" };
      tableColumns.Add("V", tableV);

      var replacer = new ReplacePlaceHolders(placeHolders);
      ast.Accept(replacer);

      var visitor = new NormalizeOrderByVisitor(tableColumns, true);
      ast.Accept(visitor);

      var stringifier = new BeautifulStringifier(144);
      ast.Accept(stringifier);
      return stringifier.ToString();
    }

    public static string ReplacePlaceHolders(string inputText
                                             , DBMSType dbmsType
                                             , Dictionary<string, string> placeHolders = null) {
      var ast = MiniSqlParserAST.CreateStmts(inputText, dbmsType);

      var visitor = new ReplacePlaceHolders(placeHolders);
      ast.Accept(visitor);

      var stringifier = new BeautifulStringifier(144);
      ast.Accept(stringifier);
      return stringifier.ToString();
    }
  }
}
