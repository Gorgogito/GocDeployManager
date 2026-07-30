using System.Security.Cryptography;
using GocDeployManager.Application.Auth;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Domain.Entities;
using Moq;
using NUnit.Framework;

namespace GocDeployManager.Application.Tests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private Mock<IAppUserRepository> _usuarios;
        private Mock<IPasswordHasher> _hasher;
        private Mock<ICredentialProtector> _protector;
        private Mock<IAppLogger> _logger;
        private AuthenticationService _servicio;

        [SetUp]
        public void SetUp()
        {
            _usuarios = new Mock<IAppUserRepository>();
            _hasher = new Mock<IPasswordHasher>();
            _protector = new Mock<ICredentialProtector>();
            _logger = new Mock<IAppLogger>();
            _servicio = new AuthenticationService(_usuarios.Object, _hasher.Object, _protector.Object, _logger.Object);
        }

        [Test]
        public void IniciarSesion_ConCredencialesCorrectas_DevuelveSesionConCredencialBitbucketDescifrada()
        {
            var usuario = new AppUser("jtorres", "Jorge Torres", RolUsuario.Operador, "hash", "sal");
            usuario.EstablecerCredencialesBitbucket("jtorres.bb", "protegida-base64");

            _usuarios.Setup(r => r.BuscarPorNombreUsuario("jtorres")).Returns(usuario);
            _hasher.Setup(h => h.Verificar("clave123", "hash", "sal")).Returns(true);
            _protector.Setup(p => p.Desproteger("protegida-base64")).Returns("clave-bitbucket-en-claro");

            var resultado = _servicio.IniciarSesion("jtorres", "clave123");

            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Usuario.NombreUsuario, Is.EqualTo("jtorres"));
            Assert.That(resultado.Value.ContrasenaBitbucketEnClaro, Is.EqualTo("clave-bitbucket-en-claro"));
        }

        [Test]
        public void IniciarSesion_SiLaCredencialBitbucketNoSePuedeDesprotegerEnEstaMaquina_PermiteElLoginIgual()
        {
            // Reproduce el bug real: DPAPI (CurrentUser) fue protegido en otra
            // máquina/usuario de Windows — Desproteger lanza CryptographicException
            // ("Clave no válida para utilizar en el estado especificado."). El
            // login no debe caerse por esto; el operador se entera recién al
            // intentar desplegar (ContrasenaBitbucketEnClaro queda en null).
            var usuario = new AppUser("jtorres", "Jorge Torres", RolUsuario.Operador, "hash", "sal");
            usuario.EstablecerCredencialesBitbucket("jtorres.bb", "protegida-en-otra-maquina");

            _usuarios.Setup(r => r.BuscarPorNombreUsuario("jtorres")).Returns(usuario);
            _hasher.Setup(h => h.Verificar("clave123", "hash", "sal")).Returns(true);
            _protector.Setup(p => p.Desproteger("protegida-en-otra-maquina"))
                .Throws(new CryptographicException("Clave no válida para utilizar en el estado especificado."));

            var resultado = _servicio.IniciarSesion("jtorres", "clave123");

            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.ContrasenaBitbucketEnClaro, Is.Null);
        }

        [Test]
        public void IniciarSesion_ConContrasenaIncorrecta_FallaConMensajeGenerico()
        {
            var usuario = new AppUser("jtorres", "Jorge Torres", RolUsuario.Operador, "hash", "sal");
            _usuarios.Setup(r => r.BuscarPorNombreUsuario("jtorres")).Returns(usuario);
            _hasher.Setup(h => h.Verificar(It.IsAny<string>(), "hash", "sal")).Returns(false);

            var resultado = _servicio.IniciarSesion("jtorres", "claveMala");

            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.EqualTo("Usuario o contraseña incorrectos."));
        }

        [Test]
        public void IniciarSesion_ConUsuarioInexistente_FallaConElMismoMensajeGenerico()
        {
            _usuarios.Setup(r => r.BuscarPorNombreUsuario(It.IsAny<string>())).Returns((AppUser)null);

            var resultado = _servicio.IniciarSesion("no-existe", "cualquiera");

            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.EqualTo("Usuario o contraseña incorrectos."));
        }

        [Test]
        public void IniciarSesion_ConUsuarioDesactivado_Falla()
        {
            var usuario = new AppUser("jtorres", "Jorge Torres", RolUsuario.Operador, "hash", "sal");
            usuario.Desactivar();

            _usuarios.Setup(r => r.BuscarPorNombreUsuario("jtorres")).Returns(usuario);
            _hasher.Setup(h => h.Verificar(It.IsAny<string>(), "hash", "sal")).Returns(true);

            var resultado = _servicio.IniciarSesion("jtorres", "clave123");

            Assert.That(resultado.IsFailure, Is.True);
        }
    }
}
