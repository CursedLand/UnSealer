
#region Usings
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnSealer.Core;
#endregion

namespace UnSealer.Protections.Devirtualizers.CawkVM
{
    public class Disassembler
    {
        /// <summary>
        /// Initalize Disassembler.
        /// </summary>
        /// <param name="owner">Method That Get Disassembled.</param>
        /// <param name="source">Raw Cawk Code.</param>
        public Disassembler(MethodDefinition owner,
                            byte[] source)
        {
            Owner = owner ?? throw new ArgumentNullException("OwnerMethod is Required.");
            Data = source ?? throw new ArgumentNullException("Main Data Must Be Not Null.");
            _reader = new(new MemoryStream(Data));
            _importer = new(Owner.Module);
            _body = new CilMethodBody(Owner);
        }
        /// <summary>
        /// Build New Disassemblied CilBody
        /// </summary>
        /// <returns>New Method Body.</returns>
        public CilMethodBody BeginRead()
        {
            foreach (CilLocalVariable Local in Owner.CilMethodBody.LocalVariables.Take(Owner.CilMethodBody.LocalVariables.Count - 2)) // Skiping Locals Madden By CawkVM
            {
                _body.LocalVariables.Add(new(_importer.ImportTypeSignature(Local.VariableType)));
            }

            int ExceptionHandlersCount = _reader.ReadInt32();
            ExceptionClause[] Claues = new ExceptionClause[ExceptionHandlersCount];
            /* ExceptionHandler Restoration. */
            for (int x = 0; x < ExceptionHandlersCount; x++)
            {
                uint TypeToken = (uint)_reader.ReadInt32();
                int StartFilter = _reader.ReadInt32();
                int HandlerEnd = _reader.ReadInt32();
                int HandlerStart = _reader.ReadInt32();
                byte HandlerType = _reader.ReadByte();
                int TryEnd = _reader.ReadInt32();
                int TryStart = _reader.ReadInt32();
                Claues[x] = new ExceptionClause()
                {
                    HandlerType = HandlerType switch
                    {
                        1 => CilExceptionHandlerType.Exception,
                        2 => null, // No Duplicates.
                        3 => CilExceptionHandlerType.Fault,
                        4 => CilExceptionHandlerType.Filter,
                        5 => CilExceptionHandlerType.Finally,
                        _ => throw new Exception("Unknown HandlerType.")
                    },
                    HandlerEnd = HandlerEnd,
                    FilterStart = StartFilter,
                    HandlerStart = HandlerStart,
                    CatchType = (int)TypeToken == -1
                    ? null /* catch { } */
                    : _importer.ImportType((ITypeDefOrRef)Owner.Module.LookupMember(new(TypeToken))), /* catch(...) { } */
                    TryEnd = TryEnd,
                    TryStart = TryStart
                };
            }
            int InstructionsCount = _reader.ReadInt32();
            for (int q = 0; q < InstructionsCount; q++)
            {
                _body.Instructions.Add(new CilInstruction(CilOpCodes.Nop));
                _body.Instructions.CalculateOffsets();
            }
            /* Instruction Restoration. */
            for (int i = 0; i < InstructionsCount; i++)
            {
                CilOpCode OpCode = Utils.GetCilOpCode(_reader.ReadInt16()); // Read OpCode by their Short Value.
                switch (_reader.ReadByte())
                {
                    case 0: _body.Instructions[i] = new(OpCode); break; // Inline None.
                    case 1: _body.Instructions[i] = new(OpCode, _importer.ImportMethod((IMethodDescriptor)Owner.Module.LookupMember(new((uint)_reader.ReadInt32())))); break; // Inline Method
                    case 2: _body.Instructions[i] = new(OpCode, _reader.ReadString()); break; // InlineString
                    case 3: _body.Instructions[i] = new(OpCode, _reader.ReadInt32()); break; // InlineI
                    case 5: _body.Instructions[i] = new(OpCode, _importer.ImportField((IFieldDescriptor)Owner.Module.LookupMember(new((uint)_reader.ReadInt32())))); break; // Inline Field
                    case 6: _body.Instructions[i] = new(OpCode, _importer.ImportType((ITypeDefOrRef)Owner.Module.LookupMember(new((uint)_reader.ReadInt32())))); break; // Inline Type
                    case 7: int Index = _reader.ReadInt32(); _body.Instructions[i] = new(OpCode, Index); break; // ShortInlineBrTarget;
                    case 8: _body.Instructions[i] = new(OpCode, _reader.ReadByte()); break; // ShortInline
                    case 9:
                        int Count = _reader.ReadInt32();
                        int[] Labels = new int[Count];
                        for (int x = 0; x < Count; x++)
                        {
                            int SIndex = _reader.ReadInt32();
                            Labels[x] = SIndex;
                        }
                        _body.Instructions[i] = new(OpCode, Labels);
                        break; // Inline Switch
                    case 10: int BIndex = _reader.ReadInt32(); _body.Instructions[i] = new(OpCode, BIndex); break; // InlineBrTarget
                    case 11:
                        int Token = _reader.ReadInt32();
                        byte Prefix = _reader.ReadByte();
                        switch (Prefix)
                        {
                            case 0: _body.Instructions[i] = new(OpCode, _importer.ImportField((IFieldDescriptor)Owner.Module.LookupMember(new((uint)Token)))); break; // fieldof(...)
                            case 1: _body.Instructions[i] = new(OpCode, _importer.ImportType((ITypeDefOrRef)Owner.Module.LookupMember(new((uint)Token)))); break; // typeof(...)
                            case 2: _body.Instructions[i] = new(OpCode, _importer.ImportMethod((IMethodDescriptor)Owner.Module.LookupMember(new((uint)Token)))); break; // methodof(...)
                        }
                        break; // InlineTok.
                    case 13: _body.Instructions[i] = new(OpCode, BitConverter.ToSingle(_reader.ReadBytes(4), 0)); break;   // ShortInline
                    case 14: _body.Instructions[i] = new(OpCode, _reader.ReadDouble()); break; // InlineR
                    case 15: _body.Instructions[i] = new(OpCode, _reader.ReadInt64()); break; // InlineI8
                    case 4:
                    case 12:
                        int PIndex = _reader.ReadInt32();
                        if (_reader.ReadByte() == 0)
                        {
                            _body.Instructions[i] = new(OpCode, _body.LocalVariables[PIndex]);
                        }
                        else
                        {
                            _body.Instructions[i] = new(OpCode, PIndex == 0 && Owner.Signature.HasThis ? Owner.Parameters.ThisParameter : Owner.Parameters[Owner.Signature.HasThis ? PIndex - 1 : PIndex]);
                        }

                        break; // (Inline/Short)Var

                }
            }
            /* Branch Fixing. */
            for (int v = 0; v < InstructionsCount; v++)
            {
                switch (_body.Instructions[v].OpCode.OperandType)
                {
                    case CilOperandType.InlineSwitch: /* switch(...) */
                        int[] Labels = (int[])_body.Instructions[v].Operand;
                        List<ICilLabel> NewLabels = new List<ICilLabel>();
                        foreach (int l in Labels)
                        {
                            NewLabels.Add(_body.Instructions[l].CreateLabel());
                        }

                        _body.Instructions[v].Operand = NewLabels;
                        break;
                    case CilOperandType.ShortInlineBrTarget: /* br_s , bgt_s , etc. */
                    case CilOperandType.InlineBrTarget:  /* brfalse , brtrue , etc. */
                        ICilLabel Label = _body.Instructions[(int)_body.Instructions[v].Operand].CreateLabel();
                        _body.Instructions[v].Operand = Label;
                        break;
                }
            }
            /* ExceptionHandlers Setting. */
            for (int z = 0; z < ExceptionHandlersCount; z++)
            {
                ExceptionClause Clause = Claues[z];
                CilExceptionHandler EhHandler = new CilExceptionHandler()
                {
                    ExceptionType = Clause.CatchType,
                    FilterStart = Clause.FilterStart == -1 ? null : _body.Instructions[Clause.FilterStart].CreateLabel(),
                    HandlerStart = _body.Instructions[Clause.HandlerStart].CreateLabel(),
                    HandlerEnd = _body.Instructions[Clause.HandlerEnd].CreateLabel(),
                    TryStart = _body.Instructions[Clause.TryStart].CreateLabel(),
                    HandlerType = Clause.HandlerType.Value,
                    TryEnd = _body.Instructions[Clause.TryEnd].CreateLabel()
                };
                _body.ExceptionHandlers.Add(EhHandler);
            }
            _body.Instructions.CalculateOffsets(); // Calculate New Offsets.
            _body.Instructions.ExpandMacros();
            _body.Instructions.OptimizeMacros(); // Serialize Body.
            return _body;
        }
        public byte[] Data { get; }
        public MethodDefinition Owner { get; }

        #region PrivateFields
        private CilMethodBody _body;
        private BinaryReader _reader;
        private ReferenceImporter _importer;
        #endregion
    }
    internal struct ExceptionClause
    {
        public ITypeDefOrRef CatchType;
        public int FilterStart;
        public int HandlerEnd;
        public int HandlerStart;
        public CilExceptionHandlerType? HandlerType;
        public int TryEnd;
        public int TryStart;
    }
}