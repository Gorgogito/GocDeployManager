using System;
using System.Collections.Generic;
using System.Linq;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.Notifications.Tests
{
    internal sealed class NotificationOutboxRepositoryEnMemoria : INotificationOutboxRepository
    {
        private readonly List<NotificacionPendiente> _filas = new List<NotificacionPendiente>();
        private int _siguienteId = 1;

        public IReadOnlyList<NotificacionPendiente> Todas => _filas.AsReadOnly();

        public int Encolar(NotificacionPendiente notificacion)
        {
            var id = _siguienteId++;
            _filas.Add(NotificacionPendiente.Reconstruir(
                id, notificacion.DespliegueId, notificacion.Canal, notificacion.Destinatarios, notificacion.Asunto,
                notificacion.Contenido, EstadoNotificacion.Pendiente, 0, null, null));
            return id;
        }

        public IReadOnlyList<NotificacionPendiente> ObtenerPendientes(DateTime ahora) =>
            _filas.Where(f =>
                (f.Estado == EstadoNotificacion.Pendiente || f.Estado == EstadoNotificacion.Reintentando) &&
                (f.ProximoIntentoEn == null || f.ProximoIntentoEn <= ahora))
                .ToList().AsReadOnly();

        public void MarcarEnviada(int id) => Reemplazar(id, f => NotificacionPendiente.Reconstruir(
            f.Id, f.DespliegueId, f.Canal, f.Destinatarios, f.Asunto, f.Contenido, EstadoNotificacion.Enviado, f.IntentosRealizados, null, null));

        public void MarcarFallidaConReintento(int id, string mensajeError, DateTime proximoIntentoEn) => Reemplazar(id, f => NotificacionPendiente.Reconstruir(
            f.Id, f.DespliegueId, f.Canal, f.Destinatarios, f.Asunto, f.Contenido, EstadoNotificacion.Reintentando,
            f.IntentosRealizados + 1, proximoIntentoEn, mensajeError));

        public void MarcarFallidaDefinitiva(int id, string mensajeError) => Reemplazar(id, f => NotificacionPendiente.Reconstruir(
            f.Id, f.DespliegueId, f.Canal, f.Destinatarios, f.Asunto, f.Contenido, EstadoNotificacion.Fallido,
            f.IntentosRealizados + 1, null, mensajeError));

        private void Reemplazar(int id, Func<NotificacionPendiente, NotificacionPendiente> actualizar)
        {
            var indice = _filas.FindIndex(f => f.Id == id);
            if (indice >= 0)
                _filas[indice] = actualizar(_filas[indice]);
        }
    }
}
