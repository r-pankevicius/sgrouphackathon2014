using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samagon.BigNumber
{
	public class Arguments
	{
		#region Constructors

		public Arguments(bool isFast)
		{
			IsFast = isFast;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Input file, that will be used as the first operand.
		/// </summary>
		public string Input1 { get; set; }

		/// <summary>
		/// Input file, that will be used as the second operand.
		/// </summary>
		public string Input2 { get; set; }

		/// <summary>
		/// Output file, where result will be written.
		/// </summary>
		public string Output { get; set; }

		/// <summary>
		/// Operation: addition or subtraction
		/// </summary>
		public Operation Operator { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public bool IsFast { get; private set; }

		#endregion

		#region Overrides

		/// <summary>
		/// Convert class instance to its string representation.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder b = new StringBuilder();

			b.AppendLine(String.Format("Input file for 1 operand: '{0}'", Input1));
			b.AppendLine(String.Format("Input file for 2 operand: '{0}'", Input2));
			b.AppendLine(String.Format("Operation will be used: '{0}'", Enum.GetName(typeof(Operation), Operator)));
			b.AppendLine(String.Format("Output file for result: '{0}'", Output));

			return b.ToString();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Validates class and collects errors and warnings.
		/// </summary>
		/// <param name="errors"></param>
		/// <param name="warnings"></param>
		public void Validate(out string[] errors, out string[] warnings)
		{
			errors = new string[] { };
			warnings = new string[] { };

			List<string> errs = new List<string>();
			List<string> wars = new List<string>();

			if (!File.Exists(Input1))
			{
				errs.Add(String.Format("Input file does not exist: '{0}'", Input1));
			}

			if (!Input1.Equals(Input2, StringComparison.OrdinalIgnoreCase) && !File.Exists(Input2))
			{
				errs.Add(String.Format("Input file does not exist: '{0}'", Input2));
			}

			if (Operator == Operation.Unknown)
			{
				errs.Add("Operation is not specified. CADDIE doesn't know what to do.");
			}

			if (File.Exists(Output))
			{
				wars.Add(String.Format("Output file already exists, it will be overwritten: '{0}'", Output));
			}

			errors = errs.ToArray();
			warnings = wars.ToArray();
		}

		#endregion
	}
}
