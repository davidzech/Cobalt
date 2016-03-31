using System;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace Cobalt.Controls
{
    public sealed class Block : IDisposable
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

        ~Block()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Time.Dispose();
                Nick.Dispose();
                foreach (var textLine in Text)
                {
                    textLine.Dispose();
                }
            }        
        }
    }
}