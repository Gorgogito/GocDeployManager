using System.Collections.Generic;
using System.ComponentModel;

namespace GocDeployManager.UI.Configuracion
{
    public sealed class GrupoDestinatariosEditable
    {
        [DisplayName("Nombre del grupo")]
        public string Nombre { get; set; } = string.Empty;

        [DisplayName("Miembros")]
        [Description("Personas que reciben las notificaciones enviadas a este grupo.")]
        public List<DestinatarioEditable> Miembros { get; set; } = new List<DestinatarioEditable>();
    }
}
