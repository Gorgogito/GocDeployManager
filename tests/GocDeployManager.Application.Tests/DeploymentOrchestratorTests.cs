using System.Collections.Generic;
using GocDeployManager.Application.Deploy;
using GocDeployManager.Common;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Domain.Entities;
using Moq;
using NUnit.Framework;

namespace GocDeployManager.Application.Tests
{
    [TestFixture]
    public class DeploymentOrchestratorTests
    {
        private Mock<IGitClient> _git;
        private Mock<IMsBuildRunner> _msbuild;
        private Mock<IFileDeployer> _deployer;
        private Mock<ISistemaRepository> _sistemas;
        private Mock<IExclusionRulesRepository> _exclusionRules;
        private Mock<IDeployHistoryRepository> _historial;
        private Mock<IPublicadorEventosDespliegue> _publicador;
        private Mock<IAppLogger> _logger;
        private DeploymentOrchestrator _orquestador;

        private readonly Sistema _sit = new Sistema("SIT", "SIT");
        private ConfiguracionSistema _configuracionSit;
        private Ambiente _ambienteDesarrollo;

        [SetUp]
        public void SetUp()
        {
            _git = new Mock<IGitClient>();
            _msbuild = new Mock<IMsBuildRunner>();
            _deployer = new Mock<IFileDeployer>();
            _sistemas = new Mock<ISistemaRepository>();
            _exclusionRules = new Mock<IExclusionRulesRepository>();
            _historial = new Mock<IDeployHistoryRepository>();
            _publicador = new Mock<IPublicadorEventosDespliegue>();
            _logger = new Mock<IAppLogger>();

            _orquestador = new DeploymentOrchestrator(
                _git.Object, _msbuild.Object, _deployer.Object, _sistemas.Object, _exclusionRules.Object, _historial.Object,
                _publicador.Object, _logger.Object);

            var secuencia = new SecuenciaDeBuild(_sit, new[]
            {
                new PasoDeBuild(1, "Sit.BusinessEntities"),
                new PasoDeBuild(2, "SITSolution"),
            });
            _configuracionSit = new ConfiguracionSistema(
                _sit, "https://bitbucket.org/org/sit-integra-web.git", @"SITSolution\PrecompiledWeb\IN-SIT", secuencia);

            _ambienteDesarrollo = new Ambiente("Desarrollo", new[]
            {
                new AmbienteSistema(_sit, @"\\Sdpeapp00009\Aplicaciones\SIT"),
            });

            _exclusionRules.Setup(r => r.ObtenerPatrones()).Returns(new List<string> { "web.config" });
        }

        private SolicitudDespliegue CrearSolicitud() => new SolicitudDespliegue(
            Goc.Crear("GOC-00001").Value, _ambienteDesarrollo, new[] { _sit },
            "jtorres", "jtorres.win", "LAPTOP-01", "jtorres.bb", "clave-bb", @"C:\Clonado");

        private SolicitudDespliegue CrearSolicitudConNotificaciones() => new SolicitudDespliegue(
            Goc.Crear("GOC-00001").Value, _ambienteDesarrollo, new[] { _sit },
            "jtorres", "jtorres.win", "LAPTOP-01", "jtorres.bb", "clave-bb", @"C:\Clonado",
            notificarResultado: true, gruposDestinatariosSeleccionados: new[] { "Desarrollo" });

        [Test]
        public void EjecutarDespliegue_CaminoFeliz_RegistraExitoEnHistorial()
        {
            _sistemas.Setup(s => s.ObtenerConfiguracion(_sit)).Returns(Result.Ok(_configuracionSit));
            _git.Setup(g => g.ClonarORama(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Result.Ok());
            _msbuild.Setup(m => m.EjecutarPaso(It.IsAny<PasoDeBuild>(), It.IsAny<string>()))
                .Returns(Result.Ok("compilación correcta"));
            _deployer.Setup(d => d.Copiar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>()))
                .Returns(Result.Ok());

            var resultado = _orquestador.EjecutarDespliegue(CrearSolicitud());

            Assert.That(resultado.IsSuccess, Is.True);
            _historial.Verify(h => h.Registrar(It.Is<Despliegue>(d => d.Resultado == ResultadoDespliegue.Exitoso)), Times.Once);
        }

        [Test]
        public void EjecutarDespliegue_SiFallaElClonado_RegistraFalloYNoCompila()
        {
            _sistemas.Setup(s => s.ObtenerConfiguracion(_sit)).Returns(Result.Ok(_configuracionSit));
            _git.Setup(g => g.ClonarORama(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Result.Fail("rama no encontrada"));

            var resultado = _orquestador.EjecutarDespliegue(CrearSolicitud());

            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Does.Contain("rama no encontrada"));
            _msbuild.Verify(m => m.EjecutarPaso(It.IsAny<PasoDeBuild>(), It.IsAny<string>()), Times.Never);
            _historial.Verify(h => h.Registrar(It.Is<Despliegue>(d => d.Resultado == ResultadoDespliegue.Fallido)), Times.Once);
        }

        [Test]
        public void EjecutarDespliegue_SiFallaUnPasoDeBuild_NoCopiaYRegistraFallo()
        {
            _sistemas.Setup(s => s.ObtenerConfiguracion(_sit)).Returns(Result.Ok(_configuracionSit));
            _git.Setup(g => g.ClonarORama(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Result.Ok());
            _msbuild.Setup(m => m.EjecutarPaso(It.IsAny<PasoDeBuild>(), It.IsAny<string>()))
                .Returns(Result.Fail<string>("error de compilación en Sit.BusinessEntities"));

            var resultado = _orquestador.EjecutarDespliegue(CrearSolicitud());

            Assert.That(resultado.IsFailure, Is.True);
            _deployer.Verify(d => d.Copiar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>()), Times.Never);
            _historial.Verify(h => h.Registrar(It.Is<Despliegue>(d => d.Resultado == ResultadoDespliegue.Fallido)), Times.Once);
        }

        [Test]
        public void EjecutarDespliegue_SiElAmbienteNoSoportaElSistema_FallaSinClonar()
        {
            var otroSistema = new Sistema("IDI", "IDI");
            _sistemas.Setup(s => s.ObtenerConfiguracion(otroSistema)).Returns(
                Result.Ok(new ConfiguracionSistema(otroSistema, "https://bitbucket.org/org/idi-web.git", "PrecompiledWeb\\IN-IDI",
                    new SecuenciaDeBuild(otroSistema, new[] { new PasoDeBuild(1, "IDI.BussinessEntities") }))));

            var solicitud = new SolicitudDespliegue(
                Goc.Crear("GOC-00002").Value, _ambienteDesarrollo, new[] { otroSistema },
                "jtorres", "jtorres.win", "LAPTOP-01", "jtorres.bb", "clave-bb", @"C:\Clonado");

            var resultado = _orquestador.EjecutarDespliegue(solicitud);

            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Does.Contain("no tiene configurada una ruta"));
            _git.Verify(g => g.ClonarORama(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void EjecutarDespliegue_SiNoSeSolicitaNotificar_NuncaPublicaEventos()
        {
            _sistemas.Setup(s => s.ObtenerConfiguracion(_sit)).Returns(Result.Ok(_configuracionSit));
            _git.Setup(g => g.ClonarORama(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Result.Ok());
            _msbuild.Setup(m => m.EjecutarPaso(It.IsAny<PasoDeBuild>(), It.IsAny<string>()))
                .Returns(Result.Ok("compilación correcta"));
            _deployer.Setup(d => d.Copiar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>()))
                .Returns(Result.Ok());

            _orquestador.EjecutarDespliegue(CrearSolicitud());

            _publicador.Verify(p => p.Publicar(It.IsAny<IEventoDespliegue>()), Times.Never);
        }

        [Test]
        public void EjecutarDespliegue_CaminoFeliz_PublicaInicioYExito()
        {
            _sistemas.Setup(s => s.ObtenerConfiguracion(_sit)).Returns(Result.Ok(_configuracionSit));
            _git.Setup(g => g.ClonarORama(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Result.Ok());
            _msbuild.Setup(m => m.EjecutarPaso(It.IsAny<PasoDeBuild>(), It.IsAny<string>()))
                .Returns(Result.Ok("compilación correcta"));
            _deployer.Setup(d => d.Copiar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>()))
                .Returns(Result.Ok());
            _historial.Setup(h => h.Registrar(It.IsAny<Despliegue>())).Returns(99);

            _orquestador.EjecutarDespliegue(CrearSolicitudConNotificaciones());

            _publicador.Verify(p => p.Publicar(It.IsAny<DespliegueIniciadoEvento>()), Times.Once);
            _publicador.Verify(p => p.Publicar(It.Is<DespliegueExitosoEvento>(e => e.DespliegueId == 99 && e.Goc == "GOC-00001")), Times.Once);
        }

        [Test]
        public void EjecutarDespliegue_SiFallaElClonado_PublicaInicioYFalloConLaEtapaCorrecta()
        {
            _sistemas.Setup(s => s.ObtenerConfiguracion(_sit)).Returns(Result.Ok(_configuracionSit));
            _git.Setup(g => g.ClonarORama(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Result.Fail("rama no encontrada"));
            _historial.Setup(h => h.Registrar(It.IsAny<Despliegue>())).Returns(5);

            _orquestador.EjecutarDespliegue(CrearSolicitudConNotificaciones());

            _publicador.Verify(p => p.Publicar(It.IsAny<DespliegueIniciadoEvento>()), Times.Once);
            _publicador.Verify(p => p.Publicar(It.Is<DespliegueFallidoEvento>(
                e => e.DespliegueId == 5 && e.Etapa == EtapaDespliegue.Clonado && e.MensajeError.Contains("rama no encontrada"))), Times.Once);
        }
    }
}
