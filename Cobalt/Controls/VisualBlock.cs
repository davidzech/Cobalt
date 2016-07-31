using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace Cobalt.Controls
{
    public class VisualBlock : IBlock, IDisposable
    {       
        public MessageLine Source { get; set; }
        public string TimeString { get; set; }
        public string NickString { get; set; }
        public int CharStart { get; set; }
        public int CharEnd { get; set; }
        public double Y { get; set; }
        public double NickX { get; set; }
        public double TextX { get; set; }
        public double Height { get; set; }
        public double TimeWidth { get; set; }
        public double NickWidth { get; set; }
        public int NumLines { get; set; }     
        public TextLine TimeTextLine { get; set; }
        public TextLine NickTextLine { get; set; }
        public IList<TextLine> TextLines { get; set; }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                TimeTextLine?.Dispose();
                TimeTextLine = null;
                NickTextLine?.Dispose();
                NickTextLine = null;

                if (TextLines != null)
                {
                    foreach (var t in TextLines)
                    {
                        t?.Dispose();                        
                    }
                    TextLines.Clear();
                    TextLines = null;
                }
               

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
         ~VisualBlock() {
           // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
           Dispose(false);
         }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);            
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}