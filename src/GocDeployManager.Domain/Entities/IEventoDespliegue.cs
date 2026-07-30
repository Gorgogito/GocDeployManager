using System;
using System.Collections.Generic;

namespace GocDeployManager.Domain.Entities
{
    /// <summary>
    /// Hecho de dominio publicado por <c>DeploymentOrchestrator</c> — el
    /// orquestador no sabe quién lo escucha ni qué se hace con él (patrón
    /// Observer/Publisher, análisis de notificaciones sección 5).
    /// </summary>
    public interface IEventoDespliegue
    {
        string Goc { get; }

        string Ambiente { get; }

        IReadOnlyList<string> Sistemas { get; }

        string UsuarioAplicacion { get; }

        DateTime FechaHora { get; }

        /// <summary>Lo que el operador eligió en la sección "Notificaciones"
        /// de MainForm — el publicador no los interpreta, solo los reenvía
        /// para que el dispatcher de notificaciones resuelva destinatarios.</summary>
        IReadOnlyList<string> GruposDestinatariosSeleccionados { get; }

        IReadOnlyList<string> DestinatariosAdicionales { get; }

        /// <summary>Canales que el operador dejó marcados (ej. "Email",
        /// "Teams"). Vacío significa "sin restricción" — el dispatcher
        /// considera habilitados todos los canales activos.</summary>
        IReadOnlyList<string> CanalesSeleccionados { get; }
    }
}
