using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCodigoBarras.Services.IServices
{
    public interface IPdfService
    {
        void GeneratePdf(List<BarcodeItem> items, string outputPath);
    }
    public class BarcodeItem
    {
        public string Code { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public System.Drawing.Image BarcodeImage { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
