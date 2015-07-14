#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Weaver
{
    using System;
    using System.Reflection;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Collections.Generic;
    using Utility;

    /// <summary>
    /// Allows to Emit() to Instructions set and keep a cursor
    /// This way, this class allows to apped or insert IL instructions in method body
    /// </summary>
    public class Instructions
    {
        private readonly Collection<Instruction> _instructions;
        private readonly ModuleDefinition _moduleDefinition;

        /// <summary>
        /// Gets or sets the cursor.
        /// </summary>
        /// <value>
        /// The cursor.
        /// </value>
        public int Cursor { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Instructions"/> class.
        /// </summary>
        /// <param name="instructions">The instructions.</param>
        /// <param name="moduleDefinition">The module definition.</param>
        public Instructions(Collection<Instruction> instructions, ModuleDefinition moduleDefinition)
        {
            _instructions = instructions;
            _moduleDefinition = moduleDefinition;
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

        public Instructions Emit(OpCode opCode, byte value)
        {
            return Insert(Instruction.Create(opCode, value));
        }

        public Instructions Emit(OpCode opCode, ushort value)
        {
            return Insert(Instruction.Create(opCode, value));
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

        public Instructions Emit(OpCode opCode, Type value)
        {
            return Insert(Instruction.Create(opCode, _moduleDefinition.SafeImport(value)));
        }

        public Instructions Emit(OpCode opCode, MethodReference value)
        {
            return Insert(Instruction.Create(opCode, value));
        }

        public Instructions Emit(OpCode opCode, MethodBase value)
        {
            return Insert(Instruction.Create(opCode, _moduleDefinition.SafeImport(value)));
        }

        public Instructions Emit(OpCode opCode, FieldReference value)
        {
            return Insert(Instruction.Create(opCode, value));
        }

        public Instructions Emit(OpCode opCode, FieldInfo value)
        {
            return Insert(Instruction.Create(opCode, _moduleDefinition.SafeImport(value)));
        }

        public Instructions Emit(OpCode opCode, string value)
        {
            return Insert(Instruction.Create(opCode, value));
        }

        public Instructions Emit(OpCode opCode, ParameterDefinition value)
        {
            return Insert(Instruction.Create(opCode, value));
        }

        public Instructions Emit(OpCode opCode, VariableDefinition value)
        {
            return Insert(Instruction.Create(opCode, value));
        }

        /// <summary>
        /// Emits a ldarg instruction
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public Instructions EmitLdarg(ParameterDefinition parameter)
        {
            var index = parameter.Index;
            // when the method is non static, the parameter index must have +1 (because 0 is "this" and first parameter is indexed 0 anyway)
            if (parameter.Method.HasThis)
                index++;
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
                    return Emit(OpCodes.Ldarg_S, parameter);
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
                    return Emit(OpCodes.Ldloc_S, variableDefinition);
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
                    return Emit(OpCodes.Stloc_S, variableDefinition);
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
                    // I couldn't find the right type, and Cecil documentation... Huh what documentation?
                    //if (value < 128)
                    //    return Emit(OpCodes.Ldc_I4_S, (byte)value);
                    return Emit(OpCodes.Ldc_I4, value);
            }
        }

        /// <summary>
        /// Emits unbox or cast when necessary.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        public Instructions EmitUnboxOrCastIfNecessary(TypeReference targetType)
        {
            // for generics and some unknown reason, an unbox_any is needed
            if (targetType.IsGenericParameter)
                return Emit(OpCodes.Unbox_Any, targetType);
            if (targetType.IsValueType)
                return Emit(OpCodes.Unbox_Any, targetType);
            if (!targetType.SafeEquivalent(targetType.Module.SafeImport(typeof(object))))
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
                if (type.SafeEquivalent(type.Module.SafeImport(typeof(Byte))))
                    return Emit(OpCodes.Ldind_U1);
                if (type.SafeEquivalent(type.Module.SafeImport(typeof(SByte))))
                    return Emit(OpCodes.Ldind_I1);
                if (type.SafeEquivalent(type.Module.SafeImport(typeof(Int16))))
                    return Emit(OpCodes.Ldind_I2);
                if (type.SafeEquivalent(type.Module.SafeImport(typeof(UInt16))))
                    return Emit(OpCodes.Ldind_U2);
                if (type.SafeEquivalent(type.Module.SafeImport(typeof(Int32))))
                    return Emit(OpCodes.Ldind_I4);
                if (type.SafeEquivalent(type.Module.SafeImport(typeof(UInt32))))
                    return Emit(OpCodes.Ldind_U4);
                if (type.SafeEquivalent(type.Module.SafeImport(typeof(Int64))) || type.SafeEquivalent(type.Module.SafeImport(typeof(UInt64))))
                    return Emit(OpCodes.Ldind_I8);
                if (type.SafeEquivalent(type.Module.SafeImport(typeof(Single))))
                    return Emit(OpCodes.Ldind_R4);
                if (type.SafeEquivalent(type.Module.SafeImport(typeof(Double))))
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
                if (type.SafeEquivalent(type.Module.SafeImport(typeof(Byte))) || type.SafeEquivalent(type.Module.SafeImport(typeof(SByte))))
                    return Emit(OpCodes.Stind_I1);
                if (type.SafeEquivalent(type.Module.SafeImport(typeof(Int16))) || type.SafeEquivalent(type.Module.SafeImport(typeof(UInt16))))
                    return Emit(OpCodes.Stind_I2);
                if (type.SafeEquivalent(type.Module.SafeImport(typeof(Int32))) || type.SafeEquivalent(type.Module.SafeImport(typeof(UInt32))))
                    return Emit(OpCodes.Stind_I4);
                if (type.SafeEquivalent(type.Module.SafeImport(typeof(Int64))) || type.SafeEquivalent(type.Module.SafeImport(typeof(UInt64))))
                    return Emit(OpCodes.Stind_I8);
                if (type.SafeEquivalent(type.Module.SafeImport(typeof(Single))))
                    return Emit(OpCodes.Stind_R4);
                if (type.SafeEquivalent(type.Module.SafeImport(typeof(Double))))
                    return Emit(OpCodes.Stind_R8);
                return Emit(OpCodes.Stobj, type);
            }
            return Emit(OpCodes.Stind_Ref);
        }
    }
}