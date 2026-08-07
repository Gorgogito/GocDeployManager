using System.Collections.Generic;
using System.ComponentModel;

namespace GocDeployManager.UI.Configuracion
{
    public sealed class AmbienteEditable
    {
        [DisplayName("Nombre del ambiente")]
        public string Nombre { get; set; } = string.Empty;

        [DisplayName("Sistemas")]
        public List<AmbienteSistemaEditable> Sistemas { get; set; } = new List<AmbienteSistemaEditable>();
    }
}
