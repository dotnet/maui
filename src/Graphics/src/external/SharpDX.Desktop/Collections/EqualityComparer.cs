// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
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

namespace SharpDX.Desktop.Collections
{
    /// <summary>
    /// Provides <see cref="IEqualityComparer{T}"/> for default value types.
    /// </summary>
    internal static class EqualityComparer
    {
        /// <summary>
        /// A default <see cref="IEqualityComparer{T}"/> for <see cref="System.IntPtr"/>.
        /// </summary>
        public static readonly IEqualityComparer<IntPtr> DefaultIntPtr = new IntPtrComparer();

        internal class IntPtrComparer : EqualityComparer<IntPtr>
        {
            public override bool Equals(IntPtr x, IntPtr y)
            {
                return x == y;
            }

            public override int GetHashCode(IntPtr obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
