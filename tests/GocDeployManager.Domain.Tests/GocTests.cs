using GocDeployManager.Domain.Entities;
using NUnit.Framework;

namespace GocDeployManager.Domain.Tests
{
    [TestFixture]
    public class GocTests
    {
        [TestCase("GOC-00001")]
        [TestCase("goc-12345")]
        [TestCase("  GOC-00042  ")]
        public void Crear_AceptaFormatoValido(string entrada)
        {
            var resultado = Goc.Crear(entrada);

            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Numero, Does.Match(@"^GOC-\d{5}$"));
        }

        [Test]
        public void Crear_DerivaLaRamaDeBitbucket()
        {
            var resultado = Goc.Crear("GOC-00123");

            Assert.That(resultado.Value.RamaBitbucket, Is.EqualTo("feature/GOC-00123"));
        }

        [TestCase("")]
        [TestCase("GOC-1")]
        [TestCase("GOC-123456")]
        [TestCase("00001")]
        [TestCase(null)]
        public void Crear_RechazaFormatoInvalido(string entrada)
        {
            var resultado = Goc.Crear(entrada);

            Assert.That(resultado.IsFailure, Is.True);
        }
    }
}
