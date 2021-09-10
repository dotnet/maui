using System;

namespace Microsoft.Maui
{
	public static class PropertyMapperExtensions
	{
		public static void ModifyMapping<TVirtualView, TViewHandler>(this IPropertyMapper<TVirtualView, TViewHandler> propertyMapper,
			string key, Action<TViewHandler, TVirtualView, Action<IElementHandler, IElement>?> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			var previousMethod = propertyMapper.GetProperty(key);

			void newMethod(TViewHandler handler, TVirtualView view)
			{
				method(handler, view, previousMethod);
			}

			propertyMapper.Add(key, newMethod);
		}

		public static void AppendToMapping<TVirtualView, TViewHandler>(this IPropertyMapper<TVirtualView, TViewHandler> propertyMapper,
			string key, Action<TViewHandler, TVirtualView> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			propertyMapper.ModifyMapping(key, (handler, view, action) => { 
				action?.Invoke(handler, view);
				method(handler, view);
			});
		}

		public static void PrependToMapping<TVirtualView, TViewHandler>(this IPropertyMapper<TVirtualView, TViewHandler> propertyMapper,
			string key, Action<TViewHandler, TVirtualView> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			propertyMapper.ModifyMapping(key, (handler, view, action) => {
				method(handler, view);
				action?.Invoke(handler, view);
			});
		}
	}
}
