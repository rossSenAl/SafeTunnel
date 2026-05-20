using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SafeTunnel.Services
{
    public class SeguridadVpnService
    {
        private const string Key = "SafeTunnel-Clave-Demo-2026";

        private byte[] ObtenerClave()
        {
            return SHA256.HashData(Encoding.UTF8.GetBytes(Key));
        }

        public string Cifrar(string texto)
        {
            byte[] keyBytes = ObtenerClave();

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.GenerateIV();

                byte[] iv = aes.IV;

                using (var encryptor = aes.CreateEncryptor(aes.Key, iv))
                using (var ms = new MemoryStream())
                {
                    ms.Write(iv, 0, iv.Length);

                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(texto);
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public string Descifrar(string textoCifrado)
        {
            byte[] fullCipher = Convert.FromBase64String(textoCifrado);

            byte[] iv = new byte[16];
            byte[] cipher = new byte[fullCipher.Length - iv.Length];

            Array.Copy(fullCipher, iv, iv.Length);
            Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            byte[] keyBytes = ObtenerClave();

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream(cipher))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        public string GenerarHash(string texto)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(texto);
            byte[] hash = SHA256.HashData(bytes);

            return Convert.ToHexString(hash);
        }

        public static string Encrypt(string text)
        {
            var service = new SeguridadVpnService();
            return service.Cifrar(text);
        }

        public static string Decrypt(string encryptedText)
        {
            var service = new SeguridadVpnService();
            return service.Descifrar(encryptedText);
        }
    }
}