
#region Usings
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using System.Collections.Generic;
using System.Linq;
using UnSealer.Core;
#endregion

namespace UnSealer.Protections.Misc.L2Field {
    public class AnalyzePhase : ProtectionPhase {
        public AnalyzePhase(Protection ParentBase)
            : base(ParentBase) { }

        public override string Name => "Analyzing Phase.";

        public override ProtectionTargets PhaseTargets => ProtectionTargets.Methods;

        public override void Execute(Context context, IEnumerable<MetadataMember> targets) {
            var Storage = new Dictionary<MethodDefinition, List<IFieldDescriptor>>();
            foreach (var Method in targets.OfType<MethodDefinition>().Where(x => x.CilMethodBody is not null)) {
                Storage[Method] = new();
                var IL = Method.CilMethodBody.Instructions;
                for (int x = 0; x < IL.Count; x++) {
                    var Instr = IL[x];
                    if (!(Instr.IsCode(CilCode.Ldsfld) || Instr.IsCode(CilCode.Ldsflda) || Instr.IsCode(CilCode.Stsfld)))
                        continue;
                    var Field = Instr.Operand as FieldDefinition;
                    if (!(Field.IsStatic && Field.IsPublic && Field.DeclaringType == context.Module.GetModuleType())) /* Field Is From <Module> type & static and public. */
                        continue;
                    var STemp = Storage.Values.ToList();
                    STemp.Remove(Storage[Method]); // If The Field Is From Same Method its okey...
                    if (!STemp.Any(q => q.Contains(Field))) { /* Get Sure If It Used In Another Method. */
                        if (!Storage[Method].Contains(Field)) Storage[Method].Add(Field);
                    }
                    else {
                        List<List<IFieldDescriptor>> CList = Storage.Values.Where(l => l.Contains(Field)).ToArray().ToList();
                        CList.ForEach(cl => cl.ToArray().ToList().ForEach(lc => cl.Remove(lc))); // :)
                    }
                }
            }
            L2FieldProtection.L2FKey = Storage;
        }
    }
}
