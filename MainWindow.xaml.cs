using System;
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

namespace Websbor_PasswordRespondents
{
    public partial class MainWindow : Window
    {
        string connectionString;        
        string sqlQueryGetAllData = "SELECT * FROM [Password]";
        string sqlQuerySaveallData = "SELECT name, okpo, password, datecreate, comment  FROM [Password]";
        private static Logger logger = LogManager.GetCurrentClassLogger();        
        NameValueCollection allAppSettings;       
        Version version;
        private DBWork dataBaseWork;

        public MainWindow()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            allAppSettings = ConfigurationManager.AppSettings;
            dgDataPasswords.CanUserResizeRows = Convert.ToBoolean(allAppSettings["CanUserResizeRows"]);
            dgDataPasswords.CanUserResizeColumns = Convert.ToBoolean(allAppSettings["CanUserResizeColumns"]);
            dgDataPasswords.CanUserReorderColumns = Convert.ToBoolean(allAppSettings["CanUserReorderColumns"]);  
            version = Assembly.GetExecutingAssembly().GetName().Version;
            this.Title += $" (Version {version.Major}.{version.Minor} [build {version.Build}])";
        }

        private void LoadFile()
        {
            logger.Info("[Вызов метода LoadFile]");

            try
            {
                string fileExtension;
                string filePath;
                Action<string> readFile = null;
                System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
                fileDialog.Filter = "*.txt|*.txt|*.xlsx|*.xlsx";
                fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);               

                if (fileDialog.ShowDialog()==System.Windows.Forms.DialogResult.OK)
                {
                    filePath = fileDialog.FileName;
                    fileExtension = System.IO.Path.GetExtension(filePath);

                    if (fileExtension == ".txt")
                    {
                        readFile = ReadTextFile;
                    }
                    else if (fileExtension == ".xlsx")
                    {
                        readFile = ReadExcelFile;
                    }

                    readFile?.Invoke(filePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                logger.Error(ex.Message);
            }
        }

        void ReadExcelFile(string pathFile)
        {
            logger.Info("[Вызов метода ReadExcelFile]");
            try
            {
                SqlConnection connection = new SqlConnection(connectionString);
                FileRespondents excelFileRepondents = new FileRespondents();
                DataTable dataTable = new DataTable();
                dataTable = excelFileRepondents.ExcelToDataTable(pathFile);

                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.InsertCommand = new SqlCommand("Insert password (name, okpo, password, datecreate, comment) Values(@name, @okpo, @password, @datecreate, @comment)", connection);
                adapter.InsertCommand.Parameters.Add("@name", SqlDbType.NVarChar, 100).SourceColumn = "name";
                adapter.InsertCommand.Parameters.Add("@okpo", SqlDbType.NVarChar, 15).SourceColumn = "okpo";
                adapter.InsertCommand.Parameters.Add("@password", SqlDbType.NVarChar, 15).SourceColumn = "password";
                adapter.InsertCommand.Parameters.Add("@datecreate", SqlDbType.NVarChar, 15).SourceColumn = "datecreate";
                adapter.InsertCommand.Parameters.Add("@comment", SqlDbType.NVarChar, 100).SourceColumn = "comment";
                adapter.Update(dataTable);

                MessageBox.Show($"Загружено записей: {dataTable.Rows.Count}", "Уведомление", MessageBoxButton.OKCancel, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\nсм.log-файл", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                logger.Error(ex.Message + ex.Message);
            }
        }
        void ReadTextFile(string pathFile)
        {
            logger.Info("[Вызов метода ReadTextFile]");

            try
            {
                SqlConnection connection = new SqlConnection(connectionString);
                DataTable datafileopen = new DataTable();

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

                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.InsertCommand = new SqlCommand("Insert password (name, okpo, password, datecreate, comment) Values(@name, @okpo, @password, @datecreate, @comment)", connection);
                adapter.InsertCommand.Parameters.Add("@name", SqlDbType.NVarChar, 100).SourceColumn = "name";
                adapter.InsertCommand.Parameters.Add("@okpo", SqlDbType.NVarChar, 15).SourceColumn = "okpo";
                adapter.InsertCommand.Parameters.Add("@password", SqlDbType.NVarChar, 15).SourceColumn = "password";
                adapter.InsertCommand.Parameters.Add("@datecreate", SqlDbType.NVarChar, 15).SourceColumn = "datecreate";
                adapter.InsertCommand.Parameters.Add("@comment", SqlDbType.NVarChar, 100).SourceColumn = "comment";
                adapter.Update(datafileopen);

                System.Windows.MessageBox.Show($"Загружено записей: {datafileopen.Rows.Count}", "Уведомление", MessageBoxButton.OKCancel, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                logger.Error(ex.Message + ex.Message);
            }
            finally
            {
                //if (connection != null)
                //{
                //    connection.Close();
                //    logger.Info($"[Подключение к БД]:{connection.State}");
                //}
            }
        }
        private void dgDataPasswords_Loaded(object sender, RoutedEventArgs e)
        {
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

            if (RadioButtonOKPO.IsChecked == true & !string.IsNullOrWhiteSpace(TxtBoxSearch.Text))
            {
               dataBaseWork.GetDataDBtoTableRespondents($"SELECT* FROM[Password] WHERE okpo LIKE '%{TxtBoxSearch.Text}%'");
            }
            else if (RadioButtonName.IsChecked == true & !string.IsNullOrWhiteSpace(TxtBoxSearch.Text))
            {
                dataBaseWork.GetDataDBtoTableRespondents($"SELECT* FROM[Password] WHERE name LIKE '%{TxtBoxSearch.Text}%'");
            }
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
            LoadFile();
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
                if (RadioButtonOKPO.IsChecked == true)
                {
                   dataBaseWork.GetDataDBtoTableRespondents($"SELECT* FROM[Password] WHERE okpo LIKE '%{TxtBoxSearch.Text}%'");
                }
                else if (RadioButtonName.IsChecked == true)
                {
                    dataBaseWork.GetDataDBtoTableRespondents($"SELECT* FROM[Password] WHERE name LIKE '%{TxtBoxSearch.Text}%'");
                }
            }
        }

        private void MenuItemSaveCurrentData_Click(object sender, RoutedEventArgs e)
        {
            //SaveFileDialog saveFileDialog;
            //FileRespondents fileRespondents;

            //try
            //{
            //    if (tableRespondents.Rows.Count > 0)
            //    {
            //        fileRespondents = new FileRespondents();
            //        //tableRespondents.Columns.RemoveAt(0);
            //        var file = fileRespondents.DataTableToEcxel(tableRespondents);


            //        saveFileDialog = new SaveFileDialog();
            //        saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            //        saveFileDialog.Filter = "|*.xlsx";
            //        saveFileDialog.FileName = "Список респондентов";

            //        if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //        {
            //            File.WriteAllBytes(saveFileDialog.FileName, file);

            //            System.Windows.MessageBox.Show($"Сохранено записей: {tableRespondents.Rows.Count}", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
            //        }
            //    }
            //    else
            //    {
            //        System.Windows.MessageBox.Show("Список пуст", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    System.Windows.MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
            //    logger.Error(ex.Message);
            //}
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
            Microsoft.Win32.SaveFileDialog saveFileDialog;

            try
            {
                excelShema = new FileRespondents();
                var shema = excelShema.ShemaExcelToDB();

                saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                saveFileDialog.Filter = "|*.xlsx";
                saveFileDialog.FileName = "Шаблон загрузки";
                bool? result = saveFileDialog.ShowDialog();

                if (result==true)
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

        
    }
}

