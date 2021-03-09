using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui
{
	public class ActionMapper<TVirtualView, TViewHandler>
			where TVirtualView : IFrameworkElement
			where TViewHandler : IViewHandler
	{
		public ActionMapper(PropertyMapper<TVirtualView, TViewHandler> propertyMapper)
		{
			PropertyMapper = propertyMapper;
		}

		public PropertyMapper<TVirtualView, TViewHandler> PropertyMapper { get; }

		public Action<TViewHandler, TVirtualView> this[string key]
		{
			set => PropertyMapper._mapper[key] = ((r, v) => value?.Invoke((TViewHandler)r, (TVirtualView)v), false);
		}
	}
}
