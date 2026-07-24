using System;

namespace DesktopComponents.Theming
{
    /// <summary>
    /// Tema activo de toda la aplicación. Un control se suscribe a
    /// <see cref="TemaCambiado"/> para repintarse cuando el usuario cambia
    /// entre Claro/Oscuro.
    /// </summary>
    public static class ThemeManager
    {
        private static Theme _actual = Theme.Claro;

        public static Theme Actual => _actual;

        public static event EventHandler TemaCambiado;

        public static void Establecer(Theme tema)
        {
            if (tema == null || ReferenceEquals(tema, _actual))
                return;

            _actual = tema;
            TemaCambiado?.Invoke(null, EventArgs.Empty);
        }
    }
}
