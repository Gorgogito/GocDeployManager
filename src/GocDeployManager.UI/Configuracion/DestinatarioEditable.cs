using System.ComponentModel;

namespace GocDeployManager.UI.Configuracion
{
    public sealed class DestinatarioEditable
    {
        [DisplayName("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [DisplayName("Correo electrónico")]
        public string CorreoElectronico { get; set; } = string.Empty;

        public override string ToString() => string.IsNullOrWhiteSpace(Nombre) ? "(nuevo)" : Nombre;
    }
}
