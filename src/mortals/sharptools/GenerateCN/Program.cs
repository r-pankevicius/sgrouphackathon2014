using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GenerateFiles
{
	class Program
	{
		const byte Low = 0x0F;
		const byte High = 0xF0;

		static void Main(string[] args)
		{
			long K = 1024;
			long M = 1024 * K;
			long G = 1024 * M;
			string root = "c:\\test\\";

			MakeTestFiles(root + "12", 12);
			MakeTestFiles(root + "256", 256);
			MakeTestFiles(root + "1k", K);
			MakeTestFiles(root + "16k", 16 * K);
			MakeTestFiles(root + "512M", 512 * M, 4);
			MakeTestFiles(root + "10G", 10L * G, 2);
		}

		public static void MakeTestFiles(string path, long digits, int samples = 8)
		{
			Directory.CreateDirectory(path);
			Directory.SetCurrentDirectory(path);

			string nameA = "a", nameB = "b", nameC = "c";

			for (int i = 0; i < samples; ++i)
			{
				string suffix = (i == 0) ? string.Empty : i.ToString();

				MakeNumberFile(nameA + suffix, digits);
				MakeNumberFile(nameB + suffix, digits);

				using (var fileA = new FileStream(nameA + suffix, FileMode.Open, FileAccess.Read))
				using (var fileB = new FileStream(nameB + suffix, FileMode.Open, FileAccess.Read))
				using (StreamWriter fileC = new StreamWriter(nameC + suffix, false, Encoding.ASCII))
				using (StreamWriter negativeFileC = new StreamWriter("-" + nameC + suffix, false, Encoding.ASCII))
				{
					negativeFileC.Write('-');

					foreach (char c in Add(fileA, fileB))
					{
						fileC.Write(c);
						negativeFileC.Write(c);
					}
				}
			}
		}

		private static void MakeNumberFile(string name, long digits)
		{
			long bytes = digits / 2;
			var rnd = RandomNumberGenerator.Create();
			var buffer = new byte[Math.Min(512, bytes)];

			using (StreamWriter positiveFile = new StreamWriter(name, false, Encoding.ASCII))
			using (StreamWriter negativeFile = new StreamWriter("-" + name, false, Encoding.ASCII))
			{
				negativeFile.Write('-');

				long remainingBytes = bytes;
				while (remainingBytes > 0)
				{
					int bytesToCopy = (int)Math.Min(remainingBytes, 512);
					rnd.GetBytes(buffer);

					for (int i = 0; i < bytesToCopy; ++i)
					{
						char digit = (char)((buffer[i] & Low) % 10 + '0');

						negativeFile.Write(digit);
						positiveFile.Write(digit);

						digit = (char)((buffer[i] & High) % 10 + '0');

						negativeFile.Write(digit);
						positiveFile.Write(digit);
					}

					remainingBytes -= bytesToCopy;
				}
			}
		}

		public static IEnumerable<char> Add(Stream first, Stream second)
		{
			IEnumerator<int> a = DigitsLowToHigh(first).GetEnumerator();
			IEnumerator<int> b = DigitsLowToHigh(second).GetEnumerator();

			string fileName = Path.GetTempFileName();
			using (Stream output = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
			{
				int overflow = 0;
				bool aNext = a.MoveNext(), bNext = b.MoveNext();

				while (aNext || bNext || overflow > 0)
				{
					int sum = overflow;
					if (aNext)
						sum += a.Current;
					if (bNext)
						sum += b.Current;

					overflow = sum / 10;

					output.WriteByte((byte)(sum % 10 + '0'));

					aNext = aNext && a.MoveNext();
					bNext = bNext && b.MoveNext();
				}

				int nextDigit;
				while ((nextDigit = ReadDigitCharBackwards(output)) != -1)
					yield return (char)nextDigit;
			}

			File.Delete(fileName);
		}

		private static IEnumerable<int> DigitsLowToHigh(Stream s)
		{
			while (ReadDigitCharForward(s) != -1)
				;

			int nextDigit;
			while ((nextDigit = ReadDigitCharBackwards(s)) != -1)
				yield return nextDigit - '0';
		}

		private static int ParseSign(Stream s)
		{
			int x = s.ReadByte();
			if (-1 == x)
				throw new InvalidOperationException("Empty number!");

			if (x == '-')
				return -1;

			if (x != '+')
				s.Seek(-1, SeekOrigin.Current);

			return 1;
		}

		private static int ReadDigitCharForward(Stream s)
		{
			int x = s.ReadByte();
			if (-1 == x)
				return -1;

			if (x == '\r' || x == '\n')
				return -1;

			return x;
		}

		private static int ReadDigitCharBackwards(Stream s)
		{
			if (s.Position == 0)
				return -1;

			s.Seek(-1, SeekOrigin.Current);
			int x = s.ReadByte();
			s.Seek(-1, SeekOrigin.Current);

			return x;
		}
	}
}
