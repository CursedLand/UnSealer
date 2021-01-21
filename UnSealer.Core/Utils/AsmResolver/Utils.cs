using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

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
        public static void DiscoverMethod(string[] args, Logger Log)
        {
            foreach (var Type in ModuleDefinition.FromFile(args[0]).GetAllTypes())
                foreach (var Method in Type.Methods)
                    if (Method.Name == args[1] && Method.Parameters.Count == int.Parse(args[2])) {
                        DecMethod = Method;
                        Log.Info($"Decryption Method Found Params : {DecMethod.Resolve().Parameters.Count} :D");
                    }
        }
        #endregion
    }
}