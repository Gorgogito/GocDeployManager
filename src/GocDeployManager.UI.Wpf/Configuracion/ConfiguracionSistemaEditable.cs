using System.Collections.Generic;
using System.ComponentModel;

namespace GocDeployManager.UI.Configuracion
{
    public sealed class ConfiguracionSistemaEditable
    {
        [DisplayName("Código de sistema")]
        public string Codigo { get; set; } = string.Empty;

        [DisplayName("Nombre visible")]
        public string Nombre { get; set; } = string.Empty;

        [DisplayName("URL del repositorio Bitbucket")]
        public string RepositorioUrl { get; set; } = string.Empty;

        [DisplayName("Carpeta precompilada")]
        public string CarpetaPrecompilada { get; set; } = string.Empty;

        [DisplayName("Pasos de build")]
        public List<PasoDeBuildEditable> Pasos { get; set; } = new List<PasoDeBuildEditable>();

        public override string ToString() =>
            string.IsNullOrWhiteSpace(Codigo) ? "(nuevo sistema)" : Codigo;
    }
}
