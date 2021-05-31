
#region Usings
using System.Linq;
using UnSealer.Core;
#endregion

namespace UnSealer.Protections.Devirtualizers.CawkVM
{
    public class CawkVM : Protection
    {
        public static DevirtualizationContext CawkKey;
        public override string Name => "CawkVM Devirtualizer";

        public override string Description => "Devirtualize Modules Protect With CawkVM";

        public override string Id => "cawkvm";

        public override string Author => "CLand";

        public override void InitPipeline(Context context, Pipeline pipeline)
        {
            if (!context.Module.Resources.Any(r => r.Name == "Eddy^CZ_"))
            {
                context.Logger.Warn("Not CawkVm.");
                return;
            }
            pipeline.InsertPreStage(PipelineStage.BeginModule,
                new DiscoveryPhase(this));
            pipeline.InsertPreStage(PipelineStage.ProcessModule,
                new RestorationPhase(this));
        }
    }
}
