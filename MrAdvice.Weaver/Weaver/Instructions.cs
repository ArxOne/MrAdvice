#region Mr. Advice

// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php

#endregion

namespace ArxOne.MrAdvice.Weaver
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using dnlib.DotNet;
    using dnlib.DotNet.Emit;
    using Utility;

    /// <summary>
    /// Allows to Emit() to Instructions set and keep a cursor
    /// This way, this class allows to apped or insert IL instructions in method body
    /// </summary>
    public class Instructions
    {
        private readonly IList<Instruction> _instructions;

        public ModuleDef Module { get; }

        /// <summary>
        /// Gets or sets the cursor.
        /// </summary>
        /// <value>
        /// The cursor.
        /// </value>
        public int Cursor { get; set; }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count => _instructions.Count;

        public LocalList Variables { get; }

        /// <summary>Initializes a new instance of the <see cref="Instructions"/> class.</summary>
        /// <param name="body">The body.</param>
        /// <param name="module">The module definition.</param>
        public Instructions(CilBody body, ModuleDef module)
        {
            _instructions = body.Instructions;
            Variables = body.Variables;
            Module = module;
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

        public Instructions Emit(OpCode opCode, sbyte value)
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

        public Instructions Emit(OpCode opCode, TypeRef value)
        {
            return Insert(Instruction.Create(opCode, value));
        }

        public Instructions Emit(OpCode opCode, TypeSig value)
        {
            return Insert(Instruction.Create(opCode, value.ToTypeDefOrRef()));
        }

        public Instructions Emit(OpCode opCode, Type value)
        {
            return Insert(Instruction.Create(opCode, Module.SafeImport(value)));
        }

        public Instructions Emit(OpCode opCode, CorLibTypeSig value)
        {
            return Insert(Instruction.Create(opCode, value));
        }

        public Instructions Emit(OpCode opCode, IMethodDefOrRef value)
        {
            return Insert(Instruction.Create(opCode, value));
        }

        //public Instructions Emit(OpCode opCode, GenericParam value)
        //{
        //    var moduleDef = value.DeclaringMethod != null ? value.DeclaringMethod.Module : value.DeclaringType.Module;
        //    return Insert(Instruction.Create(opCode, new TypeRefUser(null, null, value.FullName, moduleDef)));
        //}

        public Instructions Emit(OpCode opCode, MethodBase value)
        {
            return Insert(Instruction.Create(opCode, Module.SafeImport(value)));
        }

        public Instructions Emit(OpCode opCode, IMethod value)
        {
            return Insert(Instruction.Create(opCode, value));
        }

        public Instructions Emit(OpCode opCode, IField value)
        {
            return Insert(Instruction.Create(opCode, value));
        }

        public Instructions Emit(OpCode opCode, ITokenOperand value)
        {
            return Insert(Instruction.Create(opCode, value));
        }

        public Instructions Emit(OpCode opCode, FieldInfo value)
        {
            return Insert(Instruction.Create(opCode, Module.SafeImport(value)));
        }

        public Instructions Emit(OpCode opCode, string value)
        {
            return Insert(Instruction.Create(opCode, value));
        }

        public Instructions Emit(OpCode opCode, Parameter value)
        {
            return Insert(Instruction.Create(opCode, value));
        }

        public Instructions Emit(OpCode opCode, Local value)
        {
            return Insert(Instruction.Create(opCode, value));
        }

        /// <summary>
        /// Emits a ldarg instruction
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public Instructions EmitLdarg(Parameter parameter)
        {
            switch (parameter.Index)
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
        public Instructions EmitLdloc(Local variableDefinition)
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
        public Instructions EmitStloc(Local variableDefinition)
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
                    if (value < 128)
                        return Emit(OpCodes.Ldc_I4_S, (sbyte) value);
                    return Emit(OpCodes.Ldc_I4, value);
            }
        }

        /// <summary>
        /// Emits unbox or cast when necessary.
        /// </summary>
        /// <param name="targetTypeSig">The target type sig.</param>
        /// <returns></returns>
        public Instructions EmitUnboxOrCastIfNecessary(TypeSig targetTypeSig)
        {
            // for generics and some unknown reason, an unbox_any is needed
            //if (targetTypeSig.IsGenericParameter)
            //    return Emit(OpCodes.Unbox_Any, _moduleDefinition.SafeImport(targetTypeSig));
            //if (targetTypeSig.IsValueType || targetTypeSig.IsPrimitive)
            //    return Emit(OpCodes.Unbox_Any, _moduleDefinition.SafeImport(targetTypeSig));
            //if (!targetTypeSig.SafeEquivalent(_moduleDefinition.CorLibTypes.Object))
            //    return Emit(OpCodes.Castclass, _moduleDefinition.SafeImport(targetTypeSig));
            //return this;
            if (MustBox(targetTypeSig))
                return Emit(OpCodes.Unbox_Any, Module.SafeImport(targetTypeSig));
            if (MustCast(targetTypeSig))
                return Emit(OpCodes.Castclass, Module.SafeImport(targetTypeSig));
            return this;
        }

        /// <summary>
        /// Emits box when necessary.
        /// </summary>
        /// <param name="targetTypeSig">The target type.</param>
        /// <returns></returns>
        public Instructions EmitBoxIfNecessary(TypeSig targetTypeSig)
        {
            if (MustBox(targetTypeSig))
                return Emit(OpCodes.Box, Module.SafeImport(targetTypeSig));
            return this;
        }

        private static bool MustBox(TypeSig targetTypeSig)
        {
            // for generics and some unknown reason, an unbox_any is needed
            if (targetTypeSig.IsGenericParameter)
            {
                // TODO: not sure at all!
                return true;
            }
            if (targetTypeSig.IsGenericInstanceType)
                return MustBox(((GenericInstSig) targetTypeSig).GenericType);
            //if (targetTypeSig.SafeEquivalent(Module.ImportAsTypeSig(typeof(Nullable<>))))
            //    return false;
            if (targetTypeSig.IsValueType || targetTypeSig.IsPrimitive)
                return true;
            return false;
        }

        private bool MustCast(TypeSig targetTypeSig)
        {
            if (targetTypeSig.SafeEquivalent(Module.CorLibTypes.Object))
                return false;
            return true;
        }

        /// <summary>
        /// Emits a ldind.
        /// </summary>
        /// <param name="typeSig">The type sig.</param>
        /// <returns></returns>
        public Instructions EmitLdind(TypeSig typeSig)
        {
            var corLibTypes = Module.CorLibTypes;
            if (typeSig == corLibTypes.Byte)
                return Emit(OpCodes.Ldind_U1);
            if (typeSig == corLibTypes.SByte)
                return Emit(OpCodes.Ldind_I1);
            if (typeSig == corLibTypes.Int16)
                return Emit(OpCodes.Ldind_I2);
            if (typeSig == corLibTypes.UInt16)
                return Emit(OpCodes.Ldind_U2);
            if (typeSig == corLibTypes.Int32)
                return Emit(OpCodes.Ldind_I4);
            if (typeSig == corLibTypes.UInt32)
                return Emit(OpCodes.Ldind_U4);
            if (typeSig == corLibTypes.Int64 || typeSig == corLibTypes.UInt64)
                return Emit(OpCodes.Ldind_I8);
            if (typeSig == corLibTypes.Single)
                return Emit(OpCodes.Ldind_R4);
            if (typeSig == corLibTypes.Double)
                return Emit(OpCodes.Ldind_R8);
            if (typeSig.IsPrimitive || typeSig.IsValueType)
                return Emit(OpCodes.Ldobj, Module.SafeImport(typeSig));
            return Emit(OpCodes.Ldind_Ref);
        }

        /// <summary>
        /// Emits a stind.
        /// </summary>
        /// <param name="typeSig">The type sig.</param>
        /// <returns></returns>
        public Instructions EmitStind(TypeSig typeSig)
        {
            var corLibTypes = Module.CorLibTypes;
            if (typeSig == corLibTypes.Byte || typeSig == corLibTypes.SByte)
                return Emit(OpCodes.Stind_I1);
            if (typeSig == corLibTypes.Int16 || typeSig == corLibTypes.UInt16)
                return Emit(OpCodes.Stind_I2);
            if (typeSig == corLibTypes.Int32 || typeSig == corLibTypes.UInt32)
                return Emit(OpCodes.Stind_I4);
            if (typeSig == corLibTypes.Int64 || typeSig == corLibTypes.UInt64)
                return Emit(OpCodes.Stind_I8);
            if (typeSig == corLibTypes.Single)
                return Emit(OpCodes.Stind_R4);
            if (typeSig == corLibTypes.Double)
                return Emit(OpCodes.Stind_R8);
            if (typeSig.IsPrimitive || typeSig.IsValueType)
                return Emit(OpCodes.Stobj, Module.SafeImport(typeSig));
            if (typeSig.IsGenericParameter)
                return Emit(OpCodes.Stobj, Module.SafeImport(typeSig));
            return Emit(OpCodes.Stind_Ref);
        }
    }
}