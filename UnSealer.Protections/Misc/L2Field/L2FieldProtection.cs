
#region Usings
using AsmResolver.DotNet;
using System.Collections.Generic;
using UnSealer.Core;
#endregion

namespace UnSealer.Protections.Misc.L2Field
{
    public class L2FieldProtection : Protection
    {
        public static IDictionary<MethodDefinition, List<IFieldDescriptor>> L2FKey;
        public override string Name => "Local To Field Fixer";

        public override string Description => "Fixs L2F Protection.";

        public override string Id => "l2f";

        public override string Author => "CLand";

        public override void InitPipeline(Context context, Pipeline pipeline)
        {
            pipeline.InsertPreStage(PipelineStage.BeginModule,
                new AnalyzePhase(this));
            pipeline.InsertPreStage(PipelineStage.ProcessModule,
                new RestorationPhase(this));
        }
    }
}