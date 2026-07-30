using System;
using System.Collections.Generic;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Domain.Abstractions
{
    /// <summary>
    /// Persistencia de la bandeja de salida durable de notificaciones (tabla
    /// NotificationOutbox). Cualquier instancia de la app, en cualquier
    /// laptop, puede drenar los pendientes de otra sesión que se cerró a
    /// mitad de un reintento (análisis de notificaciones, sección 12).
    /// </summary>
    public interface INotificationOutboxRepository
    {
        /// <summary>Encola una notificación nueva (Estado=Pendiente) y
        /// devuelve el Id generado.</summary>
        int Encolar(NotificacionPendiente notificacion);

        /// <summary>Filas listas para intentarse: Estado en (Pendiente,
        /// Reintentando) y sin backoff pendiente (ProximoIntentoEn nulo o ya
        /// vencido).</summary>
        IReadOnlyList<NotificacionPendiente> ObtenerPendientes(DateTime ahora);

        void MarcarEnviada(int id);

        void MarcarFallidaConReintento(int id, string mensajeError, DateTime proximoIntentoEn);

        void MarcarFallidaDefinitiva(int id, string mensajeError);
    }
}
