using GocDeployManager.Common;
using NUnit.Framework;

namespace GocDeployManager.Domain.Tests
{
    [TestFixture]
    public class ResultTests
    {
        [Test]
        public void Ok_MarcaExitoSinError()
        {
            var resultado = Result.Ok(42);

            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value, Is.EqualTo(42));
            Assert.That(resultado.Error, Is.Null);
        }

        [Test]
        public void Fail_MarcaFalloConMensaje()
        {
            var resultado = Result.Fail<int>("algo salió mal");

            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.EqualTo("algo salió mal"));
            Assert.That(resultado.Value, Is.EqualTo(0));
        }
    }
}
