using System.IO;
using GocDeployManager.Domain.Entities;
using GocDeployManager.Infrastructure.Build;
using GocDeployManager.Infrastructure.Git;
using NUnit.Framework;

namespace GocDeployManager.Infrastructure.Tests
{
    /// <summary>
    /// Pruebas contra MSBuild.exe real, compilando las capas reales de
    /// sit-integra-web (clonadas primero con GitClient real, ver
    /// GitClientRealTests) en el mismo orden que compilacion.bat. [Explicit]
    /// hace que "dotnet test" normal las omita; correrlas a propósito con
    /// "dotnet test --filter TestCategory=Real".
    /// </summary>
    [TestFixture]
    [Category("Real")]
    [Explicit("Depende de rutas reales de esta máquina y de MSBuild.exe; correr a propósito con --filter TestCategory=Real.")]
    public sealed class MsBuildRunnerRealTests
    {
        private const string RutaRepoOrigenSit = @"D:\Desarrollo\sit-integra-web";
        private const string RutaMsBuildExe = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe";
        private const string RutaGitExe = "git.exe";

        [SetUp]
        public void VerificarPrerequisitos()
        {
            if (!Directory.Exists(Path.Combine(RutaRepoOrigenSit, ".git")))
                Assert.Ignore($"No se encontró el repo real en '{RutaRepoOrigenSit}' en esta máquina.");

            if (!File.Exists(RutaMsBuildExe))
                Assert.Ignore($"No se encontró MSBuild.exe en '{RutaMsBuildExe}' en esta máquina.");
        }

        [Test]
        public void EjecutarPaso_SecuenciaRealDeSit_CompilaLasTresCapasEnOrden()
        {
            using (var carpeta = new CarpetaTemporalDePrueba())
            {
                var destino = Path.Combine(carpeta.Ruta, "clon");
                var git = new GitClient(RutaGitExe, carpeta.Ruta);
                var ramaActual = ObtenerRamaActual(RutaRepoOrigenSit);

                var clonado = git.ClonarORama(RutaRepoOrigenSit, ramaActual, destino, "u", "p");
                Assert.That(clonado.IsSuccess, Is.True, clonado.Error);

                var rutaBase = Path.Combine(destino, "codigo", "SIT");
                var runner = new MsBuildRunner(RutaMsBuildExe);

                var pasos = new[]
                {
                    new PasoDeBuild(1, "Sit.BusinessEntities"),
                    new PasoDeBuild(2, "Sit.DataAccessLayer"),
                    new PasoDeBuild(3, "Sit.BusinessLayer"),
                };

                foreach (var paso in pasos)
                {
                    var resultado = runner.EjecutarPaso(paso, rutaBase);
                    Assert.That(resultado.IsSuccess, Is.True, $"Falló '{paso.CarpetaProyecto}': {resultado.Error}");
                }

                Assert.That(File.Exists(Path.Combine(rutaBase, "Sit.BusinessEntities", "bin", "Sit.BusinessEntities.dll")), Is.True);
                Assert.That(File.Exists(Path.Combine(rutaBase, "Sit.DataAccessLayer", "bin", "Sit.DataAccessLayer.dll")), Is.True);
                Assert.That(File.Exists(Path.Combine(rutaBase, "Sit.BusinessLayer", "bin", "Sit.BusinessLayer.dll")), Is.True);
            }
        }

        [Test]
        public void EjecutarPaso_CarpetaDeProyectoInexistente_Falla()
        {
            using (var carpeta = new CarpetaTemporalDePrueba())
            {
                var runner = new MsBuildRunner(RutaMsBuildExe);
                var paso = new PasoDeBuild(1, "Carpeta.Que.No.Existe");

                var resultado = runner.EjecutarPaso(paso, carpeta.Ruta);

                Assert.That(resultado.IsSuccess, Is.False);
                Assert.That(resultado.Error, Is.Not.Null.And.Not.Empty);
            }
        }

        private static string ObtenerRamaActual(string rutaRepo)
        {
            var info = new System.Diagnostics.ProcessStartInfo
            {
                FileName = RutaGitExe,
                Arguments = "rev-parse --abbrev-ref HEAD",
                WorkingDirectory = rutaRepo,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            };

            using (var proceso = System.Diagnostics.Process.Start(info))
            {
                var rama = proceso.StandardOutput.ReadToEnd().Trim();
                proceso.WaitForExit();
                return rama;
            }
        }
    }
}
