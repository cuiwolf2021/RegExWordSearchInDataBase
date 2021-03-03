using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Utils;
using static RegExWordSearch.DictHelper;

namespace RegExWordSearch
{
    public partial class FormImportDict : Form
    {
        private string tableName2SQLite = "";
        private string fileName = "";
        private string briefIntro = "";
        public FormImportDict()
        {
            InitializeComponent();
        }

        private void TextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            //只允许输入汉字、英文字母、数字
            Regex rg = new Regex("^[\u4e00-\u9fa5a-zA-Z0-9\b]$");
            if (!rg.IsMatch(e.KeyChar.ToString()))
            {
                e.Handled = true;
            }
        }

        private void FormImportDict_Load(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Microsoft Excel 文件(*.xlsx)|*.xlsx|Microsoft Excel 97-2003 文件(*.xls)|*.xls";
            openFileDialog1.Title = "指定词典文件";
            openFileDialog1.FileName = "";
        }

        private void ButtonOpenFileDialog_Click(object sender, EventArgs e)
        {
            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                fileName = openFileDialog1.FileName;
                textBoxBriefIntroduction.Text = fileName.Substring(0,fileName.LastIndexOf("."));
            }
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            tableName2SQLite = textBoxTableName.Text;
            briefIntro = textBoxBriefIntroduction.Text;

            string errMsg = "";
            if (tableName2SQLite.Length < 2)
            {
                errMsg = "词典名字至少需要2个字符！";
            }
            else if (GetDictsInfo(FormMain.sqliteInstance).Contains(tableName2SQLite))
            {
                //tableName已经存在
                errMsg = "您指定的词典名字已经存在，请重新指定名字。";
            }
            else if (string.IsNullOrEmpty(fileName))
            {
                errMsg = "您还没有指定词典文件，请指定。";
            }
            else if (briefIntro.Length < 10)
            {
                errMsg = "词典简介10个字符，请完善简介信息。";
            }

            DataTable wordsTable = ExcelNpoiUtility.ExcelToDataTableNpoi(fileName);
            if (wordsTable == null)
            {
                errMsg = "读取EXCEL文件失败，请关闭文件后再次尝试。";
            }
            if (errMsg.Length > 0)
            {
                MessageBox.Show(errMsg, "词典错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBoxTableName.Focus();
                return;
            }

            string sql2CreateTable = DictHelper.FormatCreateWordInfoSql(tableName2SQLite);
            FormMain.sqliteInstance.WriteTable2Db(wordsTable, tableName2SQLite,sql2CreateTable);
            
            DictInfo di = new DictInfo
            {
                DictName = tableName2SQLite,
                WordCount = wordsTable.Rows.Count,
                BriefIntro = briefIntro,
                IsChecked = 1 //新加入的默认为勾选
            };
            List<DictInfo> diList = new List<DictInfo>
            {
                di
            };
            SetupDictsInfo(diList, DictOps.AddDict, FormMain.sqliteInstance);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
