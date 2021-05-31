
#region Usings
using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using System.Collections.Generic;
using System.Linq;
using UnSealer.Core;
#endregion

namespace UnSealer.Protections.Devirtualizers.CawkVM
{
    public class DiscoveryPhase : ProtectionPhase
    {
        public DiscoveryPhase(Protection ParentBase)
            : base(ParentBase) { }

        public override string Name => "Dicovery Phase";

        public override ProtectionTargets PhaseTargets => ProtectionTargets.Methods;

        public override void Execute(Context context, IEnumerable<MetadataMember> targets)
        {
            CawkVM.CawkKey = new DevirtualizationContext()
            {
                Data = Utils.DecryptXor(((DataSegment)context.Module.Resources.Single(m => m.Name == "Eddy^CZ_").EmbeddedDataSegment).Data),
                VirtualizatedMethods = new List<MethodInfo>()
            };
            foreach (MethodDefinition Method in targets.OfType<MethodDefinition>().Where(m => m.CilMethodBody is not null))
            {
                AsmResolver.DotNet.Code.Cil.CilInstructionCollection Instructions = Method.CilMethodBody.Instructions;
                for (int x = 0; x < Instructions.Count; x++)
                {
                    if (Instructions[x].IsLdcI4() && // Position
                        Instructions[x + 1].IsLdcI4() && // Size
                        Instructions[x + 2].IsLdcI4() && // ID
                        Instructions[x + 4].IsCode(CilCode.Call) &&
                        Instructions[x + 4].IsMethod("ConvertBack", "Runner"))
                    {
                        (int Position, int Size, int ID) = (Instructions[x].GetLdcI4Constant(), Instructions[x + 1].GetLdcI4Constant(), Instructions[x + 2].GetLdcI4Constant());
                        // Add Virtualized Method Into Context.
                        CawkVM.CawkKey.VirtualizatedMethods.Add(new MethodInfo(Method, Position, Size, ID));
                    }
                }
            }
        }
    }
}
