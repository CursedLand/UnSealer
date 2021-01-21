using System;
using System.Collections.Generic;

using System.Text;


namespace VMExample.Instructions
{
	class Rem : Base
	{
		public override void emu()
		{
			var val2 =(int) All.val.valueStack.Pop();
			var val1 =(int) All.val.valueStack.Pop();
			var a = (int)val1 % val2;
			All.val.valueStack.Push(a);
		}
	}
}
