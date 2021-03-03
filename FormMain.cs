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
        private List<string> wordListResult;

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

        private List<string> wordListIndex = new();
        private List<string> GetWordListAllFromSelectedDicts(bool Rebuild=false)
        {
            if (!Rebuild && wordListIndex.Count > 0)
                return wordListIndex;
            
            wordListIndex.Clear();
            List<string> dictNames2Search = GetDictsInfo(sqliteInstance).Where(i => i.IsChecked == 1).Select(di => di.DictName).ToList();
            foreach (string dictName in dictNames2Search)
            {
                string sql = $"SELECT [{WordInfoColumns[0]}] FROM [{dictName}]" ; 

                wordListIndex.AddRange(sqliteInstance.SqlColumn(sql));
            }

            return wordListIndex;
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
            //窗口载入时，只列出字母a开头的单词
            listBoxWordsIndex.DataSource = GetWordListAllFromSelectedDicts().Where(a => StringHelper.StartsWithIgnoreCase(a, 'A')).ToList() ;
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
                //发生变动时，只列出字母a开头的单词
                listBoxWordsIndex.DataSource = GetWordListAllFromSelectedDicts(true).Where(a => StringHelper.StartsWithIgnoreCase(a, 'A')).ToList(); ;
            }
        }

        private void ComboBoxRegex_TextChanged(object sender, EventArgs e)
        {
            wordListResult = new();
            string text = comboBoxRegex.Text;
            if (text.Length < 2)
                return;
#nullable enable
            Regex? rg;
            try
            {
                rg = new Regex(text, RegexOptions.IgnoreCase);
            }
            catch (Exception)
            {
                statusLabel.Text = $"{text} 不是合法的正则表达式";
                return;
            }

            List<string> wordListAll = GetWordListAllFromSelectedDicts();
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

        private void ButtonExportResult_Click(object sender, EventArgs e)
        {
            string fileName = GetSearchResultFileName();
            string fileFullPath = $"{Environment.CurrentDirectory}/{ fileName}";

            //打开或创建结果文件（每天创建一个）。
            using (StreamWriter writer = new(new FileStream(fileFullPath, FileMode.Append, FileAccess.Write)))
            {
                DateTime now = DateTime.Now;
                writer.WriteLine($"查询时间：{now.Year,4}年{now.Month,2}月{now.Day,2}日，{now.Hour,2}时{now.Minute,2}分{now.Second,2}秒");
                writer.WriteLine($"查询表达式：{comboBoxRegex.Text}");
                string resultString = wordListResult.Aggregate("", (s1, s2) => s1 + " " + s2).Trim();
                writer.WriteLine($"查询结果：{resultString}");
                writer.WriteLine();
            }
            statusLabel.Text = $"查询结果已写入文件：{fileFullPath}";
        }

        #region 点击标签上的字母
        private enum LabelChar
        {
            A2I,
            J2R,
            S2Z,
        }
        private void LabelClick(LabelChar label, MouseEventArgs e)
        {
            char ch;
            int index = e.X < 15 ? 0 : 1 + (e.X - 15) / 18;
            switch (label)
            {
                case LabelChar.A2I:
                    ch = "ABCDEFGHI"[index];
                    break;
                case LabelChar.J2R:
                    ch = "JKLMNOPQR"[index];
                    break;
                default:
                    ch = "STUVWXYZ"[index];
                    break;

            }

            listBoxWordsIndex.DataSource= GetWordListAllFromSelectedDicts().Where(a=> StringHelper.StartsWithIgnoreCase(a,ch)).ToList();
        }

        private void LabelA2I_MouseClick(object sender, MouseEventArgs e)
        {
            LabelClick(LabelChar.A2I, e);
        }

        private void labelJ2R_MouseClick(object sender, MouseEventArgs e)
        {
            LabelClick(LabelChar.J2R, e);
        }

        private void labelS2Z_MouseClick(object sender, MouseEventArgs e)
        {
            LabelClick(LabelChar.S2Z, e);
        }
        #endregion
    }
}
