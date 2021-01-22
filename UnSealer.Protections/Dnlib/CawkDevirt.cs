using dnlib.DotNet.Emit;
using System;
using System.Linq;
using UnSealer.Core;
using UnSealer.Core.Utils.Dnlib.CawkRuntime.ConversionBack;

namespace UnSealer.Protections.Dnlib
{
    public class CawkDevirt : Protection
    {
        public override string Name => "CawkVM DeVirtualizator";

        public override string Author => "CursedLand";

        public override ProtectionType Type => ProtectionType.Dnlib;

        public override string Description => "A DeVirtualizer For CawkVM";

        public override void Execute(Context Context) // Change "ConvertBack::Runner" for Modded Versions
        {
            if (Context.SysModule != null && Context.DnModule != null)
            {
                try { Initialize.Initalize("Eddy^CZ_", Context.SysModule); } catch { Context.Log.Custom("Skipping", "Not CawkVM"); return; }
                foreach (var TypeDef in Context.DnModule.Types.Where(x => x.HasMethods && !x.IsGlobalModuleType))
                {
                    foreach (var MethodDef in TypeDef.Methods.Where(x => x.HasBody))
                    {
                        var IL = MethodDef.Body.Instructions;
                        for (int x = 0; x < IL.Count; x++)
                        {
                            if (IL[x].OpCode == OpCodes.Call &&
                                IL[x].Operand.ToString().Contains("ConvertBack::Runner") &&
                                IL[x - 4].IsLdcI4() &&
                                IL[x - 3].IsLdcI4() &&
                                IL[x - 2].IsLdcI4())
                            {
                                try
                                {
                                    var Position = IL[x - 4].GetLdcI4Value();
                                    var Size = IL[x - 3].GetLdcI4Value();
                                    var ID = IL[x - 2].GetLdcI4Value();
                                    object[] Params = new object[MethodDef.Parameters.Count]; int Index = 0;
                                    foreach (var Param in MethodDef.Parameters) { Params[Index++] = Param.Type.Next; }
                                    var methodBase = Context.SysModule.ResolveMethod(MethodDef.MDToken.ToInt32());
                                    var dynamicMethod = ConvertBack.Runner(Position, Size, ID, Params, methodBase);
                                    var dynamicReader = Activator.CreateInstance(
                                                        typeof(System.Reflection.Emit.DynamicMethod).Module.GetTypes()
                                                        .FirstOrDefault(t => t.Name == "DynamicResolver"),
                                                        (System.Reflection.BindingFlags)(-1), null, new object[] { dynamicMethod.GetILGenerator() }, null);
                                    var dynamicMethodBodyReader = new DynamicMethodBodyReader(MethodDef.Module, dynamicReader);
                                    dynamicMethodBodyReader.Read();
                                    MethodDef.Body = dynamicMethodBodyReader.GetMethod().Body;
                                    Context.Log.Debug($"Done Devirtualize Method : {MethodDef.Name}");
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