using System.Collections.Generic;
using ClosedXML.Excel;
using GocDeployManager.Domain.Entities;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace GocDeployManager.UI.Historial
{
    internal static class ExportadorHistorial
    {
        private static readonly string[] Columnas =
        {
            "Fecha y hora", "GOC", "Rama", "Ambiente", "Sistemas", "Usuario", "Equipo",
            "Resultado", "Tiempo compilación", "Tiempo despliegue", "Errores", "Observaciones",
        };

        public static void ExportarExcel(IReadOnlyList<Despliegue> despliegues, string rutaArchivo)
        {
            using (var libro = new XLWorkbook())
            {
                var hoja = libro.Worksheets.Add("Historial");

                for (var col = 0; col < Columnas.Length; col++)
                    hoja.Cell(1, col + 1).Value = Columnas[col];
                hoja.Row(1).Style.Font.Bold = true;

                for (var fila = 0; fila < despliegues.Count; fila++)
                {
                    var d = despliegues[fila];
                    var r = fila + 2;

                    hoja.Cell(r, 1).Value = d.FechaHora.ToString("yyyy-MM-dd HH:mm:ss");
                    hoja.Cell(r, 2).Value = d.Goc;
                    hoja.Cell(r, 3).Value = d.Rama;
                    hoja.Cell(r, 4).Value = d.Ambiente;
                    hoja.Cell(r, 5).Value = string.Join(", ", d.Sistemas);
                    hoja.Cell(r, 6).Value = d.UsuarioAplicacion;
                    hoja.Cell(r, 7).Value = d.Equipo;
                    hoja.Cell(r, 8).Value = d.Resultado.ToString();
                    hoja.Cell(r, 9).Value = d.TiempoCompilacion.ToString(@"hh\:mm\:ss");
                    hoja.Cell(r, 10).Value = d.TiempoDespliegue.ToString(@"hh\:mm\:ss");
                    hoja.Cell(r, 11).Value = d.Errores ?? string.Empty;
                    hoja.Cell(r, 12).Value = d.Observaciones ?? string.Empty;
                }

                hoja.Columns().AdjustToContents();
                libro.SaveAs(rutaArchivo);
            }
        }

        public static void ExportarPdf(IReadOnlyList<Despliegue> despliegues, string rutaArchivo)
        {
            using (var documento = new PdfDocument())
            {
                var fuenteTitulo    = new XFont("Segoe UI", 14, XFontStyle.Bold);
                var fuenteEncabezado = new XFont("Segoe UI", 9, XFontStyle.Bold);
                var fuenteTexto     = new XFont("Segoe UI", 9, XFontStyle.Regular);

                const double margen      = 40;
                const double alturaLinea = 15;

                PdfPage  pagina = null;
                XGraphics g     = null;
                double   y      = 0;

                void NuevaPagina()
                {
                    pagina = documento.AddPage();
                    g = XGraphics.FromPdfPage(pagina);
                    g.DrawString("Historial de despliegues — GocDeployManager",
                                 fuenteTitulo, XBrushes.Black, margen, margen);
                    y = margen + 26;
                }

                NuevaPagina();

                foreach (var d in despliegues)
                {
                    if (y > pagina.Height - 90)
                        NuevaPagina();

                    g.DrawString(
                        $"{d.FechaHora:yyyy-MM-dd HH:mm:ss}  ·  {d.Goc}  ·  {d.Ambiente}" +
                        $"  ·  {string.Join(", ", d.Sistemas)}  ·  {d.Resultado}",
                        fuenteEncabezado, XBrushes.Black, margen, y);
                    y += alturaLinea;

                    g.DrawString(
                        $"Usuario: {d.UsuarioAplicacion} ({d.UsuarioWindows}@{d.Equipo})" +
                        $"  ·  Compilación: {d.TiempoCompilacion:hh\\:mm\\:ss}" +
                        $"  ·  Despliegue: {d.TiempoDespliegue:hh\\:mm\\:ss}",
                        fuenteTexto, XBrushes.Black, margen, y);
                    y += alturaLinea;

                    if (!string.IsNullOrEmpty(d.Errores))
                    {
                        g.DrawString($"Error: {d.Errores}", fuenteTexto, XBrushes.DarkRed, margen, y);
                        y += alturaLinea;
                    }

                    if (!string.IsNullOrEmpty(d.Observaciones))
                    {
                        g.DrawString($"Observaciones: {d.Observaciones}", fuenteTexto, XBrushes.Black, margen, y);
                        y += alturaLinea;
                    }

                    y += 10;
                }

                documento.Save(rutaArchivo);
            }
        }
    }
}
