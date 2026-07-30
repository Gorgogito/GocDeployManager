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
            // .Trim() saca espacios/saltos de línea al principio y al final —
            // un problema real: pegar la ruta desde otro lado (Excel, un
            // correo, "Copiar como ruta de acceso") puede dejar un espacio o
            // salto de línea invisible que Path.Combine rechaza como
            // "carácter no válido" recién al momento de desplegar.
            CarpetaPrecompilada = Guard.ContraNuloOVacio(carpetaPrecompilada?.Trim(), nameof(carpetaPrecompilada));
            SecuenciaDeBuild = Guard.ContraNulo(secuenciaDeBuild, nameof(secuenciaDeBuild));
        }
    }
}
