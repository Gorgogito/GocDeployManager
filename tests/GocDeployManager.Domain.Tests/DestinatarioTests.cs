using GocDeployManager.Domain.Entities;
using NUnit.Framework;

namespace GocDeployManager.Domain.Tests
{
    [TestFixture]
    public class DestinatarioTests
    {
        [TestCase("ana@empresa.com")]
        [TestCase("  ana.perez@empresa.com.pe  ")]
        public void Crear_AceptaCorreoValido(string correo)
        {
            var resultado = Destinatario.Crear("Ana Perez", correo);

            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.CorreoElectronico, Does.Contain("@"));
        }

        [TestCase("")]
        [TestCase("sin-arroba.com")]
        [TestCase("sin-dominio@")]
        [TestCase(null)]
        public void Crear_RechazaCorreoInvalido(string correo)
        {
            var resultado = Destinatario.Crear("Ana Perez", correo);

            Assert.That(resultado.IsFailure, Is.True);
        }

        [Test]
        public void Crear_RechazaNombreVacio()
        {
            var resultado = Destinatario.Crear("", "ana@empresa.com");

            Assert.That(resultado.IsFailure, Is.True);
        }
    }
}
