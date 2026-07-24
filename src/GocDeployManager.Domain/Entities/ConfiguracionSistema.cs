using GocDeployManager.Common;

namespace GocDeployManager.Domain.Entities
{
    /// <summary>
    /// Todo lo que la app necesita saber para compilar y desplegar un sistema:
    /// su propio repositorio Bitbucket (SIT, IDI y ProPag son 3 repos distintos,
    /// no uno compartido), la carpeta de salida precompilada (difiere por sistema:
    /// "SITSolution\PrecompiledWeb\IN-SIT" en SIT/ProPag vs "PrecompiledWeb\IN-IDI"
    /// en IDI) y su secuencia de build.
    /// </summary>
    public sealed class ConfiguracionSistema
    {
        public Sistema Sistema { get; }
        public string RepositorioUrl { get; }
        public string CarpetaPrecompilada { get; }
        public SecuenciaDeBuild SecuenciaDeBuild { get; }

        public ConfiguracionSistema(Sistema sistema, string repositorioUrl, string carpetaPrecompilada, SecuenciaDeBuild secuenciaDeBuild)
        {
            Sistema = Guard.ContraNulo(sistema, nameof(sistema));
            RepositorioUrl = Guard.ContraNuloOVacio(repositorioUrl, nameof(repositorioUrl));
            CarpetaPrecompilada = Guard.ContraNuloOVacio(carpetaPrecompilada, nameof(carpetaPrecompilada));
            SecuenciaDeBuild = Guard.ContraNulo(secuenciaDeBuild, nameof(secuenciaDeBuild));
        }
    }
}
