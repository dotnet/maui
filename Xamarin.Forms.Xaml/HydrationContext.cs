using System;
using System.Collections.Generic;

namespace Xamarin.Forms.Xaml
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
	}
}