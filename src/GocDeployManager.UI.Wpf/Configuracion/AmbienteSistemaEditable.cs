using System.ComponentModel;

namespace GocDeployManager.UI.Configuracion
{
    public sealed class AmbienteSistemaEditable
    {
        [DisplayName("Código de sistema")]
        public string Codigo { get; set; }

        [DisplayName("Nombre visible")]
        public string Nombre { get; set; }

        [DisplayName("Ruta destino")]
        public string RutaDestino { get; set; }
    }
}
