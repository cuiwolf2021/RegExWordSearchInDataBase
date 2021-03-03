using System;
using System.Collections.Generic;
using System.Text;

using System.Data.SQLite;
using System.Data;
using System.IO;
using static Utils.LogWriterInner;

namespace Utils
{
    /// <summary>
    /// 使用SQLite官方的SQLite包，不使用Microsoft提供的Sqlite包。后者不支持GetSchema
    /// </summary>
    public sealed class SqliteSingleton
    {
        private readonly string dbFileFolder;
        private readonly string DbFileNameFullPath;

        private SqliteSingleton(string dbFileFolder, string dbFileName)
        {
            this.dbFileFolder = dbFileFolder;
            DbFileNameFullPath = dbFileFolder + dbFileName;
        }

        private static SqliteSingleton _instance;

        public static SqliteSingleton GetInstance(string dbFileFolder, string dbFileName)
        {
            if (_instance == null)
                _instance = new SqliteSingleton(dbFileFolder, dbFileName);
            return _instance;
        }

        public enum DbStatus
        {
            DbIsOk,
            DbFileNotExist
        }

        /// <summary>
        /// 获取数据库文件的状态
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public DbStatus GetDbFileStatus()
        {
            if (!File.Exists(DbFileNameFullPath))
                return DbStatus.DbFileNotExist;
            return DbStatus.DbIsOk;
        }

        /// <summary>
        /// 创建数据库文件夹（系统API，若存在则不作事情）、创建数据库文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        public void CreateDBFile(string DbFileNameFullPath, string tableName = null, string wholeSql = null)
        {
            Directory.CreateDirectory(dbFileFolder);//创建所有子目录。若已存在，则不作事情

            if (!File.Exists(DbFileNameFullPath))
            {
                SQLiteConnection.CreateFile(DbFileNameFullPath);// 或 File.Create(DbFileNameFullPath);
                //CreateTable(_Dicts_Info_Table, Sql4CreatelDictsInfo);
                if (tableName != null && wholeSql != null)
                    CreateTable(tableName, wholeSql);
            }

        }

        /// <summary>
        /// 删除数据库,若不指定文件全路径，则删除
        /// </summary>
        /// <param name="fileName">文件名</param>
        public void DeleteDBFile(string dbFileFullPath = null)
        {        
            string file2Delete = string.IsNullOrEmpty(dbFileFullPath) ? DbFileNameFullPath : dbFileFullPath;
            if (File.Exists(file2Delete))
            {
                File.Delete(file2Delete);
            }
        }

        /// <summary>
        /// 生成连接字符串,默认文件名为
        /// </summary>
        /// <returns></returns>
        private string CreateConnectionString()
        {
            SQLiteConnectionStringBuilder csb = new SQLiteConnectionStringBuilder();
            csb.DataSource = DbFileNameFullPath;

            string conStr = csb.ToString();
            return conStr;
        }

        /// <summary>
        /// 连接到数据库
        /// </summary>
        /// <returns></returns>
        internal SQLiteConnection GetConnection()
        {
            SQLiteConnection connection = new SQLiteConnection(CreateConnectionString());
            connection.Open();
            return connection;
        }

        /// <summary>
        /// 在指定数据库中创建一个table
        /// </summary>
        /// <param name="sql">sql语句，如：create table highscores (name varchar(20), score int)</param>
        /// <returns>若表已存在或成功新建则返回true，否则返回false</returns>
        public bool CreateTable(string tableName, string sql)
        {
            try
            {
                using SQLiteConnection con = GetConnection();
                List<string> tableNameList = GetTableNames(con);
                if (tableNameList.Contains(tableName))
                    return true;

                SQLiteCommand command = new SQLiteCommand(sql, con);
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                ErrorLog.Insert("CreateTable：【" + sql + "】Err:" + ex);
                return false;
            }
        }

        /// <summary>
        /// 生成创建表的SQL语句。所有列都是TEXT，第一列不为空，且长度不超过255
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnNames"></param>
        /// <returns></returns>
        public static string FormatCreateTableSql(string tableName, string[] columnNames)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"CREATE TABLE [{tableName}] ( ");
            for (int i = 0; i < columnNames.Length; i++)
            {
                if (i == 0)
                    sb.Append($"[{columnNames[i]}] TEXT(255) NOT NULL,");
                else if (i == columnNames.Length - 1)
                    sb.Append($"[{columnNames[i]}] TEXT);");
                else
                    sb.Append($"[{columnNames[i]}] TEXT,");
            }
            return sb.ToString();
        }

        /// <summary>
        /// 一次删除多个表
        /// </summary>
        /// <param name="tableNames"></param>
        /// <returns></returns>
        public bool DeleteTables(List<string> tableNameList)
        {
            try
            {
                using SQLiteConnection con = GetConnection();
                using SQLiteTransaction transaction = con.BeginTransaction();
                foreach (string tableName in tableNameList)
                {
                    string sql = "DROP TABLE IF EXISTS [" + tableName + "]";
                    using SQLiteCommand cmd = new SQLiteCommand(sql, con, transaction);
                    cmd.ExecuteNonQuery();
                }
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                ErrorLog.Insert("Delete " + tableNameList.Count + " TableS,Err:" + ex);
                return false;
            }
        }

        /// <summary>
        /// 在指定数据库中删除一个table
        /// </summary>
        /// <param name="tablename">表名称</param>
        /// <returns></returns>
        public bool DeleteTable(string tableName)
        {
            return DeleteTables(new List<string>(new string[] { tableName }));
        }


        /// <summary>
        /// 获取数据库中所有的表的名字
        /// </summary>
        /// <returns></returns>
        public static List<string> GetTableNames(SQLiteConnection con)
        {
            List<string> tableNames = new List<string>();

            try
            {
                DataTable schemaTable = con.GetSchema("TABLES");
                // 移除数据表中特定的列
                schemaTable.Columns.Remove("TABLE_CATALOG");
                // 设定特定列的序号
                schemaTable.Columns["TABLE_NAME"].SetOrdinal(1);
                foreach (DataRow row in schemaTable.Rows)
                    tableNames.Add(row[1].ToString());

                return tableNames;
            }
            catch (Exception ex)
            {
                ErrorLog.Insert("GetTableNames获取数据库表名字列表出现异常！" + ex);
                return null;
            }
        }


        /// <summary>
        /// 在指定表中添加列
        /// </summary>
        /// <param name="tablename">表名</param>
        /// <param name="columnname">列名</param>
        /// <param name="ctype">列的数值类型</param>
        /// <returns></returns>
        public bool AddColumn(string tablename, string columnname, string ctype)
        {
            try
            {
                using SQLiteConnection con = GetConnection();
                string sql = "ALTER TABLE " + tablename + " ADD COLUMN " + columnname + " " + ctype;
                SQLiteCommand cmd = new SQLiteCommand(sql, con);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                ErrorLog.Insert("修改表 " + tablename + " ADD COLUMN " + columnname + " " + ctype + " Err:" + ex);
                return false;
            }
        }

        /// <summary>
        /// 执行增删改查操作
        /// </summary>
        /// <param name="sql">查询语言</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string sql)
        {
            try
            {
                using SQLiteConnection con = GetConnection();
                SQLiteCommand cmd = new SQLiteCommand(sql, con);
                cmd.ExecuteNonQuery().ToString();
                return 1;
            }
            catch (Exception ex)
            {
                ErrorLog.Insert("！ExecuteNonQuery(" + sql + ")Err:" + ex);
                return 0;
            }
        }

        /// <summary>
        /// 返回一条记录查询
        /// </summary>
        /// <param name="sql">sql查询语言</param>
        /// <returns>返回字符串数组</returns>
        public string[] SqlRow(string sql)
        {
            try
            {
                using SQLiteConnection con = GetConnection();
                SQLiteCommand sqlcmd = new SQLiteCommand(sql, con);//sql语句
                SQLiteDataReader reader = sqlcmd.ExecuteReader();
                if (!reader.Read())
                {
                    return null;
                }
                string[] Row = new string[reader.FieldCount];
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    Row[i] = (reader[i].ToString());
                }
                reader.Close();
                return Row;
            }
            catch (Exception ex)
            {
                ErrorLog.Insert("SqlRow(" + sql + ")Err:" + ex);
                return null;
            }
        }

        /// <summary>
        /// 唯一结果查询
        /// </summary>
        /// <param name="sql">sql查询语言</param>
        /// <returns>返回一个字符串</returns>
        public string SqlOne(string sql)
        {

            try
            {
                using SQLiteConnection con = GetConnection();
                var sqlcmd = new SQLiteCommand(sql, con);//sql语句
                return sqlcmd.ExecuteScalar().ToString();
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 获取一列数据
        /// </summary>
        /// <param name="sql">单列查询</param>
        /// <param name="count">返回结果数量</param>
        /// <returns>返回一个数组</returns>
        public List<string> SqlColumn(string sql)
        {
            try
            {
                List<string> Column = new List<string>();
                using SQLiteConnection con = GetConnection();
                SQLiteCommand sqlcmd = new SQLiteCommand(sql, con);//sql语句
                SQLiteDataReader reader = sqlcmd.ExecuteReader();
                while (reader.Read())
                {
                    Column.Add(reader[0].ToString());
                }
                reader.Close();
                return Column;
            }
            catch (Exception ex)
            {
                ErrorLog.Insert("SqlColumn(" + sql + ")Err:" + ex);
                return null;
            }
        }

        /// <summary>
        /// 运行多个SQL查询语句，得到DataTable的数组
        /// </summary>
        /// <param name="sql">sql查询语言</param>
        /// <returns>返回查询结果集</returns>
        public DataTable[] GetTables(string[] sqls)
        {
            try
            {
                List<DataTable> dtList = new List<DataTable>();

                for (int i = 0; i < sqls.Length; i++)
                {
                    using SQLiteConnection con = GetConnection();
                    var sqlcmd = new SQLiteCommand(sqls[i], con);//sql语句
                    sqlcmd.CommandTimeout = 120;
                    SQLiteDataReader reader = sqlcmd.ExecuteReader();
                    DataTable dt = new DataTable();
                    if (reader != null)
                    {
                        dt.Load(reader, LoadOption.PreserveChanges, null);
                        int rows = dt.Rows.Count;
                        Console.WriteLine("Rows:" + rows);
                    }
                    dtList.Add(dt);
                }
                return dtList.ToArray();
            }
            catch (Exception ex)
            {
                ErrorLog.Insert("SqlReader(" + sqls + ")Err:" + ex);
                return null;
            }

        }

        /// <summary>
        /// 返回记录集查询
        /// </summary>
        /// <param name="sql">sql查询语言</param>
        /// <returns>返回查询结果集</returns>
        public DataTable GetTable(string sql)
        {
            try
            {
                using SQLiteConnection con = GetConnection();
                var sqlcmd = new SQLiteCommand(sql, con);//sql语句
                sqlcmd.CommandTimeout = 120;
                SQLiteDataReader reader = sqlcmd.ExecuteReader();
                DataTable dt = new DataTable();
                if (reader != null)
                {
                    dt.Load(reader, LoadOption.PreserveChanges, null);
                }
                return dt;
            }
            catch (Exception ex)
            {

                ErrorLog.Insert("SqlReader(" + sql + ")Err:" + ex);
                return null;
            }
        }

        /// <summary>
        /// 将DataTable的数据写入到数据库的某个表中
        /// </summary>
        /// <param name="dataTable">包含数据的DataTable</param>
        /// <param name="tableName">数据库中表的名字</param>
        /// <param name="CreateSql">创建表的SQL语句</param>
        /// <param name="doesDeleteOrgTable">是否删除原有数据库表中的数据;若不删除，则进行更新。默认为删除原表</param>
        /// <returns></returns>
        public bool WriteTable2Db(DataTable dataTable, string tableName, string CreateSql = null,bool doesDeleteOrgTable = true)
        {
            try
            {     
                if (!File.Exists(DbFileNameFullPath))
                    CreateDBFile(DbFileNameFullPath);

                string[] colNames = new string[dataTable.Columns.Count];
                for (int i = 0; i < colNames.Length; i++)
                {
                    colNames[i] = dataTable.Columns[i].ColumnName.Trim().Replace("'", "''");
                }
                string createSql = string.IsNullOrEmpty(CreateSql) ? FormatCreateTableSql(tableName, colNames) : CreateSql;
                CreateTable(tableName, createSql);

                using SQLiteConnection con = GetConnection();
                using SQLiteTransaction transaction = con.BeginTransaction();
                if (doesDeleteOrgTable)
                {
                    string dSql = "DELETE FROM [" + tableName+"]";
                    new SQLiteCommand(dSql, con, transaction).ExecuteNonQuery();
                }
                
                foreach (DataRow row in dataTable.Rows)
                {
                    string sql = FormatReplaceSql(tableName, row);
                    using SQLiteCommand sqliteCommand = new SQLiteCommand(sql, con, transaction);
                    sqliteCommand.ExecuteNonQuery();
                }

                transaction.Commit();

                return true;

            }
            catch (Exception ex)
            {
                ErrorLog.Insert("批量数据写入Sqlite失败" + ex.Message);
                return false;
            }
        }
        /// <summary>
        /// 生成SQL语句，将一行数据插入或替换道数据库表中。根据该行数据的列数用SWITCH语句蛮力生成不同SQL语句。
        /// </summary>
        /// <param name="tableName">数据库表名字</param>
        /// <param name="row">准备插入或替换的一行数据</param>
        /// <returns></returns>
        private static string FormatReplaceSql(string tableName, DataRow row)
        {
            string result = "";
            int columnCount = row.ItemArray.Length;

            //TODO : 根据列数写出SQL
            string[] valueArr = new string[columnCount];
            //将单个单引号替换为两个单引号，避免SQL语句语法错误
            for (int i = 0; i < columnCount; i++)
                valueArr[i] = Convert.ToString(row[i]).Trim().Replace("'", "''");

            switch (columnCount)
            {
                case 1:
                    result = string.Format("REPLACE INTO [{0}] VALUES('{1}')",
                        tableName, valueArr[0]);
                    break;
                case 2:
                    result = string.Format("REPLACE INTO [{0}] VALUES('{1}', '{2}')",
                        tableName, valueArr[0], valueArr[1]);
                    break;
                case 3:
                    result = string.Format("REPLACE INTO [{0}] VALUES('{1}', '{2}', '{3}')",
                        tableName, valueArr[0], valueArr[1], valueArr[2]);
                    break;
                case 4:
                    result = string.Format("REPLACE INTO [{0}] VALUES('{1}', '{2}', '{3}', '{4}')",
                        tableName, valueArr[0], valueArr[1], valueArr[2], valueArr[3]);
                    break;
                case 5:
                    result = string.Format("REPLACE INTO [{0}] VALUES('{1}', '{2}', '{3}', '{4}', '{5}')",
                        tableName, valueArr[0], valueArr[1], valueArr[2], valueArr[3], valueArr[4]);
                    break;
                case 6:
                    result = string.Format("REPLACE INTO [{0}] VALUES('{1}', '{2}', '{3}', '{4}', '{5}', '{6}')",
                        tableName, valueArr[0], valueArr[1], valueArr[2], valueArr[3], valueArr[4], valueArr[5]);
                    break;
                case 7:
                    result = string.Format("REPLACE INTO [{0}] VALUES('{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                        tableName, valueArr[0], valueArr[1], valueArr[2], valueArr[3], valueArr[4], valueArr[5], valueArr[6]);
                    break;
                case 8:
                    result = string.Format("REPLACE INTO [{0}] VALUES('{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}')",
                        tableName, valueArr[0], valueArr[1], valueArr[2], valueArr[3], valueArr[4], valueArr[5], valueArr[6], valueArr[7]);
                    break;
            }
            return result;
        }
    }

    #region 记录数据库出错日志
    class LogWriterInner
    {
        /// <summary>
        /// 写入到文件，文件名file_name，内容log_txt
        /// </summary>
        /// <returns></returns>
        public bool Write2File(string txt, string file_name)
        {
            FileInfo fi = new FileInfo(file_name);
            if (!Directory.Exists(fi.DirectoryName))
            {
                Directory.CreateDirectory(fi.DirectoryName);
            }
            txt = DateTime.Now.ToString("HH:mm:ss") + txt;
            try
            {
                using (FileStream sw = new FileStream(file_name, FileMode.Append, FileAccess.Write))
                    if (File.Exists(file_name))
                    {
                        StreamWriter fs = new StreamWriter(sw);
                        // 为文件添加一些文本内容
                        fs.WriteLine(txt);
                        fs.Close();
                        return true;
                    }
                    else
                    {
                        using (StreamWriter fs = new StreamWriter(sw))
                        {
                            fs.WriteLine(txt);
                            fs.Close();
                            return true;
                        }
                    }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        public class ErrorLog
        {
            public static void Insert(string x)
            {
                string err_name = @$"Syslog\\Err_log-{DateTime.Now.Date.ToString("yyyy-MM-dd")}.txt";
                LogWriterInner flog = new LogWriterInner();
                flog.Write2File(Environment.NewLine + x, err_name);
            }
        }
        /// <summary>
        /// 记录操作日志
        /// </summary>
        public class ActionLog
        {
            public static void Insert(string x)
            {
                string act_name = "Syslog\\Act_log" + DateTime.Now.Date.ToString("yyyy-MM-dd") + ".txt";
                LogWriterInner flog = new LogWriterInner();
                flog.Write2File(x, act_name);
            }

        }
    }

    #endregion
}