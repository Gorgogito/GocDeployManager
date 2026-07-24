using System;

namespace DesktopComponents.Theming
{
    /// <summary>
    /// Suscribe un control a <see cref="ThemeManager.TemaCambiado"/> y garantiza
    /// que se desuscriba al hacer Dispose. Sin esto, un control ya desechado
    /// sigue referenciado por el evento estático: el siguiente cambio de tema
    /// puede lanzar una excepción sobre un control con el handle ya destruido,
    /// además de fugar memoria indefinidamente.
    /// </summary>
    public sealed class SuscripcionTema : IDisposable
    {
        private readonly EventHandler _manejador;
        private bool _desechado;

        public SuscripcionTema(Action<Theme> aplicarTema)
        {
            _manejador = (s, e) => aplicarTema(ThemeManager.Actual);
            ThemeManager.TemaCambiado += _manejador;
        }

        public void Dispose()
        {
            if (_desechado)
                return;

            _desechado = true;
            ThemeManager.TemaCambiado -= _manejador;
        }
    }
}
