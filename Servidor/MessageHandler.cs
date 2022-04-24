using Core;
using EI.SI;
using System;

namespace Servidor
{
    internal class MessageHandler
    {
        /// <summary>
        /// Envia a mensagem indicada para todos os utilizadores ativos
        /// </summary>
        /// <param name="message">Mensagem a passar ao <see cref="ProtocolSI"/></param>
        /// <param name="excludedUser">ID do utilizador a excluir do broadcast</param>
        public static void BroadcastMessage(string message, uint? excludedUser = null)
        {
            UserManagement management = new UserManagement();
            ProtocolSI SI = new ProtocolSI();

            byte[] data = SI.Make(ProtocolSICmdType.DATA, message);

            if(excludedUser.HasValue) // Verificar se há um utilizador a ser excluído
            {
                Logger.LogQuietly(String.Format("Broadcast excluíndo {0}", excludedUser));
                foreach (UserInfo user in management.users)
                {
                    if (excludedUser.Value == user.userID)
                        continue; // Passa o utilizador excluído à frente

                    user.userStream.Write(data, 0, data.Length);
                }
            }
            else
            {
                Logger.LogQuietly("Broadcast total");
                foreach (UserInfo user in management.users)
                {
                    user.userStream.Write(data, 0, data.Length);
                }
            }
        }
    }
}
