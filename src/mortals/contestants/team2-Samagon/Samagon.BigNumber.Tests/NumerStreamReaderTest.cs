using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Samagon.BigNumber.Streaming;

namespace Samagon.BigNumber.Tests
{
	[TestClass]
	public sealed class NumerStreamReaderTest
	{
		[TestMethod]
		public void NumberStreamReaderNegative()
		{
			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes("-10\n450")))
			{
				using (var numbersReader = new NumberStreamReader(stream))
				{
					numbersReader.PrepareFileInfoAsync(false).Wait();

					Assert.IsNotNull(numbersReader.Info);
					Assert.IsFalse(numbersReader.Info.IsInvalid);
					Assert.IsTrue(numbersReader.Info.IsNegative);
					Assert.IsNotNull(numbersReader.Info.RealPartPosition);
					Assert.AreEqual(1, numbersReader.Info.RealPartPosition.From);
					Assert.AreEqual(2, numbersReader.Info.RealPartPosition.To);
				}
			}
		}

		[TestMethod]
		public void NumberStreamReaderNegative2()
		{
			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes("-10\r\n450")))
			{
				using (var numbersReader = new NumberStreamReader(stream))
				{
					numbersReader.PrepareFileInfoAsync(false).Wait();

					Assert.IsNotNull(numbersReader.Info);
					Assert.IsFalse(numbersReader.Info.IsInvalid);
					Assert.IsTrue(numbersReader.Info.IsNegative);
					Assert.IsNotNull(numbersReader.Info.RealPartPosition);
					Assert.AreEqual(1, numbersReader.Info.RealPartPosition.From);
					Assert.AreEqual(2, numbersReader.Info.RealPartPosition.To);
				}
			}
		}

		[TestMethod]
		public void NumberStreamReaderNegativeWithoutDecimals()
		{
			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes("-10\n")))
			{
				using (var numbersReader = new NumberStreamReader(stream))
				{
					numbersReader.PrepareFileInfoAsync(false).Wait();

					Assert.IsNotNull(numbersReader.Info);
					Assert.IsFalse(numbersReader.Info.IsInvalid);
					Assert.IsTrue(numbersReader.Info.IsNegative);
					Assert.IsNotNull(numbersReader.Info.RealPartPosition);
					Assert.AreEqual(1, numbersReader.Info.RealPartPosition.From);
					Assert.AreEqual(2, numbersReader.Info.RealPartPosition.To);
				}
			}
		}

		[TestMethod]
		public void NumberStreamReaderNegativeAndComments()
		{
			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes("-10\n450\nBLABLAtest\n\nggsgfd\n")))
			{
				using (var numbersReader = new NumberStreamReader(stream))
				{
					numbersReader.PrepareFileInfoAsync(false).Wait();

					Assert.IsNotNull(numbersReader.Info);
					Assert.IsFalse(numbersReader.Info.IsInvalid);
					Assert.IsTrue(numbersReader.Info.IsNegative);
					Assert.IsNotNull(numbersReader.Info.RealPartPosition);
					Assert.AreEqual(1, numbersReader.Info.RealPartPosition.From);
					Assert.AreEqual(2, numbersReader.Info.RealPartPosition.To);
					;
				}
			}
		}

		[TestMethod]
		public void NumberStreamReaderNotNegative()
		{
			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes("10\n450")))
			{
				using (var numbersReader = new NumberStreamReader(stream))
				{
					numbersReader.PrepareFileInfoAsync(false).Wait();

					Assert.IsNotNull(numbersReader.Info);
					Assert.IsFalse(numbersReader.Info.IsInvalid);
					Assert.IsFalse(numbersReader.Info.IsNegative);
					Assert.IsNotNull(numbersReader.Info.RealPartPosition);
					Assert.AreEqual(0, numbersReader.Info.RealPartPosition.From);
					Assert.AreEqual(1, numbersReader.Info.RealPartPosition.To);
				}
			}
		}

		[TestMethod]
		public void NumberStreamReaderNotNegativeWithouDecimals()
		{
			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes("10\n")))
			{
				using (var numbersReader = new NumberStreamReader(stream))
				{
					numbersReader.PrepareFileInfoAsync(false).Wait();

					Assert.IsNotNull(numbersReader.Info);
					Assert.IsFalse(numbersReader.Info.IsInvalid);
					Assert.IsFalse(numbersReader.Info.IsNegative);
					Assert.IsNotNull(numbersReader.Info.RealPartPosition);
					Assert.AreEqual(0, numbersReader.Info.RealPartPosition.From);
					Assert.AreEqual(1, numbersReader.Info.RealPartPosition.To);
				}
			}
		}

		[TestMethod]
		public void NumberStreamReaderNotNegativeAndComments()
		{
			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes("10\n450\nBLABLAtest\n\nggsgfd\n")))
			{
				using (var numbersReader = new NumberStreamReader(stream))
				{
					numbersReader.PrepareFileInfoAsync(false).Wait();

					Assert.IsNotNull(numbersReader.Info);
					Assert.IsFalse(numbersReader.Info.IsInvalid);
					Assert.IsFalse(numbersReader.Info.IsNegative);
					Assert.IsNotNull(numbersReader.Info.RealPartPosition);
					Assert.AreEqual(0, numbersReader.Info.RealPartPosition.From);
					Assert.AreEqual(1, numbersReader.Info.RealPartPosition.To);
				}
			}
		}

		[TestMethod]
		public void NumberStreamReaderReadContentFromEnd()
		{
			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes("-10\n450")))
			{
				using (var numbersReader = new NumberStreamReader(stream))
				{
					numbersReader.Stream.Seek(0, SeekOrigin.End);
					var result = numbersReader.ReadContentFromEnd(2).FirstOrDefault();

					Assert.IsNotNull(result);
					Assert.AreEqual(2, result.Length);
					Assert.AreEqual(53, result[0]);
					Assert.AreEqual(48, result[1]);

					numbersReader.Stream.Seek(-1, SeekOrigin.End);
					result = numbersReader.ReadContentFromEnd(2).FirstOrDefault();

					Assert.IsNotNull(result);
					Assert.AreEqual(2, result.Length);
					Assert.AreEqual(52, result[0]);
					Assert.AreEqual(53, result[1]);

					numbersReader.Stream.Seek(1, SeekOrigin.Begin);
					result = numbersReader.ReadContentFromEnd(2).FirstOrDefault();

					Assert.IsNotNull(result);
					Assert.AreEqual(1, result.Length);
					Assert.AreEqual(NumberStreamReader.Number_Zero, result[0]);
				}
			}
		}
	}
}
