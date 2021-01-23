using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Linq;
using System.Runtime.InteropServices;
using UnSealer.Core;

namespace UnSealer.Protections.Dnlib
{
    public class SizeOfFixer : Protection
    {
        public override string Name => "SizeOf Fixer";

        public override string Author => "CursedLand";

        public override ProtectionType Type => ProtectionType.Dnlib;

        public override string Description => "sizeof() Fixer Tool.";

        public override void Execute(Context Context)
        {
            foreach (var TypeDef in Context.DnModule.Types.Where(x => x.HasMethods))
            {
                foreach (var MethodDef in TypeDef.Methods.Where(x => x.HasBody))
                {
                    var IL = MethodDef.Body.Instructions;
                    foreach (var Inst in IL)
                    {
                        if (Inst.OpCode == OpCodes.Sizeof)
                        {
                            var Size = Marshal.SizeOf(System.Type.GetType(((ITypeDefOrRef)Inst.Operand).ReflectionFullName));
                            Inst.OpCode = OpCodes.Ldc_I4;
                            Inst.Operand = Size;
                        }
                    }
                }
            }
        }
    }
}