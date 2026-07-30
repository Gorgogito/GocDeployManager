using System;
using System.Collections.Generic;
using System.Linq;
using GocDeployManager.Common;
using GocDeployManager.Domain.Abstractions;
using GocDeployManager.Domain.Entities;
using GocDeployManager.Notifications.Abstractions;

namespace GocDeployManager.Notifications
{
    /// <summary>
    /// Se suscribe a <see cref="IPublicadorEventosDespliegue"/> (el orquestador
    /// no lo conoce por nombre, solo por la interfaz <see cref="IObservadorEventoDespliegue"/>).
    /// Resuelve destinatarios, renderiza la plantilla correspondiente y
    /// encola el resultado en la bandeja de salida durable — nunca envía
    /// nada directamente, eso lo hace <c>NotificationOutboxWorker</c>
    /// (análisis de notificaciones, secciones 5 y 12).
    /// </summary>
    public sealed class NotificationDispatcher : IObservadorEventoDespliegue
    {
        private static readonly IReadOnlyDictionary<EtapaDespliegue, string> NombresDeEtapa = new Dictionary<EtapaDespliegue, string>
        {
            [EtapaDespliegue.ResolucionConfiguracion] = "Resolución de configuración",
            [EtapaDespliegue.Clonado] = "Clonado",
            [EtapaDespliegue.Compilacion] = "Compilación",
            [EtapaDespliegue.Copia] = "Copia",
        };

        private readonly IGrupoDestinatariosRepository _grupos;
        private readonly IConfiguracionCanalTeamsRepository _configuracionTeams;
        private readonly IPlantillaRepository _plantillas;
        private readonly IPlantillaRenderer _renderer;
        private readonly INotificationOutboxRepository _outbox;
        private readonly IAppLogger _logger;

        public NotificationDispatcher(
            IGrupoDestinatariosRepository grupos,
            IConfiguracionCanalTeamsRepository configuracionTeams,
            IPlantillaRepository plantillas,
            IPlantillaRenderer renderer,
            INotificationOutboxRepository outbox,
            IAppLogger logger)
        {
            _grupos = Guard.ContraNulo(grupos, nameof(grupos));
            _configuracionTeams = Guard.ContraNulo(configuracionTeams, nameof(configuracionTeams));
            _plantillas = Guard.ContraNulo(plantillas, nameof(plantillas));
            _renderer = Guard.ContraNulo(renderer, nameof(renderer));
            _outbox = Guard.ContraNulo(outbox, nameof(outbox));
            _logger = Guard.ContraNulo(logger, nameof(logger));
        }

        public void Manejar(IEventoDespliegue evento)
        {
            Guard.ContraNulo(evento, nameof(evento));

            var tipoEvento = ResolverTipoEvento(evento);
            var despliegueId = ObtenerDespliegueId(evento);

            EncolarCorreo(evento, tipoEvento, despliegueId);
            EncolarTeams(evento, tipoEvento, despliegueId);
        }

        private void EncolarCorreo(IEventoDespliegue evento, string tipoEvento, int? despliegueId)
        {
            if (!CanalHabilitado(evento, NombresDeCanal.Email))
                return;

            try
            {
                var destinatarios = ResolverDestinatariosCorreo(evento);
                if (destinatarios.Count == 0)
                    return;

                var plantilla = _plantillas.Obtener(NombresDeCanal.Email, tipoEvento);
                var contenido = _renderer.Renderizar(plantilla, ConstruirValores(evento, tipoEvento, paraJson: false));
                var asunto = ConstruirAsunto(tipoEvento, evento);

                _outbox.Encolar(NotificacionPendiente.Crear(
                    despliegueId, NombresDeCanal.Email, string.Join(",", destinatarios), asunto, contenido));
            }
            catch (Exception ex)
            {
                _logger.Error($"No se pudo encolar la notificación de correo para GOC={evento.Goc}.", ex);
            }
        }

        private void EncolarTeams(IEventoDespliegue evento, string tipoEvento, int? despliegueId)
        {
            if (!CanalHabilitado(evento, NombresDeCanal.Teams))
                return;

            try
            {
                var urlWebhook = ResolverWebhookTeams(evento);
                if (string.IsNullOrWhiteSpace(urlWebhook))
                    return;

                var plantilla = _plantillas.Obtener(NombresDeCanal.Teams, tipoEvento);
                var contenido = _renderer.Renderizar(plantilla, ConstruirValores(evento, tipoEvento, paraJson: true));

                _outbox.Encolar(NotificacionPendiente.Crear(
                    despliegueId, NombresDeCanal.Teams, urlWebhook, null, contenido));
            }
            catch (Exception ex)
            {
                _logger.Error($"No se pudo encolar la notificación de Teams para GOC={evento.Goc}.", ex);
            }
        }

        private static bool CanalHabilitado(IEventoDespliegue evento, string canal) =>
            evento.CanalesSeleccionados.Count == 0 ||
            evento.CanalesSeleccionados.Any(c => string.Equals(c, canal, StringComparison.OrdinalIgnoreCase));

        private List<string> ResolverDestinatariosCorreo(IEventoDespliegue evento)
        {
            var grupos = _grupos.ObtenerTodos();
            var direcciones = new List<string>();

            foreach (var nombreGrupo in evento.GruposDestinatariosSeleccionados)
            {
                var grupo = grupos.FirstOrDefault(g => string.Equals(g.Nombre, nombreGrupo, StringComparison.OrdinalIgnoreCase));
                if (grupo != null)
                    direcciones.AddRange(grupo.Miembros.Select(m => m.CorreoElectronico));
            }

            direcciones.AddRange(evento.DestinatariosAdicionales);

            return direcciones.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private string ResolverWebhookTeams(IEventoDespliegue evento)
        {
            var mapeos = _configuracionTeams.ObtenerTodos();
            if (mapeos.Count == 0)
                return null;

            MapeoCanalTeams mejor = null;
            var sistemas = evento.Sistemas.Count > 0 ? evento.Sistemas : new List<string> { null };

            foreach (var sistema in sistemas)
            {
                foreach (var mapeo in mapeos)
                {
                    if (mapeo.Aplica(sistema, evento.Ambiente) && (mejor == null || mapeo.Especificidad > mejor.Especificidad))
                        mejor = mapeo;
                }
            }

            return mejor?.UrlWebhook;
        }

        private static string ResolverTipoEvento(IEventoDespliegue evento)
        {
            if (evento is DespliegueIniciadoEvento) return TiposDeEvento.Iniciado;
            if (evento is DespliegueExitosoEvento) return TiposDeEvento.Exitoso;
            if (evento is DespliegueFallidoEvento) return TiposDeEvento.Fallido;

            throw new ArgumentException($"Tipo de evento de despliegue no reconocido: {evento.GetType().Name}");
        }

        private static int? ObtenerDespliegueId(IEventoDespliegue evento)
        {
            if (evento is DespliegueExitosoEvento exitoso) return exitoso.DespliegueId;
            if (evento is DespliegueFallidoEvento fallido) return fallido.DespliegueId;
            return null; // DespliegueIniciadoEvento: aún no existe la fila de historial.
        }

        private static string ConstruirAsunto(string tipoEvento, IEventoDespliegue evento)
        {
            switch (tipoEvento)
            {
                case TiposDeEvento.Iniciado: return $"[GocDeployManager] Despliegue iniciado — {evento.Goc}";
                case TiposDeEvento.Exitoso: return $"[GocDeployManager] Despliegue exitoso — {evento.Goc}";
                case TiposDeEvento.Fallido: return $"[GocDeployManager] Despliegue fallido — {evento.Goc}";
                default: return $"[GocDeployManager] {evento.Goc}";
            }
        }

        private static Dictionary<string, string> ConstruirValores(IEventoDespliegue evento, string tipoEvento, bool paraJson)
        {
            Func<string, string> escapar = paraJson ? Escapado.ParaJson : Escapado.ParaHtml;

            var valores = new Dictionary<string, string>
            {
                ["Goc"] = escapar(evento.Goc),
                ["Ambiente"] = escapar(evento.Ambiente),
                ["Sistemas"] = escapar(string.Join(", ", evento.Sistemas)),
                ["UsuarioAplicacion"] = escapar(evento.UsuarioAplicacion),
                ["FechaHora"] = escapar(evento.FechaHora.ToString("yyyy-MM-dd HH:mm:ss")),
            };

            switch (tipoEvento)
            {
                case TiposDeEvento.Exitoso when evento is DespliegueExitosoEvento exitoso:
                    valores["Rama"] = escapar(exitoso.Rama);
                    valores["FechaHoraInicio"] = escapar(exitoso.FechaHoraInicio.ToString("yyyy-MM-dd HH:mm:ss"));
                    valores["Duracion"] = escapar(exitoso.Duracion.ToString(@"hh\:mm\:ss"));
                    break;

                case TiposDeEvento.Fallido when evento is DespliegueFallidoEvento fallido:
                    valores["Etapa"] = escapar(NombresDeEtapa.TryGetValue(fallido.Etapa, out var nombre) ? nombre : fallido.Etapa.ToString());
                    valores["MensajeError"] = escapar(fallido.MensajeError);
                    break;
            }

            return valores;
        }
    }
}
