using GocDeployManager.Application.Usuarios;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Domain.Entities;
using Moq;
using NUnit.Framework;

namespace GocDeployManager.Application.Tests
{
    [TestFixture]
    public class UserManagementServiceTests
    {
        private Mock<IAppUserRepository> _usuarios;
        private Mock<IPasswordHasher> _hasher;
        private Mock<ICredentialProtector> _protector;
        private Mock<IAppLogger> _logger;
        private UserManagementService _servicio;

        [SetUp]
        public void SetUp()
        {
            _usuarios = new Mock<IAppUserRepository>();
            _hasher = new Mock<IPasswordHasher>();
            _protector = new Mock<ICredentialProtector>();
            _logger = new Mock<IAppLogger>();
            _servicio = new UserManagementService(_usuarios.Object, _hasher.Object, _protector.Object, _logger.Object);
        }

        [Test]
        public void CambiarRol_ConUsuarioExistente_ActualizaElRolYDejaRastroEnElLog()
        {
            var usuario = new AppUser("jtorres", "Jorge Torres", RolUsuario.Operador, "hash", "sal");
            _usuarios.Setup(r => r.BuscarPorNombreUsuario("jtorres")).Returns(usuario);

            var resultado = _servicio.CambiarRol("jtorres", RolUsuario.Administrador);

            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(usuario.Rol, Is.EqualTo(RolUsuario.Administrador));
            _usuarios.Verify(r => r.Actualizar(usuario), Times.Once);
            _logger.Verify(l => l.Info(It.Is<string>(m =>
                m.Contains("jtorres") && m.Contains("Operador") && m.Contains("Administrador"))), Times.Once);
        }

        [Test]
        public void CambiarRol_ConUsuarioInexistente_FallaYNoDejaRastroEnElLog()
        {
            _usuarios.Setup(r => r.BuscarPorNombreUsuario("no-existe")).Returns((AppUser)null);

            var resultado = _servicio.CambiarRol("no-existe", RolUsuario.Administrador);

            Assert.That(resultado.IsFailure, Is.True);
            _logger.Verify(l => l.Info(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ResetearContrasena_ConUsuarioExistente_DejaRastroEnElLogSinIncluirLaContrasena()
        {
            var usuario = new AppUser("jtorres", "Jorge Torres", RolUsuario.Operador, "hash-viejo", "sal-vieja");
            _usuarios.Setup(r => r.BuscarPorNombreUsuario("jtorres")).Returns(usuario);

            string hashGenerado = "hash-nuevo";
            string salGenerada = "sal-nueva";
            _hasher.Setup(h => h.Generar("ClaveNueva123!", out hashGenerado, out salGenerada));

            var resultado = _servicio.ResetearContrasena("jtorres", "ClaveNueva123!");

            Assert.That(resultado.IsSuccess, Is.True);
            _usuarios.Verify(r => r.Actualizar(usuario), Times.Once);

            _logger.Verify(l => l.Info(It.Is<string>(m =>
                m.Contains("jtorres") && !m.Contains("ClaveNueva123!"))), Times.Once);
        }

        [Test]
        public void ResetearContrasena_ConUsuarioInexistente_FallaYNoDejaRastroEnElLog()
        {
            _usuarios.Setup(r => r.BuscarPorNombreUsuario("no-existe")).Returns((AppUser)null);

            var resultado = _servicio.ResetearContrasena("no-existe", "cualquiera");

            Assert.That(resultado.IsFailure, Is.True);
            _logger.Verify(l => l.Info(It.IsAny<string>()), Times.Never);
        }
    }
}
