using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Platform
{
	public class ActionMapper<TVirtualView>
			where TVirtualView : IFrameworkElement
	{
		public ActionMapper(PropertyMapper<TVirtualView> propertyMapper)
		{
			PropertyMapper = propertyMapper;
		}

		public PropertyMapper<TVirtualView> PropertyMapper { get; }

		public Action<IViewHandler, TVirtualView> this[string key]
		{
			set => PropertyMapper._mapper[key] = ((r, v) => value?.Invoke(r, (TVirtualView)v), false);
		}
	}
}
