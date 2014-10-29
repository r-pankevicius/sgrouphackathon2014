using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace AstroGrator
{
	public class Tester
	{
		private const int TestIterationsCount = 5;
		private const string TestSuffix = "test_";
		private const string ResultSuffix = "result_";
		private const int MaxTestCount = 100;
		private const string LogFile = "log.txt";
		private static int s_CombinationsCount = 0;

		public void Test()
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			for (int i = 0; i < MaxTestCount; i++)
			{
				Cleanup();
				Initialize();
				TestCalculations();
			}
			sw.Stop();

			Console.WriteLine(sw.Elapsed);
			Console.WriteLine(s_CombinationsCount);
			Console.Read();
		}

		private void TestCalculations()
		{
			for (int i = 1; i < TestIterationsCount; i++)
			{
				string file1 = GetFileName(i, 1);
				string file2 = GetFileName(i, 2);
				string resultFile = GetFileName(i, 0, true);

				//long maxBuffer = 1024 * 1024 * 256;
				for (int bs = 2; bs < GenerateBuffer(); bs *= GenerateBufferStep(i))
				{
					Thread.Sleep(200);

					char operation = '+';

					try
					{
						//TEST SUM
						operation = '+';
						var calc = new Calculator(file1, file2, operation, "Result.txt");
						calc.ResultFile = resultFile + operation.ToString();
						calc.BufferSize = bs;
						calc.Calculate();

						CheckIfResultCorrect(file1, file2, resultFile + operation.ToString(), operation, bs);
						//Log(string.Format(string.Format("SUCCESS: {0} {1} {2}", GetNumberFromFile(file1), operation, GetNumberFromFile(file2), bs)));

						//TEST SUBSTRACT
						operation = '-';
						calc = new Calculator(file1, file2, operation, "Result.txt");
						calc.ResultFile = resultFile + operation.ToString();
						calc.BufferSize = bs;
						calc.Calculate();

						CheckIfResultCorrect(file1, file2, resultFile + operation.ToString(), operation, bs);
						//Log(string.Format(string.Format("SUCCESS: {0} {1} {2}", GetNumberFromFile(file1), operation, GetNumberFromFile(file2), bs)));

						s_CombinationsCount += 2;
					}
					catch (OutOfMemoryException)
					{
						Log(string.Format("Out of memory: {0} {1} {2}. Buffer: {4}",
							GetNumberFromFile(file1),
							operation,
							GetNumberFromFile(file2),
							bs));
					}
				}
			}
		}

		private static void Initialize()
		{
			for (int i = 0; i < TestIterationsCount; i++)
			{
				string file1 = GetFileName(i, 1);
				string file2 = GetFileName(i, 2);

				CreateTestFile(file1, GenerateInteger(i + 1));
				CreateTestFile(file2, GenerateInteger(i));
			}
		}

		private void Cleanup()
		{
			Thread.Sleep(500);

			try
			{
				for (int i = 0; i < TestIterationsCount; i++)
				{
					string file1 = GetFileName(i, 1);
					string file2 = GetFileName(i, 2);
					string resultFile = GetFileName(i, 0, true);

					if (File.Exists(file1))
						File.Delete(file1);

					if (File.Exists(file2))
						File.Delete(file2);

					if (File.Exists(resultFile + "+"))
						File.Delete(resultFile + "+");

					if (File.Exists(resultFile + "-"))
						File.Delete(resultFile + "-");
				}
			}
			catch (IOException)
			{
				Thread.Sleep(100);
				Cleanup();
			}
			catch (UnauthorizedAccessException)
			{
				Thread.Sleep(100);
				Cleanup();
			}
		}

		private static int GenerateBuffer()
		{
			Random random = new Random();
			int nextInt = random.Next(1, Int16.MaxValue);
			return nextInt;
		}

		private static BigInteger GenerateInteger(int i)
		{
			Random random = new Random();
			int generatedInt = (int)Math.Pow(2, 3 * (i + 1));
			if (generatedInt == 0)
				generatedInt = (int)Math.Pow(2, 10);

			int nextInt = random.Next(1, generatedInt);
			int sign = (int)Math.Pow(-1, nextInt);

			return sign * new BigInteger(nextInt);
		}

		private int GenerateBufferStep(int i)
		{
			if (i == 0)
				return 2;

			if (i % 2 == 0)
				return 2 * i;
			else
				return 2 * i + 1;
		}

		private static void CreateTestFile(string fileName, BigInteger integer)
		{
			if (File.Exists(fileName))
				return;

			using (StreamWriter file = new StreamWriter(fileName, true))
			{
				file.WriteLine(integer);
				file.WriteLine("RANDOMCOMMENT!");
			}
		}

		private static string GetFileName(int iteration, int number = 0, bool result = false)
		{
			if (!result)
				return TestSuffix + iteration.ToString() + "_" + number.ToString() + ".txt";
			else
				return ResultSuffix + iteration.ToString() + ".txt";
		}

		private static void CheckIfResultCorrect(string file1, string file2, string resultFile, char operation, long buffer)
		{
			BigInteger number1 = GetNumberFromFile(file1);
			BigInteger number2 = GetNumberFromFile(file2);
			BigInteger result = GetNumberFromFile(resultFile);

			if (operation == '+')
			{
				if (number1 + number2 != result)
				{
					Log(string.Format(string.Format("FAILED: {0} {1} {2}", number1, operation, number2, buffer)));
				}
			}
			else if (operation == '-')
			{
				if (number1 - number2 != result)
				{
					Log(string.Format(string.Format("FAILED: {0} {1} {2}", number1, operation, number2, buffer)));
				}
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		private static BigInteger GetNumberFromFile(string file)
		{
			string number = File.ReadLines(file).First();
			var regex = Regex.Match(number, @"\d+");
			return BigInteger.Parse(number);
		}

		private static void Log(string message)
		{
			if (!File.Exists(LogFile))
			{
				using (File.Create(LogFile))
				{

				}
			}

			using (StreamWriter fs = new StreamWriter(LogFile, true, Encoding.ASCII))
			{
				fs.WriteLine(message);
			}
		}
	}
}
