using System.Windows.Forms;
using System.Collections.Generic;
using System;
using MiniSqlParser;

namespace Tester
{
  public partial class Form1: Form
  {
    public Form1() {
      InitializeComponent();
      this.dbmsType.SelectedIndex = 0;
      this.selectedVisitor.SelectedIndex = 2;
    }

    private Dictionary<string, string> _placeHolders;

    private DBMSType GetDBMSType() {
      if(this.dbmsType.Text == "Oracle") {
        return DBMSType.Oracle;
      } else if(this.dbmsType.Text == "MySql") {
        return DBMSType.MySql;
      } else if(this.dbmsType.Text == "SQLite") {
        return DBMSType.SQLite;
      } else if(this.dbmsType.Text == "MsSql") {
        return DBMSType.MsSql;
      } else if(this.dbmsType.Text == "PostgreSql") {
        return DBMSType.PostgreSql;
      } else if(this.dbmsType.Text == "Pervasive") {
        return DBMSType.Pervasive;
      } else {
        return DBMSType.Unknown;
      }
    }

    private void doVisitor_Click(object sender, EventArgs e) {
      if(this.setPlaceHolders.Checked) {
        _placeHolders = new Dictionary<string, string>();
        _placeHolders.Add("PH1", "T.x");
        _placeHolders.Add("PH2", "T.x = 'abc'");
      }

      try{
        if(this.selectedVisitor.Text == "CompactStringifier") {
          this.inputText.Text = Tester.Compact(this.inputText.Text, this.GetDBMSType(), _placeHolders);
        } else if(this.selectedVisitor.Text == "BeautifulStringifier") {
          this.inputText.Text = Tester.Beautiful(this.inputText.Text, this.GetDBMSType(), _placeHolders);
        } else if(this.selectedVisitor.Text == "RenameTableAliasName") {
          this.inputText.Text = Tester.RenameTableAliasName(this.inputText.Text, this.GetDBMSType(), _placeHolders);
        } else if(this.selectedVisitor.Text == "GetResultInfoList") {
          this.inputText.Text = Tester.GetResultInfoList(this.inputText.Text, this.GetDBMSType(), _placeHolders);
        } else if(this.selectedVisitor.Text == "AddWherePredicate") {
          this.inputText.Text = Tester.AddWherePredicate(this.inputText.Text, this.GetDBMSType(), _placeHolders);
        } else if(this.selectedVisitor.Text == "GetCNF") {
          this.inputText.Text = Tester.GetCNF(this.inputText.Text, this.GetDBMSType(), _placeHolders);
        } else if(this.selectedVisitor.Text == "ConvertToSelectConstant") {
          this.inputText.Text = Tester.ConvertToSelectConstant(this.inputText.Text, this.GetDBMSType(), _placeHolders);
        } else if(this.selectedVisitor.Text == "GetSourceTables") {
          this.inputText.Text = Tester.GetSourceTables(this.inputText.Text, this.GetDBMSType(), _placeHolders);
        } else if(this.selectedVisitor.Text == "RenameColumnInOrderBy") {
          this.inputText.Text = Tester.RenameColumnInOrderBy(this.inputText.Text, this.GetDBMSType(), _placeHolders);
        } else if(this.selectedVisitor.Text == "ReplacePlaceHolders") {
          this.inputText.Text = Tester.ReplacePlaceHolders(this.inputText.Text, this.GetDBMSType(), _placeHolders);
        }
      }catch(SqlSyntaxErrorsException ex){
        string message = "";
        foreach(var syntaxError in ex.Errors) {
          message += syntaxError.Line.ToString() + "行目: " + syntaxError.Message + Environment.NewLine;
        }
        MessageBox.Show(message);
      }
    }

    private void clear_Click(object sender, EventArgs e) {
      this.inputText.Clear();
    }

  }
}
