using System.IO;
using System.Linq;
using GocDeployManager.Domain.Entities;
using GocDeployManager.Infrastructure.Json;
using NUnit.Framework;

namespace GocDeployManager.Infrastructure.Tests
{
    [TestFixture]
    public class JsonGrupoDestinatariosRepositoryTests
    {
        [Test]
        public void GuardarYObtenerTodos_HaceRoundTripCompleto()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var rutaArchivo = Path.Combine(temp.Ruta, "GruposDestinatarios.json");
                var repositorio = new JsonGrupoDestinatariosRepository(rutaArchivo);

                var ana = Destinatario.Crear("Ana", "ana@empresa.com").Value;
                var luis = Destinatario.Crear("Luis", "luis@empresa.com").Value;
                var desarrollo = new GrupoDestinatarios("Desarrollo", new[] { ana, luis });
                var qa = new GrupoDestinatarios("QA", new[] { ana });

                repositorio.Guardar(new[] { desarrollo, qa });

                var recuperados = repositorio.ObtenerTodos();

                Assert.That(recuperados, Has.Count.EqualTo(2));
                Assert.That(recuperados.Single(g => g.Nombre == "Desarrollo").Miembros, Has.Count.EqualTo(2));
                Assert.That(recuperados.Single(g => g.Nombre == "QA").Miembros.Single().CorreoElectronico, Is.EqualTo("ana@empresa.com"));
            }
        }

        [Test]
        public void ObtenerTodos_SiElArchivoNoExiste_DevuelveListaVacia()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var repositorio = new JsonGrupoDestinatariosRepository(Path.Combine(temp.Ruta, "NoExiste.json"));

                Assert.That(repositorio.ObtenerTodos(), Is.Empty);
            }
        }

        [Test]
        public void Guardar_DejaUnRespaldoDelArchivoAnterior()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var rutaArchivo = Path.Combine(temp.Ruta, "GruposDestinatarios.json");
                var repositorio = new JsonGrupoDestinatariosRepository(rutaArchivo);
                var ana = Destinatario.Crear("Ana", "ana@empresa.com").Value;

                repositorio.Guardar(new[] { new GrupoDestinatarios("Uno", new[] { ana }) });
                repositorio.Guardar(new[] { new GrupoDestinatarios("Dos", new[] { ana }) });

                Assert.That(Directory.GetFiles(temp.Ruta, "GruposDestinatarios.json.bak-*").Any(), Is.True);
            }
        }
    }
}
