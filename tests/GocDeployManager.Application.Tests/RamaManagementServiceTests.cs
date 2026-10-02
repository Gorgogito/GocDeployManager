using System.Collections.Generic;
using GocDeployManager.Application.Ramas;
using GocDeployManager.Domain.Abstractions;
using Moq;
using NUnit.Framework;

namespace GocDeployManager.Application.Tests
{
    [TestFixture]
    public class RamaManagementServiceTests
    {
        private Mock<IRamaConfiguradaRepository> _repositorio;
        private RamaManagementService _servicio;

        [SetUp]
        public void SetUp()
        {
            _repositorio = new Mock<IRamaConfiguradaRepository>();
            _servicio = new RamaManagementService(_repositorio.Object);
        }

        [Test]
        public void Guardar_RecortaYDescartaVacias()
        {
            IReadOnlyList<string> guardadas = null;
            _repositorio.Setup(r => r.Guardar(It.IsAny<IReadOnlyList<string>>()))
                .Callback<IReadOnlyList<string>>(l => guardadas = l);

            var resultado = _servicio.Guardar(new[] { " master ", "", "develop", "  " });

            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(guardadas, Is.EqualTo(new[] { "master", "develop" }));
        }

        [Test]
        public void Guardar_RechazaDuplicadas()
        {
            var resultado = _servicio.Guardar(new[] { "master", "master" });

            Assert.That(resultado.IsFailure, Is.True);
            _repositorio.Verify(r => r.Guardar(It.IsAny<IReadOnlyList<string>>()), Times.Never);
        }

        [Test]
        public void Guardar_RechazaNombresInvalidos()
        {
            var resultado = _servicio.Guardar(new[] { "master", "mi rama" });

            Assert.That(resultado.IsFailure, Is.True);
            _repositorio.Verify(r => r.Guardar(It.IsAny<IReadOnlyList<string>>()), Times.Never);
        }

        [Test]
        public void Guardar_PermiteListaVacia()
        {
            Assert.That(_servicio.Guardar(new string[0]).IsSuccess, Is.True);
        }
    }
}
