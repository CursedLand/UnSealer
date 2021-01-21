using System;
using System.Collections.Generic;

using System.Reflection;
using System.Text;


namespace VMExample.Instructions
{
	class Call : Base
	{
		public override void emu()
		{
			var mdtoken = All.binr.ReadInt32();
			MethodBase metho = All.mod.ResolveMethod(mdtoken);
			if (metho.IsStatic)
			{
				object[] typ = new Object[metho.GetParameters().Length];
				for (int i = 0; i < typ.Length; i++)
				{
					typ[i] = All.val.valueStack.Pop();
				}
				if (!((MethodInfo) metho).ReturnType.ToString().Contains("System.Void"))
				{
					All.val.valueStack.Push(metho.Invoke(null, typ));
				}
				else
				{
					metho.Invoke(null, typ);
				}
			}

		}
	}
}
