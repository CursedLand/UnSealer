using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnSealer.Core;
using UnSealer.Core.Utils.AsmResolver;

namespace UnSealer.Protections.AsmResolver
{
    public class StringDecryptor : Protection
    {
        public override string Name => "Universal String Decryptor";

        public override string Author => "CursedLand";

        public override ProtectionType Type => ProtectionType.AsmResolver;

        public override string Description => "A String Decryptor Uses Auto Param Collector";

        public override void Execute(Context Context)
        {
            GetJunk.Clear();
            GetJunk = new List<CilInstruction>();
            if (Utils.DecMethod != null && Context.AsmModule != null && Context.SysModule != null)
            {
                foreach (var Type in Context.AsmModule.GetAllTypes())
                {
                    foreach (var Method in Type.Methods.Where(x => x.HasMethodBody))
                    {
                        Method.CilMethodBody.Instructions.ExpandMacros();
                        var IL = Method.CilMethodBody.Instructions;
                        for (int x = 0; x < IL.Count; x++)
                        {
                            try
                            {
                                if (IL[x].OpCode == CilOpCodes.Call && IL[x].Operand is IMethodDescriptor && IL[x].Operand.ToString().Contains(Utils.DecMethod.Name) && ((IMethodDescriptor)IL[x].Operand).Resolve().Parameters.Count == Utils.DecMethod.Resolve().Parameters.Count)
                                {
                                    object Result = null;
                                    var Params = ParamsParser(Utils.DecMethod.Resolve(), x, IL, Context.SysModule);
                                    var Ref = ((MethodInfo)Context.SysModule.ResolveMethod(Utils.DecMethod.MetadataToken.ToInt32()));
                                    if (Utils.DecMethod.Resolve().CilMethodBody.Instructions.Any<CilInstruction>(i => i.ToString().Contains("StackTrace") || i.ToString().Contains("GetCallingAssembly")))
                                        Result = InvokeAsDynamic(Context.SysModule, Method, Utils.DecMethod.Resolve(), Params);
                                    else
                                        Result = Ref.Invoke(null, Params);
                                    Context.StringLog.Debug($"Restored > {Result}");
                                    IL[x].OpCode = CilOpCodes.Ldstr;
                                    IL[x].Operand = Result;
                                    foreach (var i in GetJunk)
                                        i.OpCode = CilOpCodes.Nop;
                                }
                            }
                            catch (Exception ex)
                            {
                                Context.StringLog.Error(ex.Message);
                            }
                        }
                    }
                }
            }
            else { Context.StringLog.Custom("Skipping", "Decryption Method Empty.."); }
        }

        public object[] ParamsParser(MethodDefinition DecMethod, int Index, CilInstructionCollection IL, Module Module)
        {
            var pi = 0;

            var pp = 0;

            var rMethod = Module.ResolveMethod(DecMethod.MetadataToken.ToInt32());

            var rMethodParams = rMethod.GetParameters();

            var C = rMethodParams.Length;

            var Parsed = new object[C];

            for (int x = (-C + Index); x < Index; x++)
            {

                object Result = null;

                if (IL[x].OpCode == CilOpCodes.Stsfld)
                    Result = Module.ResolveField(((IFieldDescriptor)IL[x].Operand).MetadataToken.ToInt32()).GetValue(null);

                var CurrentT = rMethodParams[pi++].ParameterType;



                if (CurrentT == typeof(String) || CurrentT == typeof(string))
                    Result = (string)IL[x].Operand;
                else if (CurrentT == typeof(Int16) || CurrentT == typeof(short))
                    Result = Result == null ? (short)IL[x].GetLdcI4Constant() : (short)Result;
                else if (CurrentT == typeof(Int32) || CurrentT == typeof(int))
                    Result = Result == null ? (int)IL[x].GetLdcI4Constant() : (int)Result;
                else if (CurrentT == typeof(Int64) || CurrentT == typeof(long))
                    Result = Result == null ? (long)IL[x].GetLdcI4Constant() : (long)Result;
                else if (CurrentT == typeof(SByte) || CurrentT == typeof(sbyte))
                    Result = Result == null ? (sbyte)IL[x].Operand : (sbyte)Result;
                else if (CurrentT == typeof(Byte) || CurrentT == typeof(byte))
                    Result = Result == null ? (byte)IL[x].Operand : (byte)Result;
                else if (CurrentT == typeof(UInt16) || CurrentT == typeof(ushort))
                    Result = Result == null ? (ushort)unchecked(IL[x].GetLdcI4Constant()) : (ushort)Result;
                else if (CurrentT == typeof(UInt32) || CurrentT == typeof(uint))
                    Result = Result == null ? (uint)unchecked(IL[x].GetLdcI4Constant()) : (uint)Result;
                else if (CurrentT == typeof(UInt64) || CurrentT == typeof(ulong))
                    Result = Result == null ? (ulong)unchecked(IL[x].GetLdcI4Constant()) : (ulong)Result;
                else if (CurrentT == typeof(Boolean) || CurrentT == typeof(bool))
                    Result = Result == null ? (IL[x].GetLdcI4Constant() == 1 ? true : false) : Convert.ToBoolean(Result);
                else if (CurrentT == typeof(Char) || CurrentT == typeof(char))
                    Result = Result == null ? Convert.ToChar(IL[x].GetLdcI4Constant()) : (char)Result;
                else
                    Result = Result == null ? Convert.ChangeType(IL[x].Operand, CurrentT) : Convert.ChangeType(Result, CurrentT);

                Parsed[pp++] = Result;

                GetJunk.Add(IL[x]);

            }

            return Parsed;
        }

        public object InvokeAsDynamic(Module Module, MethodDefinition CMethod, MethodDefinition DecMethod, object[] Params)
        {
            // Semi Bypass Any AntiInvoking Technique (StackTrace, GetCallingAssembly, etc.)
            var rMethod = ((MethodInfo)Module.ResolveMethod(DecMethod.MetadataToken.ToInt32()));

            var rType = rMethod.ReturnType;

            var pT = new List<Type>();

            foreach (var x in rMethod.GetParameters())
                pT.Add(x.ParameterType);

            var dMethod = new DynamicMethod(CMethod.Name, rType, pT.ToArray(), Module, true);

            var ILGen = dMethod.GetILGenerator();

            for (int i = 0; i < Params.Length; i++)
                ILGen.Emit(OpCodes.Ldarg, i);

            ILGen.Emit(OpCodes.Call, rMethod);

            ILGen.Emit(OpCodes.Ret);

            return dMethod.Invoke(null, Params);
        }

        public List<CilInstruction> GetJunk = new List<CilInstruction>();
    }
}