using System;
using System.Text;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Samagon.BigNumber.Tests
{
	[TestClass]
	public class CalculationTests
	{
		[TestMethod]
		public void AdditionTest()
		{
			string a = "1000";
			string b = "44";
			string expected = "1044";

			bool hasCarry;

			byte[] aBytes = Encoding.ASCII.GetBytes(a);
			byte[] bBytes = Encoding.ASCII.GetBytes(b.PadLeft(a.Length, '0'));

			var result = BigMath.Add(aBytes, bBytes, false, out hasCarry);

			Assert.AreEqual(expected, Encoding.ASCII.GetString(result));
			Assert.IsFalse(hasCarry, "Must not have carry");
		}

		[TestMethod]
		public void AdditionHasCarryTest()
		{
			string a = "999999999999999999999999999999999999";
			string b = "1";
			string expected = "000000000000000000000000000000000001";

			bool hasCarry;

			byte[] aBytes = Encoding.ASCII.GetBytes(a);
			byte[] bBytes = Encoding.ASCII.GetBytes(b.PadLeft(a.Length, '0'));

			var result = BigMath.Add(aBytes, bBytes, true, out hasCarry);

			Assert.AreEqual(expected, Encoding.ASCII.GetString(result));
			Assert.IsTrue(hasCarry, "Must have carry");
		}

		[TestMethod]
		public void AdditionRandomNumbersTest()
		{
			var rand = new Random();
			var rand2 = new Random();

			for (int i = 0; i < 10; i++)
			{
				var a = rand.Next(0, Int32.MaxValue);
				var b = rand.Next(0, Int32.MaxValue);

				bool hasCarry = false;

				string aString = a.ToString();
				string bString = b.ToString();

				if (aString.Length != bString.Length)
				{
					int length = Math.Max(aString.Length, bString.Length);
					aString = aString.PadLeft(length, '0');
					bString = bString.PadLeft(length, '0');
				}

				byte[] aBytes = Encoding.ASCII.GetBytes(aString);
				byte[] bBytes = Encoding.ASCII.GetBytes(bString);

				var result = BigMath.Add(aBytes, bBytes, false, out hasCarry);

				string number = Encoding.ASCII.GetString(result);

				if (hasCarry)
				{
					number = "1" + number;
				}

				Assert.AreEqual((long)a + (long)b, long.Parse(number));
			}
		}

		[TestMethod]
		public void SubtractionTest()
		{
			var a = ASCIIEncoding.Default.GetBytes("1000000");
			var b = ASCIIEncoding.Default.GetBytes("0000001");

			bool borrow;
			var result = BigMath.Subtract(a, b, false, out borrow);

			Assert.AreEqual("0999999", ASCIIEncoding.Default.GetString(result));
		}

		[TestMethod]
		public void SubtractionEqualsTest()
		{
			var a = ASCIIEncoding.Default.GetBytes("1000000");
			var b = ASCIIEncoding.Default.GetBytes("1000000");

			bool borrow;
			var result = BigMath.Subtract(a, b, false, out borrow);

			Assert.AreEqual("0000000", ASCIIEncoding.Default.GetString(result));
		}

		[TestMethod]
		public void SubtractionHasBorrowTest()
		{
			var a = ASCIIEncoding.Default.GetBytes("0001");
			var b = ASCIIEncoding.Default.GetBytes("1000");

			bool borrow;
			var result = BigMath.Subtract(a, b, false, out borrow);

			Assert.AreEqual(true, borrow);

			a = ASCIIEncoding.Default.GetBytes("10");
			b = ASCIIEncoding.Default.GetBytes("07");

			result = BigMath.Subtract(a, b, borrow, out borrow).Concat(result).ToArray();

			Assert.AreEqual("029001", ASCIIEncoding.Default.GetString(result));
		}

		[TestMethod]
		public void SubtractionShallHaventBorrowTest()
		{
			var a = ASCIIEncoding.Default.GetBytes("1001");
			var b = ASCIIEncoding.Default.GetBytes("1000");

			bool borrow;
			var result = BigMath.Subtract(a, b, false, out borrow);

			Assert.AreEqual(false, borrow);

			a = ASCIIEncoding.Default.GetBytes("10");
			b = ASCIIEncoding.Default.GetBytes("07");

			result = BigMath.Subtract(a, b, borrow, out borrow).Concat(result).ToArray();

			Assert.AreEqual("030001", ASCIIEncoding.Default.GetString(result));
		}

		[TestMethod]
		public void SubtractionRandomNumbersTest()
		{
			var rand = new Random();
			var rand2 = new Random();

			for (int i = 0; i < 10; i++)
			{
				var a = rand.Next(0, Int32.MaxValue);
				var b = rand.Next(0, Int32.MaxValue);

				if (b > a)
				{
					var tmp = a;
					b = a;
					a = tmp;
				}
			
				string aString = a.ToString();
				string bString = b.ToString();

				if (aString.Length != bString.Length)
				{
					int length = Math.Max(aString.Length, bString.Length);
					aString = aString.PadLeft(length, '0');
					bString = bString.PadLeft(length, '0');
				}

				byte[] aBytes = Encoding.ASCII.GetBytes(aString);
				byte[] bBytes = Encoding.ASCII.GetBytes(bString);

				bool hasBorrow = false;
				var result = BigMath.Subtract(aBytes, bBytes, false, out hasBorrow);

				string number = Encoding.ASCII.GetString(result);

				Assert.AreEqual((long)a - (long)b, long.Parse(number));
			}
		}
	}
}
