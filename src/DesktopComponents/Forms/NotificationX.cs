using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DesktopComponents.Controls;
using DesktopComponents.Theming;

namespace DesktopComponents.Forms
{
    /// <summary>
    /// Aviso tipo "toast" (esquina inferior derecha, se cierra solo) para
    /// notificar el fin de un despliegue sin bloquear con un MessageBox modal
    /// (sección 9 del análisis). Uso: <c>NotificationX.Mostrar(titulo, mensaje, tipo)</c>.
    /// </summary>
    [DesignerCategory("")]
    public sealed class NotificationX : Form, IThemedControl
    {
        private static readonly List<NotificationX> Activas = new List<NotificationX>();

        private readonly Timer _timerAnimacion;
        private readonly Timer _timerAutoOcultar;
        private readonly SuscripcionTema _suscripcionTema;
        private readonly TipoNotificacion _tipo;
        private readonly string _titulo;
        private readonly string _mensaje;
        private bool _cerrando;

        private NotificationX(string titulo, string mensaje, TipoNotificacion tipo)
        {
            _titulo = titulo;
            _mensaje = mensaje;
            _tipo = tipo;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint | ControlStyles.ResizeRedraw,
                true);

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            Font = new Font("Segoe UI", 9.5f);
            Size = new Size(LogicalToDeviceUnits(320), LogicalToDeviceUnits(84));
            Opacity = 0;

            var area = Screen.PrimaryScreen.WorkingArea;
            var margen = LogicalToDeviceUnits(16);
            var separacion = LogicalToDeviceUnits(10);

            lock (Activas)
            {
                var offsetY = Activas.Sum(n => n.Height + separacion);
                Location = new Point(area.Right - Width - margen, area.Bottom - Height - margen - offsetY);
                Activas.Add(this);
            }

            _timerAnimacion = new Timer { Interval = 15 };
            _timerAnimacion.Tick += TimerAnimacion_Tick;

            _timerAutoOcultar = new Timer { Interval = 4000 };
            _timerAutoOcultar.Tick += (s, e) => { _timerAutoOcultar.Stop(); Cerrar(); };

            Click += (s, e) => Cerrar();

            _suscripcionTema = new SuscripcionTema(AplicarTema);
        }

        public static NotificationX Mostrar(string titulo, string mensaje, TipoNotificacion tipo = TipoNotificacion.Info)
        {
            var notificacion = new NotificationX(titulo, mensaje, tipo);
            notificacion.Show();
            notificacion._timerAnimacion.Start();
            notificacion._timerAutoOcultar.Start();
            return notificacion;
        }

        public void AplicarTema(Theme tema) => Invalidate();

        private void TimerAnimacion_Tick(object sender, EventArgs e)
        {
            if (_cerrando)
            {
                Opacity = Math.Max(0, Opacity - 0.12);
                if (Opacity <= 0)
                {
                    _timerAnimacion.Stop();
                    Close();
                }
            }
            else
            {
                Opacity = Math.Min(1, Opacity + 0.12);
                if (Opacity >= 1)
                    _timerAnimacion.Stop();
            }
        }

        private void Cerrar()
        {
            if (_cerrando)
                return;

            _cerrando = true;
            _timerAutoOcultar.Stop();
            _timerAnimacion.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            lock (Activas)
                Activas.Remove(this);

            base.OnFormClosed(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var tema = ThemeManager.Actual;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var ruta = Dibujo.RutaRedondeada(rect, LogicalToDeviceUnits(8)))
            using (var pincelFondo = new SolidBrush(tema.Superficie))
            using (var lapizBorde = new Pen(tema.Borde))
            {
                g.FillPath(pincelFondo, ruta);
                g.DrawPath(lapizBorde, ruta);
            }

            var barraAcento = new Rectangle(0, 0, LogicalToDeviceUnits(4), Height);
            using (var rutaBarra = Dibujo.RutaRedondeada(barraAcento, LogicalToDeviceUnits(2)))
            using (var pincelAcento = new SolidBrush(ColorSegunTipo(tema)))
                g.FillPath(pincelAcento, rutaBarra);

            var margen = LogicalToDeviceUnits(16);
            using (var fuenteTitulo = new Font(Font, FontStyle.Bold))
            {
                TextRenderer.DrawText(
                    g, _titulo, fuenteTitulo,
                    new Rectangle(margen, LogicalToDeviceUnits(12), Width - margen * 2, LogicalToDeviceUnits(20)),
                    tema.TextoPrimario,
                    TextFormatFlags.Left | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
            }

            TextRenderer.DrawText(
                g, _mensaje, Font,
                new Rectangle(margen, LogicalToDeviceUnits(36), Width - margen * 2, LogicalToDeviceUnits(40)),
                tema.TextoSecundario, TextFormatFlags.Left | TextFormatFlags.WordBreak);
        }

        private Color ColorSegunTipo(Theme tema)
        {
            switch (_tipo)
            {
                case TipoNotificacion.Exito:
                    return tema.Exito;
                case TipoNotificacion.Advertencia:
                    return tema.Advertencia;
                case TipoNotificacion.Error:
                    return tema.Peligro;
                default:
                    return tema.Acento;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timerAnimacion?.Dispose();
                _timerAutoOcultar?.Dispose();
                _suscripcionTema.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
