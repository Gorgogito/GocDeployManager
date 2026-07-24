using System.ComponentModel;

namespace GocDeployManager.UI.Configuracion
{
    /// <summary>
    /// DTO editable vía PropertyGridX (su editor de colecciones nativo permite
    /// agregar/quitar elementos de <see cref="AmbienteEditable.Sistemas"/> sin
    /// código adicional).
    /// </summary>
    public sealed class AmbienteSistemaEditable
    {
        [DisplayName("Código de sistema")]
        [Description("SIT, IDI, PROPAG, etc. — debe coincidir con Sistemas.json.")]
        public string Codigo { get; set; }

        [DisplayName("Nombre visible")]
        public string Nombre { get; set; }

        [DisplayName("Ruta destino")]
        [Description(@"Ej. \\Sdpeapp00009\Aplicaciones\SIT")]
        public string RutaDestino { get; set; }

        public override string ToString() =>
            string.IsNullOrWhiteSpace(Codigo) ? "(nuevo sistema)" : $"{Codigo} → {RutaDestino}";
    }
}
