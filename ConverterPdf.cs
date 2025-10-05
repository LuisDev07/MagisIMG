using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MagisIMG
{
 

    public partial class ConverterPdf : BorderlessFormBase
    {
        private FormAnimator animator; // <---- variables para la animacion de la ventana
        string popplerPath = Path.Combine(Application.StartupPath, "Poppler", "Library", "bin", "pdftoppm.exe");
        string pdfPath = "";


        public ConverterPdf()
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

        private void PdfConverter_Load(object sender, EventArgs e)
        {

            AgregarPDFPorDefecto(); // agregamos el panel predeterminado

            animator.ShowWithFade(); // llamamos al metodo de animacion de la clase FormAnimator


            //llamamos la clase para redondear los botones
            BotonHelper.Redondear(btnseleccionar, 5);
            BotonHelper.Redondear(btnguardar, 5);
            BotonHelper.Redondear(btnLimpiar, 5);
        }

        private void btnseleccionar_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "PDF Files|*.pdf";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                pdfPath = ofd.FileName;

                FlowLayoutPanel flowPanel = this.Controls.Find("flowPanelImages", true).FirstOrDefault() as FlowLayoutPanel;
                if (flowPanel != null)
                {
                    // Eliminar panel por defecto
                    var defaultPanel = flowPanel.Controls.Cast<Control>()
                                        .FirstOrDefault(c => c is Panel p && p.Tag != null && p.Tag.ToString() == "DefaultPDF");
                    if (defaultPanel != null)
                        flowPanel.Controls.Remove(defaultPanel);

                    // Eliminar cualquier panel anterior (PDF previo)
                    foreach (Control c in flowPanel.Controls.Cast<Control>().ToList())
                    {
                        if (c is Panel p)
                        {
                            // Liberar imagen si hay PictureBox
                            foreach (Control pb in p.Controls)
                            {
                                if (pb is PictureBox picture && picture.Image != null)
                                {
                                    picture.Image.Dispose();
                                    picture.Image = null;
                                }
                            }

                            // Eliminar archivo temporal si existe
                            if (p.Tag != null)
                            {
                                string tempPath = p.Tag.ToString();
                                if (File.Exists(tempPath))
                                {
                                    try { File.Delete(tempPath); } catch { }
                                }
                            }

                            flowPanel.Controls.Remove(p);
                        }
                    }
                }

                // Agregar el nuevo PDF
                AgregarPDFAlFlowPanel(pdfPath);
            }
        }


        private void btnguardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(pdfPath))
            {
                MessageBox.Show("Selecciona un archivo PDF primero.");
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Guardar PDF como imagen";
                sfd.FileName = Path.GetFileNameWithoutExtension(pdfPath);
                sfd.Filter = "PNG Image|*.png|JPEG Image|*.jpg|TIFF Image|*.tiff";
                sfd.DefaultExt = "png";

                if (sfd.ShowDialog() != DialogResult.OK) return;

                string selectedFormat = Path.GetExtension(sfd.FileName).Replace(".", "").ToLower();
                string outputPath = sfd.FileName;

                try
                {
                    string uniqueName = Path.GetFileNameWithoutExtension(pdfPath) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string outputDir = Path.GetDirectoryName(sfd.FileName);
                    string outputPrefix = Path.Combine(outputDir, uniqueName);

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = popplerPath,
                        Arguments = $"-{selectedFormat} \"{pdfPath}\" \"{outputPrefix}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };


                    using (Process process = Process.Start(psi))
                    {
                        process.WaitForExit();
                        string errors = process.StandardError.ReadToEnd();

                        // Filtrar warnings de fuentes
                        string filteredErrors = string.Join("\n",
                            errors.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Where(line => !line.Contains("No display font"))
                        );

                        if (!string.IsNullOrEmpty(filteredErrors))
                        {
                            MessageBox.Show("Errores al convertir PDF: " + filteredErrors);
                        }
                        else
                        {
                            // Solo mostrar si no hay errores graves
                            MessageBox.Show("¡Conversión completada!");
                            LimpiarFlowLayout();
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ocurrió un error: " + ex.Message);
                }
            }
        }
        private void AgregarPDFAlFlowPanel(string pdfPath)
        {
            FlowLayoutPanel flowPanel = this.Controls.Find("flowPanelImages", true).FirstOrDefault() as FlowLayoutPanel;
            if (flowPanel == null) return;

            // ✅ Eliminar panel por defecto si existe
            var defaultPanel = flowPanel.Controls.Cast<Control>()
                                    .FirstOrDefault(c => c is Panel p && p.Tag != null && p.Tag.ToString() == "DefaultPDF");
            if (defaultPanel != null)
                flowPanel.Controls.Remove(defaultPanel);

            FileInfo fileInfo = new FileInfo(pdfPath);
            string carpeta = fileInfo.DirectoryName;
            string nombre = Path.GetFileNameWithoutExtension(pdfPath);
            string extension = fileInfo.Extension.ToUpper().Trim('.');
            double pesoKB = Math.Round(fileInfo.Length / 1024.0, 2);

            string tempImage = Path.Combine(Path.GetTempPath(), nombre + "_" + Guid.NewGuid().ToString() + "_preview.png");

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = popplerPath,
                    Arguments = $"-png -singlefile \"{pdfPath}\" \"{tempImage.Replace(".png", "")}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (Process process = Process.Start(psi))
                {
                    process.WaitForExit();
                }
            }
            catch
            {
                tempImage = null;
            }

            // Panel PDF
            Panel panelPDF = new Panel
            {
                Width = 200,
                Height = 280,
                Margin = new Padding(10),
                BackColor = Color.FromArgb(30, 30, 30),
                BorderStyle = BorderStyle.FixedSingle,
                Tag = tempImage
            };

            PictureBox pictureBox = new PictureBox
            {
                Width = 180,
                Height = 140,
                SizeMode = PictureBoxSizeMode.Zoom,
                Top = 10,
                Left = 10
            };

            if (!string.IsNullOrEmpty(tempImage) && File.Exists(tempImage))
            {
                using (var img = Image.FromFile(tempImage))
                {
                    pictureBox.Image = new Bitmap(img);
                }
                try { File.Delete(tempImage); } catch { }
                panelPDF.Tag = null;
            }

            Label lblCarpeta = new Label { Text = $"📁 {carpeta}", AutoSize = false, Width = 180, Height = 20, Top = 155, Left = 10, Font = new Font("Segoe UI", 8, FontStyle.Italic), ForeColor = Color.Gray };
            Label lblNombre = new Label { Text = $"📄 {nombre}", AutoSize = false, Width = 180, Height = 20, Top = 175, Left = 10, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.White };
            Label lblPeso = new Label { Text = $"⚖️ {pesoKB} KB", AutoSize = false, Width = 180, Height = 20, Top = 195, Left = 10, Font = new Font("Segoe UI", 8), ForeColor = Color.Gray };
            Label lblExt = new Label { Text = $"🗂️ {extension}", AutoSize = false, Width = 180, Height = 20, Top = 215, Left = 10, Font = new Font("Segoe UI", 8, FontStyle.Italic), ForeColor = Color.White };

            panelPDF.Controls.Add(pictureBox);
            panelPDF.Controls.Add(lblCarpeta);
            panelPDF.Controls.Add(lblNombre);
            panelPDF.Controls.Add(lblPeso);
            panelPDF.Controls.Add(lblExt);

            flowPanel.Controls.Add(panelPDF);
        }


        private void LimpiarFlowLayout()
        {
            FlowLayoutPanel flowPanel = this.Controls.Find("flowPanelImages", true).FirstOrDefault() as FlowLayoutPanel;
            if (flowPanel != null)
            {
                foreach (Control ctrl in flowPanel.Controls)
                {
                    if (ctrl is Panel panel)
                    {
                        // Borrar imágenes de PictureBox
                        foreach (Control c in panel.Controls)
                        {
                            if (c is PictureBox pb && pb.Image != null)
                            {
                                pb.Image.Dispose();
                                pb.Image = null;
                            }
                        }

                        // Borrar archivo temporal si existe
                        if (panel.Tag != null)
                        {
                            string tempPath = panel.Tag.ToString();
                            if (File.Exists(tempPath))
                            {
                                try { File.Delete(tempPath); } catch { }
                            }
                        }
                    }
                }

                // Limpiar controles del FlowLayoutPanel
                flowPanel.Controls.Clear();

                //  Resetear variables de estado
                pdfPath = "";        // Ya no hay PDF seleccionado
                AgregarPDFPorDefecto();
            }
        }



        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarFlowLayout();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnMinimizar_Click(object sender, EventArgs e)
        {
 
            animator.Minimize();
        }



        private void AgregarPDFPorDefecto()
        {
            FlowLayoutPanel flowPanel = this.Controls.Find("flowPanelImages", true).FirstOrDefault() as FlowLayoutPanel;
            if (flowPanel == null) return;

            Panel panelPDF = new Panel
            {
                Width = 200,
                Height = 280,
                Margin = new Padding(10),
                BackColor = Color.FromArgb(30, 30, 30),
                BorderStyle = BorderStyle.FixedSingle,
                Tag = "DefaultPDF" // <--- Aquí
            };

            PictureBox pictureBox = new PictureBox
            {
                Width = 180,
                Height = 140,
                SizeMode = PictureBoxSizeMode.Zoom,
                Top = 10,
                Left = 10,
                Image = Properties.Resources.Analisando
            };

            Label lblNombre = new Label
            {
                Text = "Seleccione un archivo",
                AutoSize = false,
                Width = 180,
                Height = 20,
                Top = 155,
                Left = 10,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.White
            };

            panelPDF.Controls.Add(pictureBox);
            panelPDF.Controls.Add(lblNombre);

            flowPanel.Controls.Add(panelPDF);
        }

        private void PicBoxVolver_Click(object sender, EventArgs e)
        {
            Principal Princi = new Principal();
            Princi.Show();

            this.Close();
        }
    }

}
