﻿//
// LuryString.cs
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
using Lury.Runtime;

namespace Lury.Objects
{
    public class LuryString : LuryObject
    {
        private readonly string value;

        public string Value　{ get { return this.value; } }

        public LuryString(string value)
        {
            this.value = value;
        }

        public override LuryObject Con(LuryObject other)
        {
            if (other == null)
                throw new LuryException(LuryExceptionType.NilReference);

            return new LuryString(this.value + other.ToString());
        }

        public override LuryBoolean CEq(LuryObject other)
        {
            if (other == null)
                throw new LuryException(LuryExceptionType.NilReference);

            if (!(other is LuryString))
                throw new LuryException(LuryExceptionType.NotSupportedOperationBinary);

            return this.value == ((LuryString)other).value ? LuryBoolean.True : LuryBoolean.False;
        }

        public override string ToString()
        {
            return this.value;
        }
    }
}

