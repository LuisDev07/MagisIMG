using ImageMagick;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MagisIMG
{
    public partial class ConverterIMG : BorderlessFormBase
    {
        private string selectedImagePath; // Almacena la ruta de la imagen seleccionada

        private FormAnimator animator; // <---- variables para la animacion de la ventana
        public ConverterIMG()
        {
            InitializeComponent();
            animator = new FormAnimator(this); // <---- le decimos que la ventana es la que queremos animar en este caso este form
            // Habilitar arrastre desde el panel
            EnableDrag(PanelHerramienta); //<--- llamamos el metodo del form base y le pasamos el panel  


            //agregamos color al titulo
            // Solo pasamos el Label, los colores ya están dentro de la clase
            HtmlColorAnimator animatorletra = new HtmlColorAnimator(lbltitle);


            //agregamos el efecto de hover al  picturesbox
            var hoverEffect = new PictureBoxHoverEffect(PicBoxVolver);
        }

     
        private void Principal_Load(object sender, EventArgs e)
        {
            animator.ShowWithFade(); // llamamos al metodo de animacion de la clase FormAnimator


            // Configurar el ComboBox de formatos
            ComboBox cbFormat = this.Controls.Find("cbFormat", true)[0] as ComboBox;
            // Agregar ítems al ComboBox
            cbFormat.Items.AddRange(new string[] { "PNG", "JPEG", "BMP", "GIF", "TIFF", "WEBP" });
            cbFormat.SelectedIndex = 0; // Selecciona PNG por defecto



            //llamamos la clase para redondear los botones
            BotonHelper.Redondear(btnSeleccionar, 5); 
            BotonHelper.Redondear(btnGuardar,5); 
            BotonHelper.Redondear(btnLimpiar, 5);




            //agregamos un layout por defecto
            AgregarImagenPorDefecto();
        }

     
        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnMinimizar_Click(object sender, EventArgs e)
        {
            animator.Minimize(); // llamamos la calse FormAnimator y le pasamos el metodo Minimize
        }


        private bool ValidarConversion(string imagePath, string formatoDeseado)
        {
            // Obtener la extensión actual de la imagen
            string extensionActual = Path.GetExtension(imagePath).TrimStart('.').ToLower();

            // Comparar con el formato deseado
            if (extensionActual == formatoDeseado.ToLower())
            {
                MessageBox.Show($"No puedes convertir la imagen al mismo formato ({formatoDeseado.ToUpper()}).",
                    "Conversión inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false; // no permitir conversión
            }

            return true; // válido para convertir
        }


        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Archivos de imagen|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff;*.webp|Todos los archivos|*.*";
                openFileDialog.Title = "Seleccionar Imagen";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedImagePath = openFileDialog.FileName;


                    try
                    {
                        // Limpiar panel antes de agregar nueva imagen
                        FlowLayoutPanel flowPanel = this.Controls.Find("flowPanelImages", true).FirstOrDefault() as FlowLayoutPanel;
                        if (flowPanel != null)
                            flowPanel.Controls.Clear();

                        // Mostrar imagen seleccionada dentro del FlowLayoutPanel
                        AgregarImagenAlFlowPanel(selectedImagePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al cargar la imagen: {ex.Message}",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
            }
        }


        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedImagePath))
            {
                MessageBox.Show("Por favor, selecciona una imagen primero.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ComboBox cbFormat = this.Controls.Find("cbFormat", true).FirstOrDefault() as ComboBox;
            if (cbFormat == null)
            {
                MessageBox.Show("No se encontró el ComboBox 'cbFormat'.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string selectedFormat = cbFormat.SelectedItem?.ToString()?.ToLower();
            if (string.IsNullOrEmpty(selectedFormat))
            {
                MessageBox.Show("Selecciona un formato de salida.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ✅ Validar conversión antes de continuar
            if (!ValidarConversion(selectedImagePath, selectedFormat))
                return;


            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = $"{selectedFormat.ToUpper()}|*.{selectedFormat}|Todos los archivos|*.*";
                saveFileDialog.Title = "Guardar Imagen Convertida";
                saveFileDialog.FileName = Path.GetFileNameWithoutExtension(selectedImagePath) + "_converted";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (MagickImage image = new MagickImage(selectedImagePath))
                        {
                            switch (selectedFormat)
                            {
                                case "png":
                                    image.Format = MagickFormat.Png;
                                    break;
                                case "jpeg":
                                case "jpg":
                                    image.Format = MagickFormat.Jpeg;
                                    image.Quality = 90;
                                    break;
                                case "bmp":
                                    image.Format = MagickFormat.Bmp;
                                    break;
                                case "gif":
                                    image.Format = MagickFormat.Gif;
                                    break;
                                case "tiff":
                                    image.Format = MagickFormat.Tiff;
                                    break;
                                case "webp":
                                    image.Format = MagickFormat.WebP;
                                    image.Quality = 90;
                                    break;
                                default:
                                    throw new Exception("Formato no soportado.");
                            }

                            // Guardar imagen convertida
                            image.Write(saveFileDialog.FileName);
                        }

                        MessageBox.Show("Imagen guardada exitosamente.", "Éxito",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LimpiarCampos();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al guardar la imagen: {ex.Message}",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        public void LimpiarCampos()
        {
            // Limpiar ruta seleccionada
            selectedImagePath = null;

            // Limpiar el ComboBox (si existe)
            ComboBox cbFormat = this.Controls.Find("cbFormat", true).FirstOrDefault() as ComboBox;
            if (cbFormat != null && cbFormat.Items.Count > 0)
                cbFormat.SelectedIndex = 0;

            // Limpiar el FlowLayoutPanel
            FlowLayoutPanel flowPanel = this.Controls.Find("flowPanelImages", true).FirstOrDefault() as FlowLayoutPanel;
            if (flowPanel != null)
            {
                flowPanel.Controls.Clear();
                // Volver a mostrar la imagen por defecto
                AgregarImagenPorDefecto();
            }
        }


        private void AgregarImagenAlFlowPanel(string imagePath)
        {
            FlowLayoutPanel flowPanel = this.Controls.Find("flowPanelImages", true).FirstOrDefault() as FlowLayoutPanel;
            if (flowPanel == null)
            {
                MessageBox.Show("No se encontró el FlowLayoutPanel 'flowPanelImages'.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Extraer información del archivo
            FileInfo fileInfo = new FileInfo(imagePath);
            string carpeta = fileInfo.DirectoryName;
            string nombre = Path.GetFileNameWithoutExtension(imagePath);
            string extension = fileInfo.Extension.ToUpper().Trim('.');
            string fecha = fileInfo.LastWriteTime.ToString("dd/MM/yyyy HH:mm");
            double pesoKB = Math.Round(fileInfo.Length / 1024.0, 2);

            // Crear panel contenedor
            Panel panelImagen = new Panel
            {
                Width = 200,
                Height = 250,
                Margin = new Padding(10),
                BackColor = Color.FromArgb(30, 30, 30),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Miniatura
            PictureBox pictureBox = new PictureBox
            {
                Width = 180,
                Height = 140,
                SizeMode = PictureBoxSizeMode.Zoom,
                Top = 10,
                Left = 10
            };

            using (MagickImage magickImage = new MagickImage(imagePath))
            using (MemoryStream ms = new MemoryStream())
            {
                magickImage.Write(ms, MagickFormat.Png);
                ms.Position = 0;
                pictureBox.Image = System.Drawing.Image.FromStream(ms);
            }

            // Labels separados para cada dato
            Label lblCarpeta = new Label
            {
                Text = $"📁 {carpeta}",
                AutoSize = false,
                Width = 180,
                Height = 20,
                Top = 155,
                Left = 10,
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.Gray
            };

            Label lblNombre = new Label
            {
                Text = $"🖼️ {nombre}",
                AutoSize = false,
                Width = 180,
                Height = 20,
                Top = 175,
                Left = 10,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.White
            };

            Label lblPeso = new Label
            {
                Text = $"⚖️ {pesoKB} KB",
                AutoSize = false,
                Width = 180,
                Height = 20,
                Top = 195,
                Left = 10,
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray
            };

            Label lblExt = new Label
            {
                Text = $"📄 {extension}",
                AutoSize = false,
                Width = 180,
                Height = 20,
                Top = 215,
                Left = 10,
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.White
            };

            // Agregar controles al panel
            panelImagen.Controls.Add(pictureBox);
            panelImagen.Controls.Add(lblCarpeta);
            panelImagen.Controls.Add(lblNombre);
            panelImagen.Controls.Add(lblPeso);
            panelImagen.Controls.Add(lblExt);

            // Agregar panel al FlowLayoutPanel
            flowPanel.Controls.Add(panelImagen);
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarCampos();
        }

        private void AgregarImagenPorDefecto()
        {
            FlowLayoutPanel flowPanel = this.Controls.Find("flowPanelImages", true).FirstOrDefault() as FlowLayoutPanel;
            if (flowPanel == null) return;

            // Crear panel contenedor
            Panel panelImagen = new Panel
            {
                Width = 200,
                Height = 250,
                Margin = new Padding(10),
                BackColor = Color.FromArgb(30, 30, 30),
                BorderStyle = BorderStyle.FixedSingle
            };

            // PictureBox con imagen de recursos
            PictureBox pictureBox = new PictureBox
            {
                Width = 180,
                Height = 130,
                SizeMode = PictureBoxSizeMode.Zoom,
                Top = 10,
                Left = 10,
                Image = Properties.Resources.Analisando  // <-- tu imagen por defecto
            };

            // Labels informativos vacíos o genéricos
            Label lblNombre = new Label
            {
                Text = "Selleccione una imagen ",
                AutoSize = false,
                Width = 180,
                Height = 20,
                Top = 155,
                Left = 10,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.SkyBlue
            };

            // Agregar controles al panel
            panelImagen.Controls.Add(pictureBox);
            panelImagen.Controls.Add(lblNombre);

            // Agregar panel al FlowLayoutPanel
            flowPanel.Controls.Add(panelImagen);
        }

        private void PicBoxVolver_Click(object sender, EventArgs e)
        {
           Principal Princi = new Principal(); 
            Princi.Show();

            this.Close();
        }
    }
}
