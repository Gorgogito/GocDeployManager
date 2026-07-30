using System.Net.Http;
using System.Threading.Tasks;
using GocDeployManager.Domain.Entities;
using GocDeployManager.Notifications.Canales;
using NUnit.Framework;

namespace GocDeployManager.Notifications.Tests
{
    [TestFixture]
    public class TeamsWebhookNotificationChannelTests
    {
        [Test]
        public async Task EnviarAsync_ContraUnWebhookReal_PublicaElContenidoTalCual()
        {
            using (var servidor = new ServidorHttpDePrueba())
            using (var httpClient = new HttpClient())
            {
                var canal = new TeamsWebhookNotificationChannel(httpClient);
                var notificacion = NotificacionPendiente.Crear(
                    despliegueId: 1, canal: "Teams", destinatarios: $"http://127.0.0.1:{servidor.Puerto}/",
                    asunto: null, contenido: "{\"type\":\"message\",\"attachments\":[]}");

                var resultado = await canal.EnviarAsync(notificacion);

                Assert.That(resultado.IsSuccess, Is.True, resultado.Error);
                Assert.That(servidor.UltimoCuerpoRecibido, Is.EqualTo("{\"type\":\"message\",\"attachments\":[]}"));
            }
        }

        [Test]
        public async Task EnviarAsync_SiElWebhookRespondeError_DevuelveFalloConElCodigo()
        {
            using (var servidor = new ServidorHttpDePrueba { CodigoRespuesta = 400 })
            using (var httpClient = new HttpClient())
            {
                var canal = new TeamsWebhookNotificationChannel(httpClient);
                var notificacion = NotificacionPendiente.Crear(
                    1, "Teams", $"http://127.0.0.1:{servidor.Puerto}/", null, "{}");

                var resultado = await canal.EnviarAsync(notificacion);

                Assert.That(resultado.IsFailure, Is.True);
                Assert.That(resultado.Error, Does.Contain("400"));
            }
        }

        [Test]
        public async Task EnviarAsync_SinUrlDeWebhook_DevuelveFalloExplicativo()
        {
            using (var httpClient = new HttpClient())
            {
                var canal = new TeamsWebhookNotificationChannel(httpClient);
                var notificacion = NotificacionPendiente.Crear(1, "Teams", "sin-url-valida", null, "{}");

                // "sin-url-valida" no es una URL válida -> HttpClient lanzará, el canal debe capturarlo como Result.Fail.
                var resultado = await canal.EnviarAsync(notificacion);

                Assert.That(resultado.IsFailure, Is.True);
            }
        }
    }
}
