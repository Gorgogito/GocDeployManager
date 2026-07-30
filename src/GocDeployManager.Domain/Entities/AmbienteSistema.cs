using GocDeployManager.Common;

namespace GocDeployManager.Domain.Entities
{
    /// <summary>
    /// Ruta de destino de un sistema dentro de un ambiente concreto.
    /// Un ambiente puede definir uno o varios de estos (confirmado: hay ambientes
    /// de un solo sistema).
    /// </summary>
    public sealed class AmbienteSistema
    {
        public Sistema Sistema { get; }
        public string RutaDestino { get; }

        public AmbienteSistema(Sistema sistema, string rutaDestino)
        {
            Sistema = Guard.ContraNulo(sistema, nameof(sistema));
            // .Trim() saca espacios/saltos de línea al principio y al final —
            // un problema real: pegar la ruta desde otro lado (Excel, un
            // correo, "Copiar como ruta de acceso") puede dejar un espacio o
            // salto de línea invisible que Path.Combine rechaza como
            // "carácter no válido" recién al momento de desplegar.
            RutaDestino = Guard.ContraNuloOVacio(rutaDestino?.Trim(), nameof(rutaDestino));
        }
    }
}
