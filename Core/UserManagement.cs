﻿using System.Collections.Generic;
using System.Net.Sockets;

namespace Core
{
    public class UserManagement
    {
        //////// Padrão Singleton
        private static UserManagement singleton;

        private static readonly object padlock = new object();

        public static UserManagement Retrieve
        {
            get
            {
                lock (padlock)
                {
                    if (singleton == null)
                    {
                        singleton = new UserManagement();
                    }
                    return singleton;
                }
            }
        }

        /////// Classe

        public List<UserInfo> users = new List<UserInfo>();

        /// <summary>
        /// [CLIENT-SIDE] Adiciona um utilizador à lista de utilizadores
        /// </summary>
        public void AddUser(uint userID, string username, string userImageB64)
        {
            UserInfo newUser = new UserInfo(userID, username, userImageB64);
            users.Add(newUser);
        }

        /// <summary>
        /// [SERVER-SIDE] Adiciona um utilizador à lista de utilizadores
        /// </summary>
        public void AddUser(uint userID, string username, string userImageB64, NetworkStream userStream)
        {
            UserInfo newUser = new UserInfo(userID, username, userImageB64, userStream);
            users.Add(newUser);
        }

        /// <summary>
        /// Procura por um utilizador baseado no ID indicado
        /// </summary>
        /// <param name="userID">ID do utilizador</param>
        /// <returns>Objeto <see cref="UserInfo"/> com todos os dados do utilizador</returns>
        public UserInfo GetUser(uint userID)
        {
            return users.Find(user => user.userID == userID);
        }

        /// <summary>
        /// Procura pelo username de um utilizador com base no ID indicado
        /// </summary>
        /// <param name="userID">ID do utilizador</param>
        /// <returns>Username do utilizador</returns>
        public string GetUsername(uint userID)
        {
            return users.Find(user => user.userID == userID).username;
        }

        /// <summary>
        /// Remove um utilizador da lista de utilizadores com base no seu ID
        /// </summary>
        /// <param name="userID">ID do utilizador</param>
        public void RemoveUser(uint userID)
        {
            users.Remove(GetUser(userID));
        }
    }

    public class UserInfo
    {
        public uint userID { get; }
        public string username { get; }
        public string userImage { get; }
        public NetworkStream userStream { get; }

        /// <summary>
        /// [CLIENT-SIDE] Cria um objeto de utilizador
        /// </summary>
        public UserInfo(uint userID, string username, string userImageB64)
        {
            this.userID = userID;
            this.username = username;
            this.userImage = userImageB64;
            this.userStream = null; // Client-side não guarda streams. Definido como null aqui para impedir possíveis erros
        }

        /// <summary>
        /// [SERVER-SIDE] Cria um objeto de utilizador
        /// </summary>
        public UserInfo(uint userID, string username, string userImageB64, NetworkStream stream)
        {
            this.userID = userID;
            this.username = username;
            this.userImage = userImageB64;
            this.userStream = stream;
        }
    }
}
