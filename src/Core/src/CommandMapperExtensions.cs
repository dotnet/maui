using System;

namespace Microsoft.Maui
{
	public static class CommandMapperExtensions
	{
		/// <summary>
		/// Modify a command mapping in place.
		/// </summary>
		/// <typeparam name="TVirtualView">The cross-platform type.</typeparam>
		/// <typeparam name="TViewHandler">The handler type.</typeparam>
		/// <param name="commandMapper">The command mapper in which to change the mapping.</param>
		/// <param name="key">The name of the command.</param>
		/// <param name="method">The modified method to call when the command is updated.</param>
		public static void ModifyMapping<TVirtualView, TViewHandler>(this CommandMapper<TVirtualView, TViewHandler> commandMapper,
			string key, Action<TViewHandler, TVirtualView, object?, Action<IElementHandler, IElement, object?>?> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
			=> ((ICommandMapper<TVirtualView, TViewHandler>)commandMapper).ModifyMapping(key, method);

		/// <summary>
		/// Modify a command mapping in place.
		/// </summary>
		/// <typeparam name="TVirtualView">The cross-platform type.</typeparam>
		/// <typeparam name="TViewHandler">The handler type.</typeparam>
		/// <param name="commandMapper">The command mapper in which to change the mapping.</param>
		/// <param name="key">The name of the command.</param>
		/// <param name="method">The modified method to call when the command is updated.</param>
		public static void ModifyMapping<TVirtualView, TViewHandler>(this ICommandMapper<TVirtualView, TViewHandler> commandMapper,
			string key, Action<TViewHandler, TVirtualView, object?, Action<IElementHandler, IElement, object?>?> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			var previousMethod = commandMapper.GetCommand(key);

			commandMapper.Add(key, newMethod);

			void newMethod(TViewHandler handler, TVirtualView view, object? args)
			{
				method(handler, view, args, previousMethod);
			}
		}

		/// <summary>
		/// Modify a command mapping in place but call the previous mapping if the types do not match.
		/// </summary>
		/// <typeparam name="TVirtualView">The cross-platform type.</typeparam>
		/// <typeparam name="TViewHandler">The handler type.</typeparam>
		/// <param name="commandMapper">The command mapper in which to change the mapping.</param>
		/// <param name="key">The name of the command.</param>
		/// <param name="method">The modified method to call when the command is updated.</param>
		public static void ModifyMapping<TVirtualView, TViewHandler>(this ICommandMapper<IElement, IElementHandler> commandMapper,
			string key, Action<TViewHandler, TVirtualView, object?, Action<IElementHandler, IElement, object?>?> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			var previousMethod = commandMapper.GetCommand(key);

			void newMethod(IElementHandler handler, IElement view, object? args)
			{
				if ((handler is null || handler is TViewHandler) && view is TVirtualView v)
					method((TViewHandler)handler!, v, args, previousMethod);
				else
					previousMethod?.Invoke(handler!, view, args);
			}

			commandMapper.Add(key, newMethod);
		}

		/// <summary>
		/// Replace a command mapping in place but call the previous mapping if the types do not match.
		/// </summary>
		/// <typeparam name="TVirtualView">The cross-platform type.</typeparam>
		/// <typeparam name="TViewHandler">The handler type.</typeparam>
		/// <param name="commandMapper">The command mapper in which to change the mapping.</param>
		/// <param name="key">The name of the command.</param>
		/// <param name="method">The modified method to call when the command is updated.</param>
		public static void ReplaceMapping<TVirtualView, TViewHandler>(this ICommandMapper<IElement, IElementHandler> commandMapper,
			string key, Action<TViewHandler, TVirtualView, object?> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			commandMapper.ModifyMapping<TVirtualView, TViewHandler>(key, (h, v, a, p) => method.Invoke(h, v, a));
		}

		/// <summary>
		/// Specify a method to be run after an existing command mapping.
		/// </summary>
		/// <typeparam name="TVirtualView">The cross-platform type.</typeparam>
		/// <typeparam name="TViewHandler">The handler type.</typeparam>
		/// <param name="commandMapper">The command mapper in which to change the mapping.</param>
		/// <param name="key">The name of the command.</param>
		/// <param name="method">The method to call after the existing mapping is finished.</param>
		public static void AppendToMapping<TVirtualView, TViewHandler>(this CommandMapper<TVirtualView, TViewHandler> commandMapper,
			string key, Action<TViewHandler, TVirtualView, object?> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
			=> ((ICommandMapper<TVirtualView, TViewHandler>)commandMapper).AppendToMapping(key, method);

		/// <summary>
		/// Specify a method to be run after an existing command mapping.
		/// </summary>
		/// <typeparam name="TVirtualView">The cross-platform type.</typeparam>
		/// <typeparam name="TViewHandler">The handler type.</typeparam>
		/// <param name="commandMapper">The command mapper in which to change the mapping.</param>
		/// <param name="key">The name of the command.</param>
		/// <param name="method">The method to call after the existing mapping is finished.</param>
		public static void AppendToMapping<TVirtualView, TViewHandler>(this ICommandMapper<TVirtualView, TViewHandler> commandMapper,
			string key, Action<TViewHandler, TVirtualView, object?> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			commandMapper.ModifyMapping(key, (handler, view, args, action) =>
			{
				action?.Invoke(handler, view, args);
				method(handler, view, args);
			});
		}

		/// <summary>
		/// Specify a method to be run after an existing command mapping but skip if the types do not match.
		/// </summary>
		/// <typeparam name="TVirtualView">The cross-platform type.</typeparam>
		/// <typeparam name="TViewHandler">The handler type.</typeparam>
		/// <param name="commandMapper">The command mapper in which to change the mapping.</param>
		/// <param name="key">The name of the command.</param>
		/// <param name="method">The method to call after the existing mapping is finished.</param>
		public static void AppendToMapping<TVirtualView, TViewHandler>(this ICommandMapper<IElement, IElementHandler> commandMapper,
			string key, Action<TViewHandler, TVirtualView, object?> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			commandMapper.ModifyMapping(key, (handler, view, args, action) =>
			{
				action?.Invoke(handler, view, args);

				if ((handler is null || handler is TViewHandler) && view is TVirtualView v)
					method((TViewHandler)handler!, v, args);
			});
		}

		/// <summary>
		/// Specify a method to be run before an existing command mapping.
		/// </summary>
		/// <typeparam name="TVirtualView">The cross-platform type.</typeparam>
		/// <typeparam name="TViewHandler">The handler type.</typeparam>
		/// <param name="commandMapper">The command mapper in which to change the mapping.</param>
		/// <param name="key">The name of the command.</param>
		/// <param name="method">The method to call before the existing mapping begins.</param>
		public static void PrependToMapping<TVirtualView, TViewHandler>(this CommandMapper<TVirtualView, TViewHandler> commandMapper,
			string key, Action<TViewHandler, TVirtualView, object?> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
			=> ((ICommandMapper<TVirtualView, TViewHandler>)commandMapper).PrependToMapping(key, method);

		/// <summary>
		/// Specify a method to be run before an existing command mapping.
		/// </summary>
		/// <typeparam name="TVirtualView">The cross-platform type.</typeparam>
		/// <typeparam name="TViewHandler">The handler type.</typeparam>
		/// <param name="commandMapper">The command mapper in which to change the mapping.</param>
		/// <param name="key">The name of the command.</param>
		/// <param name="method">The method to call before the existing mapping begins.</param>
		public static void PrependToMapping<TVirtualView, TViewHandler>(this ICommandMapper<TVirtualView, TViewHandler> commandMapper,
			string key, Action<TViewHandler, TVirtualView, object?> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			commandMapper.ModifyMapping(key, (handler, view, args, action) =>
			{
				method(handler, view, args);
				action?.Invoke(handler, view, args);
			});
		}

		/// <summary>
		/// Specify a method to be run before an existing command mapping but skip if the types do not match.
		/// </summary>
		/// <typeparam name="TVirtualView">The cross-platform type.</typeparam>
		/// <typeparam name="TViewHandler">The handler type.</typeparam>
		/// <param name="commandMapper">The command mapper in which to change the mapping.</param>
		/// <param name="key">The name of the command.</param>
		/// <param name="method">The method to call before the existing mapping begins.</param>
		public static void PrependToMapping<TVirtualView, TViewHandler>(this ICommandMapper<IElement, IElementHandler> commandMapper,
			string key, Action<TViewHandler, TVirtualView, object?> method)
			where TVirtualView : IElement where TViewHandler : IElementHandler
		{
			commandMapper.ModifyMapping(key, (handler, view, args, action) =>
			{
				if ((handler is null || handler is TViewHandler) && view is TVirtualView v)
					method((TViewHandler)handler!, v, args);

				action?.Invoke(handler!, view, args);
			});
		}
	}
}