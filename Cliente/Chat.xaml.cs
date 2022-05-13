using Core;
using EI.SI;
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

            // Preencher elementos do UI
            textBlock_nomeUtilizador.Text = Session.username;

            // Pedir dados de utilizadores
            Basic_Packet reequestOnlineUsers = new Basic_Packet();
            reequestOnlineUsers.Type = PacketType.USER_LIST_REQUEST;
            reequestOnlineUsers.Contents = null;
            byte[] dados = protocolSI.Make(ProtocolSICmdType.SYM_CIPHER_DATA, Cryptography.AESEncrypt(Session.aes, JsonConvert.SerializeObject(reequestOnlineUsers)));
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
            }
            desenharListaUtilizadores();

            // Aplicar imagem do utilizador atual
            userImage.ImageSource = Utilities.getImage(Session.userImage);

            // Adição de notificação de entrada
            ServerNotificationControl joinNotification = new ServerNotificationControl("Ligado ao chat");
            messagePanel.Children.Add(joinNotification);

            // Iniciar thread para tratar dos dados a receber
            threadMensagens = new Thread(CarregarMensagens);
            threadMensagens.SetApartmentState(ApartmentState.STA); // STA porque vamos pedir para mexer em elementos do UI
            threadMensagens.Start();
        }

        private void adicionarMensagemCliente(string mensagem)
        {
            if (String.IsNullOrWhiteSpace(mensagem))
                return;

            Basic_Packet pacote = new Basic_Packet();
            Message_Packet message = new Message_Packet();
            pacote.Type = PacketType.MESSAGE;
            message.message = mensagem;
            message.time = DateTime.Now;
            message.userID = (uint)Session.userID;
            pacote.Contents = message;
            byte[] dados = protocolSI.Make(ProtocolSICmdType.SYM_CIPHER_DATA, Cryptography.AESEncrypt(Session.aes, JsonConvert.SerializeObject(pacote)));

            Session.networkStream.Write(dados, 0, dados.Length);

            //Session.networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length); Temp Fix (?) (Supostamente ler ACK)

            ClientMessageControl clientMessageControl = new ClientMessageControl(Session.username, DateTime.Now, mensagem);
            messagePanel.Children.Add(clientMessageControl);
        }

        private void desenharListaUtilizadores()
        {
            textBlock_listaUtilizadores.Text = ""; // Limpar
            string userString = "";
            foreach (UserInfo user in UserManagement.users)
            {
                userString += user.username + ", ";
            }
            textBlock_listaUtilizadores.Text = userString.Substring(0, userString.Length - 2); // Aplicar a string criada menos a última virgula e espaço
        }

        private void CarregarMensagens()
        {
            try
            {
                while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT)
                {
                    // Verificação de termino de sessão
                    if (IsSignOut)
                        break;


                    Session.networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length); // Ler o próximo pacote
                    if (protocolSI.GetCmdType() == ProtocolSICmdType.SYM_CIPHER_DATA)
                    {
                        /**
                         * Por algum motivo que não me é aparente, o thread não termina
                         * a tempo de não ler mais uma vez o buffer. Essa leitura devolve
                         * sempre 0 porque não há nada no buffer e byte 0 é 0.
                         */
                        if (protocolSI.GetStringFromData() != "0")
                        {
                            //obter os dados para a estrutura
                            Basic_Packet dados = JsonConvert.DeserializeObject<Basic_Packet>(Cryptography.AESDecrypt(Session.aes, protocolSI.GetStringFromData()));
                            // Enviar o ack
                            byte[] ack = protocolSI.Make(ProtocolSICmdType.ACK);
                            Session.networkStream.Write(ack, 0, ack.Length);
                            //Verificar qual o tipo de pedido
                            switch (dados.Type)
                            {
                                #region MESSAGE
                                case PacketType.MESSAGE:
                                    if (dados.Contents != null)
                                    {
                                        messagePanel.Dispatcher.Invoke(() =>
                                        {
                                            Message_Packet msg = JsonConvert.DeserializeObject<Message_Packet>(dados.Contents.ToString());
                                            UserInfo sender = UserManagement.GetUser(msg.userID);
                                            MessageControl message = new MessageControl(sender.userID, sender.username, sender.userImage, msg.time, msg.message);
                                            messagePanel.Children.Add(message);
                                        });
                                    }
                                    break;
                                #endregion

                                //Saber qual o user que entrou no chat
                                #region USER_JOINED
                                case PacketType.USER_JOINED:
                                    //Adicionar á lista os utilizadores que estão online
                                    //Verificar se existem dados
                                    if (dados.Contents != null)
                                    {
                                        UserJoined_Packet request_user_joined = JsonConvert.DeserializeObject<UserJoined_Packet>(dados.Contents.ToString());
                                        textBlock_listaUtilizadores.Dispatcher.Invoke(() =>
                                        {
                                            UserManagement.AddUser(request_user_joined.userID, request_user_joined.username, request_user_joined.userImage);
                                            ServerNotificationControl joinNotification = new ServerNotificationControl("O utilizador " + request_user_joined.username + " juntou-se ao chat!");
                                            messagePanel.Children.Add(joinNotification);
                                            desenharListaUtilizadores();
                                        });
                                    }
                                    break;
                                #endregion

                                //Saber qual o user saiu do chat
                                //Verificar se existem dados
                                #region USER_LEFT
                                case PacketType.USER_LEFT:
                                    //Remover da lista os utilizadores que sairem
                                    if (dados.Contents != null)
                                    {
                                        textBlock_listaUtilizadores.Dispatcher.Invoke(() =>
                                        {
                                            try
                                            {
                                                uint context_user = uint.Parse(dados.Contents.ToString()); // Por algum motivo ele não aceita fazer o cast direto de object para uint

                                                ServerNotificationControl joinNotification = new ServerNotificationControl("O utilizador " + UserManagement.GetUsername(context_user) + " saiu do chat!");
                                                messagePanel.Children.Add(joinNotification);
                                                UserManagement.RemoveUser(context_user);
                                                desenharListaUtilizadores();
                                            }
                                            catch (Exception ex)
                                            {
                                                MessageBox.Show(ex.Message + "\n" + dados.Contents.ToString());
                                            }
                                        });
                                    }
                                    break;
                                    #endregion
                            }
                        }
                        else
                        {
                            break; // Quando aqui chegar a execução já devia estar mais que parada. Isto é só uma ajuda.
                        }
                    }
                }
            }
            catch (ObjectDisposedException) { /* Temp fix tentativa de aceder à stream depois dela ter sido fichada */ }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro thread", MessageBoxButton.OK, MessageBoxImage.Error);
                /**
                 * Coisas a fazer aqui:
                 *  - Tentar enviar EOT para terminar aplicação; ou
                 *  - Simplesmente forçar saída
                 */
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
                IsSignOut = true;
                UserManagement.FlushUsers();
                Session.CloseTCPSession();
                Session.loginReference.Show();
                this.Close();
            }
        }
    }
}
