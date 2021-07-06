using System;

namespace Microsoft.Maui
{
	public class ActionMapper<TVirtualView, TViewHandler>
		where TVirtualView : IElement
		where TViewHandler : IElementHandler
	{
		public ActionMapper(PropertyMapper<TVirtualView, TViewHandler> propertyMapper)
		{
			PropertyMapper = propertyMapper;
		}

		public PropertyMapper<TVirtualView, TViewHandler> PropertyMapper { get; }

		public Action<TViewHandler, TVirtualView> this[string key]
		{
			get => PropertyMapper[key];
			set => PropertyMapper.Add(key, value, false);
		}
	}
}