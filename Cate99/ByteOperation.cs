﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Inu.Cate.Tms99
{
    internal class ByteOperation : Cate.ByteOperation
    {
        private static ByteOperation? instance;
        public override List<Cate.ByteRegister> Registers => ByteRegister.Registers;
        public override List<Cate.ByteRegister> Accumulators => Registers;

        public ByteOperation()
        {
            instance = this;
        }

        protected override void OperateConstant(Instruction instruction, string operation, string value, int count)
        {
            throw new System.NotImplementedException();
        }

        protected override void OperateMemory(Instruction instruction, string operation, bool change, Variable variable, int offset, int count)
        {
            UsingAnyRegister(instruction, register =>
            {
                register.LoadFromMemory(instruction, variable, offset);
                for (var i = 0; i < count; ++i) {
                    instruction.WriteLine("\t" + operation + "\t" + register.Name);
                }
                register.StoreToMemory(instruction, variable, offset);
            });
            if (change) {
                instruction.RemoveVariableRegister(variable, offset);
            }
            instruction.ResultFlags |= Instruction.Flag.Z;
        }

        protected override void OperateIndirect(Instruction instruction, string operation, bool change, Cate.WordRegister pointerRegister, int offset,
            int count)
        {
            UsingAnyRegister(instruction, register =>
            {
                register.LoadIndirect(instruction, pointerRegister, offset);
                for (var i = 0; i < count; ++i) {
                    instruction.WriteLine("\t" + operation + "\t" + register.Name);
                }
                register.StoreIndirect(instruction, pointerRegister, offset);
            });
            instruction.ResultFlags |= Instruction.Flag.Z;
        }

        public override void StoreConstantIndirect(Instruction instruction, Cate.WordRegister pointerRegister, int offset, int value)
        {
            throw new System.NotImplementedException();
        }

        public override void ClearByte(Instruction instruction, string label)
        {
            throw new System.NotImplementedException();
        }

        public override string ToTemporaryByte(Instruction instruction, Cate.ByteRegister rightRegister)
        {
            throw new System.NotImplementedException();
        }

        public static void Operate(Instruction instruction, string operation, AssignableOperand destinationOperand, Operand leftOperand, Operand rightOperand)
        {
            if (destinationOperand.SameStorage(leftOperand)) {
                if (Tms99.Compiler.Operate(instruction, operation, rightOperand, destinationOperand)) return;
            }
            Debug.Assert(instance != null);
            instance.UsingAnyRegisterToChange(instruction, destinationOperand, leftOperand, register =>
            {
                register.Load(instruction, leftOperand);
                var right = Tms99.Compiler.OperandToString(instruction, rightOperand);
                if (right != null) {
                    instruction.WriteLine("\t" + operation + "\t" + right + "," + register.Name);
                    register.Store(instruction, destinationOperand);
                }
                else {
                    instance.UsingAnyRegisterToChange(instruction, destinationOperand, leftOperand, destinationRegister =>
                    {
                        instance.UsingAnyRegister(instruction, rightRegister =>
                        {
                            rightRegister.Load(instruction, rightOperand);
                            destinationRegister.Load(instruction, leftOperand);
                            instruction.WriteLine("\t" + operation + "\t" + rightRegister.Name + "," + destinationRegister.Name);
                            destinationRegister.Store(instruction, destinationOperand);
                        });
                    });
                }
            });
        }

        public static void OperateConstant(Instruction instruction, string operation, AssignableOperand destinationOperand, Operand leftOperand, int value)
        {
            Debug.Assert(instance != null);
            instance.UsingAnyRegisterToChange(instruction, destinationOperand, leftOperand, register =>
            {
                register.Load(instruction, leftOperand);
                instruction.WriteLine("\t" + operation + "\t" + register.Name + "," + value);
                register.Store(instruction, destinationOperand);
            });
        }

        public static void Operate(Instruction instruction, string operation, Operand leftOperand, Operand rightOperand)
        {
            void OperateRegister(Cate.ByteRegister register)
            {
                register.Load(instruction, leftOperand);
                instruction.WriteLine("\t" + operation + "\t" + register.Name + "," +
                                      Tms99.Compiler.OperandToString(instruction, rightOperand));
            }

            Debug.Assert(instance != null);
            if (leftOperand.Register is ByteRegister leftRegister) {
                OperateRegister(leftRegister);
                return;
            }
            instance.UsingAnyRegister(instruction, OperateRegister);
        }

        public static void OperateConstant(Instruction instruction, string operation, Operand leftOperand, int value)
        {
            void OperateRegister(Cate.ByteRegister register)
            {
                register.Load(instruction, leftOperand);
                instruction.WriteLine("\t" + operation + "\t" + register.Name + "," + value);
            }

            Debug.Assert(instance != null);
            if (leftOperand.Register is ByteRegister leftRegister) {
                OperateRegister(leftRegister);
                return;
            }
            instance.UsingAnyRegister(instruction, OperateRegister);
        }
    }
}
