#region Weavisor
// Arx One Aspects
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.Weavisor.Weaver.Utility
{
    using System;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Collections.Generic;

    /// <summary>
    /// Allows to Emit() to Instructions set and keep a cursor
    /// This way, this class allows to apped or insert IL instructions in method body
    /// </summary>
    public class Instructions
    {
        private readonly Collection<Instruction> _instructions;

        /// <summary>
        /// Gets or sets the cursor.
        /// </summary>
        /// <value>
        /// The cursor.
        /// </value>
        public int Cursor { get; set; }

        public Instructions(Collection<Instruction> instructions)
        {
            _instructions = instructions;
        }

        private Instructions Insert(Instruction instruction)
        {
            _instructions.Insert(Cursor++, instruction);
            return this;
        }

        public Instructions Emit(OpCode opCode)
        {
            return Insert(Instruction.Create(opCode));
        }

        public Instructions Emit(OpCode opCode, short value)
        {
            return Insert(Instruction.Create(opCode, value));
        }

        public Instructions Emit(OpCode opCode, int value)
        {
            return Insert(Instruction.Create(opCode, value));
        }

        public Instructions Emit(OpCode opCode, TypeReference value)
        {
            return Insert(Instruction.Create(opCode, value));
        }

        public Instructions Emit(OpCode opCode, MethodReference value)
        {
            return Insert(Instruction.Create(opCode, value));
        }

        public Instructions Emit(OpCode opCode, string value)
        {
            return Insert(Instruction.Create(opCode, value));
        }

        /// <summary>
        /// Emits a ldarg instruction
        /// </summary>
        /// <param name="index">argument index</param>
        /// <returns></returns>
        public Instructions EmitLdarg(int index)
        {
            switch (index)
            {
                case 0:
                    return Emit(OpCodes.Ldarg_0);
                case 1:
                    return Emit(OpCodes.Ldarg_1);
                case 2:
                    return Emit(OpCodes.Ldarg_2);
                case 3:
                    return Emit(OpCodes.Ldarg_3);
                default:
                    return Emit(OpCodes.Ldarg_S, (short)index);
            }
        }

        /// <summary>
        /// Emits a ldloc.
        /// </summary>
        /// <param name="variableDefinition">The variable definition.</param>
        public Instructions EmitLdloc(VariableDefinition variableDefinition)
        {
            switch (variableDefinition.Index)
            {
                case 0:
                    return Emit(OpCodes.Ldloc_0);
                case 1:
                    return Emit(OpCodes.Ldloc_1);
                case 2:
                    return Emit(OpCodes.Ldloc_2);
                case 3:
                    return Emit(OpCodes.Ldloc_3);
                default:
                    return Emit(OpCodes.Ldloc_S, (short)variableDefinition.Index);
            }
        }

        /// <summary>
        /// Emits a stloc.
        /// </summary>
        /// <param name="variableDefinition">The variable definition.</param>
        public Instructions EmitStloc(VariableDefinition variableDefinition)
        {
            switch (variableDefinition.Index)
            {
                case 0:
                    return Emit(OpCodes.Stloc_0);
                case 1:
                    return Emit(OpCodes.Stloc_1);
                case 2:
                    return Emit(OpCodes.Stloc_2);
                case 3:
                    return Emit(OpCodes.Stloc_3);
                default:
                    return Emit(OpCodes.Stloc_S, (short)variableDefinition.Index);
            }
        }

        /// <summary>
        /// Emits a Ldc.
        /// </summary>
        /// <param name="value">The value.</param>
        public Instructions EmitLdc(int value)
        {
            switch (value)
            {
                case 0:
                    return Emit(OpCodes.Ldc_I4_0);
                case 1:
                    return Emit(OpCodes.Ldc_I4_1);
                case 2:
                    return Emit(OpCodes.Ldc_I4_2);
                case 3:
                    return Emit(OpCodes.Ldc_I4_3);
                case 4:
                    return Emit(OpCodes.Ldc_I4_4);
                case 5:
                    return Emit(OpCodes.Ldc_I4_5);
                case 6:
                    return Emit(OpCodes.Ldc_I4_6);
                case 7:
                    return Emit(OpCodes.Ldc_I4_7);
                case 8:
                    return Emit(OpCodes.Ldc_I4_8);
                default:
                    if (value < 128)
                        return Emit(OpCodes.Ldc_I4_S, (byte)value);
                    return Emit(OpCodes.Ldc_I4, value);
            }
        }

        /// <summary>
        /// Emits unbox or cast when necessary.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        public Instructions EmitUnboxOrCastIfNecessary(TypeReference targetType)
        {
            if (targetType.IsValueType)
                return Emit(OpCodes.Unbox_Any, targetType);
            if (!targetType.SafeEquivalent(targetType.Module.Import(typeof(object))))
                return Emit(OpCodes.Castclass, targetType);
            return this;
        }

        /// <summary>
        /// Emits box when necessary.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        public Instructions EmitBoxIfNecessary(TypeReference targetType)
        {
            if (targetType.IsValueType || targetType.IsGenericParameter)
                return Emit(OpCodes.Box, targetType);
            return this;
        }

        /// <summary>
        /// Emits a ldind.
        /// </summary>
        /// <param name="type">The type.</param>
        public Instructions EmitLdind(TypeReference type)
        {
            if (type.IsValueType)
            {
                if (type.SafeEquivalent(type.Module.Import(typeof(Byte))))
                    return Emit(OpCodes.Ldind_U1);
                if (type.SafeEquivalent(type.Module.Import(typeof(SByte))))
                    return Emit(OpCodes.Ldind_I1);
                if (type.SafeEquivalent(type.Module.Import(typeof(Int16))))
                    return Emit(OpCodes.Ldind_I2);
                if (type.SafeEquivalent(type.Module.Import(typeof(UInt16))))
                    return Emit(OpCodes.Ldind_U2);
                if (type.SafeEquivalent(type.Module.Import(typeof(Int32))))
                    return Emit(OpCodes.Ldind_I4);
                if (type.SafeEquivalent(type.Module.Import(typeof(UInt32))))
                    return Emit(OpCodes.Ldind_U4);
                if (type.SafeEquivalent(type.Module.Import(typeof(Int64))) || type.SafeEquivalent(type.Module.Import(typeof(UInt64))))
                    return Emit(OpCodes.Ldind_I8);
                if (type.SafeEquivalent(type.Module.Import(typeof(Single))))
                    return Emit(OpCodes.Ldind_R4);
                if (type.SafeEquivalent(type.Module.Import(typeof(Double))))
                    return Emit(OpCodes.Ldind_R8);
                return Emit(OpCodes.Ldobj, type);
            }
            return Emit(OpCodes.Ldind_Ref);
        }

        /// <summary>
        /// Emits a stind.
        /// </summary>
        /// <param name="type">The type.</param>
        public Instructions EmitStind(TypeReference type)
        {
            if (type.IsValueType)
            {
                if (type.SafeEquivalent(type.Module.Import(typeof(Byte))) || type.SafeEquivalent(type.Module.Import(typeof(SByte))))
                    return Emit(OpCodes.Stind_I1);
                if (type.SafeEquivalent(type.Module.Import(typeof(Int16))) || type.SafeEquivalent(type.Module.Import(typeof(UInt16))))
                    return Emit(OpCodes.Stind_I2);
                if (type.SafeEquivalent(type.Module.Import(typeof(Int32))) || type.SafeEquivalent(type.Module.Import(typeof(UInt32))))
                    return Emit(OpCodes.Stind_I4);
                if (type.SafeEquivalent(type.Module.Import(typeof(Int64))) || type.SafeEquivalent(type.Module.Import(typeof(UInt64))))
                    return Emit(OpCodes.Stind_I8);
                if (type.SafeEquivalent(type.Module.Import(typeof(Single))))
                    return Emit(OpCodes.Stind_R4);
                if (type.SafeEquivalent(type.Module.Import(typeof(Double))))
                    return Emit(OpCodes.Stind_R8);
                return Emit(OpCodes.Stobj, type);
            }
            return Emit(OpCodes.Stind_Ref);
        }
    }
}