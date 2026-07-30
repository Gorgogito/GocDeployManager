using System;
using System.Linq;
using GocDeployManager.Domain.Entities;
using GocDeployManager.Infrastructure.SqlServer;
using NUnit.Framework;

namespace GocDeployManager.Infrastructure.Tests
{
    [TestFixture]
    public class SqlServerNotificationOutboxRepositoryTests
    {
        private static int RegistrarDespliegueDePrueba(SqlServerDeployHistoryRepository historial) =>
            historial.Registrar(Despliegue.RegistrarExitoso(
                "jtorres", "jtorres.win", "LAPTOP-01", "GOC-00001", "feature/GOC-00001",
                "Testing", new[] { "SIT" }, TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(10)));

        [Test]
        public void Encolar_QuedaComoPendienteConCeroIntentos()
        {
            using (var baseDeDatos = new BaseDeDatosSqlServerDePrueba())
            {
                var historial = new SqlServerDeployHistoryRepository(baseDeDatos.CadenaConexion);
                var outbox = new SqlServerNotificationOutboxRepository(baseDeDatos.CadenaConexion);
                var despliegueId = RegistrarDespliegueDePrueba(historial);

                var notificacion = NotificacionPendiente.Crear(despliegueId, "Email", "ana@empresa.com", "Despliegue exitoso", "<p>Ok</p>");
                var id = outbox.Encolar(notificacion);

                var pendientes = outbox.ObtenerPendientes(DateTime.Now);

                Assert.That(id, Is.GreaterThan(0));
                Assert.That(pendientes, Has.Count.EqualTo(1));
                Assert.That(pendientes[0].Estado, Is.EqualTo(EstadoNotificacion.Pendiente));
                Assert.That(pendientes[0].IntentosRealizados, Is.EqualTo(0));
                Assert.That(pendientes[0].DespliegueId, Is.EqualTo(despliegueId));
            }
        }

        [Test]
        public void Encolar_AceptaDespliegueIdNulo_ParaLaNotificacionDeInicio()
        {
            using (var baseDeDatos = new BaseDeDatosSqlServerDePrueba())
            {
                var outbox = new SqlServerNotificationOutboxRepository(baseDeDatos.CadenaConexion);

                var id = outbox.Encolar(NotificacionPendiente.Crear(null, "Email", "ana@empresa.com", "Despliegue iniciado", "<p>Inicio</p>"));
                var pendientes = outbox.ObtenerPendientes(DateTime.Now);

                Assert.That(id, Is.GreaterThan(0));
                Assert.That(pendientes.Single(p => p.Id == id).DespliegueId, Is.Null);
            }
        }

        [Test]
        public void MarcarEnviada_LaSacaDeLosPendientes()
        {
            using (var baseDeDatos = new BaseDeDatosSqlServerDePrueba())
            {
                var historial = new SqlServerDeployHistoryRepository(baseDeDatos.CadenaConexion);
                var outbox = new SqlServerNotificationOutboxRepository(baseDeDatos.CadenaConexion);
                var despliegueId = RegistrarDespliegueDePrueba(historial);

                var id = outbox.Encolar(NotificacionPendiente.Crear(despliegueId, "Teams", "Sura Peru Teams", null, "{}"));
                outbox.MarcarEnviada(id);

                Assert.That(outbox.ObtenerPendientes(DateTime.Now), Is.Empty);
            }
        }

        [Test]
        public void MarcarFallidaConReintento_VuelveAAparecerSoloDespuesDelProximoIntento()
        {
            using (var baseDeDatos = new BaseDeDatosSqlServerDePrueba())
            {
                var historial = new SqlServerDeployHistoryRepository(baseDeDatos.CadenaConexion);
                var outbox = new SqlServerNotificationOutboxRepository(baseDeDatos.CadenaConexion);
                var despliegueId = RegistrarDespliegueDePrueba(historial);

                var id = outbox.Encolar(NotificacionPendiente.Crear(despliegueId, "Email", "ana@empresa.com", "Asunto", "Cuerpo"));
                var proximoIntento = DateTime.Now.AddMinutes(5);
                outbox.MarcarFallidaConReintento(id, "timeout de red", proximoIntento);

                Assert.That(outbox.ObtenerPendientes(DateTime.Now), Is.Empty);

                var pendientesMasTarde = outbox.ObtenerPendientes(proximoIntento.AddSeconds(1));
                Assert.That(pendientesMasTarde, Has.Count.EqualTo(1));
                Assert.That(pendientesMasTarde[0].Estado, Is.EqualTo(EstadoNotificacion.Reintentando));
                Assert.That(pendientesMasTarde[0].IntentosRealizados, Is.EqualTo(1));
                Assert.That(pendientesMasTarde[0].MensajeError, Is.EqualTo("timeout de red"));
            }
        }

        [Test]
        public void MarcarFallidaDefinitiva_LaSacaDeLosPendientes()
        {
            using (var baseDeDatos = new BaseDeDatosSqlServerDePrueba())
            {
                var historial = new SqlServerDeployHistoryRepository(baseDeDatos.CadenaConexion);
                var outbox = new SqlServerNotificationOutboxRepository(baseDeDatos.CadenaConexion);
                var despliegueId = RegistrarDespliegueDePrueba(historial);

                var id = outbox.Encolar(NotificacionPendiente.Crear(despliegueId, "Email", "ana@empresa.com", "Asunto", "Cuerpo"));
                outbox.MarcarFallidaDefinitiva(id, "servidor SMTP inalcanzable tras varios intentos");

                Assert.That(outbox.ObtenerPendientes(DateTime.Now), Is.Empty);
            }
        }
    }
}
