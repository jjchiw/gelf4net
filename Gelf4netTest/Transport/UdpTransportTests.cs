using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;
using System.IO.Compression;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Esilog.Gelf4net.Appender;

namespace Gelf4netTest.Appender
{
    [TestFixture]
    class UdpTransportTests
    {
        [Test]
        public void TestMessageId()
        {
            const int expectedLength = 8;
            var actual = GelfUdpAppender.GenerateMessageId();
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
            byte[] result = GelfUdpAppender.CreateChunkedMessagePart(messageId, index, chunkCount);

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
            byte[] result = GelfUdpAppender.CreateChunkedMessagePart(messageId, index, chunkCount);

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
            byte[] result = GelfUdpAppender.CreateChunkedMessagePart(messageId, index, chunkCount);

            // Assert
            Assert.That(result[10], Is.EqualTo(index));
            Assert.That(result[11], Is.EqualTo(chunkCount));
        }
    }
}
