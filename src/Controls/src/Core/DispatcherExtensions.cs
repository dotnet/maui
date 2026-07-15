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
			if (bindableObject.TryFindDispatcher(includeParents: true) is IDispatcher dispatcher)
				return dispatcher;

			if (bindableObject is not Application &&
				Application.Current?.Dispatcher is IDispatcher appDispatcher)
			{
				return appDispatcher;
			}

			throw new InvalidOperationException("BindableObject was not instantiated on a thread with a dispatcher nor does the current application have a dispatcher.");
		}

		internal static IDispatcher? TryFindDispatcher(
			this BindableObject? bindableObject,
			bool includeParents)
		{
			// try find the dispatcher in the current hierarchy
			// Exclude Application because we don't want to jump
			// directly to the Application IDispatcher at this point
			if (bindableObject is not Application &&
				bindableObject is Element element)
			{
				var context = includeParents
					? element.FindMauiContext()
					: (element as Maui.IElement)?.Handler?.MauiContext;

				if (context?.Services.GetService<IDispatcher>() is IDispatcher handlerDispatcher)
					return handlerDispatcher;
			}

			// maybe this thread has a dispatcher
			if (Dispatcher.GetForCurrentThread() is IDispatcher globalDispatcher)
				return globalDispatcher;

			// If BO is of type Application then return the Dispatcher from ApplicationDispatcher
			if (bindableObject is Application app &&
				TryFindApplicationDispatcher(app) is IDispatcher appDispatcher)
				return appDispatcher;

			// Try the static app's registered dispatcher without calling its Dispatcher
			// property, which may throw. The public FindDispatcher path preserves that
			// fallback after this non-throwing lookup returns null.
			if (bindableObject is not Application && Application.Current is Application currentApp)
			{
				if (TryFindApplicationDispatcher(currentApp) is IDispatcher currentAppDispatcherService)
					return currentAppDispatcherService;
			}

			return null;
		}

		static IDispatcher? TryFindApplicationDispatcher(Application app)
		{
			if (app.FindMauiContext() is not IMauiContext appMauiContext)
				return null;

			return appMauiContext.Services.GetOptionalApplicationDispatcher()
				?? appMauiContext.Services.GetService<IDispatcher>();
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
