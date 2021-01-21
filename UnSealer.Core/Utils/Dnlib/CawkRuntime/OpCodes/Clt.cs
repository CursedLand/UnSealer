using System;
using System.Collections.Generic;

using System.Text;


namespace VMExample.Instructions
{
	class Clt : Base
	{
		public override void emu()
		{
			var value2 = All.val.valueStack.Pop();
			var value1 = All.val.valueStack.Pop();
			if ((int)value1 < (int)value2)
			{
				
				All.val.valueStack.Push(1);
			}
			else
			{
				All.val.valueStack.Push(0);
			}
		}
	}
}
