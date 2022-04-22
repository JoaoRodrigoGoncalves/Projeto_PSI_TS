﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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

        public uint GenerateUserID()
        {
            // TODO: Alterar no futo para o ID do utilizador na base de dados
            Random random = new Random();
            bool success = false;
            uint userID = 0;

            while(!success)
            {
                userID = (uint)random.Next(1, 101);

                if(GetUser(userID) == null)
                    success = true;

            }
            return userID;
        }

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

        public UserInfo GetUser(uint userID)
        {
            return users.Find(user => user.userID == userID);
        }

        public string GetUsername(uint userID)
        {
            return users.Find(user => user.userID == userID).username;
        }

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