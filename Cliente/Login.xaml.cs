using System;
using EI.SI;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Media;
using System.Net;
using Core;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Cliente
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Login : Window
    {
        ProtocolSI protocolSI;
        TcpClient client;

        public Login()
        {
            InitializeComponent();

            try
            {
                IPAddress address;

                if(IPAddress.TryParse(Properties.Settings.Default.ipaddress, out address) || (Properties.Settings.Default.port < 1000 || Properties.Settings.Default.port > 65535))
                {
                    // Se o endereço IP/porto do ficheiro de configuração seja válido, iniciamos o endpoint
                    IPEndPoint endPoint = new IPEndPoint(address, Properties.Settings.Default.port);
                    client = new TcpClient();
                    client.Connect(endPoint);
                    Session.networkStream = client.GetStream();
                    protocolSI = new ProtocolSI();

                    textBox_nomeUtilizador.Focus(); // Colocar o cursor automaticamente na caixa de nome de utilizador
                }
                else
                {
                    // Caso contrário, mostramos uma mensagem de erro e perguntamos se o utilizador pretende repor as configurações padrão
                    if(MessageBox.Show("Endereço IP ou porto indicado no ficheiro de configurações inválido. Pretendes repor as configurações padrão?", "Endereço IP inválido", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
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

        private void button_entrar_Click(object sender, RoutedEventArgs e)
        {
            // Código temporário para efeitos de teste

            if(!String.IsNullOrWhiteSpace(textBox_nomeUtilizador.Text))
            {
                //Envia os dados do utilizador(username e password) para o servidor 
                Basic_Packet pedidoLogin = new Basic_Packet();
                pedidoLogin.Type = PacketType.AUTH_REQUEST;//Indicar que o pacote é um pedido de autenticação

                Auth_Request_Packet login = new Auth_Request_Packet();
                login.username = textBox_nomeUtilizador.Text;
                login.password = textBox_palavraPasse.Password;

                pedidoLogin.Contents = login;

                byte[] dados = protocolSI.Make(ProtocolSICmdType.DATA, JsonConvert.SerializeObject(pedidoLogin));
                Session.networkStream.Write(dados, 0, dados.Length);

                // Esperar e ler os dados enviados pelo servidor. Caso a autenticação
                // tenha sido bem sucedida, guardar os dados do utilizador atual e mostrar janela de chat.
                // Caso contrário indicar que os dados fornecidos estão incorretos

                // Ler resposta do tipo AUTH_RESPONSE

                Session.networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length); // Ler o próximo pacote
                while (protocolSI.GetCmdType() != ProtocolSICmdType.DATA) // Enquanto não receber DATA (para ignorar ACKs e outros pacotes perdidos)
                {
                    Session.networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length); // Ler o próximo pacote
                }

                Basic_Packet pacote = JsonConvert.DeserializeObject<Basic_Packet>(protocolSI.GetStringFromData());

                /**
                 * Verificar se o tipo de pacote é um AUTH_RESPONSE.
                 * Em principio, isto há de funcionar. Esperar pela implementação das mensagens
                 * para ver se deixa de funcionar como deve de ser
                 */
                if (pacote.Type == PacketType.AUTH_RESPONSE)
                {
                    Auth_Response_Packet resposta_login = JsonConvert.DeserializeObject<Auth_Response_Packet>(pacote.Contents.ToString());

                    if (resposta_login.success)// Boolean: Se o login foi feito com sucesso
                    {
                        //resposta_login.userid -> uint? (Unsigned Int nullable): id do utilizador
                        //resposta_login.userImage; // String: imagem do utilizador em base64 (quando não tem imagem é null)
                        this.Hide();
                        Session.userID = resposta_login.userID;
                        Session.username = textBox_nomeUtilizador.Text;
                        Session.userImageB64 = resposta_login.userImage;
                        Chat janela_chat = new Chat();
                        janela_chat.ShowDialog();
                    }
                    else
                    {
                        //resposta_login.message; // String: Mensagem do servidor (usado caso haja um erro)
                        textBlock_Message.Text = "Erro! " + resposta_login.message; // Mostrar mensagem de erro do servidor
                    }
                }
            }
            else
            {
                textBox_nomeUtilizador.BorderBrush = Brushes.Red;
            }
        }
        
        private void ResetAndRestart()
        {
            Properties.Settings.Default.ipaddress = "127.0.0.1";
            Properties.Settings.Default.port = 5000;
            Properties.Settings.Default.Save();
            Process.Start(Application.ResourceAssembly.Location);
            Environment.Exit(0);
        }

        private void textBlock_linkRegistar_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Registar janela_registar = new Registar();
            janela_registar.ShowDialog();
        }

        private void textBlock_linkRegistar_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            textBlock_linkRegistar.TextDecorations = TextDecorations.Underline;
        }

        private void textBlock_linkRegistar_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            TextDecorationCollection decorations = new TextDecorationCollection();
            decorations.Clear();
            textBlock_linkRegistar.TextDecorations = decorations;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Session.FecharSessao();
        }

        private void BTN_Settings_Click(object sender, RoutedEventArgs e)
        {
            Settings janela_definicoes = new Settings();
            janela_definicoes.ShowDialog();
        }

        private void textBox_palavraPasse_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
                button_entrar_Click(sender, e);
        }
    }
}
