using Core;
using EI.SI;

namespace Servidor
{
    internal class MessageHandler
    {
        /// <summary>
        /// Envia a mensagem indicada para todos os utilizadores ativos
        /// </summary>
        /// <param name="message">Mensagem a passar ao <see cref="ProtocolSI"/></param>
        /// <param name="excludedUser">id do utilizador a excluir do broadcast</param>
        public static void BroadcastMessage(string message, uint? excludedUser = null)
        {
            UserManagement management = new UserManagement();
            ProtocolSI SI = new ProtocolSI();

            byte[] data = SI.Make(ProtocolSICmdType.DATA, message);

            if(excludedUser.HasValue) // Verificar se há um utilizador a ser excluído
            {
                foreach (UserInfo user in management.users)
                {
                    if (excludedUser.Value == user.userID)
                        continue; // Passa o utilizador excluído à frente

                    user.userStream.Write(data, 0, data.Length);
                }
            }
            else
            {
                foreach (UserInfo user in management.users)
                {
                    user.userStream.Write(data, 0, data.Length);
                }
            }
        }
    }
}
