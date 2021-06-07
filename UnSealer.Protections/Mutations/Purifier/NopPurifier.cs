
#region Usings
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using UnSealer.Core;
#endregion

namespace UnSealer.Protections.Mutations.Purifier {
    public class NopPurifier : ProtectionPhase {
        public NopPurifier(Protection ParentBase)
            : base(ParentBase) { }

        public override string Name => "Nop Cleaning Phase.";

        public override ProtectionTargets PhaseTargets => ProtectionTargets.Methods;

        public override void Execute(Context context, IEnumerable<MetadataMember> targets) {
            foreach (var Method in targets.OfType<MethodDefinition>().Where(x => x.CilMethodBody is not null)) {
                var IL = Method.CilMethodBody.Instructions;
                var Branches = Method.CilMethodBody.GetBranches();
                foreach (var Instr in IL.Where(x => x.IsCode(CilCode.Nop) && !Branches.Contains(x)).ToArray())
                    IL.Remove(Instr);
                IL.CalculateOffsets();
            }
        }
    }
}
