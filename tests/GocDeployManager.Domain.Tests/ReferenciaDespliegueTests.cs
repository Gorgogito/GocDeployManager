using GocDeployManager.Domain.Entities;
using NUnit.Framework;

namespace GocDeployManager.Domain.Tests
{
    [TestFixture]
    public class ReferenciaDespliegueTests
    {
        [Test]
        public void DesdeGoc_UsaLaRamaFeatureYElNumeroComoCarpeta()
        {
            var referencia = ReferenciaDespliegue.DesdeGoc(Goc.Crear("GOC-00123").Value);

            Assert.That(referencia.Rama, Is.EqualTo("feature/GOC-00123"));
            Assert.That(referencia.Etiqueta, Is.EqualTo("GOC-00123"));
            Assert.That(referencia.CarpetaTrabajo, Is.EqualTo("GOC-00123"));
            Assert.That(referencia.EsGoc, Is.True);
        }

        [TestCase("master")]
        [TestCase("develop")]
        [TestCase("release/2.3")]
        [TestCase("hotfix/GOC-00010-urgente")]
        [TestCase("  develop  ")]
        public void DesdeRama_AceptaNombresValidos(string entrada)
        {
            var resultado = ReferenciaDespliegue.DesdeRama(entrada);

            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Rama, Is.EqualTo(entrada.Trim()));
            Assert.That(resultado.Value.EsGoc, Is.False);
        }

        [Test]
        public void DesdeRama_NormalizaLaCarpetaDeTrabajo()
        {
            var resultado = ReferenciaDespliegue.DesdeRama("release/2.3");

            Assert.That(resultado.Value.CarpetaTrabajo, Is.EqualTo("release_2.3"));
        }

        [Test]
        public void DesdeRama_RecortaLaEtiquetaALaColumnaDelHistorial()
        {
            var resultado = ReferenciaDespliegue.DesdeRama("feature/" + new string('a', 100));

            Assert.That(resultado.Value.Etiqueta.Length, Is.LessThanOrEqualTo(50));
            Assert.That(resultado.Value.Rama.Length, Is.EqualTo(108));
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase(null)]
        [TestCase("mi rama")]
        [TestCase("rama..otra")]
        [TestCase("-rama")]
        [TestCase("/rama")]
        [TestCase("rama/")]
        [TestCase("rama.")]
        [TestCase("rama.lock")]
        [TestCase("a//b")]
        [TestCase("a/.b")]
        [TestCase("rama~1")]
        [TestCase("rama^")]
        [TestCase("rama:x")]
        [TestCase("ra*ma")]
        [TestCase("ra\"ma")]
        [TestCase("a@{b")]
        public void DesdeRama_RechazaNombresInvalidos(string entrada)
        {
            Assert.That(ReferenciaDespliegue.DesdeRama(entrada).IsFailure, Is.True);
        }
    }
}
