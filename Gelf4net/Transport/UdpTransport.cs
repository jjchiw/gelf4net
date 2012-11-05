using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;

namespace Esilog.Gelf4net.Transport
{
    public class UdpTransport : GelfTransport
    {
        public int MaxChunkSize { get; set; }
		
		private int _maxHeaderSize = 8;

        public override void Send(string serverHostName, string serverIpAddress, int serverPort, string message)
        {
            var ipAddress = IPAddress.Parse(serverIpAddress);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, serverPort);

            using(UdpClient udpClient = new UdpClient()){
                var gzipMessage = GzipMessage(message);

                if (MaxChunkSize < gzipMessage.Length)
                {
                    var chunkCount = (gzipMessage.Length / MaxChunkSize) + 1;
                    var messageId = GenerateMessageId(serverHostName);
                    for (int i = 0; i < chunkCount; i++)
                    {
                        var messageChunkPrefix = CreateChunkedMessagePart(messageId, i, chunkCount);
                        var skip = i * MaxChunkSize;
                        var messageChunkSuffix = gzipMessage.Skip(skip).Take(MaxChunkSize).ToArray<byte>();

                        var messageChunkFull = new byte[messageChunkPrefix.Length + messageChunkSuffix.Length];
                        messageChunkPrefix.CopyTo(messageChunkFull, 0);
                        messageChunkSuffix.CopyTo(messageChunkFull, messageChunkPrefix.Length);

                        udpClient.Send(messageChunkFull, messageChunkFull.Length, ipEndPoint);
                    }
                }
                else
                {
                    udpClient.Send(gzipMessage, gzipMessage.Length, ipEndPoint);
                }
            }
        }

        public byte[] CreateChunkedMessagePart(string messageId, int index, int chunkCount)
        {
            var result = new List<byte>();
            var gelfHeader = new byte[2] { Convert.ToByte(30), Convert.ToByte(15) };
            result.AddRange(gelfHeader);
            result.AddRange(Encoding.Default.GetBytes(messageId).ToArray<byte>());
            result.Add(Convert.ToByte(index));
			result.Add(Convert.ToByte(chunkCount));

            return result.ToArray<byte>();
        }

        public string GenerateMessageId(string serverHostName)
        {
            var md5String = String.Join("", MD5.Create().ComputeHash(Encoding.Default.GetBytes(serverHostName)).Select(it => it.ToString("x2")).ToArray<string>());
            var random = new Random((int)DateTime.Now.Ticks);
            var sb = new StringBuilder();
            var t = DateTime.Now.Ticks % 1000000000;
            var s = String.Format("{0}{1}", md5String.Substring(0, 10), md5String.Substring(20, 10));
            var r = random.Next(10000000).ToString("00000000");

            sb.Append(t);
            sb.Append(s);
            sb.Append(r);
			
			//Message ID: 8 bytes 
            return sb.ToString().Substring(0, _maxHeaderSize) ;
        }
    }
}
