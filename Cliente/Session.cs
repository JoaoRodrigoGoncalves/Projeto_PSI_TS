using EI.SI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Cliente
{
    internal static class Session
    {
        internal static NetworkStream networkStream;
        internal static uint? userID = null;
        internal static string username = null;
        internal static string userImageB64 = null;

        internal static void FecharSessao()
        {
            ProtocolSI protocolSI = new ProtocolSI();
            byte[] close = protocolSI.Make(ProtocolSICmdType.EOT);
            networkStream.Write(close, 0, close.Length);
            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            networkStream.Close();
        }
    }
}
