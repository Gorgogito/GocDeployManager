using System.IO;
using System.Linq;
using GocDeployManager.Domain.Entities;
using GocDeployManager.Infrastructure.Json;
using NUnit.Framework;

namespace GocDeployManager.Infrastructure.Tests
{
    [TestFixture]
    public class JsonConfiguracionCanalTeamsRepositoryTests
    {
        [Test]
        public void GuardarYObtenerTodos_PreservaComodinesYMapeosEspecificos()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var repositorio = new JsonConfiguracionCanalTeamsRepository(Path.Combine(temp.Ruta, "CanalTeams.json"));

                var comodin = new MapeoCanalTeams(null, null, "https://webhook/sura-peru-teams");
                var especifico = new MapeoCanalTeams("SIT", "Producción", "https://webhook/sit-prod");

                repositorio.Guardar(new[] { comodin, especifico });
                var recuperados = repositorio.ObtenerTodos();

                Assert.That(recuperados, Has.Count.EqualTo(2));
                Assert.That(recuperados.Single(m => m.Sistema == null).UrlWebhook, Is.EqualTo("https://webhook/sura-peru-teams"));
                Assert.That(recuperados.Single(m => m.Sistema == "SIT").Ambiente, Is.EqualTo("Producción"));
            }
        }

        [Test]
        public void ObtenerTodos_SiElArchivoNoExiste_DevuelveListaVacia()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var repositorio = new JsonConfiguracionCanalTeamsRepository(Path.Combine(temp.Ruta, "NoExiste.json"));

                Assert.That(repositorio.ObtenerTodos(), Is.Empty);
            }
        }
    }
}
