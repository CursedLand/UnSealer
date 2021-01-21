using System;
using System.Collections.Generic;

using System.Text;


namespace VMExample.Instructions
{
	class Pop : Base
	{
		public override void emu()
		{
			dynamic var =All.val.valueStack.Pop();
		}
	}
}
