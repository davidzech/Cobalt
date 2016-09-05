using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using System.Windows.Threading;
using Caliburn.Micro;

namespace Cobalt.Controls
{
    internal partial class ChatPresenter : Control, IScrollInfo
    {        
        private ScrollViewer _viewer;
        private VisualCollection _children;
        private int _totalLines;
        private LinkedList<VisualBlock> _blocks = new LinkedList<VisualBlock>();
        private double _scrollPos = 0.0;
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
            _children = new VisualCollection(this);               
            MessagesSource = new List<MessageLine>();
        }

        protected override int VisualChildrenCount => _children.Count;

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
            {
                throw new ArgumentOutOfRangeException($"{index} is out of range. Must be between [0 - {_children.Count}");
            }
            return _children[index];
        }

        private double LineHeight => FontSize*FontFamily.LineSpacing;

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);                         

            var visual = PresentationSource.FromVisual(this);
            if (visual?.CompositionTarget == null)
                return;
            var m = visual.CompositionTarget.TransformToDevice;

            drawingContext.DrawRectangle(Background, null, new Rect(new Size(ActualWidth, ActualHeight)));
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

        private void DrawBlock(DrawingContext ctx, VisualBlock b, ref double yPos)
        {
            b.TimeTextLine?.Draw(ctx, new Point(0, yPos), InvertAxes.None);
            b.NickTextLine?.Draw(ctx, new Point(b.NickX, yPos), InvertAxes.None);

            double acc = 0.0;
            foreach (var t in b.TextLines)
            {
                t.Draw(ctx, new Point(b.TextX, yPos + acc), InvertAxes.None);
                acc += LineHeight;
            }
            yPos += b.Height;
        }

        private Tuple<LinkedListNode<VisualBlock>, int, double> FindStartingLine()
        {
            double heightAccumulator = 0.0;
            var node = _blocks.First;
            while(node != null)
            {
                var b = node.Value;
                for (int i = 0; i < b.NumLines; i++)
                {
                    if (heightAccumulator > _scrollPos)
                    {
                        return new Tuple<LinkedListNode<VisualBlock>, int, double>(node, i, heightAccumulator);
                    }
                    heightAccumulator += LineHeight;
                }
                node = node.Next;
            }
            return null;
        }

        private void DrawVirtualizedMessages(DrawingContext ctx, double dpi = 1.0)
        {
            // find the block that should be drawn at scrollPos

            Tuple<LinkedListNode<VisualBlock>, int, double> startingLine = FindStartingLine();
            if (startingLine == null)
            {
                return;
            }

            double vPos = (startingLine.Item3 - _scrollPos);

            var node = startingLine.Item1;
            while (vPos < ViewportHeight && node != null)
            {
                DrawBlock(ctx, node.Value, ref vPos);
                node = node.Next;
            }
        }

        private void PruneOutdatedMessages()
        {
            int delta = _blocks.Count - MessagesSource.Count();
            if (delta > 0)
            {
                for (var i = 0; i < delta; i++)
                {
                    _blocks.First.Value.Dispose();
                    _blocks.RemoveFirst();
                    _totalLines--;
                }
            }
        }

        private void FormatNewMessages()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (var message in MessagesSource)
            {
                if (_blocks.All(b => b.Source != message))
                {
                    VisualBlock b = new VisualBlock()
                    {
                        Source = message,
                        TimeString = message.Time.ToShortDateString(),
                        NickString = message.NickName,
                    };

                    var time =
                       MessageFormatter.Format(b.TimeString, null, ViewportWidth, GetTypeFace(), FontSize,
                           Foreground, Background).First();
                    b.TimeWidth = time.WidthIncludingTrailingWhitespace;

                    var nick =
                       MessageFormatter.Format(b.NickString, null, ViewportWidth - b.NickX, GetTypeFace(),
                           FontSize,
                           Foreground, Background).FirstOrDefault();

                    b.NickWidth = nick?.WidthIncludingTrailingWhitespace ?? 0;

                    double testOffset = b.TimeWidth + TimeNickSeparatorPadding + b.NickWidth + SeparatorPadding;
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

                    b.TimeTextLine = time;
                    b.NickTextLine = nick;
                    b.TextLines = text;

                    _blocks.AddLast(b);
                }
            }
            sw.Stop();
            Debug.WriteLine($"Elapsed={sw.Elapsed.TotalMilliseconds}");
        }

        private void InvalidateMessagesCache()
        {
            //_blocks.Clear();
            _totalLines = 0;
        }

        private void Redraw()
        {
            //InvalidateMessagesCache();
            //FormatNewMessages();
            InvalidateAll();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            if (sizeInfo.WidthChanged)
            {
                Application.Current.Dispatcher.InvokeAsync(Redraw, DispatcherPriority.ApplicationIdle);
            }
            else
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

        ~ChatPresenter()
        {
            if (_blocks != null)
            {
                InvalidateMessagesCache();
            }
            if (MessagesSource is INotifyCollectionChanged)
            {
                INotifyCollectionChanged a = MessagesSource as INotifyCollectionChanged;
                a.CollectionChanged -= Collection_CollectionChanged;
            }
            Debug.WriteLine("ChatPresenter destroyed");
        }
    }
}
