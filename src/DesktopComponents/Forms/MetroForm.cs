using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DesktopComponents.Controls;
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
        private const int WM_NCHITTEST      = 0x84;
        private const int WM_LBUTTONDBLCLK  = 0x203;
        private const int WM_GETMINMAXINFO  = 0x24;
        private const int HTCLIENT    = 1;
        private const int HTCAPTION   = 2;
        private const int HTLEFT      = 10;
        private const int HTRIGHT     = 11;
        private const int HTTOP       = 12;
        private const int HTTOPLEFT   = 13;
        private const int HTTOPRIGHT  = 14;
        private const int HTBOTTOM    = 15;
        private const int HTBOTTOMLEFT  = 16;
        private const int HTBOTTOMRIGHT = 17;
        private const int CS_DROPSHADOW = 0x20000;

        private readonly SuscripcionTema _suscripcionTema;
        private Rectangle _rectBotonCerrar;
        private Rectangle _rectBotonMinimizar;
        private Rectangle _rectBotonMaximizar;
        private bool _cerrarHover;
        private bool _minimizarHover;
        private bool _maximizarHover;
        private bool _estaActiva = true;

        /// <summary>
        /// Si es <c>true</c>, la ventana agrega un botón maximizar/restaurar,
        /// admite arrastrar sus bordes para cambiar de tamaño, y doble clic en
        /// la barra de título maximiza/restaura.
        /// </summary>
        public bool Redimensionable { get; set; }

        // Sombra nativa DWM para todas las ventanas con FormBorderStyle.None.
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                if (!DesignMode)
                    cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
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

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            _estaActiva = true;
            Invalidate();
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            _estaActiva = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var tema = ThemeManager.Actual;
            var g = e.Graphics;
            GraficosX.PrepararAlta(g);

            var altoBarra = LogicalToDeviceUnits(AltoBarraTituloLogico);

            // Título — se oscurece sutilmente cuando la ventana pierde el foco.
            var colorBarra = _estaActiva
                ? tema.SuperficieElevada
                : Theme.Mezclar(tema.SuperficieElevada, tema.Superficie, 0.45f);

            using (var pincelBarra = new SolidBrush(colorBarra))
                g.FillRectangle(pincelBarra, new Rectangle(0, 0, Width, altoBarra));

            var colorTitulo = _estaActiva ? tema.TextoPrimario : tema.TextoSecundario;
            TextRenderer.DrawText(
                g, Text, Font,
                new Rectangle(LogicalToDeviceUnits(12), 0, Width - LogicalToDeviceUnits(120), altoBarra),
                colorTitulo,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left |
                TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);

            var ladoBoton = LogicalToDeviceUnits(24);
            var margenSuperior = (altoBarra - ladoBoton) / 2;

            _rectBotonCerrar = new Rectangle(
                Width - ladoBoton - LogicalToDeviceUnits(10), margenSuperior, ladoBoton, ladoBoton);
            _rectBotonMaximizar = Redimensionable
                ? new Rectangle(_rectBotonCerrar.X - ladoBoton - LogicalToDeviceUnits(6), margenSuperior, ladoBoton, ladoBoton)
                : Rectangle.Empty;
            var origenMinimizar = Redimensionable ? _rectBotonMaximizar.X : _rectBotonCerrar.X;
            _rectBotonMinimizar = MinimizeBox
                ? new Rectangle(origenMinimizar - ladoBoton - LogicalToDeviceUnits(6), margenSuperior, ladoBoton, ladoBoton)
                : Rectangle.Empty;

            DibujarBotonVentana(g, _rectBotonCerrar,    "✕", _cerrarHover,    tema.Peligro, tema, _estaActiva);
            if (MinimizeBox)
                DibujarBotonVentana(g, _rectBotonMinimizar, "–", _minimizarHover, tema.Acento,  tema, _estaActiva);
            if (Redimensionable)
                DibujarBotonVentana(g, _rectBotonMaximizar,
                    WindowState == FormWindowState.Maximized ? "❐" : "□",
                    _maximizarHover, tema.Acento, tema, _estaActiva);

            // Borde — visible y consistente; más sutil en ventana inactiva.
            var colorBorde = _estaActiva
                ? tema.BordereReposo
                : Theme.Mezclar(tema.BordereReposo, tema.Superficie, 0.4f);
            using (var lapizBorde = new Pen(colorBorde))
                g.DrawRectangle(lapizBorde, new Rectangle(0, 0, Width - 1, Height - 1));
        }

        private static void DibujarBotonVentana(
            Graphics g, Rectangle rect, string simbolo, bool hover, Color colorHover, Theme tema, bool activa)
        {
            if (hover)
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var pincel = new SolidBrush(colorHover))
                    g.FillEllipse(pincel, rect);
            }

            var colorTexto = hover ? Color.White : (activa ? tema.TextoSecundario : tema.BordereReposo);
            using (var fuente = new Font("Segoe UI", 9f))
            {
                TextRenderer.DrawText(g, simbolo, fuente, rect, colorTexto,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var cerrarAntes    = _cerrarHover;
            var minimizarAntes = _minimizarHover;
            var maximizarAntes = _maximizarHover;

            _cerrarHover    = _rectBotonCerrar.Contains(e.Location);
            _minimizarHover = _rectBotonMinimizar.Contains(e.Location);
            _maximizarHover = _rectBotonMaximizar.Contains(e.Location);

            if (_cerrarHover != cerrarAntes || _minimizarHover != minimizarAntes || _maximizarHover != maximizarAntes)
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
            WindowState = WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal
                : FormWindowState.Maximized;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PUNTO { public int X; public int Y; }

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
                var pantalla    = Screen.FromHandle(Handle);
                var areaTrabajo = pantalla.WorkingArea;
                var limites     = pantalla.Bounds;

                var info = (MINMAXINFO)Marshal.PtrToStructure(m.LParam, typeof(MINMAXINFO));
                info.ptPosicionMaxima.X          = areaTrabajo.X - limites.X;
                info.ptPosicionMaxima.Y          = areaTrabajo.Y - limites.Y;
                info.ptTamanoMaximo.X            = areaTrabajo.Width;
                info.ptTamanoMaximo.Y            = areaTrabajo.Height;
                info.ptTamanoMaximoSeguimiento.X = areaTrabajo.Width;
                info.ptTamanoMaximoSeguimiento.Y = areaTrabajo.Height;
                Marshal.StructureToPtr(info, m.LParam, true);
                return;
            }

            if (m.Msg == WM_LBUTTONDBLCLK && Redimensionable)
            {
                var punto     = PointToClient(new Point(m.LParam.ToInt32() & 0xFFFF, (m.LParam.ToInt32() >> 16) & 0xFFFF));
                var altoBarra = LogicalToDeviceUnits(AltoBarraTituloLogico);
                if (punto.Y < altoBarra &&
                    !_rectBotonCerrar.Contains(punto) &&
                    !_rectBotonMinimizar.Contains(punto) &&
                    !_rectBotonMaximizar.Contains(punto))
                    AlternarMaximizado();
                return;
            }

            if (m.Msg != WM_NCHITTEST) return;

            if ((int)m.Result == HTCLIENT)
            {
                var punto     = PointToClient(new Point(m.LParam.ToInt32()));
                var altoBarra = LogicalToDeviceUnits(AltoBarraTituloLogico);

                if (punto.Y < altoBarra &&
                    !_rectBotonCerrar.Contains(punto) &&
                    !_rectBotonMinimizar.Contains(punto) &&
                    !_rectBotonMaximizar.Contains(punto))
                {
                    m.Result = (IntPtr)HTCAPTION;
                    return;
                }
            }

            if (!Redimensionable || WindowState == FormWindowState.Maximized) return;

            var margen = LogicalToDeviceUnits(6);
            var local  = PointToClient(new Point(m.LParam.ToInt32()));

            var enIzquierda = local.X <= margen;
            var enDerecha   = local.X >= Width - margen;
            var enArriba    = local.Y <= margen;
            var enAbajo     = local.Y >= Height - margen;

            if      (enIzquierda && enArriba) m.Result = (IntPtr)HTTOPLEFT;
            else if (enDerecha   && enArriba) m.Result = (IntPtr)HTTOPRIGHT;
            else if (enIzquierda && enAbajo)  m.Result = (IntPtr)HTBOTTOMLEFT;
            else if (enDerecha   && enAbajo)  m.Result = (IntPtr)HTBOTTOMRIGHT;
            else if (enIzquierda)             m.Result = (IntPtr)HTLEFT;
            else if (enDerecha)               m.Result = (IntPtr)HTRIGHT;
            else if (enArriba)                m.Result = (IntPtr)HTTOP;
            else if (enAbajo)                 m.Result = (IntPtr)HTBOTTOM;
        }
    }
}
