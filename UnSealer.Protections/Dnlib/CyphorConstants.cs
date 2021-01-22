using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnSealer.Core;

namespace UnSealer.Protections.Dnlib
{
    public class CyphorConstants : Protection
    {
        public override string Name => "Cyphor Constants Decrypt";

        public override string Author => "CursedLand";

        public override ProtectionType Type => ProtectionType.Dnlib;

        public override string Description => "A Cyphor Constants Decryptor";

        public override void Execute(Context Context)
        {
            if (Context.DnModule != null && Context.SysModule != null)
            {
                var VmObject = GetVmObjectInterface(Context.SysModule.Assembly);
                foreach (var TypeDef in Context.DnModule.Types.Where(x => x.HasMethods))
                {
                    foreach (var MethodDef in TypeDef.Methods.Where(x => x.HasBody))
                    {
                        IList<Instruction> IL = MethodDef.Body.Instructions;
                        for (int x = 0; x < IL.Count; x++)
                        {
                            if (IL[x].IsLdcI4() &&
                                IL[x + 1].IsLdcI4() &&
                                IL[x + 2].OpCode == OpCodes.Call &&
                                IL[x + 2].Operand.ToString().Contains("GetConstant") &&
                                IL[x + 3].OpCode == OpCodes.Callvirt &&
                                IL[x + 3].Operand.ToString().Contains("get_FinalValue"))
                            {
                                var DecMeth = (MethodInfo)Context.SysModule.ResolveMethod((int)((IMethod)IL[x + 2].Operand).MDToken.Raw);
                                object[] Params = new object[] { IL[x].GetLdcI4Value(), IL[x + 1].GetLdcI4Value() };
                                var RestoredStr = (string)(VmObject.GetProperty("FinalValue").GetValue(DecMeth.Invoke(null, Params)));
                                IL[x + 3] = OpCodes.Nop.ToInstruction();
                                IL[x + 2] = OpCodes.Nop.ToInstruction();
                                IL[x + 1] = OpCodes.Nop.ToInstruction();
                                IL[x] = OpCodes.Ldstr.ToInstruction(RestoredStr);
                                Context.Log.Info($"Restored String : {RestoredStr}");
                            }
                        }
                    }
                }
            }
        }

        public Type GetVmObjectInterface(Assembly Ass) => Ass.GetTypes().Single(x => x.Name == "VmObject");
    }
}