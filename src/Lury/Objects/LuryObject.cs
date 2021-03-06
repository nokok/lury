﻿//
// LuryObject.cs
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
using Lury.Runtime;

namespace Lury.Objects
{
    public class LuryObject
    {
        protected readonly Dictionary<string, LuryObject> members = new Dictionary<string, LuryObject>(0);

        public virtual LuryObject this [string member]
        {
            get
            {
                if (this.members.ContainsKey(member))
                    return this.members[member];
                else
                    throw new LuryException(LuryExceptionType.NameIsNotFound);
            }
            set
            {
                if (this.members.ContainsKey(member))
                    this.members[member] = value;
                else
                    this.members.Add(member, value);
            }
        }

        public bool Has(string member)
        {
            return this.members.ContainsKey(member);
        }

        public virtual LuryObject Inc()
        {
            throw new LuryException(LuryExceptionType.NotSupportedOperationUnary);
        }

        public virtual LuryObject Dec()
        {
            throw new LuryException(LuryExceptionType.NotSupportedOperationUnary);
        }

        public virtual LuryObject Call(params LuryObject[] param)
        {
            throw new LuryException(LuryExceptionType.UnableToCall);
        }

        public virtual LuryObject Pow(LuryObject other)
        {
            throw new NotSupportedException();
        }

        public virtual LuryObject Pos()
        {
            throw new LuryException(LuryExceptionType.NotSupportedOperationUnary);
        }

        public virtual LuryObject Neg()
        {
            throw new LuryException(LuryExceptionType.NotSupportedOperationUnary);
        }

        public virtual LuryObject BNot()
        {
            throw new LuryException(LuryExceptionType.NotSupportedOperationUnary);
        }

        public virtual LuryObject Mul(LuryObject other)
        {
            throw new LuryException(LuryExceptionType.NotSupportedOperationBinary);
        }

        public virtual LuryObject Div(LuryObject other)
        {
            throw new LuryException(LuryExceptionType.NotSupportedOperationBinary);
        }

        public virtual LuryObject IDiv(LuryObject other)
        {
            throw new LuryException(LuryExceptionType.NotSupportedOperationBinary);
        }

        public virtual LuryObject Mod(LuryObject other)
        {
            throw new LuryException(LuryExceptionType.NotSupportedOperationBinary);
        }

        public virtual LuryObject Add(LuryObject other)
        {
            throw new LuryException(LuryExceptionType.NotSupportedOperationBinary);
        }

        public virtual LuryObject Sub(LuryObject other)
        {
            throw new LuryException(LuryExceptionType.NotSupportedOperationBinary);
        }

        public virtual LuryObject Con(LuryObject other)
        {
            throw new LuryException(LuryExceptionType.NotSupportedOperationBinary);
        }

        public virtual LuryObject Shl(LuryObject other)
        {
            throw new LuryException(LuryExceptionType.NotSupportedOperationBinary);
        }

        public virtual LuryObject Shr(LuryObject other)
        {
            throw new LuryException(LuryExceptionType.NotSupportedOperationBinary);
        }

        public virtual LuryObject BAnd(LuryObject other)
        {
            throw new LuryException(LuryExceptionType.NotSupportedOperationBinary);
        }

        public virtual LuryObject BXor(LuryObject other)
        {
            throw new LuryException(LuryExceptionType.NotSupportedOperationBinary);
        }

        public virtual LuryObject BOr(LuryObject other)
        {
            throw new LuryException(LuryExceptionType.NotSupportedOperationBinary);
        }

        public virtual LuryBoolean LNot()
        {
            throw new LuryException(LuryExceptionType.NotSupportedOperationUnary);
        }

        public virtual LuryBoolean LAnd(LuryObject other)
        {
            throw new LuryException(LuryExceptionType.NotSupportedOperationBinary);
        }

        public virtual LuryBoolean LOr(LuryObject other)
        {
            throw new LuryException(LuryExceptionType.NotSupportedOperationBinary);
        }

        public virtual LuryBoolean CLt(LuryObject other)
        {
            throw new LuryException(LuryExceptionType.NotSupportedOperationBinary);
        }

        public virtual LuryBoolean CGt(LuryObject other)
        {
            throw new LuryException(LuryExceptionType.NotSupportedOperationBinary);
        }

        public virtual LuryBoolean CELt(LuryObject other)
        {
            return this.CLt(other).LOr(this.CEq(other));
        }

        public virtual LuryBoolean CEGt(LuryObject other)
        {
            return this.CGt(other).LOr(this.CEq(other));
        }

        public virtual LuryBoolean CEq(LuryObject other)
        {
            throw new LuryException(LuryExceptionType.NotSupportedOperationBinary);
        }

        public virtual LuryBoolean CNe(LuryObject other)
        {
            return this.CEq(other).LNot();
        }

        public LuryBoolean Is(LuryObject other)
        {
            throw new NotImplementedException();
        }

        public LuryBoolean IsNot(LuryObject other)
        {
            return this.Is(other).LNot();
        }
    }
}

