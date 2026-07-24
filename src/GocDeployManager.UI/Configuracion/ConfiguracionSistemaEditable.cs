using System.Collections.Generic;
using System.ComponentModel;

namespace GocDeployManager.UI.Configuracion
{
    public sealed class ConfiguracionSistemaEditable
    {
        [DisplayName("Código de sistema")]
        [Description("SIT, IDI, PROPAG, etc. — identificador de negocio, no el nombre del proyecto físico.")]
        public string Codigo { get; set; } = string.Empty;

        [DisplayName("Nombre visible")]
        public string Nombre { get; set; } = string.Empty;

        [DisplayName("URL del repositorio Bitbucket")]
        [Description("SIT, IDI y ProPag son 3 repositorios distintos, no uno compartido.")]
        public string RepositorioUrl { get; set; } = string.Empty;

        [DisplayName("Carpeta precompilada")]
        [Description(@"Ej. SITSolution\PrecompiledWeb\IN-SIT (difiere por sistema).")]
        public string CarpetaPrecompilada { get; set; } = string.Empty;

        [DisplayName("Pasos de build")]
        [Description("Secuencia de compilación: cd carpetaProyecto + MSBuild /t:Clean,Build [parámetros].")]
        public List<PasoDeBuildEditable> Pasos { get; set; } = new List<PasoDeBuildEditable>();

        public override string ToString() => string.IsNullOrWhiteSpace(Codigo) ? "(nuevo sistema)" : Codigo;
    }
}
