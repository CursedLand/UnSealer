using System;
using System.Collections.Generic;
using System.Text;

namespace UnSealer.Core.Utils.Dnlib.CawkRuntime.ConversionBack
{
    //Exception handler class from the byte array
    public class ExceptionHandlerClass
    {
        public Type CatchType;
        public int FilterStart;
        public int HandlerEnd;
        public int HandlerStart;
        public int HandlerType;
        public int TryEnd;
        public int TryStart;
    }
}
