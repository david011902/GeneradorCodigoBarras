using GeneradorCodigoBarras.Models.DTOs;
using GeneradorCodigoBarras.Services;
using GeneradorCodigoBarras.Services.IServices;

namespace GeneradorCodigoBarras.Views
{
    public partial class Form1 : Form
    {
        private readonly IBarcodeService _barcodeService;
        private IPdfService _pdfService;
        private IApiService? _apiService;
        private string? _jwtToken;
        private List<BarcodeItem> _generatedItems = new();
        private List<BarcodeItem> _selectedItems = new();
        public Form1(IApiService apiService)
        {
            InitializeComponent();
            _barcodeService = new BarCodeService();
            _pdfService = new PdfService();
            button1.Click += btnGenerarLocal_Click;
            btnLogin.Click += btnLogin_Click;
            btnCargarProductos.Click += btnCargarProductos_Click;
            btnGenerarDesdeApi.Click += btnGenerarDesdeApi_Click;
            btnGuardarImagen.Click += btnGuardarImagen_Click;
            btnExportarPdf.Click += btnExportarPdf_Click;
            btnLimpiar.Click += btnLimpiar_Click;
            listBoxSeleccionados.Format += listBoxSeleccionados_Format;
            _apiService = apiService;
            ConfigurarUI();
        }

        private void ConfigurarUI()
        {
            this.Text = "Generador de Códigos de Barra";
            this.StartPosition = FormStartPosition.CenterScreen;
            lblStatus.Text = "Listo.";
            // Deshabilitar controles de API hasta que haya Login
            listBoxProductos.Enabled = false;
            btnCargarProductos.Enabled = false;
            btnGenerarDesdeApi.Enabled = false;
        }

        // Modo local
        private void btnGenerarLocal_Click(object sender, EventArgs e)
        {
            var codigo = txtCodigoLocal.Text.Trim();
            var etiqueta = txtEtiquetaLocal.Text.Trim();
            // Captura la cantidad elegida por el usuario para los codigos que se requieran imprimir
            int cantidad = (int)numCantidadLocal.Value;

            if (string.IsNullOrWhiteSpace(codigo))
            {
                MessageBox.Show("Ingresa un código.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cantidad <= 0)
            {
                MessageBox.Show("La cantidad debe ser mayor a 0.", "Aviso");
                return;
            }


            var bitmap = rbQR.Checked
                ? _barcodeService.GenerateBarcode(codigo, etiqueta)
                : _barcodeService.GenerateBarcode(codigo, etiqueta);

            if (bitmap != null)
            {
                pictureBoxPreview.Image = bitmap;

                _generatedItems.Add(new BarcodeItem
                {
                    Code = codigo,
                    Label = string.IsNullOrWhiteSpace(etiqueta) ? codigo : etiqueta,
                    BarcodeImage = bitmap,
                    Quantity = cantidad
                });

                lblStatus.Text = $" {cantidad} etiqueta(s) agregada(s) de: {codigo}";
            }
        }

        // API 
        private async void btnLogin_Click(object sender, EventArgs e)
        {
            var token = await _apiService.LoginAsync(txtUsuario.Text, txtPassword.Text);

            if (token != null)
            {
                _apiService.SetToken(token);
                lblStatus.Text = "Login correcto";
                btnCargarProductos.Enabled = true;
                listBoxProductos.Enabled = true;
            }
            else
            {
                lblStatus.Text = "Login fallido";
            }
        }

        private async void btnCargarProductos_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                var productos = await _apiService.GetProductAsync();

                listBoxProductos.BeginUpdate();
                listBoxProductos.DataSource = null;
                listBoxProductos.DataSource = productos;
                listBoxProductos.DisplayMember = "Name";
                listBoxProductos.ValueMember = "Sku";

                listBoxProductos.EndUpdate();
                btnGenerarDesdeApi.Enabled = productos.Any();
                lblStatus.Text = $"{productos.Count} productos cargados.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error API", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnGenerarDesdeApi_Click(object sender, EventArgs e)
        {
            var seleccionados = listBoxProductos.SelectedItems
        .Cast<ProductResponseDto>()
        .ToList();

            if (!seleccionados.Any())
            {
                MessageBox.Show("Selecciona al menos un producto");
                return;
            }

            int cantidad = (int)numCantidadApi.Value;

            foreach (var producto in seleccionados)
            {
                // evitar duplicados
                if (_selectedItems.Any(x => x.Code == producto.Sku))
                    continue;

                var bitmap = _barcodeService.GenerateBarcode(producto.Sku, producto.Name);

                var item = new BarcodeItem
                {
                    Code = producto.Sku,
                    Label = producto.Name,
                    BarcodeImage = bitmap,
                    Quantity = cantidad
                };

                _generatedItems.Add(item);
                _selectedItems.Add(item);
            }

            listBoxSeleccionados.DataSource = null;
            listBoxSeleccionados.DataSource = _selectedItems;

            lblStatus.Text = $"{_selectedItems.Count} productos en lista.";
        }

        // Acciones comunes
        private void btnGuardarImagen_Click(object sender, EventArgs e)
        {
            if (pictureBoxPreview.Image == null) return;
            using var sfd = new SaveFileDialog { Filter = "PNG|*.png", FileName = "barcode" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                _barcodeService.SaveBarcode((Bitmap)pictureBoxPreview.Image, sfd.FileName);
                lblStatus.Text = "Imagen guardada.";
            }
        }

        private void btnExportarPdf_Click(object sender, EventArgs e)
        {
            if (_generatedItems.Count == 0) return;
            using var sfd = new SaveFileDialog { Filter = "PDF|*.pdf", FileName = "ListaCodigos" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                _pdfService.GeneratePdf(_generatedItems, sfd.FileName);
                lblStatus.Text = "PDF generado.";
            }
        }
        private void btnQuitar_Click(object sender, EventArgs e)
        {
            if (listBoxSeleccionados.SelectedItem is BarcodeItem item)
            {
                _selectedItems.Remove(item);
                _generatedItems.Remove(item);

                listBoxSeleccionados.DataSource = null;
                listBoxSeleccionados.DataSource = _selectedItems;
            }
        }
        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            _generatedItems.Clear();
            pictureBoxPreview.Image = null;
            lblStatus.Text = "Limpiado.";
        }
        private void listBoxSeleccionados_Format(object sender, ListControlConvertEventArgs e)
        {
            if (e.ListItem is BarcodeItem item)
            {
                e.Value = $"{item.Label} - [{item.Quantity} uds]";
            }
        }
        private void InitializeComponent()
        {
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            label6 = new Label();
            numCantidadLocal = new NumericUpDown();
            label2 = new Label();
            label1 = new Label();
            button1 = new Button();
            rbQR = new RadioButton();
            rbCode128 = new RadioButton();
            txtEtiquetaLocal = new TextBox();
            txtCodigoLocal = new TextBox();
            tabPage = new TabPage();
            numCantidadApi = new NumericUpDown();
            btnGenerarDesdeApi = new Button();
            btnCargarProductos = new Button();
            listBoxProductos = new ListBox();
            label5 = new Label();
            label4 = new Label();
            btnLogin = new Button();
            txtPassword = new TextBox();
            txtUsuario = new TextBox();
            grpPreview = new GroupBox();
            pictureBoxPreview = new PictureBox();
            btnGuardarImagen = new Button();
            btnExportarPdf = new Button();
            btnLimpiar = new Button();
            lblStatus = new Label();
            listBoxSeleccionados = new ListBox();
            label3 = new Label();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCantidadLocal).BeginInit();
            tabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCantidadApi).BeginInit();
            grpPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).BeginInit();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage);
            tabControl1.Location = new Point(12, 12);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1303, 466);
            tabControl1.TabIndex = 0;
            tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(label6);
            tabPage1.Controls.Add(numCantidadLocal);
            tabPage1.Controls.Add(label2);
            tabPage1.Controls.Add(label1);
            tabPage1.Controls.Add(button1);
            tabPage1.Controls.Add(rbQR);
            tabPage1.Controls.Add(rbCode128);
            tabPage1.Controls.Add(txtEtiquetaLocal);
            tabPage1.Controls.Add(txtCodigoLocal);
            tabPage1.Location = new Point(8, 46);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(1287, 412);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Modo Local";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(627, 55);
            label6.Name = "label6";
            label6.Size = new Size(225, 32);
            label6.TabIndex = 8;
            label6.Text = "Cantidad a Imprimir";
            // 
            // numCantidadLocal
            // 
            numCantidadLocal.Location = new Point(678, 111);
            numCantidadLocal.Name = "numCantidadLocal";
            numCantidadLocal.Size = new Size(80, 39);
            numCantidadLocal.TabIndex = 7;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(122, 86);
            label2.Name = "label2";
            label2.Size = new Size(71, 32);
            label2.TabIndex = 6;
            label2.Text = "Texto";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(52, 37);
            label1.Name = "label1";
            label1.Size = new Size(141, 32);
            label1.TabIndex = 5;
            label1.Text = "Ingresa SKU";
            // 
            // button1
            // 
            button1.Location = new Point(186, 218);
            button1.Name = "button1";
            button1.Size = new Size(150, 46);
            button1.TabIndex = 4;
            button1.Text = "Generar Código";
            button1.UseVisualStyleBackColor = true;
            // 
            // rbQR
            // 
            rbQR.AutoSize = true;
            rbQR.Location = new Point(320, 154);
            rbQR.Name = "rbQR";
            rbQR.Size = new Size(161, 36);
            rbQR.TabIndex = 3;
            rbQR.TabStop = true;
            rbQR.Text = "Código QR";
            rbQR.UseVisualStyleBackColor = true;
            // 
            // rbCode128
            // 
            rbCode128.AutoSize = true;
            rbCode128.Location = new Point(65, 154);
            rbCode128.Name = "rbCode128";
            rbCode128.Size = new Size(193, 36);
            rbCode128.TabIndex = 2;
            rbCode128.TabStop = true;
            rbCode128.Text = "Código Barras";
            rbCode128.UseVisualStyleBackColor = true;
            rbCode128.CheckedChanged += radioButton1_CheckedChanged;
            // 
            // txtEtiquetaLocal
            // 
            txtEtiquetaLocal.Location = new Point(225, 86);
            txtEtiquetaLocal.Name = "txtEtiquetaLocal";
            txtEtiquetaLocal.Size = new Size(200, 39);
            txtEtiquetaLocal.TabIndex = 1;
            // 
            // txtCodigoLocal
            // 
            txtCodigoLocal.Location = new Point(229, 37);
            txtCodigoLocal.Name = "txtCodigoLocal";
            txtCodigoLocal.Size = new Size(200, 39);
            txtCodigoLocal.TabIndex = 0;
            txtCodigoLocal.TextChanged += textBox1_TextChanged;
            // 
            // tabPage
            // 
            tabPage.Controls.Add(numCantidadApi);
            tabPage.Controls.Add(btnGenerarDesdeApi);
            tabPage.Controls.Add(btnCargarProductos);
            tabPage.Controls.Add(listBoxProductos);
            tabPage.Controls.Add(label5);
            tabPage.Controls.Add(label4);
            tabPage.Controls.Add(btnLogin);
            tabPage.Controls.Add(txtPassword);
            tabPage.Controls.Add(txtUsuario);
            tabPage.Location = new Point(8, 46);
            tabPage.Name = "tabPage";
            tabPage.Padding = new Padding(3);
            tabPage.Size = new Size(1287, 412);
            tabPage.TabIndex = 1;
            tabPage.Text = "Modo Online";
            tabPage.UseVisualStyleBackColor = true;
            // 
            // numCantidadApi
            // 
            numCantidadApi.Location = new Point(1031, 340);
            numCantidadApi.Name = "numCantidadApi";
            numCantidadApi.Size = new Size(109, 39);
            numCantidadApi.TabIndex = 10;
            // 
            // btnGenerarDesdeApi
            // 
            btnGenerarDesdeApi.Location = new Point(850, 315);
            btnGenerarDesdeApi.Name = "btnGenerarDesdeApi";
            btnGenerarDesdeApi.Size = new Size(150, 87);
            btnGenerarDesdeApi.TabIndex = 9;
            btnGenerarDesdeApi.Text = "Generar Código";
            btnGenerarDesdeApi.UseVisualStyleBackColor = true;
            // 
            // btnCargarProductos
            // 
            btnCargarProductos.Location = new Point(683, 313);
            btnCargarProductos.Name = "btnCargarProductos";
            btnCargarProductos.Size = new Size(150, 91);
            btnCargarProductos.TabIndex = 8;
            btnCargarProductos.Text = "Obtener Productos";
            btnCargarProductos.UseVisualStyleBackColor = true;
            // 
            // listBoxProductos
            // 
            listBoxProductos.FormattingEnabled = true;
            listBoxProductos.Location = new Point(651, 30);
            listBoxProductos.Name = "listBoxProductos";
            listBoxProductos.SelectionMode = SelectionMode.MultiExtended;
            listBoxProductos.Size = new Size(619, 260);
            listBoxProductos.TabIndex = 7;
            listBoxProductos.SelectedIndexChanged += listBoxProductos_SelectedIndexChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(47, 124);
            label5.Name = "label5";
            label5.Size = new Size(134, 32);
            label5.TabIndex = 6;
            label5.Text = "Contraseña";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(110, 49);
            label4.Name = "label4";
            label4.Size = new Size(71, 32);
            label4.TabIndex = 5;
            label4.Text = "Email";
            label4.Click += label4_Click;
            // 
            // btnLogin
            // 
            btnLogin.Location = new Point(260, 206);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(202, 46);
            btnLogin.TabIndex = 3;
            btnLogin.Text = "Iniciar Sesion";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += btnLogin_Click_1;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(217, 121);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(336, 39);
            txtPassword.TabIndex = 2;
            // 
            // txtUsuario
            // 
            txtUsuario.Location = new Point(223, 46);
            txtUsuario.Name = "txtUsuario";
            txtUsuario.Size = new Size(330, 39);
            txtUsuario.TabIndex = 1;
            txtUsuario.TextChanged += txtUsuario_TextChanged;
            // 
            // grpPreview
            // 
            grpPreview.Controls.Add(pictureBoxPreview);
            grpPreview.Location = new Point(20, 493);
            grpPreview.Name = "grpPreview";
            grpPreview.Size = new Size(400, 200);
            grpPreview.TabIndex = 1;
            grpPreview.TabStop = false;
            grpPreview.Text = "Vista previa";
            // 
            // pictureBoxPreview
            // 
            pictureBoxPreview.Location = new Point(19, 44);
            pictureBoxPreview.Name = "pictureBoxPreview";
            pictureBoxPreview.Size = new Size(363, 156);
            pictureBoxPreview.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxPreview.TabIndex = 0;
            pictureBoxPreview.TabStop = false;
            pictureBoxPreview.Click += pictureBox1_Click;
            // 
            // btnGuardarImagen
            // 
            btnGuardarImagen.Location = new Point(449, 529);
            btnGuardarImagen.Name = "btnGuardarImagen";
            btnGuardarImagen.Size = new Size(211, 46);
            btnGuardarImagen.TabIndex = 2;
            btnGuardarImagen.Text = "Guardar Imagen";
            btnGuardarImagen.UseVisualStyleBackColor = true;
            // 
            // btnExportarPdf
            // 
            btnExportarPdf.Location = new Point(453, 599);
            btnExportarPdf.Name = "btnExportarPdf";
            btnExportarPdf.Size = new Size(207, 46);
            btnExportarPdf.TabIndex = 3;
            btnExportarPdf.Text = "Exportar Pdf";
            btnExportarPdf.UseVisualStyleBackColor = true;
            // 
            // btnLimpiar
            // 
            btnLimpiar.Location = new Point(455, 659);
            btnLimpiar.Name = "btnLimpiar";
            btnLimpiar.Size = new Size(205, 46);
            btnLimpiar.TabIndex = 4;
            btnLimpiar.Text = "Limpiar";
            btnLimpiar.UseVisualStyleBackColor = true;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(39, 705);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(78, 32);
            lblStatus.TabIndex = 5;
            lblStatus.Text = "label6";
            // 
            // listBoxSeleccionados
            // 
            listBoxSeleccionados.FormattingEnabled = true;
            listBoxSeleccionados.Location = new Point(671, 529);
            listBoxSeleccionados.Name = "listBoxSeleccionados";
            listBoxSeleccionados.Size = new Size(620, 260);
            listBoxSeleccionados.TabIndex = 6;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(852, 481);
            label3.Name = "label3";
            label3.Size = new Size(278, 32);
            label3.TabIndex = 7;
            label3.Text = "Productos Seleccionados";
            // 
            // Form1
            // 
            ClientSize = new Size(1375, 854);
            Controls.Add(label3);
            Controls.Add(listBoxSeleccionados);
            Controls.Add(lblStatus);
            Controls.Add(btnLimpiar);
            Controls.Add(btnExportarPdf);
            Controls.Add(btnGuardarImagen);
            Controls.Add(grpPreview);
            Controls.Add(tabControl1);
            Name = "Form1";
            Load += Form1_Load;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCantidadLocal).EndInit();
            tabPage.ResumeLayout(false);
            tabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCantidadApi).EndInit();
            grpPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }



        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void listBoxProductos_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void txtUsuario_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click_1(object sender, EventArgs e)
        {

        }

    }
}