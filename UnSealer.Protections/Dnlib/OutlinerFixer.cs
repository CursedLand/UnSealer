using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Collections.Generic;
using System.Linq;
using UnSealer.Core;

namespace UnSealer.Protections.Dnlib
{
    public class OutlinerFixer : Protection
    {
        public override string Name => "OutLiner Fixer";

        public override string Author => "CursedLand";

        public override string Description => "Fixes Inteager & String Outliners";

        public override ProtectionType Type => ProtectionType.Dnlib;

        public override void Execute(Context Context)
        {
            if (Context.DnModule != null)
            {
                foreach (var TypeDef in Context.DnModule.Types.Where(x => x.HasMethods && !x.IsGlobalModuleType).ToArray())
                {
                    foreach (var MethodDef in TypeDef.Methods.Where(x => x.HasBody).ToArray())
                    {
                        var IL = MethodDef.Body.Instructions;
                        for (int x = 0; x < IL.Count; x++)
                        {
                            if (IL[x].OpCode == OpCodes.Call &&
                                IL[x].Operand is IMethod &&
                                ((IMethod)IL[x].Operand).GetParams().Count == 0  &&
                                ((IMethod)IL[x].Operand).DeclaringType == Context.DnModule.GlobalType)
                            {
                                // You Can Use Invoke But Its Not safe :)
                                var OutMethod = (IMethod)IL[x].Operand;
                                if (!Cache.ContainsKey(OutMethod.ResolveMethodDef()))
                                {
                                    if (!(OutMethod.ResolveMethodDef().Body.Instructions[0].OpCode == OpCodes.Ldstr || IsLdc(OutMethod.ResolveMethodDef().Body.Instructions[0].OpCode.Code))) continue;
                                    if (OutMethod.ResolveMethodDef().Body.Instructions[1].OpCode != OpCodes.Ret) continue;
                                    var InstValue = OutMethod.ResolveMethodDef().Body.Instructions[0];
                                    Cache.Add(OutMethod.ResolveMethodDef(), InstValue);
                                    Context.Log.Info("Found Outliner Method");
                                    IL[x] = new Instruction(InstValue.OpCode, InstValue.Operand);
                                    Context.Log.Debug($"Done Recover : {InstValue.Operand}");
                                }
                                else
                                {
                                    var CacheInst = Cache[OutMethod.ResolveMethodDef()];
                                    IL[x] = new Instruction(CacheInst.OpCode, CacheInst.Operand);
                                }
                            }
                        }
                    }
                }
                Cache.ToList().ForEach(x => x.Key.DeclaringType.Methods.Remove(x.Key));
                Cache.Clear(); Cache = new Dictionary<MethodDef, Instruction>();
            }
        }

        public bool IsLdc(Code Code)
        {
            switch(Code)
            {
                case Code.Ldc_I4:
                    return true;
                case Code.Ldc_I4_S:
                    return true;
                case Code.Ldc_I8:
                    return true;
                case Code.Ldc_R4:
                    return true;
                case Code.Ldc_R8:
                    return true;
                default:
                    return false;
            }
        }

        public Dictionary<MethodDef, Instruction> Cache = new Dictionary<MethodDef, Instruction>();
    }
}