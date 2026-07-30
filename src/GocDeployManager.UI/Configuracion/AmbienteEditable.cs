using System.Collections.Generic;
using System.ComponentModel;

namespace GocDeployManager.UI.Configuracion
{
    public sealed class AmbienteEditable
    {
        [DisplayName("Nombre del ambiente")]
        public string Nombre { get; set; } = string.Empty;

        [DisplayName("Sistemas")]
        [Description("Un ambiente puede definir uno o varios sistemas (SIT, IDI, ProPag...).")]
        public List<AmbienteSistemaEditable> Sistemas { get; set; } = new List<AmbienteSistemaEditable>();

        [DisplayName("Notificaciones habilitadas")]
        [Description("Si está desmarcado, este ambiente no notifica sus despliegues (ej. Desarrollo).")]
        public bool NotificacionesHabilitadas { get; set; } = true;
    }
}
