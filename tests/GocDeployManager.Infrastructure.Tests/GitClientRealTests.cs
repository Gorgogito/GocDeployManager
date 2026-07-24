using System.Diagnostics;
using System.IO;
using GocDeployManager.Infrastructure.Git;
using NUnit.Framework;

namespace GocDeployManager.Infrastructure.Tests
{
    /// <summary>
    /// Pruebas contra git.exe real y el repo real de sit-integra-web — de solo
    /// lectura sobre el origen: clona su historial ya commiteado, nunca toca
    /// su working tree. [Explicit] hace que "dotnet test" normal las omita
    /// (Category por sí sola no alcanza: sin filtro, NUnit corre todo lo que
    /// no sea Explicit); correrlas a propósito con
    /// "dotnet test --filter TestCategory=Real".
    /// </summary>
    [TestFixture]
    [Category("Real")]
    [Explicit("Depende de rutas reales de esta máquina y de git.exe; correr a propósito con --filter TestCategory=Real.")]
    public sealed class GitClientRealTests
    {
        private const string RutaRepoOrigenSit = @"D:\Desarrollo\sit-integra-web";
        private const string RutaGitExe = "git.exe";

        [SetUp]
        public void VerificarPrerequisitos()
        {
            if (!Directory.Exists(Path.Combine(RutaRepoOrigenSit, ".git")))
                Assert.Ignore($"No se encontró el repo real en '{RutaRepoOrigenSit}' en esta máquina.");
        }

        [Test]
        public void ClonarORama_RepoRealSitIntegraWeb_ClonaLaRamaActual()
        {
            var ramaActual = ObtenerRamaActual(RutaRepoOrigenSit);

            using (var carpeta = new CarpetaTemporalDePrueba())
            {
                var destino = Path.Combine(carpeta.Ruta, "clon");
                var git = new GitClient(RutaGitExe, carpeta.Ruta);

                var resultado = git.ClonarORama(RutaRepoOrigenSit, ramaActual, destino, "usuario-no-usado", "contrasena-no-usada");

                Assert.That(resultado.IsSuccess, Is.True, resultado.Error);
                Assert.That(Directory.Exists(Path.Combine(destino, ".git")), Is.True);
                Assert.That(File.Exists(Path.Combine(destino, "codigo", "SIT", "compilacion.bat")), Is.True);
            }
        }

        [Test]
        public void ClonarORama_DestinoYaClonado_ActualizaConFetchCheckoutReset()
        {
            var ramaActual = ObtenerRamaActual(RutaRepoOrigenSit);

            using (var carpeta = new CarpetaTemporalDePrueba())
            {
                var destino = Path.Combine(carpeta.Ruta, "clon");
                var git = new GitClient(RutaGitExe, carpeta.Ruta);

                var primerClonado = git.ClonarORama(RutaRepoOrigenSit, ramaActual, destino, "u", "p");
                Assert.That(primerClonado.IsSuccess, Is.True, primerClonado.Error);

                // Segunda llamada: el destino ya tiene .git -> toma la rama de
                // actualización (fetch + checkout + reset --hard), no el clonado desde cero.
                var actualizacion = git.ClonarORama(RutaRepoOrigenSit, ramaActual, destino, "u", "p");

                Assert.That(actualizacion.IsSuccess, Is.True, actualizacion.Error);
            }
        }

        [Test]
        public void ClonarORama_RamaInexistente_Falla()
        {
            using (var carpeta = new CarpetaTemporalDePrueba())
            {
                var destino = Path.Combine(carpeta.Ruta, "clon");
                var git = new GitClient(RutaGitExe, carpeta.Ruta);

                var resultado = git.ClonarORama(RutaRepoOrigenSit, "rama-que-no-existe-jamas-en-este-repo", destino, "u", "p");

                Assert.That(resultado.IsSuccess, Is.False);
                Assert.That(resultado.Error, Is.Not.Null.And.Not.Empty);
            }
        }

        [Test]
        public void ScriptAskPass_GeneradoPorClonarORama_DevuelveUsuarioYContrasenaReales()
        {
            using (var carpeta = new CarpetaTemporalDePrueba())
            {
                var destino = Path.Combine(carpeta.Ruta, "clon");
                var git = new GitClient(RutaGitExe, carpeta.Ruta);

                // Cualquier llamada a ClonarORama genera el script antes de
                // decidir clonado-desde-cero vs actualización.
                git.ClonarORama(RutaRepoOrigenSit, ObtenerRamaActual(RutaRepoOrigenSit), destino, "usuario.real", "contrasena.real");

                var rutaScript = Path.Combine(carpeta.Ruta, "goc-git-askpass.cmd");
                Assert.That(File.Exists(rutaScript), Is.True);

                var usuario = EjecutarScriptAskPass(rutaScript, "Username for 'https://bitbucket.org/x':", "usuario.real", "contrasena.real");
                var contrasena = EjecutarScriptAskPass(rutaScript, "Password for 'https://bitbucket.org/x':", "usuario.real", "contrasena.real");

                Assert.That(usuario, Is.EqualTo("usuario.real"));
                Assert.That(contrasena, Is.EqualTo("contrasena.real"));
            }
        }

        private static string EjecutarScriptAskPass(string rutaScript, string prompt, string usuario, string contrasena)
        {
            var info = new ProcessStartInfo
            {
                FileName = rutaScript,
                Arguments = $"\"{prompt}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            };
            info.EnvironmentVariables["GOC_GIT_USUARIO"] = usuario;
            info.EnvironmentVariables["GOC_GIT_CONTRASENA"] = contrasena;

            using (var proceso = Process.Start(info))
            {
                var salida = proceso.StandardOutput.ReadToEnd().Trim();
                proceso.WaitForExit();
                return salida;
            }
        }

        private static string ObtenerRamaActual(string rutaRepo)
        {
            var info = new ProcessStartInfo
            {
                FileName = RutaGitExe,
                Arguments = "rev-parse --abbrev-ref HEAD",
                WorkingDirectory = rutaRepo,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            };

            using (var proceso = Process.Start(info))
            {
                var rama = proceso.StandardOutput.ReadToEnd().Trim();
                proceso.WaitForExit();
                return rama;
            }
        }
    }
}
