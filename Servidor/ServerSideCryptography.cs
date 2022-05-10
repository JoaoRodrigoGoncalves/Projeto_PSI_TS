using Core;
using System.Security.Cryptography;

namespace Servidor
{
    internal class ServerSideCryptography
    {
        internal static AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
        private static string pwd = null;

        internal static string getAESSecret()
        {
            if (pwd == null)
            {
                pwd = RandomPassword();
                aes.IV = Cryptography.CreateIV(pwd);
                aes.Key = Cryptography.CreatePrivateKey(pwd);
            }
            return pwd;
        }

        internal static string RandomPassword()
        {
            // https://riptutorial.com/csharp/example/32641/cryptographically-secure-random-data

            char[] valid = ("abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ").ToCharArray();

            var rnd = new byte[24];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(rnd);
            }

            var chars = new char[24];
            for (var i = 0; i < 24; i++)
                chars[i] = valid[rnd[i] % valid.Length];

            return new string(chars);
        }
    }
}
