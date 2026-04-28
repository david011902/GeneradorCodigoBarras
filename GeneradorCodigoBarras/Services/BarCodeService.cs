using GeneradorCodigoBarras.Services.IServices;
using System.Drawing.Imaging;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

namespace GeneradorCodigoBarras.Services
{
    public class BarCodeService : IBarcodeService
    {

        public Bitmap GenerateBarcode(string code, string? label = null)
        {
            var writer = new BarcodeWriter<Bitmap>
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Height = 60,
                    Width = 300,
                    Margin = 0,
                    PureBarcode = true
                },
                Renderer = new BitmapRenderer()
            };

            var barcodeOnly = writer.Write(code);
            Bitmap finalBitmap = new Bitmap(300, 110);

            using (var g = Graphics.FromImage(finalBitmap))
            {
                g.Clear(Color.White);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Dibuja barras centradas
                g.DrawImage(barcodeOnly, 0, 10);

                string textoCompleto = $"{code} - {label ?? ""}";

                var fontText = new Font("Arial", 10, FontStyle.Bold);
                var sizeText = g.MeasureString(textoCompleto, fontText);

                // texto unificado en una sola posición Y (80)
                g.DrawString(textoCompleto, fontText, Brushes.Black,
                    (finalBitmap.Width - sizeText.Width) / 2, 80);
            }

            return finalBitmap;
        }

        public void SaveBarcode(Bitmap bitmap, string filePath)
        {
            var format = filePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                            ? ImageFormat.Png
                            : ImageFormat.Jpeg;

            bitmap.Save(filePath, format);
        }
    }
}
