using System.Collections.Generic;
using GocDeployManager.Application.Configuracion;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Domain.Entities;
using Moq;
using NUnit.Framework;

namespace GocDeployManager.Application.Tests
{
    [TestFixture]
    public class MigradorConfiguracionTests
    {
        private Mock<IAmbienteRepository> _origenAmbientes;
        private Mock<IAmbienteRepository> _destinoAmbientes;
        private Mock<ISistemaRepository> _origenSistemas;
        private Mock<ISistemaRepository> _destinoSistemas;
        private MigradorConfiguracion _migrador;

        private readonly Sistema _sit = new Sistema("SIT", "SIT");
        private ConfiguracionSistema _configuracionSit;
        private Ambiente _ambienteDesarrollo;

        [SetUp]
        public void SetUp()
        {
            _origenAmbientes = new Mock<IAmbienteRepository>();
            _destinoAmbientes = new Mock<IAmbienteRepository>();
            _origenSistemas = new Mock<ISistemaRepository>();
            _destinoSistemas = new Mock<ISistemaRepository>();

            _migrador = new MigradorConfiguracion(_origenAmbientes.Object, _destinoAmbientes.Object, _origenSistemas.Object, _destinoSistemas.Object);

            _configuracionSit = new ConfiguracionSistema(
                _sit, "https://bitbucket.org/org/sit-integra-web.git", @"SITSolution\PrecompiledWeb\IN-SIT",
                new SecuenciaDeBuild(_sit, new[] { new PasoDeBuild(1, "Sit.BusinessEntities") }));
            _ambienteDesarrollo = new Ambiente("Desarrollo", new[] { new AmbienteSistema(_sit, @"\\Sdpeapp00009\Aplicaciones\SIT") });

            // Destino vacío por defecto — cada prueba ajusta lo que necesite.
            _destinoAmbientes.Setup(r => r.ObtenerTodos()).Returns(new List<Ambiente>().AsReadOnly());
            _destinoSistemas.Setup(r => r.ObtenerTodasLasConfiguraciones()).Returns(new List<ConfiguracionSistema>().AsReadOnly());
        }

        [Test]
        public void Migrar_ConDestinoVacioYOrigenConDatos_CopiaTodoYDevuelveResumen()
        {
            _origenAmbientes.Setup(r => r.ObtenerTodos()).Returns(new[] { _ambienteDesarrollo });
            _origenSistemas.Setup(r => r.ObtenerTodasLasConfiguraciones()).Returns(new[] { _configuracionSit });

            var resultado = _migrador.Migrar();

            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value, Does.Contain("1 ambiente"));
            Assert.That(resultado.Value, Does.Contain("1 sistema"));
            _destinoAmbientes.Verify(r => r.Guardar(It.Is<IReadOnlyList<Ambiente>>(l => l.Count == 1)), Times.Once);
            _destinoSistemas.Verify(r => r.Guardar(It.Is<IReadOnlyList<ConfiguracionSistema>>(l => l.Count == 1)), Times.Once);
        }

        [Test]
        public void Migrar_SiElDestinoYaTieneAmbientes_SeNiegaYNoEscribeNada()
        {
            _destinoAmbientes.Setup(r => r.ObtenerTodos()).Returns(new[] { _ambienteDesarrollo });
            _origenAmbientes.Setup(r => r.ObtenerTodos()).Returns(new[] { _ambienteDesarrollo });
            _origenSistemas.Setup(r => r.ObtenerTodasLasConfiguraciones()).Returns(new[] { _configuracionSit });

            var resultado = _migrador.Migrar();

            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Does.Contain("ya tiene"));
            _destinoAmbientes.Verify(r => r.Guardar(It.IsAny<IReadOnlyList<Ambiente>>()), Times.Never);
            _destinoSistemas.Verify(r => r.Guardar(It.IsAny<IReadOnlyList<ConfiguracionSistema>>()), Times.Never);
        }

        [Test]
        public void Migrar_SiElDestinoYaTieneSistemas_SeNiegaYNoEscribeNada()
        {
            _destinoSistemas.Setup(r => r.ObtenerTodasLasConfiguraciones()).Returns(new[] { _configuracionSit });
            _origenAmbientes.Setup(r => r.ObtenerTodos()).Returns(new[] { _ambienteDesarrollo });
            _origenSistemas.Setup(r => r.ObtenerTodasLasConfiguraciones()).Returns(new[] { _configuracionSit });

            var resultado = _migrador.Migrar();

            Assert.That(resultado.IsFailure, Is.True);
            _destinoAmbientes.Verify(r => r.Guardar(It.IsAny<IReadOnlyList<Ambiente>>()), Times.Never);
        }

        [Test]
        public void Migrar_SiElOrigenEstaVacio_NoHaceNadaYAvisa()
        {
            _origenAmbientes.Setup(r => r.ObtenerTodos()).Returns(new List<Ambiente>().AsReadOnly());
            _origenSistemas.Setup(r => r.ObtenerTodasLasConfiguraciones()).Returns(new List<ConfiguracionSistema>().AsReadOnly());

            var resultado = _migrador.Migrar();

            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Does.Contain("No se encontraron"));
            _destinoAmbientes.Verify(r => r.Guardar(It.IsAny<IReadOnlyList<Ambiente>>()), Times.Never);
        }
    }
}
