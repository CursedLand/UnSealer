
#region Usings
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.DataFlow;
using Echo.DataFlow.Analysis;
using Echo.Platforms.AsmResolver;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using System.Collections.Generic;
using System.Linq;
using UnSealer.Core;
#endregion

namespace UnSealer.Protections.Mutations.Purifier
{
    public class ArithmeticPurifier : ProtectionPhase
    {
        public ArithmeticPurifier(Protection ParentBase)
            : base(ParentBase) { }

        public override string Name => "Arithmetic Cleaning Phase";

        public override ProtectionTargets PhaseTargets => ProtectionTargets.Methods;

        public override void Execute(Context context, IEnumerable<MetadataMember> targets)
        {
            // i dont really recommend to execute this code since Echo have some problems i have discovered while i was coding on my own samples.
            /*foreach (MethodDefinition Method in targets.OfType<MethodDefinition>().Where(x => x.CilMethodBody is not null)) {
                var IL = Method.CilMethodBody.Instructions;
                IL.CalculateOffsets(); *//* nani. *//*
                DataFlowGraph<CilInstruction> dfg = null;
                try {
                    Method.CilMethodBody.ConstructSymbolicFlowGraph(out dfg); *//* Constructing DataFlow Graph. *//*
                }
                catch *//* There An Problem While i Was Coding This In Echo DataFlowGraph Constructing in Some Sample Methods... *//* {
                    continue;
                }
                var vm = new CilVirtualMachine(Method.CilMethodBody, context.Module.Is32Module()); *//* Assigning Emulator. *//*
                var ex = new CilExecutionContext(vm, vm.CurrentState, default);
                for (int x = 0; x < IL.Count; x++)
                {
                    vm.CurrentState.Stack.Clear(); *//* Clear Values In Stack To Began New Calcualtion. *//*
                    CilInstruction Instr = IL[x];
                    if (!Instr.IsArithmetic())
                    {
                        continue; *//* Check if Its An Arithmetic Operator. (ex : +,-,*,^,<<,>>,~,%,/). *//*
                    }

                    if (!dfg.Nodes.Contains(Instr.Offset))
                    {
                        continue; *//* Check If That Arithmetic Node Assigned In DataflowGraph. *//*
                    }

                    var Deps = dfg.Nodes[Instr.Offset]
                        .GetOrderedDependencies(DependencyCollectionFlags.IncludeStackDependencies)
                        .ToList(); *//* get Depndency By Stack Order. *//*
                    foreach (var Dep in Deps) {
                        CilInstruction CInstr = Dep.Contents;
                        vm.Dispatcher.Execute(ex, CInstr);
                    }
                    if (!vm.CurrentState.Stack.Top.IsKnown)
                        continue; *//* if the value is known we will replace it. *//*

                    for (int i = 0; i < Deps.Count - 1; i++)
                        Deps[i].Contents.Nop(); *//* Nop Useless Instructions. *//*

                    switch (vm.CurrentState.Stack.Top)
                    {
                        case FValue FV:
                            Instr.OpCode = CilOpCodes.Ldc_R8;
                            Instr.Operand = FV.F64;
                            break;
                        case I8Value I8V:
                            Instr.OpCode = CilOpCodes.Ldc_I8;
                            Instr.Operand = I8V.I64;
                            break;
                        case NativeIntegerValue IPtrV:
                            Instr.OpCode = CilOpCodes.Ldc_I4;
                            Instr.Operand = IPtrV.InterpretAsI4().I32;
                            IL.Insert(x + 1, new CilInstruction(CilOpCodes.Conv_I)); *//* Cast To IntPtr. *//*
                            ++x;
                            break;
                        case I4Value I4V:
                            Instr.OpCode = CilOpCodes.Ldc_I4;
                            Instr.Operand = I4V.I32;
                            break;
                    }
                    vm.CurrentState.Stack.Clear(); *//* Clear Values In Stack To Began New Calcualtion. *//*
                }
            }*/
        }
    }
}
