using System;
using System.Collections.Generic;
using Core;
using MySql.Data.MySqlClient;

namespace Servidor
{
    internal class Database
    {
        internal class ClientInfo
        {
            public uint userID;
            public string username;
            public string imageB64 = null;
        }

        /// <summary>
        /// Cria e devolve uma ligação à base de dados
        /// </summary>
        /// <returns></returns>
        private MySqlConnection ConnectToDatabase()
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection("Server=localhost;DataBase=" + Properties.Settings.Default.Database +
                                                           ";Uid=" + Properties.Settings.Default.DB_User +
                                                           ";Pwd=" + Properties.Settings.Default.DB_Password);
                conn.Open();
            }
            catch (Exception ex)
            {
                Logger.Log("Ocorreu um erro ao ligar à base de dados: " + ex.Message);
                Logger.LogQuietly(ex.InnerException.StackTrace);
            }
            return conn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="ImageB64"></param>
        /// <returns></returns>
        internal static bool RegisterUser(string username, string password, string ImageB64)
        {
            MySqlCommand cmd = null;
            try
            {
                cmd = new MySqlCommand();
                Database db = new Database();
                cmd.Connection = db.ConnectToDatabase();

                // TODO: Processar Password

                cmd.CommandText = String.Format("INSERT INTO utilizadores Username, Password, ImagemB64 VALUES '{0}', '{1}', '{2}';", username, password, ImageB64);

                if(cmd.ExecuteNonQuery() == 1)
                {
                    // Uma linha foi modificada, o que sigifica que o utilizador foi adicionado.
                    cmd.Connection.Close();
                    return true;
                }
                else
                {
                    // O utilizador não foi adicionado.
                    Logger.Log("O utilizador não foi registado.");
                    Logger.LogQuietly("Query:" + cmd.CommandText);
                }
            }
            catch(Exception ex)
            {
                Logger.Log("Ocorreu um erro ao registar o utilizador: " + ex.Message);
                Logger.LogQuietly(ex.InnerException.StackTrace);
            }
            finally
            {
                if(cmd.Connection != null)
                    cmd.Connection.Close();
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="client"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        internal static bool LogUserIn(string username, string password, out ClientInfo client, out string error)
        {
            MySqlCommand cmd = null;
            client = new ClientInfo();
            error = null;

            try
            {
                cmd = new MySqlCommand();
                Database db = new Database();
                cmd.Connection = db.ConnectToDatabase();
                
                // TODO: Processar password

                cmd.CommandText = "SELECT ID, ImagemB64 FROM Utilizadores WHERE Username=\"" + username + "\" AND Password=\"" + password + "\" LIMIT 1;";
                MySqlDataReader data = cmd.ExecuteReader();

                if(data.HasRows) // Se existe um registo
                {
                    if(data.Read())
                    {
                        client.userID = (uint)data["ID"];
                        client.username = username;
                        client.imageB64 = data["ImagemB64"].ToString();
                        data.Close();
                        cmd.Connection.Close();
                        return true;
                    }
                }
                else
                {
                    data.Close();
                    error = "Credenciais incorretas.";
                }
            }
            catch(Exception ex)
            {
                error = ex.Message;
                Logger.Log("Ocorreu um erro ao tentar autenticar o utilizador \"" + username + "\": " + ex.Message);
                Logger.LogQuietly(ex.InnerException.StackTrace);
            }
            finally
            {
                if(cmd.Connection != null)
                    cmd.Connection.Close();
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="message"></param>
        internal static void SaveUserMessage(uint userID, string message)
        {
            MySqlCommand cmd = null;

            try
            {
                cmd = new MySqlCommand();
                Database db = new Database();
                cmd.Connection = db.ConnectToDatabase();

                cmd.CommandText = "INSERT INTO Mensagens (Texto, IDUtilizador) VALUES ('" + message + "', '" + userID + "')";
                
                
                if(cmd.ExecuteNonQuery() == 1)
                {
                    return;
                }
                else
                {
                    Logger.Log("Não foi possível guardar uma mensagem.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Ocorreu um erro ao tentar guardar uma mensagem: " + ex.Message);
                Logger.LogQuietly(ex.InnerException.StackTrace);
            }
            finally
            {
                if(cmd.Connection != null)
                    cmd.Connection.Close();
            }
        }
    

        internal static List<UserMessageHistoryItem_Packet> GetMessageHistory(uint userID)
        {
            Database db = new Database();
            MySqlCommand cmd = null;

            try
            {
                cmd = new MySqlCommand();
                cmd.Connection = db.ConnectToDatabase();

                cmd.CommandText = "SELECT * FROM Mensagens WHERE IDUtilizador=" + userID + " LIMIT 3;";
                MySqlDataReader reader = cmd.ExecuteReader();

                if(reader.HasRows) // Verificar se existem mensagens em registo
                {
                    List<UserMessageHistoryItem_Packet> mensagem = new List<UserMessageHistoryItem_Packet>();
                    while(reader.Read())
                    {
                        // Adicionar todas as mensagens 
                        UserMessageHistoryItem_Packet m = new UserMessageHistoryItem_Packet();
                        m.message = reader["Texto"].ToString();
                        m.time = DateTime.Parse(reader["dtaEnvio"].ToString());
                        mensagem.Add(m);
                    }
                    reader.Close();
                    cmd.Connection.Close();
                    return mensagem;
                }
                else
                {
                    return new List<UserMessageHistoryItem_Packet>();
                }
            }
            catch(Exception ex)
            {
                UserManagement userManagement = new UserManagement();
                Logger.Log("Não foi possível obter o histórico de mensagens do utilizador \"" + userManagement.GetUsername(userID) + "\" (" + userID + "): " + ex.Message);
                Logger.LogQuietly(ex.InnerException.StackTrace);
            }
            finally
            {
                if (cmd.Connection != null)
                    cmd.Connection.Close();
            }
            return null;
        }
    }
}
