using UnSealer.Core;
using System.Linq;
using dnlib.DotNet.Emit;

namespace UnSealer.Protections.Dnlib
{
    public class AntiInvokePatch : Protection
    {
        public override string Name => "Anti Invoke Patcher";

        public override string Author => "CursedLand";

        public override ProtectionType Type => ProtectionType.Dnlib;

        public override string Description => "Patching AntiInvoking";

        public override void Execute(Context Context)
        {
            foreach(var TypeDef in Context.DnModule.Types.Where(x => x.HasMethods))
            {
                foreach(var MethodDef in TypeDef.Methods.Where(x => x.HasBody))
                {
                    var IL = MethodDef.Body.Instructions;
                    for (int x= 0; x < IL.Count; x++)
                    {
                        if(IL[x].OpCode == OpCodes.Call &&
                           IL[x].Operand.ToString().Contains("Assembly::GetCallingAssembly"))
                        {
                            Context.Log.Info("Found Anti Invoke !");
                            IL[x].Operand = Context.DnModule.Import(typeof(System.Reflection.Assembly).GetMethod(nameof(System.Reflection.Assembly.GetExecutingAssembly)));
                            Context.Log.Debug("Patched Anti Invoke Successfly");
                        }
                    }
                }
            }
        }
    }
}