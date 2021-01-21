using System;
using System.Collections.Generic;

using System.Text;


namespace VMExample.Instructions
{
	class LdelemU1 : Base
	{
		public override void emu()
		{
			var value = All.val.valueStack.Pop();
			var array =(byte[]) All.val.valueStack.Pop();
			All.val.valueStack.Push(array[(int)value]);
		}
	}
}
