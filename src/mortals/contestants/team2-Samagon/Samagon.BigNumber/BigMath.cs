using System.Collections.Generic;
using System.Linq;

namespace Samagon.BigNumber
{
	public static class BigMath
	{
		/// <summary>
		/// Subtracts one byte array from another byte array and returns the result.
		/// </summary>
		/// <param name="left">The value to subtract from.</param>
		/// <param name="right">The value to subtract.</param>
		/// <param name="addBorrow">Defines if borrow shall be added.</param>
		/// <param name="hasBorrow">Indicates if borrow from next bigger position is used.</param>
		/// <returns>Returns subtraction result.</returns>
		public static byte[] Subtract(
			IEnumerable<byte> left,
			IEnumerable<byte> right,
			bool addBorrow,
			out bool hasBorrow)
		{
			var length = left.Count();

			var eLeft = left.Reverse().GetEnumerator();
			var eRight = right.Reverse().GetEnumerator();

			byte[] result = new byte[length];

			var index = length - 1;
			while (eLeft.MoveNext())
			{
				eRight.MoveNext();
				var sum = (byte)(eLeft.Current - eRight.Current + 48);

				if (addBorrow)
				{
					sum--;
				}

				if (sum < 48)
				{
					addBorrow = true;
					sum = (byte)(sum + 10);
				}
				else
				{
					addBorrow = false;
				}

				result[index] = sum;

				index--;
			}

			hasBorrow = addBorrow;

			return result;
		}

		/// <summary>
		/// Adds two byte arrays and returns the result.
		/// </summary>
		/// <param name="left">The first value to add.</param>
		/// <param name="right">The second value to add.</param>
		/// <param name="addCarry">Defines if carry shall be added.</param>
		/// <param name="hasCarry">Indicates if carry from previous number is used.</param>
		/// <returns>The sum of left and right.</returns>
		public static byte[] Add(
			IEnumerable<byte> left,
			IEnumerable<byte> right,
			bool addCarry,
			out bool hasCarry)
		{
			var length = left.Count();

			var eLeft = left.Reverse().GetEnumerator();
			var eRight = right.Reverse().GetEnumerator();

			byte[] result = new byte[length];

			int i = length - 1;

			while (eLeft.MoveNext())
			{
				eRight.MoveNext();

				byte sum = (byte)(eLeft.Current + eRight.Current - 48);

				if (addCarry)
				{
					sum++;
				}

				if (sum > 57)
				{
					addCarry = true;
					result[i] = (byte)(sum - 10);
				}
				else
				{
					result[i] = sum;
					addCarry = false;
				}

				i--;
			}

			hasCarry = addCarry;

			return result;
		}
	}
}
