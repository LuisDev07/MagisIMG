using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MagisIMG
{
    public class BorderlessFormBase : Form
    {
        // --- Arrastre ---
        [DllImport("user32.dll")] private static extern bool ReleaseCapture();
        [DllImport("user32.dll")] private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        // --- Bordes redondeados DWM ---
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWCP_ROUND = 2;

        // --- Resize ---
        private int tolerance = 10;
        private const int WM_NCHITTEST = 0x84;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;

        // --- Estado previo al maximizar ---
        private Rectangle previousBounds;
        private bool isMaximized = false;

        // --- Botones de maximizar/restaurar ---
        private Button btnMaximize;
        private Button btnRestore;



        public BorderlessFormBase()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.White;
            this.Padding = new Padding(2);
            this.DoubleBuffered = true;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            int preference = DWMWCP_ROUND;
            DwmSetWindowAttribute(this.Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
        }

        // --- Arrastre solo desde un panel ---
        public void EnableDrag(Panel panel)
        {
            panel.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left && !isMaximized)
                {
                    ReleaseCapture();
                    SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
                }
                else if (e.Button == MouseButtons.Left && isMaximized)
                {
                    RestoreFromMaxDrag(Cursor.Position.X);
                }
            };

            panel.DoubleClick += (s, e) =>
            {
                if (isMaximized) RestoreForm();
                else MaximizeForm();
            };
        }

        // --- Botones básicos ---
        // public void AttachClose(Button btn)
        //{
        //    btn.Click += (s, e) =>
        //    {
        // Animación de cierre
        //FormAnimator.Close(this);

        // Cerrar formulario después de la animación
        //      this.Dispose();
        //   };
        //  }
        // public void AttachMinimize(Button btn)
        // {
        //    btn.Click += (s, e) =>
        //     {
        // Llamar animación
        // FormAnimator.MinimizeWithAnimation(this);
        //  };
        //  }








        public void AttachMaximize(Button btn)
        {
            btn.Click += (s, e) => MaximizeForm();
            btnMaximize = btn;
            btnMaximize.Visible = true;//inicialmente mostrandolo 
        }

        public void AttachRestore(Button btn)
        {
            btn.Click += (s, e) => RestoreForm();
            btnRestore = btn;
            btnRestore.Visible = false;//inicialmente oculto
        }


        public void MaximizeForm()
        {
            if (isMaximized) return;
            previousBounds = this.Bounds;
            var screen = Screen.FromHandle(this.Handle);

            AnimateBounds(this.Bounds, screen.WorkingArea);//llamamos la animacion

            isMaximized = true;
            UpdateDwmCorners(false);
            UpdateButtons();
        }

        public void RestoreForm()
        {
            if (!isMaximized) return;


            AnimateBounds(this.Bounds, previousBounds);//llamamos la animacion

            isMaximized = false;
            UpdateDwmCorners(true);
            UpdateButtons();
        }


        //animacion de las ventanas maximizar y restaurar///////////////////////////////////////////////////////////
        private void AnimateBounds(Rectangle start, Rectangle end, int steps = 5)
        {
            int i = 0;
            Timer t = new Timer();
            t.Interval = 10; // velocidad
            t.Tick += (s, e) =>
            {
                i++;
                float p = (float)i / steps;
                this.Bounds = new Rectangle(
                    start.X + (int)((end.X - start.X) * p),
                    start.Y + (int)((end.Y - start.Y) * p),
                    start.Width + (int)((end.Width - start.Width) * p),
                    start.Height + (int)((end.Height - start.Height) * p)
                );

                if (i >= steps) t.Stop();
            };
            t.Start();
        }

        /// ///////////////////////////////////////////////////////////////////////////////////////////////


        // --- Restaurar mientras arrastras maximizado ---
        private void RestoreFromMaxDrag(int cursorX)
        {
            // Calcula la proporción del cursor
            float percent = (float)cursorX / this.Width;

            // Restaurar tamaño y posición
            this.Bounds = previousBounds;
            isMaximized = false;
            UpdateDwmCorners(true);
            UpdateButtons();

            // Ajustar posición según cursor
            int newX = Cursor.Position.X - (int)(this.Width * percent);
            int newY = Cursor.Position.Y - 10;
            this.Location = new Point(Math.Max(0, newX), Math.Max(0, newY));

            // Iniciar arrastre inmediatamente
            ReleaseCapture();
            SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
        }


        // --- Bordes redondeados según estado ---
        private void UpdateDwmCorners(bool enable)
        {
            int preference = enable ? DWMWCP_ROUND : 0;
            DwmSetWindowAttribute(this.Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
        }

        // --- Mostrar/ocultar botones según estado ---
        private void UpdateButtons()
        {
            if (btnMaximize != null) btnMaximize.Visible = !isMaximized;
            if (btnRestore != null) btnRestore.Visible = isMaximized;
        }

        // --- Redimensionar ---
        protected override void WndProc(ref Message m)
        {


            if (m.Msg == WM_NCHITTEST)
            {
                base.WndProc(ref m);

                if (isMaximized)
                {
                    // No redimensionar, no mover desde bordes
                    return;
                }

                var cursor = this.PointToClient(new Point(m.LParam.ToInt32() & 0xFFFF, m.LParam.ToInt32() >> 16));

                // Esquinas
                if (cursor.X <= tolerance && cursor.Y <= tolerance) { m.Result = (IntPtr)HTTOPLEFT; return; }
                if (cursor.X >= this.ClientSize.Width - tolerance && cursor.Y <= tolerance) { m.Result = (IntPtr)HTTOPRIGHT; return; }
                if (cursor.X <= tolerance && cursor.Y >= this.ClientSize.Height - tolerance) { m.Result = (IntPtr)HTBOTTOMLEFT; return; }
                if (cursor.X >= this.ClientSize.Width - tolerance && cursor.Y >= this.ClientSize.Height - tolerance) { m.Result = (IntPtr)HTBOTTOMRIGHT; return; }

                // Bordes
                if (cursor.X <= tolerance) { m.Result = (IntPtr)HTLEFT; return; }
                if (cursor.X >= this.ClientSize.Width - tolerance) { m.Result = (IntPtr)HTRIGHT; return; }
                if (cursor.Y <= tolerance) { m.Result = (IntPtr)HTTOP; return; }
                if (cursor.Y >= this.ClientSize.Height - tolerance) { m.Result = (IntPtr)HTBOTTOM; return; }

                return;
            }
            base.WndProc(ref m);
        }

    }
}
