using System.Linq;
using GocDeployManager.Domain.Entities;
using NUnit.Framework;

namespace GocDeployManager.Domain.Tests
{
    [TestFixture]
    public class GrupoDestinatariosTests
    {
        [Test]
        public void Constructor_AceptaListaDeMiembros()
        {
            var ana = Destinatario.Crear("Ana", "ana@empresa.com").Value;
            var luis = Destinatario.Crear("Luis", "luis@empresa.com").Value;

            var grupo = new GrupoDestinatarios("Desarrollo", new[] { ana, luis });

            Assert.That(grupo.Nombre, Is.EqualTo("Desarrollo"));
            Assert.That(grupo.Miembros.Count, Is.EqualTo(2));
        }

        [Test]
        public void Constructor_AceptaGrupoSinMiembros()
        {
            var grupo = new GrupoDestinatarios("QA", Enumerable.Empty<Destinatario>());

            Assert.That(grupo.Miembros, Is.Empty);
        }
    }
}
