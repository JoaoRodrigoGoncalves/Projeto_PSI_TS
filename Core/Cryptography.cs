using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Core
{
    public class Cryptography
    {
        /**
         * Código referente a chaves assimétricas
         */

        /// <summary>
        /// Gera e guarda a chave pública e privada
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
        /// Obtém o CspBlob apenas com a chave pública
        /// </summary>
        /// <returns><see cref="byte[]"/> com chave pública</returns>
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
        /// Obtém o CspBlob com a chave pública/privada
        /// </summary>
        /// <returns><see cref="byte[]"/> com o key-pair de chave pública e privada</returns>
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
        /// Encripa texto com a chave pública na blob csp
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
        /// Desencripta dados que foram encriptados com a chave pública do cliente
        /// </summary>
        /// <param name="CspBlob">Chave pública e privada do cliente</param>
        /// <param name="dadosBase64">Dados a desencriptar</param>
        /// <returns><see cref="byte[]"/> dados desencriptados</returns>
        public static byte[] privateKeyDecrypt(string dadosBase64)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.ImportCspBlob(getPrivateKey());
            byte[] dados = Convert.FromBase64String(dadosBase64);
            return rsa.Decrypt(dados, true);
        }

        /// <summary>
        /// Converte um objeto numa assinatura
        /// </summary>
        /// <param name="objeto">Objeto a assinar</param>
        /// <returns>Assinatura relativa ao objeto</returns>
        public static byte[] converterDadosNumaAssinatura(object objeto)
        {
            byte[] assinatura;

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();// Inicializa a encriptação
            rsa.ImportCspBlob(getPrivateKey());// Importa o Blob - Representação da chave Privada

            string json = JsonConvert.SerializeObject(objeto);// Converte um objeto em json 

            using (SHA256 sha256 = SHA256.Create())// Criar uma instancia da hash 
            {
                byte[] dados = Encoding.UTF8.GetBytes(json);// Converte o json em bytes
                assinatura = rsa.SignData(dados, sha256);// Assinar os dados
            }
            return assinatura;// Retorna a assinatura
        }

        /// <summary>
        /// Valida uma assinatura
        /// </summary>
        /// <param name="CspBlob">Chave pública do sistema que assinou</param>
        /// <param name="objeto">Objeto a ser utilizado na validação</param>
        /// <param name="assinatura">Assinatura a validar</param>
        /// <returns></returns>
        public static bool validarAssinatura(byte[] CspBlob, object objeto, byte[] assinatura)
        {
            bool verify;
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();// Inicializa a encriptação
            rsa.ImportCspBlob(CspBlob);// Importa o Blob - Representação da chave Privada

            using (SHA256 sha256 = SHA256.Create())// Criar uma instancia da hash
            {
                byte[] dados = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(objeto));// Converte o json em bytes
                verify = rsa.VerifyData(dados, sha256, assinatura);// Verifica a vericidade dos dados
            }
            return verify;// Retorna o estado
        }

        /**
         * Código referente a chaves simétricas
         */

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

        /// <summary>
        /// Encripta uma string de texto com a chave secreta
        /// </summary>
        /// <param name="aes">Objeto <see cref="AesCryptoServiceProvider"/> preenchido com vetor de inicialização e chave privada</param>
        /// <param name="text">Texto a encriptar</param>
        /// <returns>String cifrada em Base64</returns>
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
        /// Desincripta uma string de texto cifrado em base 64 com a chave secreta
        /// </summary>
        /// <param name="aes">Objeto <see cref="AesCryptoServiceProvider"/> preenchido com vetor de inicialização e chave privada</param>
        /// <param name="textCrifradoB64"></param>
        /// <returns>String desincriptada</returns>
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
    }
}
