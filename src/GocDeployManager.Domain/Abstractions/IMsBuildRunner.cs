using System;
using GocDeployManager.Common;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Domain.Abstractions
{
    /// <summary>
    /// Ejecuta un paso de build invocando MSBuild.exe exactamente como hoy
    /// (misma ruta fija, mismo target Clean,Build). El valor de retorno trae
    /// la salida combinada de MSBuild, útil para el log en tiempo real.
    /// </summary>
    public interface IMsBuildRunner
    {
        /// <param name="onLineaSalida">
        /// Invocado por cada línea de stdout/stderr de MSBuild.exe a medida
        /// que se produce (panel de salida en tiempo real). Opcional.
        /// </param>
        Result<string> EjecutarPaso(PasoDeBuild paso, string rutaBase, Action<string> onLineaSalida = null);
    }
}
