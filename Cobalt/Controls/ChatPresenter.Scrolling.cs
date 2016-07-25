using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cobalt.Controls
{
    internal partial class ChatPresenter
    {                
        public bool IsAutoScrolling => _isAutoScrolling;
        public bool CanHorizontallyScroll { get { return false; } set { } }
        public bool CanVerticallyScroll { get { return true; } set { } }
        public double ExtentHeight => _totalLines;
        public double ExtentWidth => ActualWidth;
        public ScrollViewer ScrollOwner { get { return _viewer; } set { _viewer = value; } }
        public double ViewportHeight { get; set; }
        public double ViewportWidth => ActualWidth;
        public double HorizontalOffset => 0.0;
        public double VerticalOffset => _scrollPos;

        public void LineUp()
        {
            ScrollTo(_scrollPos + 1);
        }

        public void LineDown()
        {
            ScrollTo(_scrollPos - 1);
        }

        public void MouseWheelUp()
        {
            ScrollTo(_scrollPos + SystemParameters.WheelScrollLines);
        }

        public void MouseWheelDown()
        {
            ScrollTo(_scrollPos - SystemParameters.WheelScrollLines);
        }

        public void PageUp()
        {
            ScrollTo(_scrollPos + VisibleLineCount - 1);
        }

        public void PageDown()
        {
            ScrollTo(_scrollPos - VisibleLineCount + 1);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (double.IsInfinity(constraint.Height))
            {
                ViewportHeight = ExtentHeight;
            }
            else
            {
                ViewportHeight = (constraint.Height / LineHeight);
            }
            InvalidateScrollInfo();
            return new Size(ViewportHeight * LineHeight, ViewportWidth);
        }

        public void ScrollTo(int pos)
        {
            pos = Math.Max(0, Math.Min(_totalLines - VisibleLineCount + 1, pos));

            var delta = (pos - _scrollPos) * LineHeight;
            _scrollPos = pos;

            InvalidateVisual();
            InvalidateScrollInfo();

            _isAutoScrolling = _scrollPos == 0;
        }

        public void SetVerticalOffset(double offset)
        {
            int pos = _totalLines - (int)((offset + ViewportHeight) / LineHeight);
            ScrollTo(pos);
        }

        public void LineLeft()
        {
        }

        public void LineRight()
        {
        }

        public void PageLeft()
        {
        }

        public void PageRight()
        {
        }

        public void MouseWheelLeft()
        {
        }

        public void MouseWheelRight()
        {
        }

        public void SetHorizontalOffset(double offset)
        {
            throw new NotImplementedException();
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            return Rect.Empty;
        }

        public void ScrollToEnd()
        {
            _scrollPos = 0;
        }

        public void InvalidateScrollInfo()
        {
            _viewer?.InvalidateScrollInfo();
        }

        public int VisibleLineCount => Convert.ToInt32(ActualHeight / LineHeight);
    }
}
