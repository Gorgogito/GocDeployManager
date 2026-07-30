using GocDeployManager.Domain.Entities;
using NUnit.Framework;

namespace GocDeployManager.Domain.Tests
{
    [TestFixture]
    public class MapeoCanalTeamsTests
    {
        [Test]
        public void Aplica_UnComodinAplicaATodoSistemaYAmbiente()
        {
            var mapeo = new MapeoCanalTeams(null, null, "https://webhook/sura-peru-teams");

            Assert.That(mapeo.Aplica("SIT", "Testing"), Is.True);
            Assert.That(mapeo.Aplica("IDI", "Producción"), Is.True);
            Assert.That(mapeo.Especificidad, Is.EqualTo(0));
        }

        [Test]
        public void Aplica_UnMapeoEspecificoSoloAplicaASuCombinacion()
        {
            var mapeo = new MapeoCanalTeams("SIT", "Producción", "https://webhook/sit-prod");

            Assert.That(mapeo.Aplica("SIT", "Producción"), Is.True);
            Assert.That(mapeo.Aplica("SIT", "Testing"), Is.False);
            Assert.That(mapeo.Aplica("IDI", "Producción"), Is.False);
            Assert.That(mapeo.Especificidad, Is.EqualTo(2));
        }
    }
}
