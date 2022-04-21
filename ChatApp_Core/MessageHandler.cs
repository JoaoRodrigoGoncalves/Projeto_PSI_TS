using System;

namespace ChatApp_Core
{
    public class Basic_Packet
    {
        public string Type { get; set; }

        public object Contents { get; set; }
    }

    public class UserLeft_Packet
    {
        public uint userID { get; set; }
    }
}
