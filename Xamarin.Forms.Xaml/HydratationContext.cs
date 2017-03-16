using System;
using System.Collections.Generic;

namespace Xamarin.Forms.Xaml
{
	class HydratationContext
	{
		public HydratationContext()
		{
			Values = new Dictionary<INode, object>();
			Types = new Dictionary<IElementNode, Type>();
		}

		public Dictionary<INode, object> Values { get; private set; }

		public Dictionary<IElementNode, Type> Types { get; private set; }

		public HydratationContext ParentContext { get; set; }

		public Action<Exception> ExceptionHandler { get; set; }

		public object RootElement { get; set; }
	}
}