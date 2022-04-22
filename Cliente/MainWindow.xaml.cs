using System;
using EI.SI;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using Core;
using Newtonsoft.Json;

namespace Cliente
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int PORT = 5000;
        NetworkStream networkStream;
        ProtocolSI protocolSI;
        TcpClient client;

        public MainWindow()
        {
            InitializeComponent();

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, PORT);
            client = new TcpClient();
            client.Connect(endPoint);
            networkStream = client.GetStream();
            protocolSI = new ProtocolSI();
        }

        private void button_entrar_Click(object sender, RoutedEventArgs e)
        {
            // Código temporário para efeitos de teste

            if(!String.IsNullOrEmpty(textBox_nomeUtilizador.Text) || !String.IsNullOrWhiteSpace(textBox_nomeUtilizador.Text))
            {
                //Envia os dados do utilizador(username e password) para o servidor 
                Basic_Packet pedidoLogin = new Basic_Packet();
                pedidoLogin.Type = PacketType.AUTH_REQUEST;//Indicar que o pacote é um pedido de autenticação

                Auth_Request_Packet login = new Auth_Request_Packet();
                login.username = textBox_nomeUtilizador.Text;
                login.password = textBox_palavraPasse.Password;

                pedidoLogin.Contents = login;

                byte[] dados = protocolSI.Make(ProtocolSICmdType.DATA, JsonConvert.SerializeObject(pedidoLogin));
                networkStream.Write(dados, 0, dados.Length);

                // Esperar e ler os dados enviados pelo servidor. Caso a autenticação
                // tenha sido bem sucedida, guardar os dados do utilizador atual e mostrar janela de chat.
                // Caso contrário indicar que os dados fornecidos estão incorretos

                // Ler resposta do tipo AUTH_RESPONSE

                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length); // Ler o próximo pacote
                while (protocolSI.GetCmdType() != ProtocolSICmdType.DATA) // Enquanto não receber DATA (para ignorar ACKs e outros pacotes perdidos)
                {
                    networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length); // Ler o próximo pacote
                }

                Basic_Packet pacote = JsonConvert.DeserializeObject<Basic_Packet>(protocolSI.GetStringFromData());

                if (pacote.Type == PacketType.AUTH_RESPONSE)
                {
                    Auth_Response_Packet resposta_login = JsonConvert.DeserializeObject<Auth_Response_Packet>(pacote.Contents.ToString());

                    if (resposta_login.success)// Boolean: Se o login foi feito com sucesso
                    {
                        //resposta_login.userid -> uint? (Unsigned Int nullable): id do utilizador
                        //resposta_login.userImage; // String: imagem do utilizador em base64 (quando não tem imagem é null)
                        this.Hide();
                        Chat janela_chat = new Chat(textBox_nomeUtilizador.Text, (uint)resposta_login.userID, networkStream);
                        janela_chat.ShowDialog();
                    }
                    else //Implementar janela de mensagem caso haja um erro
                    {
                        //resposta_login.message; // String: Mensagem do servidor (usado caso haja um erro)
                        textBlock_Message.Text = "Erro! " + resposta_login.message;
                    }
                }
            }
            else
            {
                textBox_nomeUtilizador.BorderBrush = Brushes.Red;
            }
        }

        private void textBlock_linkRegistar_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            registar janela_registar = new registar();
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
            byte[] close = protocolSI.Make(ProtocolSICmdType.EOT);
            networkStream.Write(close, 0, close.Length);
            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            networkStream.Close();
        }
    }
}
