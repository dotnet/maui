using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Platform
{
	public class ActionMapper<TView>
			where TView : IFrameworkElement
	{
		public ActionMapper(PropertyMapper<TView> propertyMapper)
		{
			PropertyMapper = propertyMapper;
		}

		public PropertyMapper<TView> PropertyMapper { get; }

		public Action<IViewHandler, TView> this[string key]
		{
			set => PropertyMapper._mapper[key] = ((r, v) => value?.Invoke(r, (TView)v), false);
		}
	}
}
