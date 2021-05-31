
#region Usings
using UnSealer.Core;
#endregion

namespace UnSealer.Protections.Mutations.Purifier
{
    public class MutationPurifier : Protection
    {
        public override string Name => "Mutation Purifier";

        public override string Description => "Purify Arithmetics & Operators & Other Mutations.";

        public override string Id => "mpure";

        public override string Author => "CLand";

        public override void InitPipeline(Context context, Pipeline pipeline)
        {
            pipeline.InsertPostStage(PipelineStage.BeginModule,
                new ProtectionPhase[] { new MathPurifier(this), new NopPurifier(this), new ArithmeticPurifier(this), new NopPurifier(this), new SizeOfPurifier(this), new NopPurifier(this), new MathPurifier(this) });
        }
    }
}