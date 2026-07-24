using System.IO;
using GocDeployManager.Domain.Entities;
using GocDeployManager.Infrastructure.Json;
using NUnit.Framework;

namespace GocDeployManager.Infrastructure.Tests
{
    [TestFixture]
    public class JsonSistemaRepositoryTests
    {
        private const string JsonDeEjemplo = @"
[
  {
    ""codigoSistema"": ""SIT"",
    ""nombreSistema"": ""SIT"",
    ""repositorioUrl"": ""https://bitbucket.org/org/sit-integra-web.git"",
    ""carpetaPrecompilada"": ""SITSolution\\PrecompiledWeb\\IN-SIT"",
    ""pasos"": [
      { ""orden"": 1, ""carpetaProyecto"": ""Sit.BusinessEntities"" },
      { ""orden"": 2, ""carpetaProyecto"": ""Sit.DataAccessLayer"" },
      { ""orden"": 3, ""carpetaProyecto"": ""Sit.BusinessLayer"" },
      { ""orden"": 4, ""carpetaProyecto"": ""SITSolution"" }
    ]
  },
  {
    ""codigoSistema"": ""IDI"",
    ""nombreSistema"": ""IDI"",
    ""repositorioUrl"": ""https://bitbucket.org/org/idi-web.git"",
    ""carpetaPrecompilada"": ""PrecompiledWeb\\IN-IDI"",
    ""pasos"": [
      { ""orden"": 1, ""carpetaProyecto"": ""IDI.BussinessEntities"" },
      { ""orden"": 2, ""carpetaProyecto"": ""IDI.BussinessLogic"" },
      { ""orden"": 3, ""carpetaProyecto"": ""IDI.DataAccess"" },
      { ""orden"": 4, ""carpetaProyecto"": ""IDI.Facade"" },
      { ""orden"": 5, ""carpetaProyecto"": ""IDI.Log"" },
      { ""orden"": 6, ""carpetaProyecto"": ""IDI.Utilitario"" },
      { ""orden"": 7, ""carpetaProyecto"": ""IDI.sln"" }
    ]
  }
]";

        [Test]
        public void ObtenerConfiguracion_DevuelveLaSecuenciaCorrectaPorSistema()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var rutaArchivo = Path.Combine(temp.Ruta, "Sistemas.json");
                File.WriteAllText(rutaArchivo, JsonDeEjemplo);

                var repositorio = new JsonSistemaRepository(rutaArchivo);
                var idi = new Sistema("IDI", "IDI");

                var resultado = repositorio.ObtenerConfiguracion(idi);

                Assert.That(resultado.IsSuccess, Is.True);
                Assert.That(resultado.Value.SecuenciaDeBuild.Pasos, Has.Count.EqualTo(7));
                Assert.That(resultado.Value.CarpetaPrecompilada, Is.EqualTo(@"PrecompiledWeb\IN-IDI"));
            }
        }

        [Test]
        public void ObtenerConfiguracion_SiElSistemaNoEstaConfigurado_DevuelveFalloNoExcepcion()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var rutaArchivo = Path.Combine(temp.Ruta, "Sistemas.json");
                File.WriteAllText(rutaArchivo, JsonDeEjemplo);

                var repositorio = new JsonSistemaRepository(rutaArchivo);
                var propag = new Sistema("PROPAG", "ProPag");

                var resultado = repositorio.ObtenerConfiguracion(propag);

                Assert.That(resultado.IsFailure, Is.True);
                Assert.That(resultado.Error, Does.Contain("PROPAG"));
            }
        }

        [Test]
        public void ObtenerSistemasConocidos_ListaTodosLosCodigosDelArchivo()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var rutaArchivo = Path.Combine(temp.Ruta, "Sistemas.json");
                File.WriteAllText(rutaArchivo, JsonDeEjemplo);

                var repositorio = new JsonSistemaRepository(rutaArchivo);

                var sistemas = repositorio.ObtenerSistemasConocidos();

                Assert.That(sistemas, Has.Count.EqualTo(2));
            }
        }
    }
}
