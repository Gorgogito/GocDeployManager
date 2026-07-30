using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GocDeployManager.Common;
using GocDeployManager.Domain.Entities;
using GocDeployManager.Notifications.Abstractions;

namespace GocDeployManager.Notifications.Canales
{
    /// <summary>
    /// Microsoft Teams vía Incoming Webhook (Workflows) — sin Azure AD, sin
    /// aprobación app-only (análisis de notificaciones, sección 9/10). El
    /// destino ya viene resuelto en <see cref="NotificacionPendiente.Destinatarios"/>
    /// (la URL de webhook), y el contenido ya es el JSON completo a enviar.
    /// </summary>
    public sealed class TeamsWebhookNotificationChannel : ICanalNotificacion
    {
        private readonly HttpClient _httpClient;

        public string Nombre => NombresDeCanal.Teams;

        public TeamsWebhookNotificationChannel(HttpClient httpClient)
        {
            _httpClient = Guard.ContraNulo(httpClient, nameof(httpClient));
        }

        public async Task<Result> EnviarAsync(NotificacionPendiente notificacion)
        {
            Guard.ContraNulo(notificacion, nameof(notificacion));

            var urlWebhook = notificacion.Destinatarios;
            if (string.IsNullOrWhiteSpace(urlWebhook))
                return Result.Fail("La notificación no tiene una URL de webhook de Teams configurada.");

            try
            {
                using (var contenido = new StringContent(notificacion.Contenido, Encoding.UTF8, "application/json"))
                using (var respuesta = await _httpClient.PostAsync(urlWebhook, contenido).ConfigureAwait(false))
                {
                    if (!respuesta.IsSuccessStatusCode)
                    {
                        var cuerpo = await respuesta.Content.ReadAsStringAsync().ConfigureAwait(false);
                        return Result.Fail($"Teams respondió {(int)respuesta.StatusCode} {respuesta.ReasonPhrase}: {cuerpo}");
                    }
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error al publicar en Teams: {ex.Message}", ex);
            }
        }
    }
}
