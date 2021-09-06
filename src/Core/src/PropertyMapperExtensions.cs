using System;

namespace Microsoft.Maui
{
	public static class PropertyMapperExtensions
	{
		public static void AppendToMapping<TVirtualView, TViewHandler>(this IPropertyMapper<TVirtualView, TViewHandler> propertyMapper,
			string key, Action<TViewHandler, TVirtualView> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			var previousMethod = propertyMapper.GetProperty(key);

			void newMethod(TViewHandler handler, TVirtualView view)
			{
				previousMethod?.Invoke(handler, view);
				method(handler, view);
			}

			propertyMapper.Add(key, newMethod);
		}

		public static void PrependToMapping<TVirtualView, TViewHandler>(this IPropertyMapper<TVirtualView, TViewHandler> propertyMapper,
			string key, Action<TViewHandler, TVirtualView> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			var previousMethod = propertyMapper.GetProperty(key);

			void newMethod(TViewHandler handler, TVirtualView view)
			{
				method(handler, view);
				previousMethod?.Invoke(handler, view);
			}

			propertyMapper.Add(key, newMethod);
		}
	}
}
