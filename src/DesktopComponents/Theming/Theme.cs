using System;
using System.Drawing;

namespace DesktopComponents.Theming
{
    /// <summary>
    /// Paleta de un tema (Claro/Oscuro). Los mismos tonos usados en el documento
    /// de análisis, para que la documentación y la aplicación se vean como un
    /// mismo sistema visual.
    /// </summary>
    public sealed class Theme
    {
        public string Nombre { get; }
        public Color Fondo { get; }
        public Color Superficie { get; }
        public Color Borde { get; }
        public Color TextoPrimario { get; }
        public Color TextoSecundario { get; }
        public Color Acento { get; }
        public Color Peligro { get; }
        public Color Advertencia { get; }
        public Color Exito { get; }

        public Theme(
            string nombre,
            Color fondo,
            Color superficie,
            Color borde,
            Color textoPrimario,
            Color textoSecundario,
            Color acento,
            Color peligro,
            Color advertencia,
            Color exito)
        {
            Nombre = nombre;
            Fondo = fondo;
            Superficie = superficie;
            Borde = borde;
            TextoPrimario = textoPrimario;
            TextoSecundario = textoSecundario;
            Acento = acento;
            Peligro = peligro;
            Advertencia = advertencia;
            Exito = exito;
        }

        public static Color Mezclar(Color baseColor, Color hacia, float factor)
        {
            factor = Math.Max(0f, Math.Min(1f, factor));

            var r = (int)(baseColor.R + (hacia.R - baseColor.R) * factor);
            var g = (int)(baseColor.G + (hacia.G - baseColor.G) * factor);
            var b = (int)(baseColor.B + (hacia.B - baseColor.B) * factor);

            return Color.FromArgb(r, g, b);
        }

        public static readonly Theme Claro = new Theme(
            "Claro",
            fondo: Color.FromArgb(0xF4, 0xF5, 0xF7),
            superficie: Color.White,
            borde: Color.FromArgb(0xC3, 0xC9, 0xD1),
            textoPrimario: Color.FromArgb(0x1B, 0x21, 0x30),
            textoSecundario: Color.FromArgb(0x5B, 0x64, 0x72),
            acento: Color.FromArgb(0x2C, 0x6E, 0x8E),
            peligro: Color.FromArgb(0xB1, 0x43, 0x43),
            advertencia: Color.FromArgb(0xB8, 0x73, 0x2E),
            exito: Color.FromArgb(0x2E, 0x7D, 0x5B));

        public static readonly Theme Oscuro = new Theme(
            "Oscuro",
            fondo: Color.FromArgb(0x10, 0x14, 0x1C),
            superficie: Color.FromArgb(0x17, 0x1D, 0x28),
            borde: Color.FromArgb(0x2A, 0x32, 0x42),
            textoPrimario: Color.FromArgb(0xE7, 0xEA, 0xF0),
            textoSecundario: Color.FromArgb(0x9D, 0xA6, 0xB8),
            acento: Color.FromArgb(0x74, 0xBF, 0xD6),
            peligro: Color.FromArgb(0xE2, 0x8A, 0x8A),
            advertencia: Color.FromArgb(0xE0, 0xA0, 0x55),
            exito: Color.FromArgb(0x79, 0xCB, 0xA3));
    }
}
