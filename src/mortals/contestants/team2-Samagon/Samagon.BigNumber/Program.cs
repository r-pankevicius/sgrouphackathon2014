using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using Samagon.BigNumber.Streaming;

namespace Samagon.BigNumber
{
	public class Program
	{
		#region Static Members

		private static string[] s_Options = new string[] { "-i", "-o", "-a", "-+", "-s", "--", "-f", "-god" };

		private static Timer s_Timer = new Timer(1000) { AutoReset = true };

		private static DateTime s_Duration = new DateTime();

		#endregion

		#region Implementation

		public static void Main(string[] args)
		{
			s_Timer.Elapsed += s_Timer_Elapsed;

			// -? = help
			// -h = help
			// -i = input1 input2
			// -o = output
			// -a = addition
			// -+ = addition
			// -s = subtraction
			// -- = subtraction
			// -f = fast mode
			// -god = god mode

			if (args.Length == 0)
			{
				Console.WriteLine("You don't know what you're doing? CADIE suggests you to try this:");
				Console.WriteLine();
				Console.WriteLine(ShowHelp());
				return;
			}

			// help argument will be the first, what is checked. Do not care if there is something more
			if (IsHelpAsked(args))
			{
				Console.WriteLine(ShowHelp());
				return;
			}

			List<string> argsCopy = args.ToList();

			List<string> inputs = null;
			List<string> outputs = null;
			List<Operation> operators = null;
			List<string> anything = null;
			int fastMode;
			int godMode;

			// parse arguments and extract relevant information
			ParseArguments(
				args, out inputs, out outputs, out operators, out godMode, out fastMode, out anything);
			Arguments arguments = new Arguments(fastMode > 0);

			if (inputs.Count == 0)
			{
				Console.WriteLine("Please, help CADIE and define input files.");
				Console.WriteLine();
				Console.WriteLine(ShowHelp());
				return;
			}

			if (inputs.Count > 2)
			{
				Console.WriteLine("This is too much to handle, more than 2 input files are not excepted.");
				Console.WriteLine();
				Console.WriteLine(ShowHelp());
				return;
			}

			arguments.Input1 = inputs[0];
			arguments.Input2 = inputs.Count == 1 ? inputs[0] : inputs[1];

			if (outputs.Count == 0)
			{
				Console.WriteLine("Please, help CADIE and define output file for result.");
				Console.WriteLine();
				Console.WriteLine(ShowHelp());
				return;
			}

			if (outputs.Count > 1)
			{
				Console.WriteLine("More than one output file? Are you kidding me?");
				Console.WriteLine();
				Console.WriteLine(ShowHelp());
				return;
			}

			arguments.Output = outputs[0];

			if (operators.Count > 1)
			{
				Console.WriteLine("More than one operator? Really?");
				Console.WriteLine();
				Console.WriteLine(ShowHelp());
				return;
			}

			// addition by default
			arguments.Operator = operators.Count > 0 ? operators[0] : Operation.Addition;

			if (anything.Count > 0)
			{
				Console.WriteLine(
					String.Format(
						"You are playing around with '{0}'. CADIE will ignore this.",
						String.Join(" ", anything)));
			}

			string[] errors = null;
			string[] warnings = null;

			arguments.Validate(out errors, out warnings);

			if (errors.Length > 0)
			{
				Console.WriteLine(
					String.Format("CADIE noticed some errors in your input:\n{0}",
					String.Join(Environment.NewLine, errors)));
				return;
			}

			if (warnings.Length > 0 && godMode == 0)
			{
				Console.WriteLine(String.Join(Environment.NewLine, warnings));

				if (GetConfirmation() != YesOrNo.Yes)
				{
					return;
				}
			}

			Console.WriteLine(String.Format("These inputs will be used:\n\n{0}", arguments.ToString()));

			if (godMode == 0)
			{
				if (GetConfirmation() != YesOrNo.Yes)
				{
					return;
				}
			}

			s_Timer.Start();

			errors = null;

			Calculate(arguments, out errors);

			s_Timer.Stop();

			if (errors.Length == 0)
			{
				Console.WriteLine(
					String.Format("Great success! Check file '{0}' for CADIE'S effort.", arguments.Output));
			}
			else
			{
				Console.WriteLine(
					String.Format(
						"CADIE encountered mission impossible:\n{0}",
						String.Join(Environment.NewLine, errors)));
			}

			if (godMode == 0)
			{
				Console.WriteLine("Press any key");
				Console.ReadKey();
			}
		}

		/// <summary>
		/// Check if there is help request between entered arguments.
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public static bool IsHelpAsked(string[] args)
		{
			if (args.Any(a => a.Equals("-?") || a.Equals("-h", StringComparison.OrdinalIgnoreCase)))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Parses all arguments and collects relevant information.
		/// </summary>
		/// <param name="args"></param>
		/// <param name="inputs">Input files</param>
		/// <param name="outputs">Output files</param>
		/// <param name="operators"></param>
		/// <param name="godMode"></param>
		/// <param name="anything">Anything else, what is not relevant</param>
		public static void ParseArguments(
			string[] args, out List<string> inputs, out List<string> outputs,
			out List<Operation> operators, out int godMode, out int fastMode, out List<string> anything)
		{
			inputs = new List<string>();
			outputs = new List<string>();
			operators = new List<Operation>();
			fastMode = 0;
			godMode = 0;
			anything = new List<string>();

			for (int i = 0; i < args.Length; i++)
			{
				if (args[i].Equals("-i", StringComparison.OrdinalIgnoreCase))
				{
					while (i + 1 < args.Length && !s_Options.Contains(args[i + 1].ToLower()))
					{
						inputs.Add(args[i + 1]);
						i++;
					}
				}
				else if (args[i].Equals("-o", StringComparison.OrdinalIgnoreCase))
				{
					while (i + 1 < args.Length && !s_Options.Contains(args[i + 1].ToLower()))
					{
						outputs.Add(args[i + 1]);
						i++;
					}
				}
				else if (args[i].Equals("-a", StringComparison.OrdinalIgnoreCase) || args[i].Equals("-+"))
				{
					operators.Add(Operation.Addition);
				}
				else if (args[i].Equals("-s", StringComparison.OrdinalIgnoreCase) || args[i].Equals("--"))
				{
					operators.Add(Operation.Substraction);
				}
				else if (args[i].Equals("-f"))
				{
					fastMode++;
				}
				else if (args[i].Equals("-god"))
				{
					godMode++;
				}
				else
				{
					anything.Add(args[i]);
				}
			}
		}

		#endregion

		#region Event Handlers

		private static void s_Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			s_Duration = s_Duration.AddSeconds(1);
			Console.WriteLine(String.Format("Processing: {0}", s_Duration.ToLongTimeString()));
		}

		#endregion

		#region Tests

		private static void MMTest()
		{
			Console.WriteLine("Please run regular tests!");
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Show GPL v2 license info.
		/// </summary>
		/// <returns></returns>
		private static string ShowLicense()
		{
			return
				"\n-------------------------------------------------------------------------\n" +
				"- Samagon.BigNumber version 1, Copyright (C) 2014 Samagon & CO          -\n" +
				"- Samagon.BigNumber comes with ABSOLUTELY NO WARRANTY; for details      -\n" +
				"- navigate to www.gnu.org/licenses/gpl-2.0.html. This is free software, -\n" +
				"- and you are welcome to redistribute it under certain conditions;      -\n" +
				"- navigate to www.gnu.org/licenses/gpl-2.0.html for details.            -\n" +
				"-------------------------------------------------------------------------\n";
		}

		/// <summary>
		/// Show help with possible arguments.
		/// </summary>
		/// <returns></returns>
		private static string ShowHelp()
		{
			return "-h or -?\t CADIE will help you with the possible arguments.\n\n" +
				"-i\t\t CADIE will expect 2 operands, specified in 2 input files.\n" +
				"\t\t File names shall be separated by the space. You can specify\n" +
				"\t\t only one file, then both operands will be read from the same\n" +
				"\t\t file.\n\n" +
				"-o\t\t CADIE will expect file name where to put results.\n\n" +
				"-a or -+\t indicates addition as the operation between operands.\n" +
				"\t\t This is the default operation.\n\n" +
				"-s or --\t indicates subtraction as the operation between operands.\n\n" +
				"-f\t\t fast mode (if you know your input files are correct).\n\n" +
				"-god\t\t you can feel like God, since you will not be asked for\n" +
				"\t\t annoying confirmations.";
		}

		/// <summary>
		/// Ask for confirmation, to proceed further.
		/// </summary>
		/// <returns></returns>
		private static YesOrNo GetConfirmation()
		{
			ConsoleKeyInfo answer = default(ConsoleKeyInfo);
			int counter = 0;

			while (true)
			{
				if (counter > 5)
				{
					Console.WriteLine("Ok, I got it. I'm ending this.");
					return YesOrNo.Unknown;
				}

				if (counter > 2)
				{
					Console.WriteLine("Could you please hit Y or N?");
				}

				Console.WriteLine("Will you proceed? y/n");
				answer = Console.ReadKey();
				Console.WriteLine();

				if (answer.Key == ConsoleKey.Y)
				{
					return YesOrNo.Yes;
				}

				if (answer.Key == ConsoleKey.N)
				{
					return YesOrNo.No;
				}

				counter++;
			}
		}

		/// <summary>
		/// Execute calculation of big integers.
		/// </summary>
		/// <param name="arguments"></param>
		/// <param name="errors"></param>
		private static void Calculate(Arguments arguments, out string[] errors)
		{
			errors = new string[] { };

			using (var numberFilesManager = new NumberFilesManager(arguments))
			{
				List<string> errs = new List<string>();

				try
				{
					var task = numberFilesManager.DoArithmeticOperation();
					task.Wait();

					if (task.Exception != null)
					{
						errs.Add(task.Exception.Message);
					}
				}
				catch (Exception ex)
				{
					errs.Add(ex.Message);
				}

				errors = errs.ToArray();
			}
		}

		private void GenerateNumbersInFile(string file)
		{
			//1GB file
			long sizeLimitInBytes = (long)1 * (long)1024 * (long)1024 * (long)1024;

			//random numbers
			Random rnd = new Random();

			using (var stream = new FileStream(file, FileMode.Truncate))
			using (var writer = new StreamWriter(stream, System.Text.Encoding.ASCII))
			{
				while (stream.Position < sizeLimitInBytes)
				{
					//writer.Write(rnd.Next(0, 9));

					//add 9's in file
					writer.Write(String.Empty.PadLeft(10000000, '9'));
				}
			}
		}

		#endregion
	}

	public enum YesOrNo
	{
		Unknown = 0,
		Yes,
		No
	}
}
