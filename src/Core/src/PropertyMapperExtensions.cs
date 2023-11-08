using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Layouts;

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

		internal static void ReplaceMapping<TVirtualView, TViewHandler>(this IPropertyMapper<TVirtualView, TViewHandler> propertyMapper,
			string key, Action<TViewHandler, TVirtualView> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			var previousMethod = propertyMapper.GetProperty(key);

			if (previousMethod is null)
			{
				propertyMapper.Add(key, method);
				return;
			}

			propertyMapper.ModifyMapping(key, (h, v, p) => method.Invoke(h, v));
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

	internal static class PropertyMapperFluentExtensions
	{
		/// <summary>
		/// Creates a new PropertyMapper based on an existing PropertyMapper, targeted at a new virtual view and handler type.
		/// </summary>
		internal static IPropertyMapper<TVirtualView, THandler> RemapFor<TVirtualView, THandler>(this IPropertyMapper<IElement, IElementHandler> innerMap)
			where TVirtualView : IElement
			where THandler : IElementHandler
		{
			return new PropertyMapper<TVirtualView, THandler>(innerMap);
		}

		/// <summary>
		/// Creates a property mapping. If a previous property mapping exists, it is replaced.
		/// </summary>
		internal static IPropertyMapper<TVirtualView, THandler> Map<TVirtualView, THandler>(this IPropertyMapper<TVirtualView, THandler> propertyMapper,
			string key, Action<THandler, TVirtualView> propertyUpdater)
			where TVirtualView : IElement
			where THandler : IElementHandler
		{
			propertyMapper.Add(key, propertyUpdater);
			return propertyMapper;
		}

		/// <summary>
		/// Modifies an existing mapping.
		/// </summary>
		internal static IPropertyMapper<TVirtualView, TViewHandler> Modify<TVirtualView, TViewHandler>(this IPropertyMapper<TVirtualView, TViewHandler> propertyMapper,
			string key, Action<TViewHandler, TVirtualView, Action<IElementHandler, IElement>?> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			propertyMapper.ModifyMapping(key, method);
			return propertyMapper;
		}

		/// <summary>
		/// Inserts a new mapping before an existing mapping.
		/// </summary>
		internal static IPropertyMapper<TVirtualView, TViewHandler> Prepend<TVirtualView, TViewHandler>(this IPropertyMapper<TVirtualView, TViewHandler> propertyMapper,
			string key, Action<TViewHandler, TVirtualView> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			propertyMapper.PrependToMapping(key, method);
			return propertyMapper;
		}

		/// <summary>
		/// Inserts a new mapping after an existing mapping.
		/// </summary>
		internal static IPropertyMapper<TVirtualView, TViewHandler> Append<TVirtualView, TViewHandler>(this IPropertyMapper<TVirtualView, TViewHandler> propertyMapper,
			string key, Action<TViewHandler, TVirtualView> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			propertyMapper.AppendToMapping(key, method);
			return propertyMapper;
		}

		/// <summary>
		/// Remaps a single property for the specified virtual view and handler types. If the virtual view or handler do not match the
		/// specified type, the original mapping is used.
		/// </summary>
		internal static IPropertyMapper<IElement, IElementHandler> RemapFor<TVirtualView, TViewHandler>(this IPropertyMapper<IElement, IElementHandler> propertyMapper,
			string key, Action<TViewHandler, TVirtualView> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			var previousMethod = propertyMapper.GetProperty(key);

			void newMethod(IElementHandler handler, IElement view)
			{
				if ((handler is null || handler is TViewHandler) && view is TVirtualView v)
					method((TViewHandler)handler!, v);
				else
					previousMethod?.Invoke(handler!, view);
			}

			propertyMapper.Add(key, newMethod);
			return propertyMapper;
		}
	}
}
