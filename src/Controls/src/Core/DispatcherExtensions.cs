using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls
{
	internal static class DispatcherExtensions
	{
		public static IDispatcher FindDispatcher(this BindableObject? bindableObject)
		{
			// try find the dispatcher in the current hierarchy
			// Exclude Application because we don't want to jump
			// directly to the Application IDispatcher at this point
			if (bindableObject is not Application &&
				bindableObject is Element element &&
				element.FindMauiContext() is IMauiContext context &&
				context.Services.GetService<IDispatcher>() is IDispatcher handlerDispatcher)
				return handlerDispatcher;

			// maybe this thread has a dispatcher
			if (Dispatcher.GetForCurrentThread() is IDispatcher globalDispatcher)
				return globalDispatcher;

			// If BO is of type Application then return the Dispatcher from ApplicationDispatcher
			if (bindableObject is Application app &&
				app.FindMauiContext() is IMauiContext appMauiContext)
			{
				if (appMauiContext.Services.GetOptionalApplicationDispatcher() is IDispatcher appDispatcherServiceDispatcher)
					return appDispatcherServiceDispatcher;

				// If BO is of type Application then check for its Dispatcher
				if (appMauiContext.Services.GetService<IDispatcher>() is IDispatcher appHandlerDispatcher)
					return appHandlerDispatcher;
			}

			// try looking on the static app
			// We don't include Application because Application.Dispatcher will call
			// `FindDispatcher` if it's _dispatcher property isn't initialized so this
			// could cause a Stack Overflow Exception
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
