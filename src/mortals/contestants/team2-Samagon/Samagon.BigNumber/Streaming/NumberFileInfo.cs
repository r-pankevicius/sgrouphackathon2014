/*****************************************************************************
Created......: 10/20/2014 7:00:02 PM
Author.......: dabu, S-GROUP Solutions

COPYRIGHT (c) 2014 S-GROUP Solutions. ALL RIGHTS RESERVED.

THE SOURCE CODE MAY BE USED AND COPIED ONLY WITH THE WRITTEN PERMISSION FROM
S-GROUP Solutions OR IN ACCORDANCE WITH THE TERMS AND CONDITIONS STIPULATED
IN THE AGREEMENT/CONTRACT UNDER WHICH THE SOURCE CODE HAVE BEEN SUPPLIED.
*****************************************************************************/

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
