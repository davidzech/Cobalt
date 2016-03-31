using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cobalt.Controls
{
    internal partial class ChatPresenter : Control, IScrollInfo
    {
        private ScrollViewer _viewer;
        private int _bufferLines;
        private int _scrollPos;
        private bool _isAutoScrolling = true;
        private double _lineHeight = 0.0;
        private double _separatorPadding = 6.0;
        private double _columnWidth = 0.0;

        static ChatPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (ChatPresenter),
                new FrameworkPropertyMetadata(typeof (ChatPresenter)));
        }

        public ChatPresenter()
        {
            MessagesSource = new List<MessageLine>();
        }
                

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var visual = PresentationSource.FromVisual(this);
            if (visual?.CompositionTarget == null)
                return;
            var m = visual.CompositionTarget.TransformToDevice;
            var scaledPen = new Pen(null, 2.0/m.M11);
            double guidelineHeight = scaledPen.Thickness;

            double vPos = ActualHeight;
            int curLine = 0;
            var guidelines = new GuidelineSet();

            foreach (var message in MessagesSource)
            {
                // TODO cache rendering? probably not
                using (Block b = new Block {Source = message})
                {

                    b.TimeString = b.Source.Time.ToShortDateString();
                    b.NickString = b.Source.NickName;
                    b.Foreground = Brushes.Black;
                    b.Time =
                        MessageFormatter.Format(b.TimeString, null, this.ViewportWidth, this.Typeface, this.FontSize,
                            this.Foreground, this.Background).FirstOrDefault();
                        // always take the first one for time formatting
                    b.Nick =
                        MessageFormatter.Format(b.NickString, null, this.ViewportWidth - b.NickX, this.Typeface,
                            this.FontSize,
                            this.Foreground, this.Background).FirstOrDefault();

                    b.TextX = _columnWidth + _separatorPadding*2.0 + 1.0;

                    if (b.Nick != null)
                        b.NickX = _columnWidth + b.Nick.WidthIncludingTrailingWhitespace;
                    else
                        b.NickX = 0.0;

                    // draw block
                    b.Y = double.NaN;
                    if (b.Text != null && b.Text.Length > 0)
                    {
                        foreach (var line in b.Text)
                        {
                            vPos = line.Height;
                            _lineHeight = Math.Max(line.TextHeight, _lineHeight);
                        }
                        b.Height = b.Text.Sum((t) => t.Height);
                    }
                    b.Y = vPos;
                    b.Nick?.Draw(drawingContext, new Point(b.NickX, b.Y), InvertAxes.None);
                    b.Time?.Draw(drawingContext, new Point(0.0, b.Y), InvertAxes.None);
                    double accumulator = 0.0;

                    if (b.Text != null)
                    {
                        foreach (var line in b.Text)
                        {
                            line.Draw(drawingContext, new Point(b.TextX, b.Y + accumulator), InvertAxes.None);
                            accumulator += line.TextHeight;
                        }
                    }
                }
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            if (sizeInfo.WidthChanged)
            {
                InvalidateAll();
            }
            base.OnRenderSizeChanged(sizeInfo);
        }

        private void InvalidateAll()
        {
            InvalidateVisual();
            InvalidateScrollInfo();
        }
    }
}
