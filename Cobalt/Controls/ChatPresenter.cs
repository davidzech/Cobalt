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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cobalt.Controls
{
    internal partial class ChatPresenter : Control, IScrollInfo
    {
        private ScrollViewer _viewer;

        static ChatPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (ChatPresenter),
                new FrameworkPropertyMetadata(typeof (ChatPresenter)));
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

            foreach(var message in MessagesSource.OfType<MessageLine>())
            {
                //process
                Block b = new Block();
                b.Source = message;
                b.TimeString = b.Source.Time.ToShortDateString();
                b.NickString = b.Source.NickName;
                b.Foreground = Brushes.Black;
                var formatter = MessageFormatter.Instance;
                //var formatter = new MessageFormatter(b.TimeString, null, this.ViewportWidth, null, )
                formatter.Format(message, this.ViewportWidth);
                b.Time = formatter.Format(message)
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            if (sizeInfo.WidthChanged)
            {
                InvalidateVisual();
            }
            base.OnRenderSizeChanged(sizeInfo);
        }
    }
}
