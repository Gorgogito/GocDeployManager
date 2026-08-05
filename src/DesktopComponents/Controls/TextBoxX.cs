using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DesktopComponents.Theming;

namespace DesktopComponents.Controls
{
    /// <summary>
    /// Envuelve un TextBox nativo sin borde dentro de un panel con borde/fondo
    /// pintados a mano — así el caret, la selección y el IME siguen siendo
    /// los del control nativo (reimplementar edición de texto desde cero es
    /// un riesgo innecesario), y solo el chrome visual es propio.
    /// </summary>
    [DesignerCategory("")]
    public sealed class TextBoxX : Panel, IThemedControl
    {
        private readonly TextBox _interno;
        private readonly int _radio;
        private readonly SuscripcionTema _suscripcionTema;
        private bool _tieneFoco;
        private string _textoMarcador;

        public string TextoMarcador
        {
            get => _textoMarcador;
            set { _textoMarcador = value; Invalidate(); }
        }

        public bool EsPassword
        {
            get => _interno.UseSystemPasswordChar;
            set => _interno.UseSystemPasswordChar = value;
        }

        public override string Text
        {
            get => _interno.Text;
            set => _interno.Text = value;
        }

        public event EventHandler TextoCambiado
        {
            add => _interno.TextChanged += value;
            remove => _interno.TextChanged -= value;
        }

        public TextBoxX()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint | ControlStyles.ResizeRedraw,
                true);

            _radio = LogicalToDeviceUnits(6);
            Height = LogicalToDeviceUnits(34);
            Padding = new Padding(LogicalToDeviceUnits(10), LogicalToDeviceUnits(7), LogicalToDeviceUnits(10), LogicalToDeviceUnits(7));

            _interno = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9.5f),
                Dock = DockStyle.Fill,
            };
            _interno.GotFocus += (s, e) => { _tieneFoco = true; Invalidate(); };
            _interno.LostFocus += (s, e) => { _tieneFoco = false; Invalidate(); };
            _interno.TextChanged += (s, e) => Invalidate();

            Controls.Add(_interno);

            _suscripcionTema = new SuscripcionTema(AplicarTema);
            AplicarTema(ThemeManager.Actual);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _suscripcionTema.Dispose();

            base.Dispose(disposing);
        }

        public void AplicarTema(Theme tema)
        {
            _interno.BackColor = tema.Superficie;
            _interno.ForeColor = tema.TextoPrimario;
            BackColor = tema.Superficie;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var tema = ThemeManager.Actual;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var ruta = Dibujo.RutaRedondeada(rect, _radio))
            using (var pincelFondo = new SolidBrush(tema.Superficie))
            using (var lapizBorde = new Pen(_tieneFoco ? tema.Acento : tema.Borde, _tieneFoco ? 2f : 1f))
            {
                e.Graphics.FillPath(pincelFondo, ruta);
                e.Graphics.DrawPath(lapizBorde, ruta);
            }

            if (string.IsNullOrEmpty(_interno.Text) && !string.IsNullOrEmpty(_textoMarcador) && !_tieneFoco)
            {
                TextRenderer.DrawText(
                    e.Graphics, _textoMarcador, _interno.Font,
                    new Rectangle(Padding.Left, 0, Width - Padding.Horizontal, Height),
                    tema.TextoSecundario,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            }
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            Invalidate();
        }
    }
}
