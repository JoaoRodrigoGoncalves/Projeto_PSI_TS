using EI.SI;
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
        internal static uint? userImage = null;
        internal static Login loginReference = null; // Utilizado para poder terminar sessão e poder regressar à janela de login inicial

        internal static void StartTCPSession()
        {
            try
            {
                if (IPAddress.TryParse(Properties.Settings.Default.ipaddress, out IPAddress address) || (Properties.Settings.Default.port < 1000 || Properties.Settings.Default.port > 65535))
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
                    if (MessageBox.Show("Endereço IP ou porto indicado no ficheiro de configurações inválido. Pretendes repor as configurações padrão e tentar novamente?", "Endereço IP inválido", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                    {
                        // Caso responda que sim, redefinimos as configurações
                        ResetSettings();
                    }
                }
            }
            catch
            {
                // Ocorreu um erro ao tentar ligar ao servidor. Perguntamos se pretende repor configurações padrão
                if (MessageBox.Show("Ocorreu um erro ao tentar ligar ao servidor. Pretendes repor as configurações padrão e tentar novamente?", "Erro ao tentar ligar ao servidor", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                {
                    // Caso responda que sim, redefinimos as configurações
                    ResetSettings();
                }
            }
        }


        private static void ResetSettings()
        {
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();
            MessageBox.Show("As configurações foram repostas para os valores padrão", "Configurações repostas", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            StartTCPSession(); // Pede para que a TCPSession seja aberta de forma a continuar o fluxo da aplicação
        }

        internal static void CloseTCPSession()
        {
            if (Session.Client != null)
            {
                ProtocolSI protocolSI = new ProtocolSI();
                byte[] close = protocolSI.Make(ProtocolSICmdType.EOT);
                networkStream.Write(close, 0, close.Length);
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                networkStream.Close();
                Session.Client.Close();
                Session.Client = null;
            }
        }
    }
}
