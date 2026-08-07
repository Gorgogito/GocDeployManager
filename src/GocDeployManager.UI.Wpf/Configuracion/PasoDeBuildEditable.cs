using System.ComponentModel;

namespace GocDeployManager.UI.Configuracion
{
    public sealed class PasoDeBuildEditable
    {
        [DisplayName("Orden")]
        public int Orden { get; set; } = 1;

        [DisplayName("Carpeta del proyecto")]
        public string CarpetaProyecto { get; set; } = string.Empty;

        [DisplayName("Parámetros MSBuild (opcional)")]
        public string ParametrosMsBuild { get; set; }

        public override string ToString() =>
            string.IsNullOrWhiteSpace(CarpetaProyecto) ? "(nuevo paso)" : $"{Orden}. {CarpetaProyecto}";
    }
}
