using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnSealer.Core;
using UnSealer.Core.Utils.AsmResolver;

namespace UnSealer.Protections.AsmResolver
{
    public class StringDecryptor : Protection
    {
        public override string Name => "Universal String Decryptor";

        public override string Author => "CursedLand";

        public override ProtectionType Type => ProtectionType.AsmResolver;

        public override void Execute(Context Context)
        {
            if (Utils.DecMethod != null)
            {
                foreach (var Type in Context.AsmModule.GetAllTypes().Where(x => x.Methods != null))
                {
                    foreach (var Method in Type.Methods.Where(x => x.CilMethodBody != null))
                    {
                        Method.CilMethodBody.Instructions.ExpandMacros();
                        var IL = Method.CilMethodBody.Instructions;
                        for (int x = 0; x < IL.Count; x++)
                        {
                            try
                            {
                                if (IL[x].OpCode == CilOpCodes.Call && IL[x].Operand is IMethodDescriptor && IL[x].Operand.ToString().Contains(Utils.DecMethod.Name) && ((IMethodDescriptor)IL[x].Operand).Resolve().Parameters.Count == Utils.DecMethod.Resolve().Parameters.Count)
                                {
                                    Context.StringLog.Debug($"Found Encrypted Str ...");
                                    var Params = ParamsParser(Utils.DecMethod.Resolve(), x, IL, Context.StringLog);
                                    var Ref = ((MethodInfo)Context.SysModule.ResolveMethod(Utils.DecMethod.MetadataToken.ToInt32()));
                                    var Result = (string)Ref.Invoke(null, Params);
                                    Context.StringLog.Debug($"Restored String : {Result}");
                                    IL[x] = new CilInstruction(CilOpCodes.Ldstr, Result);
                                    foreach (var i in GetJunk)
                                        i.OpCode = CilOpCodes.Nop;
                                }
                            }
                            catch (Exception ex)
                            {
                                Context.StringLog.Error(ex.Message);
                            }
                        }
                        GetJunk.Clear();
                    }
                }
            }
            else { Context.StringLog.Custom("Skipping", "Decryption Method Empty.."); }
        }
        public object[] ParamsParser(MethodDefinition DecMethod, int Index, CilInstructionCollection IL, Logger L)
        {
            var C = DecMethod.Parameters.Count; int ParsedIndex = -0; object[] ParsedParams = new object[C]; var Temp = 0;
            for (int x = -C + Index; x < Index; x++)
            {
                ParsedParams[ParsedIndex++] = Convert.ChangeType(IL[x].Operand, System.Type.GetType(DecMethod.Parameters[Temp++].ParameterType.GetTypeFullName()));
                GetJunk.Add(IL[x]);
            }
            return ParsedParams;
        }
        public List<CilInstruction> GetJunk = new List<CilInstruction>();
    }
}