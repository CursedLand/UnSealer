using System;
using System.Collections.Generic;

using System.Reflection;


namespace VMExample.Instructions
{
	class Callvirt : Base
	{
		public static Dictionary<int,MethodBase> cache = new Dictionary<int, MethodBase>();
		public override void emu()
		{
			var mdtoken = All.binr.ReadInt32();
			

		
			var typ = new object[2];
			for (int i = typ.Length; i > 0; i--)
			{
				typ[i-1] = All.val.valueStack.Pop();

			}
				
				
			var type =(Random) All.val.valueStack.Pop();

			var a =type.Next(0, 250);
			All.val.valueStack.Push(a);
		}
	}
}
