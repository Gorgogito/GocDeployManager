using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GocDeployManager.Common;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Notifications.Abstractions;

namespace GocDeployManager.Notifications
{
    /// <summary>
    /// Drena la bandeja de salida durable en segundo plano — dentro de
    /// cualquier instancia de la app que esté abierta, no necesariamente la
    /// que originó la notificación (análisis de notificaciones, sección 12).
    /// Reintenta con backoff creciente; tras agotar los reintentos, marca la
    /// fila como fallida definitiva sin lanzar excepciones hacia el resto de
    /// la aplicación.
    /// </summary>
    public sealed class NotificationOutboxWorker : IDisposable
    {
        private static readonly int[] BackoffMinutos = { 1, 5, 15, 60 };

        private readonly INotificationOutboxRepository _outbox;
        private readonly IReadOnlyList<ICanalNotificacion> _canales;
        private readonly IAppLogger _logger;
        private readonly TimeSpan _intervalo;
        private Timer _temporizador;
        private int _drenando;

        public NotificationOutboxWorker(
            INotificationOutboxRepository outbox,
            IEnumerable<ICanalNotificacion> canales,
            IAppLogger logger,
            TimeSpan? intervalo = null)
        {
            _outbox = Guard.ContraNulo(outbox, nameof(outbox));
            _canales = Guard.ContraNulo(canales, nameof(canales)).ToList().AsReadOnly();
            _logger = Guard.ContraNulo(logger, nameof(logger));
            _intervalo = intervalo ?? TimeSpan.FromSeconds(30);
        }

        /// <summary>Arranca el drenaje periódico. Idempotente: llamarlo dos
        /// veces no crea dos temporizadores.</summary>
        public void Iniciar()
        {
            if (_temporizador != null)
                return;

            _temporizador = new Timer(_ => _ = DrenarAsync(), null, TimeSpan.Zero, _intervalo);
        }

        public async Task DrenarAsync()
        {
            if (Interlocked.Exchange(ref _drenando, 1) == 1)
                return; // ya hay un drenaje en curso en este proceso

            try
            {
                var pendientes = _outbox.ObtenerPendientes(DateTime.Now);

                foreach (var notificacion in pendientes)
                {
                    var canal = _canales.FirstOrDefault(c => c.Nombre == notificacion.Canal);
                    if (canal == null)
                    {
                        _logger.Warn($"No hay ningún canal activo llamado '{notificacion.Canal}' — se omite la notificación Id={notificacion.Id}.");
                        continue;
                    }

                    var resultado = await canal.EnviarAsync(notificacion).ConfigureAwait(false);

                    if (resultado.IsSuccess)
                    {
                        _outbox.MarcarEnviada(notificacion.Id);
                    }
                    else if (notificacion.IntentosRealizados + 1 >= BackoffMinutos.Length)
                    {
                        _outbox.MarcarFallidaDefinitiva(notificacion.Id, resultado.Error);
                        _logger.Error($"Notificación Id={notificacion.Id} (canal={notificacion.Canal}) falló definitivamente tras {notificacion.IntentosRealizados + 1} intentos: {resultado.Error}");
                    }
                    else
                    {
                        var minutos = BackoffMinutos[notificacion.IntentosRealizados];
                        _outbox.MarcarFallidaConReintento(notificacion.Id, resultado.Error, DateTime.Now.AddMinutes(minutos));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error inesperado al drenar la bandeja de salida de notificaciones.", ex);
            }
            finally
            {
                Interlocked.Exchange(ref _drenando, 0);
            }
        }

        public void Dispose()
        {
            _temporizador?.Dispose();
        }
    }
}
