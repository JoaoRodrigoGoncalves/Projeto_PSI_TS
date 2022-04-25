﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Servidor
{
    internal class Logger
    {
        static string logPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\logs\";
        static string fileName = logPath + @"log - " + DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss") + ".txt";

        /// <summary>
        /// Cria o cabeçalho do log
        /// </summary>
        internal static void StartLogger()
        {
            if(!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            var text = "------------------------------------------" + Environment.NewLine +
                       "- Data: " + DateTime.Now.ToString("G") + Environment.NewLine +
                       "- Servidor: " + Properties.Settings.Default.listenAddress + ":"+ Properties.Settings.Default.port + Environment.NewLine +
                       "------------------------------------------";
            File.WriteAllText(fileName, text);
        }

        /// <summary>
        /// Escreve no ficheiro de log gerado e na consola do servidor.
        /// </summary>
        /// <param name="message">Dados a serem registados.</param>
        internal static void Log(string message)
        {
            Console.WriteLine(message);
            File.AppendAllText(fileName, Environment.NewLine + "[" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:fff") + "] " + message);
        }

        /// <summary>
        /// Escreve apenas no log gerado.
        /// </summary>
        /// <param name="message">Dados a serem registados.</param>
        internal static void LogQuietly(string message)
        {
            try
            {
                File.AppendAllText(fileName, Environment.NewLine + "[" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:fff") + "] " + message);
            }
            catch(Exception ex)
            {
                Log("Erro ao tentar fazer Log interno: " + ex.Message);
            }
        }

        /// <summary>
        /// Obtém o socket (IP+PORTO) para se apresentado em logs
        /// </summary>
        /// <remarks>Retirado de <see href="https://stackoverflow.com/a/10341337/10935376 "/> a 24/04/2022</remarks>
        /// <param name="client"></param>
        /// <returns></returns>
        internal static string GetSocket(TcpClient client)
        {
            IPEndPoint endpoint = (IPEndPoint)client.Client.RemoteEndPoint;
            string Address = endpoint.Address.ToString();
            string Port = endpoint.Port.ToString();
            return Address + ":" + Port;
        }

    }
}
