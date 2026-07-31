using GocDeployManager.Domain.Entities;
using GocDeployManager.Infrastructure.SqlServer;
using NUnit.Framework;

namespace GocDeployManager.Infrastructure.Tests
{
    [TestFixture]
    public class SqlServerSistemaRepositoryTests
    {
        private static ConfiguracionSistema CrearConfiguracionSit()
        {
            var sit = new Sistema("SIT", "SIT");
            var secuencia = new SecuenciaDeBuild(sit, new[]
            {
                new PasoDeBuild(1, "Sit.BusinessEntities"),
                new PasoDeBuild(2, "Sit.DataAccessLayer"),
                new PasoDeBuild(3, "Sit.BusinessLayer"),
                new PasoDeBuild(4, "SITSolution"),
            });
            return new ConfiguracionSistema(sit, "https://bitbucket.org/org/sit-integra-web.git", @"SITSolution\PrecompiledWeb\IN-SIT", secuencia);
        }

        [Test]
        public void ObtenerConfiguracion_DevuelveLaSecuenciaCorrectaEnOrden()
        {
            using (var baseDeDatos = new BaseDeDatosSqlServerDePrueba())
            {
                var repositorio = new SqlServerSistemaRepository(baseDeDatos.CadenaConexion);
                repositorio.Guardar(new[] { CrearConfiguracionSit() });

                var resultado = repositorio.ObtenerConfiguracion(new Sistema("SIT", "SIT"));

                Assert.That(resultado.IsSuccess, Is.True);
                Assert.That(resultado.Value.CarpetaPrecompilada, Is.EqualTo(@"SITSolution\PrecompiledWeb\IN-SIT"));
                Assert.That(resultado.Value.SecuenciaDeBuild.Pasos, Has.Count.EqualTo(4));
                Assert.That(resultado.Value.SecuenciaDeBuild.Pasos[0].CarpetaProyecto, Is.EqualTo("Sit.BusinessEntities"));
                Assert.That(resultado.Value.SecuenciaDeBuild.Pasos[3].CarpetaProyecto, Is.EqualTo("SITSolution"));
            }
        }

        [Test]
        public void ObtenerConfiguracion_SiElSistemaNoEstaConfigurado_DevuelveFalloNoExcepcion()
        {
            using (var baseDeDatos = new BaseDeDatosSqlServerDePrueba())
            {
                var repositorio = new SqlServerSistemaRepository(baseDeDatos.CadenaConexion);
                repositorio.Guardar(new[] { CrearConfiguracionSit() });

                var resultado = repositorio.ObtenerConfiguracion(new Sistema("PROPAG", "ProPag"));

                Assert.That(resultado.IsFailure, Is.True);
                Assert.That(resultado.Error, Does.Contain("PROPAG"));
            }
        }

        [Test]
        public void ObtenerSistemasConocidos_ListaTodosLosCodigosGuardados()
        {
            using (var baseDeDatos = new BaseDeDatosSqlServerDePrueba())
            {
                var idi = new Sistema("IDI", "IDI");
                var configuracionIdi = new ConfiguracionSistema(
                    idi, "https://bitbucket.org/org/idi-web.git", @"PrecompiledWeb\IN-IDI",
                    new SecuenciaDeBuild(idi, new[] { new PasoDeBuild(1, "IDI.BussinessEntities") }));

                var repositorio = new SqlServerSistemaRepository(baseDeDatos.CadenaConexion);
                repositorio.Guardar(new[] { CrearConfiguracionSit(), configuracionIdi });

                var sistemas = repositorio.ObtenerSistemasConocidos();

                Assert.That(sistemas, Has.Count.EqualTo(2));
            }
        }

        [Test]
        public void Guardar_PreservaParametrosMsBuildOpcionales()
        {
            using (var baseDeDatos = new BaseDeDatosSqlServerDePrueba())
            {
                var propag = new Sistema("PROPAG", "ProPag");
                var secuencia = new SecuenciaDeBuild(propag, new[]
                {
                    new PasoDeBuild(1, "Sit.BusinessEntities", "/p:Configuration=Debug;TargetFrameworkVersion=v4.0"),
                });
                var configuracion = new ConfiguracionSistema(propag, "https://bitbucket.org/org/provision-pagos-web.git", "PrecompiledWeb", secuencia);

                var repositorio = new SqlServerSistemaRepository(baseDeDatos.CadenaConexion);
                repositorio.Guardar(new[] { configuracion });

                var resultado = repositorio.ObtenerConfiguracion(propag);

                Assert.That(resultado.Value.SecuenciaDeBuild.Pasos[0].ParametrosMsBuild,
                    Is.EqualTo("/p:Configuration=Debug;TargetFrameworkVersion=v4.0"));
            }
        }

        [Test]
        public void Guardar_ReemplazaLaListaCompletaEnVezDeAcumular()
        {
            using (var baseDeDatos = new BaseDeDatosSqlServerDePrueba())
            {
                var repositorio = new SqlServerSistemaRepository(baseDeDatos.CadenaConexion);

                repositorio.Guardar(new[] { CrearConfiguracionSit() });

                var idi = new Sistema("IDI", "IDI");
                var configuracionIdi = new ConfiguracionSistema(
                    idi, "https://bitbucket.org/org/idi-web.git", @"PrecompiledWeb\IN-IDI",
                    new SecuenciaDeBuild(idi, new[] { new PasoDeBuild(1, "IDI.BussinessEntities") }));
                repositorio.Guardar(new[] { configuracionIdi });

                var sistemas = repositorio.ObtenerSistemasConocidos();

                Assert.That(sistemas, Has.Count.EqualTo(1));
                Assert.That(sistemas[0].Codigo, Is.EqualTo("IDI"));
            }
        }
    }
}
