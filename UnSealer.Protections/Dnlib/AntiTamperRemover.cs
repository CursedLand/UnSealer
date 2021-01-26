using dnlib.IO;
using UnSealer.Core;
using UnSealer.Core.Utils.Dnlib.AntiTamperUtils;

namespace UnSealer.Protections.Dnlib
{
    public class AntiTamperRemover : Protection
    {
        public override string Name => "Cfex Anti Tamper Remover";

        public override string Author => "Unknown";

        public override string Description => "ConfuserEx AntiTamper Remover";

        public override ProtectionType Type => ProtectionType.Dnlib;

        public override void Execute(Context Context)
        {
            bool? IsTampred = Utils.IsTampered(Context.DnModule);
            bool Temp = true;
            if ((IsTampred.GetValueOrDefault() == Temp & IsTampred != null) && Utils.CheckTamper(Context.DnModule))
            {
                Context.Log.Info("Anti Tamper Detected");
                DataReader Reader = Context.DnModule.Metadata.PEImage.CreateReader();
                byte[] rawbytes = Reader.ReadBytes((int)Reader.Length);
                try
                {
                    Context.DnModule = Utils.UnAntiTamper(Context.DnModule, rawbytes);
                    Utils.RemoveCall(Context.DnModule.GlobalType);
                    Context.Log.Info("Anti Tamper Removed Successfully");
                }
                catch
                {
                    Context.Log.Error("Failed To Remove Anti Tamper");
                }
            }
        }
    }
}