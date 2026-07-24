using System.IO;
using System.Linq;
using GocDeployManager.Domain.Entities;
using GocDeployManager.Infrastructure.Json;
using NUnit.Framework;

namespace GocDeployManager.Infrastructure.Tests
{
    [TestFixture]
    public class JsonAmbienteRepositoryTests
    {
        [Test]
        public void GuardarYObtenerTodos_HaceRoundTripCompleto()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var rutaArchivo = Path.Combine(temp.Ruta, "Ambientes.json");
                var repositorio = new JsonAmbienteRepository(rutaArchivo);

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
        public void ObtenerTodos_SiElArchivoNoExiste_DevuelveListaVacia()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var repositorio = new JsonAmbienteRepository(Path.Combine(temp.Ruta, "NoExiste.json"));

                Assert.That(repositorio.ObtenerTodos(), Is.Empty);
            }
        }

        [Test]
        public void Guardar_DejaUnRespaldoDelArchivoAnterior()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var rutaArchivo = Path.Combine(temp.Ruta, "Ambientes.json");
                var repositorio = new JsonAmbienteRepository(rutaArchivo);
                var sit = new Sistema("SIT", "SIT");

                repositorio.Guardar(new[] { new Ambiente("Uno", new[] { new AmbienteSistema(sit, @"C:\Uno") }) });
                repositorio.Guardar(new[] { new Ambiente("Dos", new[] { new AmbienteSistema(sit, @"C:\Dos") }) });

                var hayRespaldo = Directory.GetFiles(temp.Ruta, "Ambientes.json.bak-*").Any();
                Assert.That(hayRespaldo, Is.True);
            }
        }
    }
}
