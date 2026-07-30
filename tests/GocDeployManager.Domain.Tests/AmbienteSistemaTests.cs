using GocDeployManager.Domain.Entities;
using NUnit.Framework;

namespace GocDeployManager.Domain.Tests
{
    [TestFixture]
    public class AmbienteSistemaTests
    {
        private static readonly Sistema Sit = new Sistema("SIT", "SIT");

        [Test]
        public void Constructor_RecortaEspaciosYSaltosDeLineaDeLaRutaDestino()
        {
            // Reproduce el bug real: pegar la ruta desde otro lado dejó un
            // espacio y un salto de línea al final, invisibles a simple vista,
            // que Path.Combine rechazaba recién al momento de desplegar.
            var ambienteSistema = new AmbienteSistema(Sit, "C:\\INTEGRA\\PublicarTesting\\Ambiente_VIII \n");

            Assert.That(ambienteSistema.RutaDestino, Is.EqualTo("C:\\INTEGRA\\PublicarTesting\\Ambiente_VIII"));
        }

        [Test]
        public void Constructor_RechazaRutaQueQuedaVaciaTrasRecortar()
        {
            Assert.Throws<System.ArgumentException>(() => new AmbienteSistema(Sit, "   \n  "));
        }
    }
}
