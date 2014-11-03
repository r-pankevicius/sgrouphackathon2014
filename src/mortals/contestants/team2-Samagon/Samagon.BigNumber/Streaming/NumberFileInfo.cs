using System;
using System.Collections.Generic;
using System.Text;

namespace Samagon.BigNumber.Streaming
{
	internal sealed class NumberFileInfo
	{
		public bool IsNegative { get; set; }

		public FileRangeInfo RealPartPosition { get; set; }

		public bool IsInvalid { get; set; }
	}
}
