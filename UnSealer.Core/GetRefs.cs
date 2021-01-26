using dnlib.DotNet;
using System.Collections.Generic;

namespace UnSealer.Core
{
    public class GetRefs
    {
        /// <summary>
        /// Loads Module In temp way To Get Refs Used
        /// </summary>
        private ModuleDefMD TempModule { set; get; }
        /// <summary>
        /// Initialize Constructor
        /// </summary>
        /// <param name="x"> Module Path </param>
        public GetRefs(string x) => this.TempModule = ModuleDefMD.Load(x);
        /// <summary>
        /// Collecting Used Refs
        /// </summary>
        /// <returns> List Of Used Refs </returns>
        public IList<string> CollectRefs() {
            var LRefs = new List<string>();
            foreach (var x in TempModule.GetAssemblyRefs())
                LRefs.Add(x.Name);
            return LRefs;
        }
    }
}