using GocDeployManager.Domain.Entities;
using NUnit.Framework;

namespace GocDeployManager.Domain.Tests
{
    [TestFixture]
    public class SecuenciaDeBuildTests
    {
        [Test]
        public void Constructor_OrdenaLosPasosPorOrden()
        {
            var sistema = new Sistema("IDI", "IDI");
            var secuencia = new SecuenciaDeBuild(sistema, new[]
            {
                new PasoDeBuild(3, "IDI.DataAccess"),
                new PasoDeBuild(1, "IDI.BussinessEntities"),
                new PasoDeBuild(2, "IDI.BussinessLogic"),
            });

            Assert.That(secuencia.Pasos[0].CarpetaProyecto, Is.EqualTo("IDI.BussinessEntities"));
            Assert.That(secuencia.Pasos[1].CarpetaProyecto, Is.EqualTo("IDI.BussinessLogic"));
            Assert.That(secuencia.Pasos[2].CarpetaProyecto, Is.EqualTo("IDI.DataAccess"));
        }

        [Test]
        public void PasoDeBuild_PermiteParametrosMsBuildOpcionales()
        {
            // ProPag confirmó que necesita parámetros explícitos, a diferencia de SIT/IDI.
            var paso = new PasoDeBuild(1, "Sit.BusinessEntities", "/p:Configuration=Debug;TargetFrameworkVersion=v4.0");

            Assert.That(paso.ParametrosMsBuild, Is.EqualTo("/p:Configuration=Debug;TargetFrameworkVersion=v4.0"));
        }

        [Test]
        public void PasoDeBuild_SinParametrosQuedaEnNulo()
        {
            var paso = new PasoDeBuild(1, "Sit.BusinessEntities");

            Assert.That(paso.ParametrosMsBuild, Is.Null);
        }
    }
}
