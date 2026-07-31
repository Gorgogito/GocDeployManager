using System;
using GocDeployManager.Common;

namespace GocDeployManager.Domain.Abstractions
{
    /// <summary>
    /// Clonado/actualización de una rama vía git.exe como proceso externo
    /// (confirmado: Git está garantizado en toda máquina que corra la app).
    /// </summary>
    public interface IGitClient
    {
        /// <param name="onLineaSalida">
        /// Invocado por cada línea de stdout/stderr de git.exe a medida que se
        /// produce (panel de salida en tiempo real). Opcional — no cambia qué
        /// devuelve el método, solo permite observar el progreso en vivo.
        /// </param>
        Result ClonarORama(
            string repositorioUrl,
            string rama,
            string rutaDestino,
            string usuarioBitbucket,
            string contrasenaBitbucket,
            Action<string> onLineaSalida = null);
    }
}
