using Core;
using EI.SI;
using Microsoft.JScript;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace Cliente
{
    /// <summary>
    /// Interaction logic for Chat.xaml
    /// </summary>
    public partial class Chat : Window
    {
        ProtocolSI protocolSI = new ProtocolSI();
        private bool IsSignOut = false;
        Thread threadMensagens;

        public Chat()
        {
            InitializeComponent();

            // Pedir lista de utilizadores ativos

            // Preencher elementos do UI
            textBlock_nomeUtilizador.Text = Session.username;
            // TODO: Ao longo do carregamento das informações do servidor, este campo deve ser alterado
            Basic_Packet requestUserOn = new Basic_Packet();
            requestUserOn.Type = PacketType.USER_LIST_REQUEST;

            requestUserOn.Contents = null;

            byte[] dados = protocolSI.Make(ProtocolSICmdType.SYM_CIPHER_DATA, Cryptography.AESEncrypt(Session.aes, JsonConvert.SerializeObject(requestUserOn)));
            Session.networkStream.Write(dados, 0, dados.Length);

            // Esperar e ler os dados enviados pelo servidor. Caso existam utilizadores online são mostrados na textBlock_listaUtilizadores
            // Ler resposta do tipo USER_LIST_REQUEST
            Session.networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length); // Ler o próximo pacote
            while (protocolSI.GetCmdType() != ProtocolSICmdType.SYM_CIPHER_DATA) // Enquanto não receber SYM_CIPHER_DATA (para ignorar ACKs e outros pacotes perdidos)
            {
                Session.networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length); // Ler o próximo pacote
            }

            Basic_Packet pacote = JsonConvert.DeserializeObject<Basic_Packet>(Cryptography.AESDecrypt(Session.aes, protocolSI.GetStringFromData()));

            List<UserListItem_Packet> packets = JsonConvert.DeserializeObject<List<UserListItem_Packet>>(pacote.Contents.ToString());
            foreach (var user in packets)
            {
                UserManagement.AddUser(user.userID, user.username, user.userImage);
                textBlock_listaUtilizadores.Text += user.username + " ";
            }

            // Aplicar imagem do utilizador atual
            userImage.ImageSource = Utilities.getImage(Session.userImage);

            // Adição de notificação de entrada
            ServerNotificationControl joinNotification = new ServerNotificationControl("Ligado ao chat"); // TODO: trocar para mostrar apenas quando ligação for efetuada com sucesso
            messagePanel.Children.Add(joinNotification);

            threadMensagens = new Thread(CarregarMensagens);
            threadMensagens.SetApartmentState(ApartmentState.STA);
            threadMensagens.Start();
        }
        private void adicionarMensagemCliente(string mensagem)
        {
            if (String.IsNullOrWhiteSpace(mensagem))
                return;
            Basic_Packet pacote = new Basic_Packet();
            Message_Packet message = new Message_Packet();
            pacote.Type = PacketType.MESSAGE;
            message.message = textBox_mensagem.Text;
            message.userID = (uint)Session.userID;
            pacote.Contents = message;
            byte[] dados = protocolSI.Make(ProtocolSICmdType.SYM_CIPHER_DATA,Cryptography.AESEncrypt(Session.aes,JsonConvert.SerializeObject(pacote)));

            Session.networkStream.Write(dados, 0, dados.Length);
            Session.networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);

            ClientMessageControl clientMessageControl = new ClientMessageControl(Session.username, DateTime.Now, mensagem);
            messagePanel.Children.Add(clientMessageControl);
        }
        //Funçao ----
        private void CarregarMensagens()
        {
            try
            {
                while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT) // Enquanto não receber DATA (para ignorar ACKs e outros pacotes perdidos)
                {
                    if (protocolSI.GetCmdType()==ProtocolSICmdType.SYM_CIPHER_DATA)
                    {
                        int bytesRead = Session.networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length); // Ler o próximo pacote

                        byte[] ack;

                        //obter os dados para a estrutura
                        Basic_Packet dados = JsonConvert.DeserializeObject<Basic_Packet>(Cryptography.AESDecrypt(Session.aes, protocolSI.GetStringFromData()));
                        // Enviar o ack
                        ack = protocolSI.Make(ProtocolSICmdType.ACK);
                        Session.networkStream.Write(ack, 0, ack.Length);
                        //Verificar qual o tipo de pedido
                        switch (dados.Type)
                        {
                            //Saber qual o user que entrou no chat
                            case PacketType.USER_JOINED:
                                //Adicionar á lista os utilizadores que estão online
                                //Verificar se existem dados
                                if (dados.Contents != null)
                                {
                                    UserJoined_Packet request_user_joined = JsonConvert.DeserializeObject<UserJoined_Packet>(dados.Contents.ToString());

                                    UserManagement.AddUser((uint)request_user_joined.userID, request_user_joined.username, request_user_joined.userImage);

                                    ServerNotificationControl joinNotification = new ServerNotificationControl("O utilizador " + request_user_joined.username + " juntou-se ao chat!"); // TODO: trocar para mostrar apenas quando ligação for efetuada com sucesso
                                    messagePanel.Children.Add(joinNotification);
                                    textBlock_listaUtilizadores.Dispatcher.Invoke (() =>
                                    {
                                        textBlock_listaUtilizadores.Text += " " + request_user_joined.username;
                                    });
                                    //if (textBlock_listaUtilizadores.Dispatcher.CheckAccess())
                                    //{
                                    //    textBlock_listaUtilizadores.Text += " " + request_user_joined.username;
                                    //}
                                    //else
                                    //{
                                    //    textBlock_listaUtilizadores.Dispatcher.Invoke(new Action(()=>{ textBlock_listaUtilizadores.Text += " " + request_user_joined.username; }));
                                    //}
                                    

                                }
                                break;
                            //Saber qual o user saiu do chat
                            //Verificar se existem dados
                            case PacketType.USER_LEFT:
                                //Remover da lista os utilizadores que sairem
                                if (dados.Contents != null)
                                {
                                    ServerNotificationControl joinNotification = new ServerNotificationControl("O utilizador " + UserManagement.GetUsername((uint)dados.Contents) + " saiu do chat!"); // TODO: trocar para mostrar apenas quando ligação for efetuada com sucesso
                                    messagePanel.Children.Add(joinNotification);

                                    UserManagement.RemoveUser((uint)dados.Contents);

                                    foreach (var user in UserManagement.users)
                                    {
                                        textBlock_listaUtilizadores.Text += user.username + " ";
                                    }
                                }
                                break;

                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void textBlock_nomeUtilizador_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            UserProfile userProfile = new UserProfile((uint)Session.userID);
            userProfile.ShowDialog();
        }

        private void textBlock_nomeUtilizador_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            textBlock_nomeUtilizador.TextDecorations = TextDecorations.Underline;
        }

        private void textBlock_nomeUtilizador_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            TextDecorationCollection decorations = new TextDecorationCollection();
            decorations.Clear();
            textBlock_nomeUtilizador.TextDecorations = decorations;
        }

        private void button_enviarMensagem_Click(object sender, RoutedEventArgs e)
        {
            adicionarMensagemCliente(textBox_mensagem.Text);
            textBox_mensagem.Clear();
            messagePanelScroll.ScrollToEnd(); // Exclusivo para quando a mensagem é do cliente atual
        }

        private void textBox_mensagem_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                adicionarMensagemCliente(textBox_mensagem.Text);
                textBox_mensagem.Clear();
                messagePanelScroll.ScrollToEnd(); // Exclusivo para quando a mensagem é do cliente atual
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (!IsSignOut)
            {
                // Terminar processo
                Application.Current.Shutdown();
            }
        }

        private void button_terminarSessao_Click(object sender, RoutedEventArgs e)
        {
            //Termina a sessão do cliente e mostra a janela de login
            if (Session.Client != null)
            {
                Session.CloseTCPSession();
                Session.loginReference.Show();
                IsSignOut = true;
                this.Close();
            }
        }
    }
}
