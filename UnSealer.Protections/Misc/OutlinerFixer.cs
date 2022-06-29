
#region Usings
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using System.Collections.Generic;
using System.Linq;
using UnSealer.Core;
#endregion

namespace UnSealer.Protections.Misc {
    public class OutlinerFixer : Protection {
        public override string Name => "Outliner Fixer";

        public override string Description => "Fixes Proxies Method That Outlines Strings/Ints";

        public override string Id => "inliner";

        public override string Author => "CLand";

        public override void InitPipeline(Context context, Pipeline pipeline) {
            pipeline.InsertPreStage(PipelineStage.ProcessModule,
                new InlinerPhase(this));
        }
    }
    public class InlinerPhase : ProtectionPhase {
        public InlinerPhase(Protection ParentBase)
            : base(ParentBase) { }

        public override string Name => "Inline Phase.";

        public override ProtectionTargets PhaseTargets => ProtectionTargets.Methods;

        public override void Execute(Context context, IEnumerable<MetadataMember> targets) {
            foreach (var Method in targets.OfType<MethodDefinition>().Where(x => x.CilMethodBody is not null)) {
                var IL = Method.CilMethodBody.Instructions;
                for (int x = 0; x < IL.Count; x++) {
                    var Instr = IL[x];
                    if ((Instr.IsCode(CilCode.Call) || Instr.IsCode(CilCode.Callvirt)) &&
                        Instr.Operand is MethodDefinition OutlineMethod &&
                        OutlineMethod.Parameters.Count <= 0 &&
                        OutlineMethod.CilMethodBody is not null &&
                        OutlineMethod.CilMethodBody.Instructions.Count <= 2 &&
                        OutlineMethod.CilMethodBody.Instructions[1].IsCode(CilCode.Ret) &&
                        (OutlineMethod.CilMethodBody.Instructions[0].IsCode(CilCode.Ldstr) || OutlineMethod.CilMethodBody.Instructions[0].IsLdcI4())) {
                        var OutLined = OutlineMethod.CilMethodBody.Instructions[0];
                        Instr.OpCode = OutLined.OpCode;
                        Instr.Operand = OutLined.Operand;
                        OutlineMethod?.DeclaringType?.Methods?.Remove(OutlineMethod);
                    }
                }
            }
        }
    }
}
