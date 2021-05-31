
#region Usings
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
#endregion

namespace UnSealer.Core
{
    public static class Utils
    {
        #region DeobfuscationUtils
        public static bool IsProxy(this MethodDefinition Method, out CilInstruction inlinemethod) {
            inlinemethod = null;
            if (!Method.HasMethodBody) 
                return false;
            if (Method.CilMethodBody is null) 
                return false;
            if (Method.CilMethodBody.Instructions.Count is 0)
                return false;
            var Instructions = Method.CilMethodBody.Instructions;
            var ParamertersCount = Method.Signature.GetTotalParameterCount();
            if (Instructions.Count != (ParamertersCount + 2))
                return false;
            var CallInstruction = Instructions[ParamertersCount];
            if (CallInstruction.OpCode == CilOpCodes.Call || CallInstruction.OpCode == CilOpCodes.Callvirt || CallInstruction.OpCode == CilOpCodes.Newobj) {
                inlinemethod = CallInstruction;
                return true;
            }
            else {
                return false;
            }
        }
        #endregion
        #region Extentions
        public static IEnumerable<MetadataMember> Purify(this ProtectionTargets NeededTargets,
            IEnumerable<MetadataMember> Targets)
        {
            return NeededTargets switch
            {
                ProtectionTargets.None => null,
                ProtectionTargets.AllDefinitions => Targets,
                ProtectionTargets.Events => Targets.Where(Target => Target is EventDefinition),
                ProtectionTargets.Fields => Targets.Where(Target => Target is IFieldDescriptor),
                ProtectionTargets.Methods => Targets.Where(Target => Target is IMethodDefOrRef),
                ProtectionTargets.Properties => Targets.Where(Target => Target is PropertyDefinition),
                ProtectionTargets.Types => Targets.Where(Target => Target is ITypeDescriptor || Target is ITypeDefOrRef),
                _ => throw new ArgumentOutOfRangeException(nameof(NeededTargets))
            };
        }

        public static IEnumerable<MetadataMember> GetDefs(this ModuleDefinition Module)
        {
            foreach (TypeDefinition _type in Module.GetAllTypes())
            {
                yield return _type;

                foreach (MethodDefinition _method in _type.Methods)
                {
                    yield return _method;
                }

                foreach (FieldDefinition _field in _type.Fields)
                {
                    yield return _field;
                }

                foreach (PropertyDefinition _property in _type.Properties)
                {
                    yield return _property;
                }

                foreach (EventDefinition _event in _type.Events)
                {
                    yield return _event;
                }
            }
        }
        public static ITypeDefOrRef GetFromCorlib(this Context context, string ns, string name)
        {
            return context.Mscorlib.GetAllTypes().SingleOrDefault(x => x.Name == name && x.Namespace == ns);
        }
        public static bool IsFromElement(this TypeSignature signature, ElementType elementType)
            => signature.ElementType == elementType;
        public static bool IsCode(this CilInstruction instruction, CilCode code)
            => instruction.OpCode.Code == code;
        public static bool IsArithmetic(this CilInstruction i)
        {
            return i.IsCode(CilCode.Add) ||
                   i.IsCode(CilCode.And) ||
                   i.IsCode(CilCode.Sub) ||
                   i.IsCode(CilCode.Mul) ||
                   i.IsCode(CilCode.Div) ||
                   i.IsCode(CilCode.Rem) ||
                   i.IsCode(CilCode.Neg) ||
                   i.IsCode(CilCode.Not) ||
                   i.IsCode(CilCode.Xor) ||
                   i.IsCode(CilCode.Shl) ||
                   i.IsCode(CilCode.Shr) ||
                   i.IsCode(CilCode.Shr_Un) ||
                   i.IsCode(CilCode.Or);
        }
        public static bool IsMethod(this CilInstruction instruction, string ClassName, string MethodName)
            => instruction.ToString().Contains($"{ClassName}::{MethodName}");
        public static bool IsFromNS(this CilInstruction instruction, string NS, string TypeName)
            => instruction.Operand is IMethodDescriptor && ((IMethodDescriptor) instruction.Operand).DeclaringType.Namespace == NS && ((IMethodDescriptor) instruction.Operand).DeclaringType.Name == TypeName;
        public static void Nop(this IEnumerable<CilInstruction> instructions)
        {
            foreach (var i in instructions)
                i.Nop();
        }
        public static bool IsLdcI4(this CilInstruction instruction, out object value) {
            if (instruction.IsLdcI4() || instruction.IsCode(CilCode.Ldc_I8) || instruction.IsCode(CilCode.Ldc_R4) || instruction.IsCode(CilCode.Ldc_R8)) {
                value = instruction.Operand;
                return true;
            }
            else {
                value = null;
                return false;
            }
        }
        public static void Nop(this CilInstruction instruction) {
            instruction.OpCode = CilOpCodes.Nop;
            instruction.Operand = null;
        }
        public static bool Is32Module(this ModuleDefinition module)
            => module.IsBit32Preferred || module.IsBit32Required;
        #endregion
        #region CryptoUtils
        public static byte[] DecryptXor(byte[] EncryptedArray)
        {
            Random rand = new Random(23546654);
            byte[] Decrypted = new byte[EncryptedArray.Length];
            for (int i = 0; i < EncryptedArray.Length; i++)
            {
                Decrypted[i] = (byte)(EncryptedArray[i] ^ rand.Next(0, 250));
            }
            return Decrypted;
        }
        private static byte[] DecryptBytes(SymmetricAlgorithm alg, byte[] message)
        {
            if (message == null || message.Length == 0)
            {
                return message;
            }
            if (alg == null)
            {
                throw new ArgumentNullException("alg is null");
            }
            byte[] result;
            using (MemoryStream stream = new MemoryStream())
            {
                using (ICryptoTransform decryptor = alg.CreateDecryptor())
                {
                    using (CryptoStream encrypt = new CryptoStream(stream, decryptor, CryptoStreamMode.Write))
                    {
                        encrypt.Write(message, 0, message.Length);
                        encrypt.FlushFinalBlock();
                        result = stream.ToArray();
                    }
                }
            }
            return result;
        }
        public static byte[] Decrypt(byte[] key, byte[] message)
        {
            byte[] result;
            using (RijndaelManaged rijndael = new RijndaelManaged())
            {
                rijndael.Key = key;
                rijndael.IV = key;
                result = DecryptBytes(rijndael, message);
            }
            return result;
        }
        private static byte[] XorB(byte[] toEncrypt, int len)
        {
            string key = "HCP";
            for (int i = 0; i < len; i++)
            {
                toEncrypt[i] = (byte)((char)toEncrypt[i] ^ key[i % key.Length]);
            }
            return toEncrypt;
        }
        public static byte[] BDerive(byte[] data, int datalen, byte[] key, int keylen)
        {
            int N = 12;
            int N2 = 14;
            int NS = 258;
            for (int I = 0; I < keylen; I++)
            {
                NS += NS % (key[I] + 1);
            }
            for (int I = 0; I < datalen; I++)
            {
                NS = key[I % keylen] + NS;
                N = (NS + 5) * (N & 255) + (N >> 8);
                N2 = (NS + 7) * (N2 & 255) + (N2 >> 8);
                NS = ((N << 8) + N2 & 255);
                data[I] = (byte)(data[I] ^ NS);
            }
            return XorB(data, datalen);
        }
        #endregion
        #region Misc
        public static bool Is32Bit(this ModuleDefinition module)
        {
            return module.IsBit32Preferred || module.IsBit32Required;
        }

        public static CilOpCode GetCilOpCode(short value)
        {
            if (_opcodecache.ContainsKey(value))
            {
                return _opcodecache[value];
            }
            else
            {
                foreach (System.Reflection.FieldInfo _field in typeof(CilOpCodes).GetFields().Where(x => x.FieldType == typeof(CilOpCode))) // No Shit Exceptions Please :)
                {
                    if (((short)((CilOpCode)_field.GetValue(null)).Code) == value)
                    {
                        CilOpCode val = (CilOpCode)_field.GetValue(null); // Yes I use Reflection (Insted Of MultiByteOpCodes or SingleByteOpCodes) :/
                        _opcodecache[value] = val;
                        return val;
                    }
                }
            }
            throw new Exception("Unknown OpCode.");
        }
        public static IEnumerable<CilInstruction> GetBranches(CilMethodBody body)
        {
            List<CilInstruction> Branches = new List<CilInstruction>();
            CilInstructionCollection Instructions = body.Instructions;
            foreach (CilInstruction Instr in Instructions.Where(x => x.OpCode.OperandType == CilOperandType.InlineBrTarget || x.OpCode.OperandType == CilOperandType.InlineSwitch || x.OpCode.OperandType == CilOperandType.ShortInlineBrTarget))
            {
                Branches.Add(Instructions.GetByOffset((Instr.Operand as ICilLabel).Offset));
            }

            foreach (CilExceptionHandler EHandler in body.ExceptionHandlers)
            {
                if (EHandler.FilterStart != null)
                {
                    Branches.Add(Instructions.GetByOffset(EHandler.FilterStart.Offset));
                }

                if (EHandler.HandlerEnd != null)
                {
                    Branches.Add(Instructions.GetByOffset(EHandler.HandlerEnd.Offset));
                }

                if (EHandler.HandlerStart != null)
                {
                    Branches.Add(Instructions.GetByOffset(EHandler.HandlerStart.Offset));
                }

                if (EHandler.TryEnd != null)
                {
                    Branches.Add(Instructions.GetByOffset(EHandler.TryEnd.Offset));
                }

                if (EHandler.TryStart != null)
                {
                    Branches.Add(Instructions.GetByOffset(EHandler.TryStart.Offset));
                }
            }
            return Branches;
        }
        #endregion
        #region Private Fields
        private static Dictionary<short, CilOpCode> _opcodecache = new();
        #endregion
    }
}