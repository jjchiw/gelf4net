using System;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace Esilog.Gelf4net.Transport
{
    public abstract class GelfTransport
    {
        public abstract void Send(string serverHostName, string serverIpAddress, int serverPort, string message);

        protected byte[] GzipMessage(String message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            var ms = new MemoryStream();
            using (var zip = new System.IO.Compression.GZipStream(ms, CompressionMode.Compress, true))
            {
                zip.Write(buffer, 0, buffer.Length);
            }
            ms.Position = 0;
            byte[] compressed = new byte[ms.Length];
            ms.Read(compressed, 0, compressed.Length);
            return compressed;

        }
    }
}
