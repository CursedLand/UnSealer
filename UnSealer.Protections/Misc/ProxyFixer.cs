
#region Usings
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using System.Collections.Generic;
using System.Linq;
using UnSealer.Core;
#endregion

namespace UnSealer.Protections.Misc {
    internal class ProxyFixer : Protection {
        public override string Name => "Proxy Fixer";

        public override string Description => "Inlines Calls That Get Outlined By Some Obfuscators.";

        public override string Id => "proxy";

        public override string Author => "CLand";

        public override void InitPipeline(Context context, Pipeline pipeline) {
            pipeline.InsertPreStage(PipelineStage.BeginModule,
                new ProxyInlinerPhase(this));
        }
    }
    internal class ProxyInlinerPhase : ProtectionPhase {
        public ProxyInlinerPhase(Protection ParentBase) 
            : base(ParentBase) { }

        public override string Name => "Inline Phase";

        public override ProtectionTargets PhaseTargets => ProtectionTargets.Methods;

        public override void Execute(Context context, IEnumerable<MetadataMember> targets) {
            var Cache = new List<MethodDefinition>();
            foreach (var Method in targets.OfType<MethodDefinition>().Where(x => x.CilMethodBody is not null)) {
                var IL = Method.CilMethodBody.Instructions;
                for (var x = 0; x < IL.Count; x++) {
                    if (!(IL[x].IsCode(CilCode.Call) && IL[x].Operand is MethodDefinition ProxyMethod
                        && ProxyMethod.IsProxy(out var callinstr))) continue; // Check If Its Proxy.
                    IL[x].OpCode = callinstr.OpCode;
                    IL[x].Operand = context.Importer.ImportMethod((IMethodDescriptor)callinstr.Operand);
                    lock (Cache) Cache.Add(ProxyMethod);
                }
            }
            Cache.ForEach(x => x?.DeclaringType?.Methods?.Remove(x)); // Nullables is love :3
        }
    }
}