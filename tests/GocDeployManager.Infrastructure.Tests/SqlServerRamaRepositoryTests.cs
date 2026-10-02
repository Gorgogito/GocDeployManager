using GocDeployManager.Infrastructure.SqlServer;
using NUnit.Framework;

namespace GocDeployManager.Infrastructure.Tests
{
    [TestFixture]
    public class SqlServerRamaRepositoryTests
    {
        [Test]
        public void ObtenerTodas_ConLaTablaRecienCreada_DevuelveMasterYDevelop()
        {
            using (var baseDeDatos = new BaseDeDatosSqlServerDePrueba())
            {
                var repositorio = new SqlServerRamaRepository(baseDeDatos.CadenaConexion);

                Assert.That(repositorio.ObtenerTodas(), Is.EqualTo(new[] { "master", "develop" }));
            }
        }

        [Test]
        public void Guardar_ReemplazaLaListaYConservaElOrden()
        {
            using (var baseDeDatos = new BaseDeDatosSqlServerDePrueba())
            {
                var repositorio = new SqlServerRamaRepository(baseDeDatos.CadenaConexion);

                repositorio.Guardar(new[] { "release/1.0", "master" });

                Assert.That(repositorio.ObtenerTodas(), Is.EqualTo(new[] { "release/1.0", "master" }));
            }
        }

        [Test]
        public void Guardar_ListaVacia_DejaLaTablaVacia()
        {
            using (var baseDeDatos = new BaseDeDatosSqlServerDePrueba())
            {
                var repositorio = new SqlServerRamaRepository(baseDeDatos.CadenaConexion);

                repositorio.Guardar(new string[0]);

                Assert.That(repositorio.ObtenerTodas(), Is.Empty);
            }
        }
    }
}
