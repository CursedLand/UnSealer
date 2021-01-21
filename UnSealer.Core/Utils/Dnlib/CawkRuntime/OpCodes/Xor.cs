using System;
using System.Collections.Generic;

using System.Text;


namespace VMExample.Instructions
{
	class Xor : Base
	{
		public override void emu()
		{
			var val2 = All.val.valueStack.Pop();
			var val1 = All.val.valueStack.Pop();
			var a = Convert.ToByte(val1) ^ (int)val2;
			All.val.valueStack.Push(a);
		}
	}
}
