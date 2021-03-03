using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using static Utils.LogWriterInner;

namespace RegExWordSearch
{
    public static class DictHelper
    {
        #region 帮助文件
        public const string RegexHelpFile = "正则表达式简介.docx";
        public const string AppHelpFile = "程序使用帮助.docx";
        //public const string SearchResultFile = "查询结果.txt";
        public static string GetSearchResultFileName()
        {
            DateTime now = DateTime.Now;           
            return $"查询结果-{now.Year:4}-{now.Month:2}-{now.Day:2}.txt";
        }
        #endregion

        #region 词相关
        public readonly struct WordInfo
        {
            public readonly string Word;    //单词 
            public readonly string Class;   //词性
            public readonly string Meaning; //释义
        }
        public static readonly string[] WordInfoColumns = new string[3] { "单词", "词性", "释义" };

        public static string FormatCreateWordInfoSql(string tableName)
        {
            return $@"CREATE TABLE [{tableName}](  [{
                WordInfoColumns[0]}] TEXT NOT NULL UNIQUE,  [{
                WordInfoColumns[1]}] TEXT ,  [{
                WordInfoColumns[2]}] TEXT);";
        }
        #endregion

        #region Dict相关
        /// <summary>
        /// 对词典的操作：添加词典，删除一个或多个词典，词典优先级提高，词典优先级降低
        /// 
        /// </summary>
        public enum DictOps
        {
            AddDict,
            RemoveDicts,
            DictToHigher,
            DictToLower,
            DictChecked,
            DictUnchecked
        }

        private const int DictInfoItemCount = 3;
        public class DictInfo
        {
            public string DictName;
            public int WordCount;
            public string BriefIntro;
            public int Priority;     //优先级，数字越小则优先级越高。
            public int IsChecked;   //是否已勾选，用在搜索中:为1，代表勾选；0代表未勾选。
        }

        public static readonly string[] DictInfoColumns = new string[5] { "词典名字", "单词数量", "词典简介", "优先顺序", "是否勾选" };

        public static readonly string DbFileFolder = Environment.CurrentDirectory + @"/Data/";
        public static readonly string DbFileName = "Dict4RegexSearch.db";
        public static readonly string DbFileNameFullPath = DbFileFolder + DbFileName;
        private static readonly string _Dicts_Info_Table = "_DICTS_INFO";
        private static readonly string Sql2CreatelDictsInfo = $@"CREATE TABLE [{_Dicts_Info_Table}](  [{
            DictInfoColumns[0]}] TEXT NOT NULL UNIQUE,  [{
            DictInfoColumns[1]}] INT NOT NULL,  [{
            DictInfoColumns[2]}] TEXT NOT NULL,  [{
            DictInfoColumns[3]}] INT NOT NULL UNIQUE,[{
            DictInfoColumns[4]}] INT NOT NULL);";

        private static List<DictInfo> dictinfoList = null;

        /// <summary>
        /// 设置_Dicts_Info的信息，每当有词典的导入、卸载、向上、向下时，都要设置
        /// </summary>
        /// <param name="tableNames"></param>
        /// <param name="dictOop"></param>
        public static int SetupDictsInfo(List<DictInfo> toOpDictInfos, DictOps dictOp, SqliteSingleton sqliteInstance)
        {
            if (toOpDictInfos.Count == 0)
            {
                return 0;
            }
            int resultCount = 1;
            switch (dictOp)
            {
                case DictOps.AddDict:
                    DictInfo toAddDictInfo = toOpDictInfos[0];                
                    toAddDictInfo.Priority = dictinfoList.Count + 1;//其他信息已经设置
                    dictinfoList.Add(toAddDictInfo);
                    break;
                case DictOps.RemoveDicts:
                    foreach (DictInfo di in toOpDictInfos) //其实只允许选中一个
                        dictinfoList.Remove(di);
                    for (int i = 0; i < dictinfoList.Count; i++)
                    {
                        DictInfo di = dictinfoList[i];
                        di.Priority = i + 1;
                    }
                    resultCount = toOpDictInfos.Count;
                    break;
                case DictOps.DictToHigher://TODO：暂时只向上移动一个

                    DictInfo toHigherDictInfo = toOpDictInfos[0];
                    toHigherDictInfo.Priority -= 1;
                    DictInfo adjecent2Higher = GetAdjecentDictInfo(dictinfoList, toHigherDictInfo, Direction.ToHigher);
                    adjecent2Higher.Priority += 1;
                    Swap(dictinfoList, toHigherDictInfo, adjecent2Higher);
                    break;
                case DictOps.DictToLower://TODO：暂时只向下移动一个
                    DictInfo toLowerrDictInfo = toOpDictInfos[0];
                    DictInfo adjecent2Lower = GetAdjecentDictInfo(dictinfoList, toLowerrDictInfo, Direction.ToLower);
                    toLowerrDictInfo.Priority += 1;
                    adjecent2Lower.Priority -= 1;
                    Swap(dictinfoList, toLowerrDictInfo, adjecent2Lower);
                    break;
                case DictOps.DictChecked:
                    DictInfo checkedDictInfo = toOpDictInfos[0];
                    checkedDictInfo.IsChecked = 1;
                    break;
                case DictOps.DictUnchecked:
                    DictInfo uncheckedDictInfo = toOpDictInfos[0];
                    uncheckedDictInfo.IsChecked = 0;
                    break;
            }

            DataTable dt = ConvertDictInfoList(dictinfoList);
            sqliteInstance.WriteTable2Db(dt, _Dicts_Info_Table, Sql2CreatelDictsInfo);
            return resultCount;
        }

        private enum Direction
        {
            ToHigher,
            ToLower

        }
        private static DictInfo GetAdjecentDictInfo(List<DictInfo> diList, DictInfo me, Direction direction)
        {
            DictInfo di;
            int index = diList.IndexOf(me);
            if (direction == Direction.ToHigher)
            {
                di = diList[index - 1];
            }
            else
                di = diList[index + 1];
            return di;

        }
        private static bool Swap(List<DictInfo> diList, DictInfo di1, DictInfo di2)
        {
            int pos1 = diList.IndexOf(di1);
            int pos2 = diList.IndexOf(di2);
            diList[pos1] = di2;
            diList[pos2] = di1;
            return true;
        }

        private static DataTable ConvertDictInfoList(List<DictInfo> diList)
        {
            DataTable dt = new DataTable();
            foreach (string col in DictInfoColumns)
                dt.Columns.Add(col);
            foreach (DictInfo di in diList)
            {
                DataRow row = dt.NewRow();
                row[0] = di.DictName;
                row[1] = di.WordCount;
                row[2] = di.BriefIntro;
                row[3] = di.Priority;
                row[4] = di.IsChecked;
                dt.Rows.Add(row);

            }
            return dt;
        }

        /// <summary>
        /// 读取数据库中 _Dicts_Info 的信息
        /// </summary>
        /// <returns></returns>
        public static List<DictInfo> GetDictsInfo(SqliteSingleton sqliteInstance)
        {
            try
            {
                if (dictinfoList != null)
                    return dictinfoList;

                dictinfoList = new List<DictInfo>();

                using SQLiteConnection con = sqliteInstance.GetConnection();
                string sql = $"SELECT * FROM [{_Dicts_Info_Table}]";
                var sqlcmd = new SQLiteCommand(sql, con);//sql语句
                SQLiteDataReader reader = sqlcmd.ExecuteReader();
                DataTable dt = new DataTable();
                if (reader != null)
                {
                    dt.Load(reader, LoadOption.PreserveChanges, null);
                }

                int rowCount = dt.Rows.Count;//行数
                int columnCount = dt.Columns.Count;//列数
                if (columnCount != DictInfoItemCount)
                {
                    ErrorLog.Insert("GetDictsInfo 得到的列数与结构体不匹配");
                }
                for (int i = 0; i < rowCount; i++)
                {
                    DictInfo di = new DictInfo();
                    di.DictName = dt.Rows[i][0].ToString();
                    di.WordCount = int.Parse(dt.Rows[i][1].ToString());
                    di.BriefIntro = dt.Rows[i][2].ToString();
                    di.Priority = int.Parse(dt.Rows[i][3].ToString());
                    di.IsChecked = int.Parse(dt.Rows[i][4].ToString());
                    dictinfoList.Add(di);
                }

                return dictinfoList;
            }
            catch (Exception ex)
            {
                ErrorLog.Insert("GetTableNames获取数据库表名字列表出现异常！" + ex);
                return null;
            }
        }

        /// <summary>
        /// 扩展方法，从
        /// </summary>
        /// <param name="infoList"></param>
        /// <param name="dictName"></param>
        /// <returns></returns>
        public static bool Contains(this List<DictInfo> infoList, string dictName)
        {
            if (infoList == null || string.IsNullOrEmpty(dictName))
                return false;

            foreach (DictInfo di in infoList)
                if (di.DictName == dictName)
                    return true;
            return false;
        }

        #endregion
    }
}
