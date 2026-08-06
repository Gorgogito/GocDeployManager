using System.Drawing;
using System.Drawing.Drawing2D;

namespace DesktopComponents.Controls
{
    internal static class GraficosX
    {
        /// <summary>
        /// Configura el contexto GDI+ para renderizado de alta calidad:
        /// antialiasing vectorial, alineación de píxel precisa y composición
        /// de alta calidad. Llamar al inicio de cada OnPaint de UserPaint.
        /// </summary>
        public static void PrepararAlta(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
        }
    }
}
