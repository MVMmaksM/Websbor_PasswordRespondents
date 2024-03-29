﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Reflection;
using NLog;
using Path = System.IO.Path;

namespace Websbor_PasswordRespondents
{
    public partial class MainWindow : Window
    {
        string connectionString;
        string sqlQueryGetAllData = "SELECT * FROM [Password]";
        string sqlQuerySaveallData = "SELECT name, okpo, password, datecreate, comment  FROM [Password]";
        private static Logger logger = LogManager.GetCurrentClassLogger();
        NameValueCollection allAppSettings;
        public Version version;
        private DBWork dataBaseWork;
        private FileRespondents readFileRespondents;
        private DataTable tableRespondentsFromFile;
        public MainWindow()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            allAppSettings = ConfigurationManager.AppSettings;
            dgDataPasswords.CanUserResizeRows = Convert.ToBoolean(allAppSettings["CanUserResizeRows"]);
            dgDataPasswords.CanUserResizeColumns = Convert.ToBoolean(allAppSettings["CanUserResizeColumns"]);
            dgDataPasswords.CanUserReorderColumns = Convert.ToBoolean(allAppSettings["CanUserReorderColumns"]);
            version = Assembly.GetExecutingAssembly().GetName().Version;
            this.Title += $" v{version.Major}.{version.Minor} [build {version.Build}]";
        }
        private void Websbor_PasswordRespondents_Window_Loded(object sender, RoutedEventArgs e)
        {
            ProtocolFileDB.CreateDirectoryProtocol();

            dataBaseWork = new DBWork(connectionString);

            if (dataBaseWork.DbUserExist())
            {
                dataBaseWork.GetShemaTableRespondents();
                dgDataPasswords.ItemsSource = dataBaseWork.tableRespondents.DefaultView;
            }
            else
            {
                MenuFile.IsEnabled = false;
                MenuDB.IsEnabled = false;
                MenuAdmUser.IsEnabled = false;
                GroupBoxSearch.IsEnabled = false;
                GroupBoxRedact.IsEnabled = false;
                dgDataPasswords.IsEnabled = false;
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgDataPasswords.SelectedItems != null)
                {
                    for (int i = 0; i < dgDataPasswords.SelectedItems.Count; i++)
                    {
                        DataRowView datarowView = dgDataPasswords.SelectedItems[i] as DataRowView;
                        if (datarowView != null)
                        {
                            DataRow dataRow = (DataRow)datarowView.Row;
                            dataRow.Delete();
                        }
                    }
                }

                dataBaseWork.UpdateDB();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                logger.Error(ex.Message + ex.StackTrace);
            }
        }

        private void dgDataPasswords_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void ButtonGetAllData_Click(object sender, RoutedEventArgs e)
        {
            dataBaseWork.GetDataDBtoTableRespondents(sqlQueryGetAllData);
        }

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            string search = dataBaseWork.CreateStringSeaarch(GridSearch.Children.OfType<TextBox>());
            if (search != null)
            {
                dataBaseWork.GetDataDBtoTableRespondents($"SELECT * FROM[Password] WHERE {search}");
            }

            //if (RadioButtonOKPO.IsChecked == true & !string.IsNullOrWhiteSpace(TxtBoxSearch.Text))
            //{
            //    dataBaseWork.GetDataDBtoTableRespondents($"SELECT* FROM[Password] WHERE okpo LIKE '%{TxtBoxSearch.Text}%'");
            //}
            //else if (RadioButtonName.IsChecked == true & !string.IsNullOrWhiteSpace(TxtBoxSearch.Text))
            //{
            //    dataBaseWork.GetDataDBtoTableRespondents($"SELECT* FROM[Password] WHERE name LIKE '%{TxtBoxSearch.Text}%'");
            //}
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            dataBaseWork.UpdateDB();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItemLoad_Click(object sender, RoutedEventArgs e)
        {
            readFileRespondents = new FileRespondents();
            tableRespondentsFromFile = new DataTable();
            string fileExtension;
            string filePath;
            Func<string, DataTable> readFile = null;
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "*.txt|*.txt|*.xlsx|*.xlsx";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            try
            {
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;

                    fileExtension = Path.GetExtension(filePath);

                    if (fileExtension == ".txt")
                    {
                        readFile = readFileRespondents.ReadTextToDataTable;
                    }
                    else if (fileExtension == ".xlsx")
                    {
                        readFile = readFileRespondents.ReadExcelToDataTable;
                    }

                    tableRespondentsFromFile = readFile?.Invoke(filePath);

                    if (tableRespondentsFromFile.Rows.Count > 0)
                    {
                        dataBaseWork.LoadDataFileToDB(tableRespondentsFromFile);
                    }
                    else if (tableRespondentsFromFile.Rows.Count == 0)
                    {
                        if (MessageBox.Show("Нет данных для загрузки \nОткрыть протокол? ", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                            Process.Start(ProtocolFileDB.pathProtocolFile + ProtocolFileDB.fileNameProtocol);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                logger.Error(ex.Message);
            }
        }

        private void MenuItemLoadWebSbor_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItemSaveAllData_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog;
            FileRespondents fileRespondents;
            DataTable dataTable;

            try
            {
                fileRespondents = new FileRespondents();
                dataTable = dataBaseWork.GetDataDB(sqlQuerySaveallData);
                var file = fileRespondents.DataTableToExcel(dataTable);


                saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                saveFileDialog.Filter = "|*.xlsx";
                saveFileDialog.FileName = "Список респондентов";

                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.WriteAllBytes(saveFileDialog.FileName, file);

                    MessageBox.Show($"Сохранено записей: {dataTable.Rows.Count}", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                logger.Error(ex.Message);
            }
        }

        private void MenuItemOPenLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start($"{Environment.CurrentDirectory}\\logs\\{DateTime.Now:yyyy-MM-dd}.log");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MenuItemOpenDirectoryLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start($"{Environment.CurrentDirectory}\\logs");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MenuItemDeleteAllLog_Click(object sender, RoutedEventArgs e)
        {
            string[] nameLogFiles = Directory.GetFiles($"{Environment.CurrentDirectory}\\logs");
            int count = 0;

            if (MessageBox.Show("Удалить все log-файлы?", "Сообщение", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
            {
                for (int i = 0; i < nameLogFiles.Length; i++)
                {
                    File.Delete(nameLogFiles[i]);
                    count++;
                }

                MessageBox.Show($"Удалено файлов: {count}", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            logger.Info("[Завершение программы]");
        }

        private void TxtBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {               
                string search = dataBaseWork.CreateStringSeaarch(GridSearch.Children.OfType<TextBox>());
                if (search!=null)
                {                
                    dataBaseWork.GetDataDBtoTableRespondents($"SELECT * FROM[Password] WHERE {search}");
                }

                //if (RadioButtonOKPO.IsChecked == true)
                //{
                //    dataBaseWork.GetDataDBtoTableRespondents($"SELECT* FROM[Password] WHERE okpo LIKE '%{TxtBoxSearch.Text}%'");
                //}
                //else if (RadioButtonName.IsChecked == true)
                //{
                //    dataBaseWork.GetDataDBtoTableRespondents($"SELECT* FROM[Password] WHERE name LIKE '%{TxtBoxSearch.Text}%'");
                //}
            }
        }

        private void MenuItemSaveCurrentData_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog;
            FileRespondents fileRespondents;
            DataTable dataTable;

            try
            {
                if (dataBaseWork.tableRespondents.Rows.Count != 0)
                {
                    fileRespondents = new FileRespondents();

                    dataTable = dataBaseWork.tableRespondents.Copy();
                    dataTable.PrimaryKey = null;
                    dataTable.Columns.Remove("ID");
                    dataTable.Columns.Remove("usercreate"); // переделать
                    dataTable.Columns.Remove("dateupdate");
                    dataTable.Columns.Remove("userupdate");
                    var file = fileRespondents.DataTableToExcel(dataTable);

                    saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                    saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    saveFileDialog.Filter = "|*.xlsx";
                    saveFileDialog.FileName = "Список выбранных респондентов";

                    if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        File.WriteAllBytes(saveFileDialog.FileName, file);

                        MessageBox.Show($"Сохранено записей: {dataTable.Rows.Count}", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Нет записей для сохранения", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                logger.Error(ex.Message);
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            dataBaseWork.DeleteAllRowsTable();
        }

        private void MenuItemOpenSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.Show();
        }

        private void MenuItemShemaEcxel_Click(object sender, RoutedEventArgs e)
        {
            FileRespondents excelShema;
            System.Windows.Forms.SaveFileDialog saveFileDialog;

            try
            {
                excelShema = new FileRespondents();
                var shema = excelShema.ShemaExcelToDB();

                saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                saveFileDialog.Filter = "|*.xlsx";
                saveFileDialog.FileName = "Шаблон загрузки";

                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.WriteAllBytes(saveFileDialog.FileName, shema);

                    MessageBox.Show($"Шаблон успешно сохранен", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                logger.Error(ex.Message);
            }
        }

        private void MenuItemAddUser_Click(object sender, RoutedEventArgs e)
        {
            WindowAddUser windowAddUSer = new WindowAddUser();
            windowAddUSer.Owner = this;
            windowAddUSer.Show();
        }

        private void ButtonClearDatagrid_Click(object sender, RoutedEventArgs e)
        {

            if (dataBaseWork.tableRespondents.Rows.Count != 0)
            {
                dataBaseWork.tableRespondents.Clear();
            }
        }

        private void MenuItemOpenProtocol_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Directory.Exists(ProtocolFileDB.pathProtocolFile) & File.Exists(ProtocolFileDB.pathProtocolFile + ProtocolFileDB.fileNameProtocol))
                {
                    Process.Start(ProtocolFileDB.pathProtocolFile + ProtocolFileDB.fileNameProtocol);
                }
                else
                {
                    //MessageBox.Show("Протокол отсутствует", "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                logger.Error(ex.Message);
            }
        }

        private void MenuItemOpenDirectoryProtocol_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Directory.Exists(ProtocolFileDB.pathProtocolFile))
                {
                    Process.Start(ProtocolFileDB.pathProtocolFile);
                }
                else
                {
                    MessageBox.Show("Не найдена директория хранения протокола", "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                logger.Error(ex.Message);
            }
        }
    }
}

