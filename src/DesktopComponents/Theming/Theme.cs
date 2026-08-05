using System;
using System.Drawing;

namespace DesktopComponents.Theming
{
    public sealed class Theme
    {
        public string Nombre { get; }
        public Color Fondo { get; }
        public Color Superficie { get; }
        public Color SuperficieElevada { get; }
        public Color Borde { get; }
        public Color TextoPrimario { get; }
        public Color TextoSecundario { get; }
        public Color Acento { get; }
        public Color TextoEnAcento { get; }
        public Color Peligro { get; }
        public Color Advertencia { get; }
        public Color Exito { get; }
        public Color Sombra { get; }

        public Theme(
            string nombre,
            Color fondo,
            Color superficie,
            Color superficieElevada,
            Color borde,
            Color textoPrimario,
            Color textoSecundario,
            Color acento,
            Color textoEnAcento,
            Color peligro,
            Color advertencia,
            Color exito,
            Color sombra)
        {
            Nombre = nombre;
            Fondo = fondo;
            Superficie = superficie;
            SuperficieElevada = superficieElevada;
            Borde = borde;
            TextoPrimario = textoPrimario;
            TextoSecundario = textoSecundario;
            Acento = acento;
            TextoEnAcento = textoEnAcento;
            Peligro = peligro;
            Advertencia = advertencia;
            Exito = exito;
            Sombra = sombra;
        }

        public static Color Mezclar(Color baseColor, Color hacia, float factor)
        {
            factor = Math.Max(0f, Math.Min(1f, factor));
            var r = (int)(baseColor.R + (hacia.R - baseColor.R) * factor);
            var g = (int)(baseColor.G + (hacia.G - baseColor.G) * factor);
            var b = (int)(baseColor.B + (hacia.B - baseColor.B) * factor);
            return Color.FromArgb(r, g, b);
        }

        // MD3 Light — Blue 800 primary
        public static readonly Theme Claro = new Theme(
            "Claro",
            fondo:             Color.FromArgb(0xF4, 0xF4, 0xF8),
            superficie:        Color.FromArgb(0xFE, 0xFB, 0xFF),
            superficieElevada: Color.FromArgb(0xEA, 0xF0, 0xFF),
            borde:             Color.FromArgb(0x79, 0x74, 0x7E),
            textoPrimario:     Color.FromArgb(0x1C, 0x1B, 0x1F),
            textoSecundario:   Color.FromArgb(0x49, 0x45, 0x4F),
            acento:            Color.FromArgb(0x15, 0x65, 0xC0),
            textoEnAcento:     Color.FromArgb(0xFF, 0xFF, 0xFF),
            peligro:           Color.FromArgb(0xB3, 0x26, 0x1E),
            advertencia:       Color.FromArgb(0x7D, 0x57, 0x00),
            exito:             Color.FromArgb(0x00, 0x6E, 0x2C),
            sombra:            Color.FromArgb(0x00, 0x00, 0x00));

        // MD3 Dark — Blue 200 primary
        public static readonly Theme Oscuro = new Theme(
            "Oscuro",
            fondo:             Color.FromArgb(0x14, 0x12, 0x18),
            superficie:        Color.FromArgb(0x1C, 0x1B, 0x1F),
            superficieElevada: Color.FromArgb(0x28, 0x28, 0x31),
            borde:             Color.FromArgb(0x93, 0x8F, 0x99),
            textoPrimario:     Color.FromArgb(0xE6, 0xE1, 0xE5),
            textoSecundario:   Color.FromArgb(0xCA, 0xC4, 0xD0),
            acento:            Color.FromArgb(0xAD, 0xC6, 0xFF),
            textoEnAcento:     Color.FromArgb(0x00, 0x2E, 0x69),
            peligro:           Color.FromArgb(0xF2, 0xB8, 0xB5),
            advertencia:       Color.FromArgb(0xFF, 0xB9, 0x51),
            exito:             Color.FromArgb(0x78, 0xDC, 0x77),
            sombra:            Color.FromArgb(0x00, 0x00, 0x00));
    }
}
