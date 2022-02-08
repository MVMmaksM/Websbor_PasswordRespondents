using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SqlClient;
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
using System.Windows.Shapes;

namespace Websbor_PasswordRespondents
{
    public partial class SettingsWindow : Window
    {
        public delegate void VisibilityDGHandler(int columnNumber, bool visibility);
        public VisibilityDGHandler visibilityDGHandler;
            
        ConnectionStringSettings connectionStringSettings;
        SqlConnectionStringBuilder sqlConnectionStringBuilder;
        NameValueCollection allAppSettings;
        Configuration config;
        ConnectionStringsSection connectionStringsSection;
        AppSettingsSection appSettings;
        public SettingsWindow()
        {
            InitializeComponent();
            connectionStringSettings = ConfigurationManager.ConnectionStrings["DefaultConnection"];
            sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionStringSettings.ConnectionString);
            TxtBoxProviderName.Text = connectionStringSettings.ProviderName;
            TxtBoxConStrSetName.Content = connectionStringSettings.Name;
            TxtBoxDataSorce.Text = sqlConnectionStringBuilder.DataSource;
            TxtBoxInitCatalog.Text = sqlConnectionStringBuilder.InitialCatalog;
            CmBxIntegratredSecurity.Text = sqlConnectionStringBuilder.IntegratedSecurity.ToString();

            allAppSettings = ConfigurationManager.AppSettings;
            CmBxCanResizeRowHeader.Text = allAppSettings["CanUserResizeRows"];
            CmBxCanResizeColumnWidth.Text = allAppSettings["CanUserResizeColumns"];
            CmBxCanUserReorderColumns.Text = allAppSettings["CanUserReorderColumns"];
        }

        private void Btn_SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            sqlConnectionStringBuilder = new SqlConnectionStringBuilder();
            sqlConnectionStringBuilder.DataSource = TxtBoxDataSorce.Text;
            sqlConnectionStringBuilder.InitialCatalog = TxtBoxInitCatalog.Text;
            sqlConnectionStringBuilder.IntegratedSecurity = Convert.ToBoolean(CmBxIntegratredSecurity.Text);

            config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            connectionStringsSection = (ConnectionStringsSection)config.GetSection("connectionStrings");
            connectionStringsSection.ConnectionStrings["DefaultConnection"].ConnectionString = sqlConnectionStringBuilder.ConnectionString;

            appSettings = (AppSettingsSection)config.GetSection("appSettings");
            appSettings.Settings["CanUserResizeRows"].Value = CmBxCanResizeRowHeader.Text;
            appSettings.Settings["CanUserResizeColumns"].Value = CmBxCanResizeColumnWidth.Text;
            appSettings.Settings["CanUserReorderColumns"].Value = CmBxCanUserReorderColumns.Text;

            config.Save();
            ConfigurationManager.RefreshSection("connectionStrings");
            ConfigurationManager.RefreshSection("appSettings");


            if (MessageBox.Show("Настройки успешно сохранены! \nДля применения настроек необходимо перезапустить приложение, перезапустить сейчас?", "Сообщение", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            }
        }

        private void Btn_CancelSettings_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }       

        //private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        //{
        //    visibilityDGHandler?.Invoke(1, true);
        //}

        //private void Check_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    visibilityDGHandler?.Invoke(1, false);
        //}

        //private void CheckBox_Checked(object sender, RoutedEventArgs e)
        //{
        //    visibilityDGHandler?.Invoke(0, true);
        //}

        //private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    visibilityDGHandler?.Invoke(0, false);
        //}
    }
}
