using System;
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
            _logger = new Mock<IAppLogger>();

            _orquestador = new DeploymentOrchestrator(
                _git.Object, _msbuild.Object, _deployer.Object, _sistemas.Object, _exclusionRules.Object, _historial.Object, _logger.Object);

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

        [Test]
        public void EjecutarDespliegue_CaminoFeliz_RegistraExitoEnHistorial()
        {
            _sistemas.Setup(s => s.ObtenerConfiguracion(_sit)).Returns(Result.Ok(_configuracionSit));
            _git.Setup(g => g.ClonarORama(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<string>>()))
                .Returns(Result.Ok());
            _msbuild.Setup(m => m.EjecutarPaso(It.IsAny<PasoDeBuild>(), It.IsAny<string>(), It.IsAny<Action<string>>()))
                .Returns(Result.Ok("compilación correcta"));
            _deployer.Setup(d => d.Copiar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<Action<string, bool>>()))
                .Returns(Result.Ok());

            var resultado = _orquestador.EjecutarDespliegue(CrearSolicitud());

            Assert.That(resultado.IsSuccess, Is.True);
            _historial.Verify(h => h.Registrar(It.Is<Despliegue>(d => d.Resultado == ResultadoDespliegue.Exitoso)), Times.Once);
        }

        [Test]
        public void EjecutarDespliegue_SiGitOMsBuildEmitenLineasEnBlanco_NoRompeElReporteDeProgreso()
        {
            // Reproduce el bug real: git y MSBuild emiten líneas vacías/con
            // espacios como separadores visuales de su propia salida, y
            // MensajeSalidaDespliegue exige texto no vacío — antes de este fix,
            // una línea así tumbaba el despliegue completo con "El valor no
            // puede estar vacío. Nombre del parámetro: texto".
            _sistemas.Setup(s => s.ObtenerConfiguracion(_sit)).Returns(Result.Ok(_configuracionSit));
            _git.Setup(g => g.ClonarORama(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<string>>()))
                .Callback<string, string, string, string, string, Action<string>>((url, rama, ruta, u, p, onLinea) => onLinea?.Invoke("   "))
                .Returns(Result.Ok());
            _msbuild.Setup(m => m.EjecutarPaso(It.IsAny<PasoDeBuild>(), It.IsAny<string>(), It.IsAny<Action<string>>()))
                .Callback<PasoDeBuild, string, Action<string>>((paso, ruta, onLinea) => onLinea?.Invoke(string.Empty))
                .Returns(Result.Ok("compilación correcta"));
            _deployer.Setup(d => d.Copiar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<Action<string, bool>>()))
                .Returns(Result.Ok());

            var mensajes = new List<MensajeSalidaDespliegue>();
            var progreso = new Progress<MensajeSalidaDespliegue>(mensajes.Add);

            Assert.DoesNotThrow(() =>
            {
                var resultado = _orquestador.EjecutarDespliegue(CrearSolicitud(), progreso);
                Assert.That(resultado.IsSuccess, Is.True);
            });
        }

        [Test]
        public void EjecutarDespliegue_SiFallaElClonado_RegistraFalloYNoCompila()
        {
            _sistemas.Setup(s => s.ObtenerConfiguracion(_sit)).Returns(Result.Ok(_configuracionSit));
            _git.Setup(g => g.ClonarORama(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<string>>()))
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
            _git.Setup(g => g.ClonarORama(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<string>>()))
                .Returns(Result.Ok());
            _msbuild.Setup(m => m.EjecutarPaso(It.IsAny<PasoDeBuild>(), It.IsAny<string>(), It.IsAny<Action<string>>()))
                .Returns(Result.Fail<string>("error de compilación en Sit.BusinessEntities"));

            var resultado = _orquestador.EjecutarDespliegue(CrearSolicitud());

            Assert.That(resultado.IsFailure, Is.True);
            _deployer.Verify(d => d.Copiar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>()), Times.Never);
            _historial.Verify(h => h.Registrar(It.Is<Despliegue>(d => d.Resultado == ResultadoDespliegue.Fallido)), Times.Once);
        }

        [Test]
        public void EjecutarDespliegue_SiElCodigoDeSistemaTieneCaracteresInvalidos_FallaSinCrashearYSinClonar()
        {
            // Reproduce el bug real: un código de sistema con un carácter no
            // válido para una ruta (p. ej. pegado desde "Copiar como ruta de
            // acceso", que envuelve el texto en comillas) hacía que
            // Path.Combine lanzara ArgumentException sin capturar, tumbando
            // el despliegue completo en vez de registrarlo como fallido.
            var sistemaConComillas = new Sistema("\"SIT\"", "SIT");
            _sistemas.Setup(s => s.ObtenerConfiguracion(sistemaConComillas)).Returns(Result.Ok(_configuracionSit));

            var ambiente = new Ambiente("Desarrollo", new[] { new AmbienteSistema(sistemaConComillas, @"\\Sdpeapp00009\Aplicaciones\SIT") });
            var solicitud = new SolicitudDespliegue(
                Goc.Crear("GOC-00003").Value, ambiente, new[] { sistemaConComillas },
                "jtorres", "jtorres.win", "LAPTOP-01", "jtorres.bb", "clave-bb", @"C:\Clonado");

            var resultado = _orquestador.EjecutarDespliegue(solicitud);

            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Does.Contain("Bitbucket"));
            _git.Verify(g => g.ClonarORama(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _historial.Verify(h => h.Registrar(It.Is<Despliegue>(d => d.Resultado == ResultadoDespliegue.Fallido)), Times.Once);
        }

        [Test]
        public void EjecutarDespliegue_SiLaCarpetaPrecompiladaTieneCaracteresInvalidos_FallaSinCrashearYSinCopiar()
        {
            // Mismo bug, en el punto donde de verdad se reportó: la carpeta
            // precompilada (Configuración › Bitbucket) con comillas rompía
            // Path.Combine justo antes de copiar, sin capturar la excepción.
            var configuracionConComillas = new ConfiguracionSistema(
                _sit, "https://bitbucket.org/org/sit-integra-web.git", "\"SITSolution\\PrecompiledWeb\\IN-SIT\"",
                new SecuenciaDeBuild(_sit, new[] { new PasoDeBuild(1, "Sit.BusinessEntities") }));

            _sistemas.Setup(s => s.ObtenerConfiguracion(_sit)).Returns(Result.Ok(configuracionConComillas));
            _git.Setup(g => g.ClonarORama(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<string>>()))
                .Returns(Result.Ok());
            _msbuild.Setup(m => m.EjecutarPaso(It.IsAny<PasoDeBuild>(), It.IsAny<string>(), It.IsAny<Action<string>>()))
                .Returns(Result.Ok("compilación correcta"));

            var resultado = _orquestador.EjecutarDespliegue(CrearSolicitud());

            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Does.Contain("carpeta precompilada"));
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
    }
}
