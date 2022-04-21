using System;
using System.Collections.Generic;
using Core;
using Newtonsoft.Json;
using EI.SI;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Servidor
{
    internal class Program
    {
        private const int DEFAULT_PORT = 5000;
        private static IPAddress DEFAULT_ADDRESS = IPAddress.Parse("127.0.0.1");

        static void Main(string[] args)
        {
            Console.WriteLine("A iniciar servidor...");

            IPEndPoint endpoint = null;

            if(Properties.Settings.Default.listenAddress != "127.0.0.1" || Properties.Settings.Default.port != 5000)
            {
                IPAddress address;
                if(IPAddress.TryParse(Properties.Settings.Default.listenAddress, out address))
                {
                    endpoint = new IPEndPoint(address, Properties.Settings.Default.port);
                }
                else
                {
                    Console.WriteLine("ERRO: Endereço IP fornecido no ficheiro de configuração inválido.");
                    Console.WriteLine("Prima qualquer botão para terinar a execução");
                    Console.ReadKey();
                    Environment.Exit(1);
                }
            }
            else
            {
                endpoint = new IPEndPoint(IPAddress.Loopback, DEFAULT_PORT);
            }

            TcpListener listener = new TcpListener(endpoint);
            listener.Start();
            ProtocolSI protocolSI = new ProtocolSI();
            Console.WriteLine("Servidor iniciado em " + endpoint.Address + ":" + endpoint.Port);

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                ClientHandler handler = new ClientHandler(client);
                handler.Handle();
            }

        }
    }
}
