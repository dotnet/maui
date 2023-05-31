using System;

namespace Microsoft.Maui
{
	public static class PropertyMapperExtensions
	{
		/// <summary>
		/// Modify a property mapping in place.
		/// </summary>
		/// <typeparam name="TVirtualView">The cross-platform type.</typeparam>
		/// <typeparam name="TViewHandler">The handler type.</typeparam>
		/// <param name="propertyMapper">The property mapper in which to change the mapping.</param>
		/// <param name="key">The name of the property.</param>
		/// <param name="method">The modified method to call when the property is updated.</param>
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

		/// <summary>
		/// Modify a property mapping in place but call the previous mapping if the types do not match.
		/// </summary>
		/// <typeparam name="TVirtualView">The cross-platform type.</typeparam>
		/// <typeparam name="TViewHandler">The handler type.</typeparam>
		/// <param name="propertyMapper">The property mapper in which to change the mapping.</param>
		/// <param name="key">The name of the property.</param>
		/// <param name="method">The modified method to call when the property is updated.</param>
		public static void ModifyMapping<TVirtualView, TViewHandler>(this IPropertyMapper<IElement, IElementHandler> propertyMapper,
			string key, Action<TViewHandler, TVirtualView, Action<IElementHandler, IElement>?> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			var previousMethod = propertyMapper.GetProperty(key);

			void newMethod(IElementHandler handler, IElement view)
			{
				if ((handler is null || handler is TViewHandler) && view is TVirtualView v)
					method((TViewHandler)handler!, v, previousMethod);
				else
					previousMethod?.Invoke(handler!, view);
			}

			propertyMapper.Add(key, newMethod);
		}

		/// <summary>
		/// Replace a property mapping in place but call the previous mapping if the types do not match.
		/// </summary>
		/// <typeparam name="TVirtualView">The cross-platform type.</typeparam>
		/// <typeparam name="TViewHandler">The handler type.</typeparam>
		/// <param name="propertyMapper">The property mapper in which to change the mapping.</param>
		/// <param name="key">The name of the property.</param>
		/// <param name="method">The modified method to call when the property is updated.</param>
		public static void ReplaceMapping<TVirtualView, TViewHandler>(this IPropertyMapper<IElement, IElementHandler> propertyMapper,
			string key, Action<TViewHandler, TVirtualView> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			propertyMapper.ModifyMapping<TVirtualView, TViewHandler>(key, (h, v, p) => method.Invoke(h, v));
		}

		/// <summary>
		/// Specify a method to be run after an existing property mapping.
		/// </summary>
		/// <typeparam name="TVirtualView">The cross-platform type.</typeparam>
		/// <typeparam name="TViewHandler">The handler type.</typeparam>
		/// <param name="propertyMapper">The property mapper in which to change the mapping.</param>
		/// <param name="key">The name of the property.</param>
		/// <param name="method">The method to call after the existing mapping is finished.</param>
		public static void AppendToMapping<TVirtualView, TViewHandler>(this IPropertyMapper<TVirtualView, TViewHandler> propertyMapper,
			string key, Action<TViewHandler, TVirtualView> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			propertyMapper.ModifyMapping(key, (handler, view, action) =>
			{
				action?.Invoke(handler, view);
				method(handler, view);
			});
		}

		/// <summary>
		/// Specify a method to be run after an existing property mapping but skip if the types do not match.
		/// </summary>
		/// <typeparam name="TVirtualView">The cross-platform type.</typeparam>
		/// <typeparam name="TViewHandler">The handler type.</typeparam>
		/// <param name="propertyMapper">The property mapper in which to change the mapping.</param>
		/// <param name="key">The name of the property.</param>
		/// <param name="method">The method to call after the existing mapping is finished.</param>
		public static void AppendToMapping<TVirtualView, TViewHandler>(this IPropertyMapper<IElement, IElementHandler> propertyMapper,
			string key, Action<TViewHandler, TVirtualView> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			propertyMapper.ModifyMapping(key, (handler, view, action) =>
			{
				action?.Invoke(handler, view);

				if ((handler is null || handler is TViewHandler) && view is TVirtualView v)
					method((TViewHandler)handler!, v);
			});
		}

		/// <summary>
		/// Specify a method to be run before an existing property mapping.
		/// </summary>
		/// <typeparam name="TVirtualView">The cross-platform type.</typeparam>
		/// <typeparam name="TViewHandler">The handler type.</typeparam>
		/// <param name="propertyMapper">The property mapper in which to change the mapping.</param>
		/// <param name="key">The name of the property.</param>
		/// <param name="method">The method to call before the existing mapping begins.</param>
		public static void PrependToMapping<TVirtualView, TViewHandler>(this IPropertyMapper<TVirtualView, TViewHandler> propertyMapper,
			string key, Action<TViewHandler, TVirtualView> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			propertyMapper.ModifyMapping(key, (handler, view, action) =>
			{
				method(handler, view);
				action?.Invoke(handler, view);
			});
		}

		/// <summary>
		/// Specify a method to be run before an existing property mapping but skip if the types do not match.
		/// </summary>
		/// <typeparam name="TVirtualView">The cross-platform type.</typeparam>
		/// <typeparam name="TViewHandler">The handler type.</typeparam>
		/// <param name="propertyMapper">The property mapper in which to change the mapping.</param>
		/// <param name="key">The name of the property.</param>
		/// <param name="method">The method to call before the existing mapping begins.</param>
		public static void PrependToMapping<TVirtualView, TViewHandler>(this IPropertyMapper<IElement, IElementHandler> propertyMapper,
			string key, Action<TViewHandler, TVirtualView> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			propertyMapper.ModifyMapping(key, (handler, view, action) =>
			{
				if ((handler is null || handler is TViewHandler) && view is TVirtualView v)
					method((TViewHandler)handler!, v);

				action?.Invoke(handler!, view);
			});
		}
	}
}
