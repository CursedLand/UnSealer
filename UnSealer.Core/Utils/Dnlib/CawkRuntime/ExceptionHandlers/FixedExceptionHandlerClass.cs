using System;
using System.Collections.Generic;
using System.Text;

namespace UnSealer.Core.Utils.Dnlib.CawkRuntime.ConversionBack
{
    //fixed exception handlers for handlers that start on the same instruction
    public class FixedExceptionHandlersClass
    {
        public List<Type> CatchType = new List<Type>();
        public int FilterStart;
        public int HandlerEnd;
        public List<int> HandlerStart = new List<int>();
        public int HandlerType;
        public int TryEnd;
        public int TryStart;
    }
}
