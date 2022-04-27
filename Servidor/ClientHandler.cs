using EI.SI;
using Core;
using System;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Servidor
{
    internal class ClientHandler
    {
        private TcpClient client;
        private uint? userID = null; // Pode ser null porque só é atribuído um ID ao utilizador após a autenticação

        internal ClientHandler(TcpClient client)
        {
            this.client = client;
            Logger.LogQuietly(String.Format("Nova ligação de {0}.", Logger.GetSocket(client)));
        }

        internal void Handle()
        {
            Thread thread = new Thread(threadHandler);
            thread.Start();
        }

        private void threadHandler()
        {
            NetworkStream networkStream = this.client.GetStream();
            ProtocolSI protocolSI = new ProtocolSI();

            while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT)
            {
                try
                {
                    int bytesRead = networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                    byte[] ack;

                    switch (protocolSI.GetCmdType())
                    {

                        case ProtocolSICmdType.DATA:
                            //Obter os dados para a estrutura antes de enviar o ack
                            Basic_Packet dados = JsonConvert.DeserializeObject<Basic_Packet>(protocolSI.GetStringFromData());
                            // Enviar o ack
                            ack = protocolSI.Make(ProtocolSICmdType.ACK);
                            networkStream.Write(ack, 0, ack.Length);

                            // Verificar qual o tipo de pedido
                            switch (dados.Type)
                            {
                                #region MESSAGE
                                case PacketType.MESSAGE:

                                    if(dados.Contents != null)
                                    {
                                        Message_Packet mensagem = JsonConvert.DeserializeObject<Message_Packet>(dados.Contents.ToString());

                                        /**
                                         * Apesar do cliente nos enviar o seu userID, para ter a certeza que não há
                                         * nenhuma tentativa de spoofing do ID, iremos reescrever o ID no pacote com base na
                                         * instancia do ClientHandler atual.
                                         */
                                        mensagem.userID = (uint)userID;
                                        dados.Contents = mensagem;


                                        Logger.Log(String.Format("<{0}> {1}", UserManagement.GetUsername(mensagem.userID), mensagem.message));
                                        Database.SaveUserMessage(mensagem.userID, mensagem.message);

                                        // Broadcast para todos os utilizadores, excepto o que enviou a mensagem
                                        MessageHandler.BroadcastMessage(JsonConvert.SerializeObject(dados), userID);
                                    }
                                    else
                                    {
                                        Logger.Log("Recebido pacote mal formatado do tipo MESSAGE");
                                        Logger.LogQuietly(protocolSI.GetStringFromData());
                                    }

                                    break; 
                                #endregion

                                #region USER_LIST_REQUEST
                                case PacketType.USER_LIST_REQUEST:
                                
                                    Basic_Packet user_list_response_packet = new Basic_Packet();
                                    user_list_response_packet.Type = PacketType.USER_LIST_RESPONSE;

                                    List<UserListItem_Packet> user_list = new List<UserListItem_Packet>();

                                    foreach (UserInfo user in UserManagement.users)
                                    {
                                        UserListItem_Packet this_user = new UserListItem_Packet();
                                        this_user.userID = user.userID;
                                        this_user.username = user.username;
                                        user_list.Add(this_user);
                                    }

                                    user_list_response_packet.Contents = user_list;

                                    string a = JsonConvert.SerializeObject(user_list_response_packet);

                                    byte[] userList_byte_response = protocolSI.Make(ProtocolSICmdType.DATA, a);
                                    networkStream.Write(userList_byte_response, 0, userList_byte_response.Length);
                                    Logger.LogQuietly("Enviada lista de clientes para " + UserManagement.GetUsername((uint)userID));
                                    Logger.LogQuietly(a);
                                    break;
                                #endregion

                                #region MESSAGE_HISTORY_REQUEST
                                case PacketType.MESSAGE_HISTORY_REQUEST:

                                    if(dados.Contents != null)
                                    {
                                        if(UserManagement.GetUser((uint)dados.Contents) != null)
                                        {
                                            List<UserMessageHistoryItem_Packet> messageHistory = Database.GetMessageHistory((uint)dados.Contents);

                                            Basic_Packet messageHistory_response_packet = new Basic_Packet();
                                            messageHistory_response_packet.Type = PacketType.MESSAGE_HISTORY_RESPONSE;
                                            messageHistory_response_packet.Contents = messageHistory;

                                            byte[] messageHistory_byte_response = protocolSI.Make(ProtocolSICmdType.DATA, JsonConvert.SerializeObject(messageHistory_response_packet));
                                            networkStream.Write(messageHistory_byte_response, 0, messageHistory_byte_response.Length);
                                        }
                                        else
                                        {
                                            Logger.Log("Recebido pacote com conteúdos inválidos.");
                                            Logger.LogQuietly(protocolSI.GetStringFromData());
                                        }
                                    }
                                    else
                                    {
                                        Logger.Log("Recebido pacote mal formatado do tipo MESSAGE_HISTORY_REQUEST");
                                        Logger.LogQuietly(protocolSI.GetStringFromData());
                                    }
                                    break; 
                                #endregion

                                #region AUTH_REQUEST
                                case PacketType.AUTH_REQUEST:

                                    if(dados.Contents != null)
                                    {
                                        Auth_Request_Packet auth_request = JsonConvert.DeserializeObject<Auth_Request_Packet>(dados.Contents.ToString());
                                    
                                        if(!String.IsNullOrWhiteSpace(auth_request.username) || !String.IsNullOrWhiteSpace(auth_request.password))
                                        {
                                            // Criar pacote base
                                            Basic_Packet auth_response = new Basic_Packet();
                                            auth_response.Type = PacketType.AUTH_RESPONSE;

                                            // Instanciar resposta
                                            Auth_Response_Packet auth = new Auth_Response_Packet();

                                            Database.ClientInfo dados_cliente;
                                            string errorMessage = null; // necessário para ir buscar a mensagem de erro à função de login

                                            // Se as credenciais estiverem corretas
                                            if (Database.LogUserIn(auth_request.username, auth_request.password, out dados_cliente, out errorMessage))
                                            {
                                                userID = dados_cliente.userID;
                                                Logger.Log(String.Format("Cliente {0} ({1}) juntou-se!", dados_cliente.username, userID));

                                                // Adicionar à lista de utilizadores
                                                UserManagement.AddUser((uint)userID, dados_cliente.username, dados_cliente.imageB64, networkStream);

                                                // Preencher dados de resposta
                                                auth.success = true;
                                                auth.userID = userID;
                                                auth.userImage = dados_cliente.imageB64;
                                                auth.message = null;

                                                if (UserManagement.users.Count > 1)
                                                {
                                                    // Se existirem utilizadores ativos, avisa-los que alguém se juntou
                                                    Basic_Packet user_join = new Basic_Packet();
                                                    user_join.Type = PacketType.USER_JOINED;

                                                    UserJoined_Packet join = new UserJoined_Packet();
                                                    join.userID = dados_cliente.userID;
                                                    join.username = dados_cliente.username;
                                                    join.userImage = dados_cliente.imageB64;
                                                    user_join.Contents = join;

                                                    // Emitr mensagem para todos os utilizadores, excepto o atual
                                                    MessageHandler.BroadcastMessage(JsonConvert.SerializeObject(user_join), userID);
                                                }
                                            }
                                            else
                                            {
                                                // Preencher dados de resposta
                                                auth.success = false;
                                                auth.userID = null;
                                                auth.userImage = null;
                                                auth.message = errorMessage;
                                            }

                                            auth_response.Contents = auth;
                                            byte[] auth_byte_response = protocolSI.Make(ProtocolSICmdType.DATA, JsonConvert.SerializeObject(auth_response));
                                            networkStream.Write(auth_byte_response, 0, auth_byte_response.Length);
                                        }
                                        else
                                        {
                                            Logger.Log("Recebido pacote com conteúdos inválidos");
                                            Logger.LogQuietly(protocolSI.GetStringFromData());
                                        }
                                    }
                                    else
                                    {
                                        Logger.Log("Recebido pacote mal formatado do tipo AUTH_REQUEST");
                                        Logger.LogQuietly(protocolSI.GetStringFromData());
                                    }

                                    break;
                                #endregion

                                #region REGISTER_REQUEST
                                case PacketType.REGISTER_REQUEST:

                                    if(dados.Contents != null)
                                    {
                                        Register_Request_Packet register_request = JsonConvert.DeserializeObject<Register_Request_Packet>(dados.Contents.ToString());

                                        // Criar o pacote base
                                        Basic_Packet register_response = new Basic_Packet();
                                        register_response.Type = PacketType.REGISTER_RESPONSE;

                                        // Instanciar a resposta
                                        Register_Response_Packet register = new Register_Response_Packet();
                                
                                        /**
                                         * Verificações de comprimento máximo do nome de utilizador e de segurança de password
                                         */

                                        if (String.IsNullOrWhiteSpace(register_request.username) || String.IsNullOrWhiteSpace(register_request.password))
                                        {
                                            register.success = false;
                                            register.message = "O campo username ou passowrd devem ser preenchidos";
                                        }
                                        else
                                        {
                                            if (register_request.username.Length <= 15)
                                            {
                                                if (register_request.password.Length >= 8)
                                                {
                                                    if(Database.RegisterUser(register_request.username, register_request.password, register_request.userImage))
                                                    {
                                                        register.success = true;
                                                        register.message = null;
                                                    }
                                                }
                                                else
                                                {
                                                    register.success = false;
                                                    register.message = "A password não cumpre os requisitos minimos (8 caracteres)";
                                                }
                                            }
                                            else
                                            {
                                                register.success = false;
                                                register.message = "O username não pode ser maior do que 15 caracteres";
                                            }
                                        }

                                        register_response.Contents = register;

                                        byte[] register_byte_response = protocolSI.Make(ProtocolSICmdType.DATA, JsonConvert.SerializeObject(register_response));
                                        networkStream.Write(register_byte_response, 0, register_byte_response.Length);
                                    }
                                    else
                                    {
                                        Logger.Log("Recebido pacote mal formatado do tipo REGISTER_REQUEST");
                                        Logger.LogQuietly(protocolSI.GetStringFromData());
                                    }
                                    break; 
                                #endregion

                                default:
                                    Logger.Log("WARN: Recebido packetType inválido. Pedido ignorado");
                                    break;
                            }
                            break;

                        #region EOT
                        case ProtocolSICmdType.EOT:
                            Logger.LogQuietly(String.Format("Ligação de {0} desligada.", Logger.GetSocket(client)));
                            ack = protocolSI.Make(ProtocolSICmdType.ACK);
                            networkStream.Write(ack, 0, ack.Length);
                            if (userID != null)
                            {
                                Logger.Log(String.Format("Cliente {0} ({1}) desligado.", UserManagement.GetUsername((uint)userID), userID));
                                userDisconnectHandler();
                            }
                            break; 
                            #endregion
                    }
                }
                catch (IOException ex) // A ligação foi fechada forçadamente
                {
                    Logger.Log(String.Format("Cliente {0} ({1}) perdeu ligação: Ligação fechada forçadamente", UserManagement.GetUsername((uint)userID), userID));
                    Logger.LogQuietly(ex.Message);
                    userDisconnectHandler();
                    break; // Saltar fora do while loop
                }
                catch (Exception ex) // Outras exceções
                {
                    Logger.Log("Erro: " + ex.Message);
                    if (ex.InnerException != null)
                        Logger.LogQuietly(ex.InnerException.StackTrace);
                    userDisconnectHandler();
                    break; // Saltar fora do while loop
                }
            }
            networkStream.Close();
            client.Close();
        }

        /// <summary>
        /// Remove o utilizador da lista de utilizadores em memória e faz broadcast do sinal de que o utilizador foi disconectado
        /// </summary>
        private void userDisconnectHandler()
        {
            UserManagement.RemoveUser((uint)userID);

            if(UserManagement.users.Count > 0)
            {
                /**
                 * Se houver utilizadores ligados criamos ou pacote,
                 * caso contrário não gastamos tempo de processamento
                 * para tratar disso
                 */

                Basic_Packet saida_utilizador = new Basic_Packet();
                saida_utilizador.Type = PacketType.USER_LEFT;
                saida_utilizador.Contents = userID;

                /**
                 * O utilizador atual já foi removido da lista de utilizadores ativos
                 * por isso, apenas é necessário fazer o broadcast da mensagem
                 */
                MessageHandler.BroadcastMessage(JsonConvert.SerializeObject(saida_utilizador));
            }
        }
    }
}