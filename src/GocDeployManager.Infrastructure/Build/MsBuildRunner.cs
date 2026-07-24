using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using GocDeployManager.Common;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Infrastructure.Build
{
    /// <summary>
    /// Ejecuta "cd carpetaProyecto &amp;&amp; msbuild /t:Clean,Build [parámetros]"
    /// exactamente como hacen hoy compilacion.bat de SIT/IDI/ProPag.
    /// </summary>
    public sealed class MsBuildRunner : IMsBuildRunner
    {
        private readonly string _rutaMsBuildExe;

        public MsBuildRunner(string rutaMsBuildExe)
        {
            _rutaMsBuildExe = Guard.ContraNuloOVacio(rutaMsBuildExe, nameof(rutaMsBuildExe));
        }

        public Result<string> EjecutarPaso(PasoDeBuild paso, string rutaBase)
        {
            Guard.ContraNulo(paso, nameof(paso));
            Guard.ContraNuloOVacio(rutaBase, nameof(rutaBase));

            var carpetaProyecto = Path.Combine(rutaBase, paso.CarpetaProyecto);
            if (!Directory.Exists(carpetaProyecto))
                return Result.Fail<string>($"No se encontró la carpeta del proyecto '{paso.CarpetaProyecto}' en '{rutaBase}'.");

            var argumentos = "/t:Clean,Build";
            if (!string.IsNullOrEmpty(paso.ParametrosMsBuild))
                argumentos += " " + paso.ParametrosMsBuild;

            try
            {
                var infoProceso = new ProcessStartInfo
                {
                    FileName = _rutaMsBuildExe,
                    Arguments = argumentos,
                    WorkingDirectory = carpetaProyecto,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };

                using (var proceso = new Process { StartInfo = infoProceso })
                {
                    var salida = new StringBuilder();
                    proceso.OutputDataReceived += (s, e) => { if (e.Data != null) salida.AppendLine(e.Data); };
                    proceso.ErrorDataReceived += (s, e) => { if (e.Data != null) salida.AppendLine(e.Data); };

                    proceso.Start();
                    proceso.BeginOutputReadLine();
                    proceso.BeginErrorReadLine();
                    proceso.WaitForExit();

                    var textoSalida = salida.ToString();

                    return proceso.ExitCode == 0
                        ? Result.Ok(textoSalida)
                        : Result.Fail<string>($"MSBuild terminó con código {proceso.ExitCode} en '{paso.CarpetaProyecto}'.{Environment.NewLine}{textoSalida}");
                }
            }
            catch (Exception ex)
            {
                return Result.Fail<string>($"No se pudo ejecutar MSBuild para '{paso.CarpetaProyecto}': {ex.Message}", ex);
            }
        }
    }
}
