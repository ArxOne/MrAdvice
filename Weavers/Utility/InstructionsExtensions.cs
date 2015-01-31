#region Weavisor
// Arx One Aspects
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Release under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.Weavisor.Weaver.Utility
{
    using System;
    using System.Collections.Generic;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    /// <summary>
    /// Extensions to Instructions
    /// </summary>
    public static class InstructionExtensions
    {
        public static void Emit(this ICollection<Instruction> instructions, OpCode opCode)
        {
            instructions.Add(Instruction.Create(opCode));
        }

        public static void Emit(this ICollection<Instruction> instructions, OpCode opCode, short value)
        {
            instructions.Add(Instruction.Create(opCode, value));
        }

        public static void Emit(this ICollection<Instruction> instructions, OpCode opCode, int value)
        {
            instructions.Add(Instruction.Create(opCode, value));
        }

        public static void Emit(this ICollection<Instruction> instructions, OpCode opCode, TypeReference value)
        {
            instructions.Add(Instruction.Create(opCode, value));
        }

        public static void Emit(this ICollection<Instruction> instructions, OpCode opCode, MethodReference value)
        {
            instructions.Add(Instruction.Create(opCode, value));
        }

        /// <summary>
        /// Emits a ldarg instruction
        /// </summary>
        /// <param name="instructions"></param>
        /// <param name="index">argument index</param>
        public static void EmitLdarg(this ICollection<Instruction> instructions, int index)
        {
            switch (index)
            {
                case 0:
                    instructions.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    instructions.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    instructions.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    instructions.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    instructions.Emit(OpCodes.Ldarg_S, (short)index);
                    break;
            }
        }

        /// <summary>
        /// Emits a ldloc.
        /// </summary>
        /// <param name="instructions">The il generator.</param>
        /// <param name="variableDefinition">The variable definition.</param>
        public static void EmitLdloc(this ICollection<Instruction> instructions, VariableDefinition variableDefinition)
        {
            switch (variableDefinition.Index)
            {
                case 0:
                    instructions.Emit(OpCodes.Ldloc_0);
                    break;
                case 1:
                    instructions.Emit(OpCodes.Ldloc_1);
                    break;
                case 2:
                    instructions.Emit(OpCodes.Ldloc_2);
                    break;
                case 3:
                    instructions.Emit(OpCodes.Ldloc_3);
                    break;
                default:
                    instructions.Emit(OpCodes.Ldloc_S, (short)variableDefinition.Index);
                    break;
            }
        }

        /// <summary>
        /// Emits a stloc.
        /// </summary>
        /// <param name="instructions">The il generator.</param>
        /// <param name="variableDefinition">The variable definition.</param>
        public static void EmitStloc(this ICollection<Instruction> instructions, VariableDefinition variableDefinition)
        {
            switch (variableDefinition.Index)
            {
                case 0:
                    instructions.Emit(OpCodes.Stloc_0);
                    break;
                case 1:
                    instructions.Emit(OpCodes.Stloc_1);
                    break;
                case 2:
                    instructions.Emit(OpCodes.Stloc_2);
                    break;
                case 3:
                    instructions.Emit(OpCodes.Stloc_3);
                    break;
                default:
                    instructions.Emit(OpCodes.Stloc_S, (short)variableDefinition.Index);
                    break;
            }
        }

        /// <summary>
        /// Emits a Ldc.
        /// </summary>
        /// <param name="instructions">The il generator.</param>
        /// <param name="value">The value.</param>
        public static void EmitLdc(this ICollection<Instruction> instructions, int value)
        {
            switch (value)
            {
                case 0:
                    instructions.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    instructions.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    instructions.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    instructions.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    instructions.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    instructions.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    instructions.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    instructions.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    instructions.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    if (value < 128)
                        instructions.Emit(OpCodes.Ldc_I4_S, (byte)value);
                    else
                        instructions.Emit(OpCodes.Ldc_I4, value);
                    break;
            }
        }

        /// <summary>
        /// Emits unbox or cast when necessary.
        /// </summary>
        /// <param name="instructions">The il generator.</param>
        /// <param name="targetType">Type of the target.</param>
        public static void EmitUnboxOrCastIfNecessary(this ICollection<Instruction> instructions, TypeReference targetType)
        {
            if (targetType.IsValueType)
                instructions.Emit(OpCodes.Unbox_Any, targetType);
            else if (!targetType.SafeEquivalent(targetType.Module.Import(typeof(object))))
                instructions.Emit(OpCodes.Castclass, targetType);
        }

        /// <summary>
        /// Emits box when necessary.
        /// </summary>
        /// <param name="instructions">The il generator.</param>
        /// <param name="targetType">Type of the target.</param>
        public static void EmitBoxIfNecessary(this ICollection<Instruction> instructions, TypeReference targetType)
        {
            if (targetType.IsValueType)
                instructions.Emit(OpCodes.Box, targetType);
        }

        /// <summary>
        /// Emits a ldind.
        /// </summary>
        /// <param name="instructions">The il generator.</param>
        /// <param name="type">The type.</param>
        public static void EmitLdind(this ICollection<Instruction> instructions, TypeReference type)
        {
            if (type.IsValueType)
            {
                if (type.SafeEquivalent(type.Module.Import(typeof(Byte))))
                {
                    instructions.Emit(OpCodes.Ldind_U1);
                    return;
                }
                if (type.SafeEquivalent(type.Module.Import(typeof(SByte))))
                {
                    instructions.Emit(OpCodes.Ldind_I1);
                    return;
                }
                if (type.SafeEquivalent(type.Module.Import(typeof(Int16))))
                {
                    instructions.Emit(OpCodes.Ldind_I2);
                    return;
                }
                if (type.SafeEquivalent(type.Module.Import(typeof(UInt16))))
                {
                    instructions.Emit(OpCodes.Ldind_U2);
                    return;
                }
                if (type.SafeEquivalent(type.Module.Import(typeof(Int32))))
                {
                    instructions.Emit(OpCodes.Ldind_I4);
                    return;
                }
                if (type.SafeEquivalent(type.Module.Import(typeof(UInt32))))
                {
                    instructions.Emit(OpCodes.Ldind_U4);
                    return;
                }
                if (type.SafeEquivalent(type.Module.Import(typeof(Int64))) || type.SafeEquivalent(type.Module.Import(typeof(UInt64))))
                {
                    instructions.Emit(OpCodes.Ldind_I8);
                    return;
                }
                if (type.SafeEquivalent(type.Module.Import(typeof(Single))))
                {
                    instructions.Emit(OpCodes.Ldind_R4);
                    return;
                }
                if (type.SafeEquivalent(type.Module.Import(typeof(Double))))
                {
                    instructions.Emit(OpCodes.Ldind_R8);
                    return;
                }
                instructions.Emit(OpCodes.Ldobj, type);
            }
            else
            {
                instructions.Emit(OpCodes.Ldind_Ref);
            }
        }

        /// <summary>
        /// Emits a stind.
        /// </summary>
        /// <param name="instructions">The il generator.</param>
        /// <param name="type">The type.</param>
        public static void EmitStind(this ICollection<Instruction> instructions, TypeReference type)
        {
            if (type.IsValueType)
            {
                if (type .SafeEquivalent( type.Module.Import(typeof(Byte))) || type .SafeEquivalent( type.Module.Import(typeof(SByte))))
                {
                    instructions.Emit(OpCodes.Stind_I1);
                    return;
                }
                if (type .SafeEquivalent( type.Module.Import(typeof(Int16))) || type .SafeEquivalent( type.Module.Import(typeof(UInt16))))
                {
                    instructions.Emit(OpCodes.Stind_I2);
                    return;
                }
                if (type .SafeEquivalent( type.Module.Import(typeof(Int32))) || type .SafeEquivalent( type.Module.Import(typeof(UInt32))))
                {
                    instructions.Emit(OpCodes.Stind_I4);
                    return;
                }
                if (type .SafeEquivalent( type.Module.Import(typeof(Int64))) || type .SafeEquivalent( type.Module.Import(typeof(UInt64))))
                {
                    instructions.Emit(OpCodes.Stind_I8);
                    return;
                }
                if (type .SafeEquivalent( type.Module.Import(typeof(Single))))
                {
                    instructions.Emit(OpCodes.Stind_R4);
                    return;
                }
                if (type .SafeEquivalent( type.Module.Import(typeof(Double))))
                {
                    instructions.Emit(OpCodes.Stind_R8);
                    return;
                }
                instructions.Emit(OpCodes.Stobj, type);
            }
            else
            {
                instructions.Emit(OpCodes.Stind_Ref);
            }
        }
    }
}