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

            if (!Directory.Exists(_llavesPath))
                Directory.CreateDirectory(_llavesPath);
        }

        private (string publicKey, string privateKey) GenerarParLlaves()
        {
            using RSA rsa = RSA.Create(2048);
            string publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
            string privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

            File.WriteAllText(Path.Combine(_llavesPath, "public.key"), publicKey);
            File.WriteAllText(Path.Combine(_llavesPath, "private.key"), privateKey);

            return (publicKey, privateKey);
        }

        public string ObtenerLlavePublica()
        {
            string path = Path.Combine(_llavesPath, "public.key");
            if (File.Exists(path))
                return File.ReadAllText(path);

            var llaves = GenerarParLlaves();
            return llaves.publicKey;
        }

        private string ObtenerLlavePrivada()
        {
            string path = Path.Combine(_llavesPath, "private.key");
            if (File.Exists(path))
                return File.ReadAllText(path);

            var llaves = GenerarParLlaves();
            return llaves.privateKey;
        }

        public string CifrarConLlavePublica(string textoPlano)
        {
            if (string.IsNullOrEmpty(textoPlano))
                return string.Empty;

            string publicKey = ObtenerLlavePublica();

            using RSA rsa = RSA.Create();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);

            byte[] data = Encoding.UTF8.GetBytes(textoPlano);
            byte[] encrypted = rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);

            return Convert.ToBase64String(encrypted);
        }

        public string FirmarMensaje(string mensaje)
        {
            if (string.IsNullOrEmpty(mensaje))
                return string.Empty;

            string privateKey = ObtenerLlavePrivada();

            using RSA rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);

            byte[] data = Encoding.UTF8.GetBytes(mensaje);
            byte[] signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            return Convert.ToBase64String(signature);
        }

        public string ObtenerHuellaDigital()
        {
            string publicKey = ObtenerLlavePublica();
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(publicKey));
            return Convert.ToHexString(hash).Substring(0, 16);
        }
    }
}