using Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Servidor
{
    internal class Database
    {

        private const int SALTSIZE = 10;
        private const int NUMBER_ITERATIONS = 1200;

        internal class ClientInfo
        {
            public uint userID;
            public string username;
            public uint? userImage = null;
        }

        /// <summary>
        /// Regista um utilizador na base de dados
        /// </summary>
        /// <param name="username">Username a registar</param>
        /// <param name="password">Password a registar</param>
        /// <param name="userImage">Nullable: Imagem selecionada pelo utilizador</param>
        /// <returns>True se foi possível registar, caso contrário false</returns>
        internal static bool RegisterUser(string username, string password, uint? userImage)
        {
            ChatAppEntities chatAppEntities = new ChatAppEntities();
            try
            {

                byte[] salt = ServerSideCryptography.GenerateSalt(SALTSIZE);
                byte[] saltedPassword = ServerSideCryptography.GenerateSaltedHash(password, salt, NUMBER_ITERATIONS);

                Utilizadores user = new Utilizadores();
                user.Username = username;
                user.Salt = salt;
                user.SaltedPassword = saltedPassword;
                user.userImage = (int?)userImage;

                chatAppEntities.Utilizadores.Add(user);
                chatAppEntities.SaveChanges();
                return true;

            }
            catch (Exception ex)
            {
                Logger.Log("Ocorreu um erro ao registar o utilizador: " + ex.Message);
                if (ex.InnerException != null)
                    Logger.LogQuietly(ex.InnerException.StackTrace);
            }
            finally
            {
                chatAppEntities.Dispose();
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
            ChatAppEntities chatAppEntities = new ChatAppEntities();
            client = new ClientInfo();
            error = "Credenciais incorretas";

            try
            {
                Utilizadores user = chatAppEntities.Utilizadores.FirstOrDefault(x => x.Username == username);

                chatAppEntities.Dispose();

                if (user == null)
                    return false;

                byte[] hash = ServerSideCryptography.GenerateSaltedHash(password, user.Salt, NUMBER_ITERATIONS);

                if (user.SaltedPassword.SequenceEqual(hash)) //
                {
                    // Converter objeto Utilizadores em objeto ClientInfo

                    client.userID = (uint)user.ID;
                    client.username = user.Username;
                    client.userImage = (uint?)user.userImage;
                    error = null;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Logger.Log("Ocorreu um erro ao tentar autenticar o utilizador \"" + username + "\": " + ex.Message);
                if (ex.InnerException != null)
                    Logger.LogQuietly(ex.InnerException.StackTrace);
            }
            finally
            {
                chatAppEntities.Dispose();
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
            ChatAppEntities chatAppEntities = new ChatAppEntities();

            try
            {
                Mensagens mensagem = new Mensagens();

                mensagem.IDUtilizador = (int)userID;
                mensagem.dtaEnvio = DateTime.Now;
                mensagem.Texto = message;

                chatAppEntities.Mensagens.Add(mensagem);

                chatAppEntities.SaveChanges();
                chatAppEntities.Dispose();
                return;
            }
            catch (Exception ex)
            {
                Logger.Log("Ocorreu um erro ao tentar guardar uma mensagem: " + ex.Message);
                Logger.LogQuietly(ex.InnerException.StackTrace);
            }
            finally
            {
                chatAppEntities.Dispose();
            }
        }

        /// <summary>
        /// Obtém uma lista com as últimas 3 mensagens enviadas por um utilizador
        /// </summary>
        /// <param name="userID">ID do utilizador</param>
        /// <returns>Lista com <see cref="UserMessageHistoryItem_Packet"/></returns>
        internal static List<UserMessageHistoryItem_Packet> GetMessageHistory(int userID)
        {
            ChatAppEntities chatAppEntities = new ChatAppEntities();
            try
            {
                List<UserMessageHistoryItem_Packet> mensagem = new List<UserMessageHistoryItem_Packet>();

                foreach (Mensagens msg in chatAppEntities.Mensagens.Where(m => m.IDUtilizador == userID).Take(3))
                {
                    UserMessageHistoryItem_Packet m = new UserMessageHistoryItem_Packet();
                    m.message = msg.Texto;
                    m.time = msg.dtaEnvio;
                    mensagem.Add(m);
                }
                chatAppEntities.Dispose();
                return mensagem;
            }
            catch (Exception ex)
            {
                Logger.Log("Não foi possível obter o histórico de mensagens do utilizador \"" + UserManagement.GetUsername((uint)userID) + "\" (" + userID + "): " + ex.Message);
                Logger.LogQuietly(ex.InnerException.StackTrace);
            }
            finally
            {
                chatAppEntities.Dispose();
            }
            return null;
        }

        /// <summary>
        /// Verifica se um utilizador existe com base no userID indicado
        /// </summary>
        /// <param name="userID">ID do utilizador a verificar</param>
        /// <returns>True se existir, caso contrario false</returns>
        internal static bool UserExists(int userID)
        {
            ChatAppEntities chatAppEntities = new ChatAppEntities();
            try
            {
                return (chatAppEntities.Utilizadores.Find(userID) != null);
            }
            catch (Exception ex)
            {
                Logger.Log("Não foi possível procurar pelo utilizador (" + userID + "): " + ex.Message);
                if (ex.InnerException != null)
                    Logger.LogQuietly(ex.InnerException.StackTrace);
            }
            finally
            {
                chatAppEntities.Dispose();
            }
            return false;
        }

        /// <summary>
        /// Verifica se um utilizador existe com base no username indicado
        /// </summary>
        /// <param name="username">username a verificar</param>
        /// <returns>True se existir, caso contrario false</returns>
        internal static bool UserExists(string username)
        {
            ChatAppEntities chatAppEntities = new ChatAppEntities();
            try
            {
                return (chatAppEntities.Utilizadores.FirstOrDefault(u => u.Username == username) != null);
            }
            catch (Exception ex)
            {
                Logger.Log("Não foi possível procurar pelo utilizador (" + username + "): " + ex.Message);
                Logger.LogQuietly(ex.InnerException.StackTrace);
            }
            finally
            {
                chatAppEntities.Dispose();
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        internal static ClientInfo GetUserData(int userID)
        {
            ChatAppEntities chatAppEntities = new ChatAppEntities();
            ClientInfo client = new ClientInfo();
            try
            {
                Utilizadores user = chatAppEntities.Utilizadores.Find(userID);
                chatAppEntities.Dispose();
                if (user != null)
                {
                    client.userID = (uint)userID;
                    client.username = user.Username;
                    client.userImage = (uint?)user.userImage;
                    return client;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Ocorreu um erro ao tentar obter os dados do utilizador " + userID + ": " + ex.Message);
                if (ex.InnerException != null)
                    Logger.LogQuietly(ex.InnerException.StackTrace);
            }
            finally
            {
                if (chatAppEntities != null)
                    chatAppEntities.Dispose();
            }
            return null;
        }
    }
}
