using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
        private SqlParameter parameter;

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
                DBHandleException(ex);
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
                DBHandleException(ex);
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
                sqlDataAdapter.InsertCommand.Connection = connection;
                sqlDataAdapter.InsertCommand.CommandType = CommandType.StoredProcedure;
                sqlDataAdapter.InsertCommand.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar, 100, "name"));
                sqlDataAdapter.InsertCommand.Parameters.Add(new SqlParameter("@okpo", SqlDbType.NVarChar, 15, "okpo"));
                sqlDataAdapter.InsertCommand.Parameters.Add(new SqlParameter("@password", SqlDbType.NVarChar, 15, "password"));
                sqlDataAdapter.InsertCommand.Parameters.Add(new SqlParameter("@dateupdate", SqlDbType.NVarChar, 50, "dateupdate"));
                sqlDataAdapter.InsertCommand.Parameters.Add(new SqlParameter("@userupdate", SqlDbType.NVarChar, 50, "userupdate"));
                sqlDataAdapter.InsertCommand.Parameters.Add(new SqlParameter("@comment", SqlDbType.NVarChar, 100, "comment"));
                parameter = sqlDataAdapter.InsertCommand.Parameters.Add("@ID", SqlDbType.Int, 0, "ID");
                parameter.Direction = ParameterDirection.Output;
                parameter = sqlDataAdapter.InsertCommand.Parameters.Add("@datecreate", SqlDbType.NVarChar, 50, "datecreate");
                parameter.Direction = ParameterDirection.Output;
                parameter = sqlDataAdapter.InsertCommand.Parameters.Add("@usercreate", SqlDbType.NVarChar, 50, "usercreate");
                parameter.Direction = ParameterDirection.Output;
                sqlDataAdapter.UpdateCommand = new SqlCommand("sp_UpdatePassword");
                sqlDataAdapter.UpdateCommand.CommandType = CommandType.StoredProcedure;
                sqlDataAdapter.UpdateCommand.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int, 0, "ID"));
                sqlDataAdapter.UpdateCommand.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar, 100, "name"));
                sqlDataAdapter.UpdateCommand.Parameters.Add(new SqlParameter("@okpo", SqlDbType.NVarChar, 15, "okpo"));
                sqlDataAdapter.UpdateCommand.Parameters.Add(new SqlParameter("@password", SqlDbType.NVarChar, 15, "password"));
                sqlDataAdapter.UpdateCommand.Parameters.Add(new SqlParameter("@comment", SqlDbType.NVarChar, 100, "comment"));
                parameter = sqlDataAdapter.UpdateCommand.Parameters.Add("@dateupdate", SqlDbType.NVarChar, 50, "dateupdate");
                parameter.Direction = ParameterDirection.Output;
                parameter = sqlDataAdapter.UpdateCommand.Parameters.Add("@userupdate", SqlDbType.NVarChar, 50, "userupdate");
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
                    if (tableRespondents.Rows.Count > 0)
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
            logger.Info("[Вызов метода GetDataDB]");

            DataTable dataTable;

            using (SqlConnection sqlConnection = new SqlConnection(_connectionString))
            {
                SqlCommand sqlCommand = connection.CreateCommand();
                sqlCommand.CommandText = command;
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                dataTable = new DataTable();
                sqlDataAdapter.Fill(dataTable);
            }
            return dataTable;
        }

        public void LoadDataFileToDB(DataTable dataTable)
        {
            logger.Info("[Вызов метода LoadDataFileToDB]");

            int countExecuteRows = 0;
            string commandInsert = "INSERT INTO [Password] (name, okpo, password, datecreate, comment) VALUES (@name, @okpo, @password, @datecreate, @comment)";
            SqlCommand sqlCommand;
            List<string> loadResult = new List<string>();

            try
            {
                if (dataTable.Rows.Count > 0)
                {
                    using (SqlConnection sqlConnection = new SqlConnection(_connectionString))
                    {
                        sqlConnection.Open();

                        foreach (DataRow row in dataTable.Rows)
                        {
                            sqlCommand = sqlConnection.CreateCommand();
                            sqlCommand.CommandText = commandInsert;
                            sqlCommand.Parameters.AddWithValue("@name", row["name"]);
                            sqlCommand.Parameters.AddWithValue("@okpo", row["okpo"]);
                            sqlCommand.Parameters.AddWithValue("@password", row["password"]);
                            sqlCommand.Parameters.AddWithValue("@datecreate", row["datecreate"]);
                            sqlCommand.Parameters.AddWithValue("@comment", row["comment"]);

                            try
                            {
                                countExecuteRows += sqlCommand.ExecuteNonQuery();
                            }
                            catch (SqlException exSql)
                            {
                                loadResult.Add($"ОКПО {row["okpo"]} не загружен: " + exSql.Message);
                            }
                        }
                    }
                }

                ProtocolFileDB.ProtocolLoadFileToDB(loadResult);

                MessageBox.Show($"Загружено записей: {countExecuteRows} \nНе загружено записей: {loadResult.Count}", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DBHandleException(ex);            
                logger.Error(ex.Message);
            }
        }

        private void DBHandleException(Exception ex)
        {
            if (ex is DBConcurrencyException exConcurrency)
            {
                MessageBox.Show("\nВозможно сохраняемая запись удалена другим пользователем \nНеобходимо обновить данные", "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                logger.Error(exConcurrency.Message + exConcurrency.StackTrace);

                return;
            }
            else if (ex is SqlException sqlException)
            {
                switch (sqlException.Number)
                {
                    case 2627:
                        MessageBox.Show($"Добавляемая записиь уже существует в БД!", "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                        logger.Error(sqlException.Message + sqlException.StackTrace);
                        return;

                    case 4060:
                        MessageBox.Show($"Заправшиваемая БД не найдена \nПроверьте имя БД в настройках и повторите вход!", "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                        logger.Error(sqlException.Message + sqlException.StackTrace);
                        return;

                    case 53:
                        MessageBox.Show($"При соединении с сервером произошла ошибка. \nПроверьте имя сервера в настройках и убедитесь что в параметрах SQL Server по умолчанию не запрещены удаленные соединения!", "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                        logger.Error(sqlException.Message + sqlException.StackTrace);
                        return;
                }
            }

            MessageBox.Show(ex.Message, "Необработанное исключение", MessageBoxButton.OKCancel, MessageBoxImage.Error);
            logger.Error(ex.Message + ex.StackTrace);
        }

        public string CreateStringSeaarch(IEnumerable<TextBox> constrolSearchTextBox) 
        {
            string search = null;
            List<Search> listSearch = new List<Search>();

            foreach (var textBox in constrolSearchTextBox)
            {
                if (!string.IsNullOrWhiteSpace(textBox.Text))
                {
                    listSearch.Add(new Search() { nameTextBox = textBox.Name, valueTextBox = textBox.Text });
                }
            }

            if (listSearch.Count == 1)
            {
                search = $"{listSearch[0].nameTextBox} LIKE '%{listSearch[0].valueTextBox.Trim()}%'";
            }
            else if (listSearch.Count > 1)
            {
                for (int i = 0; i < listSearch.Count; i++)
                {
                    if (i == listSearch.Count - 1)
                    {
                        search += $"{listSearch[i].nameTextBox} LIKE '%{listSearch[i].valueTextBox.Trim()}%'";
                    }
                    else
                    {
                        search += $"{listSearch[i].nameTextBox} LIKE '%{listSearch[i].valueTextBox.Trim()}%' AND ";
                    }
                }
            }

            return search;
        }
    }    
}

