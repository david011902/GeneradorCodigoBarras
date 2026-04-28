using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCodigoBarras.Models.DTOs
{
    public class ProductResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string VehicleTypeName { get; set; } = string.Empty; // Solo el nombre
        public string SocketTypeName { get; set; } = string.Empty;  // Solo el nombre
        public Guid? VehicleTypeId { get; set; }
        public Guid? SocketTypeId { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
    }
}
