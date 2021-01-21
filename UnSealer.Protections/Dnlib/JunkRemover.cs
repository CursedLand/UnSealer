using System.Linq;
using UnSealer.Core;

namespace UnSealer.Protections.Dnlib
{
    public class JunkRemover : Protection
    {
        public override string Name => "Junk Remover";

        public override string Author => "CursedLand";

        public override ProtectionType Type => ProtectionType.Dnlib;

        public override void Execute(Context Context)
        {
            if (Context.DnModule != null)
            {
                foreach (var TypeDef in Context.DnModule.Types.Where(x => !x.HasMethods).ToArray())
                {
                    Context.DnModule.Types.Remove(TypeDef);
                }
            }
        }
    }
}