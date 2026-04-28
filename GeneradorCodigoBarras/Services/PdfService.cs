using GeneradorCodigoBarras.Services.IServices;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Drawing.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace GeneradorCodigoBarras.Services
{
    public class PdfService : IPdfService
    {
        public void GeneratePdf(List<BarcodeItem> items, string outputPath)
        {
            var document = new PdfDocument();

            // 1. Configuraciones base
            double cellWidth = 100;
            double cellHeight = 65;
            double imgWidth = 90;
            double imgHeight = 50;
            XPen cutLinePen = new XPen(XColors.LightGray, 0.3);

            var page = document.AddPage();
            page.Size = PageSize.A4;
            var gfx = XGraphics.FromPdfPage(page);
            // Permite sabes cuantos codigos de barra caben 
            int columnsPerPage = (int)(page.Width / cellWidth);

            // Margenes
            double totalGridWidth = columnsPerPage * cellWidth;
            double startMarginX = (page.Width - totalGridWidth) / 2;
            double startMarginY = 40;

            double currentX = startMarginX;
            double currentY = startMarginY;

            foreach (var item in items)
            {
                for (int i = 0; i < item.Quantity; i++)
                {
                    // Dibujar recuadro de corte
                    gfx.DrawRectangle(cutLinePen, currentX, currentY, cellWidth, cellHeight);

                    if (item.BarcodeImage != null)
                    {
                        using (var ms = new MemoryStream())
                        {
                            using (var tempBitmap = new Bitmap(item.BarcodeImage))
                            {
                                tempBitmap.Save(ms, ImageFormat.Png);
                            }
                            ms.Position = 0;
                            XImage img = XImage.FromStream(ms);

                            // Centrar imagen dentro de la celda
                            double offsetX = (cellWidth - imgWidth) / 2;
                            double offsetY = (cellHeight - imgHeight) / 2;

                            gfx.DrawImage(img, currentX + offsetX, currentY + offsetY, imgWidth, imgHeight);

                            // Movimiento de coordenadas
                            currentX += cellWidth;

                            // Evita saltos sin sentido
                            if (currentX + cellWidth > page.Width - startMarginX + 0.1)
                            {
                                currentX = startMarginX;
                                currentY += cellHeight;
                            }

                            // Verifica si se llego al final de la hoja
                            if (currentY + cellHeight > page.Height - startMarginY)
                            {
                                page = document.AddPage();
                                page.Size = PageSize.A4;
                                gfx = XGraphics.FromPdfPage(page);
                                currentX = startMarginX;
                                currentY = startMarginY;
                            }
                        }
                    }
                }
            }
            document.Save(outputPath);
        }
    }
}