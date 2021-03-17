using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Linq;
using System.Reflection.Emit;
using UnSealer.Core;
using OpCodes = System.Reflection.Emit.OpCodes;

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
                        if (Inst.OpCode == dnlib.DotNet.Emit.OpCodes.Sizeof)
                        {
                            // More Comptapility.
                            var Dynamic = new DynamicMethod(string.Empty, typeof(int), new[]
                            {
                                typeof(int)
                            });
                            var ILGen = Dynamic.GetILGenerator();
                            ILGen.Emit(OpCodes.Sizeof, Context.SysModule.ResolveType(((ITypeDefOrRef)Inst.Operand).MDToken.ToInt32()));
                            ILGen.Emit(OpCodes.Ret);
                            var i = Instruction.CreateLdcI4((int)Dynamic.Invoke(null, null));
                            Inst.OpCode = i.OpCode;
                            Inst.Operand = i.Operand;
                        }
                    }
                }
            }
        }
    }
}