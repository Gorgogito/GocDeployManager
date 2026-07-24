using GocDeployManager.Services;
using NUnit.Framework;

namespace GocDeployManager.Infrastructure.Tests
{
    [TestFixture]
    public class DpapiCredentialProtectorTests
    {
        [Test]
        public void Proteger_YDesproteger_HaceRoundTrip()
        {
            var protector = new DpapiCredentialProtector();

            var protegido = protector.Proteger("clave-secreta-de-bitbucket");
            var recuperado = protector.Desproteger(protegido);

            Assert.That(recuperado, Is.EqualTo("clave-secreta-de-bitbucket"));
            Assert.That(protegido, Is.Not.EqualTo("clave-secreta-de-bitbucket"));
        }
    }
}
