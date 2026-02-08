using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MagisIMG
{

    public class HtmlColorAnimator
    {
        private Label label;
        private Timer timer;
        private List<Color> colors;
        private int currentColorIndex = 0;
        private int nextColorIndex = 1;
        private float t = 0f; // valor de interpolación
        private float step = 0.02f; // velocidad del cambio

        // Constructor con colores internos
        public HtmlColorAnimator(Label lbl, int interval = 50)
        {
            label = lbl;

            // Lista de colores HTML predeterminados
            List<string> htmlColors = new List<string>
        {
            "#FF0000", // rojo
            "#00FF00", // verde
            "#0000FF", // azul
            "#FFFF00", // amarillo
            "#FF00FF", // magenta
        };

            // Convertir a Color
            colors = new List<Color>();
            foreach (var c in htmlColors)
            {
                colors.Add(ColorTranslator.FromHtml(c));
            }

            timer = new Timer();
            timer.Interval = interval;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Interpolación lineal entre el color actual y el siguiente
            int r = (int)(colors[currentColorIndex].R + t * (colors[nextColorIndex].R - colors[currentColorIndex].R));
            int g = (int)(colors[currentColorIndex].G + t * (colors[nextColorIndex].G - colors[currentColorIndex].G));
            int b = (int)(colors[currentColorIndex].B + t * (colors[nextColorIndex].B - colors[currentColorIndex].B));

            label.ForeColor = Color.FromArgb(r, g, b);

            t += step;
            if (t >= 1f)
            {
                t = 0f;
                currentColorIndex = nextColorIndex;
                nextColorIndex = (nextColorIndex + 1) % colors.Count;
            }
        }

        public void Stop() => timer.Stop();
        public void Start() => timer.Start();
    }
}
