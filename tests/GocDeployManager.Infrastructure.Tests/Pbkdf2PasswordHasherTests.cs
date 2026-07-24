using GocDeployManager.Services;
using NUnit.Framework;

namespace GocDeployManager.Infrastructure.Tests
{
    [TestFixture]
    public class Pbkdf2PasswordHasherTests
    {
        [Test]
        public void Verificar_ConLaContrasenaCorrecta_DevuelveVerdadero()
        {
            var hasher = new Pbkdf2PasswordHasher();
            hasher.Generar("MiClaveSegura123", out var hash, out var sal);

            Assert.That(hasher.Verificar("MiClaveSegura123", hash, sal), Is.True);
        }

        [Test]
        public void Verificar_ConLaContrasenaIncorrecta_DevuelveFalso()
        {
            var hasher = new Pbkdf2PasswordHasher();
            hasher.Generar("MiClaveSegura123", out var hash, out var sal);

            Assert.That(hasher.Verificar("OtraClave", hash, sal), Is.False);
        }

        [Test]
        public void Generar_ProduceUnaSalDistintaCadaVez()
        {
            var hasher = new Pbkdf2PasswordHasher();
            hasher.Generar("MismaClave", out var hash1, out var sal1);
            hasher.Generar("MismaClave", out var hash2, out var sal2);

            Assert.That(sal1, Is.Not.EqualTo(sal2));
            Assert.That(hash1, Is.Not.EqualTo(hash2));
        }
    }
}
