
#region Usings
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Concrete.Values.ValueType;
using Echo.DataFlow.Analysis;
using Echo.Platforms.AsmResolver;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using System;
using System.Collections.Generic;
using System.Linq;
using UnSealer.Core;
#endregion

namespace UnSealer.Protections.Mutations.Purifier {
    public class MathPurifier : ProtectionPhase {
        public MathPurifier(Protection ParentBase)
            : base(ParentBase) { }

        public override string Name => "Math Cleaning Phase";

        public override ProtectionTargets PhaseTargets => ProtectionTargets.Methods;

        public override void Execute(Context context, IEnumerable<MetadataMember> targets) {
            if (!context.IsReflectionCorlibSafe) return;
            foreach (var Method in targets.OfType<MethodDefinition>().Where(m => m.CilMethodBody is not null)) {
                var Instructions = Method.CilMethodBody.Instructions;
                Method.CilMethodBody.ConstructSymbolicFlowGraph(out var DFG); /* Dfg Constructing. */
                for (int x = 0; x < Instructions.Count; x++) {
                    var Instr = Instructions[x];   /* Some People Make It Callvirt To Deafet Public tools :p */
                    if ((Instr.IsCode(CilCode.Call) || Instr.IsCode(CilCode.Callvirt)) &&
                         Instr.IsFromNS("System", "Math") && DFG.Nodes.Contains(Instr.Offset)) {

                        var CallNode = DFG.Nodes[Instr.Offset]
                            .GetOrderedDependencies(DependencyCollectionFlags.IncludeStackDependencies)
                            .ToList();

                        if (CallNode.Any(x => x.Contents.OpCode.OperandType == CilOperandType.InlineMethod)) continue;

                        var vm = new CilVirtualMachine(Method.CilMethodBody,
                            context.Module.Is32Module());
                        var ex = new CilExecutionContext(vm, vm.CurrentState, default);

                        foreach (var Dep in CallNode) {
                            var CInstr = Dep.Contents;
                            vm.Dispatcher.Execute(ex, CInstr);
                            CInstr.Nop();
                        }
                        var ISlot = new object[((IMethodDescriptor)Instr.Operand)
                            .Signature.GetTotalParameterCount()];
                        for (int i = 0; i < ISlot.Length; i++) {
                            var Value = vm.CurrentState.Stack.Pop();
                            ISlot[i] = Value switch {
                                I4Value I4 => I4.I32,
                                I8Value I8 => I8.I64,
                                FValue F => F.F64,
                                Float32Value F32 => F32.F32,
                                Float64Value F64 => F64.F64,
                                Integer16Value I6 => I6.I16,
                                Integer32Value I32 => I32.I32,
                                Integer64Value I64 => I64.I64,
                                _ => throw new NotSupportedException(nameof(Value))
                            };
                        }
                        var InvocationValue = context.ReflectionCorlib.ResolveMethod(((IMethodDescriptor)Instr.Operand).MetadataToken.ToInt32())
                            .Invoke(null, ISlot);
                        Instr.OpCode = ((IMethodDescriptor)Instr.Operand).Signature.ReturnType.ElementType switch {
                            ElementType.I4 => CilOpCodes.Ldc_I4,
                            ElementType.I8 => CilOpCodes.Ldc_I8,
                            ElementType.R4 => CilOpCodes.Ldc_R4,
                            ElementType.R8 => CilOpCodes.Ldc_R8,
                            _ => CilOpCodes.Ldc_I4,
                        };
                        Instr.Operand = InvocationValue;
                    }
                }
            }
        }
    }
}