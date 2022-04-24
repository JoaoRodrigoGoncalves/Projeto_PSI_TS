using EI.SI;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Windows;

namespace Cliente
{
    internal static class Session
    {
        internal static TcpClient Client { get; private set; }
        internal static NetworkStream networkStream { get; private set; }
        internal static uint? userID = null;
        internal static string username = null;
        internal static string userImageB64 = null;

        internal static void StartTCPSession()
        {
            IPAddress address;
            
            try
            {
                if (IPAddress.TryParse(Properties.Settings.Default.ipaddress, out address) || (Properties.Settings.Default.port < 1000 || Properties.Settings.Default.port > 65535))
                {
                    // Se o endereço IP/porto do ficheiro de configuração seja válido, iniciamos o endpoint
                    IPEndPoint endPoint = new IPEndPoint(address, Properties.Settings.Default.port);
                    Session.Client = new TcpClient();
                    Session.Client.Connect(endPoint);
                    Session.networkStream = Session.Client.GetStream();
                    ProtocolSI protocolSI = new ProtocolSI();
                }
                else
                {
                    // Caso contrário, mostramos uma mensagem de erro e perguntamos se o utilizador pretende repor as configurações padrão
                    if (MessageBox.Show("Endereço IP ou porto indicado no ficheiro de configurações inválido. Pretendes repor as configurações padrão?", "Endereço IP inválido", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                    {
                        // Caso responda que sim, redefinimos as configurações e reiniciamos a aplicação
                        ResetAndRestart();
                    }
                    else
                    {
                        // Caso contrário, saímos com erro -3
                        Environment.Exit(-3);
                    }
                }
            }
            catch
            {
                // Ocorreu um erro ao tentar ligar ao servidor. Perguntamos se pretende repor configurações padrão
                if (MessageBox.Show("Ocorreu um erro ao tentar ligar ao servidor. Pretendes repor as configurações padrão?", "Erro ao tentar ligar ao servidor", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                {
                    // Caso responda que sim, redefinimos as configurações e reiniciamos a aplicação
                    ResetAndRestart();
                }
                else
                {
                    // Caso contrário, saímos com erro -2
                    Environment.Exit(-2);
                }
            }
        }

        private static void ResetAndRestart()
        {
            Properties.Settings.Default.ipaddress = "127.0.0.1";
            Properties.Settings.Default.port = 5000;
            Properties.Settings.Default.Save();
            Process.Start(Application.ResourceAssembly.Location);
            Environment.Exit(0);
        }

        internal static void CloseTCPSession()
        {
            if(Session.Client != null)
            {
                ProtocolSI protocolSI = new ProtocolSI();
                byte[] close = protocolSI.Make(ProtocolSICmdType.EOT);
                networkStream.Write(close, 0, close.Length);
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                networkStream.Close();
            }
        }
    }
}
