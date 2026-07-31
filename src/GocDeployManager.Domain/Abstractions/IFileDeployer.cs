using System;
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
        /// <param name="onArchivo">
        /// Invocado por cada archivo procesado (nombre, si se omitió por
        /// exclusión) — panel de salida en tiempo real. Opcional.
        /// </param>
        Result Copiar(string rutaOrigen, string rutaDestino, IReadOnlyList<string> patronesExclusion, Action<string, bool> onArchivo = null);
    }
}
