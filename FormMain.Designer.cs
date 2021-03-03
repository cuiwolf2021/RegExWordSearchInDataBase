
namespace RegExWordSearch
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxRegex = new System.Windows.Forms.ComboBox();
            this.btnShowWordsIndex = new System.Windows.Forms.Button();
            this.btnDictManage = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.listBoxWordsIndex = new System.Windows.Forms.ListBox();
            this.textBoxMeaning = new System.Windows.Forms.TextBox();
            this.listBoxResults = new System.Windows.Forms.ListBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnImportDict2Db = new System.Windows.Forms.Button();
            this.btnUsageHelp = new System.Windows.Forms.Button();
            this.btnRegexHelp = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(149, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "要查询的正则表达式：";
            // 
            // comboBoxRegex
            // 
            this.comboBoxRegex.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxRegex.FormattingEnabled = true;
            this.comboBoxRegex.Location = new System.Drawing.Point(208, 5);
            this.comboBoxRegex.Name = "comboBoxRegex";
            this.comboBoxRegex.Size = new System.Drawing.Size(535, 28);
            this.comboBoxRegex.TabIndex = 1;
            this.comboBoxRegex.Text = "请在此处输入正则表达式";
            this.comboBoxRegex.TextChanged += new System.EventHandler(this.comboBoxRegex_TextChanged);
            this.comboBoxRegex.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ComboBox1_KeyPress);
            // 
            // btnShowWordsIndex
            // 
            this.btnShowWordsIndex.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnShowWordsIndex.BackgroundImage")));
            this.btnShowWordsIndex.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnShowWordsIndex.Location = new System.Drawing.Point(12, 122);
            this.btnShowWordsIndex.Name = "btnShowWordsIndex";
            this.btnShowWordsIndex.Size = new System.Drawing.Size(30, 30);
            this.btnShowWordsIndex.TabIndex = 2;
            this.btnShowWordsIndex.Tag = "单词索引";
            this.toolTip1.SetToolTip(this.btnShowWordsIndex, "显示单词索引");
            this.btnShowWordsIndex.UseVisualStyleBackColor = true;
            // 
            // btnDictManage
            // 
            this.btnDictManage.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnDictManage.BackgroundImage")));
            this.btnDictManage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnDictManage.Location = new System.Drawing.Point(12, 86);
            this.btnDictManage.Name = "btnDictManage";
            this.btnDictManage.Size = new System.Drawing.Size(30, 30);
            this.btnDictManage.TabIndex = 3;
            this.toolTip1.SetToolTip(this.btnDictManage, "导入、卸载词典等");
            this.btnDictManage.UseVisualStyleBackColor = true;
            this.btnDictManage.Click += new System.EventHandler(this.BtnDictManage_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel,
            this.toolStripProgressBar1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 430);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.statusStrip1.Size = new System.Drawing.Size(799, 22);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.toolStripProgressBar1.RightToLeftLayout = true;
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 18);
            this.toolStripProgressBar1.Visible = false;
            // 
            // listBoxWordsIndex
            // 
            this.listBoxWordsIndex.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxWordsIndex.FormattingEnabled = true;
            this.listBoxWordsIndex.ItemHeight = 20;
            this.listBoxWordsIndex.Location = new System.Drawing.Point(48, 45);
            this.listBoxWordsIndex.Name = "listBoxWordsIndex";
            this.listBoxWordsIndex.Size = new System.Drawing.Size(176, 364);
            this.listBoxWordsIndex.TabIndex = 5;
            this.toolTip1.SetToolTip(this.listBoxWordsIndex, "所有单词的列表");
            this.listBoxWordsIndex.SelectedIndexChanged += new System.EventHandler(this.ListBoxWordsIndex_SelectedIndexChanged);
            // 
            // textBoxMeaning
            // 
            this.textBoxMeaning.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxMeaning.Location = new System.Drawing.Point(421, 45);
            this.textBoxMeaning.Multiline = true;
            this.textBoxMeaning.Name = "textBoxMeaning";
            this.textBoxMeaning.Size = new System.Drawing.Size(378, 366);
            this.textBoxMeaning.TabIndex = 6;
            this.toolTip1.SetToolTip(this.textBoxMeaning, "选中单词的释义");
            // 
            // listBoxResults
            // 
            this.listBoxResults.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxResults.FormattingEnabled = true;
            this.listBoxResults.ItemHeight = 20;
            this.listBoxResults.Location = new System.Drawing.Point(234, 45);
            this.listBoxResults.Name = "listBoxResults";
            this.listBoxResults.Size = new System.Drawing.Size(176, 364);
            this.listBoxResults.Sorted = true;
            this.listBoxResults.TabIndex = 7;
            this.toolTip1.SetToolTip(this.listBoxResults, "匹配正则表达式的单词列表");
            // 
            // btnImportDict2Db
            // 
            this.btnImportDict2Db.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnImportDict2Db.BackgroundImage")));
            this.btnImportDict2Db.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnImportDict2Db.Location = new System.Drawing.Point(12, 49);
            this.btnImportDict2Db.Name = "btnImportDict2Db";
            this.btnImportDict2Db.Size = new System.Drawing.Size(30, 30);
            this.btnImportDict2Db.TabIndex = 12;
            this.toolTip1.SetToolTip(this.btnImportDict2Db, "将词典导入数据库");
            this.btnImportDict2Db.UseVisualStyleBackColor = true;
            this.btnImportDict2Db.Click += new System.EventHandler(this.BtnImportDict2Db_ClickAsync);
            // 
            // btnUsageHelp
            // 
            this.btnUsageHelp.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnUsageHelp.BackgroundImage")));
            this.btnUsageHelp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnUsageHelp.Location = new System.Drawing.Point(11, 158);
            this.btnUsageHelp.Name = "btnUsageHelp";
            this.btnUsageHelp.Size = new System.Drawing.Size(30, 30);
            this.btnUsageHelp.TabIndex = 13;
            this.toolTip1.SetToolTip(this.btnUsageHelp, "软件使用帮助");
            this.btnUsageHelp.UseVisualStyleBackColor = true;
            this.btnUsageHelp.Click += new System.EventHandler(this.btnUsageHelp_Click);
            // 
            // btnRegexHelp
            // 
            this.btnRegexHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRegexHelp.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnRegexHelp.BackgroundImage")));
            this.btnRegexHelp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnRegexHelp.Location = new System.Drawing.Point(753, 4);
            this.btnRegexHelp.Name = "btnRegexHelp";
            this.btnRegexHelp.Size = new System.Drawing.Size(30, 30);
            this.btnRegexHelp.TabIndex = 8;
            this.toolTip1.SetToolTip(this.btnRegexHelp, "正则表达式使用指南");
            this.btnRegexHelp.UseVisualStyleBackColor = true;
            this.btnRegexHelp.Click += new System.EventHandler(this.btnRegexHelp_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(799, 452);
            this.Controls.Add(this.btnUsageHelp);
            this.Controls.Add(this.btnImportDict2Db);
            this.Controls.Add(this.btnRegexHelp);
            this.Controls.Add(this.listBoxResults);
            this.Controls.Add(this.textBoxMeaning);
            this.Controls.Add(this.listBoxWordsIndex);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.btnDictManage);
            this.Controls.Add(this.btnShowWordsIndex);
            this.Controls.Add(this.comboBoxRegex);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.HelpButton = true;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "正则表达式英语单词查询";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxRegex;
        private System.Windows.Forms.Button btnShowWordsIndex;
        private System.Windows.Forms.Button btnDictManage;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ListBox listBoxWordsIndex;
        private System.Windows.Forms.TextBox textBoxMeaning;
        private System.Windows.Forms.ListBox listBoxResults;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnRegexHelp;
        private System.Windows.Forms.Button btnImportDict2Db;
        private System.Windows.Forms.Button btnUsageHelp;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
    }
}

