namespace Tester
{
  partial class Form1
  {
    /// <summary>
    /// 必要なデザイナー変数です。
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// 使用中のリソースをすべてクリーンアップします。
    /// </summary>
    /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
    protected override void Dispose(bool disposing) {
      if(disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows フォーム デザイナーで生成されたコード

    /// <summary>
    /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
    /// コード エディターで変更しないでください。
    /// </summary>
    private void InitializeComponent() {
      this.doVisitor = new System.Windows.Forms.Button();
      this.inputText = new System.Windows.Forms.TextBox();
      this.clear = new System.Windows.Forms.Button();
      this.selectedVisitor = new System.Windows.Forms.ComboBox();
      this.dbmsType = new System.Windows.Forms.ComboBox();
      this.setPlaceHolders = new System.Windows.Forms.CheckBox();
      this.SuspendLayout();
      // 
      // doVisitor
      // 
      this.doVisitor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.doVisitor.Location = new System.Drawing.Point(471, 399);
      this.doVisitor.Name = "doVisitor";
      this.doVisitor.Size = new System.Drawing.Size(243, 29);
      this.doVisitor.TabIndex = 0;
      this.doVisitor.Text = "Do Visitor";
      this.doVisitor.UseVisualStyleBackColor = true;
      this.doVisitor.Click += new System.EventHandler(this.doVisitor_Click);
      // 
      // inputText
      // 
      this.inputText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.inputText.Font = new System.Drawing.Font("ＭＳ ゴシック", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
      this.inputText.Location = new System.Drawing.Point(12, 13);
      this.inputText.Multiline = true;
      this.inputText.Name = "inputText";
      this.inputText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.inputText.Size = new System.Drawing.Size(768, 381);
      this.inputText.TabIndex = 1;
      // 
      // clear
      // 
      this.clear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.clear.Location = new System.Drawing.Point(720, 399);
      this.clear.Name = "clear";
      this.clear.Size = new System.Drawing.Size(60, 29);
      this.clear.TabIndex = 2;
      this.clear.Text = "Clear";
      this.clear.UseVisualStyleBackColor = true;
      this.clear.Click += new System.EventHandler(this.clear_Click);
      // 
      // selectedVisitor
      // 
      this.selectedVisitor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.selectedVisitor.DropDownHeight = 128;
      this.selectedVisitor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.selectedVisitor.Font = new System.Drawing.Font("MS UI Gothic", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
      this.selectedVisitor.FormattingEnabled = true;
      this.selectedVisitor.IntegralHeight = false;
      this.selectedVisitor.Items.AddRange(new object[] {
            "CompactStringifier",
            "RenameTableAliasName",
            "BeautifulStringifier",
            "GetResultInfoList",
            "AddWherePredicate",
            "GetCNF",
            "ConvertToSelectConstant",
            "GetSourceTables",
            "RenameColumnInOrderBy",
            "ReplacePlaceHolders"});
      this.selectedVisitor.Location = new System.Drawing.Point(199, 399);
      this.selectedVisitor.Margin = new System.Windows.Forms.Padding(8);
      this.selectedVisitor.Name = "selectedVisitor";
      this.selectedVisitor.Size = new System.Drawing.Size(271, 29);
      this.selectedVisitor.TabIndex = 4;
      // 
      // dbmsType
      // 
      this.dbmsType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.dbmsType.DropDownHeight = 128;
      this.dbmsType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.dbmsType.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
      this.dbmsType.FormattingEnabled = true;
      this.dbmsType.IntegralHeight = false;
      this.dbmsType.Items.AddRange(new object[] {
            "Unkown",
            "Oracle",
            "MySql",
            "SQLite",
            "MsSql",
            "PostgreSql",
            "Pervasive"});
      this.dbmsType.Location = new System.Drawing.Point(97, 399);
      this.dbmsType.Margin = new System.Windows.Forms.Padding(8);
      this.dbmsType.Name = "dbmsType";
      this.dbmsType.Size = new System.Drawing.Size(100, 23);
      this.dbmsType.TabIndex = 5;
      // 
      // setPlaceHolders
      // 
      this.setPlaceHolders.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.setPlaceHolders.AutoSize = true;
      this.setPlaceHolders.Location = new System.Drawing.Point(12, 399);
      this.setPlaceHolders.Name = "setPlaceHolders";
      this.setPlaceHolders.Size = new System.Drawing.Size(85, 16);
      this.setPlaceHolders.TabIndex = 6;
      this.setPlaceHolders.Text = "PlaceHolder";
      this.setPlaceHolders.UseVisualStyleBackColor = true;
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(792, 435);
      this.Controls.Add(this.setPlaceHolders);
      this.Controls.Add(this.dbmsType);
      this.Controls.Add(this.selectedVisitor);
      this.Controls.Add(this.clear);
      this.Controls.Add(this.inputText);
      this.Controls.Add(this.doVisitor);
      this.Name = "Form1";
      this.Text = "Form1";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button doVisitor;
    private System.Windows.Forms.TextBox inputText;
    private System.Windows.Forms.Button clear;
    private System.Windows.Forms.ComboBox selectedVisitor;
    private System.Windows.Forms.ComboBox dbmsType;
    private System.Windows.Forms.CheckBox setPlaceHolders;
  }
}

