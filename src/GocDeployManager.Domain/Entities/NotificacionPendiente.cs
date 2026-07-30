using System;
using GocDeployManager.Common;

namespace GocDeployManager.Domain.Entities
{
    /// <summary>
    /// Una fila de la bandeja de salida durable de notificaciones (tabla
    /// NotificationOutbox) — sirve a la vez de cola de envío y de auditoría
    /// (análisis de notificaciones, sección 12). El contenido ya viene
    /// renderizado desde la plantilla; el trabajador en segundo plano solo
    /// tiene que entregarlo por el canal indicado.
    /// </summary>
    public sealed class NotificacionPendiente
    {
        public int Id { get; }

        /// <summary>Nulo para la notificación de "inicio de despliegue" —
        /// se publica antes de que exista la fila de historial.</summary>
        public int? DespliegueId { get; }
        public string Canal { get; }
        public string Destinatarios { get; }
        public string Asunto { get; }
        public string Contenido { get; }
        public EstadoNotificacion Estado { get; }
        public int IntentosRealizados { get; }
        public DateTime? ProximoIntentoEn { get; }
        public string MensajeError { get; }

        private NotificacionPendiente(
            int id, int? despliegueId, string canal, string destinatarios, string asunto, string contenido,
            EstadoNotificacion estado, int intentosRealizados, DateTime? proximoIntentoEn, string mensajeError)
        {
            Id = id;
            DespliegueId = despliegueId;
            Canal = canal;
            Destinatarios = destinatarios;
            Asunto = asunto;
            Contenido = contenido;
            Estado = estado;
            IntentosRealizados = intentosRealizados;
            ProximoIntentoEn = proximoIntentoEn;
            MensajeError = mensajeError;
        }

        /// <summary>Crea una nueva notificación pendiente de envío (Id=0: lo
        /// asigna la base de datos al encolarla).</summary>
        public static NotificacionPendiente Crear(int? despliegueId, string canal, string destinatarios, string asunto, string contenido)
        {
            return new NotificacionPendiente(
                0, despliegueId, Guard.ContraNuloOVacio(canal, nameof(canal)),
                Guard.ContraNuloOVacio(destinatarios, nameof(destinatarios)),
                asunto, Guard.ContraNuloOVacio(contenido, nameof(contenido)),
                EstadoNotificacion.Pendiente, 0, null, null);
        }

        /// <summary>Reconstruye una fila ya persistida — solo para uso de los
        /// repositorios de infraestructura.</summary>
        public static NotificacionPendiente Reconstruir(
            int id, int? despliegueId, string canal, string destinatarios, string asunto, string contenido,
            EstadoNotificacion estado, int intentosRealizados, DateTime? proximoIntentoEn, string mensajeError)
        {
            return new NotificacionPendiente(
                id, despliegueId, canal, destinatarios, asunto, contenido, estado, intentosRealizados, proximoIntentoEn, mensajeError);
        }
    }
}
