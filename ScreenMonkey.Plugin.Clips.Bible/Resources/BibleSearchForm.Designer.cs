namespace ScreenMonkey.Plugin.Clips.Bible.Resources
{
    partial class BibleSearchForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSelect = new System.Windows.Forms.Button();
            this.lstResults = new System.Windows.Forms.ListBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lblSearchTerm = new System.Windows.Forms.Label();
            this.txtSearchTerms = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblProgress = new System.Windows.Forms.Label();
            this.chkCaseSensitivity = new System.Windows.Forms.CheckBox();
            this.chkSearchWithin = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(489, 359);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSelect
            // 
            this.btnSelect.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSelect.Location = new System.Drawing.Point(391, 359);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(75, 23);
            this.btnSelect.TabIndex = 3;
            this.btnSelect.Text = "Select";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // lstResults
            // 
            this.lstResults.FormattingEnabled = true;
            this.lstResults.Location = new System.Drawing.Point(24, 115);
            this.lstResults.Name = "lstResults";
            this.lstResults.Size = new System.Drawing.Size(521, 212);
            this.lstResults.TabIndex = 2;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(24, 95);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(521, 14);
            this.progressBar1.TabIndex = 3;
            this.progressBar1.Visible = false;
            // 
            // lblSearchTerm
            // 
            this.lblSearchTerm.AutoSize = true;
            this.lblSearchTerm.Location = new System.Drawing.Point(21, 43);
            this.lblSearchTerm.Name = "lblSearchTerm";
            this.lblSearchTerm.Size = new System.Drawing.Size(82, 13);
            this.lblSearchTerm.TabIndex = 4;
            this.lblSearchTerm.Text = "Search Term(s):";
            // 
            // txtSearchTerms
            // 
            this.txtSearchTerms.Location = new System.Drawing.Point(109, 40);
            this.txtSearchTerms.Name = "txtSearchTerms";
            this.txtSearchTerms.Size = new System.Drawing.Size(350, 20);
            this.txtSearchTerms.TabIndex = 0;
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(470, 40);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(75, 23);
            this.btnSearch.TabIndex = 1;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(189, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(145, 23);
            this.lblTitle.TabIndex = 7;
            this.lblTitle.Text = "Bible Search";
            // 
            // lblProgress
            // 
            this.lblProgress.AutoSize = true;
            this.lblProgress.Location = new System.Drawing.Point(213, 70);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(97, 13);
            this.lblProgress.TabIndex = 8;
            this.lblProgress.Text = "Progress of Search";
            this.lblProgress.Visible = false;
            // 
            // chkCaseSensitivity
            // 
            this.chkCaseSensitivity.AutoSize = true;
            this.chkCaseSensitivity.Location = new System.Drawing.Point(24, 359);
            this.chkCaseSensitivity.Name = "chkCaseSensitivity";
            this.chkCaseSensitivity.Size = new System.Drawing.Size(140, 17);
            this.chkCaseSensitivity.TabIndex = 9;
            this.chkCaseSensitivity.Text = "Case Insensitive Search";
            this.chkCaseSensitivity.UseVisualStyleBackColor = true;
            // 
            // chkSearchWithin
            // 
            this.chkSearchWithin.AutoSize = true;
            this.chkSearchWithin.Location = new System.Drawing.Point(193, 359);
            this.chkSearchWithin.Name = "chkSearchWithin";
            this.chkSearchWithin.Size = new System.Drawing.Size(180, 17);
            this.chkSearchWithin.TabIndex = 10;
            this.chkSearchWithin.Text = "Search Within Displayed Results";
            this.chkSearchWithin.UseVisualStyleBackColor = true;
            // 
            // BibleSearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(576, 394);
            this.Controls.Add(this.chkSearchWithin);
            this.Controls.Add(this.chkCaseSensitivity);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.txtSearchTerms);
            this.Controls.Add(this.lblSearchTerm);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.lstResults);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BibleSearchForm";
            this.Text = "Bible Search";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.ListBox lstResults;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lblSearchTerm;
        private System.Windows.Forms.TextBox txtSearchTerms;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.CheckBox chkCaseSensitivity;
        private System.Windows.Forms.CheckBox chkSearchWithin;
    }
}