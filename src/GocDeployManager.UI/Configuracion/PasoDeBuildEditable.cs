using System.ComponentModel;

namespace GocDeployManager.UI.Configuracion
{
    public sealed class PasoDeBuildEditable
    {
        [DisplayName("Orden")]
        public int Orden { get; set; } = 1;

        [DisplayName("Carpeta del proyecto")]
        [Description(@"Ej. Sit.BusinessEntities — la carpeta donde se ejecuta MSBuild.")]
        public string CarpetaProyecto { get; set; } = string.Empty;

        [DisplayName("Parámetros MSBuild (opcional)")]
        [Description(@"Ej. /p:Configuration=Debug;TargetFrameworkVersion=v4.0 — solo si el sistema los necesita.")]
        public string ParametrosMsBuild { get; set; }

        public override string ToString() =>
            string.IsNullOrWhiteSpace(CarpetaProyecto) ? "(nuevo paso)" : $"{Orden}. {CarpetaProyecto}";
    }
}
