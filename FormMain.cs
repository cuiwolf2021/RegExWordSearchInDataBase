using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils;
using static RegExWordSearch.DictHelper;
using static Utils.SqliteSingleton;

namespace RegExWordSearch
{
    public partial class FormMain : Form
    {
        public static SqliteSingleton sqliteInstance = GetInstance(DictHelper.DbFileFolder, DictHelper.DbFileName);
        public FormMain()
        {
            InitializeComponent();
        }

        private void ComboBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.Equals((char)13))
            {
                string text = comboBoxRegex.Text;
                if (!comboBoxRegex.Items.Contains(text))
                    comboBoxRegex.Items.Add(comboBoxRegex.Text);
            }
        }

        //TODO: Lazy模式，只创建一次，若没有其他修改，则不再创建。
        private static List<string> GetWordListAllFromSelectedDicts(bool Rebuild)
        {
            List<string> wordListAll = new();
            List<string> dictNames2Search = GetDictsInfo(sqliteInstance).Where(i => i.IsChecked == 1).Select(di => di.DictName).ToList();
            foreach (string dictName in dictNames2Search)
            {
                string sql = $"SELECT [{WordInfoColumns[0]}] FROM [{dictName}]";
                wordListAll.AddRange(sqliteInstance.SqlColumn(sql));
            }

            return wordListAll;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //检查数据库文件夹、数据库文件、数据库表是否存在
            //SqliteHelper.GetDbFileStatus();
            if (sqliteInstance.GetDbFileStatus() != DbStatus.DbIsOk)
            {
                MessageBox.Show("词典不存在，请首先导入词典！",
                    "数据库错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                btnDictManage.Visible = false;
                btnShowWordsIndex.Visible = false;
                return;
            }

            //若库中有词典文件存在，且在搜索组被勾选，则列出被选中词典的词
            //TODO: 按需列出，当拉滚动条时，再从数据库获取新的。单词按照搜索组勾选顺序当一个词典全部显示完毕后再显示后面的词典。
            listBoxWordsIndex.DataSource = GetWordListAllFromSelectedDicts(true);
        }

        private void BtnImportDict2Db_ClickAsync(object sender, EventArgs e)
        {
            FormImportDict formId = new();
            if (DialogResult.OK == formId.ShowDialog())
            {
                statusLabel.Text = "完成词典文件导入任务。";
                btnDictManage.Visible = true;
                btnShowWordsIndex.Visible = true;
            }
        }

        /// <summary>
        /// 选中一个单词。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBoxWordsIndex_SelectedIndexChanged(object sender, EventArgs e)
        {
            string text = (string)listBoxWordsIndex.SelectedValue;
            statusLabel.Text = "选中单词：" + text;
            comboBoxRegex.Text = text;
        }

        /// <summary>
        /// 打开词典管理窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDictManage_Click(object sender, EventArgs e)
        {
            FormDictManage formDm = new();
            DialogResult dr = formDm.ShowDialog();
            if (formDm.IsCheckIndiceChanged)
            {
                listBoxWordsIndex.DataSource = GetWordListAllFromSelectedDicts(true);
            }
        }

        private void comboBoxRegex_TextChanged(object sender, EventArgs e)
        {
            string text = comboBoxRegex.Text;
            if (text.Length < 2)
                return;
#nullable enable
            Regex? rg;
            try
            {
                rg = new Regex(text,RegexOptions.IgnoreCase);
            }
            catch (Exception)
            {
                statusLabel.Text = $"{text} 不是合法的正则表达式";
                return;
            }
            List<string> wordListAll = GetWordListAllFromSelectedDicts(false);

            List<string> wordListResult = new();
            foreach (string word in wordListAll)
            {
                if (rg!.IsMatch(word))
                    wordListResult.Add(word);
            }
            listBoxResults.DataSource = wordListResult;
            
            listBoxResults.Refresh();
            statusLabel.Text = "匹配单词数量：" + wordListResult.Count;
        }

        private void btnRegexHelp_Click(object sender, EventArgs e)
        {
            OpenHelpFile(RegexHelpFile);
        }


        #region 打开帮助文件
        private void btnUsageHelp_Click(object sender, EventArgs e)
        {
            OpenHelpFile(AppHelpFile);
        }       
        private void OpenHelpFile(string fileName)
        {
            try
            {
                string fileFullPath = $"{Environment.CurrentDirectory}/{ fileName}";
                ProcessStartInfo processStartInfo = new(fileFullPath);
                Process process = new()
                {
                    StartInfo = processStartInfo
                };
                process.StartInfo.UseShellExecute = true;
                process.Start();
            }
            catch (Exception)
            {
                statusLabel.Text = $"本机未安装合适程序，无法打开【 {RegexHelpFile} 】文件，或文件被删除。";
            }
        }
        #endregion
    }
}
