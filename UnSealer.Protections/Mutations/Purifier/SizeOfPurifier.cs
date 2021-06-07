
#region Usings
using AsmResolver.DotNet;
using AsmResolver.DotNet.Memory;
using AsmResolver.PE.DotNet.Cil;
using System.Collections.Generic;
using System.Linq;
using UnSealer.Core;
#endregion

namespace UnSealer.Protections.Mutations.Purifier {
    public class SizeOfPurifier : ProtectionPhase {
        public SizeOfPurifier(Protection ParentBase)
            : base(ParentBase) { }

        public override string Name => "SizeOf Cleaning Phase";

        public override ProtectionTargets PhaseTargets => ProtectionTargets.Methods;

        public override void Execute(Context context, IEnumerable<MetadataMember> targets) {
            foreach (var Method in targets.OfType<MethodDefinition>().Where(x => x.CilMethodBody is not null)) {
                var Instructions = Method.CilMethodBody.Instructions;
                foreach (var SizeOfInstr in Instructions.Where(x => x.IsCode(CilCode.Sizeof) && x.Operand is not TypeSpecification /* No Generics. */)) {
                    var Size = (SizeOfInstr.Operand as ITypeDefOrRef).GetImpliedMemoryLayout(context.Module.Is32Module()).Size;
                    var NewInstr = CilInstruction.CreateLdcI4((int)Size);
                    SizeOfInstr.OpCode = NewInstr.OpCode;
                    SizeOfInstr.Operand = NewInstr.Operand;
                }
            }
        }
    }
}