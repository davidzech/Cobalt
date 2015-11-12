using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cobalt.Core.Network
{
    public static class StreamExtensions
    {
        public static async Task WriteByteAsync(this Stream stream, byte value)
        {
            byte[] buf = new byte[1];
            buf[0] = value;
            await stream.WriteAsync(buf, 0, 1).ConfigureAwait(false);            
        }       
    }
}
