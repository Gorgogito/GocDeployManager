using System.Windows;
using System.Windows.Media;
using GocDeployManager.Domain.Entities;
using GocDeployManager.UI.Ventanas;

namespace GocDeployManager.UI.Historial
{
    public partial class DetalleDespliegueDialog : VentanaBase
    {
        private static readonly Brush BrushExito  = new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32));
        private static readonly Brush BrushFallido = new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28));

        public DetalleDespliegueDialog(Despliegue despliegue)
        {
            InitializeComponent();
            Loaded += (s, e) => Poblar(despliegue);
        }

        private void Poblar(Despliegue d)
        {
            lblTituloVentana.Text = $"Detalle del despliegue — {d.Goc}";
            Title = lblTituloVentana.Text;

            lblFechaHora.Text         = d.FechaHora.ToString("yyyy-MM-dd HH:mm:ss");
            lblResultado.Text         = d.Resultado.ToString();
            lblResultado.Foreground   = d.Resultado == ResultadoDespliegue.Exitoso ? BrushExito : BrushFallido;
            lblGoc.Text               = d.Goc;
            lblRama.Text              = d.Rama;
            lblAmbiente.Text          = d.Ambiente;
            lblSistemas.Text          = string.Join(", ", d.Sistemas);
            lblUsuarioApp.Text        = d.UsuarioAplicacion;
            lblUsuarioWindows.Text    = d.UsuarioWindows;
            lblEquipo.Text            = d.Equipo;
            lblTiempoCompilacion.Text = d.TiempoCompilacion.ToString(@"hh\:mm\:ss");
            lblTiempoDespliegue.Text  = d.TiempoDespliegue.ToString(@"hh\:mm\:ss");
            lblErrores.Text           = string.IsNullOrEmpty(d.Errores) ? "(ninguno)" : d.Errores;
            lblObservaciones.Text     = string.IsNullOrEmpty(d.Observaciones) ? "(ninguna)" : d.Observaciones;

            if (d.Resultado == ResultadoDespliegue.Fallido)
                lblErrores.Foreground = BrushFallido;
        }
    }
}
