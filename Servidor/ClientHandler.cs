using Core;
using EI.SI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Servidor
{
    internal class ClientHandler
    {
        private TcpClient client;
        private byte[] clientPublicKey = null; // É guardada aqui pois é apenas necessária para o login
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
                        case ProtocolSICmdType.PUBLIC_KEY:
                            clientPublicKey = Convert.FromBase64String(protocolSI.GetStringFromData()); //Obter os bytes da string base64
                            ack = protocolSI.Make(ProtocolSICmdType.ACK);
                            networkStream.Write(ack, 0, ack.Length);

                            byte[] pubKey_response = protocolSI.Make(ProtocolSICmdType.SECRET_KEY, Cryptography.publicKeyEncrypt(clientPublicKey, ServerSideCryptography.getAESSecret()));
                            networkStream.Write(pubKey_response, 0, pubKey_response.Length);

                            break;

                        case ProtocolSICmdType.SYM_CIPHER_DATA:
                            //Obter os dados para a estrutura antes de enviar o ack
                            Basic_Packet dados = JsonConvert.DeserializeObject<Basic_Packet>(Cryptography.AESDecrypt(ServerSideCryptography.aes, protocolSI.GetStringFromData()));
                            // Enviar o ack
                            ack = protocolSI.Make(ProtocolSICmdType.ACK);
                            networkStream.Write(ack, 0, ack.Length);

                            // Verificar qual o tipo de pedido
                            switch (dados.Type)
                            {
                                #region MESSAGE
                                case PacketType.MESSAGE:

                                    if (dados.Contents != null)
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
                                        if (UserManagement.users.Count > 1)
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
                                        this_user.userImage = user.userImage;
                                        user_list.Add(this_user);
                                    }

                                    user_list_response_packet.Contents = user_list;

                                    string sentUserData = JsonConvert.SerializeObject(user_list_response_packet);
                                    byte[] userList_byte_response = protocolSI.Make(ProtocolSICmdType.SYM_CIPHER_DATA, Cryptography.AESEncrypt(ServerSideCryptography.aes, sentUserData));
                                    networkStream.Write(userList_byte_response, 0, userList_byte_response.Length);
                                    Logger.LogQuietly("Enviada lista de clientes para " + UserManagement.GetUsername((uint)userID));
                                    Logger.LogQuietly(sentUserData);
                                    break;
                                #endregion

                                #region MESSAGE_HISTORY_REQUEST
                                case PacketType.MESSAGE_HISTORY_REQUEST:

                                    if (dados.Contents != null)
                                    {
                                        if (uint.TryParse(dados.Contents.ToString(), out uint requested_user_id))
                                        {
                                            if (Database.UserExists(requested_user_id))
                                            {
                                                List<UserMessageHistoryItem_Packet> messageHistory = Database.GetMessageHistory(requested_user_id);

                                                Basic_Packet messageHistory_response_packet = new Basic_Packet();
                                                messageHistory_response_packet.Type = PacketType.MESSAGE_HISTORY_RESPONSE;
                                                if (messageHistory.Count > 0)
                                                {
                                                    messageHistory_response_packet.Contents = messageHistory;
                                                }
                                                else
                                                {
                                                    messageHistory_response_packet.Contents = null;
                                                }

                                                byte[] messageHistory_byte_response = protocolSI.Make(ProtocolSICmdType.SYM_CIPHER_DATA, Cryptography.AESEncrypt(ServerSideCryptography.aes, JsonConvert.SerializeObject(messageHistory_response_packet)));
                                                networkStream.Write(messageHistory_byte_response, 0, messageHistory_byte_response.Length);
                                            }
                                            else
                                            {
                                                Logger.Log("Utilizador pedido não existe");
                                                Logger.LogQuietly(protocolSI.GetStringFromData());
                                            }
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

                                    if (dados.Contents != null)
                                    {
                                        Auth_Request_Packet auth_request = JsonConvert.DeserializeObject<Auth_Request_Packet>(dados.Contents.ToString());

                                        if (!String.IsNullOrWhiteSpace(auth_request.username) || !String.IsNullOrWhiteSpace(auth_request.password))
                                        {
                                            // Criar pacote base
                                            Basic_Packet auth_response = new Basic_Packet();
                                            auth_response.Type = PacketType.AUTH_RESPONSE;

                                            // Instanciar resposta
                                            Auth_Response_Packet auth = new Auth_Response_Packet();

                                            // Se as credenciais estiverem corretas
                                            if (Database.LogUserIn(auth_request.username, auth_request.password, out Database.ClientInfo dados_cliente, out string errorMessage))
                                            {

                                                if (UserManagement.GetUser((uint)dados_cliente.userID) == null)
                                                {
                                                    userID = dados_cliente.userID; // Só podemos dar userID à sessão depois de establecer que não é uma dupla ligação
                                                    Logger.Log(String.Format("Cliente {0} ({1}) juntou-se!", dados_cliente.username, userID));

                                                    // Adicionar à lista de utilizadores
                                                    UserManagement.AddUser((uint)userID, dados_cliente.username, dados_cliente.userImage, networkStream);

                                                    // Preencher dados de resposta
                                                    auth.success = true;
                                                    auth.userID = userID;
                                                    auth.userImage = dados_cliente.userImage;
                                                    auth.message = null;

                                                    if (UserManagement.users.Count > 1)
                                                    {
                                                        // Se existirem utilizadores ativos, avisa-los que alguém se juntou
                                                        Basic_Packet user_join = new Basic_Packet();
                                                        user_join.Type = PacketType.USER_JOINED;

                                                        UserJoined_Packet join = new UserJoined_Packet();
                                                        join.userID = dados_cliente.userID;
                                                        join.username = dados_cliente.username;
                                                        join.userImage = dados_cliente.userImage;
                                                        user_join.Contents = join;

                                                        // Emitr mensagem para todos os utilizadores, excepto o atual
                                                        MessageHandler.BroadcastMessage(JsonConvert.SerializeObject(user_join), userID);
                                                    }
                                                }
                                                else
                                                {
                                                    Logger.Log(String.Format("Tentativa de dupla sessão para o utilizador {0} em {1}", auth_request.username, Logger.GetSocket(this.client)));

                                                    auth.success = false;
                                                    auth.userID = null;
                                                    auth.userImage = null;
                                                    auth.message = "Dupla sessão não permitida.";
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
                                            byte[] auth_byte_response = protocolSI.Make(ProtocolSICmdType.SYM_CIPHER_DATA, Cryptography.AESEncrypt(ServerSideCryptography.aes, JsonConvert.SerializeObject(auth_response)));
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

                                    if (dados.Contents != null)
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

                                        register.success = false;
                                        if (String.IsNullOrWhiteSpace(register_request.username) || String.IsNullOrWhiteSpace(register_request.password))
                                        {
                                            register.message = "O campo username ou passowrd devem ser preenchidos";
                                        }
                                        else
                                        {
                                            if (register_request.username.Length <= 15)
                                            {
                                                if (register_request.password.Length >= 8)
                                                {
                                                    if (!Database.UserExists(register_request.username))
                                                    {
                                                        if (Database.RegisterUser(register_request.username, register_request.password, register_request.userImage))
                                                        {
                                                            register.success = true;
                                                            register.message = null;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        register.message = "Já existe um utilizador com esse nome de utilizador";
                                                    }
                                                }
                                                else
                                                {
                                                    register.message = "A password não cumpre os requisitos minimos (8 caracteres)";
                                                }
                                            }
                                            else
                                            {
                                                register.message = "O username não pode ser maior do que 15 caracteres";
                                            }
                                        }

                                        register_response.Contents = register;

                                        byte[] register_byte_response = protocolSI.Make(ProtocolSICmdType.SYM_CIPHER_DATA, Cryptography.AESEncrypt(ServerSideCryptography.aes, JsonConvert.SerializeObject(register_response)));
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
                    Logger.LogQuietly(ex.Message);
                    if (userID.HasValue)
                    {
                        Logger.Log(String.Format("Cliente {0} ({1}) perdeu ligação: Ligação fechada forçadamente", UserManagement.GetUsername((uint)userID), userID));
                        userDisconnectHandler();
                    }
                    break; // Saltar fora do while loop
                }
                catch (Exception ex)
                {
                    Logger.Log(String.Format("Ocorreu um erro inesperado com a ligação ao utilizador {0}: {1}", userID, ex.Message));
                    if (ex.InnerException != null)
                    {
                        Logger.Log(ex.InnerException.Message);
                    }
                    try
                    {
                        // Enviar EOT para o utilizador
                        byte[] force_close = protocolSI.Make(ProtocolSICmdType.EOT);
                        networkStream.Write(force_close, 0, force_close.Length);
                        networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length); // Esta linha pode ou não ter de sair daqui
                    }
                    catch (Exception new_ex)
                    {
                        Logger.Log("Não foi possível enviar um EOT para o utilizador: " + new_ex.Message);
                    }
                    finally
                    {
                        userDisconnectHandler();
                    }
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
            if (userID.HasValue)
            {
                UserManagement.RemoveUser((uint)userID);

                if (UserManagement.users.Count > 0)
                {
                    /**
                     * Se houver utilizadores ligados criamos ou pacote,
                     * caso contrário não gastamos tempo de processamento
                     * para tratar disso
                     */

                    Basic_Packet saida_utilizador = new Basic_Packet();
                    saida_utilizador.Type = PacketType.USER_LEFT;
                    saida_utilizador.Contents = (uint)userID;

                    /**
                     * O utilizador atual já foi removido da lista de utilizadores ativos
                     * por isso, apenas é necessário fazer o broadcast da mensagem
                     */
                    MessageHandler.BroadcastMessage(JsonConvert.SerializeObject(saida_utilizador));
                }
            }
        }
    }
}