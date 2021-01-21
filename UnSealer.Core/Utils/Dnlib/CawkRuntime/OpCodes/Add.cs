using System;
using System.Collections.Generic;


namespace VMExample.Instructions
{
	class Add:Base
	{
		public override void emu()
		{
			var val2 = All.val.valueStack.Pop();
			var val1 = All.val.valueStack.Pop();
			All.val.valueStack.Push((int)val1 + (int)val2);
		}
	}
}
