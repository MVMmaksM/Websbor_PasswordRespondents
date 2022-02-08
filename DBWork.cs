using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Websbor_PasswordRespondents
{
    class DBWork
    {
        public DataTable tableRespondents;
        private string _connectionString;
        private SqlConnection connection = null;        
        private SqlDataAdapter sqlDataAdapter;
        private SqlCommand sqlCommand;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string _userName;
        private string sqlQueryDeleteTable = "DELETE FROM [Password]";
        
        public DBWork(string connectionString)
        {
            _connectionString = connectionString;
            _userName = Environment.UserName;
        }
        public bool DbUserExist()
        {
            logger.Info($"[Вызов метода DbUserExist] : Пользователь {_userName}");
            try
            {
                using (SqlConnection connectionDboUser = new SqlConnection(_connectionString))
                {
                    SqlCommand commandExistUser = connectionDboUser.CreateCommand();
                    commandExistUser.CommandText = $"SELECT COUNT (*) FROM [USERS] WHERE username = @username";
                    commandExistUser.Parameters.AddWithValue("@username", _userName);
                    connectionDboUser.Open();
                    var resultCommandExistUser = commandExistUser.ExecuteScalar() as int?;

                    if (resultCommandExistUser != 1)
                    {                     
                        MessageBox.Show($"Пользователь {_userName} не найден в dbo.Users", "Ошибка доступа к БД", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                        logger.Warn($"[Пользователь {_userName} не найден в dbo.Users]");

                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {        
                MessageBox.Show($"При подключении к dbo.Users возникла ошибка: {ex.Message} \n\nсм. log-файл ", "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                logger.Error(ex.Message + ex.StackTrace);

                return false;
            }
        }

        public void GetDataDBtoTableRespondents(string command)
        {
            logger.Info("[Вызов метода GetDataDB]");
            try
            {
                SqlCommand sqlCommandGetData = connection.CreateCommand();
                sqlCommandGetData.CommandText = command;
                SqlDataAdapter sqlGetDataDataAdapter = new SqlDataAdapter(sqlCommandGetData);
                tableRespondents.Clear();
                sqlGetDataDataAdapter.Fill(tableRespondents);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                logger.Error(ex.Message + ex.StackTrace);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public void UpdateDB()
        {
            logger.Info("[Вызов метода UpdateDB]");

            try
            {
                SqlCommandBuilder comandbuilder = new SqlCommandBuilder(sqlDataAdapter);
                comandbuilder.ConflictOption = ConflictOption.OverwriteChanges;
                //tableRespondents = tableRespondents.Rows.Cast<DataRow>().Where(row => !row.ItemArray.All(field => field is DBNull || string.IsNullOrWhiteSpace(field as string))).CopyToDataTable();                
                sqlDataAdapter.Update(tableRespondents);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                logger.Error(ex.Message + ex.StackTrace);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public void GetShemaTableRespondents() 
        {
            try
            {
                logger.Info("[Получение SchemaTableRespondents]");

                connection = new SqlConnection(_connectionString);
                sqlCommand = connection.CreateCommand();
                sqlCommand.CommandText = "SELECT* FROM [Password]";

                sqlDataAdapter = new SqlDataAdapter();
                sqlDataAdapter.UpdateBatchSize = 3; // ???
                sqlDataAdapter.SelectCommand = sqlCommand;

                sqlDataAdapter.InsertCommand = new SqlCommand("sp_InsertPassword");
                sqlDataAdapter.InsertCommand.CommandType = CommandType.StoredProcedure;
                sqlDataAdapter.InsertCommand.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar, 100, "name"));
                sqlDataAdapter.InsertCommand.Parameters.Add(new SqlParameter("@okpo", SqlDbType.NVarChar, 15, "okpo"));
                sqlDataAdapter.InsertCommand.Parameters.Add(new SqlParameter("@password", SqlDbType.NVarChar, 15, "password"));
                sqlDataAdapter.InsertCommand.Parameters.Add(new SqlParameter("@datecreate", SqlDbType.NVarChar, 15, "datecreate"));
                sqlDataAdapter.InsertCommand.Parameters.Add(new SqlParameter("@comment", SqlDbType.NVarChar, 100, "comment"));
                SqlParameter parameter = sqlDataAdapter.InsertCommand.Parameters.Add("@ID", SqlDbType.Int, 0, "ID");
                parameter.Direction = ParameterDirection.Output;


                tableRespondents = new DataTable();
                sqlDataAdapter.FillSchema(tableRespondents, SchemaType.Source);               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                logger.Error(ex.Message + ex.StackTrace);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public void DeleteAllRowsTable() 
        {
            int count;

            try
            {
                if (MessageBox.Show("Удалить все записи в таблице?", "Сообщение", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                {
                    if (tableRespondents.Rows.Count>0)
                    {
                        tableRespondents.Clear();
                    }                   

                    using (SqlConnection connectionDeleteAll = new SqlConnection(_connectionString))
                    {
                        SqlCommand sqlCommand = new SqlCommand(sqlQueryDeleteTable, connectionDeleteAll);
                        connectionDeleteAll.Open();
                        count = sqlCommand.ExecuteNonQuery();
                        connectionDeleteAll.Close();
                    }

                    MessageBox.Show($"Удалено записей: {count}", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                logger.Error(ex.Message);
            }

        }

        public DataTable GetDataDB(string command) 
        {
            DataTable dataTable;           

            using (SqlConnection sqlConnection = new SqlConnection(_connectionString))
            {
                SqlCommand sqlCommand = connection.CreateCommand();
                sqlCommand.CommandText =command;
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                dataTable = new DataTable();
                sqlDataAdapter.Fill(dataTable);
            }          
            return dataTable;
        }
    }
}
