using EI.SI;
using Core;
using System;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;

namespace Servidor
{
    internal class ClientHandler
    {
        private TcpClient client;
        private uint? userID = null; // Pode ser null porque só é atribuído um ID ao utilizador após a autenticação

        internal ClientHandler(TcpClient client)
        {
            this.client = client;
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
            UserManagement userManagement = new UserManagement();

            while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT)
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
                            case PacketType.MESSAGE:

                                // TODO: Extrair os dados para o log

                                // Broadcast para todos os utilizadores, excepto o que enviou a mensagem
                                MessageHandler.BroadcastMessage(JsonConvert.SerializeObject(dados), userID);
                                break;

                            case PacketType.USER_LIST_REQUEST:
                                // TODO: Implementar
                                Console.WriteLine("WARN: Recebido packetType ainda não implementado");
                                break;

                            case PacketType.MESSAGE_HISTORY_REQUEST:
                                // TODO: Implementar
                                Console.WriteLine("WARN: Recebido packetType ainda não implementado");
                                break;

                            case PacketType.AUTH_REQUEST:

                                Auth_Request_Packet auth_request = JsonConvert.DeserializeObject<Auth_Request_Packet>(dados.Contents.ToString());

                                // TODO: Verificar credenciais e obter imagem da base de dados

                                Basic_Packet auth_response = new Basic_Packet();
                                auth_response.Type = PacketType.AUTH_RESPONSE;

                                Console.WriteLine("Cliente {0} juntou-se!", auth_request.username);
                                userID = userManagement.GenerateUserID(); // Obter um id para este utilizador
                                userManagement.AddUser((uint)userID, auth_request.username, null); // Adicionar à lista de utilizadores

                                // Preencher dados de resposta
                                Auth_Response_Packet auth = new Auth_Response_Packet();
                                auth.success = true;
                                auth.userID = userID;
                                auth.userImage = null;
                                auth_response.Contents = auth;

                                byte[] data = protocolSI.Make(ProtocolSICmdType.DATA, JsonConvert.SerializeObject(auth_response));
                                networkStream.Write(data, 0, data.Length);

                                // Avisar todos os utilizadores de que alguém se juntou
                                Basic_Packet user_join = new Basic_Packet();
                                user_join.Type = PacketType.USER_JOINED;

                                UserJoined_Packet join = new UserJoined_Packet();
                                join.userID = (uint)userID;
                                join.username = auth_request.username;
                                user_join.Contents = join;

                                // Emitr mensagem para todos os utilizadores, excepto o atual
                                MessageHandler.BroadcastMessage(JsonConvert.SerializeObject(user_join), userID);

                                break;

                            case PacketType.REGISTER_REQUEST:
                                // TODO: Implementar
                                Console.WriteLine("WARN: Recebido packetType ainda não implementado");
                                break;

                            default:
                                Console.WriteLine("WARN: Recebido packetType inválido. A ignorar pedido");
                                break;
                        }
                        break;

                    case ProtocolSICmdType.EOT:
                        ack = protocolSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(ack, 0, ack.Length);
                        if(userID != null)
                        {
                            Console.WriteLine("Cliente {0} desligado.", userManagement.GetUsername((uint)userID));
                            userManagement.RemoveUser((uint)userID);

                            Basic_Packet saida_utilizador = new Basic_Packet();
                            saida_utilizador.Type = PacketType.USER_LEFT;
                            saida_utilizador.Contents = userID;

                            /**
                             * O utilizador atual já foi removido da lista de utilizadores ativos
                             * por isso, apenas é necessário fazer o broadcast da mensagem
                             */
                            MessageHandler.BroadcastMessage(JsonConvert.SerializeObject(saida_utilizador));
                        }
                        break;
                }
            }
            networkStream.Close();
            client.Close();
        }
    }
}