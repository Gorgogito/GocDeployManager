using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DesktopComponents.Theming;

namespace DesktopComponents.Controls
{
    /// <summary>
    /// ListView (vista Detalles) temizado con OwnerDraw + SetWindowTheme —
    /// pensado para el log en tiempo real del despliegue, coloreado por
    /// severidad (sección 10 del análisis: "ListView moderno... coloreado por
    /// severidad info/warn/error").
    /// </summary>
    [DesignerCategory("")]
    public sealed class ListViewX : ListView, IThemedControl
    {
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        private readonly SuscripcionTema _suscripcionTema;

        public ListViewX()
        {
            View = View.Details;
            FullRowSelect = true;
            GridLines = false;
            HeaderStyle = ColumnHeaderStyle.Nonclickable;
            OwnerDraw = true;
            BorderStyle = BorderStyle.None;
            Font = new Font("Segoe UI", 9.5f);

            _suscripcionTema = new SuscripcionTema(AplicarTema);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _suscripcionTema.Dispose();

            base.Dispose(disposing);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            SetWindowTheme(Handle, string.Empty, string.Empty);
            AplicarTema(ThemeManager.Actual);
        }

        public void AplicarTema(Theme tema)
        {
            BackColor = tema.Superficie;
            ForeColor = tema.TextoPrimario;
            Invalidate();
        }

        private const string MarcaSeparadorDeEtapa = "__separador_etapa__";

        // Ancho máximo de texto visto en la columna "Mensaje" desde el último
        // Items.Clear() — permite expandir la columna y mostrar el scroll
        // horizontal cuando algún mensaje supera el ancho de la ventana.
        private int _anchoMinimoMensaje = 0;

        /// <summary>
        /// Agrega una fila (una o más columnas) con un color de texto propio —
        /// ej. rojo para errores, ámbar para advertencias, en el log de despliegue.
        /// Solo autoscrollea al agregarla si el usuario ya estaba viendo el
        /// final de la lista — si hizo scroll hacia arriba a propósito, no lo
        /// arrastra de vuelta abajo.
        /// </summary>
        public ListViewItem AgregarLinea(Color color, params string[] columnas)
        {
            var estabaAlFinal = EstaAlFinal();
            var item = new ListViewItem(columnas) { ForeColor = color };
            Items.Add(item);

            var textoMensaje = columnas.Length > 1 ? columnas[1] : (columnas.Length > 0 ? columnas[0] : string.Empty);
            if (!string.IsNullOrEmpty(textoMensaje))
                ExpansionarColumnaParaMensaje(textoMensaje, Font);

            if (estabaAlFinal)
                item.EnsureVisible();
            return item;
        }

        /// <summary>
        /// Agrega una fila de ancho completo (ignora columnas) para marcar
        /// visualmente el inicio de una etapa nueva en el panel de salida.
        /// </summary>
        public ListViewItem AgregarSeparadorDeEtapa(Color color, string texto)
        {
            var estabaAlFinal = EstaAlFinal();
            var item = new ListViewItem(new[] { string.Empty, texto }) { Tag = MarcaSeparadorDeEtapa, ForeColor = color };
            Items.Add(item);

            if (!string.IsNullOrEmpty(texto))
                ExpansionarColumnaParaSeparador(texto);

            if (estabaAlFinal)
                item.EnsureVisible();
            return item;
        }

        private void ExpansionarColumnaParaMensaje(string texto, Font fuente)
        {
            var ancho = TextRenderer.MeasureText(
                texto, fuente, new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width + LogicalToDeviceUnits(12);

            if (ancho > _anchoMinimoMensaje)
            {
                _anchoMinimoMensaje = ancho;
                ActualizarAnchoUltimaColumna();
            }
        }

        // El separador dibuja su texto a lo ancho de toda la fila (col0 + col1),
        // así que el mínimo para col1 es el ancho total del texto menos col0.
        private void ExpansionarColumnaParaSeparador(string texto)
        {
            using (var fuenteNegrita = new Font(Font, FontStyle.Bold))
            {
                var anchoTotal = TextRenderer.MeasureText(
                    texto, fuenteNegrita, new Size(int.MaxValue, int.MaxValue),
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width + LogicalToDeviceUnits(12);

                var anchoCol0 = Columns.Count > 0 ? Columns[0].Width : 0;
                var anchoNecesario = anchoTotal - anchoCol0;

                if (anchoNecesario > _anchoMinimoMensaje)
                {
                    _anchoMinimoMensaje = anchoNecesario;
                    ActualizarAnchoUltimaColumna();
                }
            }
        }

        /// <summary>
        /// True si la última fila visible coincide con la última fila de la
        /// lista (o si la lista todavía no tiene handle/está vacía) — o sea,
        /// si el usuario está viendo el final y corresponde autoscrollear.
        /// </summary>
        private bool EstaAlFinal()
        {
            if (Items.Count == 0 || !IsHandleCreated)
                return true;

            var rectUltimoItem = GetItemRect(Items.Count - 1);
            return rectUltimoItem.Bottom <= ClientSize.Height;
        }

        /// <summary>
        /// Ajusta la última columna al ancho disponible o al contenido, el que
        /// sea mayor — si el contenido supera la ventana aparece el scroll
        /// horizontal nativo. Llamar después de configurar las columnas.
        /// </summary>
        public void EstirarUltimaColumna() => ActualizarAnchoUltimaColumna();

        private void ActualizarAnchoUltimaColumna()
        {
            if (Columns.Count == 0 || !IsHandleCreated)
                return;

            var anchoFijo = 0;
            for (var i = 0; i < Columns.Count - 1; i++)
                anchoFijo += Columns[i].Width;

            var anchoDisponible = Math.Max(LogicalToDeviceUnits(60), ClientSize.Width - anchoFijo);
            Columns[Columns.Count - 1].Width = Math.Max(anchoDisponible, _anchoMinimoMensaje);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ActualizarAnchoUltimaColumna();
        }

        protected override void WndProc(ref Message m)
        {
            const int LvmDeleteAllItems = 0x1009;
            if (m.Msg == LvmDeleteAllItems)
                _anchoMinimoMensaje = 0;
            base.WndProc(ref m);
        }

        protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
        {
            var tema = ThemeManager.Actual;

            using (var pincel = new SolidBrush(tema.Fondo))
                e.Graphics.FillRectangle(pincel, e.Bounds);

            TextRenderer.DrawText(
                e.Graphics, e.Header.Text, Font,
                new Rectangle(e.Bounds.X + LogicalToDeviceUnits(6), e.Bounds.Y, e.Bounds.Width, e.Bounds.Height),
                tema.TextoSecundario,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left |
                TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);

            using (var lapiz = new Pen(tema.Borde))
                e.Graphics.DrawLine(lapiz, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
        }

        protected override void OnDrawItem(DrawListViewItemEventArgs e)
        {
            // Con OwnerDraw=true en vista Detalles, el dibujo real ocurre por
            // subitem en OnDrawSubItem; este método solo evita que WinForms
            // recurra al dibujo nativo para la fila completa.
        }

        protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
        {
            var tema = ThemeManager.Actual;

            if (e.Item.Tag as string == MarcaSeparadorDeEtapa)
            {
                DibujarSeparadorDeEtapa(e, tema);
                return;
            }

            // OJO: e.ItemState viene poco confiable en .NET (suele reportar
            // "Selected" siempre); e.Item.Selected es la fuente de verdad real.
            var seleccionado = e.Item.Selected;

            var colorFondo = seleccionado ? Theme.Mezclar(tema.Acento, tema.Superficie, 0.82f) : tema.Superficie;
            var colorTexto = e.Item.ForeColor;

            using (var pincel = new SolidBrush(colorFondo))
                e.Graphics.FillRectangle(pincel, e.Bounds);

            TextRenderer.DrawText(
                e.Graphics, e.SubItem.Text, Font,
                new Rectangle(e.Bounds.X + LogicalToDeviceUnits(6), e.Bounds.Y, e.Bounds.Width, e.Bounds.Height),
                colorTexto,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left |
                TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
        }

        /// <summary>
        /// Dibuja la fila separadora una sola vez, a todo el ancho de la fila
        /// (todas las columnas juntas) — se pinta en la primera subcolumna y
        /// las siguientes no dibujan nada, para no repetir la barra.
        /// </summary>
        private void DibujarSeparadorDeEtapa(DrawListViewSubItemEventArgs e, Theme tema)
        {
            if (e.ColumnIndex != 0)
                return;

            var bounds = e.Item.Bounds;

            using (var pincel = new SolidBrush(Theme.Mezclar(e.Item.ForeColor, tema.Superficie, 0.85f)))
                e.Graphics.FillRectangle(pincel, bounds);

            using (var fuenteNegrita = new Font(Font, FontStyle.Bold))
            {
                TextRenderer.DrawText(
                    e.Graphics, e.Item.SubItems[1].Text, fuenteNegrita,
                    new Rectangle(bounds.X + LogicalToDeviceUnits(6), bounds.Y, bounds.Width - LogicalToDeviceUnits(12), bounds.Height),
                    e.Item.ForeColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.SingleLine);
            }
        }
    }
}
