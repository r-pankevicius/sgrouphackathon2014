using System;
using System.IO;
using System.Media;
using System.Reflection;
using System.Threading;

namespace AstroGrator
{
	public class Program
	{
		private static bool Test = false;
		//private static bool Test = true;

		enum FileNum
		{
			FIRST,
			SECOND,
			THIRD
		}

		public static void Main(string[] args)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("HELLO!");
			Console.WriteLine();

			if (Test)
			{
				var test = new Tester();
				test.Test();
				return;
			}

			string fileName1 = string.Empty;
			string fileName2 = string.Empty;
			string resultFileName = string.Empty;
			string operation = "+";

			if (args.Length == 0)
			{
				fileName1 = EnterFileName(FileNum.FIRST);
				operation = EnterOperation();
				fileName2 = EnterFileName(FileNum.SECOND);
				resultFileName = EnterFileName(FileNum.THIRD, true);
			}
			else if (args.Length == 4)
			{
				fileName1 = args[0];
				operation = args[1];
				fileName2 = args[2];
				resultFileName = args[3];

				if (!CheckIfValidFileName(fileName1))
				{
					Console.WriteLine("Exiting...");
					Environment.Exit(1);
				}

				if (!CheckIfValidFileName(fileName1))
				{
					Console.WriteLine("Exiting...");
					Environment.Exit(2);
				}

				if (!CheckIfValidOperation(operation))
				{
					Console.WriteLine("Exiting...");
					Environment.Exit(3);
				}
			}
			else
			{
				Console.WriteLine("Wrong number of arguments specified! Exiting...");
				Environment.Exit(0);
			}

			if (fileName1 == resultFileName || fileName2 == resultFileName)
			{
				Console.WriteLine("File1 or File2 cannot be the same as the result file!");
				Environment.Exit(0);
			}

			Console.WriteLine("Your CPU might melt down!");

			var calculator = new Calculator(fileName1, fileName2, Convert.ToChar(operation), resultFileName);
			calculator.Calculate();

			Console.WriteLine("DONE...");
			Console.WriteLine("PLEASE GIVE UP");

			// some entertainment
			//var t = new Thread(new ThreadStart(Demo));
			//t.Start();
			//Console.ReadKey();
		}

		private static void Demo()
		{
			try
			{
				string text;

				var asm = Assembly.GetExecutingAssembly();
				using (var stream = asm.GetManifestResourceStream(typeof(Program), "pinup00.txt"))
				{
					using (var tr = new StreamReader(stream))
					{
						text = tr.ReadToEnd();
					}
				}
				string[] lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

				var musicStream = asm.GetManifestResourceStream(typeof(Program), "t2.wav");
				var p = new SoundPlayer(musicStream);
				p.Load();
				p.PlayLooping();

				Console.ForegroundColor = ConsoleColor.Green;

				foreach (string line in lines)
				{
					Console.WriteLine(line);
					Thread.Sleep(500);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		private static string EnterFileName(FileNum file, bool resultFile = false)
		{
			while (true)
			{
				Console.WriteLine(string.Format(">>>@@@ Enter name of the {0} file!", file));

				string fileName = Console.ReadLine();
				if (resultFile)
					return fileName;

				if (CheckIfValidFileName(fileName))
					return fileName;
			}
		}

		private static string EnterOperation()
		{
			while (true)
			{
				Console.WriteLine(">>>@@@ Enter operation (+/-)!");

				string operation = Console.ReadLine();

				if (operation == "+")
					return "+";
				else if (operation == "-")
					return "-";
				else
					Console.WriteLine("Wrong operation specified!");
			}
		}

		private static bool CheckIfValidFileName(string fileName)
		{
			if (File.Exists(fileName))
			{
				return true;
			}
			else
			{
				Console.WriteLine(string.Format("File {0} does not exists. Get a hold of yourself!", fileName));
				return false;
			}
		}

		private static bool CheckIfValidOperation(string operation)
		{
			if (operation == "+")
				return true;
			else if (operation == "-")
				return true;
			else
			{
				Console.WriteLine(string.Format("Wrong operation: {0} is specified! Should be + or -.", operation));
				return false;
			}
		}
	}
}
