using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroGrator
{
	public class DoomsDayException : Exception
	{
		public bool EverythingIsDoomed { get; set; }

		public DoomsDayException()
		{
		}

		public override string Message
		{
			get
			{
				if(EverythingIsDoomed)
					return "If this is thrown you should start your program again!";
				else
					return "Screw it! If this is thrown you have hope to recover!";
			}
		}
	}
}
