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
            RutaDestino = Guard.ContraNuloOVacio(rutaDestino, nameof(rutaDestino));
        }
    }
}
