using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DesktopComponents.Controls;
using DesktopComponents.Forms;
using GocDeployManager.Domain.Entities;

namespace GocDeployManager.UI.Historial
{
    /// <summary>
    /// Detalle completo de un despliegue (sección 10 del análisis). Solo
    /// lectura: nada de lo que se ve aquí se puede editar.
    /// </summary>
    [DesignerCategory("")]
    public sealed class DetalleDespliegueDialog : MetroForm
    {
        public DetalleDespliegueDialog(Despliegue despliegue)
        {
            Text = $"Detalle del despliegue — {despliegue.Goc}";
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(LogicalToDeviceUnits(520), LogicalToDeviceUnits(420));

            var propGrid = new PropertyGridX
            {
                Location = new Point(0, 0),
                Size = new Size(ClientSize.Width, ClientSize.Height - LogicalToDeviceUnits(56)),
                SelectedObject = new DespliegueDetalle(despliegue),
            };

            var btnCerrar = new ButtonX
            {
                Text = "Cerrar",
                Width = LogicalToDeviceUnits(120),
                Location = new Point(LogicalToDeviceUnits(20), ClientSize.Height - LogicalToDeviceUnits(44)),
                Variante = VarianteButtonX.Secundario,
            };
            btnCerrar.Click += (s, e) => Close();

            Controls.Add(propGrid);
            Controls.Add(btnCerrar);

            AcceptButton = btnCerrar;
        }
    }
}
