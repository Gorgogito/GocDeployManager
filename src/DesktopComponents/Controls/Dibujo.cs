using System.Drawing;
using System.Drawing.Drawing2D;

namespace DesktopComponents.Controls
{
    internal static class Dibujo
    {
        /// <summary>
        /// Degradado vertical sutil de 2 tonos — el mismo recurso que usan
        /// los controles de DevComponents/DotNetBar para dar sensación de
        /// volumen en vez de un relleno plano. <paramref name="rect"/> con
        /// ancho o alto 0 se ajusta a 1px (LinearGradientBrush no acepta un
        /// rectángulo vacío).
        /// </summary>
        public static LinearGradientBrush PincelDegradado(Rectangle rect, Color desde, Color hacia, float angulo = 90f)
        {
            var rectAjustado = rect;
            if (rectAjustado.Width <= 0) rectAjustado.Width = 1;
            if (rectAjustado.Height <= 0) rectAjustado.Height = 1;

            return new LinearGradientBrush(rectAjustado, desde, hacia, angulo);
        }

        public static GraphicsPath RutaRedondeada(Rectangle rect, int radio)
        {
            var ruta = new GraphicsPath();

            if (radio <= 0)
            {
                ruta.AddRectangle(rect);
                return ruta;
            }

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
