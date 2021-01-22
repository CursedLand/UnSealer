
#region Usings
using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using System;
using System.Linq;
using System.Reflection;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using UnSealer.Core;
using UnSealer.Core.Utils.AsmResolver;
#endregion

namespace UnSealer.Protections.AsmResolver
{
    // From : https://github.com/dr4k0nia/Unscrambler/blob/master/Unscrambler/Features/MethodFeatures/MathReplace.cs
    // All Credits Go To drakoniа#0601
    public class MathRemover : Protection
    {
        public override string Name => "Math Remover";

        public override string Author => "drakoniа";

        public override ProtectionType Type => ProtectionType.AsmResolver;

        public override string Description => "Remove Math.*() Mutations.";

        public override void Execute(Context Context)
        {
            foreach(var Type in Context.AsmModule.GetAllTypes())
            {
                foreach(var Method in Type.Methods.Where(x=>x.CilMethodBody != null))
                {
                    Method.CilMethodBody.Instructions.ExpandMacros();
                    var instr = Method.CilMethodBody.Instructions;
                    for (int i = 0; i < instr.Count; i++)
                    {
                        if (!(instr[i].Operand is MemberReference memberRef) ||
                             !memberRef.DeclaringType.IsTypeOf("System", "Math"))
                            continue;
                        var mathMethod =
                            typeof(Math).Assembly.ManifestModule.ResolveMethod(memberRef.Resolve().MetadataToken
                                .ToInt32());

                        var arguments = GetArguments(mathMethod, instr, i);

                        if (arguments.Any(o => o is null))
                        {
                            _instructionsToRemove.Clear();
                            continue;
                        }

                        var result = mathMethod.Invoke(null, arguments);
                        var opcode = Utils.GetCilCode(Method.Module.CorLibTypeFactory.FromType(
                            ((MethodSignature)memberRef.Signature)
                            .ReturnType).ElementType);
                        instr[i].OpCode = opcode;
                        instr[i].Operand = result;
                        Context.Log.Debug($"Fixed Math.{mathMethod.Name} : {result.ToString()}");
                        foreach (var instruction in _instructionsToRemove)
                        {
                            instruction.OpCode = CilOpCodes.Nop;
                        }
                        _instructionsToRemove.Clear();
                    }

                    Method.CilMethodBody.Instructions.OptimizeMacros();
                }
            }
        }

        private object[] GetArguments(MethodBase mathMethod, CilInstructionCollection instr, int i)
        {
            var arguments = new object[mathMethod.GetParameters().Length];
            for (int j = 0; j < arguments.Length; j++)
            {
                switch (instr[i - j - 1].OpCode.OperandType)
                {
                    case CilOperandType.InlineI:
                    case CilOperandType.InlineI8:
                    case CilOperandType.InlineR:
                    case CilOperandType.ShortInlineR:
                        arguments[arguments.Length - j - 1] = instr[i - j - 1].Operand;
                        _instructionsToRemove.Add(instr[i - j - 1]);
                        break;
                    default:
                        arguments[arguments.Length - j - 1] = null;
                        break;
                }
            }

            return arguments;
        }

        private readonly List<CilInstruction> _instructionsToRemove = new List<CilInstruction>();
    }
}