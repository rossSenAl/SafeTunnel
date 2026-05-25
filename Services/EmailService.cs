using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;

namespace SafeTunnel.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService>? _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService>? logger = null)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> EnviarCodigoAsync(string destino, string codigo)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var portStr = _configuration["EmailSettings:Port"];
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderPassword = _configuration["EmailSettings:SenderPassword"];
                var useSSL = _configuration["EmailSettings:EnableSSL"];

                if (string.IsNullOrWhiteSpace(smtpServer))
                    throw new InvalidOperationException("Falta SmtpServer en configuración");
                if (!int.TryParse(portStr, out int port))
                    throw new InvalidOperationException("Puerto SMTP inválido");
                if (string.IsNullOrWhiteSpace(senderEmail))
                    throw new InvalidOperationException("Falta SenderEmail");
                if (string.IsNullOrWhiteSpace(senderPassword))
                    throw new InvalidOperationException("Falta SenderPassword");

                // Determinar opción de seguridad según puerto y configuración
                SecureSocketOptions options;
                if (port == 465)
                    options = SecureSocketOptions.SslOnConnect;
                else if (port == 587)
                    options = SecureSocketOptions.StartTls;
                else
                    options = bool.TryParse(useSSL, out var ssl) && ssl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.Auto;

                _logger?.LogInformation("Conectando a {Server}:{Port} con {Options}", smtpServer, port, options);

                using (var client = new SmtpClient())
                {
                    // Para desarrollo: ignorar errores de certificado (solo si es necesario)
                    // client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    await client.ConnectAsync(smtpServer, port, options);
                    await client.AuthenticateAsync(senderEmail, senderPassword);

                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("SafeTunnel", senderEmail));
                    message.To.Add(new MailboxAddress("Usuario", destino));
                    message.Subject = "🔐 Código de verificación SafeTunnel";

                    message.Body = new TextPart("html")
                    {
                        Text = $@"
                            <h2>Código de acceso</h2>
                            <p>Tu código de verificación es:</p>
                            <h1 style='font-size: 32px; letter-spacing: 4px; background: #f0f0f0; padding: 10px; text-align: center;'>{codigo}</h1>
                            <p>Este código expira en 5 minutos.</p>
                            <p>Si no solicitaste este código, ignora este mensaje.</p>
                            <hr/>
                            <small>SafeTunnel - Simulador de seguridad VPN</small>
                        "
                    };

                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                _logger?.LogInformation("Correo enviado exitosamente a {Destino}", destino);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error enviando correo a {Destino}", destino);
                Console.WriteLine($"Error detallado: {ex}");
                return false;
            }
        }
    }
}