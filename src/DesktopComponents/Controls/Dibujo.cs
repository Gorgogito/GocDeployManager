using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DesktopComponents.Controls
{
    internal static class Dibujo
    {
        public static GraphicsPath RutaRedondeada(Rectangle rect, int radio)
        {
            var ruta = new GraphicsPath();

            if (radio <= 0)
            {
                ruta.AddRectangle(rect);
                return ruta;
            }

            // Evita que arcos adyacentes se superpongan cuando el radio supera
            // la mitad de la dimensión menor del rectángulo.
            radio = Math.Min(radio, Math.Min(rect.Width, rect.Height) / 2);
            var diametro = radio * 2;
            ruta.AddArc(rect.X, rect.Y, diametro, diametro, 180, 90);
            ruta.AddArc(rect.Right - diametro, rect.Y, diametro, diametro, 270, 90);
            ruta.AddArc(rect.Right - diametro, rect.Bottom - diametro, diametro, diametro, 0, 90);
            ruta.AddArc(rect.X, rect.Bottom - diametro, diametro, diametro, 90, 90);
            ruta.CloseFigure();

            return ruta;
        }
    }
}
