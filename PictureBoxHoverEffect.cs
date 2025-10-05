using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MagisIMG
{

    public class PictureBoxHoverEffect
    {
        private PictureBox pictureBox;
        private float zoomFactor;

        private Size originalSize;
        private Point originalLocation;

        public PictureBoxHoverEffect(PictureBox pb, float zoom = 1.1f)
        {
            pictureBox = pb;
            zoomFactor = zoom;

            originalSize = pb.Size;
            originalLocation = pb.Location;

            // Suscribirse a los eventos
            pictureBox.MouseEnter += PictureBox_MouseEnter;
            pictureBox.MouseLeave += PictureBox_MouseLeave;
        }

        private void PictureBox_MouseEnter(object sender, EventArgs e)
        {
            int newWidth = (int)(originalSize.Width * zoomFactor);
            int newHeight = (int)(originalSize.Height * zoomFactor);

            pictureBox.Size = new Size(newWidth, newHeight);
            pictureBox.Location = new Point(
                originalLocation.X - (newWidth - originalSize.Width) / 2,
                originalLocation.Y - (newHeight - originalSize.Height) / 2
            );
        }

        private void PictureBox_MouseLeave(object sender, EventArgs e)
        {
            pictureBox.Size = originalSize;
            pictureBox.Location = originalLocation;
        }
    }
}
