using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Security.Cryptography;
using Esilog.Gelf4net.Transport;
using System.Net;
using System.Net.Sockets;
using System.IO.Compression;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace Gelf4netTest.Appender
{
    [TestFixture]
    class UdpTransportTests
    {
        [Test]
        public void TestMessageId()
        {
            // Arrange
            string hostName = "localhost";

            // Act
            string actual = new UdpTransport(hostName, "127.0.0.1", 0).GenerateMessageId();

            // Assert
            const int expectedLength = 8;
            Assert.AreEqual(actual.Length, expectedLength);
        }

        [Test]
        public void CreateChunkedMessagePart_StartsWithCorrectHeader()
        {
            // Arrange
            string messageId = "A1B2C3D4";
            int index = 1;
            int chunkCount = 1;

            // Act
            byte[] result = new UdpTransport("localhost", "127.0.0.1", 0).CreateChunkedMessagePart(messageId, index, chunkCount);

            // Assert
            Assert.That(result[0], Is.EqualTo(30));
            Assert.That(result[1], Is.EqualTo(15));
        }

        [Test]
        public void CreateChunkedMessagePart_ContainsMessageId()
        {
            // Arrange
            string messageId = "A1B2C3D4";
            int index = 1;
            int chunkCount = 1;

            // Act
            byte[] result = new UdpTransport("localhost", "127.0.0.1", 0).CreateChunkedMessagePart(messageId, index, chunkCount);

            // Assert
            Assert.That(result[2], Is.EqualTo((int)'A'));
            Assert.That(result[3], Is.EqualTo((int)'1'));
            Assert.That(result[4], Is.EqualTo((int)'B'));
            Assert.That(result[5], Is.EqualTo((int)'2'));
            Assert.That(result[6], Is.EqualTo((int)'C'));
            Assert.That(result[7], Is.EqualTo((int)'3'));
            Assert.That(result[8], Is.EqualTo((int)'D'));
            Assert.That(result[9], Is.EqualTo((int)'4'));
        }

        [Test]
        public void CreateChunkedMessagePart_EndsWithIndexAndCount()
        {
            // Arrange
            string messageId = "A1B2C3D4";
            int index = 1;
            int chunkCount = 2;

            // Act
            byte[] result = new UdpTransport("localhost", "127.0.0.1", 0).CreateChunkedMessagePart(messageId, index, chunkCount);

            // Assert
            Assert.That(result[10], Is.EqualTo(index));
            Assert.That(result[11], Is.EqualTo(chunkCount));
        }

        [Test]
        public void SendsMessage()
        {
            string receivedMessage = null;
            var endPoint = new IPEndPoint(IPAddress.Any, 0);
            var client = new UdpClient(endPoint);
            var port = ((IPEndPoint)client.Client.LocalEndPoint).Port;
            client.BeginReceive(new AsyncCallback(result =>
            {
                UdpClient u = (UdpClient)((UdpState)(result.AsyncState)).Client;
                IPEndPoint e = (IPEndPoint)((UdpState)(result.AsyncState)).Endpoint;

                Byte[] receiveBytes = u.EndReceive(result, ref e);
                receivedMessage = Encoding.UTF8.GetString(Decompress(receiveBytes));
            }), new UdpState
            {
                Endpoint = endPoint,
                Client = client
            });

            var messageToSend = "THIS IS A TEST!!!";
            var transport = new UdpTransport(Environment.MachineName, "127.0.0.1", port);
            transport.MaxChunkSize = 1024;

            transport.Send(messageToSend);

            var stopWatch = Stopwatch.StartNew();
            while (receivedMessage == null || stopWatch.ElapsedMilliseconds < 10000)
            {
                Thread.Sleep(100);
            }

            Assert.AreEqual(messageToSend, receivedMessage);
        }

        private static byte[] Decompress(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }
    }
}
