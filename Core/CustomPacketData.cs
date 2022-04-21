using System;
using System.Collections.Generic;

namespace Core
{
    public enum PacketType
    {
        USER_JOINED,
        USER_LEFT,
        MESSAGE,
        USER_LIST,
        MESSAGE_HISTORY,
        AUTH,
        REGISTER
    }

    public class Basic_Packet
    {
        public PacketType Type { get; set; }

        /// <summary>
        /// Conteúdos a enviar.
        /// Deve enviar-se <see cref="UserJoined_Packet"/>, <see cref="Message_Packet"/>, <see cref="UserListItem_Packet"/>, <see cref="UserMessageHistoryItem_Packet"/>, <see cref="Auth_Packet"/> e <see cref="Register_Packet"/>.
        /// Para enviar a informação de saída de um utilizador do chat, colocar apenas a <see cref="uint"/> desse utilizador.
        /// </summary>
        public object Contents { get; set; }
    }

    public class UserJoined_Packet
    {
        public uint userID { get; set; }
        public string username { get; set; }

    }

    public class Message_Packet
    {
        public uint userID { get; set; }
        public string message { get; set; }
    }

    public class UserListItem_Packet
    {
        public uint userID { get; set; }
        public string username { get; set; }
    }

    public class UserMessageHistoryItem_Packet
    {
        public string message { get; set; }
        public DateTime time { get; set; }
    }

    public class Auth_Packet
    {
        public bool success { get; set; }
        public string message { get; set; }
    }

    public class Register_Packet
    {
        public bool success { get; set; }
        public string message { get; set; }
    }

    public class CustomPacketData
    {
        /// <summary>
        /// Aplica a estrutura de dados correta
        /// </summary>
        /// <param name="packet">Pacote "original" com os dados</param>
        /// <returns>Estrutura correta</returns>
        /// <exception cref="Exception">Caso o tipo de pacote indicado não exista</exception>
        public dynamic GetPacketContents(Basic_Packet packet)
        {
            dynamic contents;

            switch (packet.Type)
            {
                case PacketType.USER_JOINED:
                    contents = (UserJoined_Packet)packet.Contents;
                    break;

                case PacketType.USER_LEFT:
                    contents = (uint)packet.Contents;
                    break;

                case PacketType.MESSAGE:
                    contents = (Message_Packet)packet.Contents;
                    break;

                case PacketType.USER_LIST:
                    contents = (List<UserListItem_Packet>)packet.Contents;
                    break;

                case PacketType.MESSAGE_HISTORY:
                    contents = (List<UserMessageHistoryItem_Packet>)packet.Contents;
                    break;

                case PacketType.AUTH:
                    contents = (Auth_Packet)packet.Contents;
                    break;

                case PacketType.REGISTER:
                    contents = (Register_Packet)packet.Contents;
                    break;

                default:
                    throw new Exception("PacketType inválido");
            }
            return contents;
        }
    }
}
