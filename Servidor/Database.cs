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
        /// <returns><see cref="MySqlConnection"/></returns>
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
        /// Regista um utilizador na base de dados
        /// </summary>
        /// <param name="username">Username a registar</param>
        /// <param name="password">Password a registar</param>
        /// <param name="ImageB64">Nullable: Imagem do utilizador em base64</param>
        /// <returns>True se foi possível registar, caso contrário false</returns>
        internal static bool RegisterUser(string username, string password, string ImageB64)
        {
            MySqlCommand cmd = null;
            try
            {
                cmd = new MySqlCommand();
                Database db = new Database();
                cmd.Connection = db.ConnectToDatabase();

                // TODO: Processar Password

                if(ImageB64 == null)
                {
                    cmd.CommandText = String.Format("INSERT INTO utilizadores (Username, Password) VALUES ('{0}', '{1}');", username, password);
                }
                else
                {
                    cmd.CommandText = String.Format("INSERT INTO utilizadores (Username, Password, ImagemB64) VALUES ('{0}', '{1}', '{2}');", username, password, ImageB64);
                }


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
        /// Verficia as credenciais de um utilizador e, caso estejam corretas,
        /// devolve um objeto com a sua informação por referência no parametro
        /// <paramref name="client"/>. 
        /// </summary>
        /// <param name="username">Username do utilizador a iniciar sessão</param>
        /// <param name="password">Password do utilizador a iniciar sessão</param>
        /// <param name="client">Objeto do tipo <see cref="ClientInfo"/> onde são passados os dados do utilizador por referência</param>
        /// <param name="error">String passada por referência com o erro</param>
        /// <returns>True caso os dados estejam corretos, caso contrário false</returns>
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
                if(ex.InnerException != null)
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
        /// Guarda uma mensagem de utilizador na base de dados
        /// </summary>
        /// <param name="userID">O ID do utilizador que enviou a mensagem</param>
        /// <param name="message">O texto da mensagem</param>
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
    
        /// <summary>
        /// Obtém uma lista com as últimas 3 mensagens enviadas por um utilizador
        /// </summary>
        /// <param name="userID">ID do utilizador</param>
        /// <returns>Lista com <see cref="UserMessageHistoryItem_Packet"/></returns>
        internal static List<UserMessageHistoryItem_Packet> GetMessageHistory(uint userID)
        {
            Database db = new Database();
            MySqlCommand cmd = null;

            try
            {
                cmd = new MySqlCommand();
                cmd.Connection = db.ConnectToDatabase();

                cmd.CommandText = "SELECT * FROM Mensagens WHERE IDUtilizador=" + userID + " ORDER BY ID DESC LIMIT 3;";
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
                Logger.Log("Não foi possível obter o histórico de mensagens do utilizador \"" + UserManagement.GetUsername(userID) + "\" (" + userID + "): " + ex.Message);
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
