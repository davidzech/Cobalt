using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace Cobalt.Controls
{
    class BlockDrawingVisual : DrawingVisual
    {
        public readonly VisualBlock Block;

        public BlockDrawingVisual(VisualBlock block)
        {
            Block = block;
            Redraw();
        }

        public void Redraw()
        {
            DrawingContext dc = RenderOpen();

            Block.TimeTextLine.Draw(dc, new Point(0, 0), InvertAxes.None);
            Block.NickTextLine.Draw(dc, new Point(Block.NickX, 0), InvertAxes.None);
            double yPos = 0;
            foreach (TextLine t in Block.TextLines)
            {
                t.Draw(dc, new Point(Block.TextX, yPos), InvertAxes.None);
                yPos += t.TextHeight;
            }            
            dc.Close();
        }
    }
}
