using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using GocDeployManager.Common;
using GocDeployManager.Domain.Abstractions;

namespace GocDeployManager.Infrastructure.Git
{
    /// <summary>
    /// Clonado/actualización invocando git.exe como proceso externo (confirmado:
    /// Git está garantizado en toda máquina que corra la app). Las credenciales
    /// se pasan vía variables de entorno + GIT_ASKPASS, nunca como argumento de
    /// línea de comandos (visible para cualquier usuario de la máquina vía
    /// administrador de tareas).
    /// </summary>
    public sealed class GitClient : IGitClient
    {
        private readonly string _rutaGitExe;
        private readonly string _rutaTemporales;

        public GitClient(string rutaGitExe, string rutaTemporales)
        {
            _rutaGitExe = Guard.ContraNuloOVacio(rutaGitExe, nameof(rutaGitExe));
            _rutaTemporales = Guard.ContraNuloOVacio(rutaTemporales, nameof(rutaTemporales));
        }

        public Result ClonarORama(string repositorioUrl, string rama, string rutaDestino, string usuarioBitbucket, string contrasenaBitbucket, Action<string> onLineaSalida = null)
        {
            Guard.ContraNuloOVacio(repositorioUrl, nameof(repositorioUrl));
            Guard.ContraNuloOVacio(rama, nameof(rama));
            Guard.ContraNuloOVacio(rutaDestino, nameof(rutaDestino));
            Guard.ContraNuloOVacio(usuarioBitbucket, nameof(usuarioBitbucket));
            Guard.ContraNuloOVacio(contrasenaBitbucket, nameof(contrasenaBitbucket));

            try
            {
                var variablesEntorno = new System.Collections.Generic.Dictionary<string, string>
                {
                    ["GIT_ASKPASS"] = ObtenerOCrearScriptAskPass(),
                    ["GOC_GIT_USUARIO"] = usuarioBitbucket,
                    ["GOC_GIT_CONTRASENA"] = contrasenaBitbucket,
                    ["GIT_TERMINAL_PROMPT"] = "0",
                };

                if (Directory.Exists(Path.Combine(rutaDestino, ".git")))
                    return ActualizarRama(rama, rutaDestino, variablesEntorno, onLineaSalida);

                return ClonarDesdeCero(repositorioUrl, rama, rutaDestino, variablesEntorno, onLineaSalida);
            }
            catch (Exception ex)
            {
                return Result.Fail($"No se pudo clonar/actualizar '{rama}': {ex.Message}", ex);
            }
        }

        private Result ClonarDesdeCero(string repositorioUrl, string rama, string rutaDestino, System.Collections.Generic.IDictionary<string, string> variablesEntorno, Action<string> onLineaSalida)
        {
            var carpetaPadre = Path.GetDirectoryName(rutaDestino.TrimEnd(Path.DirectorySeparatorChar));
            if (!string.IsNullOrEmpty(carpetaPadre))
                Directory.CreateDirectory(carpetaPadre);

            var argumentos = $"clone --branch \"{rama}\" --single-branch \"{repositorioUrl}\" \"{rutaDestino}\"";
            var ejecucion = EjecutarGit(argumentos, workingDirectory: null, variablesEntorno, onLineaSalida);

            return ejecucion.ExitCode == 0
                ? Result.Ok()
                : Result.Fail($"git clone terminó con código {ejecucion.ExitCode}:{Environment.NewLine}{ejecucion.Salida}");
        }

        private Result ActualizarRama(string rama, string rutaDestino, System.Collections.Generic.IDictionary<string, string> variablesEntorno, Action<string> onLineaSalida)
        {
            var fetch = EjecutarGit($"fetch origin \"{rama}\"", rutaDestino, variablesEntorno, onLineaSalida);
            if (fetch.ExitCode != 0)
                return Result.Fail($"git fetch terminó con código {fetch.ExitCode}:{Environment.NewLine}{fetch.Salida}");

            var checkout = EjecutarGit($"checkout \"{rama}\"", rutaDestino, variablesEntorno, onLineaSalida);
            if (checkout.ExitCode != 0)
                return Result.Fail($"git checkout terminó con código {checkout.ExitCode}:{Environment.NewLine}{checkout.Salida}");

            var reset = EjecutarGit($"reset --hard \"origin/{rama}\"", rutaDestino, variablesEntorno, onLineaSalida);
            return reset.ExitCode == 0
                ? Result.Ok()
                : Result.Fail($"git reset --hard terminó con código {reset.ExitCode}:{Environment.NewLine}{reset.Salida}");
        }

        private (int ExitCode, string Salida) EjecutarGit(string argumentos, string workingDirectory, System.Collections.Generic.IDictionary<string, string> variablesEntorno, Action<string> onLineaSalida)
        {
            var infoProceso = new ProcessStartInfo
            {
                FileName = _rutaGitExe,
                Arguments = argumentos,
                WorkingDirectory = string.IsNullOrEmpty(workingDirectory) ? Environment.CurrentDirectory : workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            foreach (var variable in variablesEntorno)
                infoProceso.EnvironmentVariables[variable.Key] = variable.Value;

            using (var proceso = new Process { StartInfo = infoProceso })
            {
                var salida = new StringBuilder();
                DataReceivedEventHandler alRecibirLinea = (s, e) =>
                {
                    if (e.Data == null)
                        return;

                    salida.AppendLine(e.Data);
                    onLineaSalida?.Invoke(e.Data);
                };
                proceso.OutputDataReceived += alRecibirLinea;
                proceso.ErrorDataReceived += alRecibirLinea;

                proceso.Start();
                proceso.BeginOutputReadLine();
                proceso.BeginErrorReadLine();
                proceso.WaitForExit();

                return (proceso.ExitCode, salida.ToString());
            }
        }

        private string ObtenerOCrearScriptAskPass()
        {
            Directory.CreateDirectory(_rutaTemporales);
            var rutaScript = Path.Combine(_rutaTemporales, "goc-git-askpass.cmd");

            if (!File.Exists(rutaScript))
            {
                // Contenido estático: no incrusta la credencial, solo lee las
                // variables de entorno que EjecutarGit ya fijó para este proceso.
                var contenido =
                    "@echo off\r\n" +
                    "echo %1 | findstr /I \"Username\" >nul\r\n" +
                    "if %errorlevel%==0 (\r\n" +
                    "    echo %GOC_GIT_USUARIO%\r\n" +
                    ") else (\r\n" +
                    "    echo %GOC_GIT_CONTRASENA%\r\n" +
                    ")\r\n";

                File.WriteAllText(rutaScript, contenido);
            }

            return rutaScript;
        }
    }
}
