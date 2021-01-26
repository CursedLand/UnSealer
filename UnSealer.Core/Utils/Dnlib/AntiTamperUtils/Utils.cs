using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.PE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnSealer.Core.Utils.Dnlib.AntiTamperUtils
{
    public class Utils
    {
		public static bool? IsTampered(ModuleDefMD DnModule)
		{
			IList<ImageSectionHeader> PE = DnModule.Metadata.PEImage.ImageSectionHeaders;
			foreach (ImageSectionHeader PESection in PE)
			{
				string displayName = PESection.DisplayName;
				if (!(displayName == ".text") && !(displayName == ".rsrc") && !(displayName == ".reloc"))
				{
					return new bool?(true);
				}
			}
			return null;
		}
		public static bool CheckTamper(ModuleDefMD module)
		{
			bool IsTampred;
			MethodDef cctor = module.GlobalType.FindStaticConstructor();
			if (cctor == null)
			{
				IsTampred = false;
			}
			else
			{
				for (int i = 0; i < cctor.Body.Instructions.Count; i++)
				{
					if (cctor.Body.Instructions[i].OpCode == OpCodes.Call)
					{
						try
						{
							MethodDef method = (MethodDef)cctor.Body.Instructions[i].Operand;
							for (int z = 0; z < method.Body.Instructions.Count; z++)
							{
								if (method.Body.Instructions[z].OpCode == OpCodes.Call)
								{
									string operand = method.Body.Instructions[z].Operand.ToString();
									if (operand.Contains("GetHINSTANCE"))
									{
										if (method.FindInstructionsNumber(OpCodes.Call, "(System.IntPtr,System.UInt32,System.UInt32,System.UInt32&)") == 1)
										{
											ATMeth = method;
											return true;
										}
									}
								}
							}
						}
						catch
						{
						}
					}
				}
				IsTampred = false;
			}
			return IsTampred;
		}
		public static ModuleDefMD UnAntiTamper(ModuleDefMD module, byte[] rawbytes)
		{
			dynInstr = new List<Instruction>();
			initialKeys = new uint[4];
			cctor = module.GlobalType.FindStaticConstructor();
			antitamp = (cctor.Body.Instructions[0].Operand as MethodDef);
			ModuleDefMD result;
			if (antitamp == null)
			{
				result = null;
			}
			else
			{
				IList<ImageSectionHeader> imageSectionHeaders = module.Metadata.PEImage.ImageSectionHeaders;
				ImageSectionHeader confSec = imageSectionHeaders[0];
				FindInitialKeys(antitamp);
				if (initialKeys == null)
				{
					result = null;
				}
				else
				{
					input = new MemoryStream(rawbytes);
					reader = new BinaryReader(input);
					Hash1(input, reader, imageSectionHeaders, confSec);
					arrayKeys = GetArrayKeys();
					DecryptMethods(reader, confSec, input);
					ModuleDefMD fmd2 = ModuleDefMD.Load(input);
					fmd2.GlobalType.FindStaticConstructor().Body.Instructions.RemoveAt(0);
					result = fmd2;
				}
			}
			return result;
		}
		private static void DecryptMethods(BinaryReader reader, ImageSectionHeader confSec, Stream stream)
		{
			int num = (int)(confSec.SizeOfRawData >> 2);
			int pointerToRawData = (int)confSec.PointerToRawData;
			stream.Position = (long)pointerToRawData;
			uint[] numArray = new uint[num];
			uint i = 0U;
			while ((ulong)i < (ulong)((long)num))
			{
				uint num2 = reader.ReadUInt32();
				numArray[(int)i] = (num2 ^ arrayKeys[(int)((IntPtr)((long)((ulong)(i & 15U))))]);
				arrayKeys[(int)((IntPtr)((long)((ulong)(i & 15U))))] = num2 + 1035675673U;
				i += 1U;
			}
			byteResult = new byte[num << 2];
			byteResult = numArray.SelectMany(new Func<uint, IEnumerable<byte>>(BitConverter.GetBytes)).ToArray<byte>();
			byte[] byteArray = ConvertUInt32ArrayToByteArray(numArray);
			stream.Position = (long)pointerToRawData;
			stream.Write(byteResult, 0, byteResult.Length);
		}
		private static byte[] ConvertUInt32ArrayToByteArray(uint[] value)
		{
			byte[] result = new byte[value.Length * 4];
			for (int index = 0; index < value.Length; index++)
			{
				byte[] partialResult = BitConverter.GetBytes(value[index]);
				for (int indexTwo = 0; indexTwo < partialResult.Length; indexTwo++)
				{
					result[index * 4 + indexTwo] = partialResult[indexTwo];
				}
			}
			return result;
		}
		private static uint[] GetArrayKeys()
		{
			uint[] dst = new uint[16];
			uint[] src = new uint[16];
			for (int i = 0; i < 16; i++)
			{
				dst[i] = initialKeys[3];
				src[i] = initialKeys[1];
				initialKeys[0] = (initialKeys[1] >> 5 | initialKeys[1] << 27);
				initialKeys[1] = (initialKeys[2] >> 3 | initialKeys[2] << 29);
				initialKeys[2] = (initialKeys[3] >> 7 | initialKeys[3] << 25);
				initialKeys[3] = (initialKeys[0] >> 11 | initialKeys[0] << 21);
			}
			return DeriveKeyAntiTamp(dst, src);
		}
		private static void FindInitialKeys(MethodDef antitamp)
		{
			int count = antitamp.Body.Instructions.Count;
			int num2 = count - 293;
			for (int i = 0; i < count; i++)
			{
				Instruction item = antitamp.Body.Instructions[i];
				bool flag = item.OpCode.Equals(OpCodes.Ldc_I4);
				if (flag)
				{
					bool flag2 = antitamp.Body.Instructions[i + 1].OpCode.Equals(OpCodes.Stloc_S);
					if (flag2)
					{
						bool flag3 = antitamp.Body.Instructions[i + 1].Operand.ToString().Contains("V_10");
						if (flag3)
						{
							initialKeys[0] = (uint)((int)item.Operand);
						}
						bool flag4 = antitamp.Body.Instructions[i + 1].Operand.ToString().Contains("V_11");
						if (flag4)
						{
							initialKeys[1] = (uint)((int)item.Operand);
						}
						bool flag5 = antitamp.Body.Instructions[i + 1].Operand.ToString().Contains("V_12");
						if (flag5)
						{
							initialKeys[2] = (uint)((int)item.Operand);
						}
						bool flag6 = antitamp.Body.Instructions[i + 1].Operand.ToString().Contains("V_13");
						if (flag6)
						{
							initialKeys[3] = (uint)((int)item.Operand);
						}
					}
				}
			}
		}
		public static uint[] DeriveKeyAntiTamp(uint[] dst, uint[] src)
		{
			uint[] numArray = new uint[16];
			for (int i = 0; i < 16; i++)
			{
				switch (i % 3)
				{
					case 0:
						numArray[i] = (dst[i] ^ src[i]);
						break;
					case 1:
						numArray[i] = dst[i] * src[i];
						break;
					case 2:
						numArray[i] = dst[i] + src[i];
						break;
				}
			}
			return numArray;
		}
		public static void RemoveCall(TypeDef type)
		{
			MethodDef tampMethod = null;
			foreach (MethodDef tamperMethod in type.Methods)
			{
				if (tamperMethod != null)
				{
					bool IsCctorMethod = !tamperMethod.DeclaringType.IsGlobalModuleType;
					if (!IsCctorMethod)
					{
						bool IsPrivate = tamperMethod.Attributes != (MethodAttributes.Private | MethodAttributes.FamANDAssem | MethodAttributes.Static | MethodAttributes.HideBySig);
						if (!IsPrivate)
						{
							bool ILCode = tamperMethod.CodeType > MethodImplAttributes.IL;
							if (!ILCode)
							{
								bool IsVoid = tamperMethod.ReturnType.ElementType != ElementType.Void;
								if (!IsVoid)
								{
									bool IsTamperCall = tamperMethod.FindInstructionsNumber(OpCodes.Call, "(System.IntPtr,System.UInt32,System.UInt32,System.UInt32&)") != 1;
									if (!IsTamperCall)
									{
										tampMethod = tamperMethod;
									}
								}
							}
						}
					}
				}
			}
			bool flag7 = tampMethod != null;
			if (flag7)
			{
				type.Methods.Remove(tampMethod);
			}
		}
		private static void Hash1(Stream stream, BinaryReader reader, IList<ImageSectionHeader> sections, ImageSectionHeader confSec)
		{
			foreach (ImageSectionHeader header in sections)
			{
				bool flag = header != confSec && header.DisplayName != "";
				if (flag)
				{
					int num = (int)(header.SizeOfRawData >> 2);
					int pointerToRawData = (int)header.PointerToRawData;
					stream.Position = (long)pointerToRawData;
					for (int i = 0; i < num; i++)
					{
						uint num2 = reader.ReadUInt32();
						uint num3 = (initialKeys[0] ^ num2) + initialKeys[1] + initialKeys[2] * initialKeys[3];
						initialKeys[0] = initialKeys[1];
						initialKeys[1] = initialKeys[2];
						initialKeys[1] = initialKeys[3];
						initialKeys[3] = num3;
					}
				}
			}
		}
		#region Fields
		public static string DirectoryName = "";
        private static MethodDef antitamp;
		private static uint[] arrayKeys;
		private static byte[] byteResult;
		private static MethodDef cctor;
		private static List<Instruction> dynInstr;
		private static uint[] initialKeys;
		private static BinaryReader reader;
		private static MemoryStream input;
		private static MethodDef ATMeth;
        #endregion
    }
	public static class Extensions
    {
		public static int FindInstructionsNumber(this MethodDef Method, OpCode TargetOpCode, object WantedOperand)
		{
			int num = 0;
			foreach (Instruction i in Method.Body.Instructions)
			{
				if (i.OpCode == TargetOpCode)
				{
					if (WantedOperand is int)
					{
						int value = i.GetLdcI4Value();
						if (value == (int)WantedOperand)
						{
							num++;
						}
					}
					else
					{
						if (WantedOperand is string)
						{
							string Operand = i.Operand.ToString();
							if (Operand.Contains(WantedOperand.ToString()))
							{
								num++;
							}
						}
					}
				}
			}
			return num;
		}
	}
}