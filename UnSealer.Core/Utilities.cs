
#region Usings
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using UnSealer.Core;
#endregion

namespace UnSealer.Core
{
    public static class Utilities {
		#region Extensions
		public static IEnumerable<CilInstruction> GetBranches(this CilMethodBody body) {
			var Branchs = new List<CilInstruction>();
			var IL = body.Instructions;
			foreach (var i in IL.Where(x => x.OpCode.OperandType == CilOperandType.InlineBrTarget || x.OpCode.OperandType == CilOperandType.InlineSwitch || x.OpCode.OperandType == CilOperandType.ShortInlineBrTarget)) {
				if (i.OpCode.OperandType == CilOperandType.InlineSwitch)
					Branchs.AddRange((i.Operand as IList<ICilLabel>).Select(x => IL.GetByOffset(x.Offset)));
				else
					Branchs.Add(IL.GetByOffset((i.Operand as ICilLabel).Offset));
			}
			foreach (var Handler in body.ExceptionHandlers) {
				if (Handler.FilterStart != null) Branchs.Add(IL.GetByOffset(Handler.FilterStart.Offset));
				if (Handler.HandlerEnd != null) Branchs.Add(IL.GetByOffset(Handler.HandlerEnd.Offset));
				if (Handler.HandlerStart != null) Branchs.Add(IL.GetByOffset(Handler.HandlerStart.Offset));
				if (Handler.TryEnd != null) Branchs.Add(IL.GetByOffset(Handler.TryEnd.Offset));
				if (Handler.TryStart != null) Branchs.Add(IL.GetByOffset(Handler.TryStart.Offset));
			}
			return Branchs;
		}
		public static bool IsProxy(this MethodDefinition Method, out CilInstruction inlinemethod) {
			inlinemethod = null;
			if (!Method.HasMethodBody)
				return false;
			if (Method.CilMethodBody == null)
				return false;
			if (Method.CilMethodBody.Instructions.Count == 0)
				return false;
			var instructions = Method.CilMethodBody.Instructions;
			int ParametersCount = Method.Signature.GetTotalParameterCount();
			if (instructions.Count != ParametersCount + 2)
				return false;
			var InlineMethodInstr = instructions[ParametersCount];
			if (InlineMethodInstr.IsCode(CilCode.Call) || 
				InlineMethodInstr.IsCode(CilCode.Callvirt) ||
				InlineMethodInstr.IsCode(CilCode.Newobj)) {
				inlinemethod = InlineMethodInstr;
				return true;
			}
			return false;
		}
		public static IEnumerable<MetadataMember> Purify(this ProtectionTargets NeededTargets, IEnumerable<MetadataMember> Targets) 
			=> NeededTargets switch {
				ProtectionTargets.None => null,
				ProtectionTargets.AllDefinitions => Targets,
				ProtectionTargets.Events => Targets.Where(Target => Target is EventDefinition),
				ProtectionTargets.Fields => Targets.Where(Target => Target is IFieldDescriptor),
				ProtectionTargets.Methods => Targets.Where(Target => Target is IMethodDefOrRef),
				ProtectionTargets.Properties => Targets.Where(Target => Target is PropertyDefinition),
				ProtectionTargets.Types => Targets.Where(Target => Target is ITypeDescriptor || Target is ITypeDefOrRef),
				_ => throw new ArgumentOutOfRangeException(nameof(NeededTargets)),
			};
		public static IEnumerable<MetadataMember> GetDefs(this ModuleDefinition Module) {
			foreach (var Type in Module.GetAllTypes()) {
				yield return Type;
				foreach (var Method in Type.Methods)
					yield return Method;
				foreach (var Field in Type.Fields)
					yield return Field;
				foreach (var Property in Type.Properties)
					yield return Property;
				foreach (var Event in Type.Events)
					yield return Event;
			}
		}
		public static ITypeDefOrRef GetFromCorlib(this Context context, string ns, string name)
			=> context.Mscorlib.GetAllTypes().SingleOrDefault(x => x.Name == name && x.Namespace == ns);
		public static bool IsFromElement(this TypeSignature signature, ElementType elementType)
			=> signature.ElementType == elementType;
		public static bool IsCode(this CilInstruction instruction, CilCode code)
			=> instruction.OpCode.Code == code;
		public static bool IsArithmetic(this CilInstruction i)
			=> i.IsCode(CilCode.Add) ||
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
		public static bool IsMethod(this CilInstruction instruction, string ClassName, string MethodName)
			=> instruction.ToString().Contains(ClassName + "::" + MethodName);
		public static bool IsFromNS(this CilInstruction instruction, string NS, string TypeName)
			=> instruction.Operand is IMethodDescriptor && ((IMethodDescriptor)instruction.Operand).DeclaringType.Namespace == NS && ((IMethodDescriptor)instruction.Operand).DeclaringType.Name == TypeName;
		public static void Nop(this IEnumerable<CilInstruction> instructions) {
			foreach (var i in instructions)
				i.Nop();
		}
		public static void Nop(this CilInstruction instruction) {
			instruction.OpCode = CilOpCodes.Nop;
			instruction.Operand = null;
		}
		public static bool Is32Module(this ModuleDefinition module)
			=> module.IsBit32Preferred || module.IsBit32Required;
        #endregion
        #region Cryptography
        public static byte[] DecryptXor(byte[] EncryptedArray)
		{
			Random random = new Random(23546654);
			byte[] array = new byte[EncryptedArray.Length];
			for (int i = 0; i < EncryptedArray.Length; i++)
			{
				array[i] = (byte)(EncryptedArray[i] ^ random.Next(0, 250));
			}
			return array;
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
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using ICryptoTransform transform = alg.CreateDecryptor();
				using CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
				cryptoStream.Write(message, 0, message.Length);
				cryptoStream.FlushFinalBlock();
				result = memoryStream.ToArray();
			}
			return result;
		}
		public static byte[] Decrypt(byte[] key, byte[] message)
		{
			using RijndaelManaged rijndaelManaged = new RijndaelManaged();
			rijndaelManaged.Key = key;
			rijndaelManaged.IV = key;
			return DecryptBytes(rijndaelManaged, message);
		}
		private static byte[] XorB(byte[] toEncrypt, int len)
		{
			string text = "HCP";
			for (int i = 0; i < len; i++)
			{
				toEncrypt[i] = (byte)(toEncrypt[i] ^ text[i % text.Length]);
			}
			return toEncrypt;
		}
		public static byte[] BDerive(byte[] data, int datalen, byte[] key, int keylen)
		{
			int num = 12;
			int num2 = 14;
			int num3 = 258;
			for (int i = 0; i < keylen; i++)
			{
				num3 += num3 % (key[i] + 1);
			}
			for (int j = 0; j < datalen; j++)
			{
				num3 = key[j % keylen] + num3;
				num = (num3 + 5) * (num & 0xFF) + (num >> 8);
				num2 = (num3 + 7) * (num2 & 0xFF) + (num2 >> 8);
				num3 = ((num << 8) + num2) & 0xFF;
				data[j] = (byte)(data[j] ^ num3);
			}
			return XorB(data, datalen);
		}
        #endregion
	}
}