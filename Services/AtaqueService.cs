using SafeTunnel.Models;

namespace SafeTunnel.Services
{
    public class AtaqueService
    {
        private readonly Random _random = new Random();

        public (bool exitoso, string alerta, string? mensajeInterceptado, int latenciaAdicional, string? ipSpoofeada)
            EjecutarAtaque(TipoAtaque ataque, string mensajeOriginal, int latenciaBase)
        {
            switch (ataque)
            {
                case TipoAtaque.MITM:
                    string mensajeModificado = ModificarMensajeMITM(mensajeOriginal);
                    return (true,
                            "⚠️ ATAQUE MITM: El mensaje fue interceptado y modificado.",
                            mensajeModificado, 50, null);

                case TipoAtaque.DoS:
                    if (_random.Next(1, 100) <= 40)
                        return (false, "❌ ATAQUE DoS: Paquete perdido.", null, 2000, null);
                    else
                        return (true, "⚠️ ATAQUE DoS: Latencia extrema.", mensajeOriginal, _random.Next(800, 2500), null);

                case TipoAtaque.ARP_Spoofing:
                    string ipFalsa = $"192.168.1.{_random.Next(2, 254)}";
                    return (true, $"⚠️ ARP Spoofing: Tráfico redirigido a {ipFalsa}",
                            mensajeOriginal + "\n[DESVIADO]", 80, ipFalsa);

                case TipoAtaque.Phishing:
                    return (true, "🎣 PHISHING: Mensaje sospechoso.",
                            $"[ALERTA] {mensajeOriginal}\n\n--- No haga clic en enlaces ---", 0, null);

                default:
                    return (true, "", mensajeOriginal, 0, null);
            }
        }

        private string ModificarMensajeMITM(string original)
        {
            if (string.IsNullOrEmpty(original)) return original;

            int tipo = _random.Next(1, 7);

            switch (tipo)
            {
                case 1:
                    return original + " " + ObtenerFraseFalsa();
                case 2:
                    return TransformarMayusculas(original);
                case 3:
                    var palabras = original.Split(' ');
                    Array.Reverse(palabras);
                    return string.Join(" ", palabras);
                case 4:
                    int mitad = original.Length / 2;
                    return original.Insert(mitad, " [INTERCEPTADO] ");
                case 5:
                    return ReemplazarVocales(original);
                case 6:
                    return "¡URGENTE! " + TransformarMayusculas(original) + " [Verifique aquí: http://falso-link.com]";
                default:
                    return original + " [MODIFICADO]";
            }
        }

        private string ObtenerFraseFalsa()
        {
            var frases = new[]
            {
                "- Verifique su cuenta: http://falso-enlace.com",
                "- Adjunto factura: http://malicioso.com/factura.pdf",
                "- Su sesión ha expirado: http://suplantacion.com/login",
                "- Oferta exclusiva: http://spam.com/oferta",
                "- Confirme sus datos: http://phishing-bank.com"
            };
            return frases[_random.Next(frases.Length)];
        }

        private string TransformarMayusculas(string texto)
        {
            char[] chars = texto.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (char.IsLetter(chars[i]))
                {
                    chars[i] = _random.Next(2) == 0 ? char.ToLower(chars[i]) : char.ToUpper(chars[i]);
                }
            }
            return new string(chars);
        }

        private string ReemplazarVocales(string texto)
        {
            return texto
                .Replace('a', '4')
                .Replace('e', '3')
                .Replace('i', '1')
                .Replace('o', '0')
                .Replace("u", "|_|")
                .Replace('A', '4')
                .Replace('E', '3')
                .Replace('I', '1')
                .Replace('O', '0')
                .Replace("U", "|_|");
        }
    }
}