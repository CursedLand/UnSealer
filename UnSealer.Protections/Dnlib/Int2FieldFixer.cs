using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Linq;
using UnSealer.Core;

namespace UnSealer.Protections.Dnlib
{
    public class Int2FieldFixer : Protection
    {
        public override string Name => "Int2Fields Fixer";

        public override string Author => "CursedLand";

        public override string Description => "Fixes Int Fields";

        public override ProtectionType Type => ProtectionType.Dnlib;

        public override void Execute(Context Context)
        {
            if (Context.DnModule != null && Context.SysModule != null)
            {
                foreach (var TypeDef in Context.DnModule.Types.Where(x => x.HasMethods && !x.IsGlobalModuleType))
                {
                    foreach (var MethodDef in TypeDef.Methods.Where(x => x.HasBody))
                    {
                        var IL = MethodDef.Body.Instructions;
                        for (int x = 0; x < IL.Count; x++)
                        {
                            if (IL[x].OpCode == OpCodes.Ldsfld &&
                                ((IField)IL[x].Operand).DeclaringType == Context.DnModule.GlobalType &&
                                ((IField)IL[x].Operand).ResolveFieldDef().FieldType == Context.DnModule.CorLibTypes.Int32)
                            {
                                try
                                {
                                    var FieldVal = Context.SysModule.ResolveField(((IField)IL[x].Operand).MDToken.ToInt32()).GetValue(null);
                                    IL[x] = new Instruction(OpCodes.Ldc_I4, FieldVal);
                                    Context.Log.Info($"Field Restored : {FieldVal}");
                                }
                                catch (Exception ex)
                                {
                                    Context.Log.Error(ex.Message);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}