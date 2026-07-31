using System.Linq;
using GocDeployManager.Domain.Entities;
using GocDeployManager.Infrastructure.SqlServer;
using NUnit.Framework;

namespace GocDeployManager.Infrastructure.Tests
{
    [TestFixture]
    public class SqlServerAmbienteRepositoryTests
    {
        [Test]
        public void GuardarYObtenerTodos_HaceRoundTripCompleto()
        {
            using (var baseDeDatos = new BaseDeDatosSqlServerDePrueba())
            {
                var repositorio = new SqlServerAmbienteRepository(baseDeDatos.CadenaConexion);

                var sit = new Sistema("SIT", "SIT");
                var idi = new Sistema("IDI", "IDI");
                var ambiente = new Ambiente("Desarrollo", new[]
                {
                    new AmbienteSistema(sit, @"\\Sdpeapp00009\Aplicaciones\SIT"),
                    new AmbienteSistema(idi, @"\\Sdpeapp00009\Aplicaciones\IDI"),
                });

                repositorio.Guardar(new[] { ambiente });

                var recuperados = repositorio.ObtenerTodos();

                Assert.That(recuperados, Has.Count.EqualTo(1));
                Assert.That(recuperados[0].Nombre, Is.EqualTo("Desarrollo"));
                Assert.That(recuperados[0].Sistemas, Has.Count.EqualTo(2));
                Assert.That(recuperados[0].BuscarSistema(sit).RutaDestino, Is.EqualTo(@"\\Sdpeapp00009\Aplicaciones\SIT"));
            }
        }

        [Test]
        public void GuardarYObtenerTodos_PreservaNotificacionesHabilitadas()
        {
            using (var baseDeDatos = new BaseDeDatosSqlServerDePrueba())
            {
                var repositorio = new SqlServerAmbienteRepository(baseDeDatos.CadenaConexion);
                var sit = new Sistema("SIT", "SIT");

                var desarrollo = new Ambiente("Desarrollo", new[] { new AmbienteSistema(sit, @"C:\Desarrollo") }, notificacionesHabilitadas: false);
                var testing = new Ambiente("Testing", new[] { new AmbienteSistema(sit, @"C:\Testing") });

                repositorio.Guardar(new[] { desarrollo, testing });
                var recuperados = repositorio.ObtenerTodos();

                Assert.That(recuperados.Single(a => a.Nombre == "Desarrollo").NotificacionesHabilitadas, Is.False);
                Assert.That(recuperados.Single(a => a.Nombre == "Testing").NotificacionesHabilitadas, Is.True);
            }
        }

        [Test]
        public void ObtenerTodos_SiNoHayFilas_DevuelveListaVacia()
        {
            using (var baseDeDatos = new BaseDeDatosSqlServerDePrueba())
            {
                var repositorio = new SqlServerAmbienteRepository(baseDeDatos.CadenaConexion);

                Assert.That(repositorio.ObtenerTodos(), Is.Empty);
            }
        }

        [Test]
        public void Guardar_ReemplazaLaListaCompletaEnVezDeAcumular()
        {
            using (var baseDeDatos = new BaseDeDatosSqlServerDePrueba())
            {
                var repositorio = new SqlServerAmbienteRepository(baseDeDatos.CadenaConexion);
                var sit = new Sistema("SIT", "SIT");

                repositorio.Guardar(new[] { new Ambiente("Uno", new[] { new AmbienteSistema(sit, @"C:\Uno") }) });
                repositorio.Guardar(new[] { new Ambiente("Dos", new[] { new AmbienteSistema(sit, @"C:\Dos") }) });

                var recuperados = repositorio.ObtenerTodos();

                Assert.That(recuperados, Has.Count.EqualTo(1));
                Assert.That(recuperados[0].Nombre, Is.EqualTo("Dos"));
            }
        }
    }
}
