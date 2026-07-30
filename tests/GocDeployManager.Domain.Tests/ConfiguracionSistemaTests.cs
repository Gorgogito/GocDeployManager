using GocDeployManager.Domain.Entities;
using NUnit.Framework;

namespace GocDeployManager.Domain.Tests
{
    [TestFixture]
    public class ConfiguracionSistemaTests
    {
        [Test]
        public void Constructor_RecortaEspaciosYSaltosDeLineaDeLaCarpetaPrecompilada()
        {
            // Reproduce el bug real: la carpeta precompilada quedó guardada
            // con un espacio y un salto de línea al final, invisibles a
            // simple vista, que Path.Combine rechazaba recién al copiar los
            // archivos compilados.
            var sistema = new Sistema("SIT", "SIT");
            var secuencia = new SecuenciaDeBuild(sistema, new[] { new PasoDeBuild(1, "Sit.BusinessEntities") });

            var configuracion = new ConfiguracionSistema(
                sistema, "https://bitbucket.org/org/sit-integra-web.git",
                " SITSolution\\PrecompiledWeb\\IN-SIT \n", secuencia);

            Assert.That(configuracion.CarpetaPrecompilada, Is.EqualTo("SITSolution\\PrecompiledWeb\\IN-SIT"));
        }

        [Test]
        public void Constructor_RechazaCarpetaPrecompiladaQueQuedaVaciaTrasRecortar()
        {
            var sistema = new Sistema("SIT", "SIT");
            var secuencia = new SecuenciaDeBuild(sistema, new[] { new PasoDeBuild(1, "Sit.BusinessEntities") });

            Assert.Throws<System.ArgumentException>(() => new ConfiguracionSistema(
                sistema, "https://bitbucket.org/org/sit-integra-web.git", "   \n  ", secuencia));
        }
    }
}
