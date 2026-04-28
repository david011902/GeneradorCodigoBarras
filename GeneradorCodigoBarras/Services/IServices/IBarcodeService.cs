using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCodigoBarras.Services.IServices
{
    public interface IBarcodeService
    {
        Bitmap GenerateBarcode(string code, string? label = null);
        void SaveBarcode(Bitmap bitmap, string filePath);
    }
}
