using GocDeployManager.Domain.Entities;
using NUnit.Framework;

namespace GocDeployManager.Domain.Tests
{
    [TestFixture]
    public class NotificacionPendienteTests
    {
        [Test]
        public void Crear_EmpiezaPendienteYSinIntentos()
        {
            var notificacion = NotificacionPendiente.Crear(
                despliegueId: 10, canal: "Email", destinatarios: "ana@empresa.com", asunto: "Despliegue exitoso", contenido: "<p>Ok</p>");

            Assert.That(notificacion.Estado, Is.EqualTo(EstadoNotificacion.Pendiente));
            Assert.That(notificacion.IntentosRealizados, Is.EqualTo(0));
            Assert.That(notificacion.ProximoIntentoEn, Is.Null);
            Assert.That(notificacion.MensajeError, Is.Null);
        }

        [Test]
        public void Crear_AceptaDespliegueIdNulo_ParaLaNotificacionDeInicio()
        {
            var notificacion = NotificacionPendiente.Crear(
                despliegueId: null, canal: "Email", destinatarios: "ana@empresa.com", asunto: "Despliegue iniciado", contenido: "<p>Inicio</p>");

            Assert.That(notificacion.DespliegueId, Is.Null);
        }

        [Test]
        public void Reconstruir_PreservaTodosLosCampos()
        {
            var ahora = System.DateTime.Now;
            var notificacion = NotificacionPendiente.Reconstruir(
                id: 3, despliegueId: 10, canal: "Teams", destinatarios: "Sura Peru Teams", asunto: null, contenido: "{}",
                estado: EstadoNotificacion.Reintentando, intentosRealizados: 2, proximoIntentoEn: ahora, mensajeError: "timeout");

            Assert.That(notificacion.Id, Is.EqualTo(3));
            Assert.That(notificacion.Estado, Is.EqualTo(EstadoNotificacion.Reintentando));
            Assert.That(notificacion.IntentosRealizados, Is.EqualTo(2));
            Assert.That(notificacion.ProximoIntentoEn, Is.EqualTo(ahora));
            Assert.That(notificacion.MensajeError, Is.EqualTo("timeout"));
        }
    }
}
