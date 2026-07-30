using System.Collections.Generic;
using GocDeployManager.Notifications.Plantillas;
using NUnit.Framework;

namespace GocDeployManager.Notifications.Tests
{
    [TestFixture]
    public class PlantillaRendererDeTokensTests
    {
        [Test]
        public void Renderizar_ReemplazaTodosLosTokensPresentes()
        {
            var renderer = new PlantillaRendererDeTokens();
            var valores = new Dictionary<string, string> { ["Goc"] = "GOC-00001", ["Ambiente"] = "Testing" };

            var resultado = renderer.Renderizar("GOC {{Goc}} en {{Ambiente}}", valores);

            Assert.That(resultado, Is.EqualTo("GOC GOC-00001 en Testing"));
        }

        [Test]
        public void Renderizar_DejaIntactoUnTokenSinValor()
        {
            var renderer = new PlantillaRendererDeTokens();
            var valores = new Dictionary<string, string> { ["Goc"] = "GOC-00001" };

            var resultado = renderer.Renderizar("{{Goc}} - {{NoExiste}}", valores);

            Assert.That(resultado, Is.EqualTo("GOC-00001 - {{NoExiste}}"));
        }
    }
}
