using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace gelf4net
{
    public static class Extensions
    {
        public static IDictionary ToDictionary(this object values)
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            if (values != null)
            {
                foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(values))
                {
                    object obj = propertyDescriptor.GetValue(values);
                    dict.Add(propertyDescriptor.Name, obj);
                }
            }

            return dict;
        }

        /// <summary>
        /// Truncate the message
        /// </summary>
        public static string TruncateMessage(this string message, int length)
        {
            return (message.Length > length)
                       ? message.Substring(0, length - 1)
                       : message;
        }

        /// <summary>
        /// Gzips a string
        /// </summary>
        public static byte[] GzipMessage(this string message, Encoding encoding)
        {
            byte[] buffer = encoding.GetBytes(message);
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

        public static double ToUnixTimestamp(this DateTime d)
        {
          var duration = d.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0);

          return duration.TotalSeconds;
        }

        public static DateTime FromUnixTimestamp(this double d)
        {

          var datetime = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(d*1000).ToLocalTime();

          return datetime;
        }
    }
}
