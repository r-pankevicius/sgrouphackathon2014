using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samagon.BigNumber.Streaming
{
	internal sealed class FileRangeInfo
	{
		public long From { get; set; }
		public long To { get; set; }
		public long GetLength()
		{
			return  To - From + 1;
		}
	}
}
