using System;
using System.IO;
using System.Linq;
using GocDeployManager.Services;
using NLog;
using NUnit.Framework;

namespace GocDeployManager.Infrastructure.Tests
{
    /// <summary>
    /// Pruebas contra archivo real (no mockeado): confirman que NLog.config
    /// (autoReload, target de archivo) realmente escribe en disco, en la
    /// ruta inyectada en runtime — no solo que la llamada a Info/Error no
    /// lance una excepción.
    /// </summary>
    [TestFixture]
    public sealed class NLogAppLoggerTests
    {
        [TearDown]
        public void Limpiar()
        {
            // Solo Flush (espera a que el target asíncrono vacíe su cola) —
            // nunca Shutdown(): NLog solo busca NLog.config una vez por
            // proceso, así que un Shutdown() entre pruebas deja la
            // configuración en null para el resto de la corrida.
            LogManager.Flush();
        }

        [Test]
        public void Info_EscribeUnaLineaRealEnElArchivoDeLog()
        {
            using (var carpeta = new CarpetaTemporalDePrueba())
            {
                var logger = new NLogAppLogger(carpeta.Ruta);

                logger.Info("mensaje de prueba de info");
                LogManager.Flush();

                var contenido = LeerArchivoDeLogGenerado(carpeta.Ruta);

                Assert.That(contenido, Does.Contain("INFO"));
                Assert.That(contenido, Does.Contain("mensaje de prueba de info"));
            }
        }

        [Test]
        public void Error_ConExcepcion_EscribeElMensajeYElDetalleDeLaExcepcion()
        {
            using (var carpeta = new CarpetaTemporalDePrueba())
            {
                var logger = new NLogAppLogger(carpeta.Ruta);

                Exception excepcionReal;
                try
                {
                    throw new InvalidOperationException("fallo simulado real");
                }
                catch (Exception ex)
                {
                    excepcionReal = ex;
                }

                logger.Error("ocurrió un error", excepcionReal);
                LogManager.Flush();

                var contenido = LeerArchivoDeLogGenerado(carpeta.Ruta);

                Assert.That(contenido, Does.Contain("ERROR"));
                Assert.That(contenido, Does.Contain("ocurrió un error"));
                Assert.That(contenido, Does.Contain("fallo simulado real"));
            }
        }

        [Test]
        public void Warn_EscribeConNivelWarn()
        {
            using (var carpeta = new CarpetaTemporalDePrueba())
            {
                var logger = new NLogAppLogger(carpeta.Ruta);

                logger.Warn("advertencia de prueba");
                LogManager.Flush();

                var contenido = LeerArchivoDeLogGenerado(carpeta.Ruta);

                Assert.That(contenido, Does.Contain("WARN"));
                Assert.That(contenido, Does.Contain("advertencia de prueba"));
            }
        }

        private static string LeerArchivoDeLogGenerado(string carpeta)
        {
            var archivo = Directory.GetFiles(carpeta, "goc-deploy-*.log").SingleOrDefault();
            Assert.That(archivo, Is.Not.Null, "NLog no generó el archivo de log esperado.");
            return File.ReadAllText(archivo);
        }
    }
}
