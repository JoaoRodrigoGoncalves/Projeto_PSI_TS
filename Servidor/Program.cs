using System;
using System.Net;
using System.Net.Sockets;

namespace Servidor
{
    internal class Program
    {
        private const int DEFAULT_PORT = 5000;
        private static IPAddress DEFAULT_ADDRESS = IPAddress.Parse("127.0.0.1");

        static void Main(string[] args)
        {
            Logger.StartLogger();
            Logger.Log("A iniciar servidor...");

            IPEndPoint endpoint = null;

            if(Properties.Settings.Default.listenAddress != DEFAULT_ADDRESS.ToString() || Properties.Settings.Default.port != DEFAULT_PORT)
            {
                IPAddress address;
                if(IPAddress.TryParse(Properties.Settings.Default.listenAddress, out address))
                {
                    endpoint = new IPEndPoint(address, Properties.Settings.Default.port);
                }
                else
                {
                    Logger.Log("ERRO: Endereço IP fornecido no ficheiro de configuração é inválido.");
                    Logger.Log("Prima qualquer botão para terinar a execução");
                    Console.ReadKey();
                    Environment.Exit(1);
                }
            }
            else
            {
                endpoint = new IPEndPoint(DEFAULT_ADDRESS, DEFAULT_PORT);
            }

            TcpListener listener = new TcpListener(endpoint);
            listener.Start();
            Logger.Log("Servidor iniciado em " + endpoint.Address + ":" + endpoint.Port);

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                ClientHandler handler = new ClientHandler(client);
                handler.Handle();
            }
        }
    }
}
