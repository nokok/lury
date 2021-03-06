﻿//
// IR.cs
//
// Author:
//       Tomona Nanase <nanase@users.noreply.github.com>
//
// The MIT License (MIT)
//
// Copyright (c) 2015 Tomona Nanase
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using Lury.Objects;
using Lury.Runtime;

namespace Lury.Compiling
{
    class Routine
    {
        private readonly IEnumerable<Statement> statements;

        public Routine(IEnumerable<Statement> statements)
        {
            this.statements = statements;
        }

        public Routine()
        {
            this.statements = Enumerable.Empty<Statement>();
        }

        public StatementExit Evaluate(LuryContext context)
        {
            var ret = StatementExit.NormalExit;

            foreach (var statement in this.statements)
            {
                ret = statement.Evaluate(context);

                if (ret.ExitReason != StatementExitReason.NormalExit)
                    break;
            }

            return ret;
        }
    }

    class StatementExit
    {
        public static readonly StatementExit NormalExit = new StatementExit(null, StatementExitReason.NormalExit);

        public LuryObject ReturnValue { get; private set; }

        public StatementExitReason ExitReason { get; private set; }

        public StatementExit(LuryObject returnValue, StatementExitReason reason)
        {
            this.ReturnValue = returnValue;
            this.ExitReason = reason;
        }
    }

    enum StatementExitReason
    {
        NormalExit,
        Return,
        Break,
        Continue,
        Yield,
        ExceptionThrow
    }

    abstract class Statement
    {
        public virtual StatementExit Evaluate(LuryContext context)
        {
            return null;
        }
    }

    class FunctionDefinition : Statement
    {
        private readonly LValueNode name;
        private readonly List<string> parameters;
        private readonly Routine suite;

        public FunctionDefinition(LValueNode name, List<string> parameters, Routine suite)
        {
            this.name = name;
            this.parameters = parameters;
            this.suite = suite;
        }

        public FunctionDefinition(LValueNode name, Routine suite)
            : this(name, null, suite)
        {
        }

        public override StatementExit Evaluate(LuryContext context)
        {
            this.name.Assign(new LuryFunction(args =>
            {
                var newContext = new LuryContext(context);
                var paramCount = (this.parameters == null ? 0 : this.parameters.Count);

                if (args.Length != paramCount)
                    throw new LuryException(LuryExceptionType.NotEnoughFunctionArgumentNumber);

                for (int i = 0; i < args.Length; i++)
                    newContext.SetMemberNoRecursion(this.parameters[i], args[i]);

                return this.Invoke(newContext);
            }), context);

            return StatementExit.NormalExit;
        }

        private LuryObject Invoke(LuryContext context)
        {
            var exit = this.suite.Evaluate(context);

            if (exit.ExitReason == StatementExitReason.Return)
                return exit.ReturnValue;
            else if (exit.ExitReason == StatementExitReason.NormalExit)
                return null;
            else
                throw new LuryException(LuryExceptionType.WrongBreak);
        }
    }

    class IfStatement : Statement
    {
        private readonly Node condition;
        private readonly Routine suite;
        private readonly IfStatement nextIf;

        public IfStatement(Routine elseSuite)
        {
            this.condition = null;
            this.suite = elseSuite;
            this.nextIf = null;
        }

        public IfStatement(Node condition, Routine suite, IfStatement nextIf)
        {
            this.condition = condition;
            this.suite = suite;
            this.nextIf = nextIf;
        }

        public IfStatement(Node condition, Routine suite)
        {
            this.condition = condition;
            this.suite = suite;
            this.nextIf = null;
        }

        public override StatementExit Evaluate(LuryContext context)
        {
            if (this.condition == null)
            {
                // else block
                return this.suite.Evaluate(new LuryContext(context));
            }
            else
            {
                // if block
                var cond = this.condition.Evaluate(new LuryContext(context));

                if (cond == null)
                    throw new LuryException(LuryExceptionType.NilReference);

                if (!(cond is LuryBoolean))
                    throw new LuryException(LuryExceptionType.ConditionValueIsNotBoolean);

                if (cond == LuryBoolean.True)           // if suite
                        return this.suite.Evaluate(new LuryContext(context));
                else if (this.nextIf != null)           // elif block
                        return this.nextIf.Evaluate(new LuryContext(context));
            }

            return StatementExit.NormalExit;
        }
    }

    class PassStatement : Statement
    {
        public override StatementExit Evaluate(LuryContext context)
        {
            return StatementExit.NormalExit;
        }
    }

    class BreakStatement : Statement
    {
        public override StatementExit Evaluate(LuryContext context)
        {
            return new StatementExit(null, StatementExitReason.Break);
        }
    }

    class ContinueStatement : Statement
    {
        public override StatementExit Evaluate(LuryContext context)
        {
            return new StatementExit(null, StatementExitReason.Continue);
        }
    }

    class ReturnStatement : Statement
    {
        private readonly Node returnValue;

        public ReturnStatement(Node returnValue)
        {
            this.returnValue = returnValue;
        }

        public override StatementExit Evaluate(LuryContext context)
        {
            if (this.returnValue == null)
                return new StatementExit(null, StatementExitReason.Return);
            else
                return new StatementExit(this.returnValue.Evaluate(context), StatementExitReason.Return);
        }
    }

    class WhileStatement : Statement
    {
        private readonly Node condition;
        private readonly Routine suite;
        private readonly Routine elseSuite;

        public WhileStatement(Node condition, Routine suite, Routine elseSuite)
        {
            this.condition = condition;
            this.suite = suite;
            this.elseSuite = elseSuite;
        }

        public WhileStatement(Node condition, Routine suite)
        {
            this.condition = condition;
            this.suite = suite;
            this.elseSuite = null;
        }

        public override StatementExit Evaluate(LuryContext context)
        {
            StatementExit exit;
            var newContext = new LuryContext(context);

            while (true)
            {
                var cond = this.condition.Evaluate(newContext);
                
                if (cond == null)
                    throw new LuryException(LuryExceptionType.NilReference);

                if (!(cond is LuryBoolean))
                    throw new LuryException(LuryExceptionType.ConditionValueIsNotBoolean);

                if (cond == LuryBoolean.True)
                {
                    exit = this.suite.Evaluate(newContext);

                    if (exit.ExitReason == StatementExitReason.Break)
                        break;
                    else if (exit.ExitReason == StatementExitReason.Continue ||
                             exit.ExitReason == StatementExitReason.NormalExit)
                        continue;
                    else
                        return exit;
                }
                else
                {
                    if (this.elseSuite != null)
                        return this.elseSuite.Evaluate(new LuryContext(context));
                        
                    break;
                }
            }
            
            return StatementExit.NormalExit;
        }
    }

    class ExpressionStatement : Statement
    {
        private readonly Node expression;

        public ExpressionStatement(Node expression)
        {
            this.expression = expression;
        }

        public override StatementExit Evaluate(LuryContext context)
        {
            this.expression.Evaluate(context);
            return StatementExit.NormalExit;
        }
    }

    abstract class Node
    {
        public abstract LuryObject Evaluate(LuryContext context);
    }

    abstract class LValueNode : Node
    {
        public abstract void Assign(LuryObject value, LuryContext context);
    }

    class NormalLValueNode : LValueNode
    {
        private readonly string reference;

        public NormalLValueNode(object reference)
        {
            this.reference = ((Lexer.Token)reference).Text;
        }

        public NormalLValueNode(string reference)
        {
            this.reference = reference;
        }

        public override LuryObject Evaluate(LuryContext context)
        {
            return context[this.reference];
        }

        public override void Assign(LuryObject value, LuryContext context)
        {
            context[this.reference] = value;
        }
    }

    class ExpandLValueNode : LValueNode
    {
        private readonly LValueNode parent;
        private readonly string child;

        public ExpandLValueNode(LValueNode parent, string child)
        {
            this.parent = parent;
            this.child = child;
        }

        public ExpandLValueNode(LValueNode parent, object child)
            : this(parent, ((Lexer.Token)child).Text)
        {
        }

        public override LuryObject Evaluate(LuryContext context)
        {
            var parentObj = this.parent.Evaluate(context);

            if (parentObj == null)
                throw new LuryException(LuryExceptionType.NilReference);

            return parentObj[this.child];
        }

        public override void Assign(LuryObject value, LuryContext context)
        {
            var parentObj = this.parent.Evaluate(context);

            if (parentObj == null)
                throw new LuryException(LuryExceptionType.NilReference);

            if (parentObj.Has(this.child))
                parentObj[this.child] = value;
            else
                throw new LuryException(LuryExceptionType.AttributeIsNotFound);
        }
    }

    class CallingLValueNode : LValueNode
    {
        private readonly LValueNode lvalue;

        public CallingLValueNode(LValueNode lvalue)
        {
            this.lvalue = lvalue;
        }

        public override LuryObject Evaluate(LuryContext context)
        {
            var obj = this.lvalue.Evaluate(context);

            if (obj == null)
                throw new LuryException(LuryExceptionType.NilReference);

            if (obj is LuryFunction)
                return obj.Call();
            else
                return obj;
        }

        public override void Assign(LuryObject value, LuryContext context)
        {
            this.lvalue.Assign(value, context);
        }
    }

    class ConstantNode : Node
    {
        private readonly LuryObject constant;

        public ConstantNode(LuryObject constant)
        {
            this.constant = constant;
        }

        public override LuryObject Evaluate(LuryContext context)
        {
            return this.constant;
        }
    }

    class UnaryNode : Node
    {
        private readonly Node target;
        private readonly UnaryOperator operation;

        public UnaryNode(Node target, UnaryOperator operation)
        {
            this.target = target;
            this.operation = operation;
        }

        public override LuryObject Evaluate(LuryContext context)
        {
            if (this.operation == UnaryOperator.Reference)
            {
                if (!(this.target is LValueNode))
                    throw new LuryException(LuryExceptionType.WrongRefReference);
                
                return this.target.Evaluate(context);
            }

            var value = this.target.Evaluate(context);

            if (value == null)
                throw new LuryException(LuryExceptionType.NilReference);

            switch (this.operation)
            {
                case UnaryOperator.SignNegative:
                    return value.Neg();

                case UnaryOperator.SignPositive:
                    return value.Pos();

                case UnaryOperator.BitwiseNot:
                    return value.BNot();

                case UnaryOperator.LogicalNot:
                    return value.LNot();

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    class UnaryAssignNode : Node
    {
        private readonly Node target;
        private readonly UnaryAssignOperator operation;

        public UnaryAssignNode(Node target, UnaryAssignOperator operation)
        {
            this.target = target;
            this.operation = operation;
        }

        public override LuryObject Evaluate(LuryContext context)
        {
            if (!(this.target is LValueNode))
                throw new LuryException(LuryExceptionType.WrongLValue);

            var lvalue = (LValueNode)this.target;
            var dr_value = lvalue.Evaluate(context);

            switch (this.operation)
            {
                case UnaryAssignOperator.IncrementPostfix:
                    lvalue.Assign(dr_value.Inc(), context);
                    return dr_value;

                case UnaryAssignOperator.DecrementPostfix:
                    lvalue.Assign(dr_value.Dec(), context);
                    return dr_value;

                case UnaryAssignOperator.IncrementPrefix:
                    dr_value = dr_value.Inc();
                    lvalue.Assign(dr_value, context);
                    return dr_value;

                case UnaryAssignOperator.DecrementPrefix:
                    dr_value = dr_value.Dec();
                    lvalue.Assign(dr_value, context);
                    return dr_value;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    class BinaryNode : Node
    {
        private readonly Node x;
        private readonly Node y;
        private readonly BinaryOperator operation;

        public BinaryNode(Node x, Node y, BinaryOperator operation)
        {
            this.x = x;
            this.y = y;
            this.operation = operation;
        }

        public override LuryObject Evaluate(LuryContext context)
        {
            var x = this.x.Evaluate(context);
            var y = this.y.Evaluate(context);

            if (x == null)
                throw new LuryException(LuryExceptionType.NilReference);

            switch (this.operation)
            {
                case BinaryOperator.Power:
                    return x.Pow(y);

                case BinaryOperator.Multiplication:
                    return x.Mul(y);

                case BinaryOperator.Division:
                    return x.Div(y);

                case BinaryOperator.IntDivision:
                    return x.IDiv(y);

                case BinaryOperator.Modulo:
                    return x.Mod(y);

                case BinaryOperator.Addition:
                    return x.Add(y);

                case BinaryOperator.Subtraction:
                    return x.Sub(y);

                case BinaryOperator.Concatenation:
                    return x.Con(y);

                case BinaryOperator.LeftShift:
                    return x.Shl(y);

                case BinaryOperator.RightShift:
                    return x.Shl(y);

                case BinaryOperator.ArithmeticAnd:
                    return x.BAnd(y);

                case BinaryOperator.ArithmeticXor:
                    return x.BXor(y);

                case BinaryOperator.ArithmeticOr:
                    return x.BOr(y);

                case BinaryOperator.LogicalAnd:
                    return x.LAnd(y);

                case BinaryOperator.LogicalOr:
                    return x.LOr(y);

                case BinaryOperator.LessThan:
                    return x.CLt(y);

                case BinaryOperator.GreaterThan:
                    return x.CGt(y);

                case BinaryOperator.LessThanEqual:
                    return x.CELt(y);

                case BinaryOperator.GreaterThanEqual:
                    return x.CEGt(y);

                case BinaryOperator.Equal:
                    return x.CEq(y);

                case BinaryOperator.NotEqual:
                    return x.CNe(y);

                case BinaryOperator.Is:
                    return x.Is(y);

                case BinaryOperator.IsNot:
                    return x.IsNot(y);

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    class BinaryAssignNode : Node
    {
        private readonly Node lvalue;
        private readonly Node rvalue;
        private readonly BinaryAssignOperator operation;

        public BinaryAssignNode(Node lvalue, Node rvalue, BinaryAssignOperator operation)
        {
            this.lvalue = lvalue;
            this.rvalue = rvalue;
            this.operation = operation;
        }

        public override LuryObject Evaluate(LuryContext context)
        {
            if (!(this.lvalue is LValueNode))
                throw new LuryException(LuryExceptionType.WrongLValue);

            var dst = (LValueNode)lvalue;
            var value = this.rvalue.Evaluate(context);

            if (this.operation == BinaryAssignOperator.Assign)
            {
                dst.Assign(value, context);
                return value;
            }

            var dstValue = dst.Evaluate(context);

            switch (this.operation)
            {
                case BinaryAssignOperator.Power:
                    value = dstValue.Pow(value);
                    break;

                case BinaryAssignOperator.Multiplication:
                    value = dstValue.Mul(value);
                    break;

                case BinaryAssignOperator.Division:
                    value = dstValue.Div(value);
                    break;

                case BinaryAssignOperator.IntDivision:
                    value = dstValue.IDiv(value);
                    break;

                case BinaryAssignOperator.Modulo:
                    value = dstValue.Mod(value);
                    break;

                case BinaryAssignOperator.Addition:
                    value = dstValue.Add(value);
                    break;

                case BinaryAssignOperator.Subtraction:
                    value = dstValue.Sub(value);
                    break;

                case BinaryAssignOperator.Concatenation:
                    value = dstValue.Con(value);
                    break;

                case BinaryAssignOperator.LeftShift:
                    value = dstValue.Shl(value);
                    break;

                case BinaryAssignOperator.RightShift:
                    value = dstValue.Shr(value);
                    break;

                case BinaryAssignOperator.ArithmeticAnd:
                    value = dstValue.BAnd(value);
                    break;

                case BinaryAssignOperator.ArithmeticXor:
                    value = dstValue.BXor(value);
                    break;

                case BinaryAssignOperator.ArithmeticOr:
                    value = dstValue.BOr(value);
                    break;

                default:
                    throw new InvalidOperationException();
            }

            dst.Assign(value, context);
            return value;
        }
    }

    class TernaryNode : Node
    {
        private readonly Node x;
        private readonly Node y;
        private readonly Node z;
        private readonly TernaryOperator operation;

        public TernaryNode(Node x, Node y, Node z, TernaryOperator operation)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.operation = operation;
        }

        public override LuryObject Evaluate(LuryContext context)
        {
            var x = this.x.Evaluate(context);

            if (x == null)
                throw new LuryException(LuryExceptionType.NilReference);

            switch (this.operation)
            {
                case TernaryOperator.Condition:
                    if (!(x is LuryBoolean))
                        throw new LuryException(LuryExceptionType.ConditionValueIsNotBoolean);

                    if (((LuryBoolean)x).Value)
                        return this.y.Evaluate(context);
                    else
                        return this.z.Evaluate(context);

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    class CallNode : Node
    {
        private readonly Node function;
        private readonly IEnumerable<Node> param;

        public CallNode(Node function, IEnumerable<Node> param)
        {
            this.function = function;
            this.param = param;
        }

        public CallNode(Node function)
        {
            this.function = function;
            this.param = Enumerable.Empty<Node>();
        }

        public override LuryObject Evaluate(LuryContext context)
        {
            var objects = this.param.Select(p => p == null ? null : p.Evaluate(context)).ToArray();
            var func = this.function.Evaluate(context);

            if (func == null)
                throw new LuryException(LuryExceptionType.NilReference);

            return func.Call(objects);
        }
    }
}

