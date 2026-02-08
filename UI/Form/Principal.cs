using MagisIMG.Core.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MagisIMG
{
    public partial class Principal : BorderlessFormBase
    {
        private FormAnimator animator; // <---- variables para la animacion de la ventana


        ButtonHoverMoveAnimator animatorbtn = new ButtonHoverMoveAnimator(); // <---- variables para la animacion de los botones
        public Principal()
        {
           InitializeComponent();
            animator = new FormAnimator(this); // <---- le decimos que la ventana es la que queremos animar en este caso este form
            // Habilitar arrastre desde el panel
            EnableDrag(PanelHerramienta); //<--- llamamos el metodo del form base y le pasamos el panel  


            //agregamos color al titulo
            // Solo pasamos el Label, los colores ya están dentro de la clase
            HtmlColorAnimator animatorletra = new HtmlColorAnimator(lbltitle);


           
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnMinimizar_Click(object sender, EventArgs e)
        {
            animator.Minimize();
        }

        private void Principal_Load(object sender, EventArgs e)
        {
            animator.ShowWithFade(); // llamamos al metodo de animacion de la clase FormAnimator

            BotonHelper.Redondear(btnIMG, 5); //Redondeamos los botones utilizando la clase BotonHelper
            BotonHelper.Redondear(btnPDF, 5); //Redondeamos los botones utilizando la clase BotonHelper

            animatorbtn.Register(btnIMG, MoveDirection.Right);
            animatorbtn.Register(btnPDF, MoveDirection.Right);
        }

      
       

        private void btnIMG_Click(object sender, EventArgs e)
        {
            ConverterIMG IMG = new ConverterIMG();
            IMG.Show();

            this.Hide();
        }

        private void btnPDF_Click(object sender, EventArgs e)
        {
            ConverterPdf pdf = new ConverterPdf();
            pdf.Show();

            this.Hide();
        }
    }
}
