
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
//use FrameWork 2.0 for compatibility on older files
namespace UnSealer.Core.Utils.Dnlib.CawkRuntime.ConversionBack
{
    public class ConvertBack
    {
        public static object locker2 = new object();
        public static DynamicMethod Runner(int position, int size, int ID, object[] parameters, MethodBase Meth)
        {

            DynamicMethod value;
            parameters = parameters;
            if (cache.TryGetValue(ID, out value))//Check cache to see if method has already been created
            {
                return (DynamicMethod)value.Invoke(null, parameters);//if it has directly invoke the method instead of converting it to dynamicmethod
            }
            else
            {
                MethodBase callingMethod = Meth;//get calling method, using this can prevent invocation and dynamic unpacking
                var grabbedBytes = byteArrayGrabber(Initialize.byteArrayResource, position, size);
                var decryptionKey = MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(callingMethod.Name));
                var ab = callingMethod.GetMethodBody().GetILAsByteArray();
                Initialize.bc(grabbedBytes, grabbedBytes.Length, ab, ab.Length);
                var decrypted = Decrypt(decryptionKey, grabbedBytes);
                return ConversionBack(decrypted, ID, parameters, callingMethod);//if not convert the method to a dynamic method
            }
        }


        public static DynamicMethod ConversionBack(byte[] bytes, int ID, object[] parameters, MethodBase callingMethod)
        {


            MethodBody methodBody = callingMethod.GetMethodBody();//get calling methods body 
            BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes));//cast byte[] Position


            var methodParameters = callingMethod.GetParameters();//get its parameters
            var allLocals = new List<LocalBuilder>();
            var _allExceptionHandlerses = new List<ExceptionHandlerClass>();
            Type[] parametersArray;
            int start = 0;
            if (callingMethod.IsStatic)//check if the method is static or not
            {
                parametersArray = new Type[methodParameters.Length];//if method is static set the parameters to the amount in calling method
            }
            else
            {
                parametersArray = new Type[methodParameters.Length + 1];//if its not static this means there is an additional hidden parameter (this.) this is always used as the first parameter so we need to account for this
                parametersArray[0] = callingMethod.DeclaringType;
                start = 1;
            }
            for (var i = 0; i < methodParameters.Length; i++)
            {
                var parameterInfo = methodParameters[i];
                parametersArray[start + i] = parameterInfo.ParameterType;//set parameter types
            }
            DynamicMethod dynamicMethod = new DynamicMethod("", callingMethod.MemberType == MemberTypes.Constructor ? null : ((MethodInfo)callingMethod).ReturnParameter.ParameterType, parametersArray, Initialize.callingModule, true);//create the dynamic method
            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();//get ilgenerator
            var locs = methodBody.LocalVariables;
            var locals = new Type[locs.Count];
            foreach (var localVariableInfo in locs)
                allLocals.Add(ilGenerator.DeclareLocal(localVariableInfo.LocalType));//declare the local for use of stloc,ldloc/ldloca
            var exceptionHandlersCount = binaryReader.ReadInt32();//read amount of exception handlers
            processExceptionHandler(binaryReader, exceptionHandlersCount, callingMethod, _allExceptionHandlerses);//convert exception handlers
            var sortedExceptionHandlers = fixAndSortExceptionHandlers(_allExceptionHandlerses);//we need to sort the exception handlers incase there is multiple handlers that start at the same instruction
            var instructionCount = binaryReader.ReadInt32();//read the amount of instructions
            var _allLabelsDictionary = new Dictionary<int, Label>();

            for (var u = 0; u < instructionCount; u++)
            {
                var label = ilGenerator.DefineLabel();//we need to label each instruction to use with branches

                _allLabelsDictionary.Add(u, label);
            }


            for (var i = 0; i < instructionCount; i++)
            {
                checkAndSetExceptionHandler(sortedExceptionHandlers, i, ilGenerator);//we check the instruction against our exception handlers to determine if we need to start/end any handlers
                var opcode = binaryReader.ReadInt16();//read opcode short this will relate to the correct opcode
                OpCode opc;
                if (opcode >= 0 && opcode < Initialize.oneByteOpCodes.Length)
                {
                    opc = Initialize.oneByteOpCodes[opcode];//we check against one byte opcodes
                }
                else
                {
                    var b2 = (byte)(opcode | 0xFE00);
                    opc = Initialize.twoByteOpCodes[b2];//check against two byte opcodes
                }

                ilGenerator.MarkLabel(_allLabelsDictionary[i]);//we now need to mark the label in the ilgenerator
                var operandType = binaryReader.ReadByte();//we get the operand type

                HandleOpType(operandType, opc, ilGenerator, binaryReader, _allLabelsDictionary, allLocals);//we process the instruction with ilgenerator
            }
            lock (locker)//we lock threads here to prevent exceptions of item already exists
            {
                if (!cache.ContainsKey(ID))
                {
                    cache.Add(ID, dynamicMethod);//add to cache if first time creating method
                }
            }

            return dynamicMethod;//.Invoke(null,parameters);//.Invoke(null, parameters);//invoke the dynamic method which is the users original method and return the result
        }
        public static object locker = new object();
        public static Dictionary<int, DynamicMethod> cache = new Dictionary<int, DynamicMethod>();

        /// <summary>
        /// We handle operand type and convert this to a real instruction
        /// </summary>
        /// <param name="opType"></param>
        /// <param name="opcode"></param>
        /// <param name="ilGenerator"></param>
        /// <param name="binaryReader"></param>
        /// <param name="_allLabelsDictionary"></param>
        /// <param name="allLocals"></param>
        private static void HandleOpType(int opType, OpCode opcode, ILGenerator ilGenerator, BinaryReader binaryReader, Dictionary<int, Label> _allLabelsDictionary, List<LocalBuilder> allLocals)
        {
            switch (opType)//we switch on operand type
            {
                case 0:
                    InlineNoneEmitter(ilGenerator, opcode, binaryReader);
                    break;
                case 1:
                    InlineMethodEmitter(ilGenerator, opcode, binaryReader);
                    break;
                case 2:
                    InlineStringEmitter(ilGenerator, opcode, binaryReader);
                    break;
                case 3:
                    InlineIEmitter(ilGenerator, opcode, binaryReader);
                    break;

                case 5:
                    InlineFieldEmitter(ilGenerator, opcode, binaryReader);
                    break;
                case 6:
                    InlineTypeEmitter(ilGenerator, opcode, binaryReader);
                    break;
                case 7:
                    ShortInlineBrTargetEmitter(ilGenerator, opcode, binaryReader, _allLabelsDictionary);
                    break;
                case 8:
                    ShortInlineIEmitter(ilGenerator, opcode, binaryReader);
                    break;
                case 9:
                    InlineSwitchEmitter(ilGenerator, opcode, binaryReader, _allLabelsDictionary);
                    break;
                case 10:
                    InlineBrTargetEmitter(ilGenerator, opcode, binaryReader, _allLabelsDictionary);
                    break;
                case 11:
                    InlineTokEmitter(ilGenerator, opcode, binaryReader);
                    break;
                case 12:
                case 4:
                    InlineVarEmitter(ilGenerator, opcode, binaryReader, allLocals);
                    break;
                case 13:
                    ShortInlineREmitter(ilGenerator, opcode, binaryReader);
                    break;
                case 14:
                    InlineREmitter(ilGenerator, opcode, binaryReader);
                    break;
                case 15:
                    InlineI8Emitter(ilGenerator, opcode, binaryReader);
                    break;
                default:
                    throw new Exception("Operand Type Unknown " + opType);
            }
        }
        /// <summary>
        /// this operand type does nothing it is for opcodes that have no operands
        /// </summary>
        /// <param name="ilGenerator"></param>
        /// <param name="opcode"></param>
        /// <param name="binaryReader"></param>
        private static void InlineNoneEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader)
        {
            ilGenerator.Emit(opcode);
        }

        /// <summary>
        /// this is for calling of methods where it will resolve the metadata token that relates to the method
        /// </summary>
        /// <param name="ilGenerator"></param>
        /// <param name="opcode"></param>
        /// <param name="binaryReader"></param>
        private static void InlineMethodEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader)
        {
            var mdtoken = binaryReader.ReadInt32();
            var resolvedMethodBase = Initialize.callingModule.ResolveMethod(mdtoken);
            if (resolvedMethodBase is MethodInfo)
                ilGenerator.Emit(opcode, (MethodInfo)resolvedMethodBase);
            else if (resolvedMethodBase is ConstructorInfo)
                ilGenerator.Emit(opcode, (ConstructorInfo)resolvedMethodBase);
            else
                throw new Exception("Check resolvedMethodBase Type");
        }
        /// <summary>
        /// This is for operands that handle variables and parameters we need to emit the label that it relates to which we defined earlier
        /// </summary>
        /// <param name="ilGenerator"></param>
        /// <param name="opcode"></param>
        /// <param name="binaryReader"></param>
        /// <param name="allLocals"></param>
        private static void InlineVarEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader, List<LocalBuilder> allLocals)
        {
            var index = binaryReader.ReadInt32();
            var parOrloc = binaryReader.ReadByte();
            if (parOrloc == 0)
            {
                var label = allLocals[index];
                ilGenerator.Emit(opcode, label);
            }
            else
            {
                ilGenerator.Emit(opcode, index);
            }

        }

        /// <summary>
        /// read the string from the byte[] and emit the opcode with this string
        /// </summary>
        /// <param name="ilGenerator"></param>
        /// <param name="opcode"></param>
        /// <param name="binaryReader"></param>
        private static void InlineStringEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader)
        {
            var readString = binaryReader.ReadString();
            ilGenerator.Emit(opcode, readString);
        }
        private static void InlineIEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader)
        {
            var readInt32 = binaryReader.ReadInt32();

            ilGenerator.Emit(opcode, readInt32);
        }

        private static void InlineFieldEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader)
        {
            int mdtoken = binaryReader.ReadInt32();
            FieldInfo fieldInfo = Initialize.callingModule.ResolveField(mdtoken);
            ilGenerator.Emit(opcode, fieldInfo);
        }

        private static void InlineTypeEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader)
        {
            int mdtoken = binaryReader.ReadInt32();
            Type type = Initialize.callingModule.ResolveType(mdtoken);
            ilGenerator.Emit(opcode, type);
        }

        private static void ShortInlineBrTargetEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader, Dictionary<int, Label> _allLabelsDictionary)
        {
            int index = binaryReader.ReadInt32();
            var location = _allLabelsDictionary[index];
            ilGenerator.Emit(opcode, location);
        }

        private static void ShortInlineIEmitter(ILGenerator ilGenerator, OpCode opCode, BinaryReader binaryReader)
        {
            byte b = binaryReader.ReadByte();
            ilGenerator.Emit(opCode, b);
        }
        private static void ShortInlineREmitter(ILGenerator ilGenerator, OpCode opCode, BinaryReader binaryReader)
        {
            var value = binaryReader.ReadBytes(4);
            var myFloat = BitConverter.ToSingle(value, 0);
            ilGenerator.Emit(opCode, myFloat);
        }
        private static void InlineREmitter(ILGenerator ilGenerator, OpCode opCode, BinaryReader binaryReader)
        {
            var value = binaryReader.ReadDouble();

            ilGenerator.Emit(opCode, value);
        }
        private static void InlineI8Emitter(ILGenerator ilGenerator, OpCode opCode, BinaryReader binaryReader)
        {
            var value = binaryReader.ReadInt64();

            ilGenerator.Emit(opCode, value);
        }

        private static void InlineSwitchEmitter(ILGenerator ilGenerator, OpCode opCode, BinaryReader binaryReader, Dictionary<int, Label> _allLabelsDictionary)
        {
            int count = binaryReader.ReadInt32();
            Label[] allLabels = new Label[count];
            for (int i = 0; i < count; i++)
            {
                allLabels[i] = _allLabelsDictionary[binaryReader.ReadInt32()];

            }
            ilGenerator.Emit(opCode, allLabels);
        }
        private static void InlineBrTargetEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader, Dictionary<int, Label> _allLabelsDictionary)
        {
            int index = binaryReader.ReadInt32();
            var location = _allLabelsDictionary[index];
            ilGenerator.Emit(opcode, location);
        }

        private static void InlineTokEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader)
        {
            int mdtoken = binaryReader.ReadInt32();
            byte type = binaryReader.ReadByte();
            if (type == 0)
            {
                var fieldinfo = Initialize.callingModule.ResolveField(mdtoken);
                ilGenerator.Emit(opcode, fieldinfo);
            }
            else if (type == 1)
            {
                var typeInfo = Initialize.callingModule.ResolveType(mdtoken);
                ilGenerator.Emit(opcode, typeInfo);
            }
            else if (type == 2)
            {
                var methodinfo = Initialize.callingModule.ResolveMethod(mdtoken);
                if (methodinfo is MethodInfo)
                    ilGenerator.Emit(opcode, (MethodInfo)methodinfo);
                else if (methodinfo is ConstructorInfo)
                    ilGenerator.Emit(opcode, (ConstructorInfo)methodinfo);
            }
        }

        public static void checkAndSetExceptionHandler(List<FixedExceptionHandlersClass> sorted, int i, ILGenerator ilGenerator)
        {
            foreach (var allExceptionHandlerse in sorted)
                if (allExceptionHandlerse.HandlerType == 1)
                {
                    if (allExceptionHandlerse.TryStart == i)
                    {
                        ilGenerator.BeginExceptionBlock();

                    }
                    if (allExceptionHandlerse.HandlerEnd == i)
                    {
                        ilGenerator.EndExceptionBlock();
                    }
                    if (allExceptionHandlerse.HandlerStart.Contains(i))
                    {
                        var indes = allExceptionHandlerse.HandlerStart.IndexOf(i);
                        ilGenerator.BeginCatchBlock(allExceptionHandlerse.CatchType[indes]);
                    }
                }
                else if (allExceptionHandlerse.HandlerType == 5)
                {
                    if (allExceptionHandlerse.TryStart == i)
                        ilGenerator.BeginExceptionBlock();
                    else if (allExceptionHandlerse.HandlerEnd == i)
                        ilGenerator.EndExceptionBlock();
                    else if (allExceptionHandlerse.TryEnd == i)
                        ilGenerator.BeginFinallyBlock();
                }
        }

        public static void processExceptionHandler(BinaryReader bin, int count, MethodBase method, List<ExceptionHandlerClass> _allExceptionHandlerses)
        {
            for (var i = 0; i < count; i++)
            {
                //catchType
                var expExceptionHandlers = new ExceptionHandlerClass();
                var catchTypeMdToken = bin.ReadInt32();
                if (catchTypeMdToken == -1)
                {
                    expExceptionHandlers.CatchType = null;
                }
                else
                {
                    var catchType = method.Module.ResolveType(catchTypeMdToken);
                    expExceptionHandlers.CatchType = catchType;
                }

                //filterStart
                var filterStartIndex = bin.ReadInt32();
                expExceptionHandlers.FilterStart = filterStartIndex;
                //handlerEnd
                var handlerEnd = bin.ReadInt32();
                expExceptionHandlers.HandlerEnd = handlerEnd;
                //handlerStart
                var handlerStart = bin.ReadInt32();
                expExceptionHandlers.HandlerStart = handlerStart;
                //handlerType
                var handlerType = bin.ReadByte();
                switch (handlerType)
                {
                    case 1:
                        expExceptionHandlers.HandlerType = 1;
                        break;
                    case 2:
                        expExceptionHandlers.HandlerType = 2;
                        break;
                    case 3:
                        expExceptionHandlers.HandlerType = 3;
                        break;
                    case 4:
                        expExceptionHandlers.HandlerType = 4;
                        break;
                    case 5:
                        expExceptionHandlers.HandlerType = 5;
                        break;
                    default:
                        throw new Exception("Out of Range");
                }
                //tryEnd 
                var tryEnd = bin.ReadInt32();
                expExceptionHandlers.TryEnd = tryEnd;
                //tryStart
                var tryStart = bin.ReadInt32();
                expExceptionHandlers.TryStart = tryStart;
                _allExceptionHandlerses.Add(expExceptionHandlers);
            }
        }

        public static List<FixedExceptionHandlersClass> fixAndSortExceptionHandlers(List<ExceptionHandlerClass> expHandlers)
        {
            var multiExp = new List<ExceptionHandlerClass>();
            var exceptionDictionary = new Dictionary<ExceptionHandlerClass, int>();
            foreach (var handler in expHandlers)
                if (handler.HandlerType == 5)
                {
                    exceptionDictionary.Add(handler, handler.TryStart);
                }
                else
                {
                    if (exceptionDictionary.ContainsValue(handler.TryStart))
                        if (handler.CatchType != null)
                            exceptionDictionary.Add(handler, handler.TryStart);
                        else
                            multiExp.Add(handler);

                    else
                        exceptionDictionary.Add(handler, handler.TryStart);
                }
            var sorted = new List<FixedExceptionHandlersClass>();
            foreach (var keyValuePair in exceptionDictionary)
            {
                if (keyValuePair.Key.HandlerType == 5)
                {
                    var fixedExceptionHandlers = new FixedExceptionHandlersClass();
                    fixedExceptionHandlers.TryStart = keyValuePair.Key.TryStart;
                    fixedExceptionHandlers.TryEnd = keyValuePair.Key.TryEnd;
                    fixedExceptionHandlers.FilterStart = keyValuePair.Key.FilterStart;
                    fixedExceptionHandlers.HandlerEnd = keyValuePair.Key.HandlerEnd;

                    fixedExceptionHandlers.HandlerType = keyValuePair.Key.HandlerType;

                    fixedExceptionHandlers.HandlerStart.Add(keyValuePair.Key.HandlerStart);
                    fixedExceptionHandlers.CatchType.Add(keyValuePair.Key.CatchType);

                    sorted.Add(fixedExceptionHandlers);
                    continue;
                }
                var rrr = WhereAlternate(multiExp, keyValuePair.Value);
                if (rrr.Count == 0)
                {
                    var fixedExceptionHandlers = new FixedExceptionHandlersClass();
                    fixedExceptionHandlers.TryStart = keyValuePair.Key.TryStart;
                    fixedExceptionHandlers.TryEnd = keyValuePair.Key.TryEnd;
                    fixedExceptionHandlers.FilterStart = keyValuePair.Key.FilterStart;
                    fixedExceptionHandlers.HandlerEnd = keyValuePair.Key.HandlerEnd;

                    fixedExceptionHandlers.HandlerType = keyValuePair.Key.HandlerType;

                    fixedExceptionHandlers.HandlerStart.Add(keyValuePair.Key.HandlerStart);
                    fixedExceptionHandlers.CatchType.Add(keyValuePair.Key.CatchType);

                    sorted.Add(fixedExceptionHandlers);
                }
                else
                {
                    var fixedExceptionHandlers = new FixedExceptionHandlersClass();
                    fixedExceptionHandlers.TryStart = keyValuePair.Key.TryStart;
                    fixedExceptionHandlers.TryEnd = keyValuePair.Key.TryEnd;
                    fixedExceptionHandlers.FilterStart = keyValuePair.Key.FilterStart;
                    fixedExceptionHandlers.HandlerEnd = rrr[rrr.Count - 1].HandlerEnd;

                    fixedExceptionHandlers.HandlerType = keyValuePair.Key.HandlerType;
                    fixedExceptionHandlers.HandlerStart.Add(keyValuePair.Key.HandlerStart);
                    fixedExceptionHandlers.CatchType.Add(keyValuePair.Key.CatchType);
                    foreach (var exceptionHandlerse in rrr)
                    {
                        fixedExceptionHandlers.HandlerStart.Add(exceptionHandlerse.HandlerStart);
                        fixedExceptionHandlers.CatchType.Add(exceptionHandlerse.CatchType);
                    }
                    sorted.Add(fixedExceptionHandlers);
                }
            }
            return sorted;
        }

        public static List<ExceptionHandlerClass> WhereAlternate(List<ExceptionHandlerClass> exp, int val)
        {
            var returnList = new List<ExceptionHandlerClass>();
            //    var rrr = MultiExp.Where(i => i.tryStart == keyValuePair.Value && i.HandlerType != 5);
            foreach (var handlers2 in exp)
                if (handlers2.TryStart == val && handlers2.HandlerType != 5)
                    returnList.Add(handlers2);
            return returnList;
        }
        public static byte[] byteArrayGrabber(byte[] bytes, int skip, int take)
        {
            byte[] newBarray = new byte[take];
            int y = 0;
            for (int i = 0; i < take; i++, y++)
            {
                byte curByte = bytes[skip + i];
                newBarray[y] = curByte;
            }

            return newBarray;

        }
        private static byte[] DecryptBytes(
           SymmetricAlgorithm alg,
           byte[] message)
        {
            if (message == null || message.Length == 0)
                return message;

            if (alg == null)
                throw new ArgumentNullException("alg is null");

            using (var stream = new MemoryStream())
            using (var decryptor = alg.CreateDecryptor())
            using (var encrypt = new CryptoStream(stream, decryptor, CryptoStreamMode.Write))
            {
                encrypt.Write(message, 0, message.Length);
                encrypt.FlushFinalBlock();
                return stream.ToArray();
            }
        }
        public static byte[] Decrypt(byte[] key, byte[] message)
        {
            using (var rijndael = new RijndaelManaged())
            {
                rijndael.Key = key;
                rijndael.IV = key;
                return DecryptBytes(rijndael, message);
            }
        }
    }
}
