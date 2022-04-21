using System;

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

        public object Contents { get; set; }
    }

    public class UserJoined_Packet
    {
        public uint userID { get; set; }
        public string username { get; set; }

    }

    public class UserLeft_Packet
    {
        public uint userID { get; set; }
    }

    public class Message_Packet
    {
        public uint userID { get; set; }
        public string message { get; set; }
    }

    public class UserList_Packet
    {
        //Isto deve levar cast (List<UserList_Packet>)contents

        public uint userID { get; set; }
        public string username { get; set; }
    }

    public class UserMessageHistory_Packet
    {
        //Isto deve levar um cast (List<UserMessageHistory_Packet>)Contents

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
        public dynamic getPacketContents(Basic_Packet packet)
        {
            dynamic contents;

            switch (packet.Type)
            {
                case PacketType.USER_JOINED:
                    contents = (UserJoined_Packet)packet.Contents;
                    break;

                case PacketType.USER_LEFT:
                    contents = (UserLeft_Packet)packet.Contents;
                    break;

                case PacketType.MESSAGE:
                    contents = (Message_Packet)packet.Contents;
                    break;

                case PacketType.USER_LIST:
                    contents = (UserList_Packet)packet.Contents;
                    break;

                case PacketType.MESSAGE_HISTORY:
                    contents = (UserMessageHistory_Packet)packet.Contents;
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
