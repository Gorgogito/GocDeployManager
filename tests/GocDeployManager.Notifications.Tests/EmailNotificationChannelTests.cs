using System.Linq;
using System.Threading.Tasks;
using GocDeployManager.Domain.Entities;
using GocDeployManager.Notifications.Canales;
using NUnit.Framework;

namespace GocDeployManager.Notifications.Tests
{
    [TestFixture]
    public class EmailNotificationChannelTests
    {
        [Test]
        public async Task EnviarAsync_ContraUnRelaySmtpReal_EntregaAsuntoYCuerpo()
        {
            using (var servidor = new ServidorSmtpDePrueba())
            {
                var configuracion = new ConfiguracionCanalEmailRepositoryEnMemoria(
                    new ConfiguracionCanalEmail("127.0.0.1", servidor.Puerto, "gocdeploy@sura.pe", "GOC Deploy Manager"));
                var canal = new EmailNotificationChannel(configuracion);

                var notificacion = NotificacionPendiente.Crear(
                    despliegueId: 1, canal: "Email", destinatarios: "ana@empresa.com, luis@empresa.com",
                    asunto: "Despliegue exitoso GOC-00001", contenido: "<p>Todo salio bien</p>");

                var resultado = await canal.EnviarAsync(notificacion);

                Assert.That(resultado.IsSuccess, Is.True, resultado.Error);
                Assert.That(servidor.UltimoMensajeRecibido, Does.Contain("Despliegue exitoso"));

                // SmtpClient codifica el cuerpo HTML en base64 (Content-Transfer-Encoding: base64) —
                // se decodifica el bloque final del mensaje para comprobar el contenido real enviado.
                var lineaBase64 = servidor.UltimoMensajeRecibido
                    .Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None)
                    .Last(l => !string.IsNullOrWhiteSpace(l));
                var cuerpoDecodificado = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(lineaBase64));

                Assert.That(cuerpoDecodificado, Does.Contain("Todo salio bien"));
            }
        }

        [Test]
        public async Task EnviarAsync_SinConfiguracion_DevuelveFalloExplicativo()
        {
            var canal = new EmailNotificationChannel(new ConfiguracionCanalEmailRepositoryEnMemoria(null));
            var notificacion = NotificacionPendiente.Crear(1, "Email", "ana@empresa.com", "Asunto", "Cuerpo");

            var resultado = await canal.EnviarAsync(notificacion);

            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Does.Contain("CanalEmail.json"));
        }

        [Test]
        public async Task EnviarAsync_SinDestinatariosValidos_DevuelveFallo()
        {
            var configuracion = new ConfiguracionCanalEmailRepositoryEnMemoria(
                new ConfiguracionCanalEmail("127.0.0.1", 25, "gocdeploy@sura.pe", "GOC Deploy Manager"));
            var canal = new EmailNotificationChannel(configuracion);
            var notificacion = NotificacionPendiente.Crear(1, "Email", " , ; ", "Asunto", "Cuerpo");

            var resultado = await canal.EnviarAsync(notificacion);

            Assert.That(resultado.IsFailure, Is.True);
        }
    }
}
