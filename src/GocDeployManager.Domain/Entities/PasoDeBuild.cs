using GocDeployManager.Common;

namespace GocDeployManager.Domain.Entities
{
    /// <summary>
    /// Un paso de compilación: "cd carpetaProyecto" + "msbuild /t:Clean,Build [parámetros]",
    /// tal como hacen hoy compilacion.bat de SIT/IDI/ProPag. Los parámetros MSBuild son
    /// opcionales porque ProPag ya los necesita distinto a SIT/IDI.
    /// </summary>
    public sealed class PasoDeBuild
    {
        public int Orden { get; }
        public string CarpetaProyecto { get; }
        public string ParametrosMsBuild { get; }

        public PasoDeBuild(int orden, string carpetaProyecto, string parametrosMsBuild = null)
        {
            Orden = Guard.Positivo(orden, nameof(orden));
            CarpetaProyecto = Guard.ContraNuloOVacio(carpetaProyecto, nameof(carpetaProyecto));
            ParametrosMsBuild = string.IsNullOrWhiteSpace(parametrosMsBuild) ? null : parametrosMsBuild;
        }
    }
}
