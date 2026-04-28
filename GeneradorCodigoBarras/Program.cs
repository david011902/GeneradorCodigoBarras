using GeneradorCodigoBarras.Services;
using GeneradorCodigoBarras.Services.IServices;
using GeneradorCodigoBarras.Views;
using Microsoft.Extensions.Configuration;
using System;
using System.Text;
using System.Windows.Forms;
namespace GeneradorCodigoBarras
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ApplicationConfiguration.Initialize();

            //Cargar configuración
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string baseUrl = config["ApiSettings:BaseUrl"];

            IApiService apiService = new ApiService(baseUrl);
            Application.Run(new Form1(apiService));
        }
    }
}
