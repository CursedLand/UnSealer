
#region Usings
using System.Collections.Generic;
#endregion

namespace UnSealer.Protections.Devirtualizers.CawkVM
{
    public class DevirtualizationContext
    {
        public IList<MethodInfo> VirtualizatedMethods { set; get; }
        public byte[] Data { set; get; }
    }
}