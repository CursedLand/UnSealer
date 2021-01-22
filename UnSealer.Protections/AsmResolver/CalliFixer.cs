using AsmResolver.PE.DotNet.Cil;
using System.Linq;
using UnSealer.Core;

namespace UnSealer.Protections.AsmResolver
{
    // From : https://github.com/dr4k0nia/Unscrambler/blob/master/Unscrambler/Features/MethodFeatures/CalliReplace.cs
    // All Credits Go To drakoniа#0601
    public class CalliFixer : Protection
    {
        public override string Name => "Calli Fixer";

        public override string Author => "drakoniа";

        public override ProtectionType Type => ProtectionType.AsmResolver;

        public override string Description => "Fixing CalliCalls.";

        public override void Execute(Context Context)
        {
            foreach (var Type in Context.AsmModule.GetAllTypes())
            {
                foreach (var Method in Type.Methods.Where(x => x.CilMethodBody != null))
                {
                    var IL = Method.CilMethodBody.Instructions;
                    for (int i = 0; i < IL.Count; i++)
                    {
                        if (IL[i].OpCode != CilOpCodes.Ldftn || IL[i + 1].OpCode != CilOpCodes.Calli)
                            continue;

                        // Change ldftn to call and remove the calli opcode
                        IL[i].OpCode = CilOpCodes.Call;
                        IL[i + 1].OpCode = CilOpCodes.Nop;
                    }
                }
            }
        }
    }
}