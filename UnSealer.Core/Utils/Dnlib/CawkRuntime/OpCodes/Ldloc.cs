using System;
using System.Collections.Generic;

using System.Text;


namespace VMExample.Instructions
{
	class Ldloc : Base
	{
		public override void emu()
		{
			
			var index = All.binr.ReadInt32();
			All.val.valueStack.Push(All.val.locals[index]) ;
		}
	}
}
