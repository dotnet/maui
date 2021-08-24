using System;

namespace Microsoft.Maui
{
	public static class FactoryMapperExtensions
	{
		// This is used to append to the mapping after the factory method has been executed
		// then you can do stuff to it or return something else
		public static void AppendToCreatedMapping<TVirtualView, TViewHandler>(this FactoryMapper<TVirtualView, TViewHandler> propertyMapper,
			string key, Func<TViewHandler, TVirtualView, object?, object?> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			var previousMethod = propertyMapper[key];

			object? newMethod(TViewHandler handler, TVirtualView view)
			{
				var result = previousMethod?.Invoke(handler, view);
				return method(handler, view, result);
			}

			propertyMapper[key] = newMethod;
		}
	}
}
