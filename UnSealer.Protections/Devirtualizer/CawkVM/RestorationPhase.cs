
#region Usings
using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.PE.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnSealer.Core;
#endregion

namespace UnSealer.Protections.Devirtualizer.CawkVM {
    public class RestorationPhase : ProtectionPhase {
        public RestorationPhase(Protection ParentBase)
            : base(ParentBase) { }

        public override string Name => "Restoration Phase";

        public override ProtectionTargets PhaseTargets => ProtectionTargets.Methods;

        public override void Execute(Context context, IEnumerable<MetadataMember> targets) {
            var MethodsTables = context.Module.DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetTable<MethodDefinitionRow>();  // Methods Rows in .Net Binary that Get Access to Rawbytes of Methods without reflection Usage.
            foreach (var VirualizedMethod in CawkVM.CawkKey.VirtualizatedMethods) {
                try {
                    #region Decryption
                    byte[] RawBytes = CawkVM.CawkKey.Data.Skip(VirualizedMethod.Position).Take(VirualizedMethod.Size).ToArray();
                    byte[] HashKey = MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(VirualizedMethod.Parent.Name));
                    var ReaderRef = MethodsTables.GetByRid(VirualizedMethod.Parent.MetadataToken.Rid).Body as PESegmentReference?;
                    var Reader = ReaderRef.Value.CreateReader();
                    byte[] RawBody = ((DataSegment)CilRawMethodBody.FromReader(null, ref Reader).Code).Data;
                    Utilities.BDerive(RawBytes, RawBytes.Length, RawBody, RawBody.Length);
                    #endregion
                    var Disassembler = new Disassembler(VirualizedMethod.Parent, Utilities.Decrypt(HashKey, RawBytes));
                    VirualizedMethod.Parent.CilMethodBody = Disassembler.BeginRead();
                    context.Logger.InfoFormat("Done Restoring {0} Instruction.",
                        VirualizedMethod.Parent.CilMethodBody.Instructions.Count);
                }
                catch (Exception) /* It will not but maybe ¯\_(ツ)_/¯ */ {
                    context.Logger.WarnFormat("Method {0} Failed Devirtualizaing.", VirualizedMethod.Parent.Name);
                }
            }
            var Resources = new List<string>() {
                "Eddy^CZ‎",
                "Eddy^CZ_‎",
                "RT",
                "X64",
                "X86"
            };
            foreach (var Resource in context.Module.Resources.Where(x => Resources.Contains(x.Name)).ToArray()) 
                context.Module.Resources.Remove(Resource);
        }
    }
}
