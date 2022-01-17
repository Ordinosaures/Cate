﻿using System;
using System.Collections.Generic;

namespace Inu.Cate.Tms99
{
    internal class SubroutineInstruction : Cate.SubroutineInstruction
    {
        public SubroutineInstruction(Function function, Function targetFunction, AssignableOperand? destinationOperand, List<Operand> sourceOperands) : base(function, targetFunction, destinationOperand, sourceOperands) { }

        protected override void Call()
        {
            WriteLine("\tbl\t@" + TargetFunction.Label);
        }

        protected override void StoreParameters()
        {
            StoreParametersDirect();
        }
    }
}
