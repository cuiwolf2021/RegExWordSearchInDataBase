using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils;
using static RegExWordSearch.DictHelper;

namespace RegExWordSearch
{
    public partial class FormDictManage : Form
    {
        public bool IsCheckIndiceChanged = false;
        /// <summary>
        /// 包含每一词典名字、词典内单词数量
        /// </summary>
        private List<DictInfo> DictsInfoList = null;

        /// <summary>
        /// 包含每一词典的勾选状态
        /// </summary>
        private int[] CheckedStateArr = null;
        
        public FormDictManage()
        {
            InitializeComponent();
        }

        private void LoadDicts2ListView(ListView listView)
        {
            listView.Items.Clear();
            DictsInfoList = GetDictsInfo(FormMain.sqliteInstance);
            CheckedStateArr = new int[DictsInfoList.Count];

            for (int i = 0; i < DictsInfoList.Count; i++)
            {
                DictInfo di = DictsInfoList[i];
                ListViewItem item = new ListViewItem(new string[] { di.DictName, di.WordCount.ToString(), di.BriefIntro })
                {
                    Checked = di.IsChecked == 1
                };//VS推荐的简化对象初始化

                listView.Items.Add(item);
                CheckedStateArr[i] = di.IsChecked;
            }
            listView.Refresh();
        }
        private void FormDictManage_Load(object sender, EventArgs e)
        {
            LoadDicts2ListView(listViewDictsManage);
        }

        private void ButtonImportDict_Click(object sender, EventArgs e)
        {
            FormImportDict formId = new FormImportDict();
            if (DialogResult.OK == formId.ShowDialog())
            {
                statusLabel.Text = "完成词典文件导入任务。";
                LoadDicts2ListView(listViewDictsManage);
            }
        }

        private void ButtonRemoveDict_Click(object sender, EventArgs e)
        {
            var items = GetIndicesToOp();
            if (items.Count == 0)
            {
                MessageBox.Show("没有选中要卸载的词典", "卸载词典", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult dr = MessageBox.Show("即将卸载选中的" + items.Count + "个词典，是否确定？", "卸载词典", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.No)
                return;
            else
            {
                List<DictInfo> dicts2Remove = new List<DictInfo>();

                foreach (var item in items)
                {
                    int i = (int)item;
                    dicts2Remove.Add(DictsInfoList[i]);
                }

                FormMain.sqliteInstance.DeleteTables(dicts2Remove.Select(i => i.DictName).ToList()); //Select中是个函数，对每个DictInfo获得其中的DictName。
                DictHelper.SetupDictsInfo(dicts2Remove, DictOps.RemoveDicts, FormMain.sqliteInstance);
                LoadDicts2ListView(listViewDictsManage);
                statusLabel.Text = "完成词典文件删除任务。";
            }
        }

        private void ButtonCloseForm_Click(object sender, EventArgs e)
        {
            Close();
        }

        private enum OperateOnIndices
        {
            CheckedIndices,
            SelectedIndices
        }
        /// <summary>
        /// 获取View上要操作的词典索引。按 Checked还是按Selected？
        /// </summary>
        /// <returns></returns>
        private IList GetIndicesToOp(OperateOnIndices ooi = OperateOnIndices.SelectedIndices)
        {
            IList result = null;
            ooi = ooi == OperateOnIndices.CheckedIndices ? OperateOnIndices.CheckedIndices : OperateOnIndices.SelectedIndices;
            switch (ooi)
            {
                case OperateOnIndices.CheckedIndices:
                    result = listViewDictsManage.CheckedIndices;
                    break;
                case OperateOnIndices.SelectedIndices:
                    result = listViewDictsManage.SelectedIndices;
                    break;
            }
            return result;

        }
        private void Button2Higher_Click(object sender, EventArgs e)
        {
            var items = GetIndicesToOp();
            if (items.Count > 1)
            {
                MessageBox.Show("选中的词典个数大于1，功能暂未实现！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            List<DictInfo> dicts2Higher = new List<DictInfo>();
            foreach (var item in items)
            {
                int i = (int)item;
                if (i == 0)
                {
                    //包含第一个，无法再升高
                    MessageBox.Show("选中的词典包含第一个，无法再提升优先级！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    statusLabel.Text = "词典文件优先级提升失败。";
                    return;
                }
                dicts2Higher.Add(DictsInfoList[i]);
            }

            DictHelper.SetupDictsInfo(dicts2Higher, DictOps.DictToHigher, FormMain.sqliteInstance);
            LoadDicts2ListView(listViewDictsManage);
            statusLabel.Text = "完成词典文件优先级提升任务。";
        }

        private void Button2Lower_Click(object sender, EventArgs e)
        {
            var items = GetIndicesToOp();
            if (items.Count > 1)
            {
                MessageBox.Show("选中的词典个数大于1，功能暂未实现！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            List<DictInfo> dicts2Lower = new List<DictInfo>();
            foreach (var item in items)
            {
                int i = (int)item;
                if (i == listViewDictsManage.Items.Count - 1)
                {
                    //包含最后个，无法再降低
                    MessageBox.Show("选中的词典包含最后一个词典，无法再降低优先级！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    statusLabel.Text = "词典文件优先级降低失败。";
                    return;
                }
                dicts2Lower.Add(DictsInfoList[i]);
            }

            SetupDictsInfo(dicts2Lower, DictOps.DictToLower, FormMain.sqliteInstance);
            LoadDicts2ListView(listViewDictsManage);
            statusLabel.Text = "完成词典文件优先级降低任务。";
        }

        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                LoadDicts2ListView(listViewDictsManage);
            }
            else if (tabControl1.SelectedIndex == 1)
            {
                LoadDicts2ListView(listViewSearchManage);
            }

        }

        /// <summary>
        /// 每当搜索项中的词典的勾选状态发生改变，则写入到数据库中。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListViewSearchManage_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            string dictName = e.Item.Text;

            List<DictInfo> checkChangedList = DictsInfoList.Where(i => i.DictName == dictName).ToList();
            DictInfo checkChangedDict = checkChangedList[0];
            int index = DictsInfoList.IndexOf(checkChangedDict);
            bool checkStateInArr = CheckedStateArr[index] == 1;
            if (e.Item.Checked == checkStateInArr)//没有变化，是又进入了TabControl或进行了TabPage切换
                return;

            DictOps dictOp = e.Item.Checked ? DictOps.DictChecked : DictOps.DictUnchecked;
            SetupDictsInfo(checkChangedList, dictOp, FormMain.sqliteInstance);
            IsCheckIndiceChanged = true;
        }
    }
}
