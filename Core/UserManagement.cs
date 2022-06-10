using System.Collections.Generic;
using System.Net.Sockets;

namespace Core
{
    public static class UserManagement
    {
        public static List<UserInfo> users = new List<UserInfo>();

        /// <summary>
        /// [CLIENT-SIDE] Adiciona um utilizador à lista de utilizadores
        /// </summary>
        public static void AddUser(uint userID, string username, uint? userImage)
        {
            UserInfo newUser = new UserInfo(userID, username, userImage);
            users.Add(newUser);
        }

        /// <summary>
        /// [SERVER-SIDE] Adiciona um utilizador à lista de utilizadores
        /// </summary>
        public static void AddUser(uint userID, string username, uint? userImage, NetworkStream userStream)
        {
            UserInfo newUser = new UserInfo(userID, username, userImage, userStream);
            users.Add(newUser);
        }

        /// <summary>
        /// Procura por um utilizador baseado no ID indicado
        /// </summary>
        /// <param name="userID">ID do utilizador</param>
        /// <returns>Objeto <see cref="UserInfo"/> com todos os dados do utilizador</returns>
        public static UserInfo GetUser(uint userID)
        {
            return users.Find(user => user.userID == userID);
        }

        /// <summary>
        /// Procura pelo username de um utilizador com base no ID indicado
        /// </summary>
        /// <param name="userID">ID do utilizador</param>
        /// <returns>Username do utilizador</returns>
        public static string GetUsername(uint userID)
        {
            return users.Find(user => user.userID == userID).username;
        }

        /// <summary>
        /// Remove um utilizador da lista de utilizadores com base no seu ID
        /// </summary>
        /// <param name="userID">ID do utilizador</param>
        public static void RemoveUser(uint userID)
        {
            users.Remove(GetUser(userID));
        }

        /// <summary>
        /// Remove todos os utilizadores da lista de utilizadores
        /// </summary>
        public static void FlushUsers()
        {
            users.Clear();
        }
    }

    public class UserInfo
    {
        public uint userID { get; }
        public string username { get; }
        public uint? userImage { get; }
        public NetworkStream userStream { get; }

        /// <summary>
        /// [CLIENT-SIDE] Cria um objeto de utilizador
        /// </summary>
        public UserInfo(uint userID, string username, uint? userImage)
        {
            this.userID = userID;
            this.username = username;
            this.userImage = userImage;
            this.userStream = null; // Client-side não guarda streams. Definido como null aqui para impedir possíveis erros
        }

        /// <summary>
        /// [SERVER-SIDE] Cria um objeto de utilizador
        /// </summary>
        public UserInfo(uint userID, string username, uint? userImage, NetworkStream stream)
        {
            this.userID = userID;
            this.username = username;
            this.userImage = userImage;
            this.userStream = stream;
        }
    }
}
