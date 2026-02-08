using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MagisIMG
{

    public class FormAnimator
    {
        private Form targetForm;
        private Timer timer;
        private bool fadingIn = false;
        private bool fadingOut = false;
        private FormWindowState lastState;
        private double step = 0.05;
        private int interval = 30;

        public FormAnimator(Form form)
        {
            targetForm = form;
            lastState = form.WindowState;

            timer = new Timer();
            timer.Interval = interval;
            timer.Tick += Timer_Tick;

            // Detectar restaurar desde minimizado
            form.Resize += Form_Resize;
        }

        public void Minimize()
        {
            if (targetForm.WindowState != FormWindowState.Minimized && !fadingOut)
            {
                fadingOut = true;
                timer.Start();
            }
        }

        public void ShowWithFade()
        {
            targetForm.Opacity = 0;
            fadingIn = true;
            timer.Start();
            targetForm.Show();
        }

        private void Form_Resize(object sender, EventArgs e)
        {
            if (lastState == FormWindowState.Minimized && targetForm.WindowState == FormWindowState.Normal && !fadingIn)
            {
                fadingIn = true;
                targetForm.Opacity = 0;
                timer.Start();
            }

            lastState = targetForm.WindowState;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (fadingOut)
            {
                targetForm.Opacity -= step;
                if (targetForm.Opacity <= 0)
                {
                    timer.Stop();
                    fadingOut = false;
                    targetForm.WindowState = FormWindowState.Minimized;
                    targetForm.Opacity = 1;
                }
            }
            else if (fadingIn)
            {
                targetForm.Opacity += step;
                if (targetForm.Opacity >= 1)
                {
                    timer.Stop();
                    fadingIn = false;
                    targetForm.Opacity = 1;
                }
            }
        }

    }
}
