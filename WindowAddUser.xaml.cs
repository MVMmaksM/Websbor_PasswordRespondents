using System;
using System.Collections.Generic;
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
    public partial class WindowAddUser : Window
    {
        string connectionString;
        public WindowAddUser()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        private void ButtonSaveUser_Click(object sender, RoutedEventArgs e)
        {
            if (TxtBoxUserName.Text != string.Empty) // сделать рег выражения проверку на пустую строку с пробелами
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand commandExistUser = connection.CreateCommand();
                    commandExistUser.CommandText = $"SELECT COUNT (*) FROM [USERS] WHERE username = @username";
                    commandExistUser.Parameters.AddWithValue("@username", TxtBoxUserName.Text);
                    connection.Open();
                    var resultCommandExistUser = commandExistUser.ExecuteScalar() as int?;

                    if (resultCommandExistUser == 1)
                    {
                        MessageBox.Show($"Пользователь {TxtBoxUserName.Text} уже существует в dbo.Users", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        SqlCommand sqlCommand = connection.CreateCommand();
                        sqlCommand.CommandText = $"INSERT INTO USERS (username) VALUES ('{TxtBoxUserName.Text}')";
                        sqlCommand.ExecuteNonQuery();
                        MessageBox.Show($"Пользователь {TxtBoxUserName.Text} успешно добавлен в dbo.Users", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }
    }
}
