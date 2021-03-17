using dnlib.DotNet.Emit;
using System.Linq;
using UnSealer.Core;

namespace UnSealer.Protections.Dnlib
{
    public class StringLenFixer : Protection
    {
        public override string Name => "Str.Len Fixer";

        public override string Author => "CursedLand";

        public override string Description => "Fix String Length Mutations.";

        public override ProtectionType Type => ProtectionType.Dnlib;

        public override void Execute(Context Context)
        {
            foreach (var TypeDef in Context.DnModule.Types.Where(x => x.HasMethods))
            {
                foreach (var MethodDef in TypeDef.Methods.Where(x => x.HasBody && x.Body.HasInstructions))
                {
                    var IL = MethodDef.Body.Instructions;
                    for (int x = 0; x < IL.Count; x++)
                    {
                        if (IL[x].OpCode == OpCodes.Ldstr &&
                            IL[x + 1].OpCode == OpCodes.Ldlen)
                        {
                            IL[x] = Instruction.CreateLdcI4(IL[x].Operand.ToString().Length);
                            IL.RemoveAt(x + 1);
                            Context.Log.Debug($"Fixed StrLen : {IL[x].GetLdcI4Value()}");
                        }
                        if (IL[x].OpCode == OpCodes.Ldstr &&
                            (IL[x + 1].OpCode == OpCodes.Call || IL[x].OpCode == OpCodes.Callvirt) && IL[x + 1].Operand.ToString().Contains("get_Length"))
                        {
                            IL[x] = Instruction.CreateLdcI4(IL[x].Operand.ToString().Length);
                            IL.RemoveAt(x + 1);
                            Context.Log.Debug($"Fixed StrLen : {IL[x].GetLdcI4Value()}");
                        }
                    }
                }
            }
        }
    }
}