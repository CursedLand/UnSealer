using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using System;

namespace UnSealer.Core.Utils.AsmResolver
{
    public static class Utils
    {
        #region UtilsFields
        public static IMethodDescriptor DecMethod;
        #endregion

        #region UtilsMethods
        public static CilOpCode GetCilCode(ElementType returnType)
        {
            switch (returnType)
            {
                case ElementType.Boolean:
                    return CilOpCodes.Ldc_I4;
                case ElementType.Char:
                    return CilOpCodes.Ldc_I4;
                case ElementType.I1:
                    return CilOpCodes.Ldc_I4;
                case ElementType.U1:
                    return CilOpCodes.Ldc_I4;
                case ElementType.I2:
                    return CilOpCodes.Ldc_I4;
                case ElementType.U2:
                    return CilOpCodes.Ldc_I4;
                case ElementType.I4:
                    return CilOpCodes.Ldc_I4;
                case ElementType.U4:
                    return CilOpCodes.Ldc_I4;
                case ElementType.I8:
                    return CilOpCodes.Ldc_I8;
                case ElementType.U8:
                    return CilOpCodes.Ldc_I8;
                case ElementType.R4:
                    return CilOpCodes.Ldc_R4;
                case ElementType.R8:
                    return CilOpCodes.Ldc_R8;
            }
            return CilOpCodes.Nop;
        }
        public static void DiscoverMethod(string[] args, Logger Log, bool IsMD, string MDToken)
        {
            var Temp = ModuleDefinition.FromFile(args[0]);
            if (IsMD) {
                DecMethod = (IMethodDescriptor)Temp.LookupMember(new MetadataToken((uint)Convert.ToInt32(MDToken, 16)));
                Log.Info($"Decryption Method Found Params : {DecMethod.Resolve().Parameters.Count} :D");
                return;
            }
            foreach (var Type in Temp.GetAllTypes())
                foreach (var Method in Type.Methods)
                    if (Method.Name == args[1] && Method.Parameters.Count == int.Parse(args[2])) {
                        DecMethod = Method;
                        Log.Info($"Decryption Method Found Params : {DecMethod.Resolve().Parameters.Count} :D");
                    }
        }
        #endregion
    }
}