using System.IO;
using System.Linq;
using GocDeployManager.Notifications.Plantillas;
using NUnit.Framework;

namespace GocDeployManager.Notifications.Tests
{
    [TestFixture]
    public class FilePlantillaRepositoryTests
    {
        [Test]
        public void Obtener_SiNoHayArchivoGuardado_DevuelveLaPlantillaPorDefecto()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var repositorio = new FilePlantillaRepository(temp.Ruta);

                var contenido = repositorio.Obtener(NombresDeCanal.Email, TiposDeEvento.Exitoso);

                Assert.That(contenido, Is.EqualTo(PlantillasPorDefecto.Obtener(NombresDeCanal.Email, TiposDeEvento.Exitoso)));
            }
        }

        [Test]
        public void GuardarYObtener_HaceRoundTripCompleto()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var repositorio = new FilePlantillaRepository(temp.Ruta);

                repositorio.Guardar(NombresDeCanal.Email, TiposDeEvento.Iniciado, "<p>Plantilla personalizada {{Goc}}</p>");

                Assert.That(repositorio.Obtener(NombresDeCanal.Email, TiposDeEvento.Iniciado), Is.EqualTo("<p>Plantilla personalizada {{Goc}}</p>"));
            }
        }

        [Test]
        public void Guardar_DejaUnRespaldoDeLaVersionAnterior()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var repositorio = new FilePlantillaRepository(temp.Ruta);

                repositorio.Guardar(NombresDeCanal.Teams, TiposDeEvento.Fallido, "{\"v\":1}");
                repositorio.Guardar(NombresDeCanal.Teams, TiposDeEvento.Fallido, "{\"v\":2}");

                var carpetaCanal = Path.Combine(temp.Ruta, NombresDeCanal.Teams);
                Assert.That(Directory.GetFiles(carpetaCanal, "Fallido.json.bak-*").Any(), Is.True);
            }
        }

        [Test]
        public void RestaurarPorDefecto_SobrescribeConLaPlantillaDeFabrica()
        {
            using (var temp = new CarpetaTemporalDePrueba())
            {
                var repositorio = new FilePlantillaRepository(temp.Ruta);

                repositorio.Guardar(NombresDeCanal.Email, TiposDeEvento.Fallido, "<p>algo distinto</p>");
                repositorio.RestaurarPorDefecto(NombresDeCanal.Email, TiposDeEvento.Fallido);

                Assert.That(repositorio.Obtener(NombresDeCanal.Email, TiposDeEvento.Fallido),
                    Is.EqualTo(PlantillasPorDefecto.Obtener(NombresDeCanal.Email, TiposDeEvento.Fallido)));
            }
        }
    }
}
