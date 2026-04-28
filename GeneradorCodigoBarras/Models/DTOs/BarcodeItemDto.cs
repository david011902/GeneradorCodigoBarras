

namespace GeneradorCodigoBarras.Models.DTOs
{
    public class BarcodeItemDto
    {
        public string Code { get; set; } = string.Empty;
        public string Label { get; set; }= string.Empty;
        public System.Drawing.Image BarcodeImage { get; set; }
        public int Quantity { get; set; } = 1;
        public override string ToString()
        {
            return $"{Label} - (Cant: {Quantity})";
        }
    }
}
