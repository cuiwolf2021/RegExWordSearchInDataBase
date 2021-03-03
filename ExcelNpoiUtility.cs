using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;

namespace Utils
{
    /// <summary>
    /// 工具类，利用NPOI。可读取EXCEL文件到DataTable；也可将DataTable写入到EXCEL文件。
    /// </summary>
    public class ExcelNpoiUtility
    {

        /// <summary>
        /// 将excel中的数据导入到DataTable中
        /// </summary>
        /// <param name="fileWholePath">待处理的EXCEL文件全路径</param>
        /// <param name="sheetName">EXCEL工作薄中工作表的名称</param>
        /// <param name="isFirstRowHeaderLine">工作表第一行是否是DataTable的列名</param>
        /// <param name="trim">是否去除获取的单元格内容前后的空格</param>
        /// <returns>返回的DataTable</returns>
        public static DataTable ExcelToDataTableNpoi(string fileWholePath, string sheetName = null, bool isFirstRowHeaderLine = true, bool trim = true)
        {
            ISheet sheet = null;
            DataTable dataTable = new DataTable();

            try
            {
                IWorkbook workbook = null;
                if (!File.Exists(fileWholePath))
                {
                    Console.WriteLine("ExcelToDataTableNpoi 指定的EXCEL文件不存在。");
                    return null;
                }

                var fs = new FileStream(fileWholePath, FileMode.Open, FileAccess.Read);
                workbook = WorkbookFactory.Create(fs); //旧的方法需要判断后缀名，.xlsx 则 new XSSFWorkbook(fs)；.xls则new HSSFWorkbook(fs)

                if (workbook == null)
                {
                    Console.WriteLine("ExcelToDataTableNpoi 读取错误,请确认文件类型");
                    return null;
                }

                //如果有指定工作表名称，则按名字读取工作表
                if (!string.IsNullOrEmpty(sheetName))
                {
                    sheet = workbook.GetSheet(sheetName);
                }

                //如果没有找到指定的sheetName对应的sheet，或者根据名字读取失败，则尝试获取第一个sheet
                if (sheet == null)
                {
                    sheet = workbook.GetSheetAt(0);
                }

                if (sheet == null)
                {
                    Console.WriteLine("ExcelToDataTableNpoi 读取错误,读不出工作表。");
                    return null;
                }

                IRow firstRow = sheet.GetRow(0);
                int startRow = sheet.FirstRowNum;
                int cellCount = firstRow.LastCellNum; //一行最后一个cell的编号 即总的列数

                if (isFirstRowHeaderLine)
                {//第一行是标题行，读取标题行，设置DataTable的各列名字
                    for (int i = firstRow.FirstCellNum; i < cellCount; i++)
                    {
                        ICell cell = firstRow.GetCell(i);
                        if (cell != null)
                        {
                            string cellValue = cell.StringCellValue;
                            if (cellValue != null)
                            {
                                DataColumn column = new DataColumn(cellValue);
                                dataTable.Columns.Add(column);
                            }
                        }
                    }
                    startRow = sheet.FirstRowNum + 1;
                }

                //最后一列的标号
                int rowCount = sheet.LastRowNum;
                for (int i = startRow; i <= rowCount; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null)
                        continue; //没有数据的行默认是null　　　　　　　

                    DataRow dataRow = dataTable.NewRow();
                    for (int j = row.FirstCellNum; j < cellCount; j++)
                    {
                        if (row.GetCell(j) != null) //同理，没有数据的单元格都默认是null
                        {
                            string tmp = row.GetCell(j).ToString();
                            if (trim)
                                tmp = tmp.Trim();
                            dataRow[j] = tmp;
                        }
                    }
                    dataTable.Rows.Add(dataRow);
                }

                return dataTable;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ExcelToDataTableNpoi出现异常: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 将DataTable写入到EXCEL文件。
        /// </summary>
        /// <param name="fileWholePath">EXCEL文件完整路径</param>
        /// <param name="dt">需要写入的数据DataTable</param>
        /// <returns>是否成功写入</returns>
        public static bool DataTableToExcelNpoi(string fileWholePath, DataTable dt)
        {
            try
            {
                if (dt != null && dt.Rows.Count > 0)
                {
                    IWorkbook workbook = new XSSFWorkbook(); 

                    ISheet sheet = workbook.CreateSheet("Sheet1");
                    int rowCount = dt.Rows.Count;//行数
                    int columnCount = dt.Columns.Count;//列数

                    //设置列头
                    IRow row = sheet.CreateRow(0);
                    ICell cell;
                    for (int c = 0; c < columnCount; c++)
                    {
                        cell = row.CreateCell(c);
                        cell.SetCellValue(dt.Columns[c].ColumnName);
                    }

                    //设置每行每列的单元格,
                    for (int i = 0; i < rowCount; i++)
                    {
                        row = sheet.CreateRow(i + 1);
                        for (int j = 0; j < columnCount; j++)
                        {
                            cell = row.CreateCell(j);//excel第二行开始写入数据
                            cell.SetCellValue(dt.Rows[i][j].ToString());
                        }
                    }
                    using (FileStream fs = File.OpenWrite(fileWholePath))
                        workbook.Write(fs);//向打开的这个xls文件中写入数据
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DataTableToExcelNpoi 出现异常: " + ex.Message);
                return false;
            }
        }

        #region  https://github.com/dotnetcore/NPOI 写入到EXCEL代码示例。原代码无方法名
        public static void Write2Excel()
        {
            var newFile = @"newbook.core.xlsx";

            using (var fs = new FileStream(newFile, FileMode.Create, FileAccess.Write))
            {

                IWorkbook workbook = new XSSFWorkbook();

                ISheet sheet1 = workbook.CreateSheet("Sheet1");

                sheet1.AddMergedRegion(new CellRangeAddress(0, 0, 0, 10));
                var rowIndex = 0;
                IRow row = sheet1.CreateRow(rowIndex);
                row.Height = 30 * 80;
                row.CreateCell(0).SetCellValue("this is content");
                sheet1.AutoSizeColumn(0);
                rowIndex++;

                var sheet2 = workbook.CreateSheet("Sheet2");
                var style1 = workbook.CreateCellStyle();
                style1.FillForegroundColor = HSSFColor.Blue.Index2;
                style1.FillPattern = FillPattern.SolidForeground;

                var style2 = workbook.CreateCellStyle();
                style2.FillForegroundColor = HSSFColor.Yellow.Index2;
                style2.FillPattern = FillPattern.SolidForeground;

                var cell2 = sheet2.CreateRow(0).CreateCell(0);
                cell2.CellStyle = style1;
                cell2.SetCellValue(0);

                cell2 = sheet2.CreateRow(1).CreateCell(0);
                cell2.CellStyle = style2;
                cell2.SetCellValue(1);

                workbook.Write(fs);
            }
        }


        #endregion


        #region 一个写入固定内容到指定文件的函数
        public void WriteToExcel(string filePath)
        {
            //创建工作薄  
            IWorkbook wb;
            string extension = System.IO.Path.GetExtension(filePath);
            //根据指定的文件格式创建对应的类
            if (extension.Equals(".xls"))
            {
                wb = new HSSFWorkbook();
            }
            else
            {
                wb = new XSSFWorkbook();
            }

            ICellStyle style1 = wb.CreateCellStyle();//样式
            style1.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;//文字水平对齐方式
            style1.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;//文字垂直对齐方式
                                                                                  //设置边框
            style1.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            style1.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            style1.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            style1.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            style1.WrapText = true;//自动换行

            ICellStyle style2 = wb.CreateCellStyle();//样式
            IFont font1 = wb.CreateFont();//字体
            font1.FontName = "楷体";
            font1.Color = HSSFColor.Red.Index;//字体颜色
            font1.IsBold = true;//字体加粗样式
            style2.SetFont(font1);//样式里的字体设置具体的字体样式
                                  //设置背景色
            style2.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Yellow.Index;
            style2.FillPattern = FillPattern.SolidForeground;
            style2.FillBackgroundColor = NPOI.HSSF.Util.HSSFColor.Yellow.Index;
            style2.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;//文字水平对齐方式
            style2.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;//文字垂直对齐方式

            ICellStyle dateStyle = wb.CreateCellStyle();//样式
            dateStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;//文字水平对齐方式
            dateStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;//文字垂直对齐方式
                                                                                     //设置数据显示格式
            IDataFormat dataFormatCustom = wb.CreateDataFormat();
            dateStyle.DataFormat = dataFormatCustom.GetFormat("yyyy-MM-dd HH:mm:ss");

            //创建一个表单
            ISheet sheet = wb.CreateSheet("Sheet0");
            //设置列宽
            int[] columnWidth = { 10, 10, 20, 10 };
            for (int i = 0; i < columnWidth.Length; i++)
            {
                //设置列宽度，256*字符数，因为单位是1/256个字符
                sheet.SetColumnWidth(i, 256 * columnWidth[i]);
            }

            //测试数据
            int rowCount = 3, columnCount = 4;
            object[,] data = {
                {"列0", "列1", "列2", "列3"},
                {"", 400, 5.2, 6.01},
                {"", true, "2014-07-02", DateTime.Now}
                    //日期可以直接传字符串，NPOI会自动识别
                    //如果是DateTime类型，则要设置CellStyle.DataFormat，否则会显示为数字
            };

            IRow row;
            ICell cell;

            for (int i = 0; i < rowCount; i++)
            {
                row = sheet.CreateRow(i);//创建第i行
                for (int j = 0; j < columnCount; j++)
                {
                    cell = row.CreateCell(j);//创建第j列
                    cell.CellStyle = j % 2 == 0 ? style1 : style2;
                    //根据数据类型设置不同类型的cell
                    object obj = data[i, j];
                    SetCellValue(cell, data[i, j]);
                    //如果是日期，则设置日期显示的格式
                    if (obj.GetType() == typeof(DateTime))
                    {
                        cell.CellStyle = dateStyle;
                    }
                    //如果要根据内容自动调整列宽，需要先setCellValue再调用
                    //sheet.AutoSizeColumn(j);
                }
            }

            //合并单元格，如果要合并的单元格中都有数据，只会保留左上角的
            //CellRangeAddress(0, 2, 0, 0)，合并0-2行，0-0列的单元格
            CellRangeAddress region = new CellRangeAddress(0, 2, 0, 0);
            sheet.AddMergedRegion(region);

            try
            {
                FileStream fs = File.OpenWrite(filePath);
                wb.Write(fs);//向打开的这个Excel文件中写入表单并保存。  
                fs.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
        #endregion

        #region 读取和设置单元格的值类型，暂不使用
        /// <summary>
        ///  获取cell的数据，并设置为对应的数据类型
        /// </summary>
        public static object GetCellValue(ICell cell)
        {
            object value = null;
            try
            {
                if (cell.CellType != CellType.Blank)
                {
                    switch (cell.CellType)
                    {
                        case CellType.Numeric:
                            // Date comes here
                            if (DateUtil.IsCellDateFormatted(cell))
                            {
                                value = cell.DateCellValue;
                            }
                            else
                            {
                                // Numeric type
                                value = cell.NumericCellValue;
                            }
                            break;
                        case CellType.Boolean:
                            // Boolean type
                            value = cell.BooleanCellValue;
                            break;
                        case CellType.Formula:
                            value = cell.CellFormula;
                            break;
                        default:
                            // String type
                            value = cell.StringCellValue;
                            break;
                    }
                }
            }
            catch (Exception)
            {
                value = "";
            }

            return value;
        }

        /// <summary>
        /// https://www.cnblogs.com/restran/p/3889479.html
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="obj"></param>
        public static void SetCellValue(ICell cell, object obj)
        {
            if (obj.GetType() == typeof(int))
            {
                cell.SetCellValue((int)obj);
            }
            else if (obj.GetType() == typeof(double))
            {
                cell.SetCellValue((double)obj);
            }
            else if (obj.GetType() == typeof(IRichTextString))
            {
                cell.SetCellValue((IRichTextString)obj);
            }
            else if (obj.GetType() == typeof(string))
            {
                cell.SetCellValue(obj.ToString());
            }
            else if (obj.GetType() == typeof(DateTime))
            {
                cell.SetCellValue((DateTime)obj);
            }
            else if (obj.GetType() == typeof(bool))
            {
                cell.SetCellValue((bool)obj);
            }
            else
            {
                cell.SetCellValue(obj.ToString());
            }
        }
        #endregion
    }
}
