using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

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

            foreach (var message in MessagesSource.Reverse())
            {
                // Render from bottom up
                // refactor this block shit to not be terrible
                using (Block b = new Block {Source = message})
                {

                    b.TimeString = b.Source.Time.ToShortDateString();
                    b.NickString = b.Source.NickName;
                    b.Foreground = Brushes.Black;
                    b.Time =
                        MessageFormatter.Format(b.TimeString, null, ViewportWidth, GetTypeFace(), FontSize,
                            Foreground, Background).First();
                        // always take the first one for time formatting
                    b.Nick =
                        MessageFormatter.Format(b.NickString, null, ViewportWidth - b.NickX, GetTypeFace(),
                            FontSize,
                            Foreground, Background).First();

                    b.TextX = _columnWidth + _separatorPadding*2.0 + 1.0;

                    if (b.Nick != null)
                        b.NickX = _columnWidth + b.Nick.WidthIncludingTrailingWhitespace;
                    else
                        b.NickX = 0.0;

                    //b.Text = MessageFormatter.Format(b.Source.Text, null )

                    b.Height = Math.Max(b.Nick.Height, b.Time.Height);

                    // draw block                                        
                    b.Y = vPos - b.Height;
                    vPos -= b.Height;                    
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
