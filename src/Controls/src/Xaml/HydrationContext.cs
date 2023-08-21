// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Maui.Controls.Xaml
{
	class HydrationContext
	{
		public HydrationContext()
		{
			Values = new Dictionary<INode, object>();
			Types = new Dictionary<IElementNode, Type>();
		}

		public Dictionary<INode, object> Values { get; }
		public Dictionary<IElementNode, Type> Types { get; }
		public HydrationContext ParentContext { get; set; }
		public Action<Exception> ExceptionHandler { get; set; }
		public object RootElement { get; set; }
		public Assembly RootAssembly { get; internal set; }
	}
}