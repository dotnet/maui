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
			ModifyMapping(propertyMapper, key, previousMethod =>
			{
				void NewMethod(IElementHandler handler, IElement view)
				{
					if (view is TVirtualView virtualView)
					{
						method((TViewHandler)handler, virtualView, previousMethod);
					}
					else
					{
						previousMethod?.Invoke(handler, view);
					}
				}

				return NewMethod;
			});
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
			ModifyMapping(propertyMapper, key, previousMethod =>
			{
				void NewMethod(IElementHandler handler, IElement view)
				{
					if ((handler is null || handler is TViewHandler) && view is TVirtualView v)
						method((TViewHandler)handler!, v, previousMethod);
					else
						previousMethod?.Invoke(handler!, view);
				}

				return NewMethod;
			});
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

		internal static void ReplaceMappingForControls<TVirtualView, TViewHandler>(
			this IPropertyMapper<IElement, IElementHandler> propertyMapper,
			string key,
			Action<TViewHandler, TVirtualView> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			ModifyFrameworkMapping(propertyMapper, key, previousMethod =>
			{
				void NewMethod(IElementHandler handler, IElement view)
				{
					if ((handler is null || handler is TViewHandler) && view is TVirtualView virtualView)
						method((TViewHandler)handler!, virtualView);
					else
						previousMethod?.Invoke(handler!, view);
				}

				return NewMethod;
			});
		}

		internal static void ModifyMappingForControls<TVirtualView, TViewHandler>(
			this IPropertyMapper<TVirtualView, TViewHandler> propertyMapper,
			string key,
			Action<TViewHandler, TVirtualView, Action<IElementHandler, IElement>?> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			ModifyFrameworkMapping(propertyMapper, key, previousMethod =>
				(handler, view) => method((TViewHandler)handler, (TVirtualView)view, previousMethod));
		}

		internal static void ModifyMappingForControls<TVirtualView, TViewHandler>(
			this IPropertyMapper<IElement, IElementHandler> propertyMapper,
			string key,
			Action<TViewHandler, TVirtualView, Action<IElementHandler, IElement>?> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			ModifyFrameworkMapping(propertyMapper, key, previousMethod => (handler, view) =>
			{
				if ((handler is null || handler is TViewHandler) && view is TVirtualView virtualView)
					method((TViewHandler)handler!, virtualView, previousMethod);
				else
					previousMethod?.Invoke(handler!, view);
			});
		}

		internal static void AppendToMappingForControls<TVirtualView, TViewHandler>(
			this IPropertyMapper<TVirtualView, TViewHandler> propertyMapper,
			string key,
			Action<TViewHandler, TVirtualView> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			ModifyFrameworkMapping(propertyMapper, key, previousMethod => (handler, view) =>
			{
				previousMethod?.Invoke(handler, view);
				method((TViewHandler)handler, (TVirtualView)view);
			});
		}

		internal static void AppendToMappingForControls<TVirtualView, TViewHandler>(
			this IPropertyMapper<IElement, IElementHandler> propertyMapper,
			string key,
			Action<TViewHandler, TVirtualView> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			ModifyFrameworkMapping(propertyMapper, key, previousMethod => (handler, view) =>
			{
				previousMethod?.Invoke(handler, view);

				if ((handler is null || handler is TViewHandler) && view is TVirtualView virtualView)
					method((TViewHandler)handler!, virtualView);
			});
		}

		internal static void PrependToMappingForControls<TVirtualView, TViewHandler>(
			this IPropertyMapper<TVirtualView, TViewHandler> propertyMapper,
			string key,
			Action<TViewHandler, TVirtualView> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			ModifyFrameworkMapping(propertyMapper, key, previousMethod => (handler, view) =>
			{
				method((TViewHandler)handler, (TVirtualView)view);
				previousMethod?.Invoke(handler, view);
			});
		}

		internal static void PrependToMappingForControls<TVirtualView, TViewHandler>(
			this IPropertyMapper<IElement, IElementHandler> propertyMapper,
			string key,
			Action<TViewHandler, TVirtualView> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			ModifyFrameworkMapping(propertyMapper, key, previousMethod => (handler, view) =>
			{
				if ((handler is null || handler is TViewHandler) && view is TVirtualView virtualView)
					method((TViewHandler)handler!, virtualView);

				previousMethod?.Invoke(handler!, view);
			});
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

		static void ModifyMapping(
			IPropertyMapper propertyMapper,
			string key,
			Func<Action<IElementHandler, IElement>?, Action<IElementHandler, IElement>> customization)
		{
			if (propertyMapper is PropertyMapper concreteMapper)
			{
				concreteMapper.AddMappingCustomization(key, customization);
				return;
			}

			var mapping = customization(propertyMapper.GetProperty(key));
			((IPropertyMapper<IElement, IElementHandler>)propertyMapper).Add(key, mapping);
		}

		static void ModifyFrameworkMapping(
			IPropertyMapper propertyMapper,
			string key,
			Func<Action<IElementHandler, IElement>?, Action<IElementHandler, IElement>> modification)
		{
			if (propertyMapper is PropertyMapper concreteMapper)
			{
				concreteMapper.ModifyFrameworkMapping(key, modification);
				return;
			}

			var mapping = modification(propertyMapper.GetProperty(key));
			((IPropertyMapper<IElement, IElementHandler>)propertyMapper).Add(key, mapping);
		}
	}
}
