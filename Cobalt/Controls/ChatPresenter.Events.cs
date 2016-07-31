using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Cobalt.Controls
{
    internal partial class ChatPresenter
    {
        private bool _isDragging = false;

        private bool IsHoveringOverSeparator(Point p)
        {            
            return Math.Abs(p.X - (_separatorOffsetX)) < SeparatorPadding;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            var p = e.GetPosition(this);
            if (!_isDragging && IsHoveringOverSeparator(p))
            {
                _isDragging = true;
                CaptureMouse();
            }

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (!_isDragging)
            {
                Mouse.OverrideCursor = null;
            }
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var p = e.GetPosition(this);

            if (_isDragging)
            {
                _separatorOffsetX = Math.Round(Math.Max(0.0, Math.Min(ViewportWidth/2.0, p.X)));
                InvalidateAll();
            }
            else if (IsHoveringOverSeparator(p))
            {
                Mouse.OverrideCursor = Cursors.SizeWE;
            }
            else
            {
                Mouse.OverrideCursor = null;
            }
            base.OnMouseMove(e);
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            _isDragging = false;
            InvalidateVisual();
            base.OnLostMouseCapture(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                ReleaseMouseCapture();
                Mouse.OverrideCursor = null;
                InvalidateAll();
            }
            base.OnMouseLeftButtonUp(e);
        }
    }
}