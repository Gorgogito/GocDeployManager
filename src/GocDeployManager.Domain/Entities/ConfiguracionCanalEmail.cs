using GocDeployManager.Common;

namespace GocDeployManager.Domain.Entities
{
    /// <summary>
    /// Configuración del canal de correo — relay SMTP interno, sin
    /// credenciales ni TLS (confirmado con el relay real que ya usa SIT,
    /// análisis de notificaciones sección 8).
    /// </summary>
    public sealed class ConfiguracionCanalEmail
    {
        public string Host { get; }
        public int Puerto { get; }
        public string Remitente { get; }
        public string NombreRemitente { get; }

        public ConfiguracionCanalEmail(string host, int puerto, string remitente, string nombreRemitente)
        {
            Host = Guard.ContraNuloOVacio(host, nameof(host));
            Puerto = Guard.Positivo(puerto, nameof(puerto));
            Remitente = Guard.ContraNuloOVacio(remitente, nameof(remitente));
            NombreRemitente = Guard.ContraNuloOVacio(nombreRemitente, nameof(nombreRemitente));
        }
    }
}
