using System;
using System.Collections.Generic;

using System.Reflection;
using System.Text;



namespace IL_Emulator_Dynamic
{
	public class ValueStack
	{
		public Stack<object>valueStack = new Stack<dynamic>();
		public object[] locals;
		public Dictionary<FieldInfo, object> fields = new Dictionary<FieldInfo, dynamic>();
		public object[] parameters;
	}
}
