using GeneradorCodigoBarras.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCodigoBarras.Services.IServices
{
    public interface IApiService
    {
        Task<string?> LoginAsync(string email, string password);

        void SetToken(string token); 

        Task<List<ProductResponseDto>> GetProductAsync();

        Task<ProductResponseDto?> GetProductByCodeAsync(string code);

    }
}
