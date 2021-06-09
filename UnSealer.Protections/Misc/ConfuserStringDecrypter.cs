
#region Usings
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.DataFlow.Analysis;
using Echo.Platforms.AsmResolver;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnSealer.Core;
#endregion

namespace UnSealer.Protections.Misc {
    public class ConfuserStringDecrypter : Protection {
        public override string Name => "Confuser String Decrypter";

        public override string Description => "Decrypt ConfuserEx Constants In Advanced Way.";

        public override string Id => "cfexconst";

        public override string Author => "CLand";

        public override void InitPipeline(Context context, Pipeline pipeline) {
            pipeline.InsertPreStage(PipelineStage.ProcessModule,
                new CSDPhase(this));
        }
    }
    public class CSDPhase : ProtectionPhase
    {
        public CSDPhase(Protection ParentBase)
            : base(ParentBase) { }

        public override string Name => "Confuser String Decrypter Phase.";

        public override ProtectionTargets PhaseTargets => ProtectionTargets.Methods;

        public override void Execute(Context context, IEnumerable<MetadataMember> targets) {
            if (!context.IsReflectionSafe) return; // Sadly Only Reflection To Get Value.
            foreach (var Method in targets.OfType<MethodDefinition>().Where(x => x.CilMethodBody is not null)) {
                var IL = Method.CilMethodBody.Instructions;
                IL.CalculateOffsets(); /* DataFlowGraphs React nani... */
                Method.CilMethodBody.ConstructSymbolicFlowGraph(out var dfg); /* Constructing DataFlowGraph. */
                for (int x = 0; x < IL.Count; x++) {
                    var Instr = IL[x];
                    if (Instr.IsCode(CilCode.Call) &&
                        Instr.Operand is MethodSpecification GenericMethod && /* <Module>.Decode<T> */
                        GenericMethod.Signature.TypeArguments.Count == 1 &&
                        GenericMethod.Signature.TypeArguments[0] == context.Module.CorLibTypeFactory.String) { /* T is string. (i.e. <Module>.Decode<string>(params)) */
                        var vm = new CilVirtualMachine(Method.CilMethodBody, context.Module.Is32Module()); /* Define CilEmulator. */
                        var ex = new CilExecutionContext(vm, vm.CurrentState, default);
                        if (!dfg.Nodes.Contains(Instr.Offset)) continue; /* Maybe Some Time it fails to Make DFG for that Instruction Who Knows Hidden Features ?!  */
                        var StackDeps = dfg.Nodes[Instr.Offset]
                            .GetOrderedDependencies(DependencyCollectionFlags.IncludeStackDependencies)
                            .ToList(); /* Get Stack Dependencies For Constants Call.*/
                        StackDeps.Remove(StackDeps.Last()); /* Remove Call Instruction (Decoder Call). */
                        var BackUp = new List<CilInstruction>();
                        foreach (var Dep in StackDeps) {
                            var EmuInstr = Dep.Contents; /* Store Instruction Into Variable To Eumlate it.. */
                            BackUp.Add(new(EmuInstr.OpCode, EmuInstr.Operand ?? null));
                            vm.Dispatcher.Execute(ex, EmuInstr);
                            EmuInstr.Nop(); /* Nop That Instr. */
                        }
                        var ReflectionMethod = context.ReflectionModule.ResolveMethod(GenericMethod.MetadataToken.ToInt32());
                        var RefParams = ReflectionMethod.GetParameters();
                        var Count = RefParams.Length;
                        var IOSlot = new object[Count];
                        for (int i = 0; i < RefParams.Length; i++) {
                            object S = default;
                            var Value = vm.CurrentState.Stack.Pop();
                            var PType = RefParams[--Count].ParameterType;
                            #region Yandere Code (Close Eyes Please 🙃)
                            // Since I never saw anything using any value else so that is good (for now).
                            #region Ldstr-Handling
                            if (PType == typeof(string) && Value is StringValue @string)
                                S = @string.ToString();
                            else if (Value is not StringValue && PType == typeof(string))
                                S = string.Empty; // Hey Skids :DD
                            #endregion
                            #region I32-Handling
                            if (PType == typeof(int) && Value is I4Value @i32)
                                S = @i32.I32;
                            else if (Value is not I4Value && PType == typeof(int))
                                S = 0;
                            #endregion
                            #region U32-Handling
                            if (PType == typeof(uint) && Value is I4Value @u32)
                                S = @u32.U32;
                            else if (Value is not I4Value && PType == typeof(uint))
                                S = 0U;
                            #endregion
                            #endregion
                            /* Assume Fake Parameter. */
                            if (S == default) S = Convert.ChangeType(default, PType);
                            IOSlot[Count] = S;
                        }
                        try {
                            object Result = default;
                            if (GenericMethod.Method.Resolve().CilMethodBody.Instructions.Any(q => q.ToString().Contains(nameof(Assembly)) || q.ToString().Contains(nameof(StackTrace))))
                                Result = DynamicInvocation((MethodInfo)ReflectionMethod, Method.Name, context.ReflectionModule, IOSlot);
                            else
                                Result = ReflectionMethod.Invoke(null, IOSlot);

                            Instr.OpCode = CilOpCodes.Ldstr; /* Changing Call To Ldstr. */
                            Instr.Operand = Result as string /* Cast Value As String. */
                                ?? string.Empty; /* if its null i dont want AsmResolver Throw Errors.... */
                        }
                        catch {
                            // Restore instructions if their an problem (i.e. TargetInvocationException).
                            for (int i = 0; i < BackUp.Count; i++) {
                                CilInstruction Back = BackUp[i];
                                CilInstruction Org = StackDeps[i].Contents;
                                Org.OpCode = Back.OpCode;
                                Org.Operand = Back.Operand;
                            }
                        }
                    }
                }
            }
            object DynamicInvocation(MethodInfo method, string invokename, Module module, object[] mparams) {
                var pT = new List<Type>();

                foreach (ParameterInfo x in method.GetParameters())
                    pT.Add(x.ParameterType);

                var dMethod = new DynamicMethod(invokename, typeof(string),
                        pT.ToArray(), module,
                        true);

                var ILGen = dMethod.GetILGenerator();

                for (int i = 0; i < mparams.Length; i++)
                    ILGen.Emit(OpCodes.Ldarg, i);

                ILGen.Emit(OpCodes.Call, method);

                ILGen.Emit(OpCodes.Ret);

                return dMethod.Invoke(null, mparams);
            }
        }
    }
}