using System;
using System.Collections.Generic;

using System.Text;


namespace VMExample.Instructions
{
	class Ceq : Base
	{
		public override void emu()
		{
			var val1 = All.val.valueStack.Pop();
			var val2 = All.val.valueStack.Pop();
			All.val.valueStack.Push(val1 == val2 ? 1 : 0);
		}
	}
}
