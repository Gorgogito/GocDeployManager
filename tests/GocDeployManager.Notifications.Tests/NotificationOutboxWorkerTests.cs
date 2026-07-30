using System.Linq;
using GocDeployManager.Domain.Entities;
using NUnit.Framework;

namespace GocDeployManager.Notifications.Tests
{
    [TestFixture]
    public class NotificationOutboxWorkerTests
    {
        [Test]
        public async System.Threading.Tasks.Task DrenarAsync_SiElCanalTieneExito_MarcaLaNotificacionComoEnviada()
        {
            var outbox = new NotificationOutboxRepositoryEnMemoria();
            var id = outbox.Encolar(NotificacionPendiente.Crear(1, "Email", "ana@empresa.com", "Asunto", "Cuerpo"));
            var canal = new CanalNotificacionDePrueba("Email");
            var worker = new NotificationOutboxWorker(outbox, new[] { canal }, new AppLoggerDePrueba());

            await worker.DrenarAsync();

            Assert.That(canal.IdsEnviados, Does.Contain(id));
            Assert.That(outbox.Todas.Single(n => n.Id == id).Estado, Is.EqualTo(EstadoNotificacion.Enviado));
        }

        [Test]
        public async System.Threading.Tasks.Task DrenarAsync_SiElCanalFalla_QuedaReintentandoConBackoff()
        {
            var outbox = new NotificationOutboxRepositoryEnMemoria();
            var id = outbox.Encolar(NotificacionPendiente.Crear(1, "Email", "ana@empresa.com", "Asunto", "Cuerpo"));
            var canal = new CanalNotificacionDePrueba("Email") { DebeFallar = true };
            var worker = new NotificationOutboxWorker(outbox, new[] { canal }, new AppLoggerDePrueba());

            await worker.DrenarAsync();

            var fila = outbox.Todas.Single(n => n.Id == id);
            Assert.That(fila.Estado, Is.EqualTo(EstadoNotificacion.Reintentando));
            Assert.That(fila.IntentosRealizados, Is.EqualTo(1));
            Assert.That(fila.ProximoIntentoEn, Is.Not.Null);
        }

        [Test]
        public async System.Threading.Tasks.Task DrenarAsync_UnaNotificacionEnBackoff_NoSeReintentaAntesDeTiempo()
        {
            var outbox = new NotificationOutboxRepositoryEnMemoria();
            var id = outbox.Encolar(NotificacionPendiente.Crear(1, "Email", "ana@empresa.com", "Asunto", "Cuerpo"));
            var canal = new CanalNotificacionDePrueba("Email") { DebeFallar = true };
            var worker = new NotificationOutboxWorker(outbox, new[] { canal }, new AppLoggerDePrueba());

            await worker.DrenarAsync(); // primer intento -> Reintentando con backoff de varios minutos
            canal.DebeFallar = false;
            await worker.DrenarAsync(); // segundo drenaje inmediato: no debería reintentarlo todavía

            Assert.That(canal.IdsEnviados, Does.Not.Contain(id));
        }

        [Test]
        public async System.Threading.Tasks.Task DrenarAsync_SiNoHayCanalActivoConEseNombre_NoLanzaYLaDejaPendiente()
        {
            var outbox = new NotificationOutboxRepositoryEnMemoria();
            var id = outbox.Encolar(NotificacionPendiente.Crear(1, "Slack", "canal-x", "Asunto", "Cuerpo"));
            var worker = new NotificationOutboxWorker(outbox, new CanalNotificacionDePrueba[0], new AppLoggerDePrueba());

            Assert.DoesNotThrowAsync(async () => await worker.DrenarAsync());

            Assert.That(outbox.Todas.Single(n => n.Id == id).Estado, Is.EqualTo(EstadoNotificacion.Pendiente));
        }
    }
}
