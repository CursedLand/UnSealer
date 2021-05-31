
#region Usings
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using UnSealer.Core;
#endregion

namespace UnSealer.Protections.Misc.L2Field
{
    public class RestorationPhase : ProtectionPhase
    {
        public RestorationPhase(Protection ParentBase)
            : base(ParentBase) { }

        public override string Name => "L2F Restoration Phase.";

        public override ProtectionTargets PhaseTargets => ProtectionTargets.None;

        public override void Execute(Context context, IEnumerable<MetadataMember> targets)
        {
            foreach (KeyValuePair<MethodDefinition, List<IFieldDescriptor>> Pairs in L2FieldProtection.L2FKey)
            {
                Dictionary<IFieldDescriptor, CilLocalVariable> Cache = new Dictionary<IFieldDescriptor, CilLocalVariable>();
                CilInstructionCollection IL = Pairs.Key.CilMethodBody.Instructions;
                for (int x = 0; x < IL.Count; x++)
                {
                    CilInstruction Instr = IL[x];
                    if ((Instr.IsCode(CilCode.Ldsfld) ||
                        Instr.IsCode(CilCode.Stsfld) ||
                        Instr.IsCode(CilCode.Ldsflda)) &&
                        Pairs.Value.Contains(Instr.Operand as IFieldDescriptor))
                    {
                        IFieldDescriptor Field = Pairs.Value.SingleOrDefault(q => q == Instr.Operand as IFieldDescriptor); /* Thanks Linq :^) */
                        if (Field == null)
                        {
                            continue;
                        }

                        CilLocalVariable NewLocal = new CilLocalVariable(context.Importer.ImportTypeSignature(Field.Signature.FieldType));
                        Instr.OpCode = Instr.OpCode.Code switch
                        {
                            CilCode.Ldsfld => CilOpCodes.Ldloc,
                            CilCode.Stsfld => CilOpCodes.Stloc,
                            CilCode.Ldsflda => CilOpCodes.Ldloca,
                            _ => throw new ArgumentOutOfRangeException(nameof(Instr.OpCode))
                        };
                        if (!Cache.ContainsKey(Field))
                        {
                            Pairs.Key.CilMethodBody.LocalVariables.Add(NewLocal);
                            Cache.Add(Field, NewLocal);
                        }
                        else
                        {
                            NewLocal = Cache[Field];
                        }
                        Instr.Operand = NewLocal;
                    }
                }
                Pairs.Value.ToArray().ToList().ForEach(x => ((TypeDefinition)x.DeclaringType)?.Fields?.Remove(x.Resolve())); /* Performance Is Meh. */
            }
        }
    }
}