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
#pragma warning disable RS0016 // Add public types and members to the declared API
		public static IPropertyMapper<TVirtualView, TViewHandler> ModifyMapping<TVirtualView, TViewHandler>(this IPropertyMapper<TVirtualView, TViewHandler> propertyMapper,
			string key, Action<TViewHandler, TVirtualView, Action<IElementHandler, IElement>?> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			var previousMethod = propertyMapper.GetProperty(key);

			void newMethod(TViewHandler handler, TVirtualView view)
			{
				method(handler, view, previousMethod);
			}

			propertyMapper.Add(key, newMethod);
			return propertyMapper;
		}

		/// <summary>
		/// Modify a property mapping in place but call the previous mapping if the types do not match.
		/// </summary>
		/// <typeparam name="TVirtualView">The cross-platform type.</typeparam>
		/// <typeparam name="TViewHandler">The handler type.</typeparam>
		/// <param name="propertyMapper">The property mapper in which to change the mapping.</param>
		/// <param name="key">The name of the property.</param>
		/// <param name="method">The modified method to call when the property is updated.</param>
		public static IPropertyMapper<IElement, IElementHandler> ModifyMapping<TVirtualView, TViewHandler>(this IPropertyMapper<IElement, IElementHandler> propertyMapper,
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
			return propertyMapper;
		}

		/// <summary>
		/// Replace a property mapping in place but call the previous mapping if the types do not match.
		/// </summary>
		/// <typeparam name="TVirtualView">The cross-platform type.</typeparam>
		/// <typeparam name="TViewHandler">The handler type.</typeparam>
		/// <param name="propertyMapper">The property mapper in which to change the mapping.</param>
		/// <param name="key">The name of the property.</param>
		/// <param name="method">The modified method to call when the property is updated.</param>
		public static IPropertyMapper<IElement, IElementHandler> ReplaceMapping<TVirtualView, TViewHandler>(this IPropertyMapper<IElement, IElementHandler> propertyMapper,
			string key, Action<TViewHandler, TVirtualView> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			return propertyMapper.ModifyMapping<TVirtualView, TViewHandler>(key, (h, v, p) => method.Invoke(h, v));
		}

		internal static IPropertyMapper<TVirtualView, TViewHandler> ReplaceMapping<TVirtualView, TViewHandler>(this IPropertyMapper<TVirtualView, TViewHandler> propertyMapper,
			string key, Action<TViewHandler, TVirtualView> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			var previousMethod = propertyMapper.GetProperty(key);

			if (previousMethod is null)
			{
				propertyMapper.Add(key, method);
				return propertyMapper;
			}

			return propertyMapper.ModifyMapping(key, (h, v, p) => method.Invoke(h, v));
		}

		/// <summary>
		/// Specify a method to be run after an existing property mapping.
		/// </summary>
		/// <typeparam name="TVirtualView">The cross-platform type.</typeparam>
		/// <typeparam name="TViewHandler">The handler type.</typeparam>
		/// <param name="propertyMapper">The property mapper in which to change the mapping.</param>
		/// <param name="key">The name of the property.</param>
		/// <param name="method">The method to call after the existing mapping is finished.</param>
		public static IPropertyMapper<TVirtualView, TViewHandler> AppendToMapping<TVirtualView, TViewHandler>(this IPropertyMapper<TVirtualView, TViewHandler> propertyMapper,
			string key, Action<TViewHandler, TVirtualView> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			return propertyMapper.ModifyMapping(key, (handler, view, action) =>
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
		public static IPropertyMapper<IElement, IElementHandler> AppendToMapping<TVirtualView, TViewHandler>(this IPropertyMapper<IElement, IElementHandler> propertyMapper,
			string key, Action<TViewHandler, TVirtualView> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			return propertyMapper.ModifyMapping(key, (handler, view, action) =>
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
		public static IPropertyMapper<TVirtualView, TViewHandler> PrependToMapping<TVirtualView, TViewHandler>(this IPropertyMapper<TVirtualView, TViewHandler> propertyMapper,
			string key, Action<TViewHandler, TVirtualView> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			return propertyMapper.ModifyMapping(key, (handler, view, action) =>
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
		public static IPropertyMapper<IElement, IElementHandler> PrependToMapping<TVirtualView, TViewHandler>(this IPropertyMapper<IElement, IElementHandler> propertyMapper,
			string key, Action<TViewHandler, TVirtualView> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			return propertyMapper.ModifyMapping(key, (handler, view, action) =>
			{
				if ((handler is null || handler is TViewHandler) && view is TVirtualView v)
					method((TViewHandler)handler!, v);

				action?.Invoke(handler!, view);
			});
		}
#pragma warning restore RS0016 // Add public types and members to the declared API
	}
}
