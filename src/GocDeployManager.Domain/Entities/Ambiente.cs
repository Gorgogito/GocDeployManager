using System.Collections.Generic;
using System.Linq;
using GocDeployManager.Common;

namespace GocDeployManager.Domain.Entities
{
    /// <summary>
    /// Un ambiente de despliegue (ej. "Desarrollo", "Testing", "Producción").
    /// No tiene columnas fijas "RutaSIT"/"RutaIDI": define una lista abierta de
    /// sistemas soportados, para acomodar ambientes de un solo sistema y la
    /// incorporación futura de ProPag.
    /// </summary>
    public sealed class Ambiente
    {
        public string Nombre { get; }
        public IReadOnlyList<AmbienteSistema> Sistemas { get; }

        /// <summary>
        /// Controla si este ambiente notifica el resultado de sus despliegues
        /// (análisis de notificaciones, sección 3) — p. ej. se desactiva en
        /// Desarrollo para no generar ruido. <c>true</c> por defecto.
        /// </summary>
        public bool NotificacionesHabilitadas { get; }

        public Ambiente(string nombre, IEnumerable<AmbienteSistema> sistemas, bool notificacionesHabilitadas = true)
        {
            Nombre = Guard.ContraNuloOVacio(nombre, nameof(nombre));

            var lista = Guard.ContraNulo(sistemas, nameof(sistemas)).ToList();
            if (lista.Count == 0)
                throw new System.ArgumentException("Un ambiente debe soportar al menos un sistema.", nameof(sistemas));

            Sistemas = lista.AsReadOnly();
            NotificacionesHabilitadas = notificacionesHabilitadas;
        }

        public bool SoportaSistema(Sistema sistema) => BuscarSistema(sistema) != null;

        public AmbienteSistema BuscarSistema(Sistema sistema) =>
            sistema == null ? null : Sistemas.FirstOrDefault(s => s.Sistema.Equals(sistema));
    }
}
