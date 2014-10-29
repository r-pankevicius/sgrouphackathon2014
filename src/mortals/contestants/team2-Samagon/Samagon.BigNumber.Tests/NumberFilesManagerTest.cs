using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Samagon.BigNumber.Streaming;

namespace Samagon.BigNumber.Tests
{
	[TestClass]
	public sealed class NumberFilesManagerTest
	{
		#region Bigger Number tests

		[TestMethod]
		public void TestBiggerNumberSameNumbers()
		{
			using (var stream1 = new MemoryStream(Encoding.ASCII.GetBytes("10\n450")))
			using (var numbersReader1 = PrepareStreamReader(stream1))
			using (var stream2 = new MemoryStream(Encoding.ASCII.GetBytes("10\n450")))
			using (var numbersReader2 = PrepareStreamReader(stream2))
			{
				Assert.IsNull(NumberFilesManager.FirstIsBigger(numbersReader1, numbersReader2));
			}
		}

		[TestMethod]
		public void TestBiggerNumberSameNumbersBothNegative()
		{
			using (var stream1 = new MemoryStream(Encoding.ASCII.GetBytes("-10\n450")))
			using (var numbersReader1 = PrepareStreamReader(stream1))
			using (var stream2 = new MemoryStream(Encoding.ASCII.GetBytes("-10\n450")))
			using (var numbersReader2 = PrepareStreamReader(stream2))
			{
				Assert.IsNull(NumberFilesManager.FirstIsBigger(numbersReader1, numbersReader2));
			}
		}

		[TestMethod]
		public void TestBiggerNumberSameNumbersButOneNegative()
		{
			using (var stream1 = new MemoryStream(Encoding.ASCII.GetBytes("-10\n450")))
			using (var numbersReader1 = PrepareStreamReader(stream1))
			using (var stream2 = new MemoryStream(Encoding.ASCII.GetBytes("10\n450")))
			using (var numbersReader2 = PrepareStreamReader(stream2))
			{
				Assert.IsNull(NumberFilesManager.FirstIsBigger(numbersReader1, numbersReader2));
			}
		}

		[TestMethod]
		public void TestBiggerNumberFirstBigger()
		{
			using (var stream1 = new MemoryStream(Encoding.ASCII.GetBytes("11\n450")))
			using (var numbersReader1 = PrepareStreamReader(stream1))
			using (var stream2 = new MemoryStream(Encoding.ASCII.GetBytes("10\n450")))
			using (var numbersReader2 = PrepareStreamReader(stream2))
			{
				var result = NumberFilesManager.FirstIsBigger(numbersReader1, numbersReader2);

				Assert.IsNotNull(result);
				Assert.IsTrue(result.Value);
			}
		}

		[TestMethod]
		public void TestBiggerNumberFirstBiggerButOneNegative()
		{
			using (var stream1 = new MemoryStream(Encoding.ASCII.GetBytes("-11\n450")))
			using (var numbersReader1 = PrepareStreamReader(stream1))
			using (var stream2 = new MemoryStream(Encoding.ASCII.GetBytes("10\n450")))
			using (var numbersReader2 = PrepareStreamReader(stream2))
			{
				var result = NumberFilesManager.FirstIsBigger(numbersReader1, numbersReader2);

				Assert.IsNotNull(result);
				Assert.IsTrue(result.Value);
			}
		}

		[TestMethod]
		public void TestBiggerNumberWithoutDecimals()
		{
			using (var stream1 = new MemoryStream(Encoding.ASCII.GetBytes("11")))
			using (var numbersReader1 = PrepareStreamReader(stream1))
			using (var stream2 = new MemoryStream(Encoding.ASCII.GetBytes("10")))
			using (var numbersReader2 = PrepareStreamReader(stream2))
			{
				var result = NumberFilesManager.FirstIsBigger(numbersReader1, numbersReader2);

				Assert.IsNotNull(result);
				Assert.IsTrue(result.Value);
			}
		}

		[TestMethod]
		public void TestBiggerNumberOnlyOneWithoutDecimals()
		{
			using (var stream1 = new MemoryStream(Encoding.ASCII.GetBytes("11")))
			using (var numbersReader1 = PrepareStreamReader(stream1))
			using (var stream2 = new MemoryStream(Encoding.ASCII.GetBytes("10\n450")))
			using (var numbersReader2 = PrepareStreamReader(stream2))
			{
				var result = NumberFilesManager.FirstIsBigger(numbersReader1, numbersReader2);

				Assert.IsNotNull(result);
				Assert.IsTrue(result.Value);
			}
		}

		[TestMethod]
		public void TestBiggerNumberOnlyOneWithoutDecimalsOposite()
		{
			using (var stream1 = new MemoryStream(Encoding.ASCII.GetBytes("10\n450")))
			using (var numbersReader1 = PrepareStreamReader(stream1))
			using (var stream2 = new MemoryStream(Encoding.ASCII.GetBytes("11")))
			using (var numbersReader2 = PrepareStreamReader(stream2))
			{
				var result = NumberFilesManager.FirstIsBigger(numbersReader1, numbersReader2);

				Assert.IsNotNull(result);
				Assert.IsFalse(result.Value);
			}
		}

		[TestMethod]
		public void TestBiggerNumberOnlyOneWithoutDecimalsAndNegative()
		{
			using (var stream1 = new MemoryStream(Encoding.ASCII.GetBytes("-11")))
			using (var numbersReader1 = PrepareStreamReader(stream1))
			using (var stream2 = new MemoryStream(Encoding.ASCII.GetBytes("10\n450")))
			using (var numbersReader2 = PrepareStreamReader(stream2))
			{
				var result = NumberFilesManager.FirstIsBigger(numbersReader1, numbersReader2);

				Assert.IsNotNull(result);
				Assert.IsTrue(result.Value);
			}
		}

		#endregion

		#region Full Workflow tests

		[TestMethod]
		public void FullWorkflowTest_1And41_Addition()
		{
			using (var stream1 = new MemoryStream(Encoding.ASCII.GetBytes("1\r\nasdd")))
			using (var numbersReader1 = new NumberStreamReader(stream1))
			using (var stream2 = new MemoryStream(Encoding.ASCII.GetBytes("41\r\nasdas")))
			using (var numbersReader2 = new NumberStreamReader(stream2))
			using (var resultStream = new MemoryStream())
			{
				NumberFilesManager manager = new NumberFilesManager(
					stream1, stream2, resultStream, Operation.Addition);
				manager.DoArithmeticOperation().Wait();
				//Assert.AreEqual(1, resultStream.Length);
				resultStream.Seek(0, SeekOrigin.Begin);

				using (var streamReader = new StreamReader(resultStream))
					Assert.AreEqual("42", streamReader.ReadLine().TrimStart('0'));
			}
		}


		[TestMethod]
		public void FullWorkflowTest_3And1_Addition()
		{
			using (var stream1 = new MemoryStream(Encoding.ASCII.GetBytes("3")))
			using (var numbersReader1 = new NumberStreamReader(stream1))
			using (var stream2 = new MemoryStream(Encoding.ASCII.GetBytes("1")))
			using (var numbersReader2 = new NumberStreamReader(stream2))
			using (var resultStream = new MemoryStream())
			{
				NumberFilesManager manager = new NumberFilesManager(
					stream1, stream2, resultStream, Operation.Addition);
				manager.DoArithmeticOperation().Wait();
				//Assert.AreEqual(1, resultStream.Length);
				resultStream.Seek(0, SeekOrigin.Begin);

				using (var streamReader = new StreamReader(resultStream))
					Assert.AreEqual("4", streamReader.ReadLine().TrimStart('0'));
			}
		}

		[TestMethod]
		public void FullWorkflowTest_3And1_Substraction()
		{
			using (var stream1 = new MemoryStream(Encoding.ASCII.GetBytes("3")))
			using (var numbersReader1 = new NumberStreamReader(stream1))
			using (var stream2 = new MemoryStream(Encoding.ASCII.GetBytes("1")))
			using (var numbersReader2 = new NumberStreamReader(stream2))
			using (var resultStream = new MemoryStream())
			{
				NumberFilesManager manager = new NumberFilesManager(
					stream1, stream2, resultStream, Operation.Substraction);
				manager.DoArithmeticOperation().Wait();
				//Assert.AreEqual(1, resultStream.Length);
				resultStream.Seek(0, SeekOrigin.Begin);

				using (var streamReader = new StreamReader(resultStream))
					Assert.AreEqual("2", streamReader.ReadLine().TrimStart('0'));
			}
		}

		[TestMethod]
		public void FullWorkflowTest_3And100_Addition()
		{
			using (var stream1 = new MemoryStream(Encoding.ASCII.GetBytes("3")))
			using (var numbersReader1 = new NumberStreamReader(stream1))
			using (var stream2 = new MemoryStream(Encoding.ASCII.GetBytes("100")))
			using (var numbersReader2 = new NumberStreamReader(stream2))
			using (var resultStream = new MemoryStream())
			{
				NumberFilesManager manager = new NumberFilesManager(
					stream1, stream2, resultStream, Operation.Addition);
				manager.DoArithmeticOperation().Wait();
				//Assert.AreEqual(3, resultStream.Length);
				resultStream.Seek(0, SeekOrigin.Begin);
				using (var streamReader = new StreamReader(resultStream))
					Assert.AreEqual("103", streamReader.ReadLine().TrimStart('0'));
			}
		}

		[TestMethod]
		public void FullWorkflowTest_3And100_Substraction()
		{
			using (var stream1 = new MemoryStream(Encoding.ASCII.GetBytes("+000003")))
			using (var numbersReader1 = new NumberStreamReader(stream1))
			using (var stream2 = new MemoryStream(Encoding.ASCII.GetBytes("100")))
			using (var numbersReader2 = new NumberStreamReader(stream2))
			using (var resultStream = new MemoryStream())
			{
				NumberFilesManager manager = new NumberFilesManager(
					stream1, stream2, resultStream, Operation.Substraction);
				manager.DoArithmeticOperation().Wait();
				//Assert.AreEqual(3, resultStream.Length);
				resultStream.Seek(0, SeekOrigin.Begin);
				using (var streamReader = new StreamReader(resultStream))
					Assert.AreEqual("-0097", streamReader.ReadLine().TrimStart('0'));
			}
		}

		[TestMethod]
		public void FullWorkflowTest_2longAnd1_Addition()
		{
			using (var stream1 = new MemoryStream(Encoding.ASCII.GetBytes(long.MaxValue.ToString() + long.MaxValue.ToString())))
			using (var numbersReader1 = new NumberStreamReader(stream1))
			using (var stream2 = new MemoryStream(Encoding.ASCII.GetBytes("1")))
			using (var numbersReader2 = new NumberStreamReader(stream2))
			using (var resultStream = new MemoryStream())
			{
				NumberFilesManager manager = new NumberFilesManager(
					stream1, stream2, resultStream, Operation.Addition);
				manager.DoArithmeticOperation().Wait();
				//Assert.AreEqual("92233720368547758079223372036854775806".Length, resultStream.Length);
				resultStream.Seek(0, SeekOrigin.Begin);
				using (var streamReader = new StreamReader(resultStream))
					Assert.AreEqual("92233720368547758079223372036854775808", streamReader.ReadLine().TrimStart('0'));
			}
		}

		[TestMethod]
		public void FullWorkflowTest_2longAnd1_Substraction()
		{
			using (var stream1 = new MemoryStream(Encoding.ASCII.GetBytes(long.MaxValue.ToString() + long.MaxValue.ToString())))
			using (var numbersReader1 = new NumberStreamReader(stream1))
			using (var stream2 = new MemoryStream(Encoding.ASCII.GetBytes("1")))
			using (var numbersReader2 = new NumberStreamReader(stream2))
			using (var resultStream = new MemoryStream())
			{
				NumberFilesManager manager = new NumberFilesManager(
					stream1, stream2, resultStream, Operation.Substraction);
				manager.DoArithmeticOperation().Wait();
				//Assert.AreEqual("92233720368547758079223372036854775806".TrimStart('0').Length, resultStream.Length);
				resultStream.Seek(0, SeekOrigin.Begin);
				using (var streamReader = new StreamReader(resultStream))
					Assert.AreEqual("92233720368547758079223372036854775806", streamReader.ReadLine().TrimStart('0'));
			}
		}

		#endregion

		#region Private Helpers

		private static NumberStreamReader PrepareStreamReader(Stream stream)
		{
			var reader = new NumberStreamReader(stream);

			try
			{
				reader.PrepareFileInfoAsync(false).Wait();
			}
			catch
			{
				reader.Dispose();
				throw;
			}

			return reader;
		}

		#endregion
	}
}
