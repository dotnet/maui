//
// ViewExtensions.cs
//
// Author:
//       Stephane Delcroix <stephane@mi8.be>
//
// Copyright (c) 2013 Mobile Inception
// Copyright (c) 2013 Xamarin, Inc
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
using System.Reflection;

namespace Xamarin.Forms.Xaml
{
	public static class Extensions
	{
		public static TXaml LoadFromXaml<TXaml>(this TXaml view, Type callingType)
		{
			XamlLoader.Load(view, callingType);
			return view;
		}

		public static TXaml LoadFromXaml<TXaml>(this TXaml view, string xaml)
		{
			XamlLoader.Load(view, xaml);
			return view;
		}

		internal static TXaml LoadFromXaml<TXaml>(this TXaml view, string xaml, Assembly rootAssembly)
		{
			XamlLoader.Load(view, xaml, rootAssembly);
			return view;
		}
	}
}