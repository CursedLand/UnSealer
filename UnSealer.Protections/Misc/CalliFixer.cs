
#region Usings
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using System.Collections.Generic;
using System.Linq;
using UnSealer.Core;
#endregion

namespace UnSealer.Protections.Misc
{
    public class CalliFixer : Protection
    {
        public override string Name => "Calli Fixer";

        public override string Description => "Removes Basic Calli OpCode Usage."; /* Using Ldftn is basic. */

        public override string Id => "calli";

        public override string Author => "CLand";

        public override void InitPipeline(Context context, Pipeline pipeline)
        {
            pipeline.InsertPreStage(PipelineStage.ProcessModule,
                new CalliPhase(this));
        }
    }
    public class CalliPhase : ProtectionPhase
    {
        public CalliPhase(Protection ParentBase)
            : base(ParentBase) { }

        public override string Name => "Calli Phase.";

        public override ProtectionTargets PhaseTargets => ProtectionTargets.Methods;

        public override void Execute(Context context, IEnumerable<MetadataMember> targets)
        {
            foreach (var Method in targets.OfType<MethodDefinition>().Where(x => x.CilMethodBody is not null)) {
                var IL = Method.CilMethodBody.Instructions;
                for (int x = 0; x < IL.Count; x++) {
                    if (IL[x].IsCode(CilCode.Ldftn) &&
                        IL[x + 1].IsCode(CilCode.Calli)) {
                        IL[x].Operand = context.Importer.ImportMethod(IL[x].Operand as IMethodDescriptor); /* Import Org Call Method */
                        IL[x].OpCode = CilOpCodes.Call; /* Convert it To Call Instruction */
                        IL.RemoveAt(x + 1); /* Remove Calli's Instruction. */
                    }
                }
            }
        }
    }
}