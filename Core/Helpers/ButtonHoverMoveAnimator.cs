using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MagisIMG.Core.Helpers
{

    public enum MoveDirection
    {
        Left,
        Right
    }

    public class ButtonHoverMoveAnimator
    {
        private readonly Timer _timer;

        private readonly Dictionary<Button, Point> _originalLocation =
            new Dictionary<Button, Point>();

        private readonly Dictionary<Button, MoveDirection> _directions =
            new Dictionary<Button, MoveDirection>();

        private Button _currentButton;
        private bool _hover;

        private const int Step = 1;   // velocidad
        private const int Offset = 8; // desplazamiento

        public ButtonHoverMoveAnimator()
        {
            _timer = new Timer();
            _timer.Interval = 15;
            _timer.Tick += Animate;
        }

        public void Register(Button btn, MoveDirection direction)
        {
            _originalLocation[btn] = btn.Location;
            _directions[btn] = direction;

            btn.MouseEnter += (s, e) =>
            {
                _currentButton = btn;
                _hover = true;
                _timer.Start();
            };

            btn.MouseLeave += (s, e) =>
            {
                _currentButton = btn;
                _hover = false;
                _timer.Start();
            };
        }

        private void Animate(object sender, EventArgs e)
        {
            if (_currentButton == null) return;

            Point original = _originalLocation[_currentButton];
            Point current = _currentButton.Location;

            int dir = _directions[_currentButton] == MoveDirection.Right ? 1 : -1;

            int targetX = _hover
                ? original.X + (Offset * dir)
                : original.X;

            if (current.X < targetX)
            {
                current.X += Step;
                if (current.X > targetX) current.X = targetX;
            }
            else if (current.X > targetX)
            {
                current.X -= Step;
                if (current.X < targetX) current.X = targetX;
            }
            else
            {
                _timer.Stop();
            }

            _currentButton.Location = current;
        }
    }
}
