using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DesktopComponents.Theming;

namespace DesktopComponents.Controls
{
    /// <summary>
    /// Botón con bordes redondeados, estados hover/pressed pintados a mano y
    /// tres variantes (Primario/Secundario/Peligro). Hereda de <see cref="Button"/>,
    /// no de Control, para conservar gratis el comportamiento de clic por teclado,
    /// mnemonics y AcceptButton (Enter en un formulario de login).
    /// </summary>
    public sealed class ButtonX : Button, IThemedControl
    {
        private readonly int _radio;
        private readonly SuscripcionTema _suscripcionTema;
        private VarianteButtonX _variante = VarianteButtonX.Primario;
        private bool _mouseEncima;
        private bool _mousePresionado;

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
            Font = new Font("Segoe UI", 9.5f);
            Cursor = Cursors.Hand;
            _radio = LogicalToDeviceUnits(6);
            Height = LogicalToDeviceUnits(34);

            _suscripcionTema = new SuscripcionTema(AplicarTema);
        }

        public void AplicarTema(Theme tema) => Invalidate();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _suscripcionTema.Dispose();

            base.Dispose(disposing);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _mouseEncima = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _mouseEncima = false;
            _mousePresionado = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            _mousePresionado = true;
            Invalidate();
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            _mousePresionado = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var tema = ThemeManager.Actual;
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var colorBase = ColorBaseSegunVariante(tema);
            var colorFondo = !Enabled
                ? Theme.Mezclar(colorBase, Color.Gray, 0.4f)
                : _mousePresionado
                    ? Theme.Mezclar(colorBase, Color.Black, 0.15f)
                    : _mouseEncima
                        ? Theme.Mezclar(colorBase, Color.White, 0.12f)
                        : colorBase;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var ruta = Dibujo.RutaRedondeada(rect, _radio))
            using (var pincel = Dibujo.PincelDegradado(rect, Theme.Mezclar(colorFondo, Color.White, 0.18f), colorFondo))
            using (var lapizBorde = new Pen(Theme.Mezclar(colorFondo, Color.Black, 0.2f)))
            {
                g.FillPath(pincel, ruta);
                g.DrawPath(lapizBorde, ruta);
            }

            TextRenderer.DrawText(g, Text, Font, ClientRectangle, Color.White,
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
                    return tema.TextoSecundario;
                default:
                    return tema.Acento;
            }
        }
    }
}
