namespace GocDeployManager.Domain.Entities
{
    /// <summary>
    /// Estado de un intento de notificación en la bandeja de salida durable
    /// (<see cref="NotificacionPendiente"/> / tabla NotificationOutbox).
    /// </summary>
    public enum EstadoNotificacion
    {
        Pendiente,
        Reintentando,
        Enviado,
        Fallido,
    }
}
