using System.Collections.Generic;
using GocDeployManager.Common;

namespace GocDeployManager.Domain.Abstractions
{
    /// <summary>
    /// Copia los binarios compilados hacia el ambiente, respetando patrones de
    /// exclusión (ej. "web.config") — automatiza el paso manual de hoy.
    /// </summary>
    public interface IFileDeployer
    {
        Result Copiar(string rutaOrigen, string rutaDestino, IReadOnlyList<string> patronesExclusion);
    }
}
