using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Core
{
    public class Cryptography
    {
        /// <summary>
        /// Gera e guarda a chave publica e privada
        /// </summary>
        public static void generateKeys()
        {
            var parameters = new CspParameters
            {
                KeyContainerName = "Keys"
            };
            new RSACryptoServiceProvider(parameters);
        }

        /// <summary>
        /// Obtém o CspBlob apenas com a chave publica
        /// </summary>
        /// <returns></returns>
        public static byte[] getPublicKey()
        {
            var parameters = new CspParameters
            {
                KeyContainerName = "Keys"
            };
            var rsa = new RSACryptoServiceProvider(parameters);
            return rsa.ExportCspBlob(false);
        }

        /// <summary>
        /// Obtém o CspBlob com a chave publica/privada
        /// </summary>
        /// <returns></returns>
        public static byte[] getPrivateKey()
        {
            var parameters = new CspParameters
            {
                KeyContainerName = "Keys"
            };
            var rsa = new RSACryptoServiceProvider(parameters);
            return rsa.ExportCspBlob(true);
        }

        /// <summary>
        /// Encripa texto com a chave publica na blob csp
        /// </summary>
        /// <param name="CspBlob"></param>
        /// <param name="text"></param>
        /// <returns>Base64</returns>
        public static string publicKeyEncrypt(byte[] CspBlob, string text)
        {
            // https://stackoverflow.com/a/5846621/10935376 acedido em 09/05/2022
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.ImportCspBlob(CspBlob);
            return Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes(text), true));
        }

        /// <summary>
        /// Desencripta dados que foram encriptados com a chave publica do cliente
        /// </summary>
        /// <param name="CspBlob">Chave publica e privada do cliente</param>
        /// <param name="dadosBase64">Dados a desencriptar</param>
        /// <returns></returns>
        public static byte[] Decrypt(string dadosBase64)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.ImportCspBlob(getPrivateKey());
            byte[] dados = Convert.FromBase64String(dadosBase64);
            return rsa.Decrypt(dados, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string AESEncrypt(AesCryptoServiceProvider aes, string text)
        {
            byte[] textDecifrado = Encoding.UTF8.GetBytes(text);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(textDecifrado, 0, textDecifrado.Length);
            cs.Close();
            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textCrifradoB64"></param>
        /// <returns></returns>
        public static string AESDecrypt(AesCryptoServiceProvider aes, string textCrifradoB64)
        {
            byte[] textCifrado = Convert.FromBase64String(textCrifradoB64);
            MemoryStream ms = new MemoryStream(textCifrado);
            CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            byte[] textDecifrado = new byte[ms.Length];
            int bytesLidos = cs.Read(textDecifrado, 0, textDecifrado.Length);
            cs.Close();
            return Encoding.UTF8.GetString(textDecifrado, 0, bytesLidos);
        }

        /// <summary>
        /// Cria um vetor de inicialização baseeado na chave secreta
        /// </summary>
        /// <param name="secret_key"></param>
        /// <returns></returns>
        public static byte[] CreateIV(string secret_key)
        {
            byte[] salt = new byte[] { 1, 5, 8, 4, 7, 3, 6, 5 };
            Rfc2898DeriveBytes pwgne = new Rfc2898DeriveBytes(secret_key, salt, 1000);
            return pwgne.GetBytes(16);
        }

        /// <summary>
        /// Cria uma chave privada (AES) com base na chave secreta
        /// </summary>
        /// <param name="secret_key"></param>
        /// <returns></returns>
        public static byte[] CreatePrivateKey(string secret_key)
        {
            byte[] salt = new byte[] { 5, 4, 2, 3, 6, 5, 1, 9 };
            Rfc2898DeriveBytes pwgne = new Rfc2898DeriveBytes(secret_key, salt, 1000);
            return pwgne.GetBytes(16);
        }
    }
}
