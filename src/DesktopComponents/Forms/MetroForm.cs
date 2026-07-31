using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DesktopComponents.Theming;

namespace DesktopComponents.Forms
{
    /// <summary>
    /// Chrome de ventana propio: sin borde nativo, barra de título pintada a
    /// mano con arrastre (vía WM_NCHITTEST) y botones minimizar/cerrar.
    /// Todas las pantallas de la aplicación heredan de esta clase.
    /// </summary>
    [DesignerCategory("")]
    public class MetroForm : Form, IThemedControl
    {
        private const int AltoBarraTituloLogico = 36;
        private const int CS_DROPSHADOW = 0x00020000;
        private const int WM_NCHITTEST = 0x84;
        private const int WM_LBUTTONDBLCLK = 0x203;
        private const int WM_GETMINMAXINFO = 0x24;
        private const int HTCLIENT = 1;
        private const int HTCAPTION = 2;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;

        private readonly SuscripcionTema _suscripcionTema;
        private Rectangle _rectBotonCerrar;
        private Rectangle _rectBotonMinimizar;
        private Rectangle _rectBotonMaximizar;
        private bool _cerrarHover;
        private bool _minimizarHover;
        private bool _maximizarHover;
        private bool _ventanaActiva = true;

        /// <summary>
        /// Si es <c>true</c>, la ventana agrega un botón maximizar/restaurar,
        /// admite arrastrar sus bordes para cambiar de tamaño, y doble clic en
        /// la barra de título maximiza/restaura — igual que una ventana nativa
        /// con <see cref="FormBorderStyle.Sizable"/>. Por defecto en
        /// <c>false</c>: el resto de las pantallas (diálogos, Login,
        /// Historial...) mantiene su tamaño fijo de siempre.
        /// </summary>
        public bool Redimensionable { get; set; }

        /// <summary>
        /// Agrega la sombra nativa de Windows/DWM alrededor de la ventana.
        /// Con <see cref="FormBorderStyle.None"/>, WinForms no la dibuja
        /// gratis como con un borde nativo — sin esto, dos ventanas
        /// superpuestas de este mismo tono son casi indistinguibles entre sí.
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                var parametros = base.CreateParams;
                parametros.ClassStyle |= CS_DROPSHADOW;
                return parametros;
            }
        }

        public MetroForm()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint | ControlStyles.ResizeRedraw,
                true);

            FormBorderStyle = FormBorderStyle.None;
            Font = new Font("Segoe UI", 9.5f);
            Padding = new Padding(0, LogicalToDeviceUnits(AltoBarraTituloLogico), 0, 0);

            // Como ninguna pantalla usa un .resx con Designer, WinForms no
            // toma sola el ícono de la aplicación (ApplicationIcon del
            // .csproj) para el título/la barra de tareas — hay que
            // asignárselo. Se hace una sola vez acá porque todas las
            // pantallas heredan de MetroForm.
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            _suscripcionTema = new SuscripcionTema(AplicarTema);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _suscripcionTema.Dispose();

            base.Dispose(disposing);
        }

        public void AplicarTema(Theme tema)
        {
            BackColor = tema.Fondo;
            Invalidate();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            AplicarTema(ThemeManager.Actual);
        }

        /// <summary>
        /// El borde se pinta con el color de acento (más grueso) cuando la
        /// ventana tiene el foco, y con el color neutro de siempre cuando no
        /// lo tiene — igual que el borde activo/inactivo de MetroForm en
        /// DevComponents. Ayuda a distinguir cuál ventana está "encima"
        /// cuando hay varias superpuestas.
        /// </summary>
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            _ventanaActiva = true;
            Invalidate();
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            _ventanaActiva = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var tema = ThemeManager.Actual;
            var altoBarra = LogicalToDeviceUnits(AltoBarraTituloLogico);

            var rectBarra = new Rectangle(0, 0, Math.Max(1, Width), altoBarra);
            using (var pincelBarra = new LinearGradientBrush(
                rectBarra, Theme.Mezclar(tema.Superficie, Color.White, 0.35f), tema.Superficie, LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(pincelBarra, rectBarra);
            }

            TextRenderer.DrawText(
                e.Graphics, Text, Font,
                new Rectangle(LogicalToDeviceUnits(12), 0, Width - LogicalToDeviceUnits(100), altoBarra),
                tema.TextoPrimario,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);

            var ladoBoton = LogicalToDeviceUnits(24);
            var margenSuperior = (altoBarra - ladoBoton) / 2;

            _rectBotonCerrar = new Rectangle(Width - ladoBoton - LogicalToDeviceUnits(10), margenSuperior, ladoBoton, ladoBoton);
            _rectBotonMaximizar = Redimensionable
                ? new Rectangle(_rectBotonCerrar.X - ladoBoton - LogicalToDeviceUnits(6), margenSuperior, ladoBoton, ladoBoton)
                : Rectangle.Empty;
            var origenMinimizar = Redimensionable ? _rectBotonMaximizar.X : _rectBotonCerrar.X;
            _rectBotonMinimizar = MinimizeBox
                ? new Rectangle(origenMinimizar - ladoBoton - LogicalToDeviceUnits(6), margenSuperior, ladoBoton, ladoBoton)
                : Rectangle.Empty;

            DibujarBotonVentana(e.Graphics, _rectBotonCerrar, "✕", _cerrarHover, tema.Peligro, tema);
            if (MinimizeBox)
                DibujarBotonVentana(e.Graphics, _rectBotonMinimizar, "–", _minimizarHover, tema.Acento, tema);
            if (Redimensionable)
                DibujarBotonVentana(e.Graphics, _rectBotonMaximizar, WindowState == FormWindowState.Maximized ? "❐" : "□", _maximizarHover, tema.Acento, tema);

            var colorBorde = _ventanaActiva ? tema.Acento : tema.Borde;
            var grosorBorde = _ventanaActiva ? 2 : 1;
            var mitad = grosorBorde / 2;
            using (var lapizBorde = new Pen(colorBorde, grosorBorde))
                e.Graphics.DrawRectangle(lapizBorde, new Rectangle(mitad, mitad, Width - 1 - mitad, Height - 1 - mitad));
        }

        private static void DibujarBotonVentana(Graphics g, Rectangle rect, string simbolo, bool hover, Color colorHover, Theme tema)
        {
            if (hover)
            {
                using (var pincel = new SolidBrush(colorHover))
                    g.FillEllipse(pincel, rect);
            }

            var colorTexto = hover ? Color.White : tema.TextoSecundario;
            using (var fuente = new Font("Segoe UI", 9f))
            {
                TextRenderer.DrawText(g, simbolo, fuente, rect, colorTexto,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var cerrarHoverAntes = _cerrarHover;
            var minimizarHoverAntes = _minimizarHover;
            var maximizarHoverAntes = _maximizarHover;

            _cerrarHover = _rectBotonCerrar.Contains(e.Location);
            _minimizarHover = _rectBotonMinimizar.Contains(e.Location);
            _maximizarHover = _rectBotonMaximizar.Contains(e.Location);

            if (_cerrarHover != cerrarHoverAntes || _minimizarHover != minimizarHoverAntes || _maximizarHover != maximizarHoverAntes)
                Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _cerrarHover = false;
            _minimizarHover = false;
            _maximizarHover = false;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (_rectBotonCerrar.Contains(e.Location))
                Close();
            else if (_rectBotonMinimizar.Contains(e.Location))
                WindowState = FormWindowState.Minimized;
            else if (Redimensionable && _rectBotonMaximizar.Contains(e.Location))
                AlternarMaximizado();
        }

        private void AlternarMaximizado()
        {
            WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PUNTO
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MINMAXINFO
        {
            public PUNTO ptReservado;
            public PUNTO ptTamanoMaximo;
            public PUNTO ptPosicionMaxima;
            public PUNTO ptTamanoMinimoSeguimiento;
            public PUNTO ptTamanoMaximoSeguimiento;
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_GETMINMAXINFO && Redimensionable)
            {
                // Sin esto, un formulario con FormBorderStyle.None se maximiza
                // contra el tamaño físico completo de la pantalla y termina
                // tapando la barra de tareas — hay que forzarlo contra el
                // área de trabajo real de la pantalla donde está la ventana.
                var pantalla = Screen.FromHandle(Handle);
                var areaTrabajo = pantalla.WorkingArea;
                var limitesPantalla = pantalla.Bounds;

                var info = (MINMAXINFO)Marshal.PtrToStructure(m.LParam, typeof(MINMAXINFO));
                info.ptPosicionMaxima.X = areaTrabajo.X - limitesPantalla.X;
                info.ptPosicionMaxima.Y = areaTrabajo.Y - limitesPantalla.Y;
                info.ptTamanoMaximo.X = areaTrabajo.Width;
                info.ptTamanoMaximo.Y = areaTrabajo.Height;
                info.ptTamanoMaximoSeguimiento.X = areaTrabajo.Width;
                info.ptTamanoMaximoSeguimiento.Y = areaTrabajo.Height;
                Marshal.StructureToPtr(info, m.LParam, true);
                return;
            }

            if (m.Msg == WM_LBUTTONDBLCLK && Redimensionable)
            {
                var punto = PointToClient(new Point(m.LParam.ToInt32() & 0xFFFF, (m.LParam.ToInt32() >> 16) & 0xFFFF));
                var altoBarraDoble = LogicalToDeviceUnits(AltoBarraTituloLogico);
                if (punto.Y < altoBarraDoble && !_rectBotonCerrar.Contains(punto) && !_rectBotonMinimizar.Contains(punto) && !_rectBotonMaximizar.Contains(punto))
                    AlternarMaximizado();
                return;
            }

            if (m.Msg != WM_NCHITTEST)
                return;

            if ((int)m.Result == HTCLIENT)
            {
                var punto = PointToClient(new Point(m.LParam.ToInt32()));
                var altoBarra = LogicalToDeviceUnits(AltoBarraTituloLogico);

                if (punto.Y < altoBarra && !_rectBotonCerrar.Contains(punto) && !_rectBotonMinimizar.Contains(punto) && !_rectBotonMaximizar.Contains(punto))
                {
                    m.Result = (IntPtr)HTCAPTION;
                    return;
                }
            }

            if (!Redimensionable || WindowState == FormWindowState.Maximized)
                return;

            var margen = LogicalToDeviceUnits(6);
            var puntoPantalla = new Point(m.LParam.ToInt32());
            var local = PointToClient(puntoPantalla);

            var enIzquierda = local.X <= margen;
            var enDerecha = local.X >= Width - margen;
            var enArriba = local.Y <= margen;
            var enAbajo = local.Y >= Height - margen;

            if (enIzquierda && enArriba) m.Result = (IntPtr)HTTOPLEFT;
            else if (enDerecha && enArriba) m.Result = (IntPtr)HTTOPRIGHT;
            else if (enIzquierda && enAbajo) m.Result = (IntPtr)HTBOTTOMLEFT;
            else if (enDerecha && enAbajo) m.Result = (IntPtr)HTBOTTOMRIGHT;
            else if (enIzquierda) m.Result = (IntPtr)HTLEFT;
            else if (enDerecha) m.Result = (IntPtr)HTRIGHT;
            else if (enArriba) m.Result = (IntPtr)HTTOP;
            else if (enAbajo) m.Result = (IntPtr)HTBOTTOM;
        }
    }
}
