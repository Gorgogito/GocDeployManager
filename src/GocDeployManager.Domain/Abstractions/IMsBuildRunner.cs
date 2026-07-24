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
        Result<string> EjecutarPaso(PasoDeBuild paso, string rutaBase);
    }
}
