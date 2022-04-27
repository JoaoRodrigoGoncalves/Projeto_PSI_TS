using System;
using EI.SI;
using System.Windows;
using System.Windows.Media;
using Core;
using Newtonsoft.Json;

namespace Cliente
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Login : Window
    {
        ProtocolSI protocolSI = new ProtocolSI();

        public Login()
        {
            InitializeComponent();
        }

        private void button_entrar_Click(object sender, RoutedEventArgs e)
        {
            /**
             * Por causa da implementação das definições de servidor,
             * é necessário que a ligação só comece a ser establecida
             * depois de o utilizador pressionar o botão de inicio de sessão.
             * De forma a que isso seja possível, é necessário verificar
             * se a ligação já foi establecida anteriormente, de forma
             * a impedir que se tente iniciar a ligação multiplas vezes
             */
            
            if(Session.Client == null)
                Session.StartTCPSession();

            if (!String.IsNullOrWhiteSpace(textBox_nomeUtilizador.Text))
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
            Session.CloseTCPSession();
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
