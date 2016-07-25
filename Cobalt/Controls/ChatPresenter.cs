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
        private int _totalLines;
        private IList<IBlock> _blocks = new List<IBlock>();
        private int _scrollPos = 0;
        private bool _isAutoScrolling = true;
        private readonly double TimeNickSeparatorPadding = 6.0;
        private readonly double SeparatorPadding = 6.0;
        private double _separatorOffsetX;

        static ChatPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (ChatPresenter),
                new FrameworkPropertyMetadata(typeof (ChatPresenter)));                       
        }

        public ChatPresenter()
        {
            MessagesSource = new List<MessageLine>();
        }

        private double LineHeight => FontSize*FontFamily.LineSpacing;

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);             

            var visual = PresentationSource.FromVisual(this);
            if (visual?.CompositionTarget == null)
                return;
            var m = visual.CompositionTarget.TransformToDevice;

            FormatAndDrawAllMessages(drawingContext);
            DrawSeparatorLine(drawingContext, m.M11);
        }

        private void DrawSeparatorLine(DrawingContext dc, double dpi = 1.0)
        {
            var guidelines = new GuidelineSet();

            double offsetX = _separatorOffsetX;
            Pen p = new Pen(Brushes.Black, 1.0 / dpi);
            guidelines.GuidelinesX.Add(Math.Ceiling(offsetX) + p.Thickness / 2);
            guidelines.GuidelinesX.Add(Math.Ceiling(offsetX + 1) + p.Thickness / 2);
            dc.PushGuidelineSet(guidelines);
            dc.DrawLine(p, new Point(offsetX, 0.0), new Point(offsetX, ActualHeight));
        }

        private void DrawBlock(DrawingContext ctx, Block b, double yPos)
        {
            
        }

        private void RenderVirtualizedMessages(DrawingContext ctx, double dpi = 1.0)
        {
            
        }

        private void FormatAndDrawAllMessages(DrawingContext ctx, double dpi = 1.0)
        {
            _blocks.Clear();
            _totalLines = 0;
            double vPos = -(_scrollPos * LineHeight);
            foreach (var message in MessagesSource)
            {
                Block b = new Block { Source = message };
                {
                    b.TimeString = b.Source.Time.ToShortDateString();
                    b.NickString = b.Source.NickName;
                    var time =
                        MessageFormatter.Format(b.TimeString, null, ViewportWidth, GetTypeFace(), FontSize,
                            Foreground, Background).First();

                    b.TimeWidth = time.WidthIncludingTrailingWhitespace;

                    var nick =
                        MessageFormatter.Format(b.NickString, null, ViewportWidth - b.NickX, GetTypeFace(),
                            FontSize,
                            Foreground, Background).FirstOrDefault();

                    b.NickWidth = nick?.WidthIncludingTrailingWhitespace ?? 0;

                    // update the separator line, pushing it out to fit longest nick
                    double testOffset = b.TimeWidth + TimeNickSeparatorPadding +
                                        b.NickWidth + SeparatorPadding;
                    if (testOffset > _separatorOffsetX)
                    {
                        _separatorOffsetX = testOffset;
                    }

                    // nick goes left of Separator
                    b.NickX = _separatorOffsetX - (SeparatorPadding + b.NickWidth);
                    // text goes right of separator
                    b.TextX = _separatorOffsetX + (SeparatorPadding);

                    var text = MessageFormatter.Format(b.Source.Text, b.Source, ViewportWidth - b.TextX, GetTypeFace(),
                        FontSize, Foreground, Background);
                    b.NumLines = text.Count;
                    _totalLines += Math.Max(1, text.Count);                    
                    b.Height = Math.Max(1, text.Count) * LineHeight;

                    // draw block                                        
                    b.Y = vPos;
                    vPos += b.Height;
                    nick?.Draw(ctx, new Point(b.NickX, b.Y), InvertAxes.None);
                    time.Draw(ctx, new Point(0.0, b.Y), InvertAxes.None);
                    double accumulator = 0.0;

                    foreach (var textLine in text)
                    {
                        textLine.Draw(ctx, new Point(b.TextX, b.Y + accumulator), InvertAxes.None);
                        accumulator += textLine.TextHeight;
                        textLine.Dispose();
                    }
                    time.Dispose();
                    _blocks.Add(b);
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
