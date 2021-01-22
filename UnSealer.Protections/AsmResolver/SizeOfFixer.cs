using AsmResolver.DotNet;
using AsmResolver.DotNet.Memory;
using AsmResolver.PE.DotNet.Cil;
using System.Linq;
using UnSealer.Core;

namespace UnSealer.Protections.AsmResolver
{
    // From : https://github.com/dr4k0nia/Unscrambler/blob/master/Unscrambler/Features/MethodFeatures/SizeOfReplace.cs
    // All Credits Go To drakoniа#0601
    public class SizeOfFixer : Protection
    {
        public override string Name => "SizeOf Fixer";

        public override string Author => "drakoniа";

        public override ProtectionType Type => ProtectionType.AsmResolver;

        public override string Description => "sizeof() Fixer Tool.";

        public override void Execute(Context Context)
        {
            foreach (var Type in Context.AsmModule.GetAllTypes())
            {
                foreach (var Method in Type.Methods.Where(x => x.CilMethodBody != null))
                {
                    bool is32Bit = Method.Module.IsBit32Preferred || Method.Module.IsBit32Required;

                    var instr = Method.CilMethodBody.Instructions;
                    for (int i = 0; i < instr.Count; i++)
                    {
                        // Search for Sizeof opcode
                        if (instr[i].OpCode != CilOpCodes.Sizeof)
                            continue;

                        var op = (ITypeDefOrRef)instr[i].Operand;
                        // Determine integer value of operand type
                        int value = (int)op.GetImpliedMemoryLayout(is32Bit).Size;

                        instr[i].OpCode = CilOpCodes.Ldc_I4;
                        instr[i].Operand = value;

                        // Optimize IL
                        instr.OptimizeMacros();
                    }
                }
            }
        }
    }
}