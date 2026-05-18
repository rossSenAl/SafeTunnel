using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SafeTunnel.Services
{
    public class SeguridadVpnService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public SeguridadVpnService()
        {
            string claveDemo = "SafeTunnel-Clave-Demo-2026";
            string ivDemo = "SafeTunnel-IV-Demo";

            _key = SHA256.HashData(Encoding.UTF8.GetBytes(claveDemo));
            _iv = MD5.HashData(Encoding.UTF8.GetBytes(ivDemo));
        }

        public string Cifrar(string texto)
        {
            using Aes aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using MemoryStream ms = new MemoryStream();
            using CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            using StreamWriter sw = new StreamWriter(cs);

            sw.Write(texto);
            sw.Close();

            return Convert.ToBase64String(ms.ToArray());
        }

        public string Descifrar(string textoCifrado)
        {
            byte[] buffer = Convert.FromBase64String(textoCifrado);

            using Aes aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using MemoryStream ms = new MemoryStream(buffer);
            using CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using StreamReader sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }

        public string GenerarHash(string texto)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(texto);
            byte[] hash = SHA256.HashData(bytes);

            return Convert.ToHexString(hash);
        }
    }
}