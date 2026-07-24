using GocDeployManager.Common;

namespace GocDeployManager.Domain.Abstractions
{
    /// <summary>
    /// Clonado/actualización de una rama vía git.exe como proceso externo
    /// (confirmado: Git está garantizado en toda máquina que corra la app).
    /// </summary>
    public interface IGitClient
    {
        Result ClonarORama(
            string repositorioUrl,
            string rama,
            string rutaDestino,
            string usuarioBitbucket,
            string contrasenaBitbucket);
    }
}
