using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace Cobalt.Controls
{
    public struct Block
    {
        public MessageLine Source;
        public Brush Foreground;

        public string TimeString;
        public string NickString;

        public TextLine Time;
        public TextLine Nick;
        public TextLine[] Text;

        public int CharStart;
        public int CharEnd;
        public double Y;
        public double NickX;
        public double TextX;
        public double Height;
    }
}