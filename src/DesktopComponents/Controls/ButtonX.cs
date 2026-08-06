using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DesktopComponents.Theming;

namespace DesktopComponents.Controls
{
    public sealed class ButtonX : Button, IThemedControl
    {
        private readonly int _radio;
        private readonly SuscripcionTema _suscripcionTema;
        private readonly Timer _timerRipple;
        private VarianteButtonX _variante = VarianteButtonX.Primario;
        private bool _mouseEncima;
        private bool _mousePresionado;
        private Point _puntoRipple;
        private float _radioRipple;
        private float _opacidadRipple;

        public VarianteButtonX Variante
        {
            get => _variante;
            set { _variante = value; Invalidate(); }
        }

        public ButtonX()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint | ControlStyles.ResizeRedraw,
                true);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor   = false;
            Font   = new Font("Segoe UI", 9.5f);
            Cursor = Cursors.Hand;
            _radio = LogicalToDeviceUnits(20);
            Height = LogicalToDeviceUnits(36);

            _timerRipple = new Timer { Interval = 16 };
            _timerRipple.Tick += (s, e) =>
            {
                _radioRipple    += LogicalToDeviceUnits(10);
                _opacidadRipple  = Math.Max(0f, _opacidadRipple - 0.04f);
                if (_opacidadRipple <= 0)
                    _timerRipple.Stop();
                Invalidate();
            };

            _suscripcionTema = new SuscripcionTema(AplicarTema);
        }

        public void AplicarTema(Theme tema) => Invalidate();

        // Suprime el cambio de estilo Win32 BS_DEFPUSHBUTTON que provoca
        // el borde negro rectangular cuando el botón es Form.AcceptButton.
        public override void NotifyDefault(bool value) { }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // Pinta el color del padre en el área rectangular completa para que
            // las esquinas fuera de la forma redondeada sean invisibles.
            var bgColor = Parent?.BackColor ?? ThemeManager.Actual.SuperficieElevada;
            using (var pincel = new SolidBrush(bgColor))
                pevent.Graphics.FillRectangle(pincel, ClientRectangle);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _suscripcionTema.Dispose();
                _timerRipple.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnMouseEnter(EventArgs e) { _mouseEncima    = true;  Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _mouseEncima    = false; _mousePresionado = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            _mousePresionado = true;
            _puntoRipple     = mevent.Location;
            _radioRipple     = 0;
            _opacidadRipple  = 0.28f;
            _timerRipple.Stop();
            _timerRipple.Start();
            Invalidate();
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent) { _mousePresionado = false; Invalidate(); base.OnMouseUp(mevent); }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var tema = ThemeManager.Actual;
            var g    = pevent.Graphics;
            GraficosX.PrepararAlta(g);

            // Variante tonal (Secundario): fondo azul muy suave + texto acento.
            // Variantes con fondo sólido (Primario, Peligro): texto blanco.
            var esTonal     = _variante == VarianteButtonX.Secundario;
            var colorBase   = ColorBaseSegunVariante(tema);
            var colorEstado = esTonal ? tema.Acento : tema.TextoEnAcento;

            var colorFondo = !Enabled
                ? Theme.Mezclar(colorBase, tema.Superficie, 0.55f)
                : _mousePresionado
                    ? Theme.Mezclar(colorBase, colorEstado, 0.12f)
                    : _mouseEncima
                        ? Theme.Mezclar(colorBase, colorEstado, 0.08f)
                        : colorBase;

            var rect = new Rectangle(1, 1, Width - 3, Height - 3);
            using (var ruta = Dibujo.RutaRedondeada(rect, _radio))
            {
                using (var pincel = new SolidBrush(colorFondo))
                    g.FillPath(pincel, ruta);

                // Borde sutil para variante tonal
                if (esTonal)
                {
                    using (var lapizBorde = new Pen(Theme.Mezclar(tema.Acento, tema.Superficie, 0.55f)))
                        g.DrawPath(lapizBorde, ruta);
                }

                if (_radioRipple > 0 && _opacidadRipple > 0)
                {
                    var estado      = g.Save();
                    g.SetClip(ruta);
                    var rippleColor = Color.FromArgb((int)(255 * _opacidadRipple), colorEstado);
                    using (var pincelRipple = new SolidBrush(rippleColor))
                        g.FillEllipse(pincelRipple,
                            _puntoRipple.X - _radioRipple,
                            _puntoRipple.Y - _radioRipple,
                            _radioRipple * 2, _radioRipple * 2);
                    g.Restore(estado);
                }
            }

            var colorTexto = !Enabled
                ? Theme.Mezclar(colorEstado, tema.Superficie, 0.62f)
                : colorEstado;

            TextRenderer.DrawText(g, Text, Font, ClientRectangle, colorTexto,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
        }

        private Color ColorBaseSegunVariante(Theme tema)
        {
            switch (_variante)
            {
                case VarianteButtonX.Peligro:
                    return tema.Peligro;
                case VarianteButtonX.Secundario:
                    // Tonal button: fondo azul muy tintado — el texto usará tema.Acento.
                    return Theme.Mezclar(tema.Acento, tema.Superficie, 0.88f);
                default:
                    return tema.Acento;
            }
        }
    }
}
