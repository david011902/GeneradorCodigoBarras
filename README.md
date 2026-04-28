# Generador Codigo Barras
 
Herramienta de escritorio personal (**WinForms / .NET 8**) que complementa el sistema gestor de inventario principal. Se conecta al backend **ASP.NET Core Web API** alojado en **Azure**, obtiene los productos vía JWT y genera códigos de barras en PDF listos para imprimir, basados en el SKU de cada ítem. Tambien incluye generación de codigo de barras en modo local.
 
---
 
## Funcionalidades
 
- Autenticación con **JWT** contra la API en Azure.
- Consulta de productos desde el endpoint del sistema de inventario.
- Generación de **códigos de barras** (formato Code 128) a partir del SKU.
- Exportación a **PDF** con PDFsharp, listo para impresión.
- Inyección de dependencias y configuración por `appsettings.json`.
---
 
## Estructura del proyecto
 
```
GeneradorCodigoBarras/
├── Models/
│   └── DTOs/
│       ├── BarcodeItemDto.cs         # Modelo del ítem de código de barras
│       └── ProductResponseDto.cs     # Respuesta de la API de productos
├── Services/
│   ├── IServices/
│   │   ├── IApiService.cs            # Contrato para consumo de API
│   │   ├── IBarcodeService.cs        # Contrato para generación de códigos de barras
│   │   └── IPdfService.cs            # Contrato para generación de PDF
│   ├── ApiService.cs                 # Implementación: HttpClient + JWT
│   ├── BarCodeService.cs             # Implementación: ZXing.Net
│   └── PdfService.cs                 # Implementación: PDFsharp
├── Views/
│   └── Form1.cs                      # UI principal (WinForms)
├── Program.cs                        # Entrada + DI container
└── appsettings.json                  # URL de la API, credenciales, config
```
 
---
## Cómo ejecutar
 
### Requisitos previos
 
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Acceso a la API del sistema de inventario en Azure
- Visual Studio 2022+ (recomendado) o `dotnet CLI`
### Pasos
 
```bash
# 1. Clonar el repositorio
git clone https://github.com/tu-usuario/GeneradorCodigoBarras.git
cd GeneradorCodigoBarras
 
# 2. Configurar appsettings.json con tus credenciales
 
# 3. Restaurar dependencias
dotnet restore
 
# 4. Ejecutar
dotnet run
```
 
O directamente desde Visual Studio: **F5** / `Ctrl+F5`.
 
---
 
## Flujo de trabajo
 
```
Inicio
  └─► Autenticación JWT (ApiService)
        └─► GET /api/products (ApiService)
              └─► Generación de código de barras por SKU (BarCodeService)
                    └─► Renderizado en PDF (PdfService)
                          └─► Descarga / impresión
```
 
---
 
 
## Proyecto relacionado
 
Este generador es un complemento del **Sistema Gestor de Inventario** cuyo backend es una ASP.NET Core Web API desplegada en Azure.

---
## Autor
**David Acosta**  
Ingeniero en Sistemas Computacionales  
[GitHub](https://github.com/david011902)

 
