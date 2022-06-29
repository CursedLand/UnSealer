
#region Usings
using AsmResolver.DotNet;
using System.Collections.Generic;
using System.Linq;
using UnSealer.Core;
#endregion

namespace UnSealer.Protections.Devirtualizer.CawkVM
{
    public class CawkVM : Protection {
        public static DevirtualizationContext CawkKey;
        public override string Name => "CawkVM Devirtualizer";

        public override string Description => "Devirtualize Modules Protect With CawkVM";

        public override string Id => "cawkvm";

        public override string Author => "CLand";

        public override void InitPipeline(Context context, Pipeline pipeline) {
            if (!context.Module.Resources.Any(r => r.Name == "Eddy^CZ_")) {
                context.Logger.Warn("Not CawkVm.");
                return;
            }
            pipeline.InsertPreStage(PipelineStage.BeginModule,
                new DiscoveryPhase(this));
            pipeline.InsertPreStage(PipelineStage.ProcessModule,
                new RestorationPhase(this));
        }
    }
    public class DevirtualizationContext {
        public IList<MethodInfo> VirtualizatedMethods { set; get; }
        public byte[] Data { set; get; }
    }
    public struct MethodInfo {
        public MethodInfo(MethodDefinition Method,
            int Position, int Size, int ID)
        {
            Parent = Method;
            this.Position = Position;
            this.Size = Size;
            this.ID = ID;
        }
        public MethodDefinition Parent { get; }
        public int Position { get; }
        public int Size { get; }
        public int ID { get; }
    }
}
