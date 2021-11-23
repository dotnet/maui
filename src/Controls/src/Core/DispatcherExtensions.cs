#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Controls
{
	internal static class DispatcherExtensions
	{
		public static IDispatcher FindDispatcher(this BindableObject? bindableObject)
		{
			// try find the dispatcher in the current hierarchy
			if (bindableObject is VisualElement visual &&
				visual.FindMauiContext() is IMauiContext context &&
				context.Services.GetService<IDispatcher>() is IDispatcher handlerDispatcher)
				return handlerDispatcher;

			// maybe this thread has a dispatcher
			if (Dispatcher.GetForCurrentThread() is IDispatcher globalDispatcher)
				return globalDispatcher;

			// try looking on the app
			if (bindableObject is not Application &&
				Application.Current?.Dispatcher is IDispatcher appDispatcher)
				return appDispatcher;

			// no dispatchers found at all
			throw new InvalidOperationException("BindableObject was not instantiated on a thread with a dispatcher nor does the current application have a dispatcher.");
		}

		public static void DispatchIfRequired(this IDispatcher? dispatcher, Action action)
		{
			dispatcher = EnsureDispatcher(dispatcher);
			if (dispatcher.IsDispatchRequired)
			{
				dispatcher.Dispatch(action);
			}
			else
			{
				action();
			}
		}

		public static Task DispatchIfRequiredAsync(this IDispatcher? dispatcher, Action action)
		{
			dispatcher = EnsureDispatcher(dispatcher);
			if (dispatcher.IsDispatchRequired)
			{
				return dispatcher.DispatchAsync(action);
			}
			else
			{
				action();
				return Task.CompletedTask;
			}
		}

		public static Task DispatchIfRequiredAsync(this IDispatcher? dispatcher, Func<Task> action)
		{
			dispatcher = EnsureDispatcher(dispatcher);
			if (dispatcher.IsDispatchRequired)
			{
				return dispatcher.DispatchAsync(action);
			}
			else
			{
				return action();
			}
		}

		static IDispatcher EnsureDispatcher(IDispatcher? dispatcher)
		{
			if (dispatcher is not null)
				return dispatcher;

			// maybe this thread has a dispatcher
			if (Dispatcher.GetForCurrentThread() is IDispatcher globalDispatcher)
				return globalDispatcher;

			// try looking on the app
			if (Application.Current?.Dispatcher is IDispatcher appDispatcher)
				return appDispatcher;

			// no dispatchers found at all
			throw new InvalidOperationException("The dispatcher was not found and the current application does not have a dispatcher.");
		}
	}
}