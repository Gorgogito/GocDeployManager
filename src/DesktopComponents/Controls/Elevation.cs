using System.Drawing;
using System.Drawing.Drawing2D;

namespace DesktopComponents.Controls
{
    // Simula sombra MD3 con capas de relleno semitransparente offset hacia abajo.
    internal static class Elevation
    {
        public static void DibujarSombra(Graphics g, Rectangle rect, int radio, Color colorSombra, int capas = 3)
        {
            for (var i = capas; i >= 1; i--)
            {
                var alpha = (int)(60f * i / (capas * (capas + 1) / 2f));
                var c = Color.FromArgb(alpha, colorSombra.R, colorSombra.G, colorSombra.B);
                var r = new Rectangle(rect.X - 1, rect.Y + i, rect.Width + 2, rect.Height);
                using (var ruta = Dibujo.RutaRedondeada(r, radio))
                using (var pincel = new SolidBrush(c))
                    g.FillPath(pincel, ruta);
            }
        }
    }
}
