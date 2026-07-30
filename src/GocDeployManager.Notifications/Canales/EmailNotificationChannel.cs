using System;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using GocDeployManager.Common;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Domain.Entities;
using GocDeployManager.Notifications.Abstractions;

namespace GocDeployManager.Notifications.Canales
{
    /// <summary>
    /// Correo vía <see cref="SmtpClient"/> nativo contra el relay SMTP
    /// interno — confirmado sin autenticación ni TLS, igual que el que ya
    /// usa SIT (<c>UIUtility.vb:EnviarMail</c>; análisis de notificaciones,
    /// sección 8).
    /// </summary>
    public sealed class EmailNotificationChannel : ICanalNotificacion
    {
        private readonly IConfiguracionCanalEmailRepository _configuracion;

        public string Nombre => NombresDeCanal.Email;

        public EmailNotificationChannel(IConfiguracionCanalEmailRepository configuracion)
        {
            _configuracion = Guard.ContraNulo(configuracion, nameof(configuracion));
        }

        public async Task<Result> EnviarAsync(NotificacionPendiente notificacion)
        {
            Guard.ContraNulo(notificacion, nameof(notificacion));

            var configuracion = _configuracion.Obtener();
            if (configuracion == null)
                return Result.Fail("No hay configuración del canal de correo (CanalEmail.json) — configúrala en Configuración > Canales de notificación.");

            try
            {
                using (var mensaje = new MailMessage())
                {
                    mensaje.From = new MailAddress(configuracion.Remitente, configuracion.NombreRemitente, Encoding.UTF8);

                    foreach (var destino in notificacion.Destinatarios.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                        mensaje.To.Add(destino.Trim());

                    if (mensaje.To.Count == 0)
                        return Result.Fail("La notificación no tiene destinatarios de correo válidos.");

                    mensaje.Subject = notificacion.Asunto ?? "GocDeployManager";
                    mensaje.SubjectEncoding = Encoding.UTF8;
                    mensaje.Body = notificacion.Contenido;
                    mensaje.BodyEncoding = Encoding.UTF8;
                    mensaje.IsBodyHtml = true;

                    using (var smtp = new SmtpClient(configuracion.Host, configuracion.Puerto))
                    {
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        await smtp.SendMailAsync(mensaje).ConfigureAwait(false);
                    }
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error al enviar correo: {ex.Message}", ex);
            }
        }
    }
}
