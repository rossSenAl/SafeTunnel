using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SafeTunnel.Services
{
    public class CifradoRsaService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _llavesPath;

        public CifradoRsaService(IWebHostEnvironment environment)
        {
            _environment = environment;
            _llavesPath = Path.Combine(_environment.ContentRootPath, "Data", "RsaKeys");

            // Crear directorio si no existe
            if (!Directory.Exists(_llavesPath))
                Directory.CreateDirectory(_llavesPath);
        }

        /// <summary>
        /// Genera un nuevo par de llaves RSA (pública y privada)
        /// </summary>
        public (string publicKey, string privateKey) GenerarParLlaves()
        {
            using RSA rsa = RSA.Create(2048);

            string publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
            string privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

            // Guardar llaves automáticamente
            GuardarLlaves(publicKey, privateKey);

            return (publicKey, privateKey);
        }

        /// <summary>
        /// Obtiene la llave pública guardada (o la genera si no existe)
        /// </summary>
        public string ObtenerLlavePublica()
        {
            string path = Path.Combine(_llavesPath, "public.key");
            if (File.Exists(path))
                return File.ReadAllText(path);

            var llaves = GenerarParLlaves();
            return llaves.publicKey;
        }

        /// <summary>
        /// Obtiene la llave privada guardada (o la genera si no existe)
        /// </summary>
        private string ObtenerLlavePrivada()
        {
            string path = Path.Combine(_llavesPath, "private.key");
            if (File.Exists(path))
                return File.ReadAllText(path);

            var llaves = GenerarParLlaves();
            return llaves.privateKey;
        }

        /// <summary>
        /// Guarda las llaves en archivos
        /// </summary>
        private void GuardarLlaves(string publicKey, string privateKey)
        {
            File.WriteAllText(Path.Combine(_llavesPath, "public.key"), publicKey);
            File.WriteAllText(Path.Combine(_llavesPath, "private.key"), privateKey);
        }

        /// <summary>
        /// Cifra un mensaje usando la llave pública RSA
        /// </summary>
        public string CifrarConLlavePublica(string textoPlano)
        {
            string publicKey = ObtenerLlavePublica();

            using RSA rsa = RSA.Create();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);

            byte[] data = Encoding.UTF8.GetBytes(textoPlano);
            byte[] encrypted = rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);

            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Descifra un mensaje usando la llave privada RSA
        /// </summary>
        public string DescifrarConLlavePrivada(string textoCifrado)
        {
            string privateKey = ObtenerLlavePrivada();

            using RSA rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);

            byte[] encrypted = Convert.FromBase64String(textoCifrado);
            byte[] decrypted = rsa.Decrypt(encrypted, RSAEncryptionPadding.OaepSHA256);

            return Encoding.UTF8.GetString(decrypted);
        }

        /// <summary>
        /// Firma digitalmente un mensaje usando la llave privada
        /// </summary>
        public string FirmarMensaje(string mensaje)
        {
            string privateKey = ObtenerLlavePrivada();

            using RSA rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);

            byte[] data = Encoding.UTF8.GetBytes(mensaje);
            byte[] signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            return Convert.ToBase64String(signature);
        }

        /// <summary>
        /// Verifica la firma digital de un mensaje usando la llave pública
        /// </summary>
        public bool VerificarFirma(string mensaje, string firma)
        {
            string publicKey = ObtenerLlavePublica();

            using RSA rsa = RSA.Create();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);

            byte[] data = Encoding.UTF8.GetBytes(mensaje);
            byte[] signature = Convert.FromBase64String(firma);

            return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        /// <summary>
        /// Simula el intercambio de llaves (para mostrar en UI)
        /// </summary>
        public string ObtenerHuellaDigitalLlavePublica()
        {
            string publicKey = ObtenerLlavePublica();
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(publicKey));
            return Convert.ToHexString(hash).Substring(0, 16);
        }
    }
}