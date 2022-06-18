using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NLog;
using OfficeOpenXml;

namespace Websbor_PasswordRespondents
{
    class FileRespondents
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public byte[] DataTableToExcel(DataTable dataTable)
        {
            logger.Info("[Вызов метода DataTableToEcxel]");

            var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Список респондентов");
            int rowsCount = dataTable.Rows.Count + 1;

            sheet.Cells["A1"].Value = "Наименование";
            sheet.Cells["B1"].Value = "ОКПО";
            sheet.Cells["C1"].Value = "Пароль";
            sheet.Cells["D1"].Value = "Дата создания";
            sheet.Cells["E1"].Value = "Примечание";

            sheet.Cells[2, 1].LoadFromDataTable(dataTable);

            sheet.View.FreezePanes(2, 1);
            sheet.Cells[1, 1, 1, 5].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            sheet.Cells[1, 1, 1, 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(217, 217, 217));
            sheet.Columns[1].Width = 100;
            sheet.Columns[2].Width = 30;
            sheet.Columns[3].Width = 30;
            sheet.Columns[4].Width = 30;
            sheet.Columns[5].Width = 30;
            sheet.Columns[1, 5].Style.Font.Name = "Times New Roman";
            sheet.Columns[1, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            sheet.Columns[1, 5].Style.Font.Size = 12;
            sheet.Cells[1, 1, 1, 5].Style.Font.Bold = true;
            sheet.Cells[1, 1, 1, 5].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet.Rows[1].Height = 30;
            sheet.Cells[1, 1, rowsCount, 5].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            sheet.Cells[1, 1, rowsCount, 5].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            sheet.Cells[1, 1, rowsCount, 5].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            sheet.Cells[1, 1, rowsCount, 5].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;


            return package.GetAsByteArray();
        }

        public DataTable ReadExcelToDataTable(string pathExcelFile)
        {
            DataTable dataTableRespondents = new DataTable();
            List<string> loadResult=new List<string>();

            dataTableRespondents.Columns.AddRange(new DataColumn[5]
            { new DataColumn("name", typeof(string)),
            new DataColumn("okpo", typeof(string)),
            new DataColumn ("password", typeof(string)),
            new DataColumn("datecreate", typeof(string)),
            new DataColumn("comment", typeof(string))
            });

            try
            {
                ExcelPackage package = new ExcelPackage(pathExcelFile);
                ExcelWorksheet sheet = package.Workbook.Worksheets[0];

                if (sheet.Dimension == null)
                {
                    return dataTableRespondents;
                }

                //List<string> columnNames = new List<string>();

                //int currentColumn = 1;

                //зацикливаем все столбцы на листе и добавляем их в таблицу данных
                //foreach (var cell in sheet.Cells[1, 1, 1, sheet.Dimension.End.Column])
                //{
                //    string columnName = cell.Text.Trim();
                //    //проверьте, был ли предыдущий заголовок пустым, и добавьте его, если он был
                //    if (cell.Start.Column != currentColumn)
                //    {
                //        columnNames.Add("Header_" + currentColumn);
                //        dataTableRespondents.Columns.Add("Header_" + currentColumn);
                //        currentColumn++;
                //    }
                //    //добавьте имя столбца в список для подсчета дубликатов
                //    columnNames.Add(columnName);
                //    //подсчитайте повторяющиеся имена столбцов и сделайте их уникальными, чтобы избежать исключения
                //    //Столбец с именем "Имя" уже принадлежит этому DataTable.
                //    int occurrences = columnNames.Count(x => x.Equals(columnName));
                //    if (occurrences > 1)
                //    {
                //        columnName = columnName + "_" + occurrences;
                //    }
                //    //добавить столбец в таблицу данных
                //    dataTableRespondents.Columns.Add(columnName);
                //    currentColumn++;
                //}
                // начать добавлять содержимое файла Excel в таблицу данных


                for (int i = 2; i <= sheet.Dimension.End.Row; i++)
                {
                    var row = sheet.Cells[i, 1, i, sheet.Dimension.End.Column];
                    DataRow newRow = dataTableRespondents.NewRow();

                    foreach (var cell in row)
                    {
                        newRow[cell.Start.Column - 1] = cell.Text.Trim();
                    }

                    if (!string.IsNullOrWhiteSpace(newRow["name"].ToString()) && !string.IsNullOrWhiteSpace(newRow["okpo"].ToString()))
                    {
                        if (long.TryParse(newRow["okpo"].ToString(), out long result))
                        {
                            if (string.IsNullOrWhiteSpace(newRow["datecreate"].ToString()))
                            {
                                newRow["datecreate"] = null;
                            }

                            dataTableRespondents.Rows.Add(newRow);
                        }
                        else
                        {
                            loadResult.Add($"Значение ОКПО {newRow["okpo"]} не является числом");
                        }
                    }
                }

                ProtocolFileDB.ProtocolLoadFileToDB(loadResult);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                logger.Error(ex.Message);
            }          

            return dataTableRespondents;
        }

        public byte[] ShemaExcelToDB()
        {
            var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Список респондентов");

            sheet.Cells["A1"].Value = "Наименование";
            sheet.Cells["B1"].Value = "ОКПО";
            sheet.Cells["C1"].Value = "Пароль";
            sheet.Cells["D1"].Value = "Дата создания";
            sheet.Cells["E1"].Value = "Примечание";

            sheet.View.FreezePanes(2, 1);
            sheet.Cells[1, 1, 1, 5].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            sheet.Cells[1, 1, 1, 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(217, 217, 217));
            sheet.Columns[1].Width = 100;
            sheet.Columns[2].Width = 30;
            sheet.Columns[3].Width = 30;
            sheet.Columns[4].Width = 30;
            sheet.Columns[5].Width = 30;
            sheet.Columns[1, 5].Style.Font.Name = "Times New Roman";
            sheet.Columns[1, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            sheet.Columns[1, 5].Style.Font.Size = 12;
            sheet.Cells[1, 1, 1, 5].Style.Font.Bold = true;
            sheet.Cells[1, 1, 1, 5].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet.Rows[1].Height = 30;
            sheet.Cells[1, 1, 1, 5].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            sheet.Cells[1, 1, 1, 5].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            sheet.Cells[1, 1, 1, 5].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            sheet.Cells[1, 1, 1, 5].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            sheet.Columns[1, 5].Style.Numberformat.Format = "@";

            return package.GetAsByteArray();
        }
        //public DataTable LoadFile()
        //{
        //    logger.Info("[Вызов метода LoadFile]");
            
        //    DataTable dataTable = new DataTable();

        //    try
        //    {
        //        string fileExtension;
        //        string filePath;
        //        Func<string, DataTable> readFile = null;
        //        System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
        //        fileDialog.Filter = "*.txt|*.txt|*.xlsx|*.xlsx";
        //        fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        //        if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //        {
        //            filePath = fileDialog.FileName;
        //            fileExtension = Path.GetExtension(filePath);

        //            if (fileExtension == ".txt")
        //            {
        //                readFile = ReadTextToDataTable;
        //            }
        //            else if (fileExtension == ".xlsx")
        //            {
        //                readFile = ReadExcelToDataTable;
        //            }

        //            dataTable = readFile?.Invoke(filePath);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
        //        logger.Error(ex.Message);
        //    }

        //    return dataTable;
        //}
        public DataTable ReadTextToDataTable(string pathFile)
        {
            logger.Info("[Вызов метода ReadTextFile]");

            DataTable datafileopen = new DataTable();
            try
            {
                datafileopen.Columns.AddRange(new DataColumn[5]
                {      new DataColumn("name", typeof(string)),
                   new DataColumn("okpo", typeof(string)),
                   new DataColumn("password",typeof(string)),
                   new DataColumn("datecreate",typeof(string)),
                   new DataColumn("comment",typeof(string))
                });

                string[] vs = File.ReadAllLines(pathFile);

                foreach (string row in vs)
                {

                    if (!string.IsNullOrEmpty(row))
                    {
                        datafileopen.Rows.Add();
                        int i = 0;
                        foreach (string cell in row.Split('#'))
                        {
                            datafileopen.Rows[datafileopen.Rows.Count - 1][i] = cell;
                            i++;
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                logger.Error(ex.Message + ex.Message);
            }

            return datafileopen;
        }


    }
}
