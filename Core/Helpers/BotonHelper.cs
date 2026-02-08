using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MagisIMG
{
    public static class BotonHelper
    {
        /// <summary>
        /// Redondea los bordes de un botón.
        /// </summary>
        /// <param name="boton">El botón a redondear</param>
        /// <param name="radio">El radio de redondeo</param>
        public static void Redondear(Button boton, int radio)
        {
            if (boton == null)
                return;

            // Crear un GraphicsPath con bordes redondeados
            GraphicsPath path = new GraphicsPath();
            int diametro = radio * 2;
            path.StartFigure();
            path.AddArc(0, 0, diametro, diametro, 180, 90); // Esquina superior izquierda
            path.AddArc(boton.Width - diametro, 0, diametro, diametro, 270, 90); // Esquina superior derecha
            path.AddArc(boton.Width - diametro, boton.Height - diametro, diametro, diametro, 0, 90); // Esquina inferior derecha
            path.AddArc(0, boton.Height - diametro, diametro, diametro, 90, 90); // Esquina inferior izquierda
            path.CloseFigure();

            boton.Region = new Region(path);
        }
    }
}
