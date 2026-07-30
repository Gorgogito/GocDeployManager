using System.IO;
using GocDeployManager.Domain.Entities;
using GocDeployManager.Infrastructure.Json;
using NUnit.Framework;

namespace GocDeployManager.Infrastructure.Tests
{
    [TestFixture]
    public class JsonConfiguracionCanalEmailRepositoryTests
    {
        [Test]
        public void GuardarYObtener_HaceRoundTripCompleto()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var repositorio = new JsonConfiguracionCanalEmailRepository(Path.Combine(temp.Ruta, "CanalEmail.json"));
                var configuracion = new ConfiguracionCanalEmail("smtp.sura.pe", 25, "gocdeploy@sura.pe", "GOC Deploy Manager");

                repositorio.Guardar(configuracion);
                var recuperada = repositorio.Obtener();

                Assert.That(recuperada.Host, Is.EqualTo("smtp.sura.pe"));
                Assert.That(recuperada.Puerto, Is.EqualTo(25));
                Assert.That(recuperada.Remitente, Is.EqualTo("gocdeploy@sura.pe"));
            }
        }

        [Test]
        public void Obtener_SiElArchivoNoExiste_DevuelveNulo()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var repositorio = new JsonConfiguracionCanalEmailRepository(Path.Combine(temp.Ruta, "NoExiste.json"));

                Assert.That(repositorio.Obtener(), Is.Null);
            }
        }
    }
}
