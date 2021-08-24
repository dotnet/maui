using System;

namespace Microsoft.Maui
{
	public static class FactoryMapperExtensions
	{
		public static void AppendToMapping<TVirtualView, TViewHandler>(this FactoryMapper<TVirtualView, TViewHandler> propertyMapper,
			string key, Func<TViewHandler, TVirtualView, object?> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			var previousMethod = propertyMapper[key];

			object? newMethod(TViewHandler handler, TVirtualView view)
			{
				previousMethod?.Invoke(handler, view);
				return method(handler, view);
			}

			propertyMapper[key] = newMethod;
		}

		public static void PrependToMapping<TVirtualView, TViewHandler>(this FactoryMapper<TVirtualView, TViewHandler> propertyMapper,
			string key, Func<TViewHandler, TVirtualView, object?> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			var previousMethod = propertyMapper[key];

			object? newMethod(TViewHandler handler, TVirtualView view)
			{
				method(handler, view);
				return previousMethod?.Invoke(handler, view);
			}

			propertyMapper[key] = newMethod;
		}
	}
}
