using System;

namespace Core
{
    public enum PacketType
    {
        USER_JOINED,
        USER_LEFT,
        MESSAGE,
        USER_LIST_REQUEST,
        USER_LIST_RESPONSE,
        MESSAGE_HISTORY_REQUEST,
        MESSAGE_HISTORY_RESPONSE,
        AUTH_REQUEST,
        AUTH_RESPONSE,
        REGISTER_REQUEST,
        REGISTER_RESPONSE
    }

    /// <summary>
    /// Pacote base que encapsula o tipo de informação a ser pedida/enviada e os dados relativos a essa informação
    /// </summary>
    public class Basic_Packet
    {
        public PacketType Type { get; set; }

        /// <summary>
        /// <para>Conteúdos a enviar.</para>
        /// <para>Deve enviar-se <see cref="UserJoined_Packet"/>, <see cref="Message_Packet"/>, <see cref="UserListItem_Packet"/>, <see cref="UserMessageHistoryItem_Packet"/>, <see cref="Auth_Response_Packet"/> e <see cref="Register_Response_Packet"/>.</para>
        /// <para>Para enviar a informação de saída de um utilizador do chat, colocar apenas a <see cref="uint"/> desse utilizador.</para>
        /// <para>Quando se está a dar serialize, é visto corretamente como um objeto. Quando se está a dar deserialize do pacote todo, é tomado como String e precisa de ser deserialized individualmente</para>
        /// </summary>
        public object Contents { get; set; }
    }

    /// <summary>
    /// Esta classe serve apenas para propagar aos outros utilizadores que um utilizador se juntou.
    /// A ligação do utilizador é tratada a nível do protocolo TCP
    /// </summary>
    public class UserJoined_Packet
    {
        public uint userID { get; set; }
        public string username { get; set; }
        public uint? userImage { get; set; }
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
        public uint? userImage { get; set; }
    }

    public class UserMessageHistoryItem_Packet
    {
        public string message { get; set; }
        public DateTime time { get; set; }
    }

    public class Auth_Response_Packet
    {
        public bool success { get; set; }
        public string message { get; set; }
        public uint? userID { get; set; }
        public uint? userImage { get; set; }
    }

    public class Auth_Request_Packet
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public class Register_Response_Packet
    {
        public bool success { get; set; }
        public string message { get; set; }
    }

    public class Register_Request_Packet
    {
        public string username { get; set; }
        public string password { get; set; }
        public uint? userImage { get; set; }
    }
}
