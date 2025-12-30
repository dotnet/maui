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
			Types = new Dictionary<ElementNode, Type>();
		}

		public Dictionary<INode, object> Values { get; }
		public Dictionary<ElementNode, Type> Types { get; }
		public HydrationContext ParentContext { get; set; }
		public Action<Exception> ExceptionHandler { get; set; }
		public object RootElement { get; set; }
		public Assembly RootAssembly { get; internal set; }
	}
}